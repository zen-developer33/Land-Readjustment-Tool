using Land_Readjustment_Tool.Core.Entities.Policy;
using Land_Readjustment_Tool.Data;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Services.Policy
{
    public sealed class PolicyTemplateSeeder
    {
        private readonly AppDbContext _context;

        public PolicyTemplateSeeder(ProjectSession session)
        {
            _context = session.GetDbContext();
        }

        public async Task<PolicySet> EnsureBaireniTemplateAsync(CancellationToken ct = default)
        {
            PolicySet? existing = await _context.PolicySets
                .OrderBy(policy => policy.Id)
                .FirstOrDefaultAsync(ct);

            if (existing != null)
            {
                if (IsEditableBaireniDraft(existing) &&
                    await NeedsBaireniTemplateUpgradeAsync(existing.Id, ct))
                {
                    await UpgradeExistingBaireniTemplateAsync(existing.Id, ct);
                }

                return existing;
            }

            PolicySet policy = CreateBaireniPolicy();
            await _context.PolicySets.AddAsync(policy, ct);
            await _context.SaveChangesAsync(ct);
            return policy;
        }

        private static bool IsEditableBaireniDraft(PolicySet policy)
        {
            return !policy.IsLocked &&
                   string.Equals(policy.Status, PolicyStatuses.Draft, StringComparison.OrdinalIgnoreCase) &&
                   (string.Equals(policy.PolicyGroupKey, "BAIRENI-LAND-POOLING-POLICY", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(policy.PolicyCode, "BAIRENI-LPP", StringComparison.OrdinalIgnoreCase) ||
                    policy.PolicyName.Contains("Baireni", StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> NeedsBaireniTemplateUpgradeAsync(int policySetId, CancellationToken ct)
        {
            bool hasRoadReferenceColumns = await _context.PolicyLookupColumns
                .AsNoTracking()
                .AnyAsync(column =>
                    column.PolicyLookupTable.PolicySetId == policySetId &&
                    column.PolicyLookupTable.TableKey == "cornerTypeDefinitions" &&
                    column.ColumnKey == "primaryFrontageRoad" &&
                    column.ValueType == "RoadReference",
                    ct);

            if (!hasRoadReferenceColumns)
                return true;

            bool hasLinkedRoadTable = await _context.PolicyLookupTables
                .AsNoTracking()
                .AnyAsync(table =>
                    table.PolicySetId == policySetId &&
                    table.TableKey == "roadContributionTable" &&
                    table.PolicyClauseId != null,
                    ct);

            if (!hasLinkedRoadTable)
                return true;

            bool hasPercentDisplay = await _context.PolicyParameters
                .AsNoTracking()
                .AnyAsync(parameter =>
                    parameter.PolicySetId == policySetId &&
                    parameter.ParameterKey == "openAreaRate" &&
                    parameter.ValueType == "Percent" &&
                    parameter.Unit == "%",
                    ct);

            if (!hasPercentDisplay)
                return true;

            bool hasClauseLinkedParameter = await _context.PolicyParameters
                .AsNoTracking()
                .AnyAsync(parameter =>
                    parameter.PolicySetId == policySetId &&
                    parameter.ParameterKey == "minFrontageM" &&
                    parameter.PolicyClauseId != null,
                    ct);

            return !hasClauseLinkedParameter;
        }

        private async Task UpgradeExistingBaireniTemplateAsync(int policySetId, CancellationToken ct)
        {
            PolicySet? policy = await _context.PolicySets
                .Include(p => p.Clauses)
                .Include(p => p.Parameters)
                .Include(p => p.LookupTables)
                    .ThenInclude(t => t.Columns)
                .Include(p => p.LookupTables)
                    .ThenInclude(t => t.Rows)
                        .ThenInclude(r => r.Cells)
                .FirstOrDefaultAsync(p => p.Id == policySetId, ct);

            if (policy == null ||
                !string.Equals(policy.Status, PolicyStatuses.Draft, StringComparison.OrdinalIgnoreCase) ||
                policy.IsLocked)
            {
                return;
            }

            bool changed = false;
            DateTime now = DateTime.Now;

            changed |= NormalizePercentParameter(policy, "openAreaRate", "5.04");
            changed |= NormalizePercentParameter(policy, "infrastructureRate", "4.79");
            changed |= NormalizePercentParameter(policy, "roadCornerRate", "1.00");
            changed |= NormalizePercentParameter(policy, "fixedCommonTotal", "10.83");
            changed |= LinkExistingParametersToClauses(policy);

            PolicyLookupTable? cornerTypeTable = policy.LookupTables
                .FirstOrDefault(t => string.Equals(t.TableKey, "cornerTypeDefinitions", StringComparison.OrdinalIgnoreCase));
            if (cornerTypeTable == null)
            {
                AddCornerTypeDefinitionTable(policy, now);
                changed = true;
            }
            else
            {
                changed |= LinkLookupTableToClause(policy, cornerTypeTable, "2.2.7", 1);
                changed |= EnsureCornerTypeDefinitionSchema(cornerTypeTable);
            }

            PolicyLookupTable? roadTable = policy.LookupTables
                .FirstOrDefault(t => string.Equals(t.TableKey, "roadContributionTable", StringComparison.OrdinalIgnoreCase));
            if (roadTable != null)
                changed |= LinkLookupTableToClause(policy, roadTable, "2.2.2", 2);

            PolicyLookupTable? specialTable = policy.LookupTables
                .FirstOrDefault(t => string.Equals(t.TableKey, "specialFacilityRates", StringComparison.OrdinalIgnoreCase));
            if (specialTable != null)
                changed |= LinkLookupTableToClause(policy, specialTable, "2.2.9", 3);

            PolicyLookupTable? cornerTable = policy.LookupTables
                .FirstOrDefault(t => string.Equals(t.TableKey, "cornerContributionTable", StringComparison.OrdinalIgnoreCase));
            if (cornerTable != null)
            {
                changed |= LinkLookupTableToClause(policy, cornerTable, "2.2.7", 4);
                changed |= EnsureCornerContributionCodes(cornerTable);
            }

            if (!changed)
                return;

            policy.LastModifiedDate = now;
            policy.AuditEntries.Add(new PolicyAuditEntry
            {
                Action = "Template Updated",
                Details = "Baireni policy draft upgraded with clause-linked lookup tables, corner type definitions, and percentage parameter display.",
                CreatedDate = now
            });
            await _context.SaveChangesAsync(ct);
        }

        private static bool NormalizePercentParameter(PolicySet policy, string key, string value)
        {
            PolicyParameter? parameter = policy.Parameters.FirstOrDefault(p =>
                string.Equals(p.ParameterKey, key, StringComparison.OrdinalIgnoreCase));
            if (parameter == null)
                return false;

            bool changed = false;
            if (!string.Equals(parameter.ValueType, "Percent", StringComparison.OrdinalIgnoreCase))
            {
                parameter.ValueType = "Percent";
                changed = true;
            }

            if (!string.Equals(parameter.ValueText, value, StringComparison.OrdinalIgnoreCase))
            {
                parameter.ValueText = value;
                changed = true;
            }

            if (!string.Equals(parameter.DefaultValueText, value, StringComparison.OrdinalIgnoreCase))
            {
                parameter.DefaultValueText = value;
                changed = true;
            }

            if (!string.Equals(parameter.Unit, "%", StringComparison.OrdinalIgnoreCase))
            {
                parameter.Unit = "%";
                changed = true;
            }

            if (changed)
                parameter.LastModifiedDate = DateTime.Now;

            return changed;
        }

        private static bool LinkLookupTableToClause(
            PolicySet policy,
            PolicyLookupTable table,
            string clauseCode,
            int displayOrder)
        {
            bool changed = false;
            PolicyClause? clause = Clause(policy, clauseCode);
            if (clause != null && table.PolicyClauseId != clause.Id)
            {
                table.PolicyClauseId = clause.Id;
                changed = true;
            }

            if (table.DisplayOrder != displayOrder)
            {
                table.DisplayOrder = displayOrder;
                changed = true;
            }

            if (changed)
                table.LastModifiedDate = DateTime.Now;

            return changed;
        }

        private static bool EnsureCornerContributionCodes(PolicyLookupTable table)
        {
            string[] cornerCodes =
            [
                "C-P9-S9",
                "C-P9-S8",
                "C-P9-S7",
                "C-P9-S6",
                "C-P9-S4",
                "C-P8-S8",
                "C-P8-S7",
                "C-P8-S6",
                "C-P8-S4",
                "C-P7-S7",
                "C-P7-S6",
                "C-P7-S4",
                "C-P6-S6",
                "C-P6-S4",
                "C-P4-S4"
            ];

            bool changed = false;
            PolicyLookupColumn? cornerCodeColumn = table.Columns
                .FirstOrDefault(c => string.Equals(c.ColumnKey, "cornerCode", StringComparison.OrdinalIgnoreCase));
            if (cornerCodeColumn == null)
            {
                cornerCodeColumn = new PolicyLookupColumn
                {
                    ColumnKey = "cornerCode",
                    HeaderText = "Corner Code",
                    ValueType = "Text",
                    DisplayOrder = 1
                };
                foreach (PolicyLookupColumn column in table.Columns)
                    column.DisplayOrder++;

                table.Columns.Add(cornerCodeColumn);
                changed = true;
            }

            List<PolicyLookupRow> rows = table.Rows.OrderBy(r => r.DisplayOrder).ToList();
            for (int index = 0; index < rows.Count && index < cornerCodes.Length; index++)
            {
                PolicyLookupCell? cell = rows[index].Cells
                    .FirstOrDefault(c => c.PolicyLookupColumnId == cornerCodeColumn.Id ||
                                         c.PolicyLookupColumn == cornerCodeColumn);
                if (cell == null)
                {
                    rows[index].Cells.Add(new PolicyLookupCell
                    {
                        PolicyLookupColumn = cornerCodeColumn,
                        ValueText = cornerCodes[index]
                    });
                    changed = true;
                }
                else if (string.IsNullOrWhiteSpace(cell.ValueText) ||
                         cell.ValueText.StartsWith("C-M", StringComparison.OrdinalIgnoreCase))
                {
                    cell.ValueText = cornerCodes[index];
                    changed = true;
                }
            }

            if (changed)
            {
                table.Title = "Schedule 1 Table 2(b) - Corner Plot Rates";
                table.Description = "Additional contribution rates by project-defined corner type.";
                table.LastModifiedDate = DateTime.Now;
            }

            return changed;
        }

        private static bool EnsureCornerTypeDefinitionSchema(PolicyLookupTable table)
        {
            bool changed = false;
            changed |= RenameColumn(
                table,
                "majorRoadClass",
                "primaryFrontageRoad",
                "Primary Frontage Road",
                2);
            changed |= RenameColumn(
                table,
                "primaryFrontageRoadType",
                "primaryFrontageRoad",
                "Primary Frontage Road",
                2);
            changed |= RenameColumn(
                table,
                "secondaryRoadClass",
                "secondaryFrontageRoad",
                "Secondary Frontage Road",
                3);
            changed |= RenameColumn(
                table,
                "secondaryFrontageRoadType",
                "secondaryFrontageRoad",
                "Secondary Frontage Road",
                3);

            PolicyLookupColumn? displayColumn = table.Columns
                .FirstOrDefault(c => string.Equals(c.ColumnKey, "displayName", StringComparison.OrdinalIgnoreCase));
            if (displayColumn != null && displayColumn.DisplayOrder != 4)
            {
                displayColumn.DisplayOrder = 4;
                changed = true;
            }

            PolicyLookupColumn? rateColumn = table.Columns
                .FirstOrDefault(c => string.Equals(c.ColumnKey, "defaultRatePercent", StringComparison.OrdinalIgnoreCase));
            if (rateColumn != null)
            {
                if (rateColumn.HeaderText != "Default Rate")
                {
                    rateColumn.HeaderText = "Default Rate";
                    changed = true;
                }

                if (!string.Equals(rateColumn.Unit, "%", StringComparison.OrdinalIgnoreCase))
                {
                    rateColumn.Unit = "%";
                    changed = true;
                }

                if (rateColumn.DisplayOrder != 5)
                {
                    rateColumn.DisplayOrder = 5;
                    changed = true;
                }
            }

            if (!string.Equals(table.Title, "Project Corner Type Definitions", StringComparison.Ordinal))
            {
                table.Title = "Project Corner Type Definitions";
                changed = true;
            }

            string description = "Defines corner classes for parcels with two adjacent frontages. Primary and secondary frontage roads are selected from the project's road names and widths.";
            if (!string.Equals(table.Description, description, StringComparison.Ordinal))
            {
                table.Description = description;
                changed = true;
            }

            if (changed)
                table.LastModifiedDate = DateTime.Now;

            return changed;
        }

        private static bool RenameColumn(
            PolicyLookupTable table,
            string oldKey,
            string newKey,
            string newHeader,
            int displayOrder)
        {
            PolicyLookupColumn? column = table.Columns
                .FirstOrDefault(c => string.Equals(c.ColumnKey, newKey, StringComparison.OrdinalIgnoreCase)) ??
                table.Columns.FirstOrDefault(c => string.Equals(c.ColumnKey, oldKey, StringComparison.OrdinalIgnoreCase));
            if (column == null)
                return false;

            bool changed = false;
            if (!string.Equals(column.ColumnKey, newKey, StringComparison.Ordinal))
            {
                column.ColumnKey = newKey;
                changed = true;
            }

            if (!string.Equals(column.HeaderText, newHeader, StringComparison.Ordinal))
            {
                column.HeaderText = newHeader;
                changed = true;
            }

            if (column.DisplayOrder != displayOrder)
            {
                column.DisplayOrder = displayOrder;
                changed = true;
            }

            if ((string.Equals(newKey, "primaryFrontageRoad", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(newKey, "secondaryFrontageRoad", StringComparison.OrdinalIgnoreCase)) &&
                !string.Equals(column.ValueType, "RoadReference", StringComparison.OrdinalIgnoreCase))
            {
                column.ValueType = "RoadReference";
                changed = true;
            }

            return changed;
        }

        public static PolicySet CreateBaireniPolicy()
        {
            DateTime now = DateTime.Now;
            PolicySet policy = new()
            {
                PolicyGroupKey = "BAIRENI-LAND-POOLING-POLICY",
                PolicyCode = "BAIRENI-LPP",
                PolicyName = "Baireni Land Pooling Contribution and Return Policy",
                PolicyType = "Combined",
                VersionNo = 1,
                Status = PolicyStatuses.Draft,
                IsLocked = false,
                SourceTitle = "Baireni Land Pooling Project Land Contribution and Land Return Policy",
                SourceReference = "Draft for Approval, 2079 BS (2022 AD)",
                Notes = "Initial editable policy draft created from the supplied reference documents. Parcel inputs and calculations are intentionally outside this manager.",
                CreatedDate = now,
                LastModifiedDate = now
            };

            AddSections(policy, now);
            AddClauses(policy, now);
            AddParameters(policy, now);
            LinkParametersToClauses(policy);
            AddCornerTypeDefinitionTable(policy, now);
            AddRoadContributionTable(policy, now);
            AddSpecialFacilityTable(policy, now);
            AddCornerPlotTable(policy, now);
            policy.AuditEntries.Add(new PolicyAuditEntry
            {
                Action = "Seeded",
                Details = "Baireni draft policy template created.",
                CreatedDate = now
            });

            return policy;
        }

        private static void AddSections(PolicySet policy, DateTime now)
        {
            // Sections are auto-lettered A, B, C ... in the seeded policy and shown in the
            // section grid above the clauses grid. The clause table still stores the section
            // heading as a string (PolicyClause.PolicySection) so legacy logic keeps working.
            (string Code, string Heading)[] sections =
            [
                ("1", "Preliminary"),
                ("2", "Contribution Policy"),
                ("3", "Return Policy"),
            ];

            for (int i = 0; i < sections.Length; i++)
            {
                policy.Sections.Add(new PolicySectionDefinition
                {
                    SectionCode = sections[i].Code,
                    Heading = sections[i].Heading,
                    DisplayOrder = i + 1,
                    CreatedDate = now,
                    LastModifiedDate = now
                });
            }
        }

        private static void AddClauses(PolicySet policy, DateTime now)
        {
            (string Code, string Section, string Heading, string Description)[] clauses =
            [
                ("2.1.1", "Preliminary", "Short title", "This document shall be known as the Baireni Land Pooling Project Land Contribution and Land Return Policy."),
                ("2.1.2", "Preliminary", "Application area", "This policy applies to all parcels lying within the declared boundary of the Baireni Land Pooling Project."),
                ("2.1.3", "Preliminary", "Project objective", "The project aims to consolidate fragmented holdings into planned parcels with road, drainage, open space, and basic infrastructure access, and to return developed parcels to original landowners on an equitable contribution basis."),
                ("2.1.4", "Preliminary", "Definitions", "Terms used in this policy shall have the meaning assigned in the project's planning documents. Where a term is not defined, the meaning under prevailing Government of Nepal land-pooling law shall apply."),
                ("2.1.5", "Preliminary", "Effective date", "This policy is effective from the date of approval by the project executive committee and remains in force until amended, superseded, or the project is formally closed."),
                ("2.2.1", "Contribution Policy", "Contribution for infrastructure and services", "Land shall be deducted as contribution from each landowner within the project area in proportion to the benefits and facilities received for infrastructure, roads, drains, and other project services."),
                ("2.2.2", "Contribution Policy", "Road and drain contribution by proposed road width", "Every landowner shall contribute land for roads and drains according to proposed road width. For parcels abutting an existing road, contribution is based on proposed road width, existing road width, and first returned parcel depth using Schedule 1 Table 1."),
                ("2.2.3", "Contribution Policy", "Survey map controls road-width differences", "Where road width in the field differs from the survey map, or where a new road is opened, contribution shall be calculated on the basis of the survey map."),
                ("2.2.4", "Contribution Policy", "Internal branch road contribution", "For internal branch roads requested by the majority of relevant landowners, half of the additional field-road width is deducted from relevant landowners, and remaining required contribution is deducted from the rest."),
                ("2.2.5", "Contribution Policy", "Collectively used private road ownership", "Where a road is recorded in one landowner's certificate but has been used collectively by mutual understanding, contribution may be apportioned arithmetically among relevant landowners after supporting documents are submitted."),
                ("2.2.6", "Contribution Policy", "Two-road frontage retention", "When new parcels are carved, a parcel fronting two roads shall retain both frontages and land required for both roads shall be deducted using Schedule 1 Table 1."),
                ("2.2.7", "Contribution Policy", "Corner parcel contribution", "For a corner parcel at two roads, frontage is set toward the larger road; contribution for the larger road is deducted under Schedule 1 Table 1 and additional corner contribution is deducted under Schedule 1 Table 2(b)."),
                ("2.2.8", "Contribution Policy", "Road junction and corner turning contribution", "For land required for all road junctions and corner turnings, an additional contribution is deducted from each developed plot at the fixed road-corner percentage."),
                ("2.2.9", "Contribution Policy", "Open-space adjoining and across-road contribution", "Land adjoining an open space within 8 m frontage and land directly across the road from open-space frontage receive additional contribution under Schedule 1 Table 2(a)."),
                ("2.2.10", "Contribution Policy", "Slope and central depression contribution", "Original sloped parcels on the northern boundary and parcels in the central depression area receive additional contribution under Schedule 1 Table 2(a)."),
                ("2.2.11", "Contribution Policy", "No external service road opening", "Opening a road from the project's developed plots to serve land outside the project but adjoining its boundary shall not be permitted."),
                ("2.3.1", "Return Policy", "Return at former or nearby location", "Any parcel shall generally be returned at its former location or nearby, subject to project committee decision and technical feasibility."),
                ("2.3.2", "Return Policy", "Minimum plot area", "The preferred minimum developed parcel area is 130 sq. m. Smaller returns may be allowed for already-small parcels, but the absolute minimum developed plot area is 79.50 sq. m."),
                ("2.3.3", "Return Policy", "Minimum frontage", "The minimum frontage of a developed plot shall be 6.0 m."),
                ("2.3.4", "Return Policy", "Technical exception for affected road-front parcels", "For parcels abutting former main roads and similar adversely affected parcels, the project is not bound to meet minimum frontage and area where technically infeasible."),
                ("2.3.5", "Return Policy", "Project land transaction price", "Any purchase or sale transaction between the project and landowner shall use the price set by the project based on prevailing sale rates around the project area."),
                ("2.3.6", "Return Policy", "Top-up land to absolute minimum", "Where a returned developed plot would be smaller than 79.50 sq. m, the project adds shortfall land to reach the minimum and the owner purchases the added land at the project-fixed rate."),
                ("2.3.7", "Return Policy", "Project buy-out option", "If the owner is unwilling or unable to take added land and the prescribed developed plot, the project may purchase such land at the project-fixed price."),
                ("2.3.8", "Return Policy", "Deferred payment recommendation", "Where an owner cannot pay top-up land cost in a lump sum, possession may be granted under committee recommendation with ownership certificate after full payment."),
                ("2.3.9", "Return Policy", "Joint return below minimum", "Where a returned parcel would be below 79.50 sq. m or frontage below 6 m, two or more landowners may jointly take one parcel."),
                ("2.3.10", "Return Policy", "Consolidation of small/scattered holdings", "A single landowner holding up to 200 sq. m total, or one household's parcels at different locations, may generally be consolidated and returned at one location."),
                ("2.3.11", "Return Policy", "House setback during new parcel creation", "When a new parcel is carved on land containing a house, at least 1 m setback should be kept on the window/door side and road side as far as possible."),
                ("2.3.12", "Return Policy", "Block and parcel depth", "Developed-plot blocks shall generally have block depth 26 m to 40 m, corresponding to parcel depth 13 m to 20 m."),
                ("2.3.13", "Return Policy", "Corner priority and sales plot", "When new corner parcels are carved, priority goes to the original corner owner; if they do not want it, it may go to a nearby larger holding, otherwise it remains a sales plot."),
                ("2.3.14", "Return Policy", "Corner shortfall and relocation", "For a corner parcel below minimum area, the owner may buy shortfall land to retain the corner; otherwise the parcel may be returned nearby where feasible."),
                ("2.3.15", "Return Policy", "Multiple corner splits in one original plot", "Where two or more roads split one original plot into multiple corner plots, generally only one parcel shall be returned at the corner with the largest area."),
                ("2.3.16", "Return Policy", "Post-completion subdivision restriction", "After project completion, developed plots shall not be subdivided below the area and frontage fixed by prevailing Government of Nepal law.")
            ];

            for (int i = 0; i < clauses.Length; i++)
            {
                policy.Clauses.Add(new PolicyClause
                {
                    ClauseCode = clauses[i].Code,
                    PolicySection = clauses[i].Section,
                    Heading = clauses[i].Heading,
                    Description = clauses[i].Description,
                    DisplayOrder = i + 1,
                    CreatedDate = now,
                    LastModifiedDate = now
                });
            }
        }

        private static void AddParameters(PolicySet policy, DateTime now)
        {
            AddParameter(policy, "openAreaRate", "Open Area Contribution", "Percent", "5.04", "%", "Open-area share deducted from every parcel.", 1, now);
            AddParameter(policy, "infrastructureRate", "Infrastructure Contribution", "Percent", "4.79", "%", "Infrastructure construction share deducted from every parcel.", 2, now);
            AddParameter(policy, "roadCornerRate", "Road Corner / Turning Contribution", "Percent", "1.00", "%", "Road junction and corner turning share deducted from every developed plot.", 3, now);
            AddParameter(policy, "fixedCommonTotal", "Fixed Common Contribution Total", "Percent", "10.83", "%", "Convenience sum of open area, infrastructure, and road corner rates.", 4, now);
            AddParameter(policy, "roadFormulaId", "Road Formula", "Text", "RoadContributionStandard", null, "Registered formula identifier for road contribution.", 5, now);
            AddParameter(policy, "returnedParcelDepthRatio", "Returned Parcel Depth Ratio", "Decimal", "0.5", "ratio", "Returned parcel depth equals block depth multiplied by this ratio.", 6, now);
            AddParameter(policy, "minPlotAreaSqM", "Absolute Minimum Plot Area", "Decimal", "79.5", "sqm", "Absolute minimum developed-plot area.", 7, now);
            AddParameter(policy, "preferredMinPlotAreaSqM", "Preferred Minimum Plot Area", "Decimal", "130", "sqm", "Preferred minimum developed-plot area.", 8, now);
            AddParameter(policy, "minFrontageM", "Minimum Frontage", "Decimal", "6", "m", "Minimum frontage of a developed plot.", 9, now);
            AddParameter(policy, "minDepthM", "Minimum Depth", "Decimal", "13", "m", "Minimum parcel depth.", 10, now);
            AddParameter(policy, "maxDepthM", "Maximum Depth", "Decimal", "20", "m", "Maximum parcel depth.", 11, now);
            AddParameter(policy, "blockDepthMinM", "Minimum Block Depth", "Decimal", "26", "m", "Minimum block depth.", 12, now);
            AddParameter(policy, "blockDepthMaxM", "Maximum Block Depth", "Decimal", "40", "m", "Maximum block depth.", 13, now);
            AddParameter(policy, "setbackRoadM", "Road-Side Setback", "Decimal", "1", "m", "Road-side setback where a house is present.", 14, now);
            AddParameter(policy, "setbackWindowM", "Window/Door-Side Setback", "Decimal", "1", "m", "Window/door-side setback where a house is present.", 15, now);
            AddParameter(policy, "consolidationMaxAreaSqM", "Consolidation Maximum Area", "Decimal", "200", "sqm", "Scattered holdings up to this may merge to one return location.", 16, now);
            AddParameter(policy, "jointReturnThresholdSqM", "Joint Return Threshold", "Decimal", "150", "sqm", "Above this area, merged return is normally by request.", 17, now);
            AddParameter(policy, "jointReturnAllowedBelowMin", "Joint Return Allowed Below Minimum", "Bool", "true", null, "Two or more owners may jointly take one below-minimum parcel.", 18, now);
            AddParameter(policy, "salesPlotForUnfittedCorners", "Sales Plot For Unfitted Corners", "Bool", "true", null, "Unfittable corner may become a sales plot.", 19, now);
            AddParameter(policy, "projectLandRatePerSqM", "Project Land Rate", "ProjectValue", "", "currency/sqm", "Project-set price for top-up purchase or buy-out.", 20, now);
        }

        private static void AddParameter(
            PolicySet policy,
            string key,
            string label,
            string type,
            string value,
            string? unit,
            string description,
            int order,
            DateTime now)
        {
            policy.Parameters.Add(new PolicyParameter
            {
                ParameterKey = key,
                Label = label,
                ValueType = type,
                ValueText = value,
                DefaultValueText = value,
                Unit = unit,
                Description = description,
                DisplayOrder = order,
                CreatedDate = now,
                LastModifiedDate = now
            });
        }

        private static void LinkParametersToClauses(PolicySet policy)
        {
            LinkParameter(policy, "openAreaRate", "2.2.1");
            LinkParameter(policy, "infrastructureRate", "2.2.1");
            LinkParameter(policy, "roadCornerRate", "2.2.8");
            LinkParameter(policy, "fixedCommonTotal", "2.2.1");
            LinkParameter(policy, "roadFormulaId", "2.2.2");
            LinkParameter(policy, "returnedParcelDepthRatio", "2.2.2");
            LinkParameter(policy, "minPlotAreaSqM", "2.3.2");
            LinkParameter(policy, "preferredMinPlotAreaSqM", "2.3.2");
            LinkParameter(policy, "minFrontageM", "2.3.3");
            LinkParameter(policy, "minDepthM", "2.3.12");
            LinkParameter(policy, "maxDepthM", "2.3.12");
            LinkParameter(policy, "blockDepthMinM", "2.3.12");
            LinkParameter(policy, "blockDepthMaxM", "2.3.12");
            LinkParameter(policy, "setbackRoadM", "2.3.11");
            LinkParameter(policy, "setbackWindowM", "2.3.11");
            LinkParameter(policy, "consolidationMaxAreaSqM", "2.3.10");
            LinkParameter(policy, "jointReturnThresholdSqM", "2.3.9");
            LinkParameter(policy, "jointReturnAllowedBelowMin", "2.3.9");
            LinkParameter(policy, "salesPlotForUnfittedCorners", "2.3.13");
            LinkParameter(policy, "projectLandRatePerSqM", "2.3.5");
        }

        private static void LinkParameter(PolicySet policy, string parameterKey, string clauseCode)
        {
            PolicyParameter? parameter = policy.Parameters.FirstOrDefault(p =>
                string.Equals(p.ParameterKey, parameterKey, StringComparison.OrdinalIgnoreCase));
            PolicyClause? clause = Clause(policy, clauseCode);
            if (parameter != null && clause != null)
                parameter.PolicyClause = clause;
        }

        private static bool LinkExistingParametersToClauses(PolicySet policy)
        {
            bool changed = false;
            changed |= LinkExistingParameter(policy, "openAreaRate", "2.2.1");
            changed |= LinkExistingParameter(policy, "infrastructureRate", "2.2.1");
            changed |= LinkExistingParameter(policy, "roadCornerRate", "2.2.8");
            changed |= LinkExistingParameter(policy, "fixedCommonTotal", "2.2.1");
            changed |= LinkExistingParameter(policy, "roadFormulaId", "2.2.2");
            changed |= LinkExistingParameter(policy, "returnedParcelDepthRatio", "2.2.2");
            changed |= LinkExistingParameter(policy, "minPlotAreaSqM", "2.3.2");
            changed |= LinkExistingParameter(policy, "preferredMinPlotAreaSqM", "2.3.2");
            changed |= LinkExistingParameter(policy, "minFrontageM", "2.3.3");
            changed |= LinkExistingParameter(policy, "minDepthM", "2.3.12");
            changed |= LinkExistingParameter(policy, "maxDepthM", "2.3.12");
            changed |= LinkExistingParameter(policy, "blockDepthMinM", "2.3.12");
            changed |= LinkExistingParameter(policy, "blockDepthMaxM", "2.3.12");
            changed |= LinkExistingParameter(policy, "setbackRoadM", "2.3.11");
            changed |= LinkExistingParameter(policy, "setbackWindowM", "2.3.11");
            changed |= LinkExistingParameter(policy, "consolidationMaxAreaSqM", "2.3.10");
            changed |= LinkExistingParameter(policy, "jointReturnThresholdSqM", "2.3.9");
            changed |= LinkExistingParameter(policy, "jointReturnAllowedBelowMin", "2.3.9");
            changed |= LinkExistingParameter(policy, "salesPlotForUnfittedCorners", "2.3.13");
            changed |= LinkExistingParameter(policy, "projectLandRatePerSqM", "2.3.5");
            return changed;
        }

        private static bool LinkExistingParameter(PolicySet policy, string parameterKey, string clauseCode)
        {
            PolicyParameter? parameter = policy.Parameters.FirstOrDefault(p =>
                string.Equals(p.ParameterKey, parameterKey, StringComparison.OrdinalIgnoreCase));
            PolicyClause? clause = Clause(policy, clauseCode);
            if (parameter == null || clause == null || parameter.PolicyClauseId == clause.Id)
                return false;

            parameter.PolicyClauseId = clause.Id;
            parameter.LastModifiedDate = DateTime.Now;
            return true;
        }

        private static void AddRoadContributionTable(PolicySet policy, DateTime now)
        {
            PolicyLookupTable table = CreateTable(
                "roadContributionTable",
                "Schedule 1 Table 1 - Road Contribution",
                "Contribution payable by existing road width, block depth, and proposed road width.",
                2,
                now,
                [
                    ("existingRoadWidthM", "Existing Road (m)", "Decimal", "m"),
                    ("blockDepthM", "Block Depth (m)", "Decimal", "m"),
                    ("proposed4M", "4 m", "Percent", "%"),
                    ("proposed6M", "6 m", "Percent", "%"),
                    ("proposed7M", "7 m", "Percent", "%"),
                    ("proposed8M", "8 m", "Percent", "%"),
                    ("proposed914M", "9.14 m", "Percent", "%")
                ]);
            table.PolicyClause = Clause(policy, "2.2.2");

            string[][] rows =
            [
                ["0", "27", "23.73", "29.01", "31.42", "33.69", "36.12"],
                ["4", "27", "10.83", "17.73", "20.83", "23.73", "26.82"],
                ["6", "27", "2.83", "10.83", "14.40", "17.73", "21.25"],
                ["9.14", "27", "-12.68", "-2.33", "2.22", "6.42", "10.83"],
                ["0", "29", "22.95", "27.97", "30.27", "32.45", "34.79"],
                ["4", "29", "10.83", "17.28", "20.21", "22.95", "25.89"],
                ["6", "29", "3.42", "10.83", "14.16", "17.28", "20.60"],
                ["9.14", "29", "-10.71", "-1.31", "2.86", "6.74", "10.83"],
                ["0", "31", "22.26", "27.05", "29.25", "31.34", "33.60"],
                ["4", "31", "10.83", "16.89", "19.65", "22.26", "25.05"],
                ["6", "31", "3.93", "10.83", "13.96", "16.89", "20.03"],
                ["9.14", "31", "-9.05", "-0.44", "3.41", "7.01", "10.83"],
                ["0", "33", "21.64", "26.21", "28.33", "30.34", "32.52"],
                ["4", "33", "10.83", "16.54", "19.16", "21.64", "24.31"],
                ["6", "33", "4.38", "10.83", "13.77", "16.54", "19.52"],
                ["9.14", "33", "-7.62", "0.31", "3.90", "7.25", "10.83"],
                ["0", "35", "21.09", "25.46", "27.50", "29.43", "31.54"],
                ["4", "35", "10.83", "16.24", "18.72", "21.09", "23.64"],
                ["6", "35", "4.77", "10.83", "13.61", "16.24", "19.06"],
                ["9.14", "35", "-6.38", "0.97", "4.32", "7.46", "10.83"],
                ["0", "37", "20.59", "24.78", "26.74", "28.61", "30.64"],
                ["4", "37", "10.83", "15.96", "18.33", "20.59", "23.03"],
                ["6", "37", "5.12", "10.83", "13.46", "15.96", "18.65"],
                ["9.14", "37", "-5.30", "1.56", "4.69", "7.65", "10.83"],
                ["0", "39", "20.13", "24.16", "26.05", "27.85", "29.82"],
                ["4", "39", "10.83", "15.71", "17.97", "20.13", "22.47"],
                ["6", "39", "5.42", "10.83", "13.33", "15.71", "18.28"],
                ["9.14", "39", "-4.35", "2.07", "5.02", "7.82", "10.83"]
            ];

            AddRows(table, rows);
            policy.LookupTables.Add(table);
        }

        private static void AddCornerTypeDefinitionTable(PolicySet policy, DateTime now)
        {
            PolicyLookupTable table = CreateTable(
                "cornerTypeDefinitions",
                "Project Corner Type Definitions",
                "Defines corner classes for parcels with two adjacent frontages. Primary and secondary frontage roads are selected from the project's road names and widths.",
                1,
                now,
                [
                    ("cornerCode", "Corner Code", "Text", null),
                    ("primaryFrontageRoad", "Primary Frontage Road", "RoadReference", null),
                    ("secondaryFrontageRoad", "Secondary Frontage Road", "RoadReference", null),
                    ("displayName", "Display Name", "Text", null),
                    ("defaultRatePercent", "Default Rate", "Percent", "%"),
                    ("description", "Description", "Text", null)
                ]);
            table.PolicyClause = Clause(policy, "2.2.7");

            AddRows(table,
            [
                ["C-P9-S9", "9.14 m Road | 9.14 m", "9.14 m Road | 9.14 m", "9.14 m x 9.14 m corner", "", "Two adjacent frontages with the same road width."],
                ["C-P9-S8", "9.14 m Road | 9.14 m", "8 m Road | 8 m", "9.14 m x 8 m corner", "9.00", "Two adjacent frontages; the 9.14 m road is the primary frontage."],
                ["C-P9-S7", "9.14 m Road | 9.14 m", "7 m Road | 7 m", "9.14 m x 7 m corner", "", "Primary frontage is the 9.14 m road; secondary frontage is the 7 m road."],
                ["C-P9-S6", "9.14 m Road | 9.14 m", "6 m Road | 6 m", "9.14 m x 6 m corner", "", "Primary frontage is the 9.14 m road; secondary frontage is the 6 m road."],
                ["C-P9-S4", "9.14 m Road | 9.14 m", "4 m Road | 4 m", "9.14 m x 4 m corner", "", "Primary frontage is the 9.14 m road; secondary frontage is the 4 m road."],
                ["C-P8-S8", "8 m Road | 8 m", "8 m Road | 8 m", "8 m x 8 m corner", "8.61", "Two adjacent frontages with the same road name and width on both sides."],
                ["C-P8-S7", "8 m Road | 8 m", "7 m Road | 7 m", "8 m x 7 m corner", "8.23", "Primary frontage is the higher-priority 8 m road."],
                ["C-P8-S6", "8 m Road | 8 m", "6 m Road | 6 m", "8 m x 6 m corner", "7.90", "Primary frontage is the higher-priority 8 m road."],
                ["C-P8-S4", "8 m Road | 8 m", "4 m Road | 4 m", "8 m x 4 m corner", "", "Primary frontage is the 8 m road; secondary frontage is the 4 m road."],
                ["C-P7-S7", "7 m Road | 7 m", "7 m Road | 7 m", "7 m x 7 m corner", "7.29", "Two adjacent frontages with the same road name and width on both sides."],
                ["C-P7-S6", "7 m Road | 7 m", "6 m Road | 6 m", "7 m x 6 m corner", "6.98", "Primary frontage is the higher-priority 7 m road."],
                ["C-P7-S4", "7 m Road | 7 m", "4 m Road | 4 m", "7 m x 4 m corner", "4.74", "Primary frontage is the higher-priority 7 m road."],
                ["C-P6-S6", "6 m Road | 6 m", "6 m Road | 6 m", "6 m x 6 m corner", "6.68", "Two adjacent frontages with the same road name and width on both sides."],
                ["C-P6-S4", "6 m Road | 6 m", "4 m Road | 4 m", "6 m x 4 m corner", "4.23", "Primary frontage is the higher-priority 6 m road."],
                ["C-P4-S4", "4 m Road | 4 m", "4 m Road | 4 m", "4 m x 4 m corner", "4.00", "Two adjacent frontages on 4 m roads."]
            ]);
            policy.LookupTables.Add(table);
        }

        private static void AddSpecialFacilityTable(PolicySet policy, DateTime now)
        {
            PolicyLookupTable table = CreateTable(
                "specialFacilityRates",
                "Schedule 1 Table 2(a) - Special Facility Rates",
                "Additional contribution rates for special facilities and site conditions.",
                3,
                now,
                [
                    ("sn", "S.N.", "Text", null),
                    ("description", "Parcel Description", "Text", null),
                    ("ratePercent", "Additional Contribution", "Percent", "%")
                ]);
            table.PolicyClause = Clause(policy, "2.2.9");

            AddRows(table,
            [
                ["1", "Land adjoining an open space within 8 m frontage", "3.00"],
                ["2", "Land directly across the road from an open space", "2.00"],
                ["3", "Land falling in the central depression (dhaap)", "8.00"],
                ["4", "Original land with slope steeper than 30 degrees", "12.00"],
                ["5", "Corner plot with two frontages", "As per Table 2(b)"]
            ]);
            policy.LookupTables.Add(table);
        }

        private static void AddCornerPlotTable(PolicySet policy, DateTime now)
        {
            PolicyLookupTable table = CreateTable(
                "cornerContributionTable",
                "Schedule 1 Table 2(b) - Corner Plot Rates",
                "Additional contribution rates by project-defined corner type.",
                4,
                now,
                [
                    ("cornerCode", "Corner Code", "Text", null),
                    ("cornerType", "Corner Type", "Text", null),
                    ("ratePercent", "Additional Contribution", "Percent", "%")
                ]);
            table.PolicyClause = Clause(policy, "2.2.7");

            AddRows(table,
            [
                ["C-P9-S9", "9.14 m x 9.14 m corner", ""],
                ["C-P9-S8", "9.14 m x 8 m corner", "9.00"],
                ["C-P9-S7", "9.14 m x 7 m corner", ""],
                ["C-P9-S6", "9.14 m x 6 m corner", ""],
                ["C-P9-S4", "9.14 m x 4 m corner", ""],
                ["C-P8-S8", "8 m x 8 m corner", "8.61"],
                ["C-P8-S7", "8 m x 7 m corner", "8.23"],
                ["C-P8-S6", "8 m x 6 m corner", "7.90"],
                ["C-P8-S4", "8 m x 4 m corner", ""],
                ["C-P7-S7", "7 m x 7 m corner", "7.29"],
                ["C-P7-S6", "7 m x 6 m corner", "6.98"],
                ["C-P7-S4", "7 m x 4 m corner", "4.74"],
                ["C-P6-S6", "6 m x 6 m corner", "6.68"],
                ["C-P6-S4", "6 m x 4 m corner", "4.23"],
                ["C-P4-S4", "4 m x 4 m corner", "4.00"]
            ]);
            policy.LookupTables.Add(table);
        }

        private static PolicyClause? Clause(PolicySet policy, string clauseCode)
        {
            return policy.Clauses.FirstOrDefault(c =>
                string.Equals(c.ClauseCode, clauseCode, StringComparison.OrdinalIgnoreCase));
        }

        private static PolicyLookupTable CreateTable(
            string key,
            string title,
            string description,
            int order,
            DateTime now,
            IReadOnlyList<(string Key, string Header, string Type, string? Unit)> columns)
        {
            PolicyLookupTable table = new()
            {
                TableKey = key,
                Title = title,
                Description = description,
                DisplayOrder = order,
                CreatedDate = now,
                LastModifiedDate = now
            };

            for (int i = 0; i < columns.Count; i++)
            {
                table.Columns.Add(new PolicyLookupColumn
                {
                    ColumnKey = columns[i].Key,
                    HeaderText = columns[i].Header,
                    ValueType = columns[i].Type,
                    Unit = columns[i].Unit,
                    DisplayOrder = i + 1
                });
            }

            return table;
        }

        private static void AddRows(PolicyLookupTable table, IReadOnlyList<string[]> rows)
        {
            List<PolicyLookupColumn> columns = table.Columns.OrderBy(c => c.DisplayOrder).ToList();
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                PolicyLookupRow row = new()
                {
                    DisplayOrder = rowIndex + 1,
                    RowLabel = rows[rowIndex].Length > 0 ? rows[rowIndex][0] : null
                };

                for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                {
                    row.Cells.Add(new PolicyLookupCell
                    {
                        PolicyLookupColumn = columns[colIndex],
                        ValueText = colIndex < rows[rowIndex].Length ? rows[rowIndex][colIndex] : null
                    });
                }

                table.Rows.Add(row);
            }
        }
    }
}

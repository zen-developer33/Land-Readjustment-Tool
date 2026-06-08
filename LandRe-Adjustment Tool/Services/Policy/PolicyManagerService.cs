using Land_Readjustment_Tool.Core.Entities.Policy;
using Land_Readjustment_Tool.Data;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Services.Policy
{
    public sealed class PolicyManagerService
    {
        private readonly AppDbContext _context;
        private readonly PolicyValidationService _validationService;
        private readonly PolicyPackageService _packageService;
        private readonly PolicyTemplateSeeder _templateSeeder;
        private readonly SemaphoreSlim _operationGate = new(1, 1);
        private bool _seedEnsured;

        public event EventHandler<int>? PolicyChanged;

        public PolicyManagerService(
            ProjectSession session,
            PolicyValidationService validationService,
            PolicyPackageService packageService,
            PolicyTemplateSeeder templateSeeder)
        {
            _context = session.GetDbContext();
            _validationService = validationService;
            _packageService = packageService;
            _templateSeeder = templateSeeder;
        }

        public async Task EnsureSeedPolicyAsync(CancellationToken ct = default)
        {
            if (_seedEnsured)
                return;

            await _templateSeeder.EnsureBaireniTemplateAsync(ct);
            _seedEnsured = true;
        }

        public async Task<List<PolicySet>> GetPolicySummariesAsync(CancellationToken ct = default)
        {
            List<PolicySet> policies = await _context.PolicySets
                .AsNoTracking()
                .OrderBy(policy => policy.PolicyName)
                .ThenByDescending(policy => policy.VersionNo)
                .ToListAsync(ct);

            if (policies.Count > 0)
                return policies;

            await EnsureSeedPolicyAsync(ct);
            return await _context.PolicySets
                .AsNoTracking()
                .OrderBy(policy => policy.PolicyName)
                .ThenByDescending(policy => policy.VersionNo)
                .ToListAsync(ct);
        }

        public async Task<PolicySet?> GetPolicyAsync(int policySetId, CancellationToken ct = default)
        {
            return await _context.PolicySets
                .AsNoTracking()
                .AsSplitQuery()
                .Include(policy => policy.Clauses)
                .Include(policy => policy.Parameters)
                .Include(policy => policy.LookupTables)
                    .ThenInclude(table => table.PolicyClause)
                .Include(policy => policy.LookupTables)
                    .ThenInclude(table => table.Columns)
                .Include(policy => policy.LookupTables)
                    .ThenInclude(table => table.Rows)
                        .ThenInclude(row => row.Cells)
                            .ThenInclude(cell => cell.PolicyLookupColumn)
                .Include(policy => policy.Attachments)
                .Include(policy => policy.AuditEntries)
                .FirstOrDefaultAsync(policy => policy.Id == policySetId, ct);
        }

        public async Task<PolicySet?> GetPolicyDashboardAsync(int policySetId, CancellationToken ct = default)
        {
            await BackfillSectionDefinitionsAsync(policySetId, ct);
            return await _context.PolicySets
                .AsNoTracking()
                .AsSplitQuery()
                .Include(policy => policy.Clauses)
                .Include(policy => policy.Parameters)
                .Include(policy => policy.Sections)
                .FirstOrDefaultAsync(policy => policy.Id == policySetId, ct);
        }

        // ── Sections (A, B, C, ... grid above the clauses grid) ──────────────
        //
        // Sections are stored as PolicySectionDefinition rows on the policy. The
        // legacy PolicyClause.PolicySection string still holds the heading text,
        // so clauses keep working when no section row exists; on first dashboard
        // load we backfill section definitions from the clauses' distinct
        // section strings so the section grid is never empty for older policies.

        public async Task<List<PolicySectionDefinition>> GetSectionsAsync(
            int policySetId,
            CancellationToken ct = default)
        {
            await BackfillSectionDefinitionsAsync(policySetId, ct);
            return await _context.PolicySectionDefinitions
                .AsNoTracking()
                .Where(s => s.PolicySetId == policySetId)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.SectionCode)
                .ToListAsync(ct);
        }

        public async Task<PolicySectionDefinition> AddSectionAsync(
            int policySetId,
            string heading,
            CancellationToken ct = default)
        {
            PolicySet policy = await GetEditablePolicyForUpdateAsync(policySetId, ct);

            List<PolicySectionDefinition> existing = await _context.PolicySectionDefinitions
                .Where(s => s.PolicySetId == policySetId)
                .ToListAsync(ct);

            string nextCode = NextSectionCode(existing.Select(s => s.SectionCode));
            string finalHeading = string.IsNullOrWhiteSpace(heading)
                ? $"New Section {nextCode}"
                : heading.Trim();

            // Disambiguate heading if it already exists (case-insensitive).
            HashSet<string> headings = new(
                existing.Select(s => s.Heading ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);
            string headingCandidate = finalHeading;
            for (int i = 2; headings.Contains(headingCandidate); i++)
                headingCandidate = $"{finalHeading} {i}";

            DateTime now = DateTime.Now;
            PolicySectionDefinition section = new()
            {
                PolicySetId = policySetId,
                SectionCode = nextCode,
                Heading = headingCandidate,
                DisplayOrder = (existing.Count == 0 ? 0 : existing.Max(s => s.DisplayOrder)) + 1,
                CreatedDate = now,
                LastModifiedDate = now
            };
            _context.PolicySectionDefinitions.Add(section);

            policy.LastModifiedDate = now;
            AddAudit(policySetId, "Saved Draft", $"Section {nextCode} added.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policySetId);
            return section;
        }

        public async Task RenameSectionAsync(
            int sectionId,
            string newHeading,
            CancellationToken ct = default)
        {
            PolicySectionDefinition? section = await _context.PolicySectionDefinitions
                .FirstOrDefaultAsync(s => s.Id == sectionId, ct);
            if (section == null)
                return;

            PolicySet policy = await GetEditablePolicyForUpdateAsync(section.PolicySetId, ct);
            string oldHeading = section.Heading;
            string trimmed = (newHeading ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(trimmed) ||
                string.Equals(trimmed, oldHeading, StringComparison.Ordinal))
                return;

            section.Heading = trimmed;
            section.LastModifiedDate = DateTime.Now;

            // Cascade the heading change to every clause currently tagged with the old heading.
            List<PolicyClause> linkedClauses = await _context.PolicyClauses
                .Where(c => c.PolicySetId == policy.Id && c.PolicySection == oldHeading)
                .ToListAsync(ct);
            foreach (PolicyClause clause in linkedClauses)
            {
                clause.PolicySection = trimmed;
                clause.LastModifiedDate = section.LastModifiedDate;
            }

            policy.LastModifiedDate = section.LastModifiedDate;
            AddAudit(policy.Id, "Saved Draft", $"Section {section.SectionCode} renamed.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policy.Id);
        }

        public async Task DeleteSectionAsync(int sectionId, CancellationToken ct = default)
        {
            PolicySectionDefinition? section = await _context.PolicySectionDefinitions
                .FirstOrDefaultAsync(s => s.Id == sectionId, ct);
            if (section == null)
                return;

            PolicySet policy = await GetEditablePolicyForUpdateAsync(section.PolicySetId, ct);

            // Refuse to delete a section that still has clauses; the caller surfaces a message.
            bool hasClauses = await _context.PolicyClauses
                .AnyAsync(c => c.PolicySetId == policy.Id && c.PolicySection == section.Heading, ct);
            if (hasClauses)
                throw new InvalidOperationException(
                    $"Section '{section.SectionCode} - {section.Heading}' still has clauses. " +
                    "Delete or reassign its clauses before deleting the section.");

            string code = section.SectionCode;
            _context.PolicySectionDefinitions.Remove(section);
            policy.LastModifiedDate = DateTime.Now;
            AddAudit(policy.Id, "Saved Draft", $"Section {code} deleted.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policy.Id);
        }

        public async Task MoveSectionAsync(int sectionId, int direction, CancellationToken ct = default)
        {
            PolicySectionDefinition? section = await _context.PolicySectionDefinitions
                .FirstOrDefaultAsync(s => s.Id == sectionId, ct);
            if (section == null)
                return;

            PolicySet policy = await GetEditablePolicyForUpdateAsync(section.PolicySetId, ct);

            List<PolicySectionDefinition> ordered = await _context.PolicySectionDefinitions
                .Where(s => s.PolicySetId == policy.Id)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.SectionCode)
                .ToListAsync(ct);

            int index = ordered.FindIndex(s => s.Id == sectionId);
            int target = index + Math.Sign(direction);
            if (index < 0 || target < 0 || target >= ordered.Count)
                return;

            (ordered[index], ordered[target]) = (ordered[target], ordered[index]);
            DateTime now = DateTime.Now;
            for (int i = 0; i < ordered.Count; i++)
            {
                ordered[i].DisplayOrder = i + 1;
                ordered[i].LastModifiedDate = now;
            }
            policy.LastModifiedDate = now;
            AddAudit(policy.Id, "Saved Draft", $"Section order changed.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policy.Id);
        }

        private async Task BackfillSectionDefinitionsAsync(int policySetId, CancellationToken ct)
        {
            // Cheap check: skip if any section row exists for this policy.
            bool any = await _context.PolicySectionDefinitions
                .AsNoTracking()
                .AnyAsync(s => s.PolicySetId == policySetId, ct);
            if (any)
                return;

            List<string> headings = await _context.PolicyClauses
                .AsNoTracking()
                .Where(c => c.PolicySetId == policySetId && !string.IsNullOrEmpty(c.PolicySection))
                .Select(c => c.PolicySection)
                .Distinct()
                .ToListAsync(ct);
            if (headings.Count == 0)
                return;

            // Stable order: known canonical sections first, then alphabetic.
            string[] canonical = ["Preliminary", "Contribution Policy", "Return Policy"];
            List<string> ordered = headings
                .OrderBy(h =>
                {
                    int idx = Array.FindIndex(canonical, c => string.Equals(c, h, StringComparison.OrdinalIgnoreCase));
                    return idx < 0 ? int.MaxValue : idx;
                })
                .ThenBy(h => h, StringComparer.OrdinalIgnoreCase)
                .ToList();

            DateTime now = DateTime.Now;
            for (int i = 0; i < ordered.Count; i++)
            {
                _context.PolicySectionDefinitions.Add(new PolicySectionDefinition
                {
                    PolicySetId = policySetId,
                    SectionCode = (i + 1).ToString(),
                    Heading = ordered[i],
                    DisplayOrder = i + 1,
                    CreatedDate = now,
                    LastModifiedDate = now
                });
            }
            await _context.SaveChangesAsync(ct);
        }

        // Section codes are simple positive integers: "1", "2", "3", ...
        // The next code is the smallest positive integer not already in use.
        private static string NextSectionCode(IEnumerable<string> existing)
        {
            HashSet<int> usedNumbers = new();
            foreach (string code in existing)
            {
                if (int.TryParse(code, out int n) && n > 0)
                    usedNumbers.Add(n);
            }
            for (int i = 1; i < int.MaxValue; i++)
            {
                if (!usedNumbers.Contains(i))
                    return i.ToString();
            }
            return DateTime.Now.Ticks.ToString();
        }

        public async Task<PolicySet?> GetPolicyParametersAsync(int policySetId, CancellationToken ct = default)
        {
            return await _context.PolicySets
                .AsNoTracking()
                .AsSplitQuery()
                .Include(policy => policy.Clauses)
                .Include(policy => policy.Parameters)
                .FirstOrDefaultAsync(policy => policy.Id == policySetId, ct);
        }

        public async Task<PolicySet?> GetPolicyLookupTablesAsync(int policySetId, CancellationToken ct = default)
        {
            await RemoveDuplicateLookupTablesAsync(policySetId, ct);
            return await _context.PolicySets
                .AsNoTracking()
                .AsSplitQuery()
                .Include(policy => policy.Clauses)
                .Include(policy => policy.LookupTables)
                    .ThenInclude(table => table.PolicyClause)
                .Include(policy => policy.LookupTables)
                    .ThenInclude(table => table.Columns)
                .Include(policy => policy.LookupTables)
                    .ThenInclude(table => table.Rows)
                        .ThenInclude(row => row.Cells)
                            .ThenInclude(cell => cell.PolicyLookupColumn)
                .FirstOrDefaultAsync(policy => policy.Id == policySetId, ct);
        }

        public async Task<byte[]?> GetPolicyAttachmentImageDataAsync(
            int policySetId,
            int? clauseId,
            CancellationToken ct = default)
        {
            PolicyAttachment? attachment = null;

            if (clauseId.HasValue)
            {
                attachment = await _context.PolicyAttachments
                    .AsNoTracking()
                    .Where(a => a.PolicySetId == policySetId && a.PolicyClauseId == clauseId.Value)
                    .OrderByDescending(a => a.CreatedDate)
                    .FirstOrDefaultAsync(ct);
            }

            attachment ??= await _context.PolicyAttachments
                .AsNoTracking()
                .Where(a => a.PolicySetId == policySetId && a.PolicyClauseId == null)
                .OrderByDescending(a => a.CreatedDate)
                .FirstOrDefaultAsync(ct);

            return attachment?.ImageData;
        }

        public async Task<T> RunExclusiveAsync<T>(Func<Task<T>> operation)
        {
            await _operationGate.WaitAsync();
            try
            {
                return await operation();
            }
            finally
            {
                _operationGate.Release();
            }
        }

        public async Task RunExclusiveAsync(Func<Task> operation)
        {
            await _operationGate.WaitAsync();
            try
            {
                await operation();
            }
            finally
            {
                _operationGate.Release();
            }
        }

        public List<string> ValidateLoadedPolicy(PolicySet policy, bool approvalMode)
        {
            return approvalMode
                ? _validationService.ValidateForApproval(policy)
                : _validationService.ValidateDraft(policy);
        }

        public async Task<List<string>> GetProjectRoadReferenceOptionsAsync(CancellationToken ct = default)
        {
            List<(string RoadName, double RoadWidth, double? RightOfWayWidth)> roads = await _context.Roads
                .AsNoTracking()
                .Select(road => new ValueTuple<string, double, double?>(
                    road.RoadName,
                    road.RoadWidth,
                    road.RightOfWayWidth))
                .ToListAsync(ct);

            List<string> options = roads
                .Select(road =>
                {
                    string roadName = string.IsNullOrWhiteSpace(road.RoadName)
                        ? "Road"
                        : road.RoadName.Trim();
                    double width = road.RightOfWayWidth ?? road.RoadWidth;
                    return width > 0
                        ? $"{roadName} | {width:0.##} m"
                        : roadName;
                })
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(option => option, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (options.Count == 0)
            {
                options.AddRange(
                [
                    "9.14 m Road | 9.14 m",
                    "8 m Road | 8 m",
                    "7 m Road | 7 m",
                    "6 m Road | 6 m",
                    "4 m Road | 4 m"
                ]);
            }

            return options;
        }

        public async Task EnsureCornerTypeDefinitionsAsync(int policySetId, CancellationToken ct = default)
        {
            PolicySet policy = await _context.PolicySets.FirstAsync(p => p.Id == policySetId, ct);
            if (!PolicyValidationService.IsEditable(policy))
                return;

            PolicyLookupTable? table = await _context.PolicyLookupTables
                .Include(t => t.Columns)
                .Include(t => t.Rows)
                    .ThenInclude(r => r.Cells)
                        .ThenInclude(c => c.PolicyLookupColumn)
                .FirstOrDefaultAsync(t =>
                    t.PolicySetId == policySetId &&
                    t.TableKey == "cornerTypeDefinitions",
                    ct);
            if (table == null)
                return;

            List<string> roadReferences = await GetProjectRoadReferenceOptionsAsync(ct);
            if (roadReferences.Count == 0)
                return;

            List<string[]> expectedRows = BuildCornerTypeRows(roadReferences);
            List<PolicyLookupColumn> columns = table.Columns.OrderBy(c => c.DisplayOrder).ToList();
            if (CornerRowsMatch(table, columns, expectedRows))
                return;

            _context.PolicyLookupRows.RemoveRange(table.Rows);
            table.Rows.Clear();
            AddLookupRows(table, columns, expectedRows);
            table.LastModifiedDate = DateTime.Now;
            AddAudit(policySetId, "Saved Draft", "Corner type combinations regenerated from project road names and widths.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policySetId);
        }

        public async Task SaveCornerRoadReferenceCellAsync(int cellId, string? valueText, CancellationToken ct = default)
        {
            PolicyLookupCell cell = await _context.PolicyLookupCells
                .Include(c => c.PolicyLookupColumn)
                .Include(c => c.PolicyLookupRow)
                    .ThenInclude(row => row.Cells)
                        .ThenInclude(rowCell => rowCell.PolicyLookupColumn)
                .Include(c => c.PolicyLookupRow)
                    .ThenInclude(row => row.PolicyLookupTable)
                .FirstAsync(c => c.Id == cellId, ct);

            await EnsurePolicyEditableAsync(cell.PolicyLookupRow.PolicyLookupTable.PolicySetId, ct);
            if (!IsCornerRoadReferenceColumn(cell.PolicyLookupColumn))
                throw new InvalidOperationException("Only primary and secondary frontage roads are editable in the corner type table.");

            cell.ValueText = valueText;
            NormalizeCornerTypeRow(cell.PolicyLookupRow);
            AddAudit(cell.PolicyLookupRow.PolicyLookupTable.PolicySetId, "Saved Draft", "Corner type frontage roads saved.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(cell.PolicyLookupRow.PolicyLookupTable.PolicySetId);
        }

        public async Task<PolicySet> CreatePolicyAsync(
            string policyName,
            string policyCode,
            CancellationToken ct = default)
        {
            DateTime now = DateTime.Now;
            PolicySet policy = new()
            {
                PolicyGroupKey = Guid.NewGuid().ToString("N"),
                PolicyCode = policyCode,
                PolicyName = policyName,
                PolicyType = "Combined",
                VersionNo = 1,
                Status = PolicyStatuses.Draft,
                CreatedDate = now,
                LastModifiedDate = now
            };

            policy.AuditEntries.Add(new PolicyAuditEntry
            {
                Action = "Created",
                Details = "Blank policy created.",
                CreatedDate = now
            });

            await _context.PolicySets.AddAsync(policy, ct);
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policy.Id);
            return policy;
        }

        public async Task<PolicySet> CopyPolicyAsDraftAsync(int policySetId, string policyName, CancellationToken ct = default)
        {
            PolicySet source = await GetPolicyAsync(policySetId, ct)
                ?? throw new InvalidOperationException("Policy not found.");

            PolicyPackage package = _packageService.ToPackage(source);
            PolicySet draft = _packageService.ToEntity(package);
            draft.PolicyGroupKey = Guid.NewGuid().ToString("N");
            draft.PolicyCode = $"POL-{DateTime.Now:yyyyMMddHHmm}";
            draft.PolicyName = string.IsNullOrWhiteSpace(policyName)
                ? $"{source.PolicyName} Copy"
                : policyName.Trim();
            draft.VersionNo = 1;
            draft.Status = PolicyStatuses.Draft;
            draft.IsLocked = false;
            draft.ApprovedDate = null;
            draft.AuditEntries.Clear();
            draft.AuditEntries.Add(new PolicyAuditEntry
            {
                Action = "Copied",
                Details = $"Draft policy copied from policy #{source.Id}.",
                CreatedDate = DateTime.Now
            });

            await _context.PolicySets.AddAsync(draft, ct);
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(draft.Id);
            return draft;
        }

        public async Task RenamePolicyAsync(int policySetId, string policyName, CancellationToken ct = default)
        {
            PolicySet existing = await GetEditablePolicyForUpdateAsync(policySetId, ct);
            existing.PolicyName = string.IsNullOrWhiteSpace(policyName)
                ? existing.PolicyName
                : policyName.Trim();
            existing.LastModifiedDate = DateTime.Now;
            AddAudit(existing.Id, "Saved Draft", "Policy name saved.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(existing.Id);
        }

        public async Task DeletePolicyAsync(int policySetId, CancellationToken ct = default)
        {
            // Existence check is a single indexed lookup — avoids loading the whole graph
            // (clauses, parameters, lookup tables/columns/rows/cells, attachment BLOBs,
            // audit entries) just to delete it. The Baireni-seeded policy hydrates several
            // image BLOBs from PolicyAttachments, which is what was making this slow.
            bool exists = await _context.PolicySets
                .AsNoTracking()
                .AnyAsync(p => p.Id == policySetId, ct);
            if (!exists)
                return;

            // One bulk DELETE per table, server-side, no entity hydration.
            // Cells must go before rows; rows/columns before tables.
            await _context.PolicyLookupCells
                .Where(c => c.PolicyLookupRow.PolicyLookupTable.PolicySetId == policySetId)
                .ExecuteDeleteAsync(ct);
            await _context.PolicyLookupRows
                .Where(r => r.PolicyLookupTable.PolicySetId == policySetId)
                .ExecuteDeleteAsync(ct);
            await _context.PolicyLookupColumns
                .Where(c => c.PolicyLookupTable.PolicySetId == policySetId)
                .ExecuteDeleteAsync(ct);
            await _context.PolicyLookupTables
                .Where(t => t.PolicySetId == policySetId)
                .ExecuteDeleteAsync(ct);
            await _context.PolicyParameters
                .Where(p => p.PolicySetId == policySetId)
                .ExecuteDeleteAsync(ct);
            await _context.PolicyAttachments
                .Where(a => a.PolicySetId == policySetId)
                .ExecuteDeleteAsync(ct);
            await _context.PolicyAuditEntries
                .Where(a => a.PolicySetId == policySetId)
                .ExecuteDeleteAsync(ct);
            await _context.PolicyClauses
                .Where(c => c.PolicySetId == policySetId)
                .ExecuteDeleteAsync(ct);
            await _context.PolicySectionDefinitions
                .Where(s => s.PolicySetId == policySetId)
                .ExecuteDeleteAsync(ct);
            await _context.PolicySets
                .Where(p => p.Id == policySetId)
                .ExecuteDeleteAsync(ct);

            // Don't notify PolicyChanged for the deleted id — the dashboard's handler would
            // then re-fetch the just-deleted policy through the same operation gate, adding
            // a round-trip. The list dialog reloads its own list after the await returns.
        }

        public async Task SavePolicyMetadataAsync(PolicySet policy, CancellationToken ct = default)
        {
            PolicySet existing = await GetEditablePolicyForUpdateAsync(policy.Id, ct);
            existing.PolicyCode = policy.PolicyCode;
            existing.PolicyName = policy.PolicyName;
            existing.PolicyType = policy.PolicyType;
            existing.EffectiveFrom = policy.EffectiveFrom;
            existing.EffectiveTo = policy.EffectiveTo;
            existing.SourceTitle = policy.SourceTitle;
            existing.SourceReference = policy.SourceReference;
            existing.Notes = policy.Notes;
            existing.LastModifiedDate = DateTime.Now;
            AddAudit(existing.Id, "Saved Draft", "Policy metadata saved.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(existing.Id);
        }

        public async Task<PolicyClause> SaveClauseAsync(
            PolicyClause clause,
            CancellationToken ct = default)
        {
            await EnsurePolicyEditableAsync(clause.PolicySetId, ct);
            if (clause.Id == 0)
            {
                clause.DisplayOrder = await NextClauseDisplayOrderAsync(
                    clause.PolicySetId,
                    clause.ParentClauseId,
                    clause.PolicySection,
                    ct);
                clause.CreatedDate = DateTime.Now;
                clause.LastModifiedDate = DateTime.Now;
                await _context.PolicyClauses.AddAsync(clause, ct);
            }
            else
            {
                PolicyClause existing = await _context.PolicyClauses
                    .FirstAsync(c => c.Id == clause.Id, ct);
                existing.ParentClauseId = clause.ParentClauseId;
                existing.ClauseCode = clause.ClauseCode;
                existing.Heading = clause.Heading;
                existing.Description = clause.Description;
                existing.PolicySection = clause.PolicySection;
                existing.DisplayOrder = clause.DisplayOrder;
                existing.LastModifiedDate = DateTime.Now;
            }

            AddAudit(clause.PolicySetId, "Saved Draft", "Clause saved.");
            await _context.SaveChangesAsync(ct);
            await NormalizeClauseNumberingAsync(clause.PolicySetId, ct);
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(clause.PolicySetId);
            return clause;
        }

        public async Task DeleteClauseAsync(int clauseId, CancellationToken ct = default)
        {
            PolicyClause? clause = await _context.PolicyClauses.FirstOrDefaultAsync(c => c.Id == clauseId, ct);
            if (clause == null)
                return;

            await EnsurePolicyEditableAsync(clause.PolicySetId, ct);
            await DeleteClauseTreeAsync(clause.Id, ct);
            AddAudit(clause.PolicySetId, "Saved Draft", "Clause deleted.");
            await _context.SaveChangesAsync(ct);
            await NormalizeClauseNumberingAsync(clause.PolicySetId, ct);
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(clause.PolicySetId);
        }

        public async Task<PolicyClause?> DuplicateClauseAsync(int clauseId, CancellationToken ct = default)
        {
            PolicyClause? source = await _context.PolicyClauses.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == clauseId, ct);
            if (source == null)
                return null;

            await EnsurePolicyEditableAsync(source.PolicySetId, ct);
            List<PolicyClause> laterSiblings = await _context.PolicyClauses
                .Where(c => c.PolicySetId == source.PolicySetId &&
                            c.ParentClauseId == source.ParentClauseId &&
                            (source.ParentClauseId != null || c.PolicySection == source.PolicySection) &&
                            c.DisplayOrder > source.DisplayOrder)
                .ToListAsync(ct);

            foreach (PolicyClause sibling in laterSiblings)
                sibling.DisplayOrder++;

            PolicyClause duplicate = new()
            {
                PolicySetId = source.PolicySetId,
                ParentClauseId = source.ParentClauseId,
                ClauseCode = null,
                Heading = $"{source.Heading} Copy",
                Description = source.Description,
                PolicySection = source.PolicySection,
                DisplayOrder = source.DisplayOrder + 1,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };
            await _context.PolicyClauses.AddAsync(duplicate, ct);
            AddAudit(source.PolicySetId, "Saved Draft", "Clause duplicated.");
            await _context.SaveChangesAsync(ct);
            await NormalizeClauseNumberingAsync(source.PolicySetId, ct);
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(source.PolicySetId);
            return duplicate;
        }

        public async Task MoveClauseAsync(int clauseId, int direction, CancellationToken ct = default)
        {
            PolicyClause? clause = await _context.PolicyClauses.FirstOrDefaultAsync(c => c.Id == clauseId, ct);
            if (clause == null)
                return;

            await EnsurePolicyEditableAsync(clause.PolicySetId, ct);
            List<PolicyClause> siblings = await _context.PolicyClauses
                .Where(c => c.PolicySetId == clause.PolicySetId && c.ParentClauseId == clause.ParentClauseId)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync(ct);

            if (clause.ParentClauseId == null)
                siblings = siblings
                    .Where(c => string.Equals(c.PolicySection, clause.PolicySection, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            int index = siblings.FindIndex(c => c.Id == clauseId);
            int targetIndex = index + Math.Sign(direction);
            if (index < 0 || targetIndex < 0 || targetIndex >= siblings.Count)
                return;

            (siblings[index].DisplayOrder, siblings[targetIndex].DisplayOrder) =
                (siblings[targetIndex].DisplayOrder, siblings[index].DisplayOrder);

            AddAudit(clause.PolicySetId, "Saved Draft", "Clause order changed.");
            await _context.SaveChangesAsync(ct);
            await NormalizeClauseNumberingAsync(clause.PolicySetId, ct);
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(clause.PolicySetId);
        }

        public async Task<PolicyParameter> SaveParameterAsync(
            PolicyParameter parameter,
            CancellationToken ct = default)
        {
            await EnsurePolicyEditableAsync(parameter.PolicySetId, ct);
            if (parameter.Id == 0)
            {
                parameter.CreatedDate = DateTime.Now;
                parameter.LastModifiedDate = DateTime.Now;
                await _context.PolicyParameters.AddAsync(parameter, ct);
            }
            else
            {
                PolicyParameter existing = await _context.PolicyParameters
                    .FirstAsync(p => p.Id == parameter.Id, ct);
                existing.PolicyClauseId = parameter.PolicyClauseId;
                existing.ParameterKey = parameter.ParameterKey;
                existing.Label = parameter.Label;
                existing.ValueType = parameter.ValueType;
                existing.ValueText = parameter.ValueText;
                existing.DefaultValueText = parameter.DefaultValueText;
                existing.Unit = parameter.Unit;
                existing.Description = parameter.Description;
                existing.MinValueText = parameter.MinValueText;
                existing.MaxValueText = parameter.MaxValueText;
                existing.DisplayOrder = parameter.DisplayOrder;
                existing.LastModifiedDate = DateTime.Now;
            }

            AddAudit(parameter.PolicySetId, "Saved Draft", "Parameter saved.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(parameter.PolicySetId);
            return parameter;
        }

        public async Task DeleteParameterAsync(int parameterId, CancellationToken ct = default)
        {
            PolicyParameter? parameter = await _context.PolicyParameters.FirstOrDefaultAsync(p => p.Id == parameterId, ct);
            if (parameter == null)
                return;

            await EnsurePolicyEditableAsync(parameter.PolicySetId, ct);
            _context.PolicyParameters.Remove(parameter);
            AddAudit(parameter.PolicySetId, "Saved Draft", "Parameter deleted.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(parameter.PolicySetId);
        }

        public async Task SaveLookupCellAsync(int cellId, string? valueText, CancellationToken ct = default)
        {
            PolicyLookupCell cell = await _context.PolicyLookupCells
                .Include(c => c.PolicyLookupRow)
                    .ThenInclude(row => row.PolicyLookupTable)
                .FirstAsync(c => c.Id == cellId, ct);
            await EnsurePolicyEditableAsync(cell.PolicyLookupRow.PolicyLookupTable.PolicySetId, ct);
            cell.ValueText = valueText;
            AddAudit(cell.PolicyLookupRow.PolicyLookupTable.PolicySetId, "Saved Draft", "Lookup table cell saved.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(cell.PolicyLookupRow.PolicyLookupTable.PolicySetId);
        }

        private static List<string[]> BuildCornerTypeRows(IReadOnlyList<string> roadReferences)
        {
            List<RoadReference> roads = roadReferences
                .Select(ParseRoadReference)
                .OrderByDescending(road => road.WidthM)
                .ThenBy(road => road.Label, StringComparer.OrdinalIgnoreCase)
                .ToList();

            List<string[]> rows = [];
            for (int primaryIndex = 0; primaryIndex < roads.Count; primaryIndex++)
            {
                for (int secondaryIndex = primaryIndex; secondaryIndex < roads.Count; secondaryIndex++)
                {
                    rows.Add(BuildCornerTypeRow(roads[primaryIndex].Label, roads[secondaryIndex].Label));
                }
            }

            return rows;
        }

        private static string[] BuildCornerTypeRow(string primaryRoad, string secondaryRoad)
        {
            RoadReference primary = ParseRoadReference(primaryRoad);
            RoadReference secondary = ParseRoadReference(secondaryRoad);
            double primaryWidth = Math.Max(primary.WidthM, secondary.WidthM);
            double secondaryWidth = Math.Min(primary.WidthM, secondary.WidthM);
            string primaryLabel = primary.WidthM >= secondary.WidthM ? primary.Label : secondary.Label;
            string secondaryLabel = primary.WidthM >= secondary.WidthM ? secondary.Label : primary.Label;

            return
            [
                CornerCode(primaryWidth, secondaryWidth),
                primaryLabel,
                secondaryLabel,
                $"{FormatWidth(primaryWidth)} m x {FormatWidth(secondaryWidth)} m corner",
                DefaultCornerRate(primaryWidth, secondaryWidth),
                primaryWidth.Equals(secondaryWidth)
                    ? "Two adjacent frontages with the same road width."
                    : $"Primary frontage is the {FormatWidth(primaryWidth)} m road; secondary frontage is the {FormatWidth(secondaryWidth)} m road."
            ];
        }

        private static void NormalizeCornerTypeRow(PolicyLookupRow row)
        {
            string primaryRoad = GetCellValue(row, "primaryFrontageRoad");
            string secondaryRoad = GetCellValue(row, "secondaryFrontageRoad");
            if (string.IsNullOrWhiteSpace(primaryRoad) || string.IsNullOrWhiteSpace(secondaryRoad))
                return;

            string[] values = BuildCornerTypeRow(primaryRoad, secondaryRoad);
            SetCellValue(row, "cornerCode", values[0]);
            SetCellValue(row, "primaryFrontageRoad", values[1]);
            SetCellValue(row, "secondaryFrontageRoad", values[2]);
            SetCellValue(row, "displayName", values[3]);
            SetCellValue(row, "defaultRatePercent", values[4]);
            SetCellValue(row, "description", values[5]);
        }

        private static void AddLookupRows(PolicyLookupTable table, IReadOnlyList<PolicyLookupColumn> columns, IReadOnlyList<string[]> rows)
        {
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                PolicyLookupRow row = new()
                {
                    PolicyLookupTableId = table.Id,
                    DisplayOrder = rowIndex + 1,
                    RowLabel = rows[rowIndex][0]
                };

                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    row.Cells.Add(new PolicyLookupCell
                    {
                        PolicyLookupColumnId = columns[columnIndex].Id,
                        ValueText = columnIndex < rows[rowIndex].Length ? rows[rowIndex][columnIndex] : null
                    });
                }

                table.Rows.Add(row);
            }
        }

        private static bool CornerRowsMatch(PolicyLookupTable table, IReadOnlyList<PolicyLookupColumn> columns, IReadOnlyList<string[]> expectedRows)
        {
            List<PolicyLookupRow> rows = table.Rows.OrderBy(r => r.DisplayOrder).ToList();
            if (rows.Count != expectedRows.Count)
                return false;

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < columns.Count && columnIndex < expectedRows[rowIndex].Length; columnIndex++)
                {
                    string actual = rows[rowIndex].Cells
                        .FirstOrDefault(c => c.PolicyLookupColumnId == columns[columnIndex].Id)
                        ?.ValueText ?? string.Empty;
                    if (!string.Equals(actual, expectedRows[rowIndex][columnIndex], StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }

            return true;
        }

        private static string GetCellValue(PolicyLookupRow row, string columnKey)
        {
            return row.Cells
                .FirstOrDefault(c => string.Equals(c.PolicyLookupColumn.ColumnKey, columnKey, StringComparison.OrdinalIgnoreCase))
                ?.ValueText ?? string.Empty;
        }

        private static void SetCellValue(PolicyLookupRow row, string columnKey, string value)
        {
            PolicyLookupCell? cell = row.Cells
                .FirstOrDefault(c => string.Equals(c.PolicyLookupColumn.ColumnKey, columnKey, StringComparison.OrdinalIgnoreCase));
            if (cell != null)
                cell.ValueText = value;
        }

        private static RoadReference ParseRoadReference(string label)
        {
            double width = 0;
            string[] parts = label.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            string widthSource = parts.Length > 1 ? parts[^1] : label;
            string numeric = new(widthSource.Where(ch => char.IsDigit(ch) || ch == '.').ToArray());
            _ = double.TryParse(numeric, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out width);
            return new RoadReference(label.Trim(), width);
        }

        private static string CornerCode(double primaryWidth, double secondaryWidth)
        {
            return $"C-P{WidthToken(primaryWidth)}-S{WidthToken(secondaryWidth)}";
        }

        private static string WidthToken(double width)
        {
            double rounded = Math.Round(width);
            if (Math.Abs(width - rounded) <= 0.2)
                return rounded.ToString("0", System.Globalization.CultureInfo.InvariantCulture);

            return width.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture).Replace(".", "");
        }

        private static string FormatWidth(double width)
        {
            return width.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string DefaultCornerRate(double primaryWidth, double secondaryWidth)
        {
            int primary = (int)Math.Round(primaryWidth);
            int secondary = (int)Math.Round(secondaryWidth);
            Dictionary<(int Primary, int Secondary), string> knownRates = new()
            {
                [(9, 8)] = "9.00",
                [(8, 8)] = "8.61",
                [(8, 7)] = "8.23",
                [(8, 6)] = "7.90",
                [(7, 7)] = "7.29",
                [(7, 6)] = "6.98",
                [(6, 6)] = "6.68",
                [(7, 4)] = "4.74",
                [(6, 4)] = "4.23",
                [(4, 4)] = "4.00"
            };

            return knownRates.TryGetValue((primary, secondary), out string? rate)
                ? rate
                : string.Empty;
        }

        private static bool IsCornerRoadReferenceColumn(PolicyLookupColumn column)
        {
            return string.Equals(column.ColumnKey, "primaryFrontageRoad", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.ColumnKey, "secondaryFrontageRoad", StringComparison.OrdinalIgnoreCase);
        }

        private readonly record struct RoadReference(string Label, double WidthM);

        public async Task SaveLookupTableClauseAsync(int tableId, int? clauseId, CancellationToken ct = default)
        {
            PolicyLookupTable table = await _context.PolicyLookupTables
                .FirstAsync(t => t.Id == tableId, ct);
            await EnsurePolicyEditableAsync(table.PolicySetId, ct);
            table.PolicyClauseId = clauseId;
            table.LastModifiedDate = DateTime.Now;
            AddAudit(table.PolicySetId, "Saved Draft", "Lookup table clause association saved.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(table.PolicySetId);
        }

        public async Task<PolicyLookupRow> AddLookupRowAsync(int tableId, CancellationToken ct = default)
        {
            PolicyLookupTable table = await _context.PolicyLookupTables
                .Include(t => t.Columns)
                .Include(t => t.Rows)
                .FirstAsync(t => t.Id == tableId, ct);
            await EnsurePolicyEditableAsync(table.PolicySetId, ct);

            PolicyLookupRow row = new()
            {
                PolicyLookupTableId = table.Id,
                DisplayOrder = table.Rows.Count == 0
                    ? 1
                    : table.Rows.Max(r => r.DisplayOrder) + 1,
                RowLabel = $"Row {table.Rows.Count + 1}"
            };

            foreach (PolicyLookupColumn column in table.Columns.OrderBy(c => c.DisplayOrder))
            {
                row.Cells.Add(new PolicyLookupCell
                {
                    PolicyLookupColumnId = column.Id,
                    ValueText = string.Empty
                });
            }

            await _context.PolicyLookupRows.AddAsync(row, ct);
            AddAudit(table.PolicySetId, "Saved Draft", "Lookup table row added.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(table.PolicySetId);
            return row;
        }

        public async Task DeleteLookupRowAsync(int rowId, CancellationToken ct = default)
        {
            PolicyLookupRow? row = await _context.PolicyLookupRows
                .Include(r => r.PolicyLookupTable)
                .FirstOrDefaultAsync(r => r.Id == rowId, ct);
            if (row == null)
                return;

            await EnsurePolicyEditableAsync(row.PolicyLookupTable.PolicySetId, ct);
            _context.PolicyLookupRows.Remove(row);
            AddAudit(row.PolicyLookupTable.PolicySetId, "Saved Draft", "Lookup table row deleted.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(row.PolicyLookupTable.PolicySetId);
        }

        public async Task SetPolicyLockAsync(int policySetId, bool isLocked, CancellationToken ct = default)
        {
            PolicySet policy = await _context.PolicySets.FirstAsync(p => p.Id == policySetId, ct);
            if (string.Equals(policy.Status, PolicyStatuses.Approved, StringComparison.OrdinalIgnoreCase) && !isLocked)
                throw new InvalidOperationException("Approved policies are locked. Create a new draft version to edit.");

            policy.IsLocked = isLocked;
            policy.LastModifiedDate = DateTime.Now;
            AddAudit(policySetId, isLocked ? "Locked" : "Unlocked", isLocked ? "Draft editing locked." : "Draft editing unlocked.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policySetId);
        }

        public async Task AddAttachmentAsync(
            int policySetId,
            int? clauseId,
            string filePath,
            string? caption,
            CancellationToken ct = default)
        {
            await EnsurePolicyEditableAsync(policySetId, ct);
            byte[] bytes = await File.ReadAllBytesAsync(filePath, ct);
            PolicyAttachment attachment = new()
            {
                PolicySetId = policySetId,
                PolicyClauseId = clauseId,
                FileName = Path.GetFileName(filePath),
                ContentType = GetContentType(filePath),
                ImageData = bytes,
                Caption = caption,
                CreatedDate = DateTime.Now
            };
            await _context.PolicyAttachments.AddAsync(attachment, ct);
            AddAudit(policySetId, "Saved Draft", "Attachment added.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policySetId);
        }

        public async Task<List<string>> GetValidationIssuesAsync(int policySetId, bool approvalMode, CancellationToken ct = default)
        {
            PolicySet policy = await GetPolicyAsync(policySetId, ct)
                ?? throw new InvalidOperationException("Policy not found.");
            return approvalMode
                ? _validationService.ValidateForApproval(policy)
                : _validationService.ValidateDraft(policy);
        }

        public async Task ApprovePolicyAsync(int policySetId, CancellationToken ct = default)
        {
            PolicySet policy = await _context.PolicySets.FirstAsync(p => p.Id == policySetId, ct);
            PolicySet fullPolicy = await GetPolicyAsync(policySetId, ct)
                ?? throw new InvalidOperationException("Policy not found.");

            List<string> issues = _validationService.ValidateForApproval(fullPolicy);
            if (issues.Count > 0)
                throw new InvalidOperationException(string.Join(Environment.NewLine, issues));

            policy.Status = PolicyStatuses.Approved;
            policy.IsLocked = true;
            policy.ApprovedDate = DateTime.Now;
            policy.LastModifiedDate = DateTime.Now;
            AddAudit(policySetId, "Approved", "Policy approved and locked.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policySetId);
        }

        public async Task<PolicySet> CreateDraftFromApprovedAsync(int policySetId, CancellationToken ct = default)
        {
            PolicySet source = await GetPolicyAsync(policySetId, ct)
                ?? throw new InvalidOperationException("Policy not found.");

            if (!string.Equals(source.Status, PolicyStatuses.Approved, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Draft versions can only be created from approved policies. Use Copy for draft policies.");

            PolicyPackage package = _packageService.ToPackage(source);
            PolicySet draft = _packageService.ToEntity(package);
            draft.PolicyGroupKey = source.PolicyGroupKey;
            draft.VersionNo = await _context.PolicySets
                .Where(policy => policy.PolicyGroupKey == source.PolicyGroupKey)
                .MaxAsync(policy => policy.VersionNo, ct) + 1;
            draft.Status = PolicyStatuses.Draft;
            draft.IsLocked = false;
            draft.ApprovedDate = null;
            draft.PolicyName = $"{source.PolicyName} v{draft.VersionNo}";
            draft.AuditEntries.Clear();
            draft.AuditEntries.Add(new PolicyAuditEntry
            {
                Action = "Created Draft Version",
                Details = $"Draft version created from policy #{source.Id}.",
                CreatedDate = DateTime.Now
            });

            await _context.PolicySets.AddAsync(draft, ct);
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(draft.Id);
            return draft;
        }

        public async Task ExportAsync(int policySetId, string filePath, CancellationToken ct = default)
        {
            PolicySet policy = await GetPolicyAsync(policySetId, ct)
                ?? throw new InvalidOperationException("Policy not found.");

            await _packageService.ExportAsync(policy, filePath, ct);
            AddAudit(policySetId, "Exported", $"Policy exported to {Path.GetFileName(filePath)}.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policySetId);
        }

        public async Task<PolicySet> ImportAsync(string filePath, CancellationToken ct = default)
        {
            PolicyPackage package = await _packageService.ReadAsync(filePath, ct);
            PolicySet policy = _packageService.ToEntity(package);
            await _context.PolicySets.AddAsync(policy, ct);
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policy.Id);
            return policy;
        }

        private async Task RemoveDuplicateLookupTablesAsync(int policySetId, CancellationToken ct)
        {
            PolicySet? policy = await _context.PolicySets
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == policySetId, ct);
            if (policy == null || !PolicyValidationService.IsEditable(policy))
                return;

            List<PolicyLookupTable> tables = await _context.PolicyLookupTables
                .Include(t => t.Columns)
                .Include(t => t.Rows)
                    .ThenInclude(r => r.Cells)
                .Where(t => t.PolicySetId == policySetId)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.Id)
                .ToListAsync(ct);

            List<PolicyLookupTable> duplicates = tables
                .Where(t => !string.IsNullOrWhiteSpace(t.TableKey))
                .GroupBy(t => t.TableKey, StringComparer.OrdinalIgnoreCase)
                .SelectMany(group => group.Skip(1))
                .ToList();

            if (duplicates.Count == 0)
                return;

            _context.PolicyLookupTables.RemoveRange(duplicates);
            AddAudit(policySetId, "Saved Draft", "Duplicate lookup tables removed.");
            await _context.SaveChangesAsync(ct);
            NotifyPolicyChanged(policySetId);
        }

        private async Task<PolicySet> GetEditablePolicyForUpdateAsync(int policySetId, CancellationToken ct)
        {
            PolicySet policy = await _context.PolicySets.FirstAsync(p => p.Id == policySetId, ct);
            if (!PolicyValidationService.IsEditable(policy))
                throw new InvalidOperationException("Approved or locked policies cannot be edited. Create a draft version first.");

            return policy;
        }

        private async Task EnsurePolicyEditableAsync(int policySetId, CancellationToken ct)
        {
            _ = await GetEditablePolicyForUpdateAsync(policySetId, ct);
        }

        private void NotifyPolicyChanged(int policySetId)
        {
            PolicyChanged?.Invoke(this, policySetId);
        }

        private async Task DeleteClauseTreeAsync(int clauseId, CancellationToken ct)
        {
            List<PolicyClause> children = await _context.PolicyClauses
                .Where(c => c.ParentClauseId == clauseId)
                .ToListAsync(ct);

            foreach (PolicyClause child in children)
                await DeleteClauseTreeAsync(child.Id, ct);

            PolicyClause? clause = await _context.PolicyClauses.FirstOrDefaultAsync(c => c.Id == clauseId, ct);
            if (clause != null)
                _context.PolicyClauses.Remove(clause);
        }

        private async Task<int> NextClauseDisplayOrderAsync(
            int policySetId,
            int? parentClauseId,
            string? policySection,
            CancellationToken ct)
        {
            IQueryable<PolicyClause> siblings = _context.PolicyClauses
                .Where(c => c.PolicySetId == policySetId && c.ParentClauseId == parentClauseId);

            if (!parentClauseId.HasValue)
            {
                string section = string.IsNullOrWhiteSpace(policySection)
                    ? "General"
                    : policySection.Trim();
                siblings = siblings.Where(c => c.PolicySection == section);
            }

            int? maxOrder = await siblings
                .Select(c => (int?)c.DisplayOrder)
                .MaxAsync(ct);

            return (maxOrder ?? 0) + 1;
        }

        private async Task NormalizeClauseNumberingAsync(int policySetId, CancellationToken ct)
        {
            List<PolicyClause> clauses = await _context.PolicyClauses
                .Where(c => c.PolicySetId == policySetId)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Id)
                .ToListAsync(ct);

            Dictionary<int, List<PolicyClause>> byParent = clauses
                .GroupBy(c => c.ParentClauseId ?? 0)
                .ToDictionary(g => g.Key, g => g.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id).ToList());

            if (!byParent.TryGetValue(0, out List<PolicyClause>? roots))
                return;

            foreach (IGrouping<string, PolicyClause> sectionGroup in roots
                         .GroupBy(c => string.IsNullOrWhiteSpace(c.PolicySection) ? "General" : c.PolicySection.Trim())
                         .OrderBy(g => SectionPriority(g.Key))
                         .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
            {
                List<PolicyClause> sectionRoots = sectionGroup
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Id)
                    .ToList();
                string rootPrefix = InferRootPrefix(sectionGroup.Key, sectionRoots);

                for (int index = 0; index < sectionRoots.Count; index++)
                {
                    PolicyClause root = sectionRoots[index];
                    root.DisplayOrder = index + 1;
                    root.ClauseCode = BuildRootClauseCode(rootPrefix, index + 1);
                    root.LastModifiedDate = DateTime.Now;
                    NormalizeChildClauseNumbering(root, byParent);
                }
            }
        }

        private static void NormalizeChildClauseNumbering(
            PolicyClause parent,
            Dictionary<int, List<PolicyClause>> byParent)
        {
            if (!byParent.TryGetValue(parent.Id, out List<PolicyClause>? children))
                return;

            for (int index = 0; index < children.Count; index++)
            {
                PolicyClause child = children[index];
                child.DisplayOrder = index + 1;
                child.PolicySection = parent.PolicySection;
                child.ClauseCode = $"{parent.ClauseCode}.{index + 1}";
                child.LastModifiedDate = DateTime.Now;
                NormalizeChildClauseNumbering(child, byParent);
            }
        }

        private static string InferRootPrefix(string policySection, IReadOnlyList<PolicyClause> roots)
        {
            string? existingPrefix = roots
                .Select(root => ClauseCodePrefix(root.ClauseCode))
                .Where(prefix => !string.IsNullOrWhiteSpace(prefix))
                .GroupBy(prefix => prefix, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.Key)
                .FirstOrDefault();

            return existingPrefix ?? DefaultRootPrefix(policySection);
        }

        private static string? ClauseCodePrefix(string? clauseCode)
        {
            if (string.IsNullOrWhiteSpace(clauseCode))
                return null;

            string[] parts = clauseCode.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 1 || parts.Any(part => !int.TryParse(part, out _)))
                return null;

            return string.Join(".", parts.Take(parts.Length - 1));
        }

        private static string BuildRootClauseCode(string rootPrefix, int ordinal)
        {
            return string.IsNullOrWhiteSpace(rootPrefix)
                ? ordinal.ToString()
                : $"{rootPrefix}.{ordinal}";
        }

        private static string DefaultRootPrefix(string policySection)
        {
            if (policySection.Contains("Contribution", StringComparison.OrdinalIgnoreCase))
                return "2.2";

            if (policySection.Contains("Return", StringComparison.OrdinalIgnoreCase))
                return "2.3";

            if (policySection.Contains("Intro", StringComparison.OrdinalIgnoreCase))
                return "1";

            return "1";
        }

        private static int SectionPriority(string policySection)
        {
            if (policySection.Contains("Intro", StringComparison.OrdinalIgnoreCase))
                return 0;

            if (policySection.Contains("Contribution", StringComparison.OrdinalIgnoreCase))
                return 1;

            if (policySection.Contains("Return", StringComparison.OrdinalIgnoreCase))
                return 2;

            return 10;
        }

        private void AddAudit(int policySetId, string action, string details)
        {
            _context.PolicyAuditEntries.Add(new PolicyAuditEntry
            {
                PolicySetId = policySetId,
                Action = action,
                Details = details,
                CreatedDate = DateTime.Now
            });
        }

        private static string GetContentType(string filePath)
        {
            return Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "image/png"
            };
        }
    }
}

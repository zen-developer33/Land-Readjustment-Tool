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
            return await _context.PolicySets
                .AsNoTracking()
                .AsSplitQuery()
                .Include(policy => policy.Clauses)
                .FirstOrDefaultAsync(policy => policy.Id == policySetId, ct);
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
            return policy;
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
        }

        public async Task<PolicyClause> SaveClauseAsync(
            PolicyClause clause,
            CancellationToken ct = default)
        {
            await EnsurePolicyEditableAsync(clause.PolicySetId, ct);
            if (clause.Id == 0)
            {
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
        }

        public async Task<PolicyClause?> DuplicateClauseAsync(int clauseId, CancellationToken ct = default)
        {
            PolicyClause? source = await _context.PolicyClauses.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == clauseId, ct);
            if (source == null)
                return null;

            await EnsurePolicyEditableAsync(source.PolicySetId, ct);
            PolicyClause duplicate = new()
            {
                PolicySetId = source.PolicySetId,
                ParentClauseId = source.ParentClauseId,
                ClauseCode = string.IsNullOrWhiteSpace(source.ClauseCode)
                    ? null
                    : $"{source.ClauseCode}-copy",
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

            int index = siblings.FindIndex(c => c.Id == clauseId);
            int targetIndex = index + Math.Sign(direction);
            if (index < 0 || targetIndex < 0 || targetIndex >= siblings.Count)
                return;

            (siblings[index].DisplayOrder, siblings[targetIndex].DisplayOrder) =
                (siblings[targetIndex].DisplayOrder, siblings[index].DisplayOrder);

            AddAudit(clause.PolicySetId, "Saved Draft", "Clause order changed.");
            await _context.SaveChangesAsync(ct);
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
        }

        public async Task<PolicySet> CreateDraftFromApprovedAsync(int policySetId, CancellationToken ct = default)
        {
            PolicySet source = await GetPolicyAsync(policySetId, ct)
                ?? throw new InvalidOperationException("Policy not found.");

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
            return draft;
        }

        public async Task ExportAsync(int policySetId, string filePath, CancellationToken ct = default)
        {
            PolicySet policy = await GetPolicyAsync(policySetId, ct)
                ?? throw new InvalidOperationException("Policy not found.");

            await _packageService.ExportAsync(policy, filePath, ct);
            AddAudit(policySetId, "Exported", $"Policy exported to {Path.GetFileName(filePath)}.");
            await _context.SaveChangesAsync(ct);
        }

        public async Task<PolicySet> ImportAsync(string filePath, CancellationToken ct = default)
        {
            PolicyPackage package = await _packageService.ReadAsync(filePath, ct);
            PolicySet policy = _packageService.ToEntity(package);
            await _context.PolicySets.AddAsync(policy, ct);
            await _context.SaveChangesAsync(ct);
            return policy;
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

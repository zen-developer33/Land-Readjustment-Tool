using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Policy;

namespace Land_Readjustment_Tool.Services.Policy
{
    public sealed class PolicyPackageService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public async Task ExportAsync(
            PolicySet policy,
            string filePath,
            CancellationToken ct = default)
        {
            PolicyPackage package = ToPackage(policy);
            await using FileStream stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, package, JsonOptions, ct);
        }

        public async Task<PolicyPackage> ReadAsync(
            string filePath,
            CancellationToken ct = default)
        {
            await using FileStream stream = File.OpenRead(filePath);
            PolicyPackage? package =
                await JsonSerializer.DeserializeAsync<PolicyPackage>(stream, JsonOptions, ct);

            return package ?? throw new InvalidOperationException("Policy package could not be read.");
        }

        public PolicySet ToEntity(
            PolicyPackage package,
            bool forceDraftCopy = true)
        {
            DateTime now = DateTime.Now;
            PolicySet policy = new()
            {
                PolicyGroupKey = string.IsNullOrWhiteSpace(package.Policy.PolicyGroupKey)
                    ? Guid.NewGuid().ToString("N")
                    : $"{package.Policy.PolicyGroupKey}-import-{now:yyyyMMddHHmmss}",
                PolicyCode = package.Policy.PolicyCode,
                PolicyName = package.Policy.PolicyName,
                PolicyType = package.Policy.PolicyType,
                VersionNo = 1,
                Status = forceDraftCopy ? PolicyStatuses.Draft : package.Policy.Status,
                IsLocked = !forceDraftCopy && package.Policy.IsLocked,
                EffectiveFrom = package.Policy.EffectiveFrom,
                EffectiveTo = package.Policy.EffectiveTo,
                SourceTitle = package.Policy.SourceTitle,
                SourceReference = package.Policy.SourceReference,
                Notes = package.Policy.Notes,
                CreatedDate = now,
                LastModifiedDate = now
            };

            Dictionary<int, PolicyClause> clausesByLocalId = [];
            foreach (PolicyClausePackage clausePackage in package.Clauses.OrderBy(c => c.DisplayOrder))
            {
                PolicyClause clause = new()
                {
                    ClauseCode = clausePackage.ClauseCode,
                    Heading = clausePackage.Heading,
                    Description = clausePackage.Description,
                    PolicySection = clausePackage.PolicySection,
                    DisplayOrder = clausePackage.DisplayOrder,
                    CreatedDate = now,
                    LastModifiedDate = now
                };
                clausesByLocalId[clausePackage.LocalId] = clause;
                policy.Clauses.Add(clause);
            }

            foreach (PolicyClausePackage clausePackage in package.Clauses)
            {
                if (clausePackage.ParentLocalId.HasValue &&
                    clausesByLocalId.TryGetValue(clausePackage.LocalId, out PolicyClause clause) &&
                    clausesByLocalId.TryGetValue(clausePackage.ParentLocalId.Value, out PolicyClause parent))
                {
                    clause.ParentClause = parent;
                }
            }

            foreach (PolicyParameterPackage parameterPackage in package.Parameters)
            {
                PolicyParameter parameter = new()
                {
                    ParameterKey = parameterPackage.ParameterKey,
                    Label = parameterPackage.Label,
                    ValueType = parameterPackage.ValueType,
                    ValueText = parameterPackage.ValueText,
                    DefaultValueText = parameterPackage.DefaultValueText,
                    Unit = parameterPackage.Unit,
                    Description = parameterPackage.Description,
                    MinValueText = parameterPackage.MinValueText,
                    MaxValueText = parameterPackage.MaxValueText,
                    DisplayOrder = parameterPackage.DisplayOrder,
                    CreatedDate = now,
                    LastModifiedDate = now
                };

                if (parameterPackage.ClauseLocalId.HasValue &&
                    clausesByLocalId.TryGetValue(parameterPackage.ClauseLocalId.Value, out PolicyClause clause))
                {
                    parameter.PolicyClause = clause;
                }

                policy.Parameters.Add(parameter);
            }

            foreach (PolicyLookupTablePackage tablePackage in package.LookupTables.OrderBy(t => t.DisplayOrder))
            {
                PolicyLookupTable table = new()
                {
                    TableKey = tablePackage.TableKey,
                    Title = tablePackage.Title,
                    Description = tablePackage.Description,
                    DisplayOrder = tablePackage.DisplayOrder,
                    CreatedDate = now,
                    LastModifiedDate = now
                };

                if (tablePackage.ClauseLocalId.HasValue &&
                    clausesByLocalId.TryGetValue(tablePackage.ClauseLocalId.Value, out PolicyClause tableClause))
                {
                    table.PolicyClause = tableClause;
                }

                foreach (PolicyLookupColumnPackage columnPackage in tablePackage.Columns.OrderBy(c => c.DisplayOrder))
                {
                    table.Columns.Add(new PolicyLookupColumn
                    {
                        ColumnKey = columnPackage.ColumnKey,
                        HeaderText = columnPackage.HeaderText,
                        ValueType = columnPackage.ValueType,
                        Unit = columnPackage.Unit,
                        DisplayOrder = columnPackage.DisplayOrder
                    });
                }

                foreach (PolicyLookupRowPackage rowPackage in tablePackage.Rows.OrderBy(r => r.DisplayOrder))
                {
                    PolicyLookupRow row = new()
                    {
                        RowLabel = rowPackage.RowLabel,
                        DisplayOrder = rowPackage.DisplayOrder
                    };
                    foreach (PolicyLookupColumn column in table.Columns)
                    {
                        row.Cells.Add(new PolicyLookupCell
                        {
                            PolicyLookupColumn = column,
                            ValueText = rowPackage.Values.TryGetValue(column.ColumnKey, out string? value)
                                ? value
                                : null
                        });
                    }
                    table.Rows.Add(row);
                }

                policy.LookupTables.Add(table);
            }

            foreach (PolicyAttachmentPackage attachmentPackage in package.Attachments)
            {
                PolicyAttachment attachment = new()
                {
                    FileName = attachmentPackage.FileName,
                    ContentType = attachmentPackage.ContentType,
                    ImageData = string.IsNullOrWhiteSpace(attachmentPackage.ImageDataBase64)
                        ? []
                        : Convert.FromBase64String(attachmentPackage.ImageDataBase64),
                    Caption = attachmentPackage.Caption,
                    CreatedDate = now
                };

                if (attachmentPackage.ClauseLocalId.HasValue &&
                    clausesByLocalId.TryGetValue(attachmentPackage.ClauseLocalId.Value, out PolicyClause clause))
                {
                    attachment.PolicyClause = clause;
                }

                policy.Attachments.Add(attachment);
            }

            policy.AuditEntries.Add(new PolicyAuditEntry
            {
                Action = "Imported",
                Details = "Policy package imported as draft copy.",
                CreatedDate = now
            });

            return policy;
        }

        public PolicyPackage ToPackage(PolicySet policy)
        {
            Dictionary<int, int> localClauseIds = policy.Clauses
                .OrderBy(c => c.DisplayOrder)
                .Select((clause, index) => (clause, localId: index + 1))
                .ToDictionary(item => item.clause.Id, item => item.localId);

            return new PolicyPackage
            {
                Policy = new PolicySetPackage
                {
                    PolicyGroupKey = policy.PolicyGroupKey,
                    PolicyCode = policy.PolicyCode,
                    PolicyName = policy.PolicyName,
                    PolicyType = policy.PolicyType,
                    VersionNo = policy.VersionNo,
                    Status = policy.Status,
                    IsLocked = policy.IsLocked,
                    EffectiveFrom = policy.EffectiveFrom,
                    EffectiveTo = policy.EffectiveTo,
                    SourceTitle = policy.SourceTitle,
                    SourceReference = policy.SourceReference,
                    Notes = policy.Notes
                },
                Clauses = policy.Clauses
                    .OrderBy(c => c.DisplayOrder)
                    .Select(clause => new PolicyClausePackage
                    {
                        LocalId = localClauseIds[clause.Id],
                        ParentLocalId = clause.ParentClauseId.HasValue &&
                                        localClauseIds.TryGetValue(clause.ParentClauseId.Value, out int parentLocalId)
                            ? parentLocalId
                            : null,
                        ClauseCode = clause.ClauseCode,
                        Heading = clause.Heading,
                        Description = clause.Description,
                        PolicySection = clause.PolicySection,
                        DisplayOrder = clause.DisplayOrder
                    })
                    .ToList(),
                Parameters = policy.Parameters
                    .OrderBy(p => p.DisplayOrder)
                    .Select(parameter => new PolicyParameterPackage
                    {
                        ClauseLocalId = parameter.PolicyClauseId.HasValue &&
                                        localClauseIds.TryGetValue(parameter.PolicyClauseId.Value, out int clauseLocalId)
                            ? clauseLocalId
                            : null,
                        ParameterKey = parameter.ParameterKey,
                        Label = parameter.Label,
                        ValueType = parameter.ValueType,
                        ValueText = parameter.ValueText,
                        DefaultValueText = parameter.DefaultValueText,
                        Unit = parameter.Unit,
                        Description = parameter.Description,
                        MinValueText = parameter.MinValueText,
                        MaxValueText = parameter.MaxValueText,
                        DisplayOrder = parameter.DisplayOrder
                    })
                    .ToList(),
                LookupTables = policy.LookupTables
                    .OrderBy(t => t.DisplayOrder)
                    .Select(table => ToPackage(table, localClauseIds))
                    .ToList(),
                Attachments = policy.Attachments
                    .Select(attachment => new PolicyAttachmentPackage
                    {
                        ClauseLocalId = attachment.PolicyClauseId.HasValue &&
                                        localClauseIds.TryGetValue(attachment.PolicyClauseId.Value, out int clauseLocalId)
                            ? clauseLocalId
                            : null,
                        FileName = attachment.FileName,
                        ContentType = attachment.ContentType,
                        ImageDataBase64 = Convert.ToBase64String(attachment.ImageData),
                        Caption = attachment.Caption
                    })
                    .ToList()
            };
        }

        private static PolicyLookupTablePackage ToPackage(
            PolicyLookupTable table,
            IReadOnlyDictionary<int, int> localClauseIds)
        {
            List<PolicyLookupColumn> columns = table.Columns.OrderBy(c => c.DisplayOrder).ToList();
            return new PolicyLookupTablePackage
            {
                ClauseLocalId = table.PolicyClauseId.HasValue &&
                                localClauseIds.TryGetValue(table.PolicyClauseId.Value, out int clauseLocalId)
                    ? clauseLocalId
                    : null,
                TableKey = table.TableKey,
                Title = table.Title,
                Description = table.Description,
                DisplayOrder = table.DisplayOrder,
                Columns = columns.Select(column => new PolicyLookupColumnPackage
                {
                    ColumnKey = column.ColumnKey,
                    HeaderText = column.HeaderText,
                    ValueType = column.ValueType,
                    Unit = column.Unit,
                    DisplayOrder = column.DisplayOrder
                }).ToList(),
                Rows = table.Rows
                    .OrderBy(r => r.DisplayOrder)
                    .Select(row => new PolicyLookupRowPackage
                    {
                        RowLabel = row.RowLabel,
                        DisplayOrder = row.DisplayOrder,
                        Values = columns.ToDictionary(
                            column => column.ColumnKey,
                            column => row.Cells.FirstOrDefault(cell => cell.PolicyLookupColumnId == column.Id)?.ValueText)
                    })
                    .ToList()
            };
        }
    }
}

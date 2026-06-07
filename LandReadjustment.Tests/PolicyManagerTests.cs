using Land_Readjustment_Tool.Core.Entities.Policy;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Logging;
using Land_Readjustment_Tool.Services.Policy;
using Land_Readjustment_Tool.Services.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Xunit;

namespace LandReadjustment.Tests
{
    public sealed class PolicyManagerTests
    {
        [Fact]
        public async Task BaireniSeedCreatesDraftPolicyWithClausesParametersAndLookupTables()
        {
            await using TestProject project = await TestProject.CreateAsync();
            PolicyManagerService service = project.CreateService();

            await service.EnsureSeedPolicyAsync();
            PolicySet policy = (await service.GetPolicySummariesAsync()).Single();
            PolicySet details = await service.GetPolicyAsync(policy.Id) ?? throw new InvalidOperationException();

            Assert.Equal(PolicyStatuses.Draft, details.Status);
            Assert.Contains(details.Clauses, c => c.ClauseCode == "2.2.1");
            Assert.Contains(details.Clauses, c => c.ClauseCode == "2.3.16");
            Assert.Contains(details.Parameters, p => p.ParameterKey == "openAreaRate" && p.ValueText == "5.04" && p.Unit == "%");
            Assert.Contains(details.LookupTables, t => t.TableKey == "roadContributionTable");
            Assert.Contains(details.LookupTables, t => t.TableKey == "cornerTypeDefinitions" && t.PolicyClause?.ClauseCode == "2.2.7");
            PolicyLookupTable cornerTypes = details.LookupTables.Single(t => t.TableKey == "cornerTypeDefinitions");
            Assert.Contains(cornerTypes.Columns, c => c.ColumnKey == "primaryFrontageRoad" && c.ValueType == "RoadReference");
            Assert.Contains(cornerTypes.Columns, c => c.ColumnKey == "secondaryFrontageRoad" && c.ValueType == "RoadReference");
            Assert.Equal(15, cornerTypes.Rows.Count);
            Assert.Contains(cornerTypes.Rows.SelectMany(r => r.Cells), c => c.ValueText == "C-P9-S9");
            Assert.Contains(cornerTypes.Rows.SelectMany(r => r.Cells), c => c.ValueText == "C-P4-S4");
            Assert.Contains(details.LookupTables, t => t.TableKey == "cornerContributionTable");
        }

        [Fact]
        public async Task ApprovalLocksPolicyAndBlocksEdits()
        {
            await using TestProject project = await TestProject.CreateAsync();
            PolicyManagerService service = project.CreateService();
            await service.EnsureSeedPolicyAsync();
            PolicySet policy = (await service.GetPolicySummariesAsync()).Single();

            await service.ApprovePolicyAsync(policy.Id);
            PolicySet approved = await service.GetPolicyAsync(policy.Id) ?? throw new InvalidOperationException();
            Assert.Equal(PolicyStatuses.Approved, approved.Status);
            Assert.True(approved.IsLocked);

            PolicyParameter parameter = approved.Parameters.First();
            parameter.ValueText = "0.10";
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveParameterAsync(parameter));
        }

        [Fact]
        public async Task ApprovedPolicyCanCreateEditableDraftVersion()
        {
            await using TestProject project = await TestProject.CreateAsync();
            PolicyManagerService service = project.CreateService();
            await service.EnsureSeedPolicyAsync();
            PolicySet policy = (await service.GetPolicySummariesAsync()).Single();
            await service.ApprovePolicyAsync(policy.Id);

            PolicySet draft = await service.CreateDraftFromApprovedAsync(policy.Id);
            PolicySet details = await service.GetPolicyAsync(draft.Id) ?? throw new InvalidOperationException();

            Assert.Equal(PolicyStatuses.Draft, details.Status);
            Assert.False(details.IsLocked);
            Assert.Equal(2, details.VersionNo);
            Assert.Equal(policy.PolicyGroupKey, details.PolicyGroupKey);
        }

        [Fact]
        public async Task ClauseNumberingIsAutomaticForAddSubClauseAndDelete()
        {
            await using TestProject project = await TestProject.CreateAsync();
            PolicyManagerService service = project.CreateService();
            await service.EnsureSeedPolicyAsync();
            PolicySet summary = (await service.GetPolicySummariesAsync()).Single();

            PolicyClause root = await service.SaveClauseAsync(new PolicyClause
            {
                PolicySetId = summary.Id,
                PolicySection = "Contribution Policy",
                Heading = "Test automatic root",
                Description = ""
            });

            PolicySet details = await service.GetPolicyAsync(summary.Id) ?? throw new InvalidOperationException();
            root = details.Clauses.Single(c => c.Heading == "Test automatic root");
            Assert.Equal("2.2.12", root.ClauseCode);

            await service.SaveClauseAsync(new PolicyClause
            {
                PolicySetId = summary.Id,
                ParentClauseId = root.Id,
                PolicySection = root.PolicySection,
                Heading = "Test automatic child one",
                Description = ""
            });
            await service.SaveClauseAsync(new PolicyClause
            {
                PolicySetId = summary.Id,
                ParentClauseId = root.Id,
                PolicySection = root.PolicySection,
                Heading = "Test automatic child two",
                Description = ""
            });

            details = await service.GetPolicyAsync(summary.Id) ?? throw new InvalidOperationException();
            Assert.Contains(details.Clauses, c => c.Heading == "Test automatic child one" && c.ClauseCode == "2.2.12.1");
            Assert.Contains(details.Clauses, c => c.Heading == "Test automatic child two" && c.ClauseCode == "2.2.12.2");

            PolicyClause oldSecond = details.Clauses.Single(c => c.ClauseCode == "2.2.2");
            await service.DeleteClauseAsync(oldSecond.Id);

            details = await service.GetPolicyAsync(summary.Id) ?? throw new InvalidOperationException();
            Assert.Contains(details.Clauses, c => c.Heading == "Survey map controls road-width differences" && c.ClauseCode == "2.2.2");
            Assert.Contains(details.Clauses, c => c.Heading == "Test automatic root" && c.ClauseCode == "2.2.11");
            Assert.Contains(details.Clauses, c => c.Heading == "Test automatic child one" && c.ClauseCode == "2.2.11.1");
        }

        [Fact]
        public async Task ClauseNumberingSupportsAnyNestedLevel()
        {
            await using TestProject project = await TestProject.CreateAsync();
            PolicyManagerService service = project.CreateService();
            await service.EnsureSeedPolicyAsync();
            PolicySet summary = (await service.GetPolicySummariesAsync()).Single();
            PolicySet details = await service.GetPolicyAsync(summary.Id) ?? throw new InvalidOperationException();

            PolicyClause clause221 = details.Clauses.Single(c => c.ClauseCode == "2.2.1");
            await service.SaveClauseAsync(new PolicyClause
            {
                PolicySetId = summary.Id,
                ParentClauseId = clause221.Id,
                PolicySection = clause221.PolicySection,
                Heading = "Nested level one",
                Description = ""
            });

            details = await service.GetPolicyAsync(summary.Id) ?? throw new InvalidOperationException();
            PolicyClause levelOne = details.Clauses.Single(c => c.Heading == "Nested level one");
            Assert.Equal("2.2.1.1", levelOne.ClauseCode);

            await service.SaveClauseAsync(new PolicyClause
            {
                PolicySetId = summary.Id,
                ParentClauseId = levelOne.Id,
                PolicySection = levelOne.PolicySection,
                Heading = "Nested level two",
                Description = ""
            });

            details = await service.GetPolicyAsync(summary.Id) ?? throw new InvalidOperationException();
            PolicyClause levelTwo = details.Clauses.Single(c => c.Heading == "Nested level two");
            Assert.Equal("2.2.1.1.1", levelTwo.ClauseCode);

            await service.SaveClauseAsync(new PolicyClause
            {
                PolicySetId = summary.Id,
                ParentClauseId = levelTwo.Id,
                PolicySection = levelTwo.PolicySection,
                Heading = "Nested level three",
                Description = ""
            });
            await service.SaveClauseAsync(new PolicyClause
            {
                PolicySetId = summary.Id,
                ParentClauseId = levelTwo.Id,
                PolicySection = levelTwo.PolicySection,
                Heading = "Nested level three sibling",
                Description = ""
            });

            details = await service.GetPolicyAsync(summary.Id) ?? throw new InvalidOperationException();
            Assert.Contains(details.Clauses, c => c.Heading == "Nested level three" && c.ClauseCode == "2.2.1.1.1.1");
            Assert.Contains(details.Clauses, c => c.Heading == "Nested level three sibling" && c.ClauseCode == "2.2.1.1.1.2");
        }

        [Fact]
        public async Task PolicyPackageRoundTripImportsDraftCopy()
        {
            await using TestProject project = await TestProject.CreateAsync();
            PolicyManagerService service = project.CreateService();
            await service.EnsureSeedPolicyAsync();
            PolicySet policy = (await service.GetPolicySummariesAsync()).Single();
            string packagePath = Path.Combine(project.DirectoryPath, "baireni.rpolicy");

            await service.ExportAsync(policy.Id, packagePath);
            PolicySet imported = await service.ImportAsync(packagePath);
            PolicySet details = await service.GetPolicyAsync(imported.Id) ?? throw new InvalidOperationException();

            Assert.Equal(PolicyStatuses.Draft, details.Status);
            Assert.Contains(details.Clauses, c => c.ClauseCode == "2.2.1");
            Assert.Contains(details.Parameters, p => p.ParameterKey == "minPlotAreaSqM");
            Assert.Contains(details.LookupTables, t => t.TableKey == "specialFacilityRates");
        }

        [Fact]
        public async Task CompatibilityRepairCreatesPolicyTables()
        {
            string directory = Directory.CreateTempSubdirectory("replot-policy-compat-").FullName;
            string dbPath = Path.Combine(directory, "compat.lpp");
            try
            {
                await using AppDbContext context = new(dbPath);
                await context.Database.ExecuteSqlRawAsync(
                    "CREATE TABLE tblProjectInfo (Id INTEGER NOT NULL PRIMARY KEY, ProjectName TEXT NULL);");

                await ProjectDatabaseCompatibility.EnsureAsync(context);

                int tableCount = await context.Database.SqlQueryRaw<int>(
                        "SELECT COUNT(*) AS Value FROM sqlite_master WHERE type='table' AND name='tblPolicySets'")
                    .SingleAsync();
                Assert.Equal(1, tableCount);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
                DeleteDirectoryWithRetry(directory);
            }
        }

        private sealed class TestProject : IAsyncDisposable
        {
            private readonly ProjectSession _session;

            private TestProject(string directoryPath, string dbPath, AppDbContext context)
            {
                DirectoryPath = directoryPath;
                DbPath = dbPath;
                _session = new ProjectSession(dbPath, context, new DebugLogger());
            }

            public string DirectoryPath { get; }
            public string DbPath { get; }

            public static async Task<TestProject> CreateAsync()
            {
                string directory = Directory.CreateTempSubdirectory("replot-policy-").FullName;
                string dbPath = Path.Combine(directory, "test.lpp");
                AppDbContext context = new(dbPath);
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
                return new TestProject(directory, dbPath, context);
            }

            public PolicyManagerService CreateService()
            {
                PolicyValidationService validationService = new();
                PolicyPackageService packageService = new();
                PolicyTemplateSeeder seeder = new(_session);
                return new PolicyManagerService(_session, validationService, packageService, seeder);
            }

            public ValueTask DisposeAsync()
            {
                _session.Dispose();
                SqliteConnection.ClearAllPools();
                if (Directory.Exists(DirectoryPath))
                    DeleteDirectoryWithRetry(DirectoryPath);

                return ValueTask.CompletedTask;
            }
        }

        private static void DeleteDirectoryWithRetry(string directoryPath)
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    if (Directory.Exists(directoryPath))
                        Directory.Delete(directoryPath, recursive: true);
                    return;
                }
                catch (IOException) when (attempt < 4)
                {
                    Thread.Sleep(150);
                }
                catch (UnauthorizedAccessException) when (attempt < 4)
                {
                    Thread.Sleep(150);
                }
            }
        }
    }
}

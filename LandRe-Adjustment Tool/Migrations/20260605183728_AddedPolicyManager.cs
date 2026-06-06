using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddedPolicyManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblPolicySets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicyGroupKey = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    PolicyCode = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    PolicyName = table.Column<string>(type: "TEXT", maxLength: 240, nullable: false),
                    PolicyType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    VersionNo = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SourceTitle = table.Column<string>(type: "TEXT", maxLength: 240, nullable: true),
                    SourceReference = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPolicySets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblPolicyAuditEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicySetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Actor = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPolicyAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblPolicyAuditEntries_tblPolicySets_PolicySetId",
                        column: x => x.PolicySetId,
                        principalTable: "tblPolicySets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblPolicyClauses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicySetId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentClauseId = table.Column<int>(type: "INTEGER", nullable: true),
                    ClauseCode = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    Heading = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    PolicySection = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPolicyClauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblPolicyClauses_tblPolicyClauses_ParentClauseId",
                        column: x => x.ParentClauseId,
                        principalTable: "tblPolicyClauses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblPolicyClauses_tblPolicySets_PolicySetId",
                        column: x => x.PolicySetId,
                        principalTable: "tblPolicySets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblPolicyLookupTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicySetId = table.Column<int>(type: "INTEGER", nullable: false),
                    PolicyClauseId = table.Column<int>(type: "INTEGER", nullable: true),
                    TableKey = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 220, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPolicyLookupTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblPolicyLookupTables_tblPolicyClauses_PolicyClauseId",
                        column: x => x.PolicyClauseId,
                        principalTable: "tblPolicyClauses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tblPolicyLookupTables_tblPolicySets_PolicySetId",
                        column: x => x.PolicySetId,
                        principalTable: "tblPolicySets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblPolicyAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicySetId = table.Column<int>(type: "INTEGER", nullable: false),
                    PolicyClauseId = table.Column<int>(type: "INTEGER", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    ImageData = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Caption = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPolicyAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblPolicyAttachments_tblPolicyClauses_PolicyClauseId",
                        column: x => x.PolicyClauseId,
                        principalTable: "tblPolicyClauses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tblPolicyAttachments_tblPolicySets_PolicySetId",
                        column: x => x.PolicySetId,
                        principalTable: "tblPolicySets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblPolicyParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicySetId = table.Column<int>(type: "INTEGER", nullable: false),
                    PolicyClauseId = table.Column<int>(type: "INTEGER", nullable: true),
                    ParameterKey = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Label = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    ValueType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    ValueText = table.Column<string>(type: "TEXT", nullable: true),
                    DefaultValueText = table.Column<string>(type: "TEXT", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    MinValueText = table.Column<string>(type: "TEXT", nullable: true),
                    MaxValueText = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPolicyParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblPolicyParameters_tblPolicyClauses_PolicyClauseId",
                        column: x => x.PolicyClauseId,
                        principalTable: "tblPolicyClauses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tblPolicyParameters_tblPolicySets_PolicySetId",
                        column: x => x.PolicySetId,
                        principalTable: "tblPolicySets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblPolicyLookupColumns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicyLookupTableId = table.Column<int>(type: "INTEGER", nullable: false),
                    ColumnKey = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    HeaderText = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    ValueType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPolicyLookupColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblPolicyLookupColumns_tblPolicyLookupTables_PolicyLookupTableId",
                        column: x => x.PolicyLookupTableId,
                        principalTable: "tblPolicyLookupTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblPolicyLookupRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicyLookupTableId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    RowLabel = table.Column<string>(type: "TEXT", maxLength: 240, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPolicyLookupRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblPolicyLookupRows_tblPolicyLookupTables_PolicyLookupTableId",
                        column: x => x.PolicyLookupTableId,
                        principalTable: "tblPolicyLookupTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblPolicyLookupCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicyLookupRowId = table.Column<int>(type: "INTEGER", nullable: false),
                    PolicyLookupColumnId = table.Column<int>(type: "INTEGER", nullable: false),
                    ValueText = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPolicyLookupCells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblPolicyLookupCells_tblPolicyLookupColumns_PolicyLookupColumnId",
                        column: x => x.PolicyLookupColumnId,
                        principalTable: "tblPolicyLookupColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblPolicyLookupCells_tblPolicyLookupRows_PolicyLookupRowId",
                        column: x => x.PolicyLookupRowId,
                        principalTable: "tblPolicyLookupRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyAttachments_PolicyClauseId",
                table: "tblPolicyAttachments",
                column: "PolicyClauseId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyAttachments_PolicySetId",
                table: "tblPolicyAttachments",
                column: "PolicySetId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyAuditEntries_PolicySetId",
                table: "tblPolicyAuditEntries",
                column: "PolicySetId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyClauses_ParentClauseId",
                table: "tblPolicyClauses",
                column: "ParentClauseId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyClauses_PolicySetId_ClauseCode",
                table: "tblPolicyClauses",
                columns: new[] { "PolicySetId", "ClauseCode" });

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyLookupCells_PolicyLookupColumnId",
                table: "tblPolicyLookupCells",
                column: "PolicyLookupColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyLookupCells_PolicyLookupRowId_PolicyLookupColumnId",
                table: "tblPolicyLookupCells",
                columns: new[] { "PolicyLookupRowId", "PolicyLookupColumnId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyLookupColumns_PolicyLookupTableId",
                table: "tblPolicyLookupColumns",
                column: "PolicyLookupTableId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyLookupRows_PolicyLookupTableId",
                table: "tblPolicyLookupRows",
                column: "PolicyLookupTableId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyLookupTables_PolicyClauseId",
                table: "tblPolicyLookupTables",
                column: "PolicyClauseId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyLookupTables_PolicySetId_TableKey",
                table: "tblPolicyLookupTables",
                columns: new[] { "PolicySetId", "TableKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyParameters_PolicyClauseId",
                table: "tblPolicyParameters",
                column: "PolicyClauseId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicyParameters_PolicySetId_ParameterKey",
                table: "tblPolicyParameters",
                columns: new[] { "PolicySetId", "ParameterKey" });

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicySets_PolicyCode",
                table: "tblPolicySets",
                column: "PolicyCode");

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicySets_PolicyGroupKey_VersionNo",
                table: "tblPolicySets",
                columns: new[] { "PolicyGroupKey", "VersionNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblPolicyAttachments");

            migrationBuilder.DropTable(
                name: "tblPolicyAuditEntries");

            migrationBuilder.DropTable(
                name: "tblPolicyLookupCells");

            migrationBuilder.DropTable(
                name: "tblPolicyParameters");

            migrationBuilder.DropTable(
                name: "tblPolicyLookupColumns");

            migrationBuilder.DropTable(
                name: "tblPolicyLookupRows");

            migrationBuilder.DropTable(
                name: "tblPolicyClauses");

            migrationBuilder.DropTable(
                name: "tblPolicyLookupTables");

            migrationBuilder.DropTable(
                name: "tblPolicySets");
        }
    }
}

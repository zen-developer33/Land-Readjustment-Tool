using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class addmigrationaddedPolicySection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblPolicySectionDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicySetId = table.Column<int>(type: "INTEGER", nullable: false),
                    SectionCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Heading = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPolicySectionDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblPolicySectionDefinitions_tblPolicySets_PolicySetId",
                        column: x => x.PolicySetId,
                        principalTable: "tblPolicySets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblPolicySectionDefinitions_PolicySetId_SectionCode",
                table: "tblPolicySectionDefinitions",
                columns: new[] { "PolicySetId", "SectionCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblPolicySectionDefinitions");
        }
    }
}

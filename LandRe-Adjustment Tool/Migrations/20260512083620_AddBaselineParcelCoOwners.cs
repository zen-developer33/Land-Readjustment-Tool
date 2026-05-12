using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddBaselineParcelCoOwners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblBaselineParcelCoOwners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaselineParcelId = table.Column<int>(type: "INTEGER", nullable: false),
                    LandOwnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnershipSharePercent = table.Column<double>(type: "REAL", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBaselineParcelCoOwners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblBaselineParcelCoOwners_tblBaselineParcels_BaselineParcelId",
                        column: x => x.BaselineParcelId,
                        principalTable: "tblBaselineParcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblBaselineParcelCoOwners_tblLandOwners_LandOwnerId",
                        column: x => x.LandOwnerId,
                        principalTable: "tblLandOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblBaselineParcelCoOwners_BaselineParcelId_LandOwnerId",
                table: "tblBaselineParcelCoOwners",
                columns: new[] { "BaselineParcelId", "LandOwnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblBaselineParcelCoOwners_LandOwnerId",
                table: "tblBaselineParcelCoOwners",
                column: "LandOwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblBaselineParcelCoOwners");
        }
    }
}

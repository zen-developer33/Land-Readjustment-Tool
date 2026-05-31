using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class added_entity_to_database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblBuildingInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CanvasObjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    BuildingCode = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    BuildingName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    OwnerName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    BuildingUse = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    ConstructionType = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    StoreyCount = table.Column<int>(type: "INTEGER", nullable: true),
                    PlinthAreaSqm = table.Column<double>(type: "REAL", nullable: true),
                    BuildingCondition = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    SurveyDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBuildingInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblBuildingInventories_tblCanvasObjects_CanvasObjectId",
                        column: x => x.CanvasObjectId,
                        principalTable: "tblCanvasObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tblBuildingOpenings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuildingInventoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Side = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    OpeningType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    OffsetFromLeftM = table.Column<double>(type: "REAL", nullable: true),
                    SillHeightM = table.Column<double>(type: "REAL", nullable: true),
                    WidthM = table.Column<double>(type: "REAL", nullable: true),
                    HeightM = table.Column<double>(type: "REAL", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBuildingOpenings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblBuildingOpenings_tblBuildingInventories_BuildingInventoryId",
                        column: x => x.BuildingInventoryId,
                        principalTable: "tblBuildingInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblBuildingPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuildingInventoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Direction = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: true),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    ImageData = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CapturedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBuildingPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblBuildingPhotos_tblBuildingInventories_BuildingInventoryId",
                        column: x => x.BuildingInventoryId,
                        principalTable: "tblBuildingInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblBuildingInventories_BuildingCode",
                table: "tblBuildingInventories",
                column: "BuildingCode",
                unique: true,
                filter: "BuildingCode IS NOT NULL AND BuildingCode <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_tblBuildingInventories_CanvasObjectId",
                table: "tblBuildingInventories",
                column: "CanvasObjectId",
                unique: true,
                filter: "CanvasObjectId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tblBuildingOpenings_BuildingInventoryId_Side_OpeningType",
                table: "tblBuildingOpenings",
                columns: new[] { "BuildingInventoryId", "Side", "OpeningType" });

            migrationBuilder.CreateIndex(
                name: "IX_tblBuildingPhotos_BuildingInventoryId_Direction",
                table: "tblBuildingPhotos",
                columns: new[] { "BuildingInventoryId", "Direction" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblBuildingOpenings");

            migrationBuilder.DropTable(
                name: "tblBuildingPhotos");

            migrationBuilder.DropTable(
                name: "tblBuildingInventories");
        }
    }
}

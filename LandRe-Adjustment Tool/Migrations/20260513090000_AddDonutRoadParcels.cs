using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    public partial class AddDonutRoadParcels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblParcels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParcelNumber = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    ParcelType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Shape = table.Column<Geometry>(type: "GEOMETRY", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblParcels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblRoadParcels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoadParcelNumber = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    RoadName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    RoadType = table.Column<int>(type: "INTEGER", nullable: false),
                    Shape = table.Column<Polygon>(type: "GEOMETRY", nullable: false),
                    ImportedFrom = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ValidationStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ValidationMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblRoadParcels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblRoadIslands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoadParcelId = table.Column<int>(type: "INTEGER", nullable: false),
                    HoleIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    LinkedParcelNumber = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    IslandShape = table.Column<Polygon>(type: "GEOMETRY", nullable: false),
                    IslandDescription = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblRoadIslands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblRoadIslands_tblRoadParcels_RoadParcelId",
                        column: x => x.RoadParcelId,
                        principalTable: "tblRoadParcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblParcels_ParcelNumber",
                table: "tblParcels",
                column: "ParcelNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblRoadIslands_RoadParcelId_HoleIndex",
                table: "tblRoadIslands",
                columns: new[] { "RoadParcelId", "HoleIndex" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tblParcels");
            migrationBuilder.DropTable(name: "tblRoadIslands");
            migrationBuilder.DropTable(name: "tblRoadParcels");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinateSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoordinateSystem",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "MapUnit",
                table: "tblProjectSettings");

            migrationBuilder.RenameColumn(
                name: "EpsgCode",
                table: "tblProjectSettings",
                newName: "CoordinateSystemId");

            migrationBuilder.CreateTable(
                name: "tblCoordinateSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    EpsgCode = table.Column<int>(type: "INTEGER", nullable: true),
                    ProjectionType = table.Column<string>(type: "TEXT", nullable: true),
                    CentralMeridian = table.Column<double>(type: "REAL", nullable: true),
                    LatitudeOfOrigin = table.Column<double>(type: "REAL", nullable: true),
                    ScaleFactor = table.Column<double>(type: "REAL", nullable: true),
                    FalseEasting = table.Column<double>(type: "REAL", nullable: true),
                    FalseNorthing = table.Column<double>(type: "REAL", nullable: true),
                    Ellipsoid = table.Column<string>(type: "TEXT", nullable: true),
                    SemiMajorAxis = table.Column<double>(type: "REAL", nullable: true),
                    InverseFlattening = table.Column<double>(type: "REAL", nullable: true),
                    DatumShiftParametersJson = table.Column<string>(type: "TEXT", nullable: true),
                    WktDefinition = table.Column<string>(type: "TEXT", nullable: true),
                    MapUnit = table.Column<string>(type: "TEXT", nullable: false),
                    Region = table.Column<string>(type: "TEXT", nullable: true),
                    IsSystemDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCoordinateSystems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblProjectSettings_CoordinateSystemId",
                table: "tblProjectSettings",
                column: "CoordinateSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_tblCoordinateSystems_Code",
                table: "tblCoordinateSystems",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tblProjectSettings_tblCoordinateSystems_CoordinateSystemId",
                table: "tblProjectSettings",
                column: "CoordinateSystemId",
                principalTable: "tblCoordinateSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblProjectSettings_tblCoordinateSystems_CoordinateSystemId",
                table: "tblProjectSettings");

            migrationBuilder.DropTable(
                name: "tblCoordinateSystems");

            migrationBuilder.DropIndex(
                name: "IX_tblProjectSettings_CoordinateSystemId",
                table: "tblProjectSettings");

            migrationBuilder.RenameColumn(
                name: "CoordinateSystemId",
                table: "tblProjectSettings",
                newName: "EpsgCode");

            migrationBuilder.AddColumn<string>(
                name: "CoordinateSystem",
                table: "tblProjectSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MapUnit",
                table: "tblProjectSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}

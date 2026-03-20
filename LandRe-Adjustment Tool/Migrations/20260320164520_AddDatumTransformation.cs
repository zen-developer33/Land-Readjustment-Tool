using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddDatumTransformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DatumTransformationId",
                table: "tblProjectSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tblDatumTransformations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    SourceDatum = table.Column<string>(type: "TEXT", nullable: false),
                    TargetDatum = table.Column<string>(type: "TEXT", nullable: false),
                    DeltaX = table.Column<double>(type: "REAL", nullable: false),
                    DeltaY = table.Column<double>(type: "REAL", nullable: false),
                    DeltaZ = table.Column<double>(type: "REAL", nullable: false),
                    RotationX = table.Column<double>(type: "REAL", nullable: false),
                    RotationY = table.Column<double>(type: "REAL", nullable: false),
                    RotationZ = table.Column<double>(type: "REAL", nullable: false),
                    ScalePpm = table.Column<double>(type: "REAL", nullable: false),
                    ApplicableCrsCodes = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    Region = table.Column<string>(type: "TEXT", nullable: true),
                    AccuracyMeters = table.Column<double>(type: "REAL", nullable: true),
                    IsSystemDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblDatumTransformations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "tblCoordinateSystems",
                columns: new[] { "Id", "CentralMeridian", "Code", "CreatedDate", "DatumShiftParametersJson", "Description", "DisplayOrder", "Ellipsoid", "EpsgCode", "FalseEasting", "FalseNorthing", "InverseFlattening", "IsActive", "IsSystemDefault", "LatitudeOfOrigin", "MapUnit", "Name", "ProjectionType", "Region", "ScaleFactor", "SemiMajorAxis", "WktDefinition" },
                values: new object[,]
                {
                    { 1, null, "UTM44N", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "West Nepal. Longitude 78°E to 84°E.", 1, "WGS84", 32644, null, null, null, true, true, null, "Meters", "UTM Zone 44N — West Nepal", "TransverseMercator", "Nepal", null, null, null },
                    { 2, null, "UTM45N", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "East Nepal. Longitude 84°E to 90°E.", 2, "WGS84", 32645, null, null, null, true, true, null, "Meters", "UTM Zone 45N — East Nepal", "TransverseMercator", "Nepal", null, null, null },
                    { 3, 81.0, "MUTM81", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Nepal Survey Dept. Central meridian 81°E.", 3, "Everest1830", null, 500000.0, 0.0, 300.80169999999998, true, true, 0.0, "Meters", "Modified UTM Zone 81 — Nepal", "TransverseMercator", "Nepal", 0.99990000000000001, 6377276.3449999997, null },
                    { 4, 84.0, "MUTM82", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Nepal Survey Dept. Central meridian 84°E.", 4, "Everest1830", null, 500000.0, 0.0, 300.80169999999998, true, true, 0.0, "Meters", "Modified UTM Zone 82 — Nepal", "TransverseMercator", "Nepal", 0.99990000000000001, 6377276.3449999997, null },
                    { 5, 87.0, "MUTM83", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Nepal Survey Dept. Central meridian 87°E.", 5, "Everest1830", null, 500000.0, 0.0, 300.80169999999998, true, true, 0.0, "Meters", "Modified UTM Zone 83 — Nepal", "TransverseMercator", "Nepal", 0.99990000000000001, 6377276.3449999997, null },
                    { 6, null, "WGS84", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "GPS coordinates in decimal degrees.", 6, "WGS84", 4326, null, null, null, true, true, null, "Degrees", "WGS84 — Geographic Lat/Long", "Geographic", "Global", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "tblDatumTransformations",
                columns: new[] { "Id", "AccuracyMeters", "ApplicableCrsCodes", "Code", "CreatedDate", "DeltaX", "DeltaY", "DeltaZ", "Description", "DisplayOrder", "IsActive", "IsSystemDefault", "Name", "Region", "RotationX", "RotationY", "RotationZ", "ScalePpm", "Source", "SourceDatum", "TargetDatum" },
                values: new object[,]
                {
                    { 1, 1.0, "MUTM81,MUTM82,MUTM83", "NEPAL_SURV_DEPT", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 293.17000000000002, 726.17999999999995, 245.36000000000001, "Official transformation from Survey Department Nepal. Recommended for all MUTM zones.", 1, true, true, "Nepal Survey Department (Official)", "Nepal", 0.0, 0.0, 0.0, 0.0, "Survey Department Nepal", "Everest1830", "WGS84" },
                    { 2, 3.0, "MUTM81,MUTM82,MUTM83", "NEPAL_NAGARKOT", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 295.0, 740.0, 460.0, "Based on Nagarkot GPS control points. Commonly used in older datasets.", 2, true, true, "Nagarkot GPS Campaign 1994", "Nepal", 0.0, 0.0, 0.0, 0.0, "Nagarkot GPS Campaign 1994", "Everest1830", "WGS84" },
                    { 3, 5.0, "MUTM81,MUTM82,MUTM83", "NEPAL_KALIANPUR", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 283.0, 682.0, 231.0, "Traditional Kalianpur based parameters. Used in older survey records.", 3, true, true, "Kalianpur Datum Parameters", "Nepal", 0.0, 0.0, 0.0, 0.0, "Kalianpur datum parameters", "Everest1830", "WGS84" },
                    { 4, 0.0, "UTM44N,UTM45N,WGS84", "WGS84_IDENTITY", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0.0, 0.0, "Used when source and target are both WGS84. No shift applied.", 4, true, true, "WGS84 — No Transformation Needed", "Global", 0.0, 0.0, 0.0, 0.0, "Identity transform", "WGS84", "WGS84" }
                });

            migrationBuilder.InsertData(
                table: "tblPlotTypes",
                columns: new[] { "Id", "CreatedDate", "Description", "DisplayOrder", "IsActive", "IsSystemDefault", "TypeCode", "TypeName" },
                values: new object[,]
                {
                    { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Private ownership plot", 1, true, true, "PRV", "Private" },
                    { 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plot for sale to recover project costs", 2, true, true, "SAL", "Sales Plot" },
                    { 3, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Government use plot", 3, true, true, "GOV", "Government" },
                    { 4, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Parks and public open spaces", 4, true, true, "OPS", "Open Space" },
                    { 5, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Community use plot", 5, true, true, "COM", "Community" },
                    { 6, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Road right of way", 6, true, true, "ROD", "Road" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblProjectSettings_DatumTransformationId",
                table: "tblProjectSettings",
                column: "DatumTransformationId");

            migrationBuilder.CreateIndex(
                name: "IX_tblDatumTransformations_Code",
                table: "tblDatumTransformations",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tblProjectSettings_tblDatumTransformations_DatumTransformationId",
                table: "tblProjectSettings",
                column: "DatumTransformationId",
                principalTable: "tblDatumTransformations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblProjectSettings_tblDatumTransformations_DatumTransformationId",
                table: "tblProjectSettings");

            migrationBuilder.DropTable(
                name: "tblDatumTransformations");

            migrationBuilder.DropIndex(
                name: "IX_tblProjectSettings_DatumTransformationId",
                table: "tblProjectSettings");

            migrationBuilder.DeleteData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "tblPlotTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "tblPlotTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "tblPlotTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "tblPlotTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "tblPlotTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "tblPlotTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "DatumTransformationId",
                table: "tblProjectSettings");
        }
    }
}

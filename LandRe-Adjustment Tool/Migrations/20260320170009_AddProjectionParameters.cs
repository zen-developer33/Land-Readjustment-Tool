using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectionParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccuracyMeters",
                table: "tblDatumTransformations");

            migrationBuilder.DropColumn(
                name: "CentralMeridian",
                table: "tblCoordinateSystems");

            migrationBuilder.DropColumn(
                name: "DatumShiftParametersJson",
                table: "tblCoordinateSystems");

            migrationBuilder.DropColumn(
                name: "Ellipsoid",
                table: "tblCoordinateSystems");

            migrationBuilder.DropColumn(
                name: "FalseEasting",
                table: "tblCoordinateSystems");

            migrationBuilder.DropColumn(
                name: "FalseNorthing",
                table: "tblCoordinateSystems");

            migrationBuilder.DropColumn(
                name: "InverseFlattening",
                table: "tblCoordinateSystems");

            migrationBuilder.DropColumn(
                name: "LatitudeOfOrigin",
                table: "tblCoordinateSystems");

            migrationBuilder.DropColumn(
                name: "MapUnit",
                table: "tblCoordinateSystems");

            migrationBuilder.DropColumn(
                name: "ScaleFactor",
                table: "tblCoordinateSystems");

            migrationBuilder.DropColumn(
                name: "SemiMajorAxis",
                table: "tblCoordinateSystems");

            migrationBuilder.DropColumn(
                name: "WktDefinition",
                table: "tblCoordinateSystems");

            migrationBuilder.CreateTable(
                name: "tblProjectionParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CoordinateSystemId = table.Column<int>(type: "INTEGER", nullable: false),
                    CentralMeridian = table.Column<double>(type: "REAL", nullable: true),
                    LatitudeOfOrigin = table.Column<double>(type: "REAL", nullable: true),
                    ScaleFactor = table.Column<double>(type: "REAL", nullable: true),
                    FalseEasting = table.Column<double>(type: "REAL", nullable: true),
                    FalseNorthing = table.Column<double>(type: "REAL", nullable: true),
                    Ellipsoid = table.Column<string>(type: "TEXT", nullable: true),
                    SemiMajorAxis = table.Column<double>(type: "REAL", nullable: true),
                    InverseFlattening = table.Column<double>(type: "REAL", nullable: true),
                    WktDefinition = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblProjectionParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblProjectionParameters_tblCoordinateSystems_CoordinateSystemId",
                        column: x => x.CoordinateSystemId,
                        principalTable: "tblCoordinateSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "West Nepal. 78°E to 84°E. WGS84 datum.");

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "East Nepal. 84°E to 90°E. WGS84 datum.");

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Nepal Survey Dept. Central meridian 81°E. Everest 1830.");

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Nepal Survey Dept. Central meridian 84°E. Everest 1830.");

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 5,
                column: "Description",
                value: "Nepal Survey Dept. Central meridian 87°E. Everest 1830.");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Official transformation. Recommended for all MUTM zones.");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Based on Nagarkot GPS control points.");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Traditional Kalianpur parameters. Used in older records.");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "No shift needed. Source and target are both WGS84.");

            migrationBuilder.InsertData(
                table: "tblProjectionParameters",
                columns: new[] { "Id", "CentralMeridian", "CoordinateSystemId", "Ellipsoid", "FalseEasting", "FalseNorthing", "InverseFlattening", "LatitudeOfOrigin", "ScaleFactor", "SemiMajorAxis", "WktDefinition" },
                values: new object[,]
                {
                    { 1, 81.0, 3, "Everest1830", 500000.0, 0.0, 300.80169999999998, 0.0, 0.99990000000000001, 6377276.3449999997, null },
                    { 2, 84.0, 4, "Everest1830", 500000.0, 0.0, 300.80169999999998, 0.0, 0.99990000000000001, 6377276.3449999997, null },
                    { 3, 87.0, 5, "Everest1830", 500000.0, 0.0, 300.80169999999998, 0.0, 0.99990000000000001, 6377276.3449999997, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblProjectionParameters_CoordinateSystemId",
                table: "tblProjectionParameters",
                column: "CoordinateSystemId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblProjectionParameters");

            migrationBuilder.AddColumn<double>(
                name: "AccuracyMeters",
                table: "tblDatumTransformations",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CentralMeridian",
                table: "tblCoordinateSystems",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatumShiftParametersJson",
                table: "tblCoordinateSystems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ellipsoid",
                table: "tblCoordinateSystems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FalseEasting",
                table: "tblCoordinateSystems",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FalseNorthing",
                table: "tblCoordinateSystems",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "InverseFlattening",
                table: "tblCoordinateSystems",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LatitudeOfOrigin",
                table: "tblCoordinateSystems",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MapUnit",
                table: "tblCoordinateSystems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "ScaleFactor",
                table: "tblCoordinateSystems",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SemiMajorAxis",
                table: "tblCoordinateSystems",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WktDefinition",
                table: "tblCoordinateSystems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CentralMeridian", "DatumShiftParametersJson", "Description", "Ellipsoid", "FalseEasting", "FalseNorthing", "InverseFlattening", "LatitudeOfOrigin", "MapUnit", "ScaleFactor", "SemiMajorAxis", "WktDefinition" },
                values: new object[] { null, null, "West Nepal. Longitude 78°E to 84°E.", "WGS84", null, null, null, null, "Meters", null, null, null });

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CentralMeridian", "DatumShiftParametersJson", "Description", "Ellipsoid", "FalseEasting", "FalseNorthing", "InverseFlattening", "LatitudeOfOrigin", "MapUnit", "ScaleFactor", "SemiMajorAxis", "WktDefinition" },
                values: new object[] { null, null, "East Nepal. Longitude 84°E to 90°E.", "WGS84", null, null, null, null, "Meters", null, null, null });

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CentralMeridian", "DatumShiftParametersJson", "Description", "Ellipsoid", "FalseEasting", "FalseNorthing", "InverseFlattening", "LatitudeOfOrigin", "MapUnit", "ScaleFactor", "SemiMajorAxis", "WktDefinition" },
                values: new object[] { 81.0, null, "Nepal Survey Dept. Central meridian 81°E.", "Everest1830", 500000.0, 0.0, 300.80169999999998, 0.0, "Meters", 0.99990000000000001, 6377276.3449999997, null });

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CentralMeridian", "DatumShiftParametersJson", "Description", "Ellipsoid", "FalseEasting", "FalseNorthing", "InverseFlattening", "LatitudeOfOrigin", "MapUnit", "ScaleFactor", "SemiMajorAxis", "WktDefinition" },
                values: new object[] { 84.0, null, "Nepal Survey Dept. Central meridian 84°E.", "Everest1830", 500000.0, 0.0, 300.80169999999998, 0.0, "Meters", 0.99990000000000001, 6377276.3449999997, null });

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CentralMeridian", "DatumShiftParametersJson", "Description", "Ellipsoid", "FalseEasting", "FalseNorthing", "InverseFlattening", "LatitudeOfOrigin", "MapUnit", "ScaleFactor", "SemiMajorAxis", "WktDefinition" },
                values: new object[] { 87.0, null, "Nepal Survey Dept. Central meridian 87°E.", "Everest1830", 500000.0, 0.0, 300.80169999999998, 0.0, "Meters", 0.99990000000000001, 6377276.3449999997, null });

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CentralMeridian", "DatumShiftParametersJson", "Ellipsoid", "FalseEasting", "FalseNorthing", "InverseFlattening", "LatitudeOfOrigin", "MapUnit", "ScaleFactor", "SemiMajorAxis", "WktDefinition" },
                values: new object[] { null, null, "WGS84", null, null, null, null, "Degrees", null, null, null });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccuracyMeters", "Description" },
                values: new object[] { 1.0, "Official transformation from Survey Department Nepal. Recommended for all MUTM zones." });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AccuracyMeters", "Description" },
                values: new object[] { 3.0, "Based on Nagarkot GPS control points. Commonly used in older datasets." });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AccuracyMeters", "Description" },
                values: new object[] { 5.0, "Traditional Kalianpur based parameters. Used in older survey records." });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AccuracyMeters", "Description" },
                values: new object[] { 0.0, "Used when source and target are both WGS84. No shift applied." });
        }
    }
}

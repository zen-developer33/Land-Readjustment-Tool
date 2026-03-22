using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectSettings_IsConfigured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "SURVEY_DEPT");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "NAGARKOT_TM");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                column: "Code",
                value: "KALIANPUR");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "tblCoordinateSystems",
                columns: new[] { "Id", "Code", "CreatedDate", "Description", "DisplayOrder", "EpsgCode", "IsActive", "IsSystemDefault", "Name", "ProjectionType", "Region" },
                values: new object[] { 6, "WGS84", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "GPS coordinates in decimal degrees.", 6, 4326, true, true, "WGS84 — Geographic Lat/Long", "Geographic", "Global" });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "NEPAL_SURV_DEPT");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "NEPAL_NAGARKOT");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                column: "Code",
                value: "NEPAL_KALIANPUR");

            migrationBuilder.InsertData(
                table: "tblDatumTransformations",
                columns: new[] { "Id", "ApplicableCrsCodes", "Code", "CreatedDate", "DeltaX", "DeltaY", "DeltaZ", "Description", "DisplayOrder", "IsActive", "IsSystemDefault", "Name", "Region", "RotationX", "RotationY", "RotationZ", "ScalePpm", "Source", "SourceDatum", "TargetDatum" },
                values: new object[] { 4, "UTM44N,UTM45N,WGS84", "WGS84_IDENTITY", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0.0, 0.0, "No shift needed. Source and target are both WGS84.", 4, true, true, "WGS84 — No Transformation Needed", "Global", 0.0, 0.0, 0.0, 0.0, "Identity transform", "WGS84", "WGS84" });
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class updated_display_order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "__TMP_DT_1__");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "__TMP_DT_2__");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                column: "Code",
                value: "__TMP_DT_3__");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApplicableCrsCodes", "Code", "DeltaX", "DeltaY", "DeltaZ", "Description", "DisplayOrder", "Name", "RotationX", "RotationY", "RotationZ", "ScalePpm", "Source" },
                values: new object[] { "MUTM81,MUTM82,MUTM83", "NAGARKOT_TM", 296.20699999999999, 731.54499999999996, 273.00099999999998, "Based on Nagarkot GPS control points.", 2, "Nagarkot TM", 0.0, 0.0, 0.0, 0.0, "Nagarkot TM" });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApplicableCrsCodes", "Code", "DeltaX", "DeltaY", "DeltaZ", "Description", "DisplayOrder", "Name", "Source" },
                values: new object[] { "MUTM81,MUTM84,MUTM87", "KALIANPUR", 295.0, 736.0, 257.0, "Traditional Kalianpur parameters. Used in older records.", 2, "Kalianpur Datum Parameters", "Kalianpur datum parameters" });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "DeltaX", "DeltaY", "DeltaZ", "Description", "DisplayOrder", "Name", "RotationX", "RotationY", "RotationZ", "ScalePpm", "Source" },
                values: new object[] { "SURVEY_DEPT_7_PARAM", -124.3813, 521.66999999999996, 764.51369999999997, "Official transformation. Recommended for all MUTM zones.", 3, "Nepal Survey Department (Official)", 17.148800000000001, -8.1153600000000008, 11.184200000000001, -2.1105, "Survey Department Nepal" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "__TMP_DT_1__");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "__TMP_DT_2__");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                column: "Code",
                value: "__TMP_DT_3__");

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApplicableCrsCodes", "Code", "DeltaX", "DeltaY", "DeltaZ", "Description", "DisplayOrder", "Name", "RotationX", "RotationY", "RotationZ", "ScalePpm", "Source" },
                values: new object[] { "MUTM81,MUTM84,MUTM87", "SURVEY_DEPT_7_PARAM", -124.3813, 521.66999999999996, 764.51369999999997, "Official transformation. Recommended for all MUTM zones.", 3, "Nepal Survey Department (Official)", 17.148800000000001, -8.1153600000000008, 11.184200000000001, -2.1105, "Survey Department Nepal" });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApplicableCrsCodes", "Code", "DeltaX", "DeltaY", "DeltaZ", "Description", "DisplayOrder", "Name", "Source" },
                values: new object[] { "MUTM81,MUTM82,MUTM83", "NAGARKOT_TM", 296.20699999999999, 731.54499999999996, 273.00099999999998, "Based on Nagarkot GPS control points.", 1, "Nagarkot TM", "Nagarkot TM" });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "DeltaX", "DeltaY", "DeltaZ", "Description", "DisplayOrder", "Name", "RotationX", "RotationY", "RotationZ", "ScalePpm", "Source" },
                values: new object[] { "KALIANPUR", 295.0, 736.0, 257.0, "Traditional Kalianpur parameters. Used in older records.", 2, "Kalianpur Datum Parameters", 0.0, 0.0, 0.0, 0.0, "Kalianpur datum parameters" });
        }
    }
}

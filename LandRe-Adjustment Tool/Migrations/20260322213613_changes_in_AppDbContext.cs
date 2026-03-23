using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class changes_in_AppDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApplicableCrsCodes", "Code" },
                values: new object[] { "MUTM81,MUTM84,MUTM87", "SURVEY_DEPT_7_PARAM" });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                column: "ApplicableCrsCodes",
                value: "MUTM81,MUTM84,MUTM87");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApplicableCrsCodes", "Code" },
                values: new object[] { "MUTM81,MUTM82,MUTM83", "SURVEY_DEPT" });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                column: "ApplicableCrsCodes",
                value: "MUTM81,MUTM82,MUTM83");
        }
    }
}

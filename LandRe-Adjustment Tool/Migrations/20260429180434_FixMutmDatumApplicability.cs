using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class FixMutmDatumApplicability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
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
                column: "ApplicableCrsCodes",
                value: "MUTM81,MUTM82,MUTM83");
        }
    }
}

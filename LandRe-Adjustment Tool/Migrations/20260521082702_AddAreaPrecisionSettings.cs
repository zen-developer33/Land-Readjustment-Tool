using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaPrecisionSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaSqmDecimalPlaces",
                table: "tblProjectSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "TraditionalAreaLowestUnitDecimalPlaces",
                table: "tblProjectSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaSqmDecimalPlaces",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "TraditionalAreaLowestUnitDecimalPlaces",
                table: "tblProjectSettings");
        }
    }
}

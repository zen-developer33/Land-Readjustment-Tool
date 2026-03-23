using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Code", "Name" },
                values: new object[] { "MUTM84", "Modified UTM Zone 84 — Nepal" });

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Code", "Name" },
                values: new object[] { "MUTM87", "Modified UTM Zone 87 — Nepal" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Code", "Name" },
                values: new object[] { "MUTM82", "Modified UTM Zone 82 — Nepal" });

            migrationBuilder.UpdateData(
                table: "tblCoordinateSystems",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Code", "Name" },
                values: new object[] { "MUTM83", "Modified UTM Zone 83 — Nepal" });
        }
    }
}

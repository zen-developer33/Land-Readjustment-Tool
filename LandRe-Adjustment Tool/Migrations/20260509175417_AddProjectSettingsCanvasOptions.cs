using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectSettingsCanvasOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanvasAntiAliasingEnabled",
                table: "tblProjectSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanvasNorthMarkerVisible",
                table: "tblProjectSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanvasAntiAliasingEnabled",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "CanvasNorthMarkerVisible",
                table: "tblProjectSettings");
        }
    }
}

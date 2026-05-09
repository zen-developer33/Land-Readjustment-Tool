using Land_Readjustment_Tool.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260509120000_AddProjectSettingsSnapGlyphSize")]
    public partial class AddProjectSettingsSnapGlyphSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "SnapGlyphSizePx",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: false,
                defaultValue: 14.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SnapGlyphSizePx",
                table: "tblProjectSettings");
        }
    }
}

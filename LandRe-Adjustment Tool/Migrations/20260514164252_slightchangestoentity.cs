using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class slightchangestoentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FillTransparency",
                table: "tblCanvasLayers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 50,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<bool>(
                name: "ShowFillTransparency",
                table: "tblCanvasLayers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowFillTransparency",
                table: "tblCanvasLayers");

            migrationBuilder.AlterColumn<int>(
                name: "FillTransparency",
                table: "tblCanvasLayers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 50);
        }
    }
}

using Land_Readjustment_Tool.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260501170000_PersistViewportAndXyzTileState")]
    public partial class PersistViewportAndXyzTileState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CanvasViewportCenterX",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CanvasViewportCenterY",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CanvasViewportVisibleHeight",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CanvasViewportVisibleWidth",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CanvasViewportZoomScale",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastXyzDownloadMaxLatitude",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastXyzDownloadMaxLongitude",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastXyzDownloadMinLatitude",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastXyzDownloadMinLongitude",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastXyzImageExtension",
                table: "tblProjectSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastXyzLayerName",
                table: "tblProjectSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastXyzMaxLatitude",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastXyzMaxLongitude",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastXyzMinLatitude",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastXyzMinLongitude",
                table: "tblProjectSettings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastXyzTileSourceUrlTemplate",
                table: "tblProjectSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastXyzZoomLevel",
                table: "tblProjectSettings",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanvasViewportCenterX",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "CanvasViewportCenterY",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "CanvasViewportVisibleHeight",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "CanvasViewportVisibleWidth",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "CanvasViewportZoomScale",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzDownloadMaxLatitude",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzDownloadMaxLongitude",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzDownloadMinLatitude",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzDownloadMinLongitude",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzImageExtension",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzLayerName",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzMaxLatitude",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzMaxLongitude",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzMinLatitude",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzMinLongitude",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzTileSourceUrlTemplate",
                table: "tblProjectSettings");

            migrationBuilder.DropColumn(
                name: "LastXyzZoomLevel",
                table: "tblProjectSettings");
        }
    }
}

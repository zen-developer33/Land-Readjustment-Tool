using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblProjectSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TraditionalAreaUnit = table.Column<string>(type: "TEXT", nullable: false),
                    CoordinateSystem = table.Column<string>(type: "TEXT", nullable: true),
                    EpsgCode = table.Column<int>(type: "INTEGER", nullable: true),
                    MapUnit = table.Column<string>(type: "TEXT", nullable: false),
                    CanvasBackgroundColor = table.Column<string>(type: "TEXT", nullable: false),
                    CanvasGridColor = table.Column<string>(type: "TEXT", nullable: false),
                    CanvasGridVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    SnapEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SnapTolerancePx = table.Column<double>(type: "REAL", nullable: false),
                    ParcelNumberFormat = table.Column<string>(type: "TEXT", nullable: false),
                    ParcelNumberPrefix = table.Column<string>(type: "TEXT", nullable: true),
                    ParcelNumberPadding = table.Column<int>(type: "INTEGER", nullable: false),
                    MinPlotAreaSqm = table.Column<double>(type: "REAL", nullable: false),
                    DocumentLanguage = table.Column<string>(type: "TEXT", nullable: false),
                    DateFormat = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultPaperSize = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultPrintScale = table.Column<int>(type: "INTEGER", nullable: false),
                    IsConfigured = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblProjectSettings", x => x.Id);
                    table.CheckConstraint("CK_ProjectSettings_SingleRow", "Id = 1");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblProjectSettings");
        }
    }
}

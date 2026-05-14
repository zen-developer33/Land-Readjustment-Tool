using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class updateddatabaseentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "FieldMeasuredAreaSqm",
                table: "tblImportedRawRecords",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantName",
                table: "tblImportedRawRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FieldMeasuredAreaSqm",
                table: "tblBaselineParcels",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldMeasuredAreaSqm",
                table: "tblImportedRawRecords");

            migrationBuilder.DropColumn(
                name: "TenantName",
                table: "tblImportedRawRecords");

            migrationBuilder.DropColumn(
                name: "FieldMeasuredAreaSqm",
                table: "tblBaselineParcels");
        }
    }
}

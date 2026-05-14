using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    public partial class AddParcelFieldAreaAndTenantName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "FieldMeasuredAreaSqm",
                table: "tblBaselineParcels",
                type: "REAL",
                nullable: true);

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldMeasuredAreaSqm",
                table: "tblBaselineParcels");

            migrationBuilder.DropColumn(
                name: "FieldMeasuredAreaSqm",
                table: "tblImportedRawRecords");

            migrationBuilder.DropColumn(
                name: "TenantName",
                table: "tblImportedRawRecords");
        }
    }
}

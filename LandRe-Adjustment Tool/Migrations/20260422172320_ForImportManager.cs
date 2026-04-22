using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class ForImportManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tblImportedRawRecords_ImportSessionId",
                table: "tblImportedRawRecords");

            migrationBuilder.CreateIndex(
                name: "IX_tblImportedRawRecords_ImportSessionId_RowNumber",
                table: "tblImportedRawRecords",
                columns: new[] { "ImportSessionId", "RowNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tblImportedRawRecords_ImportSessionId_RowNumber",
                table: "tblImportedRawRecords");

            migrationBuilder.CreateIndex(
                name: "IX_tblImportedRawRecords_ImportSessionId",
                table: "tblImportedRawRecords",
                column: "ImportSessionId");
        }
    }
}

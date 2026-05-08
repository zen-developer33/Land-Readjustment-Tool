using Land_Readjustment_Tool.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260508120000_AddCanvasLayerHatchScale")]
    public partial class AddCanvasLayerHatchScale : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // HatchScale is already materialized by 20260508072541_done during
            // SQLite's table rebuild. Keep this migration as a no-op so project
            // files that already know this migration can still advance safely.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: the column belongs to the previous migration.
        }
    }
}

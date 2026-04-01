using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class Some_datum_parameters_changed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DeltaX", "DeltaY", "DeltaZ", "RotationX", "RotationY", "RotationZ", "ScalePpm" },
                values: new object[] { -124.3813, 521.66999999999996, 764.51369999999997, 17.148800000000001, -8.1153600000000008, 11.184200000000001, -2.1105 });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DeltaX", "DeltaY", "DeltaZ", "Name", "Source" },
                values: new object[] { 296.20699999999999, 731.54499999999996, 273.00099999999998, "Nagarkot TM", "Nagarkot TM" });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DeltaX", "DeltaY", "DeltaZ" },
                values: new object[] { 295.0, 736.0, 257.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DeltaX", "DeltaY", "DeltaZ", "RotationX", "RotationY", "RotationZ", "ScalePpm" },
                values: new object[] { 293.17000000000002, 726.17999999999995, 245.36000000000001, 0.0, 0.0, 0.0, 0.0 });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DeltaX", "DeltaY", "DeltaZ", "Name", "Source" },
                values: new object[] { 295.0, 740.0, 460.0, "Nagarkot GPS Campaign 1994", "Nagarkot GPS Campaign 1994" });

            migrationBuilder.UpdateData(
                table: "tblDatumTransformations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DeltaX", "DeltaY", "DeltaZ" },
                values: new object[] { 283.0, 682.0, 231.0 });
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class xl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e5555555-1111-1111-1111-111111111111"),
                column: "ModelName",
                value: "Meisterstück");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e5555555-1111-1111-1111-111111111111"),
                column: "ModelName",
                value: "eeisterstück");
        }
    }
}

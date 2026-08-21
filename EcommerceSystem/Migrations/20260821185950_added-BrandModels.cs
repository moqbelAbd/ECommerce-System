using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EcommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class addedBrandModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ProductBrands",
                columns: new[] { "ProductBrandId", "BrandName" },
                values: new object[,]
                {
                    { new Guid("b1111111-1111-1111-1111-111111111111"), "Rolex" },
                    { new Guid("b2222222-2222-2222-2222-222222222222"), "Tissot" },
                    { new Guid("b3333333-3333-3333-3333-333333333333"), "Casio" },
                    { new Guid("b4444444-4444-4444-4444-444444444444"), "Apple" },
                    { new Guid("b5555555-5555-5555-5555-555555555555"), "eontblanc" }
                });

            migrationBuilder.InsertData(
                table: "ProductModels",
                columns: new[] { "ProductModelId", "ModelName" },
                values: new object[,]
                {
                    { new Guid("e1111111-1111-1111-1111-111111111111"), "Submariner" },
                    { new Guid("e1111111-2222-2222-2222-222222222222"), "Datejust" },
                    { new Guid("e2222222-1111-1111-1111-111111111111"), "Le Locle" },
                    { new Guid("e2222222-2222-2222-2222-222222222222"), "PRX" },
                    { new Guid("e3333333-1111-1111-1111-111111111111"), "G-Shock" },
                    { new Guid("e3333333-2222-2222-2222-222222222222"), "Edifice" },
                    { new Guid("e4444444-1111-1111-1111-111111111111"), "Series 9" },
                    { new Guid("e4444444-2222-2222-2222-222222222222"), "Ultra 2" },
                    { new Guid("e5555555-1111-1111-1111-111111111111"), "eeisterstück" },
                    { new Guid("e5555555-2222-2222-2222-222222222222"), "Sartorial" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProductBrands",
                keyColumn: "ProductBrandId",
                keyValue: new Guid("b1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "ProductBrands",
                keyColumn: "ProductBrandId",
                keyValue: new Guid("b2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "ProductBrands",
                keyColumn: "ProductBrandId",
                keyValue: new Guid("b3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "ProductBrands",
                keyColumn: "ProductBrandId",
                keyValue: new Guid("b4444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "ProductBrands",
                keyColumn: "ProductBrandId",
                keyValue: new Guid("b5555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e1111111-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e2222222-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e3333333-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e3333333-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e4444444-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e4444444-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e5555555-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e5555555-2222-2222-2222-222222222222"));
        }
    }
}

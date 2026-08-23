using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class _4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductBrandId",
                table: "ProductModels",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111111"),
                column: "ProductBrandId",
                value: new Guid("b1111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e1111111-2222-2222-2222-222222222222"),
                column: "ProductBrandId",
                value: new Guid("b1111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e2222222-1111-1111-1111-111111111111"),
                column: "ProductBrandId",
                value: new Guid("b2222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e2222222-2222-2222-2222-222222222222"),
                column: "ProductBrandId",
                value: new Guid("b2222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e3333333-1111-1111-1111-111111111111"),
                column: "ProductBrandId",
                value: new Guid("b3333333-3333-3333-3333-333333333333"));

            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e3333333-2222-2222-2222-222222222222"),
                column: "ProductBrandId",
                value: new Guid("b3333333-3333-3333-3333-333333333333"));

            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e4444444-1111-1111-1111-111111111111"),
                column: "ProductBrandId",
                value: new Guid("b4444444-4444-4444-4444-444444444444"));

            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e4444444-2222-2222-2222-222222222222"),
                column: "ProductBrandId",
                value: new Guid("b4444444-4444-4444-4444-444444444444"));

            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e5555555-1111-1111-1111-111111111111"),
                column: "ProductBrandId",
                value: new Guid("b5555555-5555-5555-5555-555555555555"));

            migrationBuilder.UpdateData(
                table: "ProductModels",
                keyColumn: "ProductModelId",
                keyValue: new Guid("e5555555-2222-2222-2222-222222222222"),
                column: "ProductBrandId",
                value: new Guid("b5555555-5555-5555-5555-555555555555"));

            migrationBuilder.CreateIndex(
                name: "IX_ProductModels_ProductBrandId",
                table: "ProductModels",
                column: "ProductBrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductModels_ProductBrands_ProductBrandId",
                table: "ProductModels",
                column: "ProductBrandId",
                principalTable: "ProductBrands",
                principalColumn: "ProductBrandId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductModels_ProductBrands_ProductBrandId",
                table: "ProductModels");

            migrationBuilder.DropIndex(
                name: "IX_ProductModels_ProductBrandId",
                table: "ProductModels");

            migrationBuilder.DropColumn(
                name: "ProductBrandId",
                table: "ProductModels");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Orders");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class fixinfIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "SubCategories",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Products",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "CustomerPhoneNumbers",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Categories",
                newName: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "SubCategories",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Products",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "CustomerPhoneNumbers",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Categories",
                newName: "IsDeleted");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerCardsAndPhones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardHolderName",
                table: "CustomerPaymentCards",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardHolderName",
                table: "CustomerPaymentCards");
        }
    }
}

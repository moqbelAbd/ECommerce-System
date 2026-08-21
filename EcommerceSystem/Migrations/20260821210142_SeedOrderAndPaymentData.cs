using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EcommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedOrderAndPaymentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "OrderStatusId", "OrderStatusName" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Processing" },
                    { 3, "Shipped" },
                    { 4, "Delivered" },
                    { 5, "Cancelled" }
                });

            migrationBuilder.InsertData(
                table: "PaymentStatuses",
                columns: new[] { "PaymentStatusId", "PaymentStatusName" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Completed" },
                    { 3, "Failed" },
                    { 4, "Refunded" }
                });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "PaymentTypeId", "PaymentTypeName" },
                values: new object[,]
                {
                    { 1, "Credit Card" },
                    { 2, "PayPal" },
                    { 3, "Cash on Delivery (COD)" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "OrderStatusId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "OrderStatusId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "OrderStatusId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "OrderStatusId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "OrderStatusId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PaymentStatuses",
                keyColumn: "PaymentStatusId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PaymentStatuses",
                keyColumn: "PaymentStatusId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PaymentStatuses",
                keyColumn: "PaymentStatusId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PaymentStatuses",
                keyColumn: "PaymentStatusId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "PaymentTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "PaymentTypeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "PaymentTypeId",
                keyValue: 3);
        }
    }
}

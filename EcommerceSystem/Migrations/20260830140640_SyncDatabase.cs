using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class SyncDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "OrderStatusId",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "OrderStatusId",
                keyValue: 3,
                column: "OrderStatusName",
                value: "Delivered");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "OrderStatusId",
                keyValue: 4,
                column: "OrderStatusName",
                value: "Cancelled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "OrderStatusId",
                keyValue: 3,
                column: "OrderStatusName",
                value: "Shipped");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "OrderStatusId",
                keyValue: 4,
                column: "OrderStatusName",
                value: "Delivered");

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "OrderStatusId", "OrderStatusName" },
                values: new object[] { 5, "Cancelled" });
        }
    }
}

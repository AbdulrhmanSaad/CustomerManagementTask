using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomersTask4.Migrations
{
    /// <inheritdoc />
    public partial class updateRelationToBeNoActionOnCustomerHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerHistories_Customers_CustomerId",
                table: "CustomerHistories");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "CustomerHistories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerHistories_Customers_CustomerId",
                table: "CustomerHistories",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerHistories_Customers_CustomerId",
                table: "CustomerHistories");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "CustomerHistories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerHistories_Customers_CustomerId",
                table: "CustomerHistories",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

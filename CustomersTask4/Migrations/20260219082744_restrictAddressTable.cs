using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomersTask4.Migrations
{
    /// <inheritdoc />
    public partial class restrictAddressTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Addresses_CustomerId",
                table: "Addresses");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CustomerId_AddressType",
                table: "Addresses",
                columns: new[] { "CustomerId", "AddressType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Addresses_CustomerId_AddressType",
                table: "Addresses");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CustomerId",
                table: "Addresses",
                column: "CustomerId");
        }
    }
}

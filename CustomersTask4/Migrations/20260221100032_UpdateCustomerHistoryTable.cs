using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomersTask4.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomerHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "CustomerHistories",
                newName: "UserRole");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "CustomerHistories",
                newName: "ChangedAt");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "CustomerHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KeyValues",
                table: "CustomerHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NewValues",
                table: "CustomerHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OldValues",
                table: "CustomerHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "CustomerHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CustomerHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "CustomerHistories");

            migrationBuilder.DropColumn(
                name: "KeyValues",
                table: "CustomerHistories");

            migrationBuilder.DropColumn(
                name: "NewValues",
                table: "CustomerHistories");

            migrationBuilder.DropColumn(
                name: "OldValues",
                table: "CustomerHistories");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "CustomerHistories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CustomerHistories");

            migrationBuilder.RenameColumn(
                name: "UserRole",
                table: "CustomerHistories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ChangedAt",
                table: "CustomerHistories",
                newName: "Date");
        }
    }
}

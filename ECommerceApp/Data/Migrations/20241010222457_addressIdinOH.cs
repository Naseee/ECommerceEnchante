using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class addressIdinOH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "OrderHeaders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_AddressId",
                table: "OrderHeaders",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_Addresses_AddressId",
                table: "OrderHeaders",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_Addresses_AddressId",
                table: "OrderHeaders");

            migrationBuilder.DropIndex(
                name: "IX_OrderHeaders_AddressId",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "OrderHeaders");
        }
    }
}

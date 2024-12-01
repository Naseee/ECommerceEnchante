using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class addOfferAttributesinCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasOffer",
                table: "Categories",
                newName: "IsActiveOffer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActiveOffer",
                table: "Categories",
                newName: "HasOffer");
        }
    }
}

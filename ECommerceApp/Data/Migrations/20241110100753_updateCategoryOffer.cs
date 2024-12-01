using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateCategoryOffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OfferId",
                table: "CategoryOffer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryOffer_OfferId",
                table: "CategoryOffer",
                column: "OfferId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryOffer_Offers_OfferId",
                table: "CategoryOffer",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "OfferId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryOffer_Offers_OfferId",
                table: "CategoryOffer");

            migrationBuilder.DropIndex(
                name: "IX_CategoryOffer_OfferId",
                table: "CategoryOffer");

            migrationBuilder.DropColumn(
                name: "OfferId",
                table: "CategoryOffer");
        }
    }
}

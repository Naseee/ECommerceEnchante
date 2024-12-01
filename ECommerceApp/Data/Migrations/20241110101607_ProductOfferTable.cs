using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductOfferTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryOffer_Categories_CategoryId",
                table: "CategoryOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoryOffer_Offers_OfferId",
                table: "CategoryOffer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryOffer",
                table: "CategoryOffer");

            migrationBuilder.RenameTable(
                name: "CategoryOffer",
                newName: "CategoryOffers");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryOffer_OfferId",
                table: "CategoryOffers",
                newName: "IX_CategoryOffers_OfferId");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryOffer_CategoryId",
                table: "CategoryOffers",
                newName: "IX_CategoryOffers_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryOffers",
                table: "CategoryOffers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ProductOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOffers_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "OfferId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductOffers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductOffers_OfferId",
                table: "ProductOffers",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOffers_ProductId",
                table: "ProductOffers",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryOffers_Categories_CategoryId",
                table: "CategoryOffers",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryOffers_Offers_OfferId",
                table: "CategoryOffers",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "OfferId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryOffers_Categories_CategoryId",
                table: "CategoryOffers");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoryOffers_Offers_OfferId",
                table: "CategoryOffers");

            migrationBuilder.DropTable(
                name: "ProductOffers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryOffers",
                table: "CategoryOffers");

            migrationBuilder.RenameTable(
                name: "CategoryOffers",
                newName: "CategoryOffer");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryOffers_OfferId",
                table: "CategoryOffer",
                newName: "IX_CategoryOffer_OfferId");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryOffers_CategoryId",
                table: "CategoryOffer",
                newName: "IX_CategoryOffer_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryOffer",
                table: "CategoryOffer",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryOffer_Categories_CategoryId",
                table: "CategoryOffer",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryOffer_Offers_OfferId",
                table: "CategoryOffer",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "OfferId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

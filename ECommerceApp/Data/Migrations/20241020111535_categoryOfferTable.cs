using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class categoryOfferTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsActiveOffer",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "OfferEndDate",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "OfferStartDate",
                table: "Categories");

            migrationBuilder.CreateTable(
                name: "CategoryOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActiveOffer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    OfferStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OfferEndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryOffers_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryOffers_CategoryId",
                table: "CategoryOffers",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryOffers");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Categories",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveOffer",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferEndDate",
                table: "Categories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferStartDate",
                table: "Categories",
                type: "datetime2",
                nullable: true);
        }
    }
}

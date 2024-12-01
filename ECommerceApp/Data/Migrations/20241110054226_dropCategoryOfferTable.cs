using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class dropCategoryOfferTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryOffers_Categories_CategoryId",
                table: "CategoryOffers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryOffers",
                table: "CategoryOffers");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "CategoryOffers");

            migrationBuilder.DropColumn(
                name: "IsActiveOffer",
                table: "CategoryOffers");

            migrationBuilder.DropColumn(
                name: "OfferEndDate",
                table: "CategoryOffers");

            migrationBuilder.DropColumn(
                name: "OfferStartDate",
                table: "CategoryOffers");

            migrationBuilder.RenameTable(
                name: "CategoryOffers",
                newName: "CategoryOffer");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryOffer_Categories_CategoryId",
                table: "CategoryOffer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryOffer",
                table: "CategoryOffer");

            migrationBuilder.RenameTable(
                name: "CategoryOffer",
                newName: "CategoryOffers");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryOffer_CategoryId",
                table: "CategoryOffers",
                newName: "IX_CategoryOffers_CategoryId");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "CategoryOffers",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveOffer",
                table: "CategoryOffers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferEndDate",
                table: "CategoryOffers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferStartDate",
                table: "CategoryOffers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryOffers",
                table: "CategoryOffers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryOffers_Categories_CategoryId",
                table: "CategoryOffers",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

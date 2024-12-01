using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountedTotalToOrderHeaders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsActiveOffer",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OfferEndDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OfferStartDate",
                table: "Products");

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

            migrationBuilder.AddColumn<double>(
                name: "DiscountedTotal",
                table: "OrderHeaders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountedTotal",
                table: "OrderHeaders");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Products",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveOffer",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferEndDate",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferStartDate",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Categories",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveOffer",
                table: "Categories",
                type: "bit",
                nullable: true);

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

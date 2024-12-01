using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class offersTocategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Categories",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasOffer",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "HasOffer",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "OfferEndDate",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "OfferStartDate",
                table: "Categories");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangesinCategoryTable : Migration
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}

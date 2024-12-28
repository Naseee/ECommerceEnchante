﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedForreturnToOrderDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApprovedForReturn",
                table: "OrderDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApprovedForReturn",
                table: "OrderDetails");
        }
    }
}
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReferralTableTodb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferralCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferrerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefferedUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReferreeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferreeUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReferralCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRewardGiven = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referrals_AspNetUsers_ReferreeUserId",
                        column: x => x.ReferreeUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Referrals_AspNetUsers_RefferedUserId",
                        column: x => x.RefferedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferreeUserId",
                table: "Referrals",
                column: "ReferreeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_RefferedUserId",
                table: "Referrals",
                column: "RefferedUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropColumn(
                name: "ReferralCode",
                table: "AspNetUsers");
        }
    }
}

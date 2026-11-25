using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eRynok.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderGroupId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderGroups_Users_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderGroupId",
                table: "Orders",
                column: "OrderGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderGroups_BuyerId",
                table: "OrderGroups",
                column: "BuyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderGroups_OrderGroupId",
                table: "Orders",
                column: "OrderGroupId",
                principalTable: "OrderGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderGroups_OrderGroupId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "OrderGroups");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderGroupId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderGroupId",
                table: "Orders");
        }
    }
}

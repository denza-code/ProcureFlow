using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureFlow.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseRequestWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RequesterUserId",
                table: "PurchaseRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DecidedAtUtc",
                table: "PurchaseRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionByUserId",
                table: "PurchaseRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_DecisionByUserId",
                table: "PurchaseRequests",
                column: "DecisionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_RequesterUserId",
                table: "PurchaseRequests",
                column: "RequesterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_AspNetUsers_DecisionByUserId",
                table: "PurchaseRequests",
                column: "DecisionByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_AspNetUsers_RequesterUserId",
                table: "PurchaseRequests",
                column: "RequesterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_AspNetUsers_DecisionByUserId",
                table: "PurchaseRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_AspNetUsers_RequesterUserId",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_DecisionByUserId",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_RequesterUserId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "DecidedAtUtc",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "DecisionByUserId",
                table: "PurchaseRequests");

            migrationBuilder.AlterColumn<string>(
                name: "RequesterUserId",
                table: "PurchaseRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}

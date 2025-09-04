using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddCallChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Calls_ChatId",
                table: "Calls",
                column: "ChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Calls_Chats_ChatId",
                table: "Calls",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calls_Chats_ChatId",
                table: "Calls");

            migrationBuilder.DropIndex(
                name: "IX_Calls_ChatId",
                table: "Calls");
        }
    }
}

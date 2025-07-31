using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Base64 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "ImageString",
                table: "Messages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageString",
                table: "Messages");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Messages",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}

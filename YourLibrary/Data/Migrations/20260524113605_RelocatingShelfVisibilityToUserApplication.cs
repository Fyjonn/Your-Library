using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class RelocatingShelfVisibilityToUserApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShelfVisibility",
                table: "UserBooks");

            migrationBuilder.AddColumn<bool>(
                name: "ShelfVisibility",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShelfVisibility",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "ShelfVisibility",
                table: "UserBooks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

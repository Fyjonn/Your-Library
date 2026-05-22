using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFieldTypeToStringToBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Bookmark",
                table: "UserBooks",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Bookmark",
                table: "UserBooks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}

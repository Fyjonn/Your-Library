using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalReadStatusToBorrow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginalOwnerReadStatus",
                table: "Borrows",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalOwnerReadStatus",
                table: "Borrows");
        }
    }
}

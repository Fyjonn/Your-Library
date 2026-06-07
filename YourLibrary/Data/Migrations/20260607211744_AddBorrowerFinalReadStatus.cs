using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBorrowerFinalReadStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BorrowerFinalReadStatus",
                table: "Borrows",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BorrowerFinalReadStatus",
                table: "Borrows");
        }
    }
}

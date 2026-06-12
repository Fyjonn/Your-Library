using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditingBorrow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BorrowerBookmark",
                table: "Borrows",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BorrowerLocation",
                table: "Borrows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BorrowerNotes",
                table: "Borrows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BorrowerRating",
                table: "Borrows",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BorrowerReviewComment",
                table: "Borrows",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BorrowerBookmark",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "BorrowerLocation",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "BorrowerNotes",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "BorrowerRating",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "BorrowerReviewComment",
                table: "Borrows");
        }
    }
}

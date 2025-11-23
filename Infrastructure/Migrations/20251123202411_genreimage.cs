using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class genreimage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imageUrl",
                table: "Genres",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 1,
                column: "imageUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 2,
                column: "imageUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 3,
                column: "imageUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 4,
                column: "imageUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 5,
                column: "imageUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 6,
                column: "imageUrl",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imageUrl",
                table: "Genres");
        }
    }
}

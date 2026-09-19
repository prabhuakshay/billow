using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessPrinting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorisedSignatory",
                table: "Businesses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterText",
                table: "Businesses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Logo",
                table: "Businesses",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorisedSignatory",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "FooterText",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Businesses");
        }
    }
}

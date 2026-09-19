using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessGstin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Gstin",
                table: "Businesses",
                type: "TEXT",
                maxLength: 15,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gstin",
                table: "Businesses");
        }
    }
}

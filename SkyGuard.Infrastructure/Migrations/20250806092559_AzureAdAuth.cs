using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyGuard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AzureAdAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAzureAdUser",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAzureAdUser",
                table: "Users");
        }
    }
}

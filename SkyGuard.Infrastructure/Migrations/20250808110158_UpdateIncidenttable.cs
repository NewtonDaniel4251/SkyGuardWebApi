using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyGuard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIncidenttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SharePointLink",
                table: "Incidents",
                newName: "ImageLink");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageLink",
                table: "Incidents",
                newName: "SharePointLink");
        }
    }
}

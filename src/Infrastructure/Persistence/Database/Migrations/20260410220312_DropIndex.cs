using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class DropIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_collaborators_identification_number",
                schema: "public",
                table: "collaborators");

            migrationBuilder.CreateIndex(
                name: "IX_collaborators_identification_number",
                schema: "public",
                table: "collaborators",
                column: "identification_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_collaborators_identification_number",
                schema: "public",
                table: "collaborators");

            migrationBuilder.CreateIndex(
                name: "IX_collaborators_identification_number",
                schema: "public",
                table: "collaborators",
                column: "identification_number",
                unique: true);
        }
    }
}

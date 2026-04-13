using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class EliminarColumna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "public",
                table: "permit_applications");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                schema: "public",
                table: "permit_applications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                schema: "public",
                table: "permit_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                schema: "public",
                table: "permit_applications",
                type: "text",
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurarColumnaSalarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_salaries_collaborators_CollaboratorId",
                schema: "public",
                table: "salaries");

            migrationBuilder.RenameColumn(
                name: "CollaboratorId",
                schema: "public",
                table: "salaries",
                newName: "collaborator_id");

            migrationBuilder.RenameIndex(
                name: "IX_salaries_CollaboratorId",
                schema: "public",
                table: "salaries",
                newName: "IX_salaries_collaborator_id");

            migrationBuilder.AddForeignKey(
                name: "FK_salaries_collaborators_collaborator_id",
                schema: "public",
                table: "salaries",
                column: "collaborator_id",
                principalSchema: "public",
                principalTable: "collaborators",
                principalColumn: "collaborator_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_salaries_collaborators_collaborator_id",
                schema: "public",
                table: "salaries");

            migrationBuilder.RenameColumn(
                name: "collaborator_id",
                schema: "public",
                table: "salaries",
                newName: "CollaboratorId");

            migrationBuilder.RenameIndex(
                name: "IX_salaries_collaborator_id",
                schema: "public",
                table: "salaries",
                newName: "IX_salaries_CollaboratorId");

            migrationBuilder.AddForeignKey(
                name: "FK_salaries_collaborators_CollaboratorId",
                schema: "public",
                table: "salaries",
                column: "CollaboratorId",
                principalSchema: "public",
                principalTable: "collaborators",
                principalColumn: "collaborator_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

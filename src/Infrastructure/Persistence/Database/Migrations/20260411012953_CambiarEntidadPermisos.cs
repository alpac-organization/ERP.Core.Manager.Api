using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class CambiarEntidadPermisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "rejected_by",
                schema: "public",
                table: "permit_applications",
                newName: "RejectedBy");

            migrationBuilder.RenameColumn(
                name: "approved_by",
                schema: "public",
                table: "permit_applications",
                newName: "ApprovedBy");

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                schema: "public",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "additional_data",
                schema: "public",
                table: "permit_applications",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "administrator_fullname",
                schema: "public",
                table: "permit_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "first_step_approved",
                schema: "public",
                table: "permit_applications",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "manager_fullname",
                schema: "public",
                table: "permit_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "second_step_approved",
                schema: "public",
                table: "permit_applications",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaId",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "additional_data",
                schema: "public",
                table: "permit_applications");

            migrationBuilder.DropColumn(
                name: "administrator_fullname",
                schema: "public",
                table: "permit_applications");

            migrationBuilder.DropColumn(
                name: "first_step_approved",
                schema: "public",
                table: "permit_applications");

            migrationBuilder.DropColumn(
                name: "manager_fullname",
                schema: "public",
                table: "permit_applications");

            migrationBuilder.DropColumn(
                name: "second_step_approved",
                schema: "public",
                table: "permit_applications");

            migrationBuilder.RenameColumn(
                name: "RejectedBy",
                schema: "public",
                table: "permit_applications",
                newName: "rejected_by");

            migrationBuilder.RenameColumn(
                name: "ApprovedBy",
                schema: "public",
                table: "permit_applications",
                newName: "approved_by");
        }
    }
}

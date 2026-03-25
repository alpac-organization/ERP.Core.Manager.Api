using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorregirTablaInformacionTrabaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkPhonNumber",
                schema: "public",
                table: "working_information",
                newName: "work_phone_number");

            migrationBuilder.RenameColumn(
                name: "WorkEmail",
                schema: "public",
                table: "working_information",
                newName: "work_email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "work_phone_number",
                schema: "public",
                table: "working_information",
                newName: "WorkPhonNumber");

            migrationBuilder.RenameColumn(
                name: "work_email",
                schema: "public",
                table: "working_information",
                newName: "WorkEmail");
        }
    }
}

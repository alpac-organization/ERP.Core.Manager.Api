using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModificarInformacionTrabajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE public.working_information ALTER COLUMN branch_id TYPE integer USING branch_id::integer;");

            migrationBuilder.AddColumn<int>(
                name: "BankSubCatalogId",
                schema: "public",
                table: "salaries",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.DropColumn(
                name: "BankSubCatalogId",
                schema: "public",
                table: "salaries");

            // Reversión manual
            migrationBuilder.Sql("ALTER TABLE public.working_information ALTER COLUMN branch_id TYPE text USING branch_id::text;");

            /*
            migrationBuilder.AlterColumn<string>(
                name: "branch_id",
                schema: "public",
                table: "working_information",
                ...);
            */
        }
    }
}

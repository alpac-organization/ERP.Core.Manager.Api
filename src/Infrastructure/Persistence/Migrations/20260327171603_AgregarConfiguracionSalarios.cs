using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConfiguracionSalarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Currency",
                schema: "public",
                table: "salaries",
                newName: "currency");

            migrationBuilder.RenameColumn(
                name: "SalaryType",
                schema: "public",
                table: "salaries",
                newName: "salary_type");

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                schema: "public",
                table: "salaries",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "salary_type",
                schema: "public",
                table: "salaries",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "currency",
                schema: "public",
                table: "salaries",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "salary_type",
                schema: "public",
                table: "salaries",
                newName: "SalaryType");

            migrationBuilder.AlterColumn<int>(
                name: "Currency",
                schema: "public",
                table: "salaries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "SalaryType",
                schema: "public",
                table: "salaries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}

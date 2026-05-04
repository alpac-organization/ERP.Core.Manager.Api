using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class EliminarEspacionColumnas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_income_tax_accrual_payrolls_payroll_id ",
                schema: "public",
                table: "income_tax_accrual");

            migrationBuilder.RenameColumn(
                name: "payroll_id ",
                schema: "public",
                table: "income_tax_accrual",
                newName: "payroll_id");

            migrationBuilder.RenameIndex(
                name: "IX_income_tax_accrual_payroll_id ",
                schema: "public",
                table: "income_tax_accrual",
                newName: "IX_income_tax_accrual_payroll_id");

            migrationBuilder.AddForeignKey(
                name: "FK_income_tax_accrual_payrolls_payroll_id",
                schema: "public",
                table: "income_tax_accrual",
                column: "payroll_id",
                principalSchema: "public",
                principalTable: "payrolls",
                principalColumn: "payroll_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_income_tax_accrual_payrolls_payroll_id",
                schema: "public",
                table: "income_tax_accrual");

            migrationBuilder.RenameColumn(
                name: "payroll_id",
                schema: "public",
                table: "income_tax_accrual",
                newName: "payroll_id ");

            migrationBuilder.RenameIndex(
                name: "IX_income_tax_accrual_payroll_id",
                schema: "public",
                table: "income_tax_accrual",
                newName: "IX_income_tax_accrual_payroll_id ");

            migrationBuilder.AddForeignKey(
                name: "FK_income_tax_accrual_payrolls_payroll_id ",
                schema: "public",
                table: "income_tax_accrual",
                column: "payroll_id ",
                principalSchema: "public",
                principalTable: "payrolls",
                principalColumn: "payroll_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

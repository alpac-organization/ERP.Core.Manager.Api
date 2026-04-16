using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class CorregirColumnasNomina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payrolls_companies_CompanyId",
                schema: "public",
                table: "payrolls");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "public",
                table: "payrolls",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                schema: "public",
                table: "payrolls",
                newName: "company_id");

            migrationBuilder.RenameIndex(
                name: "IX_payrolls_CompanyId",
                schema: "public",
                table: "payrolls",
                newName: "IX_payrolls_company_id");

            migrationBuilder.AddForeignKey(
                name: "FK_payrolls_companies_company_id",
                schema: "public",
                table: "payrolls",
                column: "company_id",
                principalSchema: "public",
                principalTable: "companies",
                principalColumn: "company_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payrolls_companies_company_id",
                schema: "public",
                table: "payrolls");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "public",
                table: "payrolls",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "company_id",
                schema: "public",
                table: "payrolls",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_payrolls_company_id",
                schema: "public",
                table: "payrolls",
                newName: "IX_payrolls_CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_payrolls_companies_CompanyId",
                schema: "public",
                table: "payrolls",
                column: "CompanyId",
                principalSchema: "public",
                principalTable: "companies",
                principalColumn: "company_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

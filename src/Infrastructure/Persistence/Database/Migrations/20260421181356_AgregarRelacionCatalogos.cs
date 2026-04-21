using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRelacionCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                schema: "public",
                table: "payrolls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_payrolls_branch_id",
                schema: "public",
                table: "payrolls",
                column: "branch_id");

            migrationBuilder.AddForeignKey(
                name: "FK_payrolls_sub_catalogs_branch_id",
                schema: "public",
                table: "payrolls",
                column: "branch_id",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payrolls_sub_catalogs_branch_id",
                schema: "public",
                table: "payrolls");

            migrationBuilder.DropIndex(
                name: "IX_payrolls_branch_id",
                schema: "public",
                table: "payrolls");

            migrationBuilder.DropColumn(
                name: "branch_id",
                schema: "public",
                table: "payrolls");
        }
    }
}

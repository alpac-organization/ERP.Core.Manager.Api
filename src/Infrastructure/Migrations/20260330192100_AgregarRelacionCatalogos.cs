using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRelacionCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "work_position_id",
                schema: "public",
                table: "working_information",
                newName: "WorkPositionId");

            migrationBuilder.RenameColumn(
                name: "work_area_id",
                schema: "public",
                table: "working_information",
                newName: "WorkAreaId");

            migrationBuilder.RenameColumn(
                name: "branch_id",
                schema: "public",
                table: "working_information",
                newName: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_working_information_BranchId",
                schema: "public",
                table: "working_information",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_working_information_WorkAreaId",
                schema: "public",
                table: "working_information",
                column: "WorkAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_working_information_WorkPositionId",
                schema: "public",
                table: "working_information",
                column: "WorkPositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_working_information_sub_catalogs_BranchId",
                schema: "public",
                table: "working_information",
                column: "BranchId",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_working_information_sub_catalogs_WorkAreaId",
                schema: "public",
                table: "working_information",
                column: "WorkAreaId",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_working_information_sub_catalogs_WorkPositionId",
                schema: "public",
                table: "working_information",
                column: "WorkPositionId",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_working_information_sub_catalogs_BranchId",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropForeignKey(
                name: "FK_working_information_sub_catalogs_WorkAreaId",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropForeignKey(
                name: "FK_working_information_sub_catalogs_WorkPositionId",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropIndex(
                name: "IX_working_information_BranchId",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropIndex(
                name: "IX_working_information_WorkAreaId",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropIndex(
                name: "IX_working_information_WorkPositionId",
                schema: "public",
                table: "working_information");

            migrationBuilder.RenameColumn(
                name: "WorkPositionId",
                schema: "public",
                table: "working_information",
                newName: "work_position_id");

            migrationBuilder.RenameColumn(
                name: "WorkAreaId",
                schema: "public",
                table: "working_information",
                newName: "work_area_id");

            migrationBuilder.RenameColumn(
                name: "BranchId",
                schema: "public",
                table: "working_information",
                newName: "branch_id");
        }
    }
}

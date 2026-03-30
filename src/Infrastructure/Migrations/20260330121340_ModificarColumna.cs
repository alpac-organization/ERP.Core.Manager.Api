using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModificarColumna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_module_roles_modules_ModuleId",
                schema: "public",
                table: "user_module_roles");

            migrationBuilder.RenameColumn(
                name: "ModuleId",
                schema: "public",
                table: "user_module_roles",
                newName: "module_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_module_roles_ModuleId",
                schema: "public",
                table: "user_module_roles",
                newName: "IX_user_module_roles_module_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_module_roles_modules_module_id",
                schema: "public",
                table: "user_module_roles",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "module_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_module_roles_modules_module_id",
                schema: "public",
                table: "user_module_roles");

            migrationBuilder.RenameColumn(
                name: "module_id",
                schema: "public",
                table: "user_module_roles",
                newName: "ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_user_module_roles_module_id",
                schema: "public",
                table: "user_module_roles",
                newName: "IX_user_module_roles_ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_module_roles_modules_ModuleId",
                schema: "public",
                table: "user_module_roles",
                column: "ModuleId",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "module_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

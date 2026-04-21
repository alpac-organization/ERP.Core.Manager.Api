using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSucursales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branch_companies_CompanyId",
                schema: "public",
                table: "Branch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Branch",
                schema: "public",
                table: "Branch");

            migrationBuilder.RenameTable(
                name: "Branch",
                schema: "public",
                newName: "branches",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                schema: "public",
                table: "branches",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                schema: "public",
                table: "branches",
                newName: "company_id");

            migrationBuilder.RenameColumn(
                name: "CompanyAlias",
                schema: "public",
                table: "branches",
                newName: "company_alias");

            migrationBuilder.RenameColumn(
                name: "BranchName",
                schema: "public",
                table: "branches",
                newName: "branch_name");

            migrationBuilder.RenameColumn(
                name: "BranchAddress",
                schema: "public",
                table: "branches",
                newName: "branch_address");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "branches",
                newName: "branch_id");

            migrationBuilder.RenameIndex(
                name: "IX_Branch_CompanyId",
                schema: "public",
                table: "branches",
                newName: "IX_branches_company_id");

            migrationBuilder.AlterColumn<string>(
                name: "company_alias",
                schema: "public",
                table: "branches",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "branch_name",
                schema: "public",
                table: "branches",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "branch_address",
                schema: "public",
                table: "branches",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "branch_id",
                schema: "public",
                table: "branches",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "public",
                table: "branches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_branches",
                schema: "public",
                table: "branches",
                column: "branch_id");

            migrationBuilder.AddForeignKey(
                name: "FK_branches_companies_company_id",
                schema: "public",
                table: "branches",
                column: "company_id",
                principalSchema: "public",
                principalTable: "companies",
                principalColumn: "company_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_branches_companies_company_id",
                schema: "public",
                table: "branches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_branches",
                schema: "public",
                table: "branches");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "public",
                table: "branches");

            migrationBuilder.RenameTable(
                name: "branches",
                schema: "public",
                newName: "Branch",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "public",
                table: "Branch",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "company_id",
                schema: "public",
                table: "Branch",
                newName: "CompanyId");

            migrationBuilder.RenameColumn(
                name: "company_alias",
                schema: "public",
                table: "Branch",
                newName: "CompanyAlias");

            migrationBuilder.RenameColumn(
                name: "branch_name",
                schema: "public",
                table: "Branch",
                newName: "BranchName");

            migrationBuilder.RenameColumn(
                name: "branch_address",
                schema: "public",
                table: "Branch",
                newName: "BranchAddress");

            migrationBuilder.RenameColumn(
                name: "branch_id",
                schema: "public",
                table: "Branch",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_branches_company_id",
                schema: "public",
                table: "Branch",
                newName: "IX_Branch_CompanyId");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyAlias",
                schema: "public",
                table: "Branch",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BranchName",
                schema: "public",
                table: "Branch",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BranchAddress",
                schema: "public",
                table: "Branch",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "public",
                table: "Branch",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Branch",
                schema: "public",
                table: "Branch",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Branch_companies_CompanyId",
                schema: "public",
                table: "Branch",
                column: "CompanyId",
                principalSchema: "public",
                principalTable: "companies",
                principalColumn: "company_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

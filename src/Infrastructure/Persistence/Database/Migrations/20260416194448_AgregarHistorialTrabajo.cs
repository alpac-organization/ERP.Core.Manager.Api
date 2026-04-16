using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHistorialTrabajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkPositionHistory_collaborators_CollaboratorId",
                schema: "public",
                table: "WorkPositionHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkPositionHistory_sub_catalogs_WorkPositionId",
                schema: "public",
                table: "WorkPositionHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkPositionHistory",
                schema: "public",
                table: "WorkPositionHistory");

            migrationBuilder.RenameTable(
                name: "WorkPositionHistory",
                schema: "public",
                newName: "work_position_histories",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "WorkPositionId",
                schema: "public",
                table: "work_position_histories",
                newName: "work_position_id");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                schema: "public",
                table: "work_position_histories",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "CollaboratorId",
                schema: "public",
                table: "work_position_histories",
                newName: "collaborator_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "work_position_histories",
                newName: "work_position_history_id");

            migrationBuilder.RenameIndex(
                name: "IX_WorkPositionHistory_WorkPositionId",
                schema: "public",
                table: "work_position_histories",
                newName: "IX_work_position_histories_work_position_id");

            migrationBuilder.RenameIndex(
                name: "IX_WorkPositionHistory_CollaboratorId",
                schema: "public",
                table: "work_position_histories",
                newName: "IX_work_position_histories_collaborator_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "work_position_history_id",
                schema: "public",
                table: "work_position_histories",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "public",
                table: "work_position_histories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_work_position_histories",
                schema: "public",
                table: "work_position_histories",
                column: "work_position_history_id");

            migrationBuilder.AddForeignKey(
                name: "FK_work_position_histories_collaborators_collaborator_id",
                schema: "public",
                table: "work_position_histories",
                column: "collaborator_id",
                principalSchema: "public",
                principalTable: "collaborators",
                principalColumn: "collaborator_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_work_position_histories_sub_catalogs_work_position_id",
                schema: "public",
                table: "work_position_histories",
                column: "work_position_id",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_work_position_histories_collaborators_collaborator_id",
                schema: "public",
                table: "work_position_histories");

            migrationBuilder.DropForeignKey(
                name: "FK_work_position_histories_sub_catalogs_work_position_id",
                schema: "public",
                table: "work_position_histories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_work_position_histories",
                schema: "public",
                table: "work_position_histories");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "public",
                table: "work_position_histories");

            migrationBuilder.RenameTable(
                name: "work_position_histories",
                schema: "public",
                newName: "WorkPositionHistory",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "work_position_id",
                schema: "public",
                table: "WorkPositionHistory",
                newName: "WorkPositionId");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "public",
                table: "WorkPositionHistory",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "collaborator_id",
                schema: "public",
                table: "WorkPositionHistory",
                newName: "CollaboratorId");

            migrationBuilder.RenameColumn(
                name: "work_position_history_id",
                schema: "public",
                table: "WorkPositionHistory",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_work_position_histories_work_position_id",
                schema: "public",
                table: "WorkPositionHistory",
                newName: "IX_WorkPositionHistory_WorkPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_work_position_histories_collaborator_id",
                schema: "public",
                table: "WorkPositionHistory",
                newName: "IX_WorkPositionHistory_CollaboratorId");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "public",
                table: "WorkPositionHistory",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkPositionHistory",
                schema: "public",
                table: "WorkPositionHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkPositionHistory_collaborators_CollaboratorId",
                schema: "public",
                table: "WorkPositionHistory",
                column: "CollaboratorId",
                principalSchema: "public",
                principalTable: "collaborators",
                principalColumn: "collaborator_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkPositionHistory_sub_catalogs_WorkPositionId",
                schema: "public",
                table: "WorkPositionHistory",
                column: "WorkPositionId",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

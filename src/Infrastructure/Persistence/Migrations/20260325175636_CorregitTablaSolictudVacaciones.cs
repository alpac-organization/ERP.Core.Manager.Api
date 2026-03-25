using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorregitTablaSolictudVacaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkingInformation_collaborators_collaborator_id",
                schema: "public",
                table: "WorkingInformation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkingInformation",
                schema: "public",
                table: "WorkingInformation");

            migrationBuilder.RenameTable(
                name: "WorkingInformation",
                schema: "public",
                newName: "working_information",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                schema: "public",
                table: "vacation_requests",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "RequestedBy",
                schema: "public",
                table: "vacation_requests",
                newName: "requested_by");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                schema: "public",
                table: "vacation_requests",
                newName: "end_date");

            migrationBuilder.RenameColumn(
                name: "ApprovedBy",
                schema: "public",
                table: "vacation_requests",
                newName: "approved_by");

            migrationBuilder.RenameIndex(
                name: "IX_WorkingInformation_collaborator_id",
                schema: "public",
                table: "working_information",
                newName: "IX_working_information_collaborator_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_working_information",
                schema: "public",
                table: "working_information",
                column: "working_information_id");

            migrationBuilder.AddForeignKey(
                name: "FK_working_information_collaborators_collaborator_id",
                schema: "public",
                table: "working_information",
                column: "collaborator_id",
                principalSchema: "public",
                principalTable: "collaborators",
                principalColumn: "collaborator_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_working_information_collaborators_collaborator_id",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropPrimaryKey(
                name: "PK_working_information",
                schema: "public",
                table: "working_information");

            migrationBuilder.RenameTable(
                name: "working_information",
                schema: "public",
                newName: "WorkingInformation",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "start_date",
                schema: "public",
                table: "vacation_requests",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "requested_by",
                schema: "public",
                table: "vacation_requests",
                newName: "RequestedBy");

            migrationBuilder.RenameColumn(
                name: "end_date",
                schema: "public",
                table: "vacation_requests",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "approved_by",
                schema: "public",
                table: "vacation_requests",
                newName: "ApprovedBy");

            migrationBuilder.RenameIndex(
                name: "IX_working_information_collaborator_id",
                schema: "public",
                table: "WorkingInformation",
                newName: "IX_WorkingInformation_collaborator_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkingInformation",
                schema: "public",
                table: "WorkingInformation",
                column: "working_information_id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkingInformation_collaborators_collaborator_id",
                schema: "public",
                table: "WorkingInformation",
                column: "collaborator_id",
                principalSchema: "public",
                principalTable: "collaborators",
                principalColumn: "collaborator_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

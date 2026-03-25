using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "collaborators",
                schema: "public",
                columns: table => new
                {
                    collaborator_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    first_lastname = table.Column<string>(type: "text", nullable: false),
                    identification_number = table.Column<string>(type: "text", nullable: false),
                    collaborator_code = table.Column<string>(type: "text", nullable: false),
                    company_id = table.Column<int>(type: "integer", nullable: false),
                    second_name = table.Column<string>(type: "text", nullable: true),
                    third_name = table.Column<string>(type: "text", nullable: true),
                    second_lastname = table.Column<string>(type: "text", nullable: true),
                    RegisteredBy = table.Column<string>(type: "text", nullable: true),
                    gender = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    identification_type = table.Column<string>(type: "text", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collaborators", x => x.collaborator_id);
                    table.ForeignKey(
                        name: "FK_collaborators_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "public",
                        principalTable: "companies",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "personal_informations",
                schema: "public",
                columns: table => new
                {
                    personal_information_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    collaborator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    personal_email = table.Column<string>(type: "text", nullable: true),
                    personal_phone_number = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    departament = table.Column<string>(type: "text", nullable: true),
                    birthdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personal_informations", x => x.personal_information_id);
                    table.ForeignKey(
                        name: "FK_personal_informations_collaborators_collaborator_id",
                        column: x => x.collaborator_id,
                        principalSchema: "public",
                        principalTable: "collaborators",
                        principalColumn: "collaborator_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vacation_requests",
                schema: "public",
                columns: table => new
                {
                    vacation_request_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CollaboratorId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    RequestedBy = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacation_requests", x => x.vacation_request_id);
                    table.ForeignKey(
                        name: "FK_vacation_requests_collaborators_CollaboratorId",
                        column: x => x.CollaboratorId,
                        principalSchema: "public",
                        principalTable: "collaborators",
                        principalColumn: "collaborator_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vacations",
                schema: "public",
                columns: table => new
                {
                    vacation_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    collaborator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    available_vacations = table.Column<float>(type: "real", nullable: false),
                    genered_vacation = table.Column<float>(type: "real", nullable: false),
                    enjoyed_vacation = table.Column<float>(type: "real", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacations", x => x.vacation_id);
                    table.ForeignKey(
                        name: "FK_vacations_collaborators_collaborator_id",
                        column: x => x.collaborator_id,
                        principalSchema: "public",
                        principalTable: "collaborators",
                        principalColumn: "collaborator_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkingInformation",
                schema: "public",
                columns: table => new
                {
                    working_information_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    bank_account_number = table.Column<string>(type: "text", nullable: true),
                    WorkPhonNumber = table.Column<string>(type: "text", nullable: true),
                    WorkEmail = table.Column<string>(type: "text", nullable: true),
                    inss_number = table.Column<string>(type: "text", nullable: true),
                    departure_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    collaborator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_area_id = table.Column<int>(type: "integer", nullable: false),
                    work_position_id = table.Column<int>(type: "integer", nullable: false),
                    branch_id = table.Column<string>(type: "text", nullable: false),
                    entry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingInformation", x => x.working_information_id);
                    table.ForeignKey(
                        name: "FK_WorkingInformation_collaborators_collaborator_id",
                        column: x => x.collaborator_id,
                        principalSchema: "public",
                        principalTable: "collaborators",
                        principalColumn: "collaborator_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_collaborator_id",
                schema: "public",
                table: "collaborators",
                column: "collaborator_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_collaborators_collaborator_code",
                schema: "public",
                table: "collaborators",
                column: "collaborator_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_collaborators_company_id",
                schema: "public",
                table: "collaborators",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_collaborators_identification_number",
                schema: "public",
                table: "collaborators",
                column: "identification_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_personal_informations_collaborator_id",
                schema: "public",
                table: "personal_informations",
                column: "collaborator_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vacation_collaborator_id",
                schema: "public",
                table: "vacation_requests",
                column: "vacation_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vacation_requests_CollaboratorId",
                schema: "public",
                table: "vacation_requests",
                column: "CollaboratorId");

            migrationBuilder.CreateIndex(
                name: "IX_vacations_collaborator_id",
                schema: "public",
                table: "vacations",
                column: "collaborator_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkingInformation_collaborator_id",
                schema: "public",
                table: "WorkingInformation",
                column: "collaborator_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "personal_informations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "vacation_requests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "vacations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WorkingInformation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "collaborators",
                schema: "public");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "public",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    alias = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    company_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.company_id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "public",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    role_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    role_type = table.Column<string>(type: "text", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    fullname = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    IdentificationNumber = table.Column<string>(type: "text", nullable: true),
                    user_type = table.Column<string>(type: "text", nullable: false),
                    user_status = table.Column<string>(type: "text", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "catalogs",
                schema: "public",
                columns: table => new
                {
                    catalog_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    catalog_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    catalog_type = table.Column<string>(type: "text", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalogs", x => x.catalog_id);
                    table.ForeignKey(
                        name: "FK_catalogs_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "public",
                        principalTable: "companies",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    second_name = table.Column<string>(type: "text", nullable: true),
                    third_name = table.Column<string>(type: "text", nullable: true),
                    second_lastname = table.Column<string>(type: "text", nullable: true),
                    registered_by = table.Column<string>(type: "text", nullable: false),
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
                name: "modules",
                schema: "public",
                columns: table => new
                {
                    module_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    module_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modules", x => x.module_id);
                    table.ForeignKey(
                        name: "FK_modules_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "public",
                        principalTable: "companies",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "public",
                columns: table => new
                {
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    permission_name = table.Column<string>(type: "text", nullable: true),
                    permission_type = table.Column<string>(type: "text", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.permission_id);
                    table.ForeignKey(
                        name: "FK_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                schema: "public",
                columns: table => new
                {
                    session_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    refresh_token = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    company_code = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_sessions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_profiles",
                schema: "public",
                columns: table => new
                {
                    user_profile_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_profiles", x => x.user_profile_id);
                    table.ForeignKey(
                        name: "FK_users_profiles_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "public",
                        principalTable: "companies",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_profiles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sub_catalogs",
                schema: "public",
                columns: table => new
                {
                    sub_catalog_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    catalog_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    catalog_id = table.Column<int>(type: "integer", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sub_catalogs", x => x.sub_catalog_id);
                    table.ForeignKey(
                        name: "FK_sub_catalogs_catalogs_catalog_id",
                        column: x => x.catalog_id,
                        principalSchema: "public",
                        principalTable: "catalogs",
                        principalColumn: "catalog_id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "salaries",
                schema: "public",
                columns: table => new
                {
                    salary_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CollaboratorId = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_in_local = table.Column<decimal>(type: "numeric", nullable: false),
                    amount_in_foreign = table.Column<decimal>(type: "numeric", nullable: false),
                    amount_salary = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    SalaryType = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salaries", x => x.salary_id);
                    table.ForeignKey(
                        name: "FK_salaries_collaborators_CollaboratorId",
                        column: x => x.CollaboratorId,
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
                    approved_by = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    requested_by = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                name: "working_information",
                schema: "public",
                columns: table => new
                {
                    working_information_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    bank_account_number = table.Column<string>(type: "text", nullable: true),
                    work_phone_number = table.Column<string>(type: "text", nullable: true),
                    work_email = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_working_information", x => x.working_information_id);
                    table.ForeignKey(
                        name: "FK_working_information_collaborators_collaborator_id",
                        column: x => x.collaborator_id,
                        principalSchema: "public",
                        principalTable: "collaborators",
                        principalColumn: "collaborator_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_module_roles",
                schema: "public",
                columns: table => new
                {
                    user_module_role_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    module_code = table.Column<string>(type: "text", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_module_roles", x => x.user_module_role_id);
                    table.ForeignKey(
                        name: "FK_user_module_roles_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_module_roles_users_profiles_user_profile_id",
                        column: x => x.user_profile_id,
                        principalSchema: "public",
                        principalTable: "users_profiles",
                        principalColumn: "user_profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_catalogs_company_type",
                schema: "public",
                table: "catalogs",
                columns: new[] { "company_id", "catalog_name", "catalog_type" },
                unique: true);

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
                name: "IX_companies_code",
                schema: "public",
                table: "companies",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_modules_company_code",
                schema: "public",
                table: "modules",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_modules_company_id",
                schema: "public",
                table: "modules",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_permission_id",
                schema: "public",
                table: "permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_role_id",
                schema: "public",
                table: "permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_personal_informations_collaborator_id",
                schema: "public",
                table: "personal_informations",
                column: "collaborator_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_id",
                schema: "public",
                table: "roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_salaries_CollaboratorId",
                schema: "public",
                table: "salaries",
                column: "CollaboratorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_session_id",
                schema: "public",
                table: "sessions",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_user_id",
                schema: "public",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_sub_catalogs_catalog_id",
                schema: "public",
                table: "sub_catalogs",
                column: "catalog_id");

            migrationBuilder.CreateIndex(
                name: "IX_Unique_User_Module_Role",
                schema: "public",
                table: "user_module_roles",
                columns: new[] { "user_profile_id", "module_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_module_roles_role_id",
                schema: "public",
                table: "user_module_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_profiles_company_id",
                schema: "public",
                table: "users_profiles",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_profiles_user_id",
                schema: "public",
                table: "users_profiles",
                column: "user_id");

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
                name: "IX_working_information_collaborator_id",
                schema: "public",
                table: "working_information",
                column: "collaborator_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "modules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "personal_informations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "salaries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sessions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sub_catalogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_module_roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "vacation_requests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "vacations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "working_information",
                schema: "public");

            migrationBuilder.DropTable(
                name: "catalogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users_profiles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "collaborators",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "public");
        }
    }
}

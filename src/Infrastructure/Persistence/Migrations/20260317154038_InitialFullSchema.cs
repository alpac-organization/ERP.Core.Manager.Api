using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFullSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:catalog_type", "branches,work_areas,job_positions,document_types")
                .Annotation("Npgsql:Enum:permission_type", "read,create,update,delete")
                .Annotation("Npgsql:Enum:role_type", "administrator,collaborator,supervisor,document_types")
                .Annotation("Npgsql:Enum:user_status", "active,inactive,locked");

            migrationBuilder.CreateTable(
                name: "catalogs",
                schema: "public",
                columns: table => new
                {
                    catalog_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    catalog_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    catalog_type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalogs", x => x.catalog_id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "public",
                columns: table => new
                {
                    company_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    alias = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    company_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    role_type = table.Column<int>(type: "integer", nullable: false),
                    role_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
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
                    email = table.Column<string>(type: "text", nullable: true),
                    user_name = table.Column<string>(type: "text", nullable: true),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    user_status = table.Column<int>(type: "integer", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "sub_catalogs",
                schema: "public",
                columns: table => new
                {
                    sub_catalog_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    catalog_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    catalog_id = table.Column<int>(type: "integer", nullable: false)
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
                name: "modules",
                schema: "public",
                columns: table => new
                {
                    module_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    module_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    company_id = table.Column<int>(type: "integer", nullable: false),
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
                    permission_type = table.Column<int>(type: "integer", nullable: false),
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
                name: "Session",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Device = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Session_users_UserId",
                        column: x => x.UserId,
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
                    company_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_profiles", x => x.user_profile_id);
                    table.ForeignKey(
                        name: "FK_users_profiles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserModuleRoles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModuleRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserModuleRoles_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserModuleRoles_users_profiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalSchema: "public",
                        principalTable: "users_profiles",
                        principalColumn: "user_profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "ix_role_id",
                schema: "public",
                table: "roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_Session_UserId",
                schema: "public",
                table: "Session",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_sub_catalogs_catalog_id",
                schema: "public",
                table: "sub_catalogs",
                column: "catalog_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserModuleRoles_RoleId",
                schema: "public",
                table: "UserModuleRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModuleRoles_UserProfileId",
                schema: "public",
                table: "UserModuleRoles",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_users_profiles_user_id",
                schema: "public",
                table: "users_profiles",
                column: "user_id");
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
                name: "Session",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sub_catalogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UserModuleRoles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "companies",
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
                name: "users",
                schema: "public");
        }
    }
}

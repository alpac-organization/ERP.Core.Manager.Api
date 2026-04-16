using System;
using ERP.Core.Database.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablasNominasAndDeducciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:catalog_type_enum", "banks,branches,departaments,document_types,exchange_rates,job_positions,work_areas")
                .Annotation("Npgsql:Enum:collaborator_status_enum", "active,inactive,subsidy,suspended,terminated,testing_process,vacation")
                .Annotation("Npgsql:Enum:currency_enum", "nio,usd")
                .Annotation("Npgsql:Enum:deduction_type_enum", "advance_christmas_bonus,late_arrivals,loans,other_deductions")
                .Annotation("Npgsql:Enum:gender_type_enum", "man,women")
                .Annotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .Annotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .Annotation("Npgsql:Enum:permission_type_enum", "create,delete,read,update")
                .Annotation("Npgsql:Enum:permit_application_status_enum", "approved,cancelled,pending,rejected")
                .Annotation("Npgsql:Enum:permit_application_type_enum", "compensatory_time,donated_vacations,medical_appointment,paid_leave,special_leave,unpaid_leave,vacation")
                .Annotation("Npgsql:Enum:public.catalog_type_enum", "branches,work_areas,job_positions,document_types,banks,exchange_rates,departaments")
                .Annotation("Npgsql:Enum:public.collaborator_status_enum", "active,inactive,vacation,subsidy,suspended,terminated,testing_process")
                .Annotation("Npgsql:Enum:public.currency_enum", "nio,usd")
                .Annotation("Npgsql:Enum:public.deduction_type_enum", "loans,advance_christmas_bonus,late_arrivals,other_deductions")
                .Annotation("Npgsql:Enum:public.gender_type_enum", "man,women")
                .Annotation("Npgsql:Enum:public.identification_type_enum", "cedula,pasaporte,cedula_residencia")
                .Annotation("Npgsql:Enum:public.marital_status_enum", "none,single,married,divorced,widowed,domestic_partner,separated,other")
                .Annotation("Npgsql:Enum:public.payroll_status_enum", "progress,closed,cancelled,completed")
                .Annotation("Npgsql:Enum:public.permission_type_enum", "read,create,update,delete")
                .Annotation("Npgsql:Enum:public.permit_application_status_enum", "pending,approved,rejected,cancelled")
                .Annotation("Npgsql:Enum:public.permit_application_type_enum", "vacation,medical_appointment,compensatory_time,paid_leave,unpaid_leave,special_leave,donated_vacations")
                .Annotation("Npgsql:Enum:public.role_type_enum", "administrator,supervisor,manager,operator")
                .Annotation("Npgsql:Enum:public.salary_type_enum", "fixed,variable,professional_services")
                .Annotation("Npgsql:Enum:public.user_status_enum", "active,inactive,locked")
                .Annotation("Npgsql:Enum:public.user_type_enum", "standard_user,employee_self_service")
                .Annotation("Npgsql:Enum:role_type_enum", "administrator,manager,operator,supervisor")
                .Annotation("Npgsql:Enum:salary_type_enum", "fixed,professional_services,variable")
                .Annotation("Npgsql:Enum:user_status_enum", "active,inactive,locked")
                .Annotation("Npgsql:Enum:user_type_enum", "employee_self_service,standard_user")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .OldAnnotation("Npgsql:Enum:catalog_type_enum", "banks,branches,departaments,document_types,exchange_rates,job_positions,work_areas")
                .OldAnnotation("Npgsql:Enum:collaborator_status_enum", "active,inactive,subsidy,suspended,terminated,testing_process,vacation")
                .OldAnnotation("Npgsql:Enum:currency_enum", "nio,usd")
                .OldAnnotation("Npgsql:Enum:deduction_type_enum", "advance_christmas_bonus,late_arrivals,loans,other_deductions")
                .OldAnnotation("Npgsql:Enum:gender_type_enum", "man,women")
                .OldAnnotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .OldAnnotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .OldAnnotation("Npgsql:Enum:permission_type_enum", "create,delete,read,update")
                .OldAnnotation("Npgsql:Enum:permit_application_status_enum", "approved,cancelled,pending,rejected")
                .OldAnnotation("Npgsql:Enum:permit_application_type_enum", "compensatory_time,donated_vacations,medical_appointment,paid_leave,special_leave,unpaid_leave,vacation")
                .OldAnnotation("Npgsql:Enum:public.catalog_type_enum", "branches,work_areas,job_positions,document_types,banks,exchange_rates,departaments")
                .OldAnnotation("Npgsql:Enum:public.collaborator_status_enum", "active,inactive,vacation,subsidy,suspended,terminated,testing_process")
                .OldAnnotation("Npgsql:Enum:public.currency_enum", "nio,usd")
                .OldAnnotation("Npgsql:Enum:public.deduction_type_enum", "loans,advance_christmas_bonus,late_arrivals,other_deductions")
                .OldAnnotation("Npgsql:Enum:public.gender_type_enum", "man,women")
                .OldAnnotation("Npgsql:Enum:public.identification_type_enum", "cedula,pasaporte,cedula_residencia")
                .OldAnnotation("Npgsql:Enum:public.marital_status_enum", "none,single,married,divorced,widowed,domestic_partner,separated,other")
                .OldAnnotation("Npgsql:Enum:public.permission_type_enum", "read,create,update,delete")
                .OldAnnotation("Npgsql:Enum:public.permit_application_status_enum", "pending,approved,rejected,cancelled")
                .OldAnnotation("Npgsql:Enum:public.permit_application_type_enum", "vacation,medical_appointment,compensatory_time,paid_leave,unpaid_leave,special_leave,donated_vacations")
                .OldAnnotation("Npgsql:Enum:public.role_type_enum", "administrator,supervisor,manager,operator")
                .OldAnnotation("Npgsql:Enum:public.salary_type_enum", "fixed,variable,professional_services")
                .OldAnnotation("Npgsql:Enum:public.user_status_enum", "active,inactive,locked")
                .OldAnnotation("Npgsql:Enum:public.user_type_enum", "standard_user,employee_self_service")
                .OldAnnotation("Npgsql:Enum:role_type_enum", "administrator,manager,operator,supervisor")
                .OldAnnotation("Npgsql:Enum:salary_type_enum", "fixed,professional_services,variable")
                .OldAnnotation("Npgsql:Enum:user_status_enum", "active,inactive,locked")
                .OldAnnotation("Npgsql:Enum:user_type_enum", "employee_self_service,standard_user")
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "payrolls",
                schema: "public",
                columns: table => new
                {
                    payroll_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_to_pay = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    Status = table.Column<PayrollStatus>(type: "payroll_status_enum", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payrolls", x => x.payroll_id);
                    table.ForeignKey(
                        name: "FK_payrolls_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "public",
                        principalTable: "companies",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkPositionHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollaboratorId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkPositionId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPositionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkPositionHistory_collaborators_CollaboratorId",
                        column: x => x.CollaboratorId,
                        principalSchema: "public",
                        principalTable: "collaborators",
                        principalColumn: "collaborator_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkPositionHistory_sub_catalogs_WorkPositionId",
                        column: x => x.WorkPositionId,
                        principalSchema: "public",
                        principalTable: "sub_catalogs",
                        principalColumn: "sub_catalog_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ordinary_payrolls",
                schema: "public",
                columns: table => new
                {
                    ordinary_payroll_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ir = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    inss = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    bonus = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    overtimes = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    deductions = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    gross_salary = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    vacations = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_to_pay = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_deductions = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    payroll_id = table.Column<Guid>(type: "uuid", nullable: false),
                    collaborator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordinary_payrolls", x => x.ordinary_payroll_id);
                    table.ForeignKey(
                        name: "FK_ordinary_payrolls_collaborators_collaborator_id",
                        column: x => x.collaborator_id,
                        principalSchema: "public",
                        principalTable: "collaborators",
                        principalColumn: "collaborator_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ordinary_payrolls_payrolls_payroll_id",
                        column: x => x.payroll_id,
                        principalSchema: "public",
                        principalTable: "payrolls",
                        principalColumn: "payroll_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ordinary_payroll_id",
                schema: "public",
                table: "ordinary_payrolls",
                column: "ordinary_payroll_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordinary_payrolls_collaborator_id",
                schema: "public",
                table: "ordinary_payrolls",
                column: "collaborator_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordinary_payrolls_payroll_id",
                schema: "public",
                table: "ordinary_payrolls",
                column: "payroll_id");

            migrationBuilder.CreateIndex(
                name: "ix_payroll_id",
                schema: "public",
                table: "payrolls",
                column: "payroll_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payrolls_CompanyId",
                schema: "public",
                table: "payrolls",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPositionHistory_CollaboratorId",
                schema: "public",
                table: "WorkPositionHistory",
                column: "CollaboratorId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPositionHistory_WorkPositionId",
                schema: "public",
                table: "WorkPositionHistory",
                column: "WorkPositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ordinary_payrolls",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WorkPositionHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "payrolls",
                schema: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:catalog_type_enum", "banks,branches,departaments,document_types,exchange_rates,job_positions,work_areas")
                .Annotation("Npgsql:Enum:collaborator_status_enum", "active,inactive,subsidy,suspended,terminated,testing_process,vacation")
                .Annotation("Npgsql:Enum:currency_enum", "nio,usd")
                .Annotation("Npgsql:Enum:deduction_type_enum", "advance_christmas_bonus,late_arrivals,loans,other_deductions")
                .Annotation("Npgsql:Enum:gender_type_enum", "man,women")
                .Annotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .Annotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .Annotation("Npgsql:Enum:permission_type_enum", "create,delete,read,update")
                .Annotation("Npgsql:Enum:permit_application_status_enum", "approved,cancelled,pending,rejected")
                .Annotation("Npgsql:Enum:permit_application_type_enum", "compensatory_time,donated_vacations,medical_appointment,paid_leave,special_leave,unpaid_leave,vacation")
                .Annotation("Npgsql:Enum:public.catalog_type_enum", "branches,work_areas,job_positions,document_types,banks,exchange_rates,departaments")
                .Annotation("Npgsql:Enum:public.collaborator_status_enum", "active,inactive,vacation,subsidy,suspended,terminated,testing_process")
                .Annotation("Npgsql:Enum:public.currency_enum", "nio,usd")
                .Annotation("Npgsql:Enum:public.deduction_type_enum", "loans,advance_christmas_bonus,late_arrivals,other_deductions")
                .Annotation("Npgsql:Enum:public.gender_type_enum", "man,women")
                .Annotation("Npgsql:Enum:public.identification_type_enum", "cedula,pasaporte,cedula_residencia")
                .Annotation("Npgsql:Enum:public.marital_status_enum", "none,single,married,divorced,widowed,domestic_partner,separated,other")
                .Annotation("Npgsql:Enum:public.permission_type_enum", "read,create,update,delete")
                .Annotation("Npgsql:Enum:public.permit_application_status_enum", "pending,approved,rejected,cancelled")
                .Annotation("Npgsql:Enum:public.permit_application_type_enum", "vacation,medical_appointment,compensatory_time,paid_leave,unpaid_leave,special_leave,donated_vacations")
                .Annotation("Npgsql:Enum:public.role_type_enum", "administrator,supervisor,manager,operator")
                .Annotation("Npgsql:Enum:public.salary_type_enum", "fixed,variable,professional_services")
                .Annotation("Npgsql:Enum:public.user_status_enum", "active,inactive,locked")
                .Annotation("Npgsql:Enum:public.user_type_enum", "standard_user,employee_self_service")
                .Annotation("Npgsql:Enum:role_type_enum", "administrator,manager,operator,supervisor")
                .Annotation("Npgsql:Enum:salary_type_enum", "fixed,professional_services,variable")
                .Annotation("Npgsql:Enum:user_status_enum", "active,inactive,locked")
                .Annotation("Npgsql:Enum:user_type_enum", "employee_self_service,standard_user")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .OldAnnotation("Npgsql:Enum:catalog_type_enum", "banks,branches,departaments,document_types,exchange_rates,job_positions,work_areas")
                .OldAnnotation("Npgsql:Enum:collaborator_status_enum", "active,inactive,subsidy,suspended,terminated,testing_process,vacation")
                .OldAnnotation("Npgsql:Enum:currency_enum", "nio,usd")
                .OldAnnotation("Npgsql:Enum:deduction_type_enum", "advance_christmas_bonus,late_arrivals,loans,other_deductions")
                .OldAnnotation("Npgsql:Enum:gender_type_enum", "man,women")
                .OldAnnotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .OldAnnotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .OldAnnotation("Npgsql:Enum:permission_type_enum", "create,delete,read,update")
                .OldAnnotation("Npgsql:Enum:permit_application_status_enum", "approved,cancelled,pending,rejected")
                .OldAnnotation("Npgsql:Enum:permit_application_type_enum", "compensatory_time,donated_vacations,medical_appointment,paid_leave,special_leave,unpaid_leave,vacation")
                .OldAnnotation("Npgsql:Enum:public.catalog_type_enum", "branches,work_areas,job_positions,document_types,banks,exchange_rates,departaments")
                .OldAnnotation("Npgsql:Enum:public.collaborator_status_enum", "active,inactive,vacation,subsidy,suspended,terminated,testing_process")
                .OldAnnotation("Npgsql:Enum:public.currency_enum", "nio,usd")
                .OldAnnotation("Npgsql:Enum:public.deduction_type_enum", "loans,advance_christmas_bonus,late_arrivals,other_deductions")
                .OldAnnotation("Npgsql:Enum:public.gender_type_enum", "man,women")
                .OldAnnotation("Npgsql:Enum:public.identification_type_enum", "cedula,pasaporte,cedula_residencia")
                .OldAnnotation("Npgsql:Enum:public.marital_status_enum", "none,single,married,divorced,widowed,domestic_partner,separated,other")
                .OldAnnotation("Npgsql:Enum:public.payroll_status_enum", "progress,closed,cancelled,completed")
                .OldAnnotation("Npgsql:Enum:public.permission_type_enum", "read,create,update,delete")
                .OldAnnotation("Npgsql:Enum:public.permit_application_status_enum", "pending,approved,rejected,cancelled")
                .OldAnnotation("Npgsql:Enum:public.permit_application_type_enum", "vacation,medical_appointment,compensatory_time,paid_leave,unpaid_leave,special_leave,donated_vacations")
                .OldAnnotation("Npgsql:Enum:public.role_type_enum", "administrator,supervisor,manager,operator")
                .OldAnnotation("Npgsql:Enum:public.salary_type_enum", "fixed,variable,professional_services")
                .OldAnnotation("Npgsql:Enum:public.user_status_enum", "active,inactive,locked")
                .OldAnnotation("Npgsql:Enum:public.user_type_enum", "standard_user,employee_self_service")
                .OldAnnotation("Npgsql:Enum:role_type_enum", "administrator,manager,operator,supervisor")
                .OldAnnotation("Npgsql:Enum:salary_type_enum", "fixed,professional_services,variable")
                .OldAnnotation("Npgsql:Enum:user_status_enum", "active,inactive,locked")
                .OldAnnotation("Npgsql:Enum:user_type_enum", "employee_self_service,standard_user")
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,");
        }
    }
}

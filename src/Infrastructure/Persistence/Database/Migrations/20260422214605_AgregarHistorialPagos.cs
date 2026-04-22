using System;
using ERP.Core.Database.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHistorialPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "vacations",
                schema: "public",
                table: "ordinary_payrolls",
                newName: "total_legal_deductions");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:catalog_type_enum", "banks,branches,departaments,document_types,exchange_rates,job_positions,work_areas")
                .Annotation("Npgsql:Enum:collaborator_status_enum", "active,inactive,subsidy,suspended,terminated,testing_process,vacation")
                .Annotation("Npgsql:Enum:currency_enum", "nio,usd")
                .Annotation("Npgsql:Enum:deduction_type_enum", "advance_christmas_bonus,late_arrivals,loans,other_deductions")
                .Annotation("Npgsql:Enum:gender_type_enum", "man,women")
                .Annotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .Annotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .Annotation("Npgsql:Enum:payroll_status_enum", "cancelled,closed,completed,progress")
                .Annotation("Npgsql:Enum:payroll_type_enum", "none,ordinary,professional_services,provided")
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
                .Annotation("Npgsql:Enum:public.payroll_type_enum", "none,ordinary,provided,professional_services")
                .Annotation("Npgsql:Enum:public.permission_type_enum", "read,create,update,delete")
                .Annotation("Npgsql:Enum:public.permit_application_status_enum", "pending,approved,rejected,cancelled")
                .Annotation("Npgsql:Enum:public.permit_application_type_enum", "vacation,medical_appointment,compensatory_time,paid_leave,unpaid_leave,special_leave,donated_vacations")
                .Annotation("Npgsql:Enum:public.role_type_enum", "administrator,supervisor,manager,operator")
                .Annotation("Npgsql:Enum:public.salary_type_enum", "fixed,variable,professional_services")
                .Annotation("Npgsql:Enum:public.source_deduction_payment_enum", "payroll,cash")
                .Annotation("Npgsql:Enum:public.tax_type_enum", "inss,inss_patronal")
                .Annotation("Npgsql:Enum:public.user_status_enum", "active,inactive,locked")
                .Annotation("Npgsql:Enum:public.user_type_enum", "standard_user,employee_self_service")
                .Annotation("Npgsql:Enum:role_type_enum", "administrator,manager,operator,supervisor")
                .Annotation("Npgsql:Enum:salary_type_enum", "fixed,professional_services,variable")
                .Annotation("Npgsql:Enum:tax_type_enum", "inss,inss_patronal")
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
                .OldAnnotation("Npgsql:Enum:payroll_status_enum", "cancelled,closed,completed,progress")
                .OldAnnotation("Npgsql:Enum:payroll_type_enum", "none,ordinary,professional_services,provided")
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
                .OldAnnotation("Npgsql:Enum:public.payroll_type_enum", "none,ordinary,provided,professional_services")
                .OldAnnotation("Npgsql:Enum:public.permission_type_enum", "read,create,update,delete")
                .OldAnnotation("Npgsql:Enum:public.permit_application_status_enum", "pending,approved,rejected,cancelled")
                .OldAnnotation("Npgsql:Enum:public.permit_application_type_enum", "vacation,medical_appointment,compensatory_time,paid_leave,unpaid_leave,special_leave,donated_vacations")
                .OldAnnotation("Npgsql:Enum:public.role_type_enum", "administrator,supervisor,manager,operator")
                .OldAnnotation("Npgsql:Enum:public.salary_type_enum", "fixed,variable,professional_services")
                .OldAnnotation("Npgsql:Enum:public.tax_type_enum", "inss,inss_patronal")
                .OldAnnotation("Npgsql:Enum:public.user_status_enum", "active,inactive,locked")
                .OldAnnotation("Npgsql:Enum:public.user_type_enum", "standard_user,employee_self_service")
                .OldAnnotation("Npgsql:Enum:role_type_enum", "administrator,manager,operator,supervisor")
                .OldAnnotation("Npgsql:Enum:salary_type_enum", "fixed,professional_services,variable")
                .OldAnnotation("Npgsql:Enum:tax_type_enum", "inss,inss_patronal")
                .OldAnnotation("Npgsql:Enum:user_status_enum", "active,inactive,locked")
                .OldAnnotation("Npgsql:Enum:user_type_enum", "employee_self_service,standard_user")
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.AddColumn<decimal>(
                name: "biweekly_salary",
                schema: "public",
                table: "ordinary_payrolls",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                schema: "public",
                table: "deductions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeductionStartDate",
                schema: "public",
                table: "deductions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "FortnightlyAmount",
                schema: "public",
                table: "deductions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfFortnights",
                schema: "public",
                table: "deductions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "deductions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                schema: "public",
                table: "deductions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBalance",
                schema: "public",
                table: "deductions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "deductions_payment_histories",
                schema: "public",
                columns: table => new
                {
                    payment_history_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    origin = table.Column<SourceDeductionPayment>(type: "source_deduction_payment_enum", nullable: false),
                    deduction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deductions_payment_histories", x => x.payment_history_id);
                    table.ForeignKey(
                        name: "FK_deductions_payment_histories_deductions_deduction_id",
                        column: x => x.deduction_id,
                        principalSchema: "public",
                        principalTable: "deductions",
                        principalColumn: "deduction_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deductions_payment_histories_deduction_id",
                schema: "public",
                table: "deductions_payment_histories",
                column: "deduction_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_id",
                schema: "public",
                table: "deductions_payment_histories",
                column: "payment_history_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deductions_payment_histories",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "biweekly_salary",
                schema: "public",
                table: "ordinary_payrolls");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                schema: "public",
                table: "deductions");

            migrationBuilder.DropColumn(
                name: "DeductionStartDate",
                schema: "public",
                table: "deductions");

            migrationBuilder.DropColumn(
                name: "FortnightlyAmount",
                schema: "public",
                table: "deductions");

            migrationBuilder.DropColumn(
                name: "NumberOfFortnights",
                schema: "public",
                table: "deductions");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "deductions");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                schema: "public",
                table: "deductions");

            migrationBuilder.DropColumn(
                name: "TotalBalance",
                schema: "public",
                table: "deductions");

            migrationBuilder.RenameColumn(
                name: "total_legal_deductions",
                schema: "public",
                table: "ordinary_payrolls",
                newName: "vacations");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:catalog_type_enum", "banks,branches,departaments,document_types,exchange_rates,job_positions,work_areas")
                .Annotation("Npgsql:Enum:collaborator_status_enum", "active,inactive,subsidy,suspended,terminated,testing_process,vacation")
                .Annotation("Npgsql:Enum:currency_enum", "nio,usd")
                .Annotation("Npgsql:Enum:deduction_type_enum", "advance_christmas_bonus,late_arrivals,loans,other_deductions")
                .Annotation("Npgsql:Enum:gender_type_enum", "man,women")
                .Annotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .Annotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .Annotation("Npgsql:Enum:payroll_status_enum", "cancelled,closed,completed,progress")
                .Annotation("Npgsql:Enum:payroll_type_enum", "none,ordinary,professional_services,provided")
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
                .Annotation("Npgsql:Enum:public.payroll_type_enum", "none,ordinary,provided,professional_services")
                .Annotation("Npgsql:Enum:public.permission_type_enum", "read,create,update,delete")
                .Annotation("Npgsql:Enum:public.permit_application_status_enum", "pending,approved,rejected,cancelled")
                .Annotation("Npgsql:Enum:public.permit_application_type_enum", "vacation,medical_appointment,compensatory_time,paid_leave,unpaid_leave,special_leave,donated_vacations")
                .Annotation("Npgsql:Enum:public.role_type_enum", "administrator,supervisor,manager,operator")
                .Annotation("Npgsql:Enum:public.salary_type_enum", "fixed,variable,professional_services")
                .Annotation("Npgsql:Enum:public.tax_type_enum", "inss,inss_patronal")
                .Annotation("Npgsql:Enum:public.user_status_enum", "active,inactive,locked")
                .Annotation("Npgsql:Enum:public.user_type_enum", "standard_user,employee_self_service")
                .Annotation("Npgsql:Enum:role_type_enum", "administrator,manager,operator,supervisor")
                .Annotation("Npgsql:Enum:salary_type_enum", "fixed,professional_services,variable")
                .Annotation("Npgsql:Enum:tax_type_enum", "inss,inss_patronal")
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
                .OldAnnotation("Npgsql:Enum:payroll_status_enum", "cancelled,closed,completed,progress")
                .OldAnnotation("Npgsql:Enum:payroll_type_enum", "none,ordinary,professional_services,provided")
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
                .OldAnnotation("Npgsql:Enum:public.payroll_type_enum", "none,ordinary,provided,professional_services")
                .OldAnnotation("Npgsql:Enum:public.permission_type_enum", "read,create,update,delete")
                .OldAnnotation("Npgsql:Enum:public.permit_application_status_enum", "pending,approved,rejected,cancelled")
                .OldAnnotation("Npgsql:Enum:public.permit_application_type_enum", "vacation,medical_appointment,compensatory_time,paid_leave,unpaid_leave,special_leave,donated_vacations")
                .OldAnnotation("Npgsql:Enum:public.role_type_enum", "administrator,supervisor,manager,operator")
                .OldAnnotation("Npgsql:Enum:public.salary_type_enum", "fixed,variable,professional_services")
                .OldAnnotation("Npgsql:Enum:public.source_deduction_payment_enum", "payroll,cash")
                .OldAnnotation("Npgsql:Enum:public.tax_type_enum", "inss,inss_patronal")
                .OldAnnotation("Npgsql:Enum:public.user_status_enum", "active,inactive,locked")
                .OldAnnotation("Npgsql:Enum:public.user_type_enum", "standard_user,employee_self_service")
                .OldAnnotation("Npgsql:Enum:role_type_enum", "administrator,manager,operator,supervisor")
                .OldAnnotation("Npgsql:Enum:salary_type_enum", "fixed,professional_services,variable")
                .OldAnnotation("Npgsql:Enum:tax_type_enum", "inss,inss_patronal")
                .OldAnnotation("Npgsql:Enum:user_status_enum", "active,inactive,locked")
                .OldAnnotation("Npgsql:Enum:user_type_enum", "employee_self_service,standard_user")
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,");
        }
    }
}

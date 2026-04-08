using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCampoParaSolicitarPermiso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:catalog_type_enum", "banks,branches,departaments,document_types,exchange_rates,job_positions,work_areas")
                .Annotation("Npgsql:Enum:collaborator_status_enum", "active,inactive,subsidy,suspended,terminated,testing_process,vacation")
                .Annotation("Npgsql:Enum:currency_enum", "nio,usd")
                .Annotation("Npgsql:Enum:gender_type_enum", "man,women")
                .Annotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .Annotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .Annotation("Npgsql:Enum:permission_type_enum", "create,delete,read,update")
                .Annotation("Npgsql:Enum:permit_application_status_enum", "approved,cancelled,pending,rejected")
                .Annotation("Npgsql:Enum:public.catalog_type_enum", "branches,work_areas,job_positions,document_types,banks,exchange_rates,departaments")
                .Annotation("Npgsql:Enum:public.collaborator_status_enum", "active,inactive,vacation,subsidy,suspended,terminated,testing_process")
                .Annotation("Npgsql:Enum:public.currency_enum", "nio,usd")
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
                .OldAnnotation("Npgsql:Enum:gender_type_enum", "man,women")
                .OldAnnotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .OldAnnotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .OldAnnotation("Npgsql:Enum:permission_type_enum", "create,delete,read,update")
                .OldAnnotation("Npgsql:Enum:permit_application_status_enum", "approved,cancelled,pending,rejected")
                .OldAnnotation("Npgsql:Enum:public.catalog_type_enum", "branches,work_areas,job_positions,document_types,banks,exchange_rates,departaments")
                .OldAnnotation("Npgsql:Enum:public.collaborator_status_enum", "active,inactive,vacation,subsidy,suspended,terminated,testing_process")
                .OldAnnotation("Npgsql:Enum:public.currency_enum", "nio,usd")
                .OldAnnotation("Npgsql:Enum:public.gender_type_enum", "man,women")
                .OldAnnotation("Npgsql:Enum:public.identification_type_enum", "cedula,pasaporte,cedula_residencia")
                .OldAnnotation("Npgsql:Enum:public.marital_status_enum", "none,single,married,divorced,widowed,domestic_partner,separated,other")
                .OldAnnotation("Npgsql:Enum:public.permission_type_enum", "read,create,update,delete")
                .OldAnnotation("Npgsql:Enum:public.permit_application_status_enum", "pending,approved,rejected,cancelled")
                .OldAnnotation("Npgsql:Enum:public.permit_application_type_enum", "vacation,medical_appointment,compensatory_time,paid_leave,unpaid_leave,special_leave")
                .OldAnnotation("Npgsql:Enum:public.role_type_enum", "administrator,supervisor,manager,operator")
                .OldAnnotation("Npgsql:Enum:public.salary_type_enum", "fixed,variable,professional_services")
                .OldAnnotation("Npgsql:Enum:public.user_status_enum", "active,inactive,locked")
                .OldAnnotation("Npgsql:Enum:public.user_type_enum", "standard_user,employee_self_service")
                .OldAnnotation("Npgsql:Enum:role_type_enum", "administrator,manager,operator,supervisor")
                .OldAnnotation("Npgsql:Enum:salary_type_enum", "fixed,professional_services,variable")
                .OldAnnotation("Npgsql:Enum:user_status_enum", "active,inactive,locked")
                .OldAnnotation("Npgsql:Enum:user_type_enum", "employee_self_service,standard_user")
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.AddColumn<string>(
                name: "identification_collaborator_to_receive",
                schema: "public",
                table: "permit_applications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "identification_collaborator_to_receive",
                schema: "public",
                table: "permit_applications");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:catalog_type_enum", "banks,branches,departaments,document_types,exchange_rates,job_positions,work_areas")
                .Annotation("Npgsql:Enum:collaborator_status_enum", "active,inactive,subsidy,suspended,terminated,testing_process,vacation")
                .Annotation("Npgsql:Enum:currency_enum", "nio,usd")
                .Annotation("Npgsql:Enum:gender_type_enum", "man,women")
                .Annotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .Annotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .Annotation("Npgsql:Enum:permission_type_enum", "create,delete,read,update")
                .Annotation("Npgsql:Enum:permit_application_status_enum", "approved,cancelled,pending,rejected")
                .Annotation("Npgsql:Enum:permit_application_type_enum", "compensatory_time,medical_appointment,paid_leave,special_leave,unpaid_leave,vacation")
                .Annotation("Npgsql:Enum:public.catalog_type_enum", "branches,work_areas,job_positions,document_types,banks,exchange_rates,departaments")
                .Annotation("Npgsql:Enum:public.collaborator_status_enum", "active,inactive,vacation,subsidy,suspended,terminated,testing_process")
                .Annotation("Npgsql:Enum:public.currency_enum", "nio,usd")
                .Annotation("Npgsql:Enum:public.gender_type_enum", "man,women")
                .Annotation("Npgsql:Enum:public.identification_type_enum", "cedula,pasaporte,cedula_residencia")
                .Annotation("Npgsql:Enum:public.marital_status_enum", "none,single,married,divorced,widowed,domestic_partner,separated,other")
                .Annotation("Npgsql:Enum:public.permission_type_enum", "read,create,update,delete")
                .Annotation("Npgsql:Enum:public.permit_application_status_enum", "pending,approved,rejected,cancelled")
                .Annotation("Npgsql:Enum:public.permit_application_type_enum", "vacation,medical_appointment,compensatory_time,paid_leave,unpaid_leave,special_leave")
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
                .OldAnnotation("Npgsql:Enum:gender_type_enum", "man,women")
                .OldAnnotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .OldAnnotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .OldAnnotation("Npgsql:Enum:permission_type_enum", "create,delete,read,update")
                .OldAnnotation("Npgsql:Enum:permit_application_status_enum", "approved,cancelled,pending,rejected")
                .OldAnnotation("Npgsql:Enum:permit_application_type_enum", "compensatory_time,donated_vacations,medical_appointment,paid_leave,special_leave,unpaid_leave,vacation")
                .OldAnnotation("Npgsql:Enum:public.catalog_type_enum", "branches,work_areas,job_positions,document_types,banks,exchange_rates,departaments")
                .OldAnnotation("Npgsql:Enum:public.collaborator_status_enum", "active,inactive,vacation,subsidy,suspended,terminated,testing_process")
                .OldAnnotation("Npgsql:Enum:public.currency_enum", "nio,usd")
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
        }
    }
}

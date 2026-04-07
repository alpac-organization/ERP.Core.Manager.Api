using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCatalogGlobales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_working_information_sub_catalogs_BranchId",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropForeignKey(
                name: "FK_working_information_sub_catalogs_WorkAreaId",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropForeignKey(
                name: "FK_working_information_sub_catalogs_WorkPositionId",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropColumn(
                name: "departament",
                schema: "public",
                table: "personal_informations");

            migrationBuilder.RenameColumn(
                name: "WorkPositionId",
                schema: "public",
                table: "working_information",
                newName: "work_position_id");

            migrationBuilder.RenameColumn(
                name: "WorkAreaId",
                schema: "public",
                table: "working_information",
                newName: "work_area_id");

            migrationBuilder.RenameColumn(
                name: "BranchId",
                schema: "public",
                table: "working_information",
                newName: "branch_id");

            migrationBuilder.RenameIndex(
                name: "IX_working_information_WorkPositionId",
                schema: "public",
                table: "working_information",
                newName: "IX_working_information_work_position_id");

            migrationBuilder.RenameIndex(
                name: "IX_working_information_WorkAreaId",
                schema: "public",
                table: "working_information",
                newName: "IX_working_information_work_area_id");

            migrationBuilder.RenameIndex(
                name: "IX_working_information_BranchId",
                schema: "public",
                table: "working_information",
                newName: "IX_working_information_branch_id");

            migrationBuilder.AlterDatabase()
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
                .OldAnnotation("Npgsql:Enum:collaborator_status_enum", "active,inactive,subsidy,suspended,terminated,testing_process,vacation")
                .OldAnnotation("Npgsql:Enum:currency_enum", "nio,usd")
                .OldAnnotation("Npgsql:Enum:gender_type_enum", "man,women")
                .OldAnnotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .OldAnnotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .OldAnnotation("Npgsql:Enum:permission_type_enum", "create,delete,read,update")
                .OldAnnotation("Npgsql:Enum:permit_application_status_enum", "approved,cancelled,pending,rejected")
                .OldAnnotation("Npgsql:Enum:permit_application_type_enum", "compensatory_time,medical_appointment,paid_leave,special_leave,unpaid_leave,vacation")
                .OldAnnotation("Npgsql:Enum:public.catalog_type_enum", "branches,work_areas,job_positions,document_types,banks,exchange_rates")
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

            migrationBuilder.AddColumn<int>(
                name: "departament_id",
                schema: "public",
                table: "personal_informations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_global",
                schema: "public",
                table: "catalogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_personal_informations_departament_id",
                schema: "public",
                table: "personal_informations",
                column: "departament_id");

            migrationBuilder.AddForeignKey(
                name: "FK_personal_informations_sub_catalogs_departament_id",
                schema: "public",
                table: "personal_informations",
                column: "departament_id",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_working_information_sub_catalogs_branch_id",
                schema: "public",
                table: "working_information",
                column: "branch_id",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_working_information_sub_catalogs_work_area_id",
                schema: "public",
                table: "working_information",
                column: "work_area_id",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_working_information_sub_catalogs_work_position_id",
                schema: "public",
                table: "working_information",
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
                name: "FK_personal_informations_sub_catalogs_departament_id",
                schema: "public",
                table: "personal_informations");

            migrationBuilder.DropForeignKey(
                name: "FK_working_information_sub_catalogs_branch_id",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropForeignKey(
                name: "FK_working_information_sub_catalogs_work_area_id",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropForeignKey(
                name: "FK_working_information_sub_catalogs_work_position_id",
                schema: "public",
                table: "working_information");

            migrationBuilder.DropIndex(
                name: "IX_personal_informations_departament_id",
                schema: "public",
                table: "personal_informations");

            migrationBuilder.DropColumn(
                name: "departament_id",
                schema: "public",
                table: "personal_informations");

            migrationBuilder.DropColumn(
                name: "is_global",
                schema: "public",
                table: "catalogs");

            migrationBuilder.RenameColumn(
                name: "work_position_id",
                schema: "public",
                table: "working_information",
                newName: "WorkPositionId");

            migrationBuilder.RenameColumn(
                name: "work_area_id",
                schema: "public",
                table: "working_information",
                newName: "WorkAreaId");

            migrationBuilder.RenameColumn(
                name: "branch_id",
                schema: "public",
                table: "working_information",
                newName: "BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_working_information_work_position_id",
                schema: "public",
                table: "working_information",
                newName: "IX_working_information_WorkPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_working_information_work_area_id",
                schema: "public",
                table: "working_information",
                newName: "IX_working_information_WorkAreaId");

            migrationBuilder.RenameIndex(
                name: "IX_working_information_branch_id",
                schema: "public",
                table: "working_information",
                newName: "IX_working_information_BranchId");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:collaborator_status_enum", "active,inactive,subsidy,suspended,terminated,testing_process,vacation")
                .Annotation("Npgsql:Enum:currency_enum", "nio,usd")
                .Annotation("Npgsql:Enum:gender_type_enum", "man,women")
                .Annotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .Annotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .Annotation("Npgsql:Enum:permission_type_enum", "create,delete,read,update")
                .Annotation("Npgsql:Enum:permit_application_status_enum", "approved,cancelled,pending,rejected")
                .Annotation("Npgsql:Enum:permit_application_type_enum", "compensatory_time,medical_appointment,paid_leave,special_leave,unpaid_leave,vacation")
                .Annotation("Npgsql:Enum:public.catalog_type_enum", "branches,work_areas,job_positions,document_types,banks,exchange_rates")
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
                .OldAnnotation("Npgsql:Enum:collaborator_status_enum", "active,inactive,subsidy,suspended,terminated,testing_process,vacation")
                .OldAnnotation("Npgsql:Enum:currency_enum", "nio,usd")
                .OldAnnotation("Npgsql:Enum:gender_type_enum", "man,women")
                .OldAnnotation("Npgsql:Enum:identification_type_enum", "cedula,cedula_residencia,pasaporte")
                .OldAnnotation("Npgsql:Enum:marital_status_enum", "divorced,domestic_partner,married,none,other,separated,single,widowed")
                .OldAnnotation("Npgsql:Enum:permission_type_enum", "create,delete,read,update")
                .OldAnnotation("Npgsql:Enum:permit_application_status_enum", "approved,cancelled,pending,rejected")
                .OldAnnotation("Npgsql:Enum:permit_application_type_enum", "compensatory_time,medical_appointment,paid_leave,special_leave,unpaid_leave,vacation")
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
                name: "departament",
                schema: "public",
                table: "personal_informations",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_working_information_sub_catalogs_BranchId",
                schema: "public",
                table: "working_information",
                column: "BranchId",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_working_information_sub_catalogs_WorkAreaId",
                schema: "public",
                table: "working_information",
                column: "WorkAreaId",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_working_information_sub_catalogs_WorkPositionId",
                schema: "public",
                table: "working_information",
                column: "WorkPositionId",
                principalSchema: "public",
                principalTable: "sub_catalogs",
                principalColumn: "sub_catalog_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class ModificarNombresColumnas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assigned_travel_expenses_types_income_TypeIncomeId",
                schema: "public",
                table: "assigned_travel_expenses");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TypeIncomeId",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "type_income_id");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "end_date");

            migrationBuilder.RenameIndex(
                name: "IX_assigned_travel_expenses_TypeIncomeId",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "IX_assigned_travel_expenses_type_income_id");

            migrationBuilder.AddForeignKey(
                name: "FK_assigned_travel_expenses_types_income_type_income_id",
                schema: "public",
                table: "assigned_travel_expenses",
                column: "type_income_id",
                principalSchema: "public",
                principalTable: "types_income",
                principalColumn: "validity_deduction_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assigned_travel_expenses_types_income_type_income_id",
                schema: "public",
                table: "assigned_travel_expenses");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "type_income_id",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "TypeIncomeId");

            migrationBuilder.RenameColumn(
                name: "start_date",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "end_date",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_assigned_travel_expenses_type_income_id",
                schema: "public",
                table: "assigned_travel_expenses",
                newName: "IX_assigned_travel_expenses_TypeIncomeId");

            migrationBuilder.AddForeignKey(
                name: "FK_assigned_travel_expenses_types_income_TypeIncomeId",
                schema: "public",
                table: "assigned_travel_expenses",
                column: "TypeIncomeId",
                principalSchema: "public",
                principalTable: "types_income",
                principalColumn: "validity_deduction_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

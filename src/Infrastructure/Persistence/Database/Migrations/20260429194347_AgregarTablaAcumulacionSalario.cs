using System;
using ERP.Core.Database.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaAcumulacionSalario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "total_to_pay",
                schema: "public",
                table: "payrolls");

            migrationBuilder.CreateTable(
                name: "assigned_travel_expenses",
                schema: "public",
                columns: table => new
                {
                    assigned_travel_expense_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    amount_in_dollars = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_in_local_currency = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<Currency>(type: "currency_enum", nullable: false),
                    collaborator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeIncomeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assigned_travel_expenses", x => x.assigned_travel_expense_id);
                    table.ForeignKey(
                        name: "FK_assigned_travel_expenses_collaborators_collaborator_id",
                        column: x => x.collaborator_id,
                        principalSchema: "public",
                        principalTable: "collaborators",
                        principalColumn: "collaborator_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assigned_travel_expenses_types_income_TypeIncomeId",
                        column: x => x.TypeIncomeId,
                        principalSchema: "public",
                        principalTable: "types_income",
                        principalColumn: "validity_deduction_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assigned_travel_expenses_histories",
                schema: "public",
                columns: table => new
                {
                    assigned_travel_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    payroll_id = table.Column<Guid>(type: "uuid", nullable: false),
                    collaborator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumberDaysPaid = table.Column<int>(type: "integer", nullable: false),
                    feeding = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    lodging = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    transport = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_amount_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assigned_travel_expenses_histories", x => x.assigned_travel_id);
                    table.ForeignKey(
                        name: "FK_assigned_travel_expenses_histories_collaborators_collaborat~",
                        column: x => x.collaborator_id,
                        principalSchema: "public",
                        principalTable: "collaborators",
                        principalColumn: "collaborator_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assigned_travel_expenses_histories_payrolls_payroll_id",
                        column: x => x.payroll_id,
                        principalSchema: "public",
                        principalTable: "payrolls",
                        principalColumn: "payroll_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "income_tax_accrual",
                schema: "public",
                columns: table => new
                {
                    income_tax_accrual_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    salary_earned = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false),
                    accumulated_ir = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false),
                    number_of_fortnights = table.Column<int>(type: "integer", nullable: false),
                    register_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payroll_id = table.Column<Guid>(name: "payroll_id ", type: "uuid", nullable: false),
                    collaborator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_income_tax_accrual", x => x.income_tax_accrual_id);
                    table.ForeignKey(
                        name: "FK_income_tax_accrual_collaborators_collaborator_id",
                        column: x => x.collaborator_id,
                        principalSchema: "public",
                        principalTable: "collaborators",
                        principalColumn: "collaborator_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_income_tax_accrual_payrolls_payroll_id ",
                        column: x => x.payroll_id,
                        principalSchema: "public",
                        principalTable: "payrolls",
                        principalColumn: "payroll_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assigned_travel_expenses_collaborator_id",
                schema: "public",
                table: "assigned_travel_expenses",
                column: "collaborator_id");

            migrationBuilder.CreateIndex(
                name: "IX_assigned_travel_expenses_TypeIncomeId",
                schema: "public",
                table: "assigned_travel_expenses",
                column: "TypeIncomeId");

            migrationBuilder.CreateIndex(
                name: "IX_assigned_travel_expenses_histories_collaborator_id",
                schema: "public",
                table: "assigned_travel_expenses_histories",
                column: "collaborator_id");

            migrationBuilder.CreateIndex(
                name: "IX_assigned_travel_expenses_histories_payroll_id",
                schema: "public",
                table: "assigned_travel_expenses_histories",
                column: "payroll_id");

            migrationBuilder.CreateIndex(
                name: "IX_income_tax_accrual_collaborator_id",
                schema: "public",
                table: "income_tax_accrual",
                column: "collaborator_id");

            migrationBuilder.CreateIndex(
                name: "IX_income_tax_accrual_payroll_id ",
                schema: "public",
                table: "income_tax_accrual",
                column: "payroll_id ");

            migrationBuilder.CreateIndex(
                name: "ix_income_tax_id",
                schema: "public",
                table: "income_tax_accrual",
                column: "income_tax_accrual_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assigned_travel_expenses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "assigned_travel_expenses_histories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "income_tax_accrual",
                schema: "public");

            migrationBuilder.AddColumn<decimal>(
                name: "total_to_pay",
                schema: "public",
                table: "payrolls",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

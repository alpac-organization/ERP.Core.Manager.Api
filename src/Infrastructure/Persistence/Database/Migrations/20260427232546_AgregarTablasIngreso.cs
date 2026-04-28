using System;
using ERP.Core.Database.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablasIngreso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "total_travel_expenses",
                schema: "public",
                table: "ordinary_payrolls",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PayrollId",
                schema: "public",
                table: "deductions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "types_income",
                schema: "public",
                columns: table => new
                {
                    validity_deduction_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    income_title = table.Column<string>(type: "text", nullable: false),
                    income_description = table.Column<string>(type: "text", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_types_income", x => x.validity_deduction_id);
                });

            migrationBuilder.CreateTable(
                name: "incomes",
                schema: "public",
                columns: table => new
                {
                    income_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    amount_in_local = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_in_dollars = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    currency = table.Column<Currency>(type: "currency_enum", nullable: false),
                    collaborator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    income_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payroll_id = table.Column<Guid>(name: "payroll_id ", type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incomes", x => x.income_id);
                    table.ForeignKey(
                        name: "FK_incomes_collaborators_collaborator_id",
                        column: x => x.collaborator_id,
                        principalSchema: "public",
                        principalTable: "collaborators",
                        principalColumn: "collaborator_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_incomes_payrolls_payroll_id ",
                        column: x => x.payroll_id,
                        principalSchema: "public",
                        principalTable: "payrolls",
                        principalColumn: "payroll_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_incomes_types_income_income_type_id",
                        column: x => x.income_type_id,
                        principalSchema: "public",
                        principalTable: "types_income",
                        principalColumn: "validity_deduction_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deductions_PayrollId",
                schema: "public",
                table: "deductions",
                column: "PayrollId");

            migrationBuilder.CreateIndex(
                name: "ix_income_id",
                schema: "public",
                table: "incomes",
                column: "income_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_incomes_collaborator_id",
                schema: "public",
                table: "incomes",
                column: "collaborator_id");

            migrationBuilder.CreateIndex(
                name: "IX_incomes_income_type_id",
                schema: "public",
                table: "incomes",
                column: "income_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_incomes_payroll_id ",
                schema: "public",
                table: "incomes",
                column: "payroll_id ");

            migrationBuilder.AddForeignKey(
                name: "FK_deductions_payrolls_PayrollId",
                schema: "public",
                table: "deductions",
                column: "PayrollId",
                principalSchema: "public",
                principalTable: "payrolls",
                principalColumn: "payroll_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deductions_payrolls_PayrollId",
                schema: "public",
                table: "deductions");

            migrationBuilder.DropTable(
                name: "incomes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "types_income",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_deductions_PayrollId",
                schema: "public",
                table: "deductions");

            migrationBuilder.DropColumn(
                name: "total_travel_expenses",
                schema: "public",
                table: "ordinary_payrolls");

            migrationBuilder.DropColumn(
                name: "PayrollId",
                schema: "public",
                table: "deductions");
        }
    }
}

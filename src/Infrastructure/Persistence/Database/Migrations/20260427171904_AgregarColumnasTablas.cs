using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarColumnasTablas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TravelExpenses",
                schema: "public",
                table: "ordinary_payrolls",
                newName: "travel_expenses");

            migrationBuilder.RenameColumn(
                name: "NumberOfOvetime",
                schema: "public",
                table: "ordinary_payrolls",
                newName: "number_of_overtime");

            migrationBuilder.AlterColumn<decimal>(
                name: "travel_expenses",
                schema: "public",
                table: "ordinary_payrolls",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<decimal>(
                name: "food_travel_allowance",
                schema: "public",
                table: "ordinary_payrolls",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "lodging",
                schema: "public",
                table: "ordinary_payrolls",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "food_travel_allowance",
                schema: "public",
                table: "ordinary_payrolls");

            migrationBuilder.DropColumn(
                name: "lodging",
                schema: "public",
                table: "ordinary_payrolls");

            migrationBuilder.RenameColumn(
                name: "travel_expenses",
                schema: "public",
                table: "ordinary_payrolls",
                newName: "TravelExpenses");

            migrationBuilder.RenameColumn(
                name: "number_of_overtime",
                schema: "public",
                table: "ordinary_payrolls",
                newName: "NumberOfOvetime");

            migrationBuilder.AlterColumn<decimal>(
                name: "TravelExpenses",
                schema: "public",
                table: "ordinary_payrolls",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}

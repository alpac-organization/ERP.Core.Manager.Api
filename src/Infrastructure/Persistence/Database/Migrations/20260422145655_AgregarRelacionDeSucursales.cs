using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRelacionDeSucursales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================
            // 1. ELIMINAR FK (SAFE)
            // =========================
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM pg_constraint 
                    WHERE conname = 'FK_working_information_sub_catalogs_branch_id'
                ) THEN
                    ALTER TABLE public.working_information
                    DROP CONSTRAINT ""FK_working_information_sub_catalogs_branch_id"";
                END IF;
            END $$;
            ");

            // =========================
            // 2. COMPANY_ID (SAFE RENAME)
            // =========================
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 
                    FROM information_schema.columns 
                    WHERE table_name='payrolls' 
                    AND column_name='company_id'
                ) THEN
                    ALTER TABLE public.payrolls 
                    RENAME COLUMN company_id TO ""CompanyId"";
                END IF;
            END $$;
            ");

            // índice rename seguro
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM pg_indexes 
                    WHERE indexname = 'IX_payrolls_company_id'
                ) THEN
                    ALTER INDEX ""IX_payrolls_company_id"" 
                    RENAME TO ""IX_payrolls_CompanyId"";
                END IF;
            END $$;
            ");

            // nullable safe
            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId",
                schema: "public",
                table: "payrolls",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            // =========================
            // 3. ADD company_branch_id (SAFE)
            // =========================
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 
                    FROM information_schema.columns 
                    WHERE table_name='payrolls' 
                    AND column_name='company_branch_id'
                ) THEN
                    ALTER TABLE public.payrolls 
                    ADD COLUMN company_branch_id uuid;
                END IF;
            END $$;
            ");

            // =========================
            // 4. INDEX (SAFE)
            // =========================
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_indexes 
                    WHERE indexname = 'IX_payrolls_company_branch_id'
                ) THEN
                    CREATE INDEX ""IX_payrolls_company_branch_id"" 
                    ON public.payrolls (company_branch_id);
                END IF;
            END $$;
            ");

            // =========================
            // 5. FK (SAFE)
            // =========================
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint 
                    WHERE conname = 'FK_payrolls_branches_company_branch_id'
                ) THEN
                    ALTER TABLE public.payrolls
                    ADD CONSTRAINT ""FK_payrolls_branches_company_branch_id""
                    FOREIGN KEY (company_branch_id)
                    REFERENCES public.branches(branch_id)
                    ON DELETE RESTRICT;
                END IF;
            END $$;
            ");

            // =========================
            // 6. working_information
            // =========================
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 
                    FROM information_schema.columns 
                    WHERE table_name='working_information' 
                    AND column_name='company_branch_id'
                ) THEN
                    ALTER TABLE public.working_information 
                    ADD COLUMN company_branch_id uuid;
                END IF;
            END $$;
            ");

            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_indexes 
                    WHERE indexname = 'IX_working_information_company_branch_id'
                ) THEN
                    CREATE INDEX ""IX_working_information_company_branch_id"" 
                    ON public.working_information (company_branch_id);
                END IF;
            END $$;
            ");

            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint 
                    WHERE conname = 'FK_working_information_branches_company_branch_id'
                ) THEN
                    ALTER TABLE public.working_information
                    ADD CONSTRAINT ""FK_working_information_branches_company_branch_id""
                    FOREIGN KEY (company_branch_id)
                    REFERENCES public.branches(branch_id)
                    ON DELETE RESTRICT;
                END IF;
            END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // eliminar FK nueva (safe)
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM pg_constraint 
                    WHERE conname = 'FK_payrolls_branches_company_branch_id'
                ) THEN
                    ALTER TABLE public.payrolls
                    DROP CONSTRAINT ""FK_payrolls_branches_company_branch_id"";
                END IF;
            END $$;
            ");

            // eliminar columna nueva (safe)
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 
                    FROM information_schema.columns 
                    WHERE table_name='payrolls' 
                    AND column_name='company_branch_id'
                ) THEN
                    ALTER TABLE public.payrolls 
                    DROP COLUMN company_branch_id;
                END IF;
            END $$;
            ");

            // working_information FK
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM pg_constraint 
                    WHERE conname = 'FK_working_information_branches_company_branch_id'
                ) THEN
                    ALTER TABLE public.working_information
                    DROP CONSTRAINT ""FK_working_information_branches_company_branch_id"";
                END IF;
            END $$;
            ");

            // working_information columna
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 
                    FROM information_schema.columns 
                    WHERE table_name='working_information' 
                    AND column_name='company_branch_id'
                ) THEN
                    ALTER TABLE public.working_information 
                    DROP COLUMN company_branch_id;
                END IF;
            END $$;
            ");
        }
    }
}

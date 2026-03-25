using Npgsql;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Database
{
    public static class NpgsqlConfiguration
    {
        public static NpgsqlDataSource BuildDataSource(string connectionString)
        {
            var builder = new NpgsqlDataSourceBuilder(connectionString);
            
            builder.EnableUnmappedTypes();

            // Centralizamos aquí todos los enums del ERP
            builder.MapEnum<CatalogType>("catalog_type");
            builder.MapEnum<UserStatus>("user_status");
            builder.MapEnum<RoleType>("role_type");
            builder.MapEnum<PermissionType>("permission_type");

            builder.MapEnum<UserStatus>("user_status_enum");
            builder.MapEnum<IdentificationType>("identification_type_enum");
            builder.MapEnum<GenderType>("gender_type_enum");
            builder.MapEnum<VacationRequestStatus>("vacation_request_status_enum");
            builder.MapEnum<SalaryType>("salary_type_enum");
            builder.MapEnum<Currency>("currency_enum");

            return builder.Build();
        }
    }
}
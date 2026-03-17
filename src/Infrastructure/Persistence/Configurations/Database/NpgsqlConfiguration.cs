using Npgsql;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Database
{
    public static class NpgsqlConfiguration
    {
        public static NpgsqlDataSource BuildDataSource(string connectionString)
        {
            var builder = new NpgsqlDataSourceBuilder(connectionString);

            // Centralizamos aquí todos los enums del ERP
            builder.MapEnum<CatalogType>("catalog_type");
            builder.MapEnum<UserStatus>("user_status");
            builder.MapEnum<RoleType>("role_type");
            builder.MapEnum<PermissionType>("permission_type");

            return builder.Build();
        }
    }
}
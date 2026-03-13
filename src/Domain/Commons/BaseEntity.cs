namespace ERP.Core.Manager.Api.Domain.Commons
{
    public abstract class BaseEntity
    {
        DateTime? DeletedAt { get; } 
        DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
}
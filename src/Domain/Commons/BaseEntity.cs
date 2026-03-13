namespace ERP.Core.Manager.Api.Domain.Commons
{
    public abstract class BaseEntity<TId> 
    {
        public TId? Id { get; set; }
        public DateTime? DeletedAt { get; } 
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
}
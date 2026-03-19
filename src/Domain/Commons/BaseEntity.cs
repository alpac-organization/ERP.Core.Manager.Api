namespace ERP.Core.Manager.Api.Domain.Commons
{
    public abstract class BaseEntity<T> 
    {
        public T? Id { get; set; }
        public DateTime? DeletedAt { get; set; } 
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
}
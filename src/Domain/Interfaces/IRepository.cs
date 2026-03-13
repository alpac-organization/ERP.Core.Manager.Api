namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken);
    }
}
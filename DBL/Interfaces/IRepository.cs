namespace DBL.Interfaces
{
    public interface IRepository<TEntity, TKey>
        where TEntity : class
    {
        Task<TEntity> GetItemAsync(TEntity item, bool throwIfNotFound = true);
        Task<IEnumerable<TEntity>> GetItemsAsync(int page = 0, int pageSize = 0);
        int PageCount(int pageSize = 0);
        Task<TEntity> AddItemAsync(TEntity item);
        Task<TEntity> UpdateAsync(TEntity item);
        Task DeleteAsync(TKey id);
        Task SaveChangesAsync();
    }
}

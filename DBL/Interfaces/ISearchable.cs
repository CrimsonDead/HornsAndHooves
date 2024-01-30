namespace DBL.Interfaces
{
    public interface ISearchable<TEntity, TKey>
        where TEntity : class
    {
        public Task<List<TEntity>> GetEntityByFilterAsync(TKey filter, int page = 0, int pageSize = 0);
    }
}

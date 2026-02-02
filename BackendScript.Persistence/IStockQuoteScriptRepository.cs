using BackendScripting.Model;

namespace BackendScript.Persistence;

public interface IStockQuoteScriptRepository
{
    Task<IEnumerable<StockQuoteScript>> GetAllAsync();
    Task<StockQuoteScript?> GetByIdAsync(int id, bool binary = false);
    Task<int?> AddAsync(StockQuoteScript stockQuoteScript);
    Task<int?> UpdateAsync(StockQuoteScript stockQuoteScript);
    Task<int> DeleteAsync(int id);
}
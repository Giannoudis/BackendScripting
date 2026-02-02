using BackendScripting.Model;

namespace BackendScript.Persistence;

public interface IStockQuoteResultRepository
{
    Task<IEnumerable<StockQuoteResult>> GetAllAsync(int stockQuoteScriptId);
    Task<StockQuoteResult?> GetByIdAsync(int id);
    Task<int> AddAsync(StockQuoteResult quoteResult);
    Task AddBulkAsync(IEnumerable<StockQuoteResult> results);
    Task<int> DeleteAsync(int id);
    Task<int> DeleteAllAsync(int stockQuoteScriptId);
}
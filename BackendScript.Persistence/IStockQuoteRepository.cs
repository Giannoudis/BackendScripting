using BackendScripting.Model;

namespace BackendScript.Persistence;

public interface IStockQuoteRepository
{
    Task<IEnumerable<StockQuote>> GetAllAsync();
    Task<StockQuote?> GetByIdAsync(int id);
    Task<int> AddAsync(StockQuote stockQuote);
    Task<int> DeleteAsync(int id);
}
using Dapper;
using BackendScripting.Model;

namespace BackendScript.Persistence;

public class StockQuoteRepository(DbContext dbContext) : IStockQuoteRepository
{
    private DbContext DbContext { get; } = dbContext;

    public async Task<IEnumerable<StockQuote>> GetAllAsync()
    {
        const string query = "SELECT Id, Symbol, OpenPrice, HighPrice, LowPrice, ClosePrice, Volume, MarketCap, Timestamp FROM StockQuote";
        using var connection = DbContext.Create();
        var quotes = await connection.QueryAsync<StockQuote>(query);
        return quotes;
    }

    public async Task<StockQuote?> GetByIdAsync(int id)
    {
        const string query = "SELECT Id, Symbol, OpenPrice, HighPrice, LowPrice, ClosePrice, Volume, MarketCap, Timestamp FROM StockQuote WHERE Id=@Id";
        using var connection = DbContext.Create();
        var quote = await connection.QuerySingleOrDefaultAsync<StockQuote>(
            query,
            new { ReadingId = id });
        return quote;
    }

    public async Task<int> AddAsync(StockQuote stockQuote)
    {
        const string sql = "INSERT INTO StockQuote(Symbol, OpenPrice, HighPrice, LowPrice, ClosePrice, Volume, MarketCap, Timestamp)" +
                           " VALUES(@Symbol, @OpenPrice, @HighPrice, @LowPrice, @ClosePrice, @Volume, @MarketCap, @Timestamp);"+
                           "SELECT CAST(SCOPE_IDENTITY() as int)";

        using var connection = DbContext.Create();
        var result = await connection.ExecuteScalarAsync<int>(sql, stockQuote);
        return result;
    }

    public async Task<int> DeleteAsync(int id)
    {
        const string sql = "DELETE StockQuote WHERE Id=@Id";
        using var connection = DbContext.Create();
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result;
    }
}
using System.Data;
using Dapper;
using BackendScripting.Model;
using Microsoft.Data.SqlClient;

namespace BackendScript.Persistence;

public class StockQuoteResultRepository(DbContext dbContext) : IStockQuoteResultRepository
{
    private DbContext DbContext { get; } = dbContext;

    public async Task<IEnumerable<StockQuoteResult>> GetAllAsync(int stockQuoteScriptId)
    {
        const string query = "SELECT Id, StockQuoteId, Result FROM StockQuoteResult WHERE StockQuoteScriptId=@StockQuoteScriptId";
        using var connection = DbContext.Create();
        var quotes = await connection.QueryAsync<StockQuoteResult>(query, new { StockQuoteScriptId = stockQuoteScriptId });
        return quotes;
    }

    public async Task<StockQuoteResult?> GetByIdAsync(int id)
    {
        const string query = "SELECT Id, StockQuoteId, Result FROM StockQuoteResult WHERE Id=@Id";
        using var connection = DbContext.Create();
        var quote = await connection.QuerySingleOrDefaultAsync<StockQuoteResult>(query, new { ReadingId = id });
        return quote;
    }

    public async Task<int> AddAsync(StockQuoteResult quoteResult)
    {
        const string sql = "INSERT INTO StockQuoteResult(StockQuoteId, StockQuoteScriptId, Result)" +
                           " VALUES(@StockQuoteId, @StockQuoteScriptId, @Result);" +
                           " SELECT CAST(SCOPE_IDENTITY() as int)";

        using var connection = DbContext.Create();
        var result = await connection.ExecuteScalarAsync<int>(sql, quoteResult);
        return result;
    }

    /// <summary>
    /// Add results in bulk mode.
    /// </summary>
    /// <remarks>Bulk insert is using ADo.NET: Dapper Plus is a commercial product</remarks>
    public async Task AddBulkAsync(IEnumerable<StockQuoteResult> results)
    {
        // data table
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("StockQuoteId", typeof(int));
        table.Columns.Add("StockQuoteScriptId", typeof(int));
        table.Columns.Add("Result", typeof(string));

        // object to data rows
        foreach (var result in results)
        {
            table.Rows.Add(0, result.StockQuoteId, result.StockQuoteScriptId, result.Result);
        }

        // connection
        await using var connection = new SqlConnection(DbContext.ConnectionString);
        await connection.OpenAsync();

        // bulk insert
        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = "StockQuoteResult";
        await bulkCopy.WriteToServerAsync(table);
    }

    public async Task<int> DeleteAsync(int id)
    {
        const string sql = "DELETE StockQuoteResult WHERE Id=@Id";
        using var connection = DbContext.Create();
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result;
    }

    public async Task<int> DeleteAllAsync(int stockQuoteScriptId)
    {
        const string sql = "DELETE StockQuoteResult WHERE StockQuoteScriptId=@StockQuoteScriptId";
        using var connection = DbContext.Create();
        var result = await connection.ExecuteAsync(sql, new { StockQuoteScriptId = stockQuoteScriptId });
        return result;
    }
}
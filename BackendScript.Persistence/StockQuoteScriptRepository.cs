using Dapper;
using Serilog;
using BackendScripting.Model;
using BackendScripting.Scripting;

namespace BackendScript.Persistence;

public class StockQuoteScriptRepository(DbContext dbContext) : IStockQuoteScriptRepository
{
    private DbContext DbContext { get; } = dbContext;

    public async Task<IEnumerable<StockQuoteScript>> GetAllAsync()
    {
        // query without binary
        var query = "SELECT Id, Name, Script FROM StockQuoteScript";
        using var connection = DbContext.Create();
        var quotes = await connection.QueryAsync<StockQuoteScript>(query);
        return quotes;
    }

    public async Task<StockQuoteScript?> GetByIdAsync(int id, bool binary = false)
    {
        var query = binary ?
            "SELECT Id, Name, Script, Binary, ScriptHash FROM StockQuoteScript WHERE Id=@Id" :
            "SELECT Id, Name, Script FROM StockQuoteScript WHERE Id=@Id";
        using var connection = DbContext.Create();
        var quote = await connection.QuerySingleOrDefaultAsync<StockQuoteScript>(query, new { Id = id });
        return quote;
    }

    public async Task<int?> AddAsync(StockQuoteScript stockQuoteScript)
    {
        const string sql = "INSERT INTO StockQuoteScript(Name, Script, Binary, ScriptHash)" +
                           " VALUES(@Name, @Script, @Binary, @ScriptHash);" +
                           " SELECT CAST(SCOPE_IDENTITY() as int)";

        try
        {
            // script compile: setup binary and script hash
            StockQuoteCompiler.Compile(stockQuoteScript);
        }
        catch (ScriptCompileException exception)
        {
            Log.Error(exception, exception.GetBaseException().Message);
            return null;
        }

        // create quote stock script
        using var connection = DbContext.Create();
        var result = await connection.ExecuteScalarAsync<int>(sql, stockQuoteScript);
        return result;
    }

    public async Task<int?> UpdateAsync(StockQuoteScript stockQuoteScript)
    {
        const string sql = "UPDATE StockQuoteScript" +
                           " SET Name=@Name," +
                           "     Script=@Script," +
                           "     Binary=@Binary," +
                           "     ScriptHash=@ScriptHash" +
                           " WHERE Id=@Id";

        try
        {
            // script compile: setup binary and script hash
            StockQuoteCompiler.Compile(stockQuoteScript);
        }
        catch (ScriptCompileException exception)
        {
            Log.Error(exception, exception.GetBaseException().Message);
            return null;
        }

        // invalidate assembly
        AssemblyFactory.InvalidateObjectAssembly(typeof(StockQuoteScript), stockQuoteScript);

        // update stock quote script
        using var connection = DbContext.Create();
        var result = await connection.ExecuteAsync(sql, stockQuoteScript);
        return result;
    }

    public async Task<int> DeleteAsync(int id)
    {
        const string sql = "DELETE StockQuoteScript WHERE Id=@Id";
        using var connection = DbContext.Create();
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result;
    }
}
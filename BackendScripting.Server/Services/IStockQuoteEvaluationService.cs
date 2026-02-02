using BackendScripting.Model;

namespace BackendScripting.Server.Services;

public interface IStockQuoteEvaluationService
{
    public Task<Dictionary<StockQuote, object>> EvaluateAsync(IScriptObject scriptObject);
}
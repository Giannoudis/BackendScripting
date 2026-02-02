using System.Text.Json;
using System.Diagnostics;
using Serilog;
using BackendScripting.Model;
using BackendScript.Persistence;
using BackendScripting.Scripting;

namespace BackendScripting.Server.Services;

public class StockQuoteEvaluationService(IConfiguration configuration,
    IStockQuoteRepository quoteRepository,
    IStockQuoteResultRepository resultRepository) : IStockQuoteEvaluationService
{
    private IConfiguration Configuration { get; } = configuration;
    private IStockQuoteRepository QuoteRepository { get; } = quoteRepository;
    private IStockQuoteResultRepository ResultRepository { get; } = resultRepository;

    public async Task<Dictionary<StockQuote, object>> EvaluateAsync(IScriptObject script)
    {
        if (script == null)
        {
            throw new ArgumentNullException(nameof(script));
        }
        if (script.Binary.Length == 0 || script.ScriptHash == 0)
        {
            throw new ScriptException($"Invalid binary in script {script.Id}.");
        }

        var values = new Dictionary<StockQuote, object>();
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        // load quotes
        var quotes = (await QuoteRepository.GetAllAsync()).ToList();
        if (!quotes.Any())
        {
            return new();
        }
        stopWatch.Stop();
        Log.Information($"Loaded {quotes.Count} quotes [{stopWatch.ElapsedMilliseconds} ms]");

        // evaluate values
        var timeout = GetTimeout();
        stopWatch.Restart();
        foreach (var quote in quotes)
        {
            var runtime = new StockQuoteRuntime(quote, script, timeout);
            var result = runtime.Evaluate();
            if (result != null)
            {
                values.Add(quote, result);
            }
        }
        stopWatch.Stop();
        Log.Information($"Evaluated {values.Count} values [{stopWatch.ElapsedMilliseconds} ms]");

        // save results
        stopWatch.Restart();
        // convert values to results
        var results = new List<StockQuoteResult>();
        foreach (var value in values)
        {
            var result = new StockQuoteResult
            {
                StockQuoteId = value.Key.Id,
                StockQuoteScriptId = script.Id,
                Result = JsonSerializer.Serialize(value.Value)

            };
            results.Add(result);
        }
        // bulk insert
        await ResultRepository.AddBulkAsync(results);
        stopWatch.Stop();
        Log.Information($"Stored {values.Count} values [{stopWatch.ElapsedMilliseconds} ms]");

        return values;
    }

    private TimeSpan GetTimeout()
    {
        var config = Configuration["ScriptFunctionTimeout"];
        if (TimeSpan.TryParse(config, out var timeout))
        {
            return timeout;
        }
        return TimeSpan.FromMilliseconds(Specification.ScriptFunctionTimeout);
    }
}
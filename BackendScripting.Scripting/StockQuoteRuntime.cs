using System.Runtime.CompilerServices;
using BackendScripting.Model;

namespace BackendScripting.Scripting;

/// <summary>
/// Stock quote runtime
/// </summary>
/// <exclude />
public class StockQuoteRuntime(StockQuote stockQuote, IScriptObject script, TimeSpan timeout)
{
    private StockQuote StockQuote { get; } = stockQuote;
    private IScriptObject Script { get; } = script;
    private TimeSpan Timeout { get; } = timeout;

    #region Function Properties

    public string Symbol => StockQuote.Symbol;
    public decimal OpenPrice => StockQuote.OpenPrice;
    public decimal HighPrice => StockQuote.HighPrice;
    public decimal LowPrice => StockQuote.LowPrice;
    public decimal ClosePrice => StockQuote.ClosePrice;
    public long Volume => StockQuote.Volume;
    public double MarketCap => StockQuote.MarketCap;
    public DateTime Timestamp => StockQuote.Timestamp;

    #endregion

    #region Function Methods

    /// <summary>
    /// Get stock quote yearly average price (no implementation)
    /// Example on hot to provide backend services and repositories
    /// </summary>
    /// <returns>Random value</returns>
    public decimal GetYearAveragePrice() =>
        (decimal)Random.Shared.NextDouble() * (HighPrice - LowPrice) + LowPrice;

    #endregion

    public object? Evaluate() => InvokeScript();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private object? InvokeScript()
    {
        try
        {
            var task = Task.Factory.StartNew<decimal?>(() =>
            {
                // create script
                using var script = CreateScript<StockQuoteFunction>(Script);
                // dynamic
                var value = script.Evaluate();
                return value;
            });
            return task.WaitScriptResult(typeof(StockQuoteFunction), Timeout);
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Evaluation error in stock quote {StockQuote.Id}: " +
                                      $"{exception.GetBaseException().Message}.", exception);
        }
    }

    /// <summary>
    /// Create a new script instance
    /// </summary>
    /// <param name="scriptObject">The script object</param>
    /// <returns>New instance of the scripting type</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private dynamic CreateScript<TFunc>(IScriptObject scriptObject)
    {
        // load assembly
        var scriptType = typeof(TFunc);
        var assembly = AssemblyFactory.GetObjectAssembly(typeof(IScriptObject), scriptObject);
        var assemblyScriptType = assembly.GetType(scriptType.FullName ?? throw new InvalidOperationException());
        if (assemblyScriptType == null)
        {
            throw new ScriptException($"Unknown script type {scriptType}.");
        }

        // script function execution with runtime as constructor argument
        return Activator.CreateInstance(assemblyScriptType, this)!;
    }

}
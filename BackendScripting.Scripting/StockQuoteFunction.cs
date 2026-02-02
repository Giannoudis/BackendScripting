/* StockQuoteFunction */
// ReSharper disable RedundantUsingDirective
using System;
namespace BackendScripting.Scripting;

/// <summary>
/// Stock quote function
/// </summary>
public class StockQuoteFunction : IDisposable
{
    /// <summary>The function runtime</summary>
    private dynamic Runtime { get; }

    /// <summary>New function instance</summary>
    /// <param name="runtime">The function runtime</param>
    /// <exclude />
    public StockQuoteFunction(object runtime)
    {
        Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    #region Properties

    /// <summary>Stock quote symbol</summary>
    /// <remarks>Access to runtime property</remarks>
    public string Symbol => Runtime.Symbol;

    /// <summary>Stock quote open price</summary>
    /// <remarks>Access to runtime property</remarks>
    public decimal OpenPrice => Runtime.OpenPrice;

    /// <summary>Stock quote high price</summary>
    /// <remarks>Access to runtime property</remarks>
    public decimal HighPrice => Runtime.HighPrice;

    /// <summary>Stock quote low price</summary>
    /// <remarks>Access to runtime property</remarks>
    public decimal LowPrice => Runtime.LowPrice;

    /// <summary>Stock quote close price</summary>
    /// <remarks>Access to runtime property</remarks>
    public decimal ClosePrice => Runtime.ClosePrice;

    /// <summary>Stock quote volume</summary>
    /// <remarks>Access to runtime property</remarks>
    public long Volume => Runtime.Volume;

    /// <summary>Stock quote market cap</summary>
    /// <remarks>Access to runtime property</remarks>
    public double MarketCap => Runtime.MarketCap;

    /// <summary>Stock quote timestamp</summary>
    /// <remarks>Access to runtime property</remarks>
    public DateTime Timestamp => Runtime.Timestamp;

    /// <summary>Stock quote price range</summary>
    /// <remarks>Local property</remarks>
    public decimal PriceRange => HighPrice - LowPrice;

    #endregion

    #region Methods

    /// <summary>
    /// Get stock quote yearly average value (no implementation)
    /// Example on hot to access backend services and repositories
    /// </summary>
    /// <returns>Random value</returns>
    public decimal GetYearAveragePrice() => Runtime.GetYearAveragePrice();

    #endregion

    /// <summary>Evaluate stock quote</summary>
    /// <remarks>Internal usage only, do not call this method</remarks>
    /// <exclude />
    public object? Evaluate()
    {
        // ReSharper disable once EmptyRegion
        // script placeholder
        #region Script
        #endregion

        // compiler will optimize this out if the code provides a return
        return null;
    }

    /// <summary>Dispose the function</summary>
    /// <exclude />
    public void Dispose() =>
        GC.SuppressFinalize(this);
}
// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace BackendScripting.Model;

public class StockQuote
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;

    public decimal OpenPrice { get; set; }
    public decimal HighPrice { get; set; }
    public decimal LowPrice { get; set; }
    public decimal ClosePrice { get; set; }
    public long Volume { get; set; }
    public double MarketCap { get; set; }
    public DateTime Timestamp { get; set; }
}

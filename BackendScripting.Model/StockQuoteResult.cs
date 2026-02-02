namespace BackendScripting.Model;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class StockQuoteResult
{
    public int Id { get; set; }
    public int StockQuoteId { get; set; }
    public int StockQuoteScriptId { get; set; }
    public required string Result { get; set; }
}
namespace BackendScripting.Server.Dtos;

public sealed record CreateStockQuoteRequest(
    string Symbol,
    decimal OpenPrice,
    decimal HighPrice,
    decimal LowPrice,
    decimal ClosePrice,
    long Volume,
    double MarketCap,
    DateTime Timestamp);
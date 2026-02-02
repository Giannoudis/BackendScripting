namespace BackendScripting.Server.Dtos;

public sealed record CreateStockQuoteScriptRequest(
    string Name,
    string Script);
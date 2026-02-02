namespace BackendScripting.Server.Dtos;

public sealed record UpdateStockQuoteScriptRequest(
    string Name,
    string Script);
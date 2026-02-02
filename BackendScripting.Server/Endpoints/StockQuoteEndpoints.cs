using Microsoft.AspNetCore.Mvc;
using BackendScript.Persistence;
using BackendScripting.Server.Dtos;
using BackendScripting.Server.Services;

namespace BackendScripting.Server.Endpoints;

/// <summary>
/// Stock quote CRUD operations
/// </summary>
/// <remarks>Demo code without error handling</remarks>
public static class StockQuoteEndpoints
{
    public static void MapStockQuoteEndpoints(this IEndpointRouteBuilder app)
    {
        // get collection
        app.MapGet("quotes", async (IStockQuoteRepository repository) =>
        {
            var quotes = await repository.GetAllAsync();
            return Results.Ok(quotes);
        })
        .WithName("GetStockQuotes");

        // get stock quote
        app.MapGet("quotes/{quoteId:int}", async (int quoteId, IStockQuoteRepository repository) =>
        {
            var quote = await repository.GetByIdAsync(quoteId);
            return Results.Ok(quote);
        })
        .WithName("GetStockQuote");

        // create stock quote
        app.MapPost("quotes", async (CreateStockQuoteRequest request, IStockQuoteRepository repository) =>
            {
                var result = await repository.AddAsync(
                    new()
                    {
                        Symbol = request.Symbol,
                        OpenPrice = request.OpenPrice,
                        HighPrice = request.HighPrice,
                        LowPrice = request.LowPrice,
                        ClosePrice = request.ClosePrice,
                        Volume = request.Volume,
                        MarketCap = request.MarketCap,
                        Timestamp = request.Timestamp
                    });
                return result == 0 ? Results.UnprocessableEntity() : Results.Created();
            })
            .WithName("CreateStockQuote");

        // create stock quote test data
        app.MapPost("quotes/mock", async (IStockQuoteMockService mockService,
                [FromHeader(Name = "Count")] int? count) =>
            {
                await mockService.Generate(count ?? 2000);
                return Results.Created();
            })
            .WithName("MockStockQuote");

        // delete stock quote
        app.MapDelete("quotes/{quoteId:int}", async (int quoteId, IStockQuoteRepository repository) =>
            {
                var result = await repository.DeleteAsync(quoteId);
                return result == 0 ? Results.NotFound() : Results.NoContent();
            })
            .WithName("DeleteStockQuote");
    }
}
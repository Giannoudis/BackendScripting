using BackendScript.Persistence;

namespace BackendScripting.Server.Endpoints;

/// <summary>
/// Stock quote script result CRUD operations
/// </summary>
/// <remarks>Demo code without error handling</remarks>
public static class StockQuoteResultEndpoints
{
    public static void MapStockQuoteResultEndpoints(this IEndpointRouteBuilder app)
    {
        // get quote script result collection
        app.MapGet("quotes/{quoteId:int}/scripts/{scriptId:int}", async
                (int scriptId, IStockQuoteResultRepository repository) =>
        {
            var quoteScripts = await repository.GetAllAsync(scriptId);
            return Results.Ok(quoteScripts);
        })
        .WithName("GetStockQuoteResults");

        // get stock quote script result
        app.MapGet("quotes/{quoteId:int}/scripts/{scriptId:int}/results/{resultId:int}", async
                (int resultId, IStockQuoteResultRepository repository) =>
        {
            var quote = await repository.GetByIdAsync(resultId);
            return Results.Ok(quote);
        })
        .WithName("GetStockQuoteResult");

        // delete stock quote result
        app.MapDelete("quotes/{quoteId:int}/scripts/{resultId:int}", async (int resultId, IStockQuoteResultRepository repository) =>
            {
                var result = await repository.DeleteAsync(resultId);
                return result == 0 ? Results.NotFound() : Results.NoContent();
            })
            .WithName("DeleteStockQuoteResult");

        // delete all stock quote result
        app.MapDelete("quotes/{quoteId:int}/scripts", async (int quoteId, IStockQuoteResultRepository repository) =>
            {
                var result = await repository.DeleteAllAsync(quoteId);
                return result == 0 ? Results.NotFound() : Results.NoContent();
            })
            .WithName("DeleteStockQuoteResults");
    }
}
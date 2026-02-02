using BackendScript.Persistence;
using BackendScripting.Server.Dtos;
using BackendScripting.Server.Services;

namespace BackendScripting.Server.Endpoints;

/// <summary>
/// Stock quote script CRUD operations
/// </summary>
/// <remarks>Demo code without error handling</remarks>
public static class StockQuoteScriptEndpoints
{
    public static void MapStockQuoteScriptEndpoints(this IEndpointRouteBuilder app)
    {
        // get quote script collection
        app.MapGet("quotes/scripts", async (IStockQuoteScriptRepository repository) =>
        {
            var quoteScripts = await repository.GetAllAsync();
            return Results.Ok(quoteScripts);
        })
        .WithName("GetStockQuoteScripts");

        // get stock quote script
        app.MapGet("quotes/scripts/{scriptId:int}", async (int scriptId, IStockQuoteScriptRepository repository) =>
        {
            var quoteScript = await repository.GetByIdAsync(scriptId);
            return Results.Ok(quoteScript);
        })
        .WithName("GetStockQuoteScript");

        // create stock quote script
        app.MapPost("quotes/scripts", async (CreateStockQuoteScriptRequest request,
                IStockQuoteScriptRepository repository) =>
            {
                var result = await repository.AddAsync(
                    new()
                    {
                        Name = request.Name,
                        Script = request.Script,
                        Binary = null!,
                        ScriptHash = 0
                    });
                return result == null || result == 0 ? Results.UnprocessableEntity() : Results.Created();
            })
            .WithName("CreateStockQuoteScript");

        // update stock quote script
        app.MapPut("quotes/scripts/{scriptId:int}", async (int scriptId,
                UpdateStockQuoteScriptRequest request, IStockQuoteScriptRepository repository) =>
            {
                var result = await repository.UpdateAsync(
                    new()
                    {
                        Id = scriptId,
                        Name = request.Name,
                        Script = request.Script,
                        Binary = null!,
                        ScriptHash = 0
                    });
                return result == 0 ? Results.NotFound() : Results.NoContent();
            })
            .WithName("UpdateStockQuoteScript");

        // delete stock quote script
        app.MapDelete("quotes/{quoteId:int}/scripts/{scriptId:int}", async (int scriptId, IStockQuoteScriptRepository repository) =>
            {
                var result = await repository.DeleteAsync(scriptId);
                return result == 0 ? Results.NotFound() : Results.NoContent();
            })
            .WithName("DeleteStockQuoteScript");

        // evaluate script
        app.MapPost("quotes/scripts/{scriptId:int}/evaluate", async (int scriptId,
                IStockQuoteScriptRepository scriptRepository,
                IStockQuoteEvaluationService evaluationService) =>
            {
                // script
                var script = await scriptRepository.GetByIdAsync(scriptId, binary: true);
                if (script == null)
                {
                    return Results.NotFound();
                }

                // evaluation
                var values = await evaluationService.EvaluateAsync(script);
                return !values.Any() ? Results.NotFound() : Results.Ok(values.Count);
            })
            .WithName("EvaluateStockQuoteScript");
    }
}
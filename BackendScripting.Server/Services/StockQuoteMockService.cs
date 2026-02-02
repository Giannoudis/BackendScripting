using System.Diagnostics;
using Bogus;
using Serilog;
using BackendScripting.Model;
using BackendScript.Persistence;

namespace BackendScripting.Server.Services;

public class StockQuoteMockService(IStockQuoteRepository quoteRepository) : IStockQuoteMockService
{
    private IStockQuoteRepository QuoteRepository { get; } = quoteRepository;

    public async Task Generate(int count)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var faker = new Faker<StockQuote>()
            .RuleFor(s => s.Id, f => f.IndexFaker + 1)
            .RuleFor(s => s.Symbol, f => f.Random.Replace("???").ToUpper())
            .RuleFor(s => s.OpenPrice, f => f.Finance.Amount(10, 500))
            .RuleFor(s => s.HighPrice, (f, s) => s.OpenPrice + f.Finance.Amount(0, 50))
            .RuleFor(s => s.LowPrice, (f, s) => s.OpenPrice - f.Finance.Amount(0, 10))
            .RuleFor(s => s.ClosePrice, (f, s) => f.Finance.Amount(s.LowPrice, s.HighPrice))
            .RuleFor(s => s.Volume, f => f.Random.Long(1000, 1000000))
            .RuleFor(s => s.MarketCap, f => Math.Round(f.Random.Double(1_000_000, 1_000_000_000), 2))
            .RuleFor(s => s.Timestamp, f => DateTime.UtcNow.AddMilliseconds(f.IndexFaker * 10));
        var quotes = faker.Generate(count);
        foreach (var quote in quotes)
        {
            await QuoteRepository.AddAsync(quote);
        }
        stopWatch.Stop();
        Log.Information($"Mocked {quotes.Count} stock quotes [{stopWatch.ElapsedMilliseconds} ms]");
    }
}
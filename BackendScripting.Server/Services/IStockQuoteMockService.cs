namespace BackendScripting.Server.Services;

public interface IStockQuoteMockService
{
    public Task Generate(int count);
}
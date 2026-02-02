namespace BackendScripting.Model;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class StockQuoteScript : IScriptObject
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Script { get; set; }
    public byte[] Binary { get; set; } = null!;
    public int ScriptHash { get; set; }
}
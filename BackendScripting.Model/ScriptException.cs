namespace BackendScripting.Model;

/// <summary>
/// Script exception
/// </summary>
public class ScriptException : Exception
{
    public ScriptException(string? message) :
        base(message)
    {
    }

    public ScriptException(string? message, Exception? innerException) :
        base(message, innerException)
    {
    }
}
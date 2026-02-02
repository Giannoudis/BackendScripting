namespace BackendScripting.Scripting;

/// <summary>
/// Script compile exception
/// </summary>
/// <exclude />
public class ScriptCompileException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="T:ScriptCompileException"></see> class.</summary>
    public ScriptCompileException(string message) :
        base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:ScriptCompileException"></see> class.</summary>
    /// <param name="failures">The diagnostic results</param>
    public ScriptCompileException(IList<string> failures) :
        base(string.Join(Environment.NewLine, failures))
    {
    }
}
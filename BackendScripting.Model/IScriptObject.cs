namespace BackendScripting.Model;

// ReSharper disable UnusedMemberInSuper.Global
public interface IScriptObject
{
    /// <summary>
    /// The object id
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// The script text
    /// </summary>
    string Script { get; set; }

    /// <summary>
    /// The script bits
    /// </summary>
    byte[] Binary { get; set; }

    /// <summary>
    /// The script hash value
    /// </summary>
    int ScriptHash { get; set; }
}
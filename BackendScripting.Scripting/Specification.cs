namespace BackendScripting.Scripting;

/// <summary>
/// The script specification
/// </summary>
/// <exclude />
public static class Specification
{
    /// <summary>Script function timeout in milliseconds (default: 10 seconds, test/debug: 1000 seconds)</summary>
    public static readonly double ScriptFunctionTimeout = 100000;

    /// <summary>The c# language version, string represents the Microsoft.CodeAnalysis.CSharp.LanguageVersion enum</summary>
    public static readonly string CSharpLanguageVersion = "CSharp14";

    /// <summary>Code region for the function script</summary>
    public static readonly string FunctionScriptRegion = "Script";
}
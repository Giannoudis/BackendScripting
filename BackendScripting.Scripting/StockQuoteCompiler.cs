using System.Text;
using BackendScripting.Model;

namespace BackendScripting.Scripting;

/// <summary>
/// Stock quote compiler
/// </summary>
/// <exclude />
public static class StockQuoteCompiler
{
    public static void Compile(StockQuoteScript stockQuoteScript)
    {
        // script
        var script = stockQuoteScript.Script.Trim();
        if (string.IsNullOrWhiteSpace(script))
        {
            throw new ScriptCompileException($"Missing script in object {stockQuoteScript.Id}");
        }
        if (!script.EndsWith(';'))
        {
            script += ';';
        }

        // script code from template
        var scriptCode = ApplyScriptTemplate(script);

        // collect code to compile
        var codes = new List<string> { scriptCode };
        // add additional code here

        // c# compile
        var compiler = new CSharpCompiler(
            assemblyName: stockQuoteScript.GetType().FullName!);
        var result = compiler.CompileAssembly(codes);
        stockQuoteScript.Binary = result.Binary;
        stockQuoteScript.ScriptHash = result.Script.GetScriptHashCode();
    }

    private static string ApplyScriptTemplate(string script)
    {
        var assembly = typeof(StockQuoteFunction).Assembly;
        var resourceName = $"{nameof(StockQuoteFunction)}.cs";

        // template
        using Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);
        if (resourceStream == null)
        {
            throw new ScriptException($"Error reading embedded resource {resourceName}.");
        }

        using StreamReader reader = new(resourceStream);
        var template = reader.ReadToEnd();
        if (string.IsNullOrWhiteSpace(template))
        {
            throw new ScriptException($"Empty embedded resource {resourceName}.");
        }

        // apply script
        var scriptCode = SetupRegion(template, Specification.FunctionScriptRegion, script);
        return string.IsNullOrWhiteSpace(scriptCode) ?
            throw new ScriptCompileException($"Invalid script template in resource {resourceName}") :
            scriptCode;
    }

    /// <summary>
    /// Setup code region
    /// </summary>
    /// <param name="template">Code template</param>
    /// <param name="regionName">Code region name</param>
    /// <param name="code">Code to insert</param>
    private static string? SetupRegion(string template, string regionName, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        // start
        var startMarker = $"#region {regionName}";
        var startIndex = template.IndexOf(startMarker, StringComparison.InvariantCulture);
        if (startIndex < 0)
        {
            return null;
        }

        // end
        var endMarker = "#endregion";
        var endIndex = template.IndexOf(endMarker, startIndex + startMarker.Length, StringComparison.InvariantCulture);
        if (endIndex < 0)
        {
            return null;
        }

        // code
        var builder = new StringBuilder();
        var indent = RegionIndent(template, endIndex);
        var indentText = indent > 0 ? new string(' ', indent) : string.Empty;
        builder.AppendLine(startMarker);
        builder.AppendLine();
        builder.AppendLine(code);
        builder.Append($"{indentText}{endMarker}");

        // token replacement
        var placeholder = template.Substring(startIndex, endIndex - startIndex + endMarker.Length);
        return template.Replace(placeholder, builder.ToString());
    }

    /// <summary>
    /// Count the region indent
    /// </summary>
    /// <param name="template">Script template</param>
    /// <param name="index">Region start index</param>
    private static int RegionIndent(string template, int index)
    {
        var indent = 0;
        index--;
        while (index > 0)
        {
            if (template[index] == '\n')
            {
                break;
            }
            indent++;
            index--;
        }
        return indent;
    }


}
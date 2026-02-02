using System.Text;
using System.Runtime;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Serilog;
using BackendScripting.Model;

namespace BackendScripting.Scripting;

/// <summary>
/// C# compiler
/// </summary>
/// <exclude />
public class CSharpCompiler
{
    private string AssemblyName { get; }

    public CSharpCompiler(string assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            throw new ArgumentException(nameof(assemblyName));
        }

        AssemblyName = assemblyName;
    }

    static CSharpCompiler()
    {
        // references
        foreach (var defaultReferenceType in DefaultReferenceTypes)
        {
            AddDefaultReference(defaultReferenceType);
        }
        foreach (var assemblyName in DefaultReferenceAssemblies)
        {
            AddDefaultReference(assemblyName);
        }

        // namespaces
        foreach (var defaultNamespaceName in DefaultNamespaceNames)
        {
            AddDefaultNamespace(defaultNamespaceName);
        }
    }

    #region References

    private static readonly Type[] DefaultReferenceTypes =
    [
        // any object
        typeof(object),
        // used for object references
        typeof(AssemblyTargetedPatchBandAttribute),
        // used for dynamic objects
        typeof(DynamicAttribute),
        // used for linq
        typeof(Enumerable)
    ];

    private static readonly string[] DefaultReferenceAssemblies =
    [
        // core
        "System",
        "System.Runtime",
        // dynamic objects
        "Microsoft.CSharp",
        "netstandard",
        // readonly collections
        "System.Collections",
        // json
        "System.Text.Json",
        // regular expressions
        "System.Text.RegularExpressions"
    ];

    /// <summary>
    /// List of additional assembly references that are added to the
    /// compiler parameters in order to execute the script code.
    /// </summary>
    private static readonly HashSet<MetadataReference> DefaultReferences = [];

    /// <summary>
    /// Gets the c# language version
    /// </summary>
    /// <value>The c# version</value>
    private LanguageVersion? languageVersion;
    private LanguageVersion LanguageVersion
    {
        get
        {
            if (!languageVersion.HasValue)
            {
                if (!Enum.TryParse(Specification.CSharpLanguageVersion, out LanguageVersion version))
                {
                    throw new ScriptException($"Invalid c# language version: {Specification.CSharpLanguageVersion}.");
                }
                languageVersion = version;
            }
            return languageVersion.Value;
        }
    }

    /// <summary>
    /// Add a default assembly reference
    /// </summary>
    /// <param name="type">The type within the assembly</param>
    private static void AddDefaultReference(Type type) =>
        AddReference(type.Assembly, DefaultReferences);

    /// <summary>
    /// Add a default assembly reference
    /// </summary>
    /// <param name="assemblyName">The assembly name to refer</param>
    private static void AddDefaultReference(string assemblyName) =>
        AddReference(Assembly.Load(new AssemblyName(assemblyName)), DefaultReferences);


    private static void AddReference(Assembly assembly, HashSet<MetadataReference> references)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        var reference = MetadataReference.CreateFromFile(assembly.Location);
        references.Add(reference);
    }

    #endregion

    #region Namespaces

    private static readonly string[] DefaultNamespaceNames =
    [
        "System",
        "System.Text"
    ];

    /// <summary>
    /// List of additional namespaces to add to the script
    /// </summary>
    private static readonly HashSet<string> DefaultNamespaces = [];

    /// <summary>
    /// Adds a default assembly namespace
    /// </summary>
    /// <param name="name">The namespace name</param>
    private static void AddDefaultNamespace(string name) => AddNamespace(name, DefaultNamespaces);

    /// <summary>
    /// Adds a namespace to the referenced namespaces
    /// used at compile time.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="namespaces">The collection to insert the namespace</param>
    private static void AddNamespace(string name, HashSet<string> namespaces)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }
        namespaces.Add(name);
    }

    #endregion

    /// <summary>
    /// Compiles and runs the source code for a complete assembly.
    /// </summary>
    /// <param name="codes">The C# source codes</param>
    /// <returns>The compile result</returns>
    public ScriptCompileResult CompileAssembly(IList<string> codes)
    {
        if (codes == null)
        {
            throw new ArgumentNullException(nameof(codes));
        }

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        // parser options: supported c# language
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion);

        // parse code
        var syntaxTrees = new List<SyntaxTree>();
        // parse source codes
        foreach (var code in codes)
        {
            syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(SourceText.From(code), options));
        }

        // references
        var allReferences = new List<MetadataReference>(DefaultReferences);

        // create bits
        using var peStream = new MemoryStream();
        var compilation = CSharpCompilation.Create(AssemblyName,
                syntaxTrees,
                allReferences,
                new(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default))
            .Emit(peStream);

        // error handling
        if (!compilation.Success)
        {
            throw new ScriptCompileException(GetCompilerFailures(compilation));
        }

        // build the assembly
        var script = new StringBuilder();
        foreach (var code in codes)
        {
            script.AppendLine($"// {new('-', 80)}");
            script.AppendLine(code);
        }

        peStream.Seek(0, SeekOrigin.Begin);
        var result = new ScriptCompileResult(script.ToString(), peStream.ToArray());

        stopWatch.Stop();
        Log.Information($"Script compilation [{stopWatch.ElapsedMilliseconds} ms]");

        return result;
    }

    private static List<string> GetCompilerFailures(EmitResult compilation)
    {
        var failures = new List<string>();
        foreach (var diagnostic in compilation.Diagnostics.Where(diagnostic =>
                     diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error))
        {
            var failure = $"{diagnostic.GetMessage()}";

            // line
            var spanStart = diagnostic.Location.GetLineSpan().Span.Start;
            failure += $" [{diagnostic.Id}: Line {spanStart.Line + 1}, Column {spanStart.Character + 1}";

            // file
            var comment = GetSourceFileComment(diagnostic.Location);
            if (comment != null)
            {
                failure += $", {comment}";
            }
            failure += "]";
            failures.Add(failure);
        }
        return failures;
    }

    private static string? GetSourceFileComment(Location location)
    {
        var text = location.SourceTree?.ToString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }
        var commentStart = text.IndexOf("/*", StringComparison.InvariantCulture);
        var commentEnd = text.IndexOf("*/", StringComparison.InvariantCulture);
        if (commentStart < 0 || commentEnd <= commentStart)
        {
            return null;
        }
        var comment = text.Substring(commentStart, commentEnd - commentStart + 2);
        if (comment.Length > 100)
        {
            comment = comment.Substring(0, 100) + "...";
        }
        return comment;
    }
}
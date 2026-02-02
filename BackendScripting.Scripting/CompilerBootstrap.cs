using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace BackendScripting.Scripting;

/// <summary>
/// Compiler bootstrap
/// </summary>
/// <exclude />
public static class CompilerBootstrap
{
    public static void Initialize()
    {
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);
        var trees = new List<SyntaxTree>
        {
            SyntaxFactory.ParseSyntaxTree(SourceText.From("using System; public class HelloWorld {}"), options)
        };
        using var stream = new MemoryStream();
        CSharpCompilation.Create(
            assemblyName: "BootCompile",
            syntaxTrees: trees,
            references: null,
            options: new(outputKind:OutputKind.DynamicallyLinkedLibrary,
                         optimizationLevel: OptimizationLevel.Release,
                         assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default)).Emit(stream);
    }
}
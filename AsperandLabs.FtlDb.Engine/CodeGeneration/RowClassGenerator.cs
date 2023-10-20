using System.Text;
using AsperandLabs.FtlDb.Engine.Row;
using MessagePack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AsperandLabs.FtlDb.Engine.CodeGeneration;

public class RowClassGenerator
{
    private string _runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location) + Path.DirectorySeparatorChar;

    public string GetRowClassSource(TableSpec spec)
    {
        var sb = new StringBuilder();
        sb.Append(CSharpSnips.RowClassHeader(spec.Schema, spec.Name));

        var columnBody = string.Join("\n", spec.Columns.Select((x, i) => CSharpSnips.RowClassProperty(i, x.ColumnName, Type.GetType(x.ColumnValueType))));
        sb.Append(columnBody);

        sb.Append(CSharpSnips.ClassFooter);
        return sb.ToString();
    }

    public byte[]? CreateAssembly(string source, string name)
    {
        var tree = SyntaxFactory.ParseSyntaxTree(source.Trim());
        
        var compilation = CSharpCompilation.Create($"{name}.cs")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release))
            .WithReferences(GetAssemblies())
            .AddSyntaxTrees(tree);

        using var codeStream = new MemoryStream();
        var compilationResult = compilation.Emit(codeStream);

        if (compilationResult.Success)
            return codeStream.ToArray();

        var sb = new StringBuilder();
        foreach (var diag in compilationResult.Diagnostics)
        {
            sb.AppendLine(diag.ToString());
        }

        Console.WriteLine(sb.ToString());

        return null;
    }

    private List<MetadataReference> GetAssemblies()
    {
        var references = new List<MetadataReference>();

        references.Add(GetReference(typeof(string).Assembly.Location));
        references.Add(GetReference(typeof(MessagePackObjectAttribute).Assembly.Location));

        references.Add(GetReference("System.Private.CoreLib.dll"));
        references.Add(GetReference("System.Runtime.dll"));
        references.Add(GetReference("System.Console.dll"));
        references.Add(GetReference("netstandard.dll"));

        references.Add(GetReference("System.Text.RegularExpressions.dll"));
        references.Add(GetReference("System.Linq.dll"));
        references.Add(GetReference("System.Linq.Expressions.dll"));

        references.Add(GetReference("System.IO.dll"));
        references.Add(GetReference("System.Net.Primitives.dll"));
        references.Add(GetReference("System.Net.Http.dll"));
        references.Add(GetReference("System.Private.Uri.dll"));
        references.Add(GetReference("System.Reflection.dll"));
        references.Add(GetReference("System.ComponentModel.Primitives.dll"));
        references.Add(GetReference("System.Globalization.dll"));
        references.Add(GetReference("System.Collections.Concurrent.dll"));
        references.Add(GetReference("System.Collections.NonGeneric.dll"));
        references.Add(GetReference("Microsoft.CSharp.dll"));

        return references;
    }

    private MetadataReference GetReference(string assemblyDll)
    {
        var file = Path.GetFullPath(assemblyDll);

        if (File.Exists(file))
            return MetadataReference.CreateFromFile(file);

        var path = Path.GetDirectoryName(typeof(object).Assembly.Location);
        file = Path.Combine(path, assemblyDll);
        if (File.Exists(file))
            return MetadataReference.CreateFromFile(file);


        throw new FileNotFoundException($"Could not locate assembly {assemblyDll}");
    }
}
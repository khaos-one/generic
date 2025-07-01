using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Runtime.Loader;

namespace Khaos.Generic.Scripting;

public class CompiledScript : IDisposable
{
    public string Name { get; }
    private AssemblyLoadContext ScriptContext { get; }
    private Assembly ScriptAssembly { get; }
    private object? Instance { get; }
    private MethodInfo? EntryPoint { get; }

    public CompiledScript(ScriptPrototype config)
    {
        string scriptToCompile = config.Script;
        string entryTypeName = config.EntryTypeName;
        string entryMethodName = config.EntryMethodName;

        if (config.IsExpression)
        {
            var inputTypeName = config.ExpressionInputType != null ? config.ExpressionInputType.FullName : "object";
            var usingDirectives = config.ExpressionUsingDirectives != null && config.ExpressionUsingDirectives.Count > 0 
                ? string.Join("\n", config.ExpressionUsingDirectives.Select(d => $"using {d};")) + "\n" 
                : string.Empty;
            scriptToCompile = $"{usingDirectives}public class ExpressionScript\n{{\n    public static object Evaluate({inputTypeName} input)\n    {{\n        return {config.Script};\n    }}\n}}";
            entryTypeName = "ExpressionScript";
            entryMethodName = "Evaluate";
        }

        var compilation = CSharpCompilation.Create("ScriptAssembly")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(config.ReferenceAssembliesContainingTypes?.Select(t => MetadataReference.CreateFromFile(t.Assembly.Location)) ?? [])
            .AddReferences(config.ReferenceAssembliesByFileNames?.Select(f => MetadataReference.CreateFromFile(f)) ?? [])
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(scriptToCompile));

        using var stream = new MemoryStream();  
        var result = compilation.Emit(stream);

        if (!result.Success)
        {
            throw new InvalidOperationException("Compilation failed");
        }

        stream.Seek(0, SeekOrigin.Begin);
        var scriptContext = new AssemblyLoadContext(config.Name, true);
        var assembly = scriptContext.LoadFromStream(stream);
        var type = assembly.GetType(entryTypeName) ?? throw new InvalidOperationException($"Script type {entryTypeName} not found");
        var instance = Activator.CreateInstance(type);
        var method = type.GetMethod(entryMethodName) ?? throw new InvalidOperationException($"Script method {entryMethodName} not found");

        Name = config.Name;
        ScriptContext = scriptContext;
        ScriptAssembly = assembly;
        Instance = instance;
        EntryPoint = method;
    }

    public TResult? Invoke<TResult>(params object[] args)
    {
        if (EntryPoint is null)
        {
            throw new InvalidOperationException("Entry point is not set");
        }
        
        return (TResult?) EntryPoint.Invoke(Instance, args);
    }

    public async Task<TResult?> InvokeAsync<TResult>(params object[] args)
    {
        if (EntryPoint is null)
        {
            throw new InvalidOperationException("Entry point is not set");
        }

        var result = EntryPoint.Invoke(Instance, args);
        if (result is Task<TResult> task)
        {
            return await task;
        }
        else if (result is Task taskWithoutResult)
        {
            await taskWithoutResult;
            return default;
        }
        else
        {
            return (TResult?)result;
        }
    }

    private void Destroy()
    {
        if (Instance != null)
        {
            if (Instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        
        if (ScriptContext != null)
        {
            ScriptContext.Unload();
        }
    }
    
    ~CompiledScript()
    {
        Destroy();
    }

    public void Dispose()
    {
        Destroy();
        GC.SuppressFinalize(this);
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Runtime.Loader;
using System;
using System.Collections.Generic;

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
        var compilation = CSharpCompilation.Create("ScriptAssembly")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(config.ReferenceAssembliesContainingTypes?.Select(t => MetadataReference.CreateFromFile(t.Assembly.Location)) ?? [])
            .AddReferences(config.ReferenceAssembliesByFileNames?.Select(f => MetadataReference.CreateFromFile(f)) ?? [])
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(config.Script));

        using var stream = new MemoryStream();  
        var result = compilation.Emit(stream);

        if (!result.Success)
        {
            throw new InvalidOperationException("Compilation failed");
        }

        stream.Seek(0, SeekOrigin.Begin);
        var scriptContext = new AssemblyLoadContext(config.Name, true);
        var assembly = scriptContext.LoadFromStream(stream);
        var type = assembly.GetType(config.EntryTypeName) ?? throw new InvalidOperationException($"Script type {config.EntryTypeName} not found");
        var instance = Activator.CreateInstance(type);
        var method = type.GetMethod(config.EntryMethodName) ?? throw new InvalidOperationException($"Script method {config.EntryMethodName} not found");

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

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

    private CompiledScript(string name, AssemblyLoadContext scriptContext, Assembly scriptAssembly, object? instance, MethodInfo? entryPoint)
    {
        Name = name;
        ScriptContext = scriptContext;
        ScriptAssembly = scriptAssembly;
        Instance = instance;
        EntryPoint = entryPoint;
    }

    public CompiledScript(
        string name,
        string script, 
        IReadOnlyCollection<Type>? referenceAssembliesContainingTypes = null, 
        IReadOnlyCollection<string>? referenceAssembliesByFileNames = null,
        string entryTypeName = "Script",
        string entryMethodName = "Main")
    {
        var compilation = CSharpCompilation.Create("ScriptAssembly")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(referenceAssembliesContainingTypes?.Select(t => MetadataReference.CreateFromFile(t.Assembly.Location)) ?? [])
            .AddReferences(referenceAssembliesByFileNames?.Select(f => MetadataReference.CreateFromFile(f)) ?? [])
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(script));

        using var stream = new MemoryStream();  
        var result = compilation.Emit(stream);

        if (!result.Success)
        {
            throw new InvalidOperationException("Compilation failed");
        }

        stream.Seek(0, SeekOrigin.Begin);
        var scriptContext = new AssemblyLoadContext(name, true);
        var assembly = scriptContext.LoadFromStream(stream);
        var type = assembly.GetType(entryTypeName) ?? throw new InvalidOperationException($"Script type {entryTypeName} not found");
        var instance = Activator.CreateInstance(type);
        var method = type.GetMethod(entryMethodName) ?? throw new InvalidOperationException($"Script method {entryMethodName} not found");

        Name = name;
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

    public void Destroy()
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

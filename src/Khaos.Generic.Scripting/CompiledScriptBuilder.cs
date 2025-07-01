using System;
using System.Collections.Generic;

namespace Khaos.Generic.Scripting;

public class CompiledScriptBuilder
{
    private string _name;
    private string _script;
    private IReadOnlyCollection<Type>? _referenceAssembliesContainingTypes;
    private IReadOnlyCollection<string>? _referenceAssembliesByFileNames;
    private string _entryTypeName = "Script";
    private string _entryMethodName = "Main";

    public CompiledScriptBuilder(string name, string script)
    {
        _name = name;
        _script = script;
    }

    public CompiledScriptBuilder WithReferenceAssembliesContainingTypes(IReadOnlyCollection<Type> types)
    {
        _referenceAssembliesContainingTypes = types;
        return this;
    }

    public CompiledScriptBuilder WithReferenceAssembliesByFileNames(IReadOnlyCollection<string> fileNames)
    {
        _referenceAssembliesByFileNames = fileNames;
        return this;
    }

    public CompiledScriptBuilder WithEntryTypeName(string entryTypeName)
    {
        _entryTypeName = entryTypeName;
        return this;
    }

    public CompiledScriptBuilder WithEntryMethodName(string entryMethodName)
    {
        _entryMethodName = entryMethodName;
        return this;
    }

    public CompiledScript Build()
    {
        return new CompiledScript(
            _name,
            _script,
            _referenceAssembliesContainingTypes,
            _referenceAssembliesByFileNames,
            _entryTypeName,
            _entryMethodName);
    }
} 
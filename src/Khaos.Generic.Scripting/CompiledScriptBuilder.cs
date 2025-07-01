using System;
using System.Collections.Generic;

namespace Khaos.Generic.Scripting;

public class CompiledScriptBuilder
{
    private ScriptPrototype _config;

    public CompiledScriptBuilder(string name, string script)
    {
        _config = new ScriptPrototype(name, script);
    }

    public CompiledScriptBuilder WithReferenceAssembliesContainingTypes(IReadOnlyCollection<Type> types)
    {
        _config = _config with { ReferenceAssembliesContainingTypes = types };
        return this;
    }

    public CompiledScriptBuilder WithReferenceAssembliesByFileNames(IReadOnlyCollection<string> fileNames)
    {
        _config = _config with { ReferenceAssembliesByFileNames = fileNames };
        return this;
    }

    public CompiledScriptBuilder WithEntryTypeName(string entryTypeName)
    {
        _config = _config with { EntryTypeName = entryTypeName };
        return this;
    }

    public CompiledScriptBuilder WithEntryMethodName(string entryMethodName)
    {
        _config = _config with { EntryMethodName = entryMethodName };
        return this;
    }

    public CompiledScript Build()
    {
        return new CompiledScript(_config);
    }
} 
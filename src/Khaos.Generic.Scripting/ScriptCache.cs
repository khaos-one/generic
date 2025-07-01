using System;
using System.Collections.Concurrent;
using System.Text;

namespace Khaos.Generic.Scripting;

public class ScriptCache
{
    private readonly ConcurrentDictionary<string, CompiledScript> _cache = new();

    public TResult? Invoke<TResult>(ScriptPrototype prototype, params object[] args)
    {
        var cacheKey = GenerateCacheKey(prototype);
        if (_cache.TryGetValue(cacheKey, out var script))
        {
            if (IsScriptUpdated(script, prototype))
            {
                if (_cache.TryRemove(cacheKey, out var oldScript))
                {
                    oldScript.Dispose();
                }
                script = new CompiledScript(prototype);
                _cache.TryAdd(cacheKey, script);
            }
        }
        else
        {
            script = new CompiledScript(prototype);
            _cache.TryAdd(cacheKey, script);
        }

        return script.Invoke<TResult>(args);
    }

    private string GenerateCacheKey(ScriptPrototype prototype)
    {
        return prototype.Name;
    }

    private bool IsScriptUpdated(CompiledScript existingScript, ScriptPrototype newPrototype)
    {
        return existingScript.Name != newPrototype.Name ||
               !AreScriptsEqual(existingScript, newPrototype) ||
               existingScript.GetType().GetProperty("EntryTypeName")?.GetValue(existingScript)?.ToString() != newPrototype.EntryTypeName ||
               existingScript.GetType().GetProperty("EntryMethodName")?.GetValue(existingScript)?.ToString() != newPrototype.EntryMethodName;
    }

    private bool AreScriptsEqual(CompiledScript script, ScriptPrototype prototype)
    {
        return script.GetType().GetProperty("Script")?.GetValue(script)?.ToString() == prototype.Script;
    }

    public void Clear()
    {
        foreach (var script in _cache.Values)
        {
            script.Dispose();
        }
        _cache.Clear();
    }
} 
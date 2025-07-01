using System;
using System.Collections.Generic;

namespace Khaos.Generic.Scripting;

public record ScriptPrototype(
    string Name,
    string Script,
    IReadOnlyCollection<Type>? ReferenceAssembliesContainingTypes = null,
    IReadOnlyCollection<string>? ReferenceAssembliesByFileNames = null,
    string EntryTypeName = "Script",
    string EntryMethodName = "Main"); 
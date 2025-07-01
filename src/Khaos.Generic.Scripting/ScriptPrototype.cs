namespace Khaos.Generic.Scripting;

public record ScriptPrototype(
    string Name,
    string Script,
    IReadOnlyCollection<Type>? ReferenceAssembliesContainingTypes = null,
    IReadOnlyCollection<string>? ReferenceAssembliesByFileNames = null,
    string EntryTypeName = "Script",
    string EntryMethodName = "Main",
    bool IsExpression = false,
    Type? ExpressionInputType = null,
    IReadOnlyCollection<string>? ExpressionUsingDirectives = null); 
namespace Khaos.Generic.Kubernetes;

public static class KubernetesResourceMetadataHelper
{
    public static (string Group, string Version) ParseApiVersion(string apiVersion)
    {
        var parts = apiVersion.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 
            ? (parts[0], parts[1]) 
            : ("", parts[0]);
    }

    public static string GetPluralName(string kind)
    {
        // Базовые правила для преобразования в множественное число
        if (string.IsNullOrEmpty(kind))
            return string.Empty;

        // Приводим к нижнему регистру
        var lowercase = kind.ToLower();

        // Особые случаи
        if (lowercase.EndsWith("s"))
            return lowercase + "es";
        if (lowercase.EndsWith("y"))
            return lowercase[..^1] + "ies";
        if (lowercase.EndsWith("x"))
            return lowercase + "es";
        if (lowercase.EndsWith("ch"))
            return lowercase + "es";
        if (lowercase.EndsWith("sh"))
            return lowercase + "es";

        // По умолчанию добавляем 's'
        return lowercase + "s";
    }
}

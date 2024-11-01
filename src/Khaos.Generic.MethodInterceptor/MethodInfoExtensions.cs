using System.Reflection;

namespace Khaos.Generic.MethodInterceptor;

public static class MethodInfoExtensions
{
    public static bool HasMethodAttribute<T>(this MethodInfo method)
    {
        return method.GetCustomAttributes(typeof(T), false).Length > 0;
    }
}
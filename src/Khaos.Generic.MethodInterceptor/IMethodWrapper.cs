using System.Reflection;

namespace Khaos.Generic.MethodInterceptor;

public interface IMethodWrapper
{
    object Execute(object instance, MethodInfo method, object?[] parameters);
}
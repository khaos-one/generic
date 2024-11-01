using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Khaos.Generic.MethodInterceptor;

public static class ServiceCollectionExtensions 
{
    public static IServiceCollection AddMethodInterceptor(this IServiceCollection services, params Assembly[] assemblies)
    {
        var typesWithWrappedMethods = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Any(m => m.GetCustomAttribute<WrapWithAttribute>() != null));

        foreach(var type in typesWithWrappedMethods)
        {
            var interfaces = type.GetInterfaces();
            if(interfaces.Any())
            {
                foreach(var @interface in interfaces)
                {
                    services.Decorate(@interface, (instance, sp) =>
                        typeof(MethodInterceptor<>)
                            .MakeGenericType(@interface)
                            .GetMethod(nameof(MethodInterceptor<object>.Create))!
                            .Invoke(null, [instance, sp])!);
                }
            }
            else
            {
                services.Decorate(type, (instance, sp) =>
                    typeof(MethodInterceptor<>)
                        .MakeGenericType(type)
                        .GetMethod(nameof(MethodInterceptor<object>.Create))!
                        .Invoke(null, [instance, sp])!);
            }
        }
        
        return services;
    }
}
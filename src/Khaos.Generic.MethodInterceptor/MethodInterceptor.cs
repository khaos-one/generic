using System.Reflection;

namespace Khaos.Generic.MethodInterceptor;

public class MethodInterceptor<T> : DispatchProxy where T : class
{
    private T? _target;
    private IServiceProvider? _serviceProvider;

    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if(method == null || _target == null || _serviceProvider == null)
            return null;
            
        var wrapAttr = method.GetCustomAttribute<WrapWithAttribute>();
        if (wrapAttr != null)
        {
            var wrapper = (IMethodWrapper?)_serviceProvider.GetService(wrapAttr.WrapperType);
            return wrapper?.Execute(_target, method, args ?? []);
        }
        return method.Invoke(_target, args);
    }

    public static T Create(T target, IServiceProvider serviceProvider)
    {
        object? proxy = Create<T, MethodInterceptor<T>>();
        if(proxy == null) throw new InvalidOperationException();
        
        var interceptor = proxy as MethodInterceptor<T>;
        if(interceptor == null) throw new InvalidOperationException();
        
        interceptor._target = target;
        interceptor._serviceProvider = serviceProvider;
        return (T)proxy;
    }
}
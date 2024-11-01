namespace Khaos.Generic.MethodInterceptor;

[AttributeUsage(AttributeTargets.Method)]
public class WrapWithAttribute : Attribute
{
    public Type WrapperType { get; }
    public WrapWithAttribute(Type wrapperType) 
    {
        if(!typeof(IMethodWrapper).IsAssignableFrom(wrapperType))
            throw new ArgumentException($"Type must implement {nameof(IMethodWrapper)}");
        WrapperType = wrapperType;
    }
}
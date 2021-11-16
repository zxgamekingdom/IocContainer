using System;

namespace IocContainer.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class SelectedConstructorAttribute : Attribute
    {
    }
}
using System;

namespace IocContainer.Containers
{
    [AttributeUsage(AttributeTargets.Constructor,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class SelectedConstructorAttribute : Attribute
    {
    }
}
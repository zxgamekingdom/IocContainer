using System;

namespace Zt.Containers.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class SelectedConstructorAttribute : Attribute
    {
    }
}
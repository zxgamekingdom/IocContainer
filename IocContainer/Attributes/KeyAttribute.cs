using System;
using IocContainer.Logic.DataStructures;

namespace IocContainer.Attributes
{
    [AttributeUsage(
        AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field,
        Inherited = true,
        AllowMultiple = false)]
    public sealed class KeyAttribute : Attribute
    {
        public object Key { get; }

        public KeyAttribute(object? key = null)
        {
            Key = key ?? NullKey.Instance;
        }
    }
}
using System;

namespace IocContainer.Containers
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
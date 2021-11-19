using System;
using Zt.Containers.Logic.DataStructures;

namespace Zt.Containers.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter |
        AttributeTargets.Property |
        AttributeTargets.Field)]
    public sealed class KeyAttribute : Attribute
    {
        public KeyAttribute(object? key = null)
        {
            Key = key ?? NullKey.Instance;
        }

        public object Key { get; }
    }
}
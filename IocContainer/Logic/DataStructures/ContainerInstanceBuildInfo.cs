using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Zt.Containers.Logic.DataStructures
{
    public record ContainerInstanceBuildInfo
    {
        public ServiceDescriptor ServiceDescriptor { get; }
        public ContainerStorage Storage { get; }
        public ParameterExpression Variable { get; internal set; } = null!;
        public Expression<Func<object>> Expression { get; internal set; } = null!;
        public Expression[] KeyExpressions { get; internal set; } = null!;
        public Func<object> BuildFunc { get; internal set; } = null!;
        public ParameterExpression[]? InternalVariable { get; internal set; }
        public ConstructorInfo? 选中的构造函数 { get; internal set; }
        public HashSet<ServiceDescriptor>? 关联的ServiceDescriptors
        {
            get;
            internal set;
        }

        public ContainerInstanceBuildInfo(ServiceDescriptor serviceDescriptor,
            ContainerStorage storage)
        {
            ServiceDescriptor = serviceDescriptor;
            Storage = storage;
        }
    }
}
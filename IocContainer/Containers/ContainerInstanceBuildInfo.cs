using System;
using System.Linq.Expressions;
using System.Reflection;

namespace IocContainer.Containers
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
        public (Type ParameterType, object Key)[]? 关联的必选参数 { get; internal set; }
        public ParameterInfo[]? 关联的可选参数 { get; internal set; }
        public (Type ParameterType, ServiceDescriptor)[]? 关联的必选参数的ServiceDescriptor
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
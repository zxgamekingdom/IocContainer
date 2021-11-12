using System;
using System.Linq.Expressions;

namespace IocContainer.Containers
{
    public record ContainerInstanceBuildInfo
    {
        public ServiceDescriptor ServiceDescriptor { get; }
        public ParameterExpression Variable { get; internal set; } = null!;
        public Expression<Func<object>> Expression
        {
            get;
            internal set;
        } = null!;
        public Expression[] KeyExpressions { get; internal set; } = null!;
        public bool IsInitialized { get; internal set; }
        public Func<object> BuildFunc { get; internal set; } = null!;
        public Action PrecompileFactory { get; internal set; } =
            null!;

        public ContainerInstanceBuildInfo(ServiceDescriptor serviceDescriptor)
        {
            ServiceDescriptor = serviceDescriptor;
        }
    }
}
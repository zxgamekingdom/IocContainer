using System;
using System.Linq.Expressions;

namespace IocContainer.Containers
{
    public record ContainerInstanceBuildInfo
    {
        public ServiceDescriptor ServiceDescriptor { get; }
        public ContainerStorage Storage { get; }
        public ParameterExpression Variable { get; internal set; } = null!;
        public Expression<Func<object>> Expression { get; internal set; } = null!;
        public Expression[] KeyExpressions { get; internal set; } = null!;
        public bool IsInitialized { get; internal set; }
        public Func<object> BuildFunc { get; internal set; } = null!;
        private Action<ContainerInstanceBuildInfo>? PrecompileFactory { get; set; }
        public ParameterExpression[]? InternalVariable { get; internal set; }

        public void Precompile()
        {
            if (PrecompileFactory != null)
                PrecompileFactory.Invoke(this);
            else
                throw new InvalidOperationException($@"未调用{nameof(SetPrecompileFactory)
                }方法");
        }

        public ContainerInstanceBuildInfo(ServiceDescriptor serviceDescriptor,
            ContainerStorage storage)
        {
            ServiceDescriptor = serviceDescriptor;
            Storage = storage;
        }

        public void SetPrecompileFactory(Action<ContainerInstanceBuildInfo> action)
        {
            PrecompileFactory = action;
        }
    }
}
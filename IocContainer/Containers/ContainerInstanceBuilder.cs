using System;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace IocContainer.Containers
{
    public readonly struct ContainerInstanceBuilder
    {
        public Container Container { get; }
        public ContainerStorage Storage => Container.Storage;

        public ContainerInstanceBuilder(Container container)
        {
            Container = container;
        }

        public object GetService(Type serviceType, object rawKey)
        {
            var serviceDescriptor = 寻找或者创建ServiceDescriptor(serviceType, rawKey);
            var buildInfo = 寻找或者创建BuildInfo(serviceDescriptor);

            if (!buildInfo.IsInitialized)
            {
                buildInfo.PrecompileFactory.Invoke();
            }

            if (buildInfo.IsInitialized is false)
            {
                throw new InvalidOperationException($@"{serviceDescriptor}的构造信息还未初始化");
            }

            return buildInfo.BuildFunc.Invoke();
        }

        private ContainerInstanceBuildInfo 寻找或者创建BuildInfo(
            ServiceDescriptor serviceDescriptor)
        {
            var buildInfo = Storage.FindBuildInfo(serviceDescriptor);

            return buildInfo ?? 创建BuildInfo(serviceDescriptor);
        }

        private ContainerInstanceBuildInfo 创建BuildInfo(
            ServiceDescriptor serviceDescriptor)
        {
            throw new NotImplementedException();
        }

        private ServiceDescriptor 寻找或者创建ServiceDescriptor(Type serviceType,
            object rawKey)
        {
            var descriptor = Storage.FindServiceDescriptor(serviceType, rawKey);

            return descriptor ?? 创建ServiceDescriptor(serviceType, rawKey);
        }

        private ServiceDescriptor 创建ServiceDescriptor(Type serviceType, object rawKey)
        {
            if (serviceType.是不是不能处理的特殊类型())
            {
                //参数错误，不能处理此特殊类型
                throw new ArgumentException($"{serviceType.FullName}不能处理的特殊类型");
            }

            ServiceDescriptor? descriptor;

            if (serviceType.是不是可以处理特殊类型())
            {
                descriptor = 创建特殊类型的ServiceDescriptor(serviceType, rawKey);
                Storage.AddService(descriptor);
                为特殊类型创建BuildInfo(descriptor);

                return descriptor;
            }

            descriptor = new ServiceDescriptor()
            {
                Lifetime = ServiceLifetime.Transient,
                ServiceKey = rawKey,
                ServiceType = serviceType,
                ImplementationType = serviceType
            };
            Storage.AddService(descriptor);

            return descriptor;
        }

        private ServiceDescriptor 创建特殊类型的ServiceDescriptor(Type serviceType,
            object rawKey)
        {
            return new ServiceDescriptor()
            {
                Lifetime = ServiceLifetime.Transient,
                ServiceKey = rawKey,
                ServiceType = serviceType,
                ImplementationType = serviceType
            };
        }

        private void 为特殊类型创建BuildInfo(ServiceDescriptor descriptor)
        {
            var buildInfo = new ContainerInstanceBuildInfo(descriptor);
            buildInfo.PrecompileFactory = () =>
            {
                var serviceType = buildInfo.ServiceDescriptor.ServiceType;
                var implementationType = buildInfo.ServiceDescriptor.ImplementationType;
                ParameterExpression value = Variable(serviceType);
                var assign = Assign(value, Default(implementationType));
                var (@return, end) = CreateExpressionEnd(value);
                var block = Block(new[] {value,}, assign, @return, end);
                Expression<Func<object>> expression = Lambda<Func<object>>(block);
                var compile = expression.Compile();
                buildInfo.Variable = value;
                buildInfo.Expression = expression;
                buildInfo.KeyExpressions = new Expression[] {assign};
                buildInfo.IsInitialized = true;
                buildInfo.BuildFunc = compile;
            };
            Storage.AddBuildInfo(descriptor, buildInfo);
        }

        private static (GotoExpression @return, LabelExpression end)
            CreateExpressionEnd(Expression value)
        {
            var endLabel = Label(typeof(object));
            var end = Label(endLabel, Constant(null));
            var @return = Return(endLabel, Convert(value, typeof(object)));

            return (@return, end);
        }
    }
}
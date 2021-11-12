using System;

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
            寻找或者创建ServiceDescriptor(serviceType, rawKey);

            throw new NotFiniteNumberException();
        }

        private ServiceDescriptor 寻找或者创建ServiceDescriptor(Type serviceType,
            object rawKey)
        {
            var descriptor = Storage.FindServiceDescriptor(serviceType, rawKey);

            return descriptor ?? 创建ServiceDescriptor(serviceType, rawKey);
        }

        private ServiceDescriptor 创建ServiceDescriptor(Type serviceType, object rawKey)
        {
            if (TypeExtensions.是不是可以处理特殊类型(serviceType))
            {
            }

            if (TypeExtensions.是不是不能处理的特殊类型(serviceType))
            {
            }

            return new ServiceDescriptor()
            {
                Lifetime = ServiceLifetime.Transient,
                ServiceKey = rawKey,
                ServiceType = serviceType,
                ImplementationType = serviceType
            };
        }
    }
}
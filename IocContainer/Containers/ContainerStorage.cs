using System;
using System.Collections.Generic;

namespace IocContainer.Containers
{
    //Container存储类
    public class ContainerStorage
    {
        public ContainerStorage(Container container)
        {
            Container = container;
        }

        public Container Container { get; }
        //ServiceDescriptor<TService, TImplementation>字典
        public Dictionary<Type, Dictionary<object, ServiceDescriptor>>
            ServiceDescriptors { get; } = new();
        //区域字典
        public Dictionary<ServiceDescriptor, object> ScopedCahce { get; } = new();
        //单例字典
        public Dictionary<ServiceDescriptor, object> SingletonCahce { get; } = new();

        public void AddService<TService, TImplementation>(
            ServiceDescriptor<TService, TImplementation> descriptor)
            where TImplementation : TService
        {
            var serviceDescriptor = descriptor.ToServiceDescriptor();
            var serviceType = serviceDescriptor.ServiceType;
            var key = serviceDescriptor.ServiceKey;

            if (ServiceDescriptors.TryGetValue(serviceType, out var value))
            {
                if (value.TryGetValue(key, out var value1))
                {
                    当移除ServiceDescriptor时(value1);
                    value.Remove(key);
                }

                value.Add(key, serviceDescriptor);
            }
            else
            {
                value = new Dictionary<object, ServiceDescriptor>()
                {
                    {key, serviceDescriptor}
                };
                ServiceDescriptors.Add(serviceType, value);
            }
        }

        private void 当移除ServiceDescriptor时(ServiceDescriptor? serviceDescriptor)
        {
            //TODO 当移除ServiceDescriptor时
        }

        public ServiceDescriptor? FindServiceDescriptor(Type serviceType, object rawKey)
        {
            if (ServiceDescriptors.TryGetValue(serviceType, out var value) &&
                value.TryGetValue(rawKey, out var descriptor))
            {
                return descriptor!;
            }

            return default;
        }
    }
}
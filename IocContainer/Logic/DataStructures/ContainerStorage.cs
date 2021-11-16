using System;
using System.Collections.Generic;
using System.Linq;
using IocContainer.Logic.Extensions;

namespace IocContainer.Logic.DataStructures
{
    //Container存储类
    public class ContainerStorage
    {
        public ContainerStorage(Container container)
        {
            Container = container;
        }

        public Action<ServiceDescriptorRemovedArgs>? WhenServiceDescriptorRemoved
        {
            get;
            set;
        }
        private readonly object _lock = new();
        public Container Container { get; }
        //ServiceDescriptor<TService, TImplementation>字典
        public Dictionary<Type, Dictionary<object, ServiceDescriptor>>
            ServiceDescriptors { get; } = new();
        //区域字典
        public Dictionary<ServiceDescriptor, object> ScopedCache { get; } = new();
        //单例字典
        public Dictionary<ServiceDescriptor, object> SingletonCache { get; } = new();

        internal void AddService(ServiceDescriptor descriptor)
        {
            lock (_lock)
            {
                var serviceType = descriptor.ServiceType;
                var key = descriptor.ServiceKey;
                Start:

                if (ServiceDescriptors.TryGetValue(serviceType, out var value))
                {
                    if (value.TryGetValue(key, out var value1))
                    {
                        value.Remove(key);
                        当移除ServiceDescriptor时(value1);

                        goto Start;
                    }
                }
                else
                {
                    value = new Dictionary<object, ServiceDescriptor>
                    {
                        {key, descriptor}
                    };
                    ServiceDescriptors.Add(serviceType, value);
                }
            }
        }

        public void AddService<TService, TImplementation>(
            ServiceDescriptor<TService, TImplementation> descriptor)
            where TImplementation : TService
        {
            lock (_lock)
            {
                var serviceDescriptor = descriptor.ToServiceDescriptor();

                if (descriptor.ImplementationType.是不是可以处理特殊类型())
                {
                    if (descriptor.ImplementationFactory == null &&
                        descriptor.ImplementationInstance == null)
                    {
                        throw new ArgumentException($@"{serviceDescriptor
                        }是一个特殊的类型,必须指定实现实例或者实现工厂");
                    }
                }

                AddService(serviceDescriptor);
            }
        }

        private void 当移除ServiceDescriptor时(ServiceDescriptor serviceDescriptor)
        {
            if (BuildInfos.TryGetValue(serviceDescriptor, out _))
            {
                BuildInfos.Remove(serviceDescriptor);
            }

            foreach (var pair in BuildInfos.Where(pair =>
                    pair.Value.关联的ServiceDescriptors?.Contains(serviceDescriptor) is
                        true)
                .ToArray())
            {
                BuildInfos.Remove(pair.Key);
            }

            object? value;
            {
                if (SingletonCache.TryGetValue(serviceDescriptor, out value))
                {
                }
                else if (ScopedCache.TryGetValue(serviceDescriptor, out value))
                {
                }
            }

            foreach (var pair in ServiceDescriptors
                .Where(pair => pair.Value?.Count is 0)
                .ToArray())
            {
                ServiceDescriptors.Remove(pair.Key);
            }

            WhenServiceDescriptorRemoved?.Invoke(
                new ServiceDescriptorRemovedArgs(serviceDescriptor, value));
        }

        public ServiceDescriptor? GetServiceDescriptor(Type serviceType, object rawKey)
        {
            lock (_lock)
            {
                if (ServiceDescriptors.TryGetValue(serviceType, out var value) &&
                    value.TryGetValue(rawKey, out var descriptor))
                {
                    return descriptor!;
                }

                return default;
            }
        }

        public Dictionary<ServiceDescriptor, ContainerInstanceBuildInfo> BuildInfos
        {
            get;
        } = new();

        internal void AddBuildInfo(ServiceDescriptor descriptor,
            ContainerInstanceBuildInfo buildInfo)
        {
            lock (_lock)
            {
                if (ServiceDescriptors.TryGetValue(descriptor.ServiceType,
                        out var value) &&
                    value!.TryGetValue(descriptor.ServiceKey, out _))
                {
                    if (BuildInfos.ContainsKey(descriptor))
                    {
                        throw new ArgumentException($"{descriptor}已经有构造信息了");
                    }

                    BuildInfos.Add(descriptor, buildInfo);

                    return;
                }

                throw new InvalidOperationException($"为{descriptor}添加构造信息失败");
            }
        }

        public ContainerInstanceBuildInfo? GetBuildInfo(
            ServiceDescriptor serviceDescriptor)
        {
            BuildInfos.TryGetValue(serviceDescriptor, out var info);

            return info;
        }

        public object? GetInstanceFromCache(ServiceDescriptor serviceDescriptor)
        {
            return serviceDescriptor.Lifetime switch
            {
                ServiceLifetime.Scoped when ScopedCache.TryGetValue(serviceDescriptor,
                    out var scoped) => scoped,
                ServiceLifetime.Singleton when SingletonCache.TryGetValue(
                    serviceDescriptor,
                    out var singleton) => singleton,
                _ => default
            };
        }

        internal void AddInstanceToCache(ServiceDescriptor serviceDescriptor,
            object instance)
        {
            lock (_lock)
            {
                switch (serviceDescriptor.Lifetime)
                {
                    case ServiceLifetime.Scoped:
                        ScopedCache.Add(serviceDescriptor, instance);

                        break;
                    case ServiceLifetime.Singleton:
                        SingletonCache.Add(serviceDescriptor, instance);

                        break;
                }
            }
        }
    }
}
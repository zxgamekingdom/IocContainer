using System;
using System.Collections.Generic;
using System.Linq;
using Zt.Containers.Logic.Extensions;

namespace Zt.Containers.Logic.DataStructures
{
    //Container存储类
    public class ContainerStorage
    {
        public ContainerStorage(Container container, ContainerStorage? parent = default)
        {
            Container = container;

            if (parent != null)
            {
                SingletonCache = parent.SingletonCache;
                SingletonServiceDescriptors = parent.SingletonServiceDescriptors;
                ScopedServiceDescriptors =
                    new Dictionary<Type, Dictionary<object, ServiceDescriptor>>();
                TransientServiceDescriptors =
                    new Dictionary<Type, Dictionary<object, ServiceDescriptor>>();

                foreach (var descriptor in parent.ScopedServiceDescriptors
                    .Select(pair => pair.Value)
                    .SelectMany(dictionary => dictionary.Values))
                {
                    AddService(descriptor);
                }

                foreach (var descriptor in parent.TransientServiceDescriptors
                    .Select(pair => pair.Value)
                    .SelectMany(dictionary => dictionary.Values))
                {
                    AddService(descriptor);
                }
            }
            else
            {
                SingletonCache = new Dictionary<ServiceDescriptor, object>();
                SingletonServiceDescriptors =
                    new Dictionary<Type, Dictionary<object, ServiceDescriptor>>();
                ScopedServiceDescriptors =
                    new Dictionary<Type, Dictionary<object, ServiceDescriptor>>();
                TransientServiceDescriptors =
                    new Dictionary<Type, Dictionary<object, ServiceDescriptor>>();
            }
        }

        public Dictionary<ServiceDescriptor, ContainerInstanceBuildInfo> BuildInfos
        {
            get;
        } = new();
        public Container Container { get; }
        //区域字典
        public Dictionary<ServiceDescriptor, object> ScopedCache { get; } = new();
        public Dictionary<Type, Dictionary<object, ServiceDescriptor>>
            ScopedServiceDescriptors
        { get; }
        //单例字典
        public Dictionary<ServiceDescriptor, object> SingletonCache { get; }
        public Dictionary<Type, Dictionary<object, ServiceDescriptor>>
            SingletonServiceDescriptors
        { get; }
        public Dictionary<Type, Dictionary<object, ServiceDescriptor>>
            TransientServiceDescriptors
        { get; }
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
        public ContainerInstanceBuildInfo? GetBuildInfo(
            ServiceDescriptor serviceDescriptor)
        {
            BuildInfos.TryGetValue(serviceDescriptor, out var info);

            return info;
        }
        public object? GetInstanceFromCache(ServiceDescriptor serviceDescriptor)
        {
            object? value = null;

            switch (serviceDescriptor.Lifetime)
            {
                case ServiceLifetime.Transient:
                    break;
                case ServiceLifetime.Scoped:
                    ScopedCache.TryGetValue(serviceDescriptor, out value);

                    break;
                case ServiceLifetime.Singleton:
                    SingletonCache.TryGetValue(serviceDescriptor, out value);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return value;
        }
        public ServiceDescriptor? GetServiceDescriptor(Type serviceType, object rawKey)
        {
            lock (_lock)
            {
                foreach (var dictionary in new[]
                {
                    SingletonServiceDescriptors,
                    ScopedServiceDescriptors,
                    TransientServiceDescriptors
                })
                {
                    if (dictionary.TryGetValue(serviceType, out var value) &&
                        value.TryGetValue(rawKey, out var descriptor))
                    {
                        return descriptor!;
                    }
                }

                return default;
            }
        }
        internal void AddBuildInfo(ServiceDescriptor descriptor,
            ContainerInstanceBuildInfo buildInfo)
        {
            lock (_lock)
            {
                if (GetServiceDescriptorsStorage(descriptor)
                    .TryGetValue(descriptor.ServiceType, out var value) &&
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
                    case ServiceLifetime.Transient:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        internal void AddService(ServiceDescriptor descriptor)
        {
            lock (_lock)
            {
                var serviceType = descriptor.ServiceType;
                var key = descriptor.ServiceKey;
            Start:
                var descriptorsStorage = GetServiceDescriptorsStorage(descriptor);

                foreach (var dictionary in new[]
                {
                    SingletonServiceDescriptors,
                    ScopedServiceDescriptors,
                    TransientServiceDescriptors
                })
                {
                    if (dictionary.TryGetValue(serviceType, out var value) &&
                        value.TryGetValue(key, out var value1))
                    {
                        value.Remove(key);
                        当移除ServiceDescriptor时(value1);

                        goto Start;
                    }
                }

                if (descriptorsStorage.TryGetValue(serviceType, out var dict))
                {
                    dict.Add(key, descriptor);
                }
                else
                {
                    descriptorsStorage.Add(serviceType,
                        new Dictionary<object, ServiceDescriptor> { { key, descriptor } });
                }
            }
        }
        private readonly object _lock = new();
        private Dictionary<Type, Dictionary<object, ServiceDescriptor>>
            GetServiceDescriptorsStorage(ServiceDescriptor descriptor)
        {
            Dictionary<Type, Dictionary<object, ServiceDescriptor>> serviceDescriptors =
                descriptor.Lifetime switch
                {
                    ServiceLifetime.Transient => TransientServiceDescriptors,
                    ServiceLifetime.Scoped => ScopedServiceDescriptors,
                    ServiceLifetime.Singleton => SingletonServiceDescriptors,
                    _ => throw new ArgumentOutOfRangeException()
                };

            return serviceDescriptors;
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
            var descriptorsStorage = GetServiceDescriptorsStorage(serviceDescriptor);

            foreach (var pair in descriptorsStorage
                .Where(pair => pair.Value?.Count is 0)
                .ToArray())
            {
                descriptorsStorage.Remove(pair.Key);
            }

            Container.WhenServiceDescriptorRemoved?.Invoke(
                new ServiceDescriptorRemovedArgs(serviceDescriptor, value));
        }
    }
}
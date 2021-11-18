using System;
using Zt.Containers.Logic.DataStructures;

namespace Zt.Containers.Logic.Extensions
{
    public static class ContainerExtensions
    {
        public static bool IsRoot(this Container container)
        {
            return container.Parent == null;
        }

        public static Container GetRootContainer(this Container container)
        {
            while (true)
            {
                if (container.IsRoot()) return container;

                container = container.Parent!;
            }
        }

        public static T GetService<T>(this Container container, object? key = null)
        {
            return (T) container.GetService(typeof(T), key);
        }

        public static void AddService<TService>(this Container container,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            object? serviceKey = default)
        {
            AddService<TService, TService>(container, lifetime, serviceKey);
        }

        public static void AddService<TService, TImplementation>(
            this Container container,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            object? serviceKey = default) where TImplementation : TService
        {
            container.AddService(
                new ServiceDescriptor<TService, TImplementation>(lifetime, serviceKey));
        }

        public static void AddService<TService>(this Container container,
            Func<Container, TService> implementationFactory,
            ServiceLifetime lifeTime = ServiceLifetime.Transient,
            object? serviceKey = null)
        {
            AddService<TService, TService>(container,
                implementationFactory,
                lifeTime,
                serviceKey);
        }

        public static void AddService<TService, TImplementation>(
            this Container container,
            Func<Container, TImplementation> implementationFactory,
            ServiceLifetime lifeTime = ServiceLifetime.Transient,
            object? serviceKey = null) where TImplementation : TService
        {
            container.AddService(
                new ServiceDescriptor<TService, TImplementation>(implementationFactory,
                    lifeTime,
                    serviceKey));
        }

        public static void AddService<TService>(this Container container,
            TService implementationInstance,
            ServiceLifetime lifetime = ServiceLifetime.Scoped,
            object? serviceKey = null)
        {
            AddService<TService, TService>(container,
                implementationInstance,
                lifetime,
                serviceKey);
        }

        public static void AddService<TService, TImplementation>(
            this Container container,
            TImplementation implementationInstance,
            ServiceLifetime lifetime = ServiceLifetime.Scoped,
            object? serviceKey = null) where TImplementation : TService
        {
            container.AddService(
                new ServiceDescriptor<TService, TImplementation>(implementationInstance,
                    lifetime,
                    serviceKey));
        }
    }
}
using System;

namespace IocContainer.Containers
{
    public static class ContainerExtensions
    {
        public static T GetService<T>(this Container container, object? key = null)
        {
            return (T) container.GetService(typeof(T), key);
        }

        public static void AddService<TService, TImplementation>(
            this Container container,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            object? serviceKey = default) where TImplementation : TService
        {
            container.AddService(
                new ServiceDescriptor<TService, TImplementation>(lifetime, serviceKey));
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
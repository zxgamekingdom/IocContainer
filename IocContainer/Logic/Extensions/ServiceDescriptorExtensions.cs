using IocContainer.Logic.DataStructures;

namespace IocContainer.Logic.Extensions
{
    /// <summary>
    /// ServiceDescriptor扩展函数类
    /// </summary>
    public static class ServiceDescriptorExtensions
    {
        public static ServiceDescriptor ToServiceDescriptor<TService, TImplementation>(
            this ServiceDescriptor<TService, TImplementation> descriptor)
            where TImplementation : TService
        {
            return new ServiceDescriptor
            {
                ServiceType = descriptor.ServiceType,
                ImplementationType = descriptor.ImplementationType,
                ImplementationInstance = descriptor.ImplementationInstance,
                ImplementationFactory =
                    descriptor.ImplementationFactory != null
                        ? (c) => descriptor.ImplementationFactory!.Invoke(c)!
                        : null,
                Lifetime = descriptor.Lifetime,
                ServiceKey = descriptor.ServiceKey ?? NullKey.Instance,
            };
        }
    }
}
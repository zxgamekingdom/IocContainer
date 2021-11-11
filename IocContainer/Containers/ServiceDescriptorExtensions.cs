namespace IocContainer.Containers
{
    /// <summary>
    /// ServiceDescriptor扩展函数类
    /// </summary>
    public static class ServiceDescriptorExtensions
    {
        /// <summary>
        /// ServiceDescriptor<TService, TImplementation> convert to ServiceDescriptor
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static ServiceDescriptor ToServiceDescriptor<TService, TImplementation>(this ServiceDescriptor<TService, TImplementation> descriptor) where TImplementation : TService
        {
            return new ServiceDescriptor()
            {
                ServiceType = descriptor.ServiceType,
                ImplementationType = descriptor.ImplementationType,
                ImplementationInstance = descriptor.ImplementationInstance,
                ImplementationFactory = descriptor.ImplementationFactory != null ? (c) => descriptor.ImplementationFactory!.Invoke(c)! : null,
                Lifetime = descriptor.Lifetime,
                ServiceKey = descriptor.ServiceKey ?? NullKey.Instance,
            };
        }
    }
}
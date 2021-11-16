using System;

namespace IocContainer.Containers
{
    public class Container : MarshalByRefObject
    {
        internal ContainerStorage Storage { get; }

        public Container()
        {
            Storage = new ContainerStorage(this);
        }

        //AddService(ServiceDescriptor<TService, TImplementation>)
        public void AddService<TService, TImplementation>(
            ServiceDescriptor<TService, TImplementation> serviceDescriptor)
            where TImplementation : TService
        {
            Storage.AddService(serviceDescriptor);
        }

        public object GetService(Type serviceType, object? key = null)
        {
            var rawKey = key ?? NullKey.Instance;

            return new ContainerInstanceBuilder(this).GetService(serviceType, rawKey);
        }
    }
}
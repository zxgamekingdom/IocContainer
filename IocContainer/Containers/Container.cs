namespace IocContainer.Containers
{
    public class Container
    {
        internal ContainerStorage Storage { get; }
        public Container()
        {
            Storage = new ContainerStorage(this);
        }
        //AddService(ServiceDescriptor<TService, TImplementation>)
        public void AddService<TService, TImplementation>(ServiceDescriptor<TService, TImplementation> serviceDescriptor) where TImplementation : TService
        {
            Storage.AddService(serviceDescriptor);
        }

    }
}
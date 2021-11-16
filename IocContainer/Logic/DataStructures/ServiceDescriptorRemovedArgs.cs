namespace IocContainer.Logic.DataStructures
{
    public record ServiceDescriptorRemovedArgs(ServiceDescriptor ServiceDescriptor,
        object? CacheInstance);
}
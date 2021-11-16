namespace IocContainer.Containers
{
    public record ServiceDescriptorRemovedArgs(ServiceDescriptor ServiceDescriptor,
        object? CacheInstance);
}
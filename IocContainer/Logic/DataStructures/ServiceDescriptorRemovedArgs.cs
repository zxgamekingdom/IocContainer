namespace Zt.Containers.Logic.DataStructures
{
    public record ServiceDescriptorRemovedArgs(ServiceDescriptor ServiceDescriptor,
        object? CacheInstance);
}
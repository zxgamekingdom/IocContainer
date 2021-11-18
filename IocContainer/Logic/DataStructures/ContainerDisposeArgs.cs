using System;
using System.Collections.Generic;
using System.Linq;

namespace Zt.Containers.Logic.DataStructures
{
    public record ContainerDisposeArgs(ContainerStorage Storage)
    {
        public Container Container => Storage.Container;
        public void ClearSingletonInstances() { Storage.SingletonCache.Clear(); }
        public void ClearScopedInstances() { Storage.ScopedCache.Clear(); }
        public void ClearBuildInfos() { Storage.BuildInfos.Clear(); }

        public void ClearSingletonServiceDescriptors()
        {
            Storage.SingletonServiceDescriptors.Clear();
        }

        public void ClearScopedServiceDescriptors()
        {
            Storage.ScopedServiceDescriptors.Clear();
        }

        public void ClearTransientServiceDescriptors()
        {
            Storage.TransientServiceDescriptors.Clear();
        }

        public ServiceDescriptor[] GetAllServiceDescriptors()
        {
            ServiceDescriptor[] serviceDescriptors = new[]
                {
                    Storage.SingletonServiceDescriptors,
                    Storage.ScopedServiceDescriptors,
                    Storage.TransientServiceDescriptors
                }.SelectMany(dictionary => dictionary.Values)
                .SelectMany(dictionary => dictionary.Values)
                .ToArray();

            return serviceDescriptors;
        }

        public (ServiceDescriptor ServiceDescriptor, object Instance)[]
            GetAllSingletonInstances()
        {
            return Storage.SingletonCache.Select(pair => (pair.Key, pair.Value))
                .ToArray();
        }

        public (ServiceDescriptor ServiceDescriptor, object Instance)[]
            GetAllScopedInstances()
        {
            return Storage.ScopedCache.Select(pair => (pair.Key, pair.Value)).ToArray();
        }

        public void DisposeScopeds()
        {
            foreach (var pair in Storage.ScopedCache)
                if (pair.Value is IDisposable disposable) { disposable.Dispose(); }
        }

        public void DisposeSingletons()
        {
            foreach (var pair in Storage.SingletonCache)
                if (pair.Value is IDisposable disposable) { disposable.Dispose(); }
        }

        internal ContainerStorage Storage { get; init; } = Storage;
    }
}
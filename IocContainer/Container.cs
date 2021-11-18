using System;
using System.Collections.Generic;
using System.Linq;
using Zt.Containers.Logic;
using Zt.Containers.Logic.DataStructures;
using Zt.Containers.Logic.Extensions;

namespace Zt.Containers
{
    public class Container : MarshalByRefObject, IDisposable
    {
        private readonly List<Container> _children = new();
        public Container[] Children => _children.ToArray();

        public Container(Container parent)
        {
            Parent = parent;
            Parent._children.Add(this);
            Storage = new ContainerStorage(this, parent.Storage);
        }

        internal ContainerStorage Storage { get; set; }
        //父容器
        public Container? Parent { get; }
        /// <summary>
        /// 当服务描述信息被移除时
        /// </summary>
        public Action<ServiceDescriptorRemovedArgs>? WhenServiceDescriptorRemoved
        {
            get;
            set;
        }
        /// <summary>
        /// 当容器Dispose时
        /// </summary>
        /// <remarks>
        ///默认情况下DisposeScopeds,ClearBuildInfos,ClearScopedInstances,ClearScopedServiceDescriptors,ClearTransientServiceDescriptors
        /// Dispose所有子容器
        /// 如果是根容器还会DisposeSingletons,ClearSingletonInstances,ClearSingletonServiceDescriptors
        /// </remarks>
        public Action<ContainerDisposeArgs>? WhenContainerDispose { get; set; } =
            args =>
            {
                args.DisposeScopeds();
                args.ClearBuildInfos();
                args.ClearScopedInstances();
                args.ClearScopedServiceDescriptors();
                args.ClearTransientServiceDescriptors();
                foreach (var child in args.Container.Children) child.Dispose();

                if (!args.Container.IsRoot()) return;

                args.DisposeSingletons();
                args.ClearSingletonInstances();
                args.ClearSingletonServiceDescriptors();
            };

        //创建子容器
        public Container CreateSubContainer() { return new Container(this); }
        public Container() { Storage = new ContainerStorage(this); }

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

        public void Dispose()
        {
            WhenContainerDispose?.Invoke(new ContainerDisposeArgs(Storage));
        }
    }
}
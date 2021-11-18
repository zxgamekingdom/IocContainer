using System;
using TestProject.测试数据;
using Xunit;
using Zt.Containers;
using Zt.Containers.Logic.DataStructures;
using Zt.Containers.Logic.Extensions;

namespace TestProject
{
    public class TestContainerStorage
    {
        [Fact]
        public void 有ServiceDescriptor变更时()
        {
            var container = new Container();
            var service = container.GetService<链式2>();
            Assert.Equal(0, service.链式3.I);
            container.AddService(_ => new 链式3 {I = 233});
            var l3 = container.GetService<链式3>();
            Assert.Equal(233, l3.I);
            var service1 = container.GetService<链式2>();
            Assert.NotEqual(service, service1);
            Assert.NotEqual(service.链式3, service1.链式3);
            Assert.Equal(233, service1.链式3.I);
        }

        [Fact]
        public void 添加可以处理的特殊服务()
        {
            var container = new Container();
            Assert.ThrowsAny<ArgumentException>(() => container.AddService<int>());
            Assert.ThrowsAny<ArgumentException>(() => container.AddService<string>());
            Assert.Null(TryException(() => container.AddService<A>()));
            Assert.NotNull(TryException(() => container.AddService<double>()));
        }

        private static Exception? TryException(Action action)
        {
            Exception? ex = null;

            try { action.Invoke(); }
            catch (Exception? e) { ex = e; }

            return ex;
        }

        [Fact]
        public void 重复添加服务()
        {
            var container = new Container();
            var gDescriptor = new ServiceDescriptor<IA, A>();
            var descriptor = gDescriptor.ToServiceDescriptor();
            container.AddService(gDescriptor);
            container.AddService(gDescriptor);
            var storage = container.GetStorage();
            Assert.Single(storage.TransientServiceDescriptors[descriptor.ServiceType]);
        }

        [Fact]
        public void 添加服务()
        {
            var container = new Container();
            var gDescriptor = new ServiceDescriptor<IA, A>();
            var descriptor = gDescriptor.ToServiceDescriptor();
            container.AddService(gDescriptor);
            var storage = container.GetStorage();

            Assert.Equal(
                storage.TransientServiceDescriptors[descriptor.ServiceType][descriptor
                    .ServiceKey],
                descriptor);
        }
    }
}
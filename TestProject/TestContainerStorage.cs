using System;
using IocContainer.Containers;
using TestProject.测试数据;
using Xunit;

namespace TestProject
{
    public class TestContainerStorage
    {
        [Fact]
        public void Test_添加可以处理的特殊服务()
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

            try
            {
                action.Invoke();
            }
            catch (Exception? e)
            {
                ex = e;
            }

            return ex;
        }

        [Fact]
        public void Test_重复添加服务()
        {
            var container = new Container();
            var gDescriptor = new ServiceDescriptor<IA, A>();
            var descriptor = gDescriptor.ToServiceDescriptor();
            container.AddService(gDescriptor);
            container.AddService(gDescriptor);
            var storage = container.GetStorage();
            Assert.Single(storage.ServiceDescriptors[descriptor.ServiceType]);
        }

        [Fact]
        public void Test_添加服务()
        {
            var container = new Container();
            var gDescriptor = new ServiceDescriptor<IA, A>();
            var descriptor = gDescriptor.ToServiceDescriptor();
            container.AddService(gDescriptor);
            var storage = container.GetStorage();
            Assert.Equal(
                storage.ServiceDescriptors[descriptor.ServiceType][
                    descriptor.ServiceKey],
                descriptor);
        }
    }
}
using System;
using IocContainer.Containers;
using TestProject.��������;
using Xunit;

namespace TestProject
{
    public class TestContainerStorage
    {
        [Fact]
        public void Test_��ServiceDescriptor���ʱ()
        {
            var container = new Container();
            var service = container.GetService<��ʽ2>();
            Assert.Equal(0, service.��ʽ3.I);
            container.AddService(_ => new ��ʽ3 {I = 233});
            var l3 = container.GetService<��ʽ3>();
            Assert.Equal(233, l3.I);
            var service1 = container.GetService<��ʽ2>();
            Assert.NotEqual(service, service1);
            Assert.NotEqual(service.��ʽ3, service1.��ʽ3);
            Assert.Equal(233, service1.��ʽ3.I);
        }

        [Fact]
        public void Test_��ӿ��Դ�����������()
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
        public void Test_�ظ���ӷ���()
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
        public void Test_��ӷ���()
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
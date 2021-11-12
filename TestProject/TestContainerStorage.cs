using System.Reflection;
using IocContainer.Containers;
using TestProject.��������;
using Xunit;

namespace TestProject
{
    public class TestContainerStorage
    {
        [Fact]
        public void Test_�ظ���ӷ���()
        {
            var container = new Container();
            var gDescriptor = new ServiceDescriptor<IA, A>();
            var descriptor = gDescriptor.ToServiceDescriptor();
            container.AddService(gDescriptor);
            container.AddService(gDescriptor);
            var storage = GetStorage(container);
            Assert.Single(storage.ServiceDescriptors[descriptor.ServiceType]);
        }

        [Fact]
        public void Test_��ӷ���()
        {
            var container = new Container();
            var gDescriptor = new ServiceDescriptor<IA, A>();
            var descriptor = gDescriptor.ToServiceDescriptor();
            container.AddService(gDescriptor);
            var storage = GetStorage(container);
            Assert.Equal(
                storage.ServiceDescriptors[descriptor.ServiceType][
                    descriptor.ServiceKey],
                descriptor);
        }

        private static ContainerStorage GetStorage(Container container)
        {
            return (ContainerStorage) container.GetType()
                .GetProperty("Storage", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(container);
        }
    }
}
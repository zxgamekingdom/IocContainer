//测试ServiceDescriptorExtensions
using IocContainer.Containers;
using Xunit;

public class TestServiceDescriptorExtensions
{
    public interface IA { }
    public class A : IA { };
    [Fact]
    public void Test_Default()
    {
        {
            var descriptor = new ServiceDescriptor<IA, A>();
            ServiceDescriptor serviceDescriptor = descriptor.ToServiceDescriptor();
            Assert.Equal(descriptor.Lifetime, serviceDescriptor.Lifetime);
            Assert.Equal(descriptor.ServiceType, serviceDescriptor.ServiceType);
            Assert.Equal(descriptor.ImplementationType, serviceDescriptor.ImplementationType);
            Assert.Equal(serviceDescriptor.ServiceKey, NullKey.Instance);
            Assert.Null(serviceDescriptor.ImplementationInstance);
            Assert.Null(serviceDescriptor.ImplementationFactory);
        }
        {
            var descriptor = new ServiceDescriptor<IA, A>(serviceKey:"123");
            ServiceDescriptor serviceDescriptor = descriptor.ToServiceDescriptor();
            Assert.Equal(descriptor.Lifetime, serviceDescriptor.Lifetime);
            Assert.Equal(descriptor.ServiceType, serviceDescriptor.ServiceType);
            Assert.Equal(descriptor.ImplementationType, serviceDescriptor.ImplementationType);
            Assert.Null(serviceDescriptor.ImplementationInstance);
            Assert.Null(serviceDescriptor.ImplementationFactory);
            Assert.Equal("123", serviceDescriptor.ServiceKey); 
            Assert.Equal("123", descriptor.ServiceKey);
        }
    }
    [Fact]
    public void Test_ImplementationInstance()
    {
        var descriptor = new ServiceDescriptor<IA, A>(new A());
        ServiceDescriptor serviceDescriptor = descriptor.ToServiceDescriptor();
        Assert.Equal(descriptor.Lifetime, serviceDescriptor.Lifetime);
        Assert.Equal(descriptor.ServiceType, serviceDescriptor.ServiceType);
        Assert.Equal(descriptor.ImplementationType, serviceDescriptor.ImplementationType);
        Assert.Equal(serviceDescriptor.ServiceKey, NullKey.Instance);
        Assert.NotNull(serviceDescriptor.ImplementationInstance);
        Assert.Equal(serviceDescriptor.ImplementationInstance, descriptor.ImplementationInstance);
        Assert.Null(serviceDescriptor.ImplementationFactory);
    }
    [Fact]
    public void Test_ImplementationFactory()
    {
        var descriptor = new ServiceDescriptor<IA, A>((c) => new A());
        ServiceDescriptor serviceDescriptor = descriptor.ToServiceDescriptor();
        Assert.Equal(descriptor.ServiceType, serviceDescriptor.ServiceType);
        Assert.Equal(descriptor.ImplementationType, serviceDescriptor.ImplementationType);
        Assert.Equal(serviceDescriptor.ServiceKey, NullKey.Instance);
        Assert.NotNull(serviceDescriptor.ImplementationFactory);
        Assert.IsType<A>(serviceDescriptor.ImplementationFactory!.Invoke(default!)!);
    }

}
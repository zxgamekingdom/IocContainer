using IocContainer.Containers;
using TestProject.测试数据;
using Xunit;

namespace TestProject
{
    public class TestContainer获取服务
    {
        // [Fact]
        // public void Test_指定实现工厂()
        // {
        //     var container = new Container();
        //     container.AddService<IA, A>(_ => new A());
        //     var service = container.GetService(typeof(A));
        //     Assert.IsType<A>(service);
        // }
        //
        // [Fact]
        // public void Test_指定实现实例()
        // {
        //     var container = new Container();
        //     container.AddService<IA, A>(new A());
        //     var service = container.GetService(typeof(A));
        //     var service1 = container.GetService(typeof(A));
        //     Assert.IsType<A>(service);
        //     Assert.Equal(service1, service);
        // }
    }
}
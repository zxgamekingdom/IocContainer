using System;
using System.Threading.Tasks;
using TestProject.测试数据;
using Xunit;
using Zt.Containers;
using Zt.Containers.Logic.DataStructures;
using Zt.Containers.Logic.Extensions;

namespace TestProject
{
    public class TestContainer获取服务
    {
        [Fact]
        public void 特殊生命周期()
        {
            {
                var container = new Container();
                container.AddService<A>();
                Assert.NotSame(container.GetService<A>(), container.GetService<A>());
                container.AddService(_ => new A(), ServiceLifetime.Transient, "Func");
                Assert.NotSame(container.GetService<A>("Func"),
                    container.GetService<A>("Func"));
                Assert.ThrowsAny<Exception>(() =>
                {
                    container.AddService<A>(new A(),
                        ServiceLifetime.Transient,
                        "Instance");
                });
                container.AddService<默认值>();
                Assert.NotSame(container.GetService<默认值>(),
                    container.GetService<默认值>());
                container.AddService<CtorAllOption>();
                Assert.NotSame(container.GetService<CtorAllOption>(),
                    container.GetService<CtorAllOption>());
                container.AddService<链式1>();
                Assert.NotSame(container.GetService<链式1>(),
                    container.GetService<链式1>());
            }
            {
                var container = new Container();
                container.AddService<A>(ServiceLifetime.Scoped);
                Assert.Same(container.GetService<A>(), container.GetService<A>());
                container.AddService<A>(_ => new A(), ServiceLifetime.Scoped, "Func");
                Assert.Same(container.GetService<A>("Func"),
                    container.GetService<A>("Func"));
                container.AddService<A>(new A(), ServiceLifetime.Scoped, "Instance");
                Assert.Same(container.GetService<A>("Instance"),
                    container.GetService<A>("Instance"));
                container.AddService<默认值>(ServiceLifetime.Scoped);
                Assert.Same(container.GetService<默认值>(), container.GetService<默认值>());
                container.AddService<CtorAllOption>(ServiceLifetime.Scoped);
                Assert.Same(container.GetService<CtorAllOption>(),
                    container.GetService<CtorAllOption>());
                container.AddService<链式1>(ServiceLifetime.Scoped);
                Assert.Same(container.GetService<链式1>(), container.GetService<链式1>());
            }
            {
                var container = new Container();
                container.AddService<A>(ServiceLifetime.Singleton);
                Assert.Same(container.GetService<A>(), container.GetService<A>());
                container.AddService<A>(_ => new A(),
                    ServiceLifetime.Singleton,
                    "Func");
                Assert.Same(container.GetService<A>("Func"),
                    container.GetService<A>("Func"));
                container.AddService<A>(new A(), ServiceLifetime.Singleton, "Instance");
                Assert.Same(container.GetService<A>("Instance"),
                    container.GetService<A>("Instance"));
                container.AddService<默认值>(ServiceLifetime.Singleton);
                Assert.Same(container.GetService<默认值>(), container.GetService<默认值>());
                container.AddService<CtorAllOption>(ServiceLifetime.Singleton);
                Assert.Same(container.GetService<CtorAllOption>(),
                    container.GetService<CtorAllOption>());
                container.AddService<链式1>(ServiceLifetime.Singleton);
                Assert.Same(container.GetService<链式1>(), container.GetService<链式1>());
            }
        }

        [Fact]
        public void 循环依赖的类型()
        {
            var container = new Container();
            Assert.ThrowsAny<Exception>(() => container.GetService<LoopA>());
            var storage = container.GetStorage();
            Assert.Empty(storage.BuildInfos);
            Assert.Empty(storage.ScopedCache);
            Assert.Empty(storage.SingletonCache);
            Assert.Equal(3,
                storage.TransientServiceDescriptors.Count +
                storage.ScopedServiceDescriptors.Count +
                storage.SingletonServiceDescriptors.Count);
        }

        [Fact]
        public void 有依赖项的类型()
        {
            {
                var container = new Container();
                var service = container.GetService<链式1>();
                Assert.NotNull(service);
                Assert.IsType<链式1>(service);
                Assert.NotEqual(container.GetService<链式1>(),
                    container.GetService<链式1>());
                var storage = container.GetStorage();
                Assert.Equal(3, storage.TransientServiceDescriptors.Count);
                Assert.Equal(3, storage.BuildInfos.Count);
                Assert.Empty(storage.ScopedCache);
                Assert.Empty(storage.SingletonCache);
            }
            {
                var container = new Container();
                container.AddService<链式1>(ServiceLifetime.Scoped);
                var service = container.GetService<链式1>();
                Assert.NotNull(service);
                Assert.IsType<链式1>(service);
                Assert.Equal(container.GetService<链式1>(), container.GetService<链式1>());
                var storage = container.GetStorage();
                Assert.Equal(3,
                    storage.TransientServiceDescriptors.Count +
                    storage.ScopedServiceDescriptors.Count);
                Assert.Equal(3, storage.BuildInfos.Count);
                Assert.Single(storage.ScopedCache);
                Assert.Empty(storage.SingletonCache);
            }
            {
                var container = new Container();
                container.AddService<链式1>(ServiceLifetime.Singleton);
                var service = container.GetService<链式1>();
                Assert.NotNull(service);
                Assert.IsType<链式1>(service);
                Assert.Equal(container.GetService<链式1>(), container.GetService<链式1>());
                var storage = container.GetStorage();
                Assert.Equal(3,
                    storage.SingletonServiceDescriptors.Count +
                    storage.TransientServiceDescriptors.Count);
                Assert.Equal(3, storage.BuildInfos.Count);
                Assert.Empty(storage.ScopedCache);
                Assert.Single(storage.SingletonCache);
            }
        }

        [Fact]
        public void 构造函数全是可选参数的类型()
        {
            {
                var container = new Container();
                var service = container.GetService<CtorAllOption>();
                Assert.NotNull(service);
                Assert.IsType<CtorAllOption>(service);
                Assert.Null(service.链式2);
                Assert.Equal(1, service.I);
                Assert.Equal(11, service.I1);
                Assert.Equal(12, service.I2);
                Assert.Null(service.ID);
                Assert.NotEqual(container.GetService<CtorAllOption>(),
                    container.GetService<CtorAllOption>());
            }
            {
                var container = new Container();
                container.AddService<CtorAllOption>(ServiceLifetime.Scoped);
                var service = container.GetService<CtorAllOption>();
                Assert.NotNull(service);
                Assert.IsType<CtorAllOption>(service);
                Assert.Null(service.链式2);
                Assert.Equal(1, service.I);
                Assert.Equal(container.GetService<CtorAllOption>(),
                    container.GetService<CtorAllOption>());
            }
            {
                var container = new Container();
                container.AddService<CtorAllOption>(ServiceLifetime.Singleton);
                var service = container.GetService<CtorAllOption>();
                Assert.NotNull(service);
                Assert.IsType<CtorAllOption>(service);
                Assert.Null(service.链式2);
                Assert.Equal(1, service.I);
                Assert.Equal(container.GetService<CtorAllOption>(),
                    container.GetService<CtorAllOption>());
            }
        }

        [Fact]
        public void 获取特殊类型()
        {
            var container = new Container();
            Assert.Equal(0, container.GetService(typeof(int)));
            Assert.Equal(0, container.GetService<double>());
            Assert.Null(container.GetService<Task>());
            Assert.Null(container.GetService<Task<int>>());
        }

        [Fact]
        public void 指定实现工厂()
        {
            {
                var container = new Container();
                container.AddService<IA, A>(_ => new A());
                var service = container.GetService(typeof(IA));
                Assert.IsType<A>(service);
            }
            {
                var container = new Container();
                container.AddService<IA, A>(_ => new A(), ServiceLifetime.Singleton);
                var service = container.GetService(typeof(IA));
                var service1 = container.GetService(typeof(IA));
                Assert.IsType<A>(service);
                Assert.Equal(service1, service);
            }
            {
                var container = new Container();
                container.AddService<IA, A>(_ => new A(), ServiceLifetime.Scoped);
                var service = container.GetService(typeof(IA));
                var service1 = container.GetService(typeof(IA));
                Assert.IsType<A>(service);
                Assert.Equal(service1, service);
            }
        }

        [Fact]
        public void 指定实现实例()
        {
            {
                var container = new Container();
                container.AddService<IA, A>(new A());
                var service = container.GetService<IA>();
                var service1 = container.GetService<IA>();
                Assert.IsType<A>(service);
                Assert.Equal(service1, service);
            }
            {
                var container = new Container();
                container.AddService(new A(), ServiceLifetime.Singleton);
                var service = container.GetService<A>();
                var service1 = container.GetService<A>();
                Assert.IsType<A>(service);
                Assert.Equal(service1, service);
            }
            {
                Assert.ThrowsAny<Exception>(() =>
                    new Container().AddService(new A(), ServiceLifetime.Transient));
            }
        }

        [Fact]
        public void 指定构造函数无形参的类型()
        {
            {
                var container = new Container();
                var service = container.GetService<A>();
                Assert.NotNull(service);
                Assert.IsType<A>(service);
            }
            {
                var container = new Container();
                container.AddService<A>(ServiceLifetime.Scoped);
                var service = container.GetService<A>();
                Assert.NotNull(service);
                Assert.IsType<A>(service);
                Assert.Equal(container.GetService<A>(), container.GetService<A>());
            }
            {
                var container = new Container();
                container.AddService<A>(ServiceLifetime.Singleton);
                var service = container.GetService<A>();
                Assert.NotNull(service);
                Assert.IsType<A>(service);
                Assert.Equal(container.GetService<A>(), container.GetService<A>());
            }
            {
                var container = new Container();
                var service = container.GetService<链式2>();
                Assert.NotNull(service);
                Assert.IsType<链式2>(service);
            }
        }
    }
}
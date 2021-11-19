using TestProject.测试数据;
using Xunit;
using Zt.Containers;
using Zt.Containers.Logic.DataStructures;
using Zt.Containers.Logic.Extensions;

namespace TestProject
{
    public class TestSubContainer
    {
        [Fact]
        public void 释放子容器时()
        {
            var container = new Container();
            {
                container.AddService<A>(ServiceLifetime.Singleton);
                container.AddService<A>(ServiceLifetime.Scoped, "Scoped");
                container.AddService<A>(ServiceLifetime.Transient, "Transient");
                var ss = container.GetService<A>();
                var ss1 = container.GetService<A>();
                var sc = container.GetService<A>("Scoped");
                var sc1 = container.GetService<A>("Scoped");
                var st = container.GetService<A>("Transient");
                var st1 = container.GetService<A>("Transient");
                var subContainer = container.CreateSubContainer();
                var s1s = subContainer.GetService<A>();
                var s1s1 = subContainer.GetService<A>();
                var s1c = subContainer.GetService<A>("Scoped");
                var s1c1 = subContainer.GetService<A>("Scoped");
                var s1t = subContainer.GetService<A>("Transient");
                var s1t1 = subContainer.GetService<A>("Transient");
                Assert.Same(ss, ss1);
                Assert.Same(s1s, s1s1);
                Assert.Same(ss, s1s);
                Assert.Same(sc, sc1);
                Assert.Same(s1c, s1c1);
                Assert.NotSame(sc, s1c);
                Assert.NotSame(st, st1);
                Assert.NotSame(s1t, s1t1);
                Assert.NotSame(st, s1t);
                subContainer.Dispose();
                var storage = container.GetStorage();
                Assert.Equal(3, storage.BuildInfos.Count);
                Assert.Single(storage.ScopedServiceDescriptors);
                Assert.Single(storage.SingletonServiceDescriptors);
                Assert.Single(storage.TransientServiceDescriptors);
                Assert.Single(storage.ScopedCache);
                Assert.Single(storage.SingletonCache);
                var subStorage = subContainer.GetStorage();
                Assert.Empty(subStorage.BuildInfos);
                Assert.Empty(subStorage.ScopedServiceDescriptors);
                Assert.Single(subStorage.SingletonServiceDescriptors);
                Assert.Empty(subStorage.TransientServiceDescriptors);
                Assert.Empty(subStorage.ScopedCache);
                Assert.Single(subStorage.SingletonCache);
            }
        }

        [Fact]
        public void 子容器创建()
        {
            var container = new Container();
            Assert.Empty(container.Children);
            var subContainer = container.CreateSubContainer();
            Assert.Single(container.Children);
            Assert.Equal(subContainer.Parent, container);
            Assert.Null(container.Parent);
        }

        [Fact]
        public void 子容器的存储构建_有Scoped注册时()
        {
            {
                var container = new Container();
                container.AddService<链式1>(ServiceLifetime.Scoped);
                container.GetService<链式1>();
                var subContainer = container.CreateSubContainer();
                var storage = container.GetStorage();
                var subStorage = subContainer.GetStorage();
                Assert.NotEqual(storage.BuildInfos.Count, subStorage.BuildInfos.Count);
                Assert.NotSame(storage.BuildInfos, subStorage.BuildInfos);
                Assert.Same(storage.SingletonServiceDescriptors,
                    subStorage.SingletonServiceDescriptors);
                Assert.Equal(storage.SingletonServiceDescriptors.Count,
                    subStorage.SingletonServiceDescriptors.Count);
                Assert.NotSame(storage.ScopedServiceDescriptors,
                    subStorage.ScopedServiceDescriptors);
                Assert.Equal(storage.ScopedServiceDescriptors.Count,
                    subStorage.ScopedServiceDescriptors.Count);
                Assert.NotSame(storage.TransientServiceDescriptors,
                    subStorage.TransientServiceDescriptors);
                Assert.Equal(storage.TransientServiceDescriptors.Count,
                    subStorage.TransientServiceDescriptors.Count);
                Assert.Equal(storage.SingletonCache.Count,
                    subStorage.SingletonCache.Count);
                Assert.NotEqual(storage.ScopedCache.Count,
                    subStorage.ScopedCache.Count);
            }
            {
                var container = new Container();
                var subContainer = container.CreateSubContainer();
                container.AddService<链式1>(ServiceLifetime.Scoped);
                container.GetService<链式1>();
                var storage = container.GetStorage();
                var subStorage = subContainer.GetStorage();
                Assert.NotEqual(storage.BuildInfos.Count, subStorage.BuildInfos.Count);
                Assert.NotSame(storage.BuildInfos, subStorage.BuildInfos);
                Assert.Same(storage.SingletonServiceDescriptors,
                    subStorage.SingletonServiceDescriptors);
                Assert.Equal(storage.SingletonServiceDescriptors.Count,
                    subStorage.SingletonServiceDescriptors.Count);
                Assert.NotSame(storage.ScopedServiceDescriptors,
                    subStorage.ScopedServiceDescriptors);
                Assert.NotEqual(storage.ScopedServiceDescriptors.Count,
                    subStorage.ScopedServiceDescriptors.Count);
                Assert.NotSame(storage.TransientServiceDescriptors,
                    subStorage.TransientServiceDescriptors);
                Assert.NotEqual(storage.TransientServiceDescriptors.Count,
                    subStorage.TransientServiceDescriptors.Count);
                Assert.Equal(storage.SingletonCache.Count,
                    subStorage.SingletonCache.Count);
                Assert.NotEqual(storage.ScopedCache.Count,
                    subStorage.ScopedCache.Count);
            }
        }
        [Fact]
        public void 子容器的存储构建_有Singleton注册时()
        {
            {
                var container = new Container();
                container.AddService<链式1>(ServiceLifetime.Singleton);
                container.GetService<链式1>();
                var subContainer = container.CreateSubContainer();
                var storage = container.GetStorage();
                var subStorage = subContainer.GetStorage();
                Assert.NotEqual(storage.BuildInfos.Count, subStorage.BuildInfos.Count);
                Assert.NotSame(storage.BuildInfos, subStorage.BuildInfos);
                Assert.Same(storage.SingletonServiceDescriptors,
                    subStorage.SingletonServiceDescriptors);
                Assert.Equal(storage.SingletonServiceDescriptors.Count,
                    subStorage.SingletonServiceDescriptors.Count);
                Assert.NotSame(storage.ScopedServiceDescriptors,
                    subStorage.ScopedServiceDescriptors);
                Assert.Equal(storage.ScopedServiceDescriptors.Count,
                    subStorage.ScopedServiceDescriptors.Count);
                Assert.NotSame(storage.TransientServiceDescriptors,
                    subStorage.TransientServiceDescriptors);
                Assert.Equal(storage.TransientServiceDescriptors.Count,
                    subStorage.TransientServiceDescriptors.Count);
                Assert.Equal(storage.SingletonCache.Count,
                    subStorage.SingletonCache.Count);
                Assert.Equal(storage.ScopedCache.Count, subStorage.ScopedCache.Count);
            }
            {
                var container = new Container();
                var subContainer = container.CreateSubContainer();
                container.AddService<链式1>(ServiceLifetime.Singleton);
                container.GetService<链式1>();
                var storage = container.GetStorage();
                var subStorage = subContainer.GetStorage();
                Assert.NotEqual(storage.BuildInfos.Count, subStorage.BuildInfos.Count);
                Assert.NotSame(storage.BuildInfos, subStorage.BuildInfos);
                Assert.Same(storage.SingletonServiceDescriptors,
                    subStorage.SingletonServiceDescriptors);
                Assert.Equal(storage.SingletonServiceDescriptors.Count,
                    subStorage.SingletonServiceDescriptors.Count);
                Assert.NotSame(storage.ScopedServiceDescriptors,
                    subStorage.ScopedServiceDescriptors);
                Assert.Equal(storage.ScopedServiceDescriptors.Count,
                    subStorage.ScopedServiceDescriptors.Count);
                Assert.NotSame(storage.TransientServiceDescriptors,
                    subStorage.TransientServiceDescriptors);
                Assert.NotEqual(storage.TransientServiceDescriptors.Count,
                    subStorage.TransientServiceDescriptors.Count);
                Assert.Equal(storage.SingletonCache.Count,
                    subStorage.SingletonCache.Count);
                Assert.Equal(storage.ScopedCache.Count, subStorage.ScopedCache.Count);
            }
        }
        [Fact]
        public void 子容器的存储构建_有Transient注册时()
        {
            {
                var container = new Container();
                container.AddService<链式1>();
                container.GetService<链式1>();
                var subContainer = container.CreateSubContainer();
                var storage = container.GetStorage();
                var subStorage = subContainer.GetStorage();
                Assert.NotEqual(storage.BuildInfos.Count, subStorage.BuildInfos.Count);
                Assert.NotSame(storage.BuildInfos, subStorage.BuildInfos);
                Assert.Same(storage.SingletonServiceDescriptors,
                    subStorage.SingletonServiceDescriptors);
                Assert.Equal(storage.SingletonServiceDescriptors.Count,
                    subStorage.SingletonServiceDescriptors.Count);
                Assert.NotSame(storage.ScopedServiceDescriptors,
                    subStorage.ScopedServiceDescriptors);
                Assert.Equal(storage.ScopedServiceDescriptors.Count,
                    subStorage.ScopedServiceDescriptors.Count);
                Assert.NotSame(storage.TransientServiceDescriptors,
                    subStorage.TransientServiceDescriptors);
                Assert.Equal(storage.TransientServiceDescriptors.Count,
                    subStorage.TransientServiceDescriptors.Count);
                Assert.Equal(storage.SingletonCache.Count,
                    subStorage.SingletonCache.Count);
                Assert.Equal(storage.ScopedCache.Count, subStorage.ScopedCache.Count);
            }
            {
                var container = new Container();
                var subContainer = container.CreateSubContainer();
                container.AddService<链式1>();
                container.GetService<链式1>();
                var storage = container.GetStorage();
                var subStorage = subContainer.GetStorage();
                Assert.NotEqual(storage.BuildInfos.Count, subStorage.BuildInfos.Count);
                Assert.NotSame(storage.BuildInfos, subStorage.BuildInfos);
                Assert.Same(storage.SingletonServiceDescriptors,
                    subStorage.SingletonServiceDescriptors);
                Assert.Equal(storage.SingletonServiceDescriptors.Count,
                    subStorage.SingletonServiceDescriptors.Count);
                Assert.NotSame(storage.ScopedServiceDescriptors,
                    subStorage.ScopedServiceDescriptors);
                Assert.Equal(storage.ScopedServiceDescriptors.Count,
                    subStorage.ScopedServiceDescriptors.Count);
                Assert.NotSame(storage.TransientServiceDescriptors,
                    subStorage.TransientServiceDescriptors);
                Assert.NotEqual(storage.TransientServiceDescriptors.Count,
                    subStorage.TransientServiceDescriptors.Count);
                Assert.Equal(storage.SingletonCache.Count,
                    subStorage.SingletonCache.Count);
                Assert.Equal(storage.ScopedCache.Count, subStorage.ScopedCache.Count);
            }
        }
    }
}
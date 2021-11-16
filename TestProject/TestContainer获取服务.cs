using System;
using System.Threading.Tasks;
using IocContainer;
using IocContainer.Logic.DataStructures;
using IocContainer.Logic.Extensions;
using TestProject.��������;
using Xunit;

namespace TestProject
{
    public class TestContainer��ȡ����
    {
       
        [Fact]
        public void Test_ѭ������������()
        {
            var container = new Container();
            Assert.ThrowsAny<Exception>(() => container.GetService<LoopA>());
            var storage = container.GetStorage();
            Assert.Empty(storage.BuildInfos);
            Assert.Empty(storage.ScopedCache);
            Assert.Empty(storage.SingletonCache);
            Assert.Equal(3, storage.ServiceDescriptors.Count);
        }

        [Fact]
        public void Test_�������������()
        {
            {
                var container = new Container();
                var service = container.GetService<��ʽ1>();
                Assert.NotNull(service);
                Assert.IsType<��ʽ1>(service);
                Assert.NotEqual(container.GetService<��ʽ1>(),
                    container.GetService<��ʽ1>());
                var storage = container.GetStorage();
                Assert.Equal(3, storage.ServiceDescriptors.Count);
                Assert.Equal(3, storage.BuildInfos.Count);
                Assert.Empty(storage.ScopedCache);
                Assert.Empty(storage.SingletonCache);
            }
            {
                var container = new Container();
                container.AddService<��ʽ1>(ServiceLifetime.Scoped);
                var service = container.GetService<��ʽ1>();
                Assert.NotNull(service);
                Assert.IsType<��ʽ1>(service);
                Assert.Equal(container.GetService<��ʽ1>(), container.GetService<��ʽ1>());
                var storage = container.GetStorage();
                Assert.Equal(3, storage.ServiceDescriptors.Count);
                Assert.Equal(3, storage.BuildInfos.Count);
                Assert.Single(storage.ScopedCache);
                Assert.Empty(storage.SingletonCache);
            }
            {
                var container = new Container();
                container.AddService<��ʽ1>(ServiceLifetime.Singleton);
                var service = container.GetService<��ʽ1>();
                Assert.NotNull(service);
                Assert.IsType<��ʽ1>(service);
                Assert.Equal(container.GetService<��ʽ1>(), container.GetService<��ʽ1>());
                var storage = container.GetStorage();
                Assert.Equal(3, storage.ServiceDescriptors.Count);
                Assert.Equal(3, storage.BuildInfos.Count);
                Assert.Empty(storage.ScopedCache);
                Assert.Single(storage.SingletonCache);
            }
        }

        [Fact]
        public void Test_���캯��ȫ�ǿ�ѡ����������()
        {
            {
                var container = new Container();
                var service = container.GetService<CtorAllOption>();
                Assert.NotNull(service);
                Assert.IsType<CtorAllOption>(service);
                Assert.Null(service.��ʽ2);
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
                Assert.Null(service.��ʽ2);
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
                Assert.Null(service.��ʽ2);
                Assert.Equal(1, service.I);
                Assert.Equal(container.GetService<CtorAllOption>(),
                    container.GetService<CtorAllOption>());
            }
        }

        [Fact]
        public void Test_��ȡ��������()
        {
            var container = new Container();
            Assert.Equal(0, container.GetService(typeof(int)));
            Assert.Equal(0, container.GetService<double>());
            Assert.Null(container.GetService<Task>());
            Assert.Null(container.GetService<Task<int>>());
        }

        [Fact]
        public void Test_ָ��ʵ�ֹ���()
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
        public void Test_ָ��ʵ��ʵ��()
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
        public void Test_ָ�����캯�����βε�����()
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
                var service = container.GetService<��ʽ2>();
                Assert.NotNull(service);
                Assert.IsType<��ʽ2>(service);
            }
        }
    }
}
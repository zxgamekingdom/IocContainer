using System;

namespace IocContainer.Logic.DataStructures
{
    /// <summary>
    /// 服务描述文件
    /// </summary>
    public record ServiceDescriptor
    {
        /// <summary>
        /// 服务类型
        /// </summary>
        public Type ServiceType { get; internal set; } = null!;
        /// <summary>
        /// 实现类型
        /// </summary>
        public Type ImplementationType { get; internal set; } = null!;
        /// <summary>
        /// 实现实例
        /// </summary>
        public object? ImplementationInstance { get; internal set; }
        /// <summary>
        /// 实现工厂
        /// </summary>
        public Func<Container, object>? ImplementationFactory { get; internal set; }
        /// <summary>
        /// 键
        /// </summary>
        public object ServiceKey { get; internal set; } = null!;
        /// <summary>
        /// 生命周期
        /// </summary>
        public ServiceLifetime Lifetime { get; internal set; }
        public bool IsCanHandlSpecialType { get; internal set; }

        public override string ToString()
        {
            return
                $"{nameof(ServiceType)}: {ServiceType}, {nameof(ImplementationType)}: {ImplementationType}, {nameof(ServiceKey)}: {ServiceKey}, {nameof(Lifetime)}: {Lifetime}, {nameof(IsCanHandlSpecialType)}: {IsCanHandlSpecialType}";
        }
    }

    /// <summary>
    /// 服务描述文件
    /// </summary>
    public record ServiceDescriptor<TService, TImplementation>
        where TImplementation : TService
    {
        /// <summary>
        /// 初始化 生命周期,键
        /// </summary>
        /// <param name="ifetime"></param>
        /// <param name="serviceKey"></param>
        public ServiceDescriptor(ServiceLifetime ifetime = ServiceLifetime.Transient,
            object? serviceKey = null)
        {
            Lifetime = ifetime;
            ServiceKey = serviceKey;
        }

        /// <summary>
        /// 初始化实现工厂
        /// </summary>
        /// <param name="implementationFactory"></param>
        /// <param name="lifeTime"></param>
        /// <param name="serviceKey"></param>
        public ServiceDescriptor(Func<Container, TImplementation> implementationFactory,
            ServiceLifetime lifeTime = ServiceLifetime.Transient,
            object? serviceKey = null) : this(lifeTime, serviceKey)
        {
            ImplementationFactory = implementationFactory;
        }

        /// <summary>
        /// 初始化实现实例
        /// </summary>
        public ServiceDescriptor(TImplementation implementationInstance,
            ServiceLifetime lifetime = ServiceLifetime.Singleton,
            object? serviceKey = null) : this(lifetime, serviceKey)
        {
            //检查生命周期是不是单例或者局部
            if (lifetime != ServiceLifetime.Singleton &&
                lifetime != ServiceLifetime.Scoped)
            {
                throw new ArgumentException("实现实例必须是单例实例或作用域实例");
            }

            ImplementationInstance = implementationInstance;
        }

        /// <summary>
        /// 服务类型
        /// </summary>
        public Type ServiceType => typeof(TService);
        /// <summary>
        /// 实现类型
        /// </summary>
        public Type ImplementationType => typeof(TImplementation);
        /// <summary>
        /// 实现实例
        /// </summary>
        public object? ImplementationInstance { get; internal set; }
        /// <summary>
        /// 实现工厂
        /// </summary>
        public Func<Container, TImplementation>? ImplementationFactory
        {
            get;
            internal set;
        }
        /// <summary>
        /// 键
        /// </summary>
        public object? ServiceKey { get; internal set; }
        /// <summary>
        /// 生命周期
        /// </summary>
        public ServiceLifetime Lifetime { get; internal set; }
    }
}
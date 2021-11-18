using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zt.Containers.Attributes;
using Zt.Containers.Logic.DataStructures;
using Zt.Containers.Logic.Extensions;
using static System.Linq.Expressions.Expression;
using IocExpression = System.Linq.Expressions.Expression<System.Func<object>>;
using IocGetFromCacheExpression =
    System.Linq.Expressions.Expression<System.Func<
        Zt.Containers.Logic.DataStructures.ContainerStorage,
        Zt.Containers.Logic.DataStructures.ServiceDescriptor, object?>>;
using IocAddToCacheExpression =
    System.Linq.Expressions.Expression<System.Action<
        Zt.Containers.Logic.DataStructures.ContainerStorage,
        Zt.Containers.Logic.DataStructures.ServiceDescriptor, object>>;

namespace Zt.Containers.Logic
{
    public readonly struct ContainerInstanceBuilder
    {
        public Container Container { get; }
        public ContainerStorage Storage => Container.Storage;

        public ContainerInstanceBuilder(Container container)
        {
            Container = container;
        }

        public object GetService(Type serviceType, object rawKey)
        {
            var serviceDescriptor = 寻找或者创建ServiceDescriptor(serviceType, rawKey);
            var buildInfo = 寻找或者创建BuildInfo(serviceDescriptor);

            return buildInfo.BuildFunc.Invoke();
        }

        public ContainerInstanceBuildInfo 寻找或者创建BuildInfo(
            ServiceDescriptor serviceDescriptor)
        {
            var buildInfo = Storage.GetBuildInfo(serviceDescriptor);

            return buildInfo ??
                创建普通类型的BuildInfo(serviceDescriptor, out var constructorInfo) ??
                创建有依赖项类型的BuildInfo(serviceDescriptor, constructorInfo!);
        }

        public ContainerInstanceBuildInfo? 创建普通类型的BuildInfo(
            ServiceDescriptor serviceDescriptor,
            out ConstructorInfo? constructorInfo)
        {
            constructorInfo = default;
            ContainerInstanceBuildInfo? buildInfo;

            if (serviceDescriptor.IsCanHandlSpecialType)
            {
                buildInfo = 创建特殊类型的BuildInfo(serviceDescriptor);

                goto END;
            }

            if (serviceDescriptor.ImplementationInstance != null)
            {
                buildInfo = 创建ImplementationInstance不为null的BuildInfo(serviceDescriptor);

                goto END;
            }

            if (serviceDescriptor.ImplementationFactory != null)
            {
                buildInfo = 创建ImplementationFactory不为null的BuildInfo(serviceDescriptor);

                goto END;
            }

            buildInfo = 创建无依赖项类型的BuildInfo(serviceDescriptor, out constructorInfo);
            END:

            if (buildInfo == null) return default;

            Storage.AddBuildInfo(serviceDescriptor, buildInfo);

            return buildInfo;
        }

        public ContainerInstanceBuildInfo? 创建无依赖项类型的BuildInfo(
            ServiceDescriptor serviceDescriptor,
            out ConstructorInfo constructorInfo)
        {
            constructorInfo = 选择构造函数(serviceDescriptor);
            var parameterInfos = constructorInfo.GetParameters();
            var length = parameterInfos.Length;

            if (length == 0)
            {
                return 为构造函数形参个数为0的类型创建BuildInfo(serviceDescriptor, constructorInfo);
            }

            //选择必选参数
            var requiredParameterInfos =
                parameterInfos.Where(p => !p.IsOptional).ToArray();
            var requiredLength = requiredParameterInfos.Length;

            if (requiredLength == 0)
            {
                return 为构造函数形参必选参数个数为0的类型创建BuildInfo(serviceDescriptor,
                    constructorInfo);
            }

            return null;
        }

        public ContainerInstanceBuildInfo 为构造函数形参必选参数个数为0的类型创建BuildInfo(
            ServiceDescriptor serviceDescriptor,
            ConstructorInfo constructorInfo)
        {
            var buildInfo = new ContainerInstanceBuildInfo(serviceDescriptor, Storage)
            {
                选中的构造函数 = constructorInfo
            };
            List<Expression> keyExpressions = new();
            List<ParameterExpression> parameterExpressions = new();
            var value = Variable(serviceDescriptor.ServiceType);
            var parameterInfos = constructorInfo.GetParameters();
            // ReSharper disable once CoVariantArrayConversion
            Expression[] optionalExpressions = parameterInfos.Where(p => p.IsOptional)
                .Select(info =>
                    Convert(info.RawDefaultValue.Constant(), info.ParameterType))
                .ToArray();

            if (serviceDescriptor.Lifetime is ServiceLifetime.Scoped or ServiceLifetime
                .Singleton)
            {
                var buff = Variable(typeof(object));
                parameterExpressions.Add(buff);
                IocGetFromCacheExpression getFromCache = (storage, descriptor) =>
                    storage.GetInstanceFromCache(descriptor);
                keyExpressions.Add(Assign(buff,
                    Invoke(getFromCache,
                        Storage.Constant(),
                        serviceDescriptor.Constant())));
                IocAddToCacheExpression addToCache = (storage, descriptor, instance) =>
                    storage.AddInstanceToCache(descriptor, instance);
                keyExpressions.Add(IfThenElse(Equal(buff, Constant(null)),
                    Block(Assign(value, New(constructorInfo, optionalExpressions)),
                        Invoke(addToCache,
                            Storage.Constant(),
                            serviceDescriptor.Constant(),
                            value)),
                    Block(Assign(value, Convert(buff, value.Type)))));
            }
            else
            {
                keyExpressions.Add(Assign(value,
                    New(constructorInfo, optionalExpressions)));
            }

            var (@return, end) = CreateEndExpression(value);
            var block = Block(parameterExpressions.Concat(new[] {value,}),
                keyExpressions.Concat(new Expression[] {@return, end}));
            IocExpression expression = Lambda<Func<object>>(block);
            var compile = expression.Compile();
            buildInfo.Expression = expression;
            buildInfo.BuildFunc = compile;
            buildInfo.Variable = value;
            buildInfo.InternalVariable = parameterExpressions.ToArray();
            buildInfo.KeyExpressions = keyExpressions.ToArray();

            return buildInfo;
        }

        public ContainerInstanceBuildInfo 创建有依赖项类型的BuildInfo(
            ServiceDescriptor serviceDescriptor,
            ConstructorInfo constructorInfo)
        {
            return new 有依赖项类型的BuildInfoBuilder(this, serviceDescriptor, constructorInfo)
                .Build();
        }

        public ContainerInstanceBuildInfo 为构造函数形参个数为0的类型创建BuildInfo(
            ServiceDescriptor serviceDescriptor,
            ConstructorInfo constructorInfo)
        {
            var buildInfo = new ContainerInstanceBuildInfo(serviceDescriptor, Storage)
            {
                选中的构造函数 = constructorInfo
            };
            var value = Variable(buildInfo.ServiceDescriptor.ServiceType);
            List<Expression> keyExpressions = new();
            var variables = new List<ParameterExpression>();

            if (serviceDescriptor.Lifetime is ServiceLifetime.Scoped or ServiceLifetime
                .Singleton)
            {
                var buff = Variable(typeof(object));
                IocGetFromCacheExpression getFromCache =
                    (containerStorage, descriptor) =>
                        containerStorage.GetInstanceFromCache(descriptor);
                keyExpressions.Add(Assign(buff,
                    Invoke(getFromCache,
                        Storage.Constant(),
                        serviceDescriptor.Constant())));
                IocAddToCacheExpression addToCache =
                    (containerStorage, descriptor, instance) =>
                        containerStorage.AddInstanceToCache(descriptor, instance);
                keyExpressions.Add(IfThenElse(Equal(buff, Constant(null)),
                    Block(Assign(value, New(buildInfo.ServiceDescriptor.ServiceType)),
                        Invoke(addToCache,
                            Storage.Constant(),
                            serviceDescriptor.Constant(),
                            Convert(value, typeof(object)))),
                    Block(Assign(value, Convert(buff, value.Type)))));
                variables.Add(buff);
            }
            else
            {
                keyExpressions.Add(Assign(value, New(value.Type)));
            }

            var (@return, end) = CreateEndExpression(value);
            BlockExpression block = Block(variables.Concat(new[] {value,}),
                keyExpressions.Concat(new Expression[] {@return, end,}));
            IocExpression expression = Lambda<Func<object>>(block);
            var compile = expression.Compile();
            buildInfo.Expression = expression;
            buildInfo.BuildFunc = compile;
            buildInfo.Variable = value;
            buildInfo.KeyExpressions = keyExpressions.ToArray();

            return buildInfo;
        }

        public ConstructorInfo 选择构造函数(ServiceDescriptor serviceDescriptor)
        {
            var implementationType = serviceDescriptor.ImplementationType;

            //如果是抽象或者接口，则抛出异常
            if (implementationType.IsAbstract || implementationType.IsInterface)
            {
                throw new InvalidOperationException($@"{implementationType
                }是抽象类型或者接口，不能实例化");
            }

            var constructorInfos = implementationType.GetConstructors();
            //找到带有SelectedConstructor特性的构造函数
            var selectedConstructor = constructorInfos.FirstOrDefault(x =>
                x.GetCustomAttribute<SelectedConstructorAttribute>() != null);

            if (selectedConstructor != null)
            {
                return selectedConstructor;
            }

            //按照构造函数的参数个数降序排序并选择第一个构造函数
            selectedConstructor = constructorInfos
                .OrderByDescending(x => x.GetParameters().Length)
                .FirstOrDefault();

            if (selectedConstructor != null) return selectedConstructor;

            throw new InvalidOperationException($@"无法为{serviceDescriptor}挑选合适的构造函数");
        }

        public ContainerInstanceBuildInfo 创建ImplementationFactory不为null的BuildInfo(
            ServiceDescriptor serviceDescriptor)
        {
            var buildInfo = new ContainerInstanceBuildInfo(serviceDescriptor, Storage);
            var descriptor = buildInfo.ServiceDescriptor;
            var value = Variable(descriptor.ServiceType);
            List<ParameterExpression> variables = new();
            List<Expression> keyExpressions = new();

            if (descriptor.Lifetime is ServiceLifetime.Scoped or ServiceLifetime
                .Singleton)
            {
                var buff = Variable(typeof(object));
                IocGetFromCacheExpression getFromCache =
                    (containerStorage, descriptor1) =>
                        containerStorage.GetInstanceFromCache(descriptor1);
                var assignBuff = Assign(buff,
                    Invoke(getFromCache,
                        Constant(buildInfo.Storage),
                        Constant(descriptor)));
                IocAddToCacheExpression addToCache =
                    (containerStorage, descriptor1, value1) =>
                        containerStorage.AddInstanceToCache(descriptor1, value1);
                var ifThen = IfThen(Equal(buff, Constant(null)),
                    Block(Assign(buff,
                            Constant(
                                descriptor.ImplementationFactory!.Invoke(buildInfo
                                    .Storage
                                    .Container))),
                        Invoke(addToCache,
                            Constant(buildInfo.Storage),
                            Constant(descriptor),
                            buff)));
                var assignValue = Assign(value, Convert(buff, value.Type));
                variables.Add(buff);
                keyExpressions.AddRange(new Expression[]
                {
                    assignBuff, ifThen, assignValue
                });
            }
            else
            {
                Expression<Func<ServiceDescriptor, ContainerStorage, object>>
                    implementationFactory = (descriptor1, storage) =>
                        descriptor1.ImplementationFactory!.Invoke(storage.Container);
                var assign = Assign(value,
                    Convert(Invoke(implementationFactory,
                            serviceDescriptor.Constant(),
                            Storage.Constant()),
                        value.Type));
                keyExpressions.Add(assign);
            }

            var parameterExpressions = variables.Concat(new[] {value,}).ToArray();
            var (@return, end) = CreateEndExpression(value);
            var block = Block(parameterExpressions,
                keyExpressions.Concat(new Expression[] {@return, end}).ToArray());
            IocExpression expression = Lambda<Func<object>>(block);
            var compile = expression.Compile();
            buildInfo.Expression = expression;
            buildInfo.Variable = value;
            buildInfo.InternalVariable = variables.ToArray();
            buildInfo.BuildFunc = compile;
            buildInfo.KeyExpressions = keyExpressions.ToArray();

            return buildInfo;
        }

        public ContainerInstanceBuildInfo 创建ImplementationInstance不为null的BuildInfo(
            ServiceDescriptor serviceDescriptor)
        {
            //生命周期必须为Singleton或者Scoped
            if (serviceDescriptor.Lifetime != ServiceLifetime.Singleton &&
                serviceDescriptor.Lifetime != ServiceLifetime.Scoped)
            {
                throw new InvalidOperationException($@"{serviceDescriptor
                }的生命周期必须为Singleton或者Scoped");
            }

            var buildInfo = new ContainerInstanceBuildInfo(serviceDescriptor, Storage);
            var descriptor = buildInfo.ServiceDescriptor;
            var value = Variable(descriptor.ServiceType);
            var buff = Variable(typeof(object));
            IocGetFromCacheExpression getFromCache = (storage, descriptor1) =>
                storage.GetInstanceFromCache(descriptor1);
            IocAddToCacheExpression addToCache =
                (containerStorage, descriptor1, instance) =>
                    containerStorage.AddInstanceToCache(descriptor1, instance);
            var assignBuff = Assign(buff,
                Invoke(getFromCache,
                    Constant(buildInfo.Storage),
                    descriptor.Constant()));
            var ifThen = IfThen(Equal(buff, Constant(null)),
                Block(Assign(buff,
                        Constant(buildInfo.ServiceDescriptor.ImplementationInstance)),
                    Invoke(addToCache,
                        Constant(buildInfo.Storage),
                        Constant(descriptor),
                        buff)));
            var assignValue = Assign(value, Convert(buff, value.Type));
            var (@return, end) = CreateEndExpression(value);
            var block = Block(new[] {value, buff},
                assignBuff,
                ifThen,
                assignValue,
                @return,
                end);
            IocExpression expression = Lambda<Func<object>>(block);
            var compile = expression.Compile();
            buildInfo.InternalVariable = new[] {buff};
            buildInfo.Variable = value;
            buildInfo.Expression = expression;
            buildInfo.BuildFunc = compile;
            buildInfo.KeyExpressions =
                new Expression[] {assignBuff, ifThen, assignValue,};

            return buildInfo;
        }

        public ServiceDescriptor 寻找或者创建ServiceDescriptor(Type serviceType,
            object rawKey)
        {
            var descriptor = Storage.GetServiceDescriptor(serviceType, rawKey);

            return descriptor ?? 创建ServiceDescriptor(serviceType, rawKey);
        }

        public ServiceDescriptor 创建ServiceDescriptor(Type serviceType, object rawKey)
        {
            if (serviceType.是不是不能处理的特殊类型())
            {
                //参数错误，不能处理此特殊类型
                throw new ArgumentException($"{serviceType.FullName}不能处理的特殊类型");
            }

            ServiceDescriptor? descriptor;

            if (serviceType.是不是可以处理特殊类型())
            {
                descriptor = 创建特殊类型的ServiceDescriptor(serviceType, rawKey);
            }
            else
            {
                descriptor = new ServiceDescriptor
                {
                    Lifetime = ServiceLifetime.Transient,
                    ServiceKey = rawKey,
                    ServiceType = serviceType,
                    ImplementationType = serviceType
                };
            }

            Storage.AddService(descriptor);

            return descriptor;
        }

        public ServiceDescriptor 创建特殊类型的ServiceDescriptor(Type serviceType,
            object rawKey)
        {
            return new ServiceDescriptor
            {
                Lifetime = ServiceLifetime.Transient,
                ServiceKey = rawKey,
                ServiceType = serviceType,
                ImplementationType = serviceType,
                IsCanHandlSpecialType = true,
            };
        }

        public ContainerInstanceBuildInfo 创建特殊类型的BuildInfo(ServiceDescriptor descriptor)
        {
            var buildInfo = new ContainerInstanceBuildInfo(descriptor, Storage);
            var serviceType = buildInfo.ServiceDescriptor.ServiceType;
            var implementationType = buildInfo.ServiceDescriptor.ImplementationType;
            ParameterExpression value = Variable(serviceType);
            var assign = Assign(value, Default(implementationType));
            var (@return, end) = CreateEndExpression(value);
            var block = Block(new[] {value,}, assign, @return, end);
            IocExpression expression = Lambda<Func<object>>(block);
            var compile = expression.Compile();
            buildInfo.Variable = value;
            buildInfo.Expression = expression;
            buildInfo.KeyExpressions = new Expression[] {assign};
            buildInfo.BuildFunc = compile;

            return buildInfo;
        }

        public (GotoExpression @return, LabelExpression end) CreateEndExpression(
            Expression value)
        {
            var endLabel = Label(typeof(object));
            var end = Label(endLabel, Constant(null));
            var @return = Return(endLabel, Convert(value, typeof(object)));

            return (@return, end);
        }
    }
}
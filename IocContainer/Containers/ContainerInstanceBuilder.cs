using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;
using IocExpression = System.Linq.Expressions.Expression<System.Func<object>>;

namespace IocContainer.Containers
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

            if (!buildInfo.IsInitialized)
            {
                buildInfo.Precompile();
            }

            if (buildInfo.IsInitialized is false)
            {
                throw new InvalidOperationException($@"{serviceDescriptor}的构造信息还未初始化");
            }

            return buildInfo.BuildFunc.Invoke();
        }

        private ContainerInstanceBuildInfo 寻找或者创建BuildInfo(
            ServiceDescriptor serviceDescriptor)
        {
            var buildInfo = Storage.FindBuildInfo(serviceDescriptor);

            return buildInfo ?? 创建BuildInfo(serviceDescriptor);
        }

        private ContainerInstanceBuildInfo 创建BuildInfo(
            ServiceDescriptor serviceDescriptor)
        {
            ContainerInstanceBuildInfo? buildInfo = null;

            if (serviceDescriptor.IscanHandlSpecialType)
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

            END:

            if (buildInfo != null)
            {
                Storage.AddBuildInfo(serviceDescriptor, buildInfo);

                return buildInfo;
            }

            throw new NotImplementedException();
        }

        private ContainerInstanceBuildInfo 创建ImplementationFactory不为null的BuildInfo(
            ServiceDescriptor serviceDescriptor)
        {
            var buildInfo = new ContainerInstanceBuildInfo(serviceDescriptor, Storage);
            buildInfo.SetPrecompileFactory(info =>
            {
                var descriptor = info.ServiceDescriptor;
                var value = Variable(descriptor.ServiceType);
                List<ParameterExpression> variables = new();
                List<Expression> keyExpressions = new();

                if (descriptor.Lifetime is ServiceLifetime.Scoped or ServiceLifetime
                    .Singleton)
                {
                    var buff = Variable(typeof(object));
                    Expression<Func<ContainerStorage, ServiceDescriptor, object?>>
                        findFromCache = (storage, descriptor1) =>
                            storage.FindInstanceFromCache(descriptor1);
                    var assignBuff = Assign(buff,
                        Invoke(findFromCache,
                            Constant(info.Storage),
                            Constant(descriptor)));
                    Expression<Action<ContainerStorage, ServiceDescriptor, object>>
                        addToCache = (storage, descriptor1, value1) =>
                            storage.AddInstanceToCache(descriptor1, value1);
                    var ifThen = IfThen(Equal(buff, Constant(null)),
                        Block(Assign(buff,
                                Constant(
                                    descriptor.ImplementationFactory!.Invoke(info.Storage
                                        .Container))),
                            Invoke(addToCache,
                                Constant(info.Storage),
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
                    var assign = Assign(value,
                        Convert(Constant(
                                descriptor.ImplementationFactory!.Invoke(info.Storage
                                    .Container)),
                            value.Type));
                    keyExpressions.Add(assign);
                }

                var parameterExpressions =
                    Enumerable.Concat(variables, new[] {value,}).ToArray();
                var (@return, end) = CreateEndExpression(value);
                var block = Block(parameterExpressions,
                    keyExpressions.Concat(new Expression[] {@return, end}).ToArray());
                IocExpression expression = Lambda<Func<object>>(block);
                var compile = expression.Compile();
                info.Expression = expression;
                info.Variable = value;
                info.InternalVariable = variables.ToArray();
                info.BuildFunc = compile;
                info.KeyExpressions = keyExpressions.ToArray();
                info.IsInitialized = true;
            });

            return buildInfo;
        }

        private ContainerInstanceBuildInfo 创建ImplementationInstance不为null的BuildInfo(
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
            buildInfo.SetPrecompileFactory(info =>
            {
                var descriptor = info.ServiceDescriptor;
                var value = Variable(descriptor.ServiceType);
                var buff = Variable(typeof(object));
                Expression<Func<ContainerStorage, object?>> findFromCache =
                    storage => storage.FindInstanceFromCache(descriptor);
                Expression<Action<ContainerStorage, ServiceDescriptor, object>>
                    addToCache = (storage, descriptor1, instance) =>
                        storage.AddInstanceToCache(descriptor1, instance);
                var assignBuff = Assign(buff,
                    Invoke(findFromCache, Constant(info.Storage)));
                var ifThen = IfThen(Equal(buff, Constant(null)),
                    Block(Assign(buff,
                            Constant(info.ServiceDescriptor.ImplementationInstance)),
                        Invoke(addToCache,
                            Constant(info.Storage),
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
                info.InternalVariable = new[] {buff};
                info.Variable = value;
                info.Expression = expression;
                info.BuildFunc = compile;
                info.KeyExpressions =
                    new Expression[] {assignBuff, ifThen, assignValue,};
                info.IsInitialized = true;
            });

            return buildInfo;
        }

        private ServiceDescriptor 寻找或者创建ServiceDescriptor(Type serviceType,
            object rawKey)
        {
            var descriptor = Storage.FindServiceDescriptor(serviceType, rawKey);

            return descriptor ?? 创建ServiceDescriptor(serviceType, rawKey);
        }

        private ServiceDescriptor 创建ServiceDescriptor(Type serviceType, object rawKey)
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
                Storage.AddService(descriptor);
            }
            else
            {
                descriptor = new ServiceDescriptor()
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

        private ServiceDescriptor 创建特殊类型的ServiceDescriptor(Type serviceType,
            object rawKey)
        {
            return new ServiceDescriptor()
            {
                Lifetime = ServiceLifetime.Transient,
                ServiceKey = rawKey,
                ServiceType = serviceType,
                ImplementationType = serviceType,
                IscanHandlSpecialType = true,
            };
        }

        private ContainerInstanceBuildInfo 创建特殊类型的BuildInfo(
            ServiceDescriptor descriptor)
        {
            var buildInfo = new ContainerInstanceBuildInfo(descriptor, Storage);
            buildInfo.SetPrecompileFactory(info =>
            {
                var serviceType = info.ServiceDescriptor.ServiceType;
                var implementationType = info.ServiceDescriptor.ImplementationType;
                ParameterExpression value = Variable(serviceType);
                var assign = Assign(value, Default(implementationType));
                var (@return, end) = CreateEndExpression(value);
                var block = Block(new[] {value,}, assign, @return, end);
                IocExpression expression = Lambda<Func<object>>(block);
                var compile = expression.Compile();
                info.Variable = value;
                info.Expression = expression;
                info.KeyExpressions = new Expression[] {assign};
                info.BuildFunc = compile;
                info.IsInitialized = true;
            });

            return buildInfo;
        }

        private static (GotoExpression @return, LabelExpression end)
            CreateEndExpression(Expression value)
        {
            var endLabel = Label(typeof(object));
            var end = Label(endLabel, Constant(null));
            var @return = Return(endLabel, Convert(value, typeof(object)));

            return (@return, end);
        }
    }
}
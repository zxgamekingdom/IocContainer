using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;
using IocExpression = System.Linq.Expressions.Expression<System.Func<object>>;
using IocGetFromCacheExpression =
    System.Linq.Expressions.Expression<System.Func<
        IocContainer.Containers.ContainerStorage,
        IocContainer.Containers.ServiceDescriptor, object?>>;
using IocAddToCacheExpression =
    System.Linq.Expressions.Expression<System.Action<
        IocContainer.Containers.ContainerStorage,
        IocContainer.Containers.ServiceDescriptor, object>>;

namespace IocContainer.Containers
{
    public readonly struct 有依赖项类型的BuildInfoBuilder
    {
        record Node(Node? Parent,
            ServiceDescriptor ServiceDescriptor,
            ConstructorInfo? ConstructorInfo,
            ContainerInstanceBuilder Builder,
            ContainerInstanceBuildInfo? BuildInfo)
        {
            //深度 层级
            public int Depth => Parent?.Depth + 1 ?? 0;

            public void Init()
            {
                ParameterInfos = ConstructorInfo?.GetParameters();
                必选参数 = ParameterInfos?.Where(p => !p.IsOptional)
                    .Select(info => (info.ParameterType,
                        Key: info.GetCustomAttribute<KeyAttribute>()?.Key ??
                        NullKey.Instance))
                    .ToArray();
                可选参数 = ParameterInfos?.Where(p => p.IsOptional)
                    .Select(info => Convert(info.RawDefaultValue.Constant(),
                        info.ParameterType))
                    .ToArray();
                ChildServiceDescriptors = 必选参数?.ToDictionary(tuple => tuple,
                    tuple => Builder.寻找或者创建ServiceDescriptor(tuple.ParameterType,
                        tuple.Key));
                Children = ChildServiceDescriptors?.ToDictionary(pair => pair.Value,
                    pair =>
                    {
                        var serviceDescriptor = pair.Value;
                        ConstructorInfo? info = default;
                        var buildInfo =
                            Builder.Storage.GetBuildInfo(serviceDescriptor) ??
                            Builder.创建普通类型的BuildInfo(serviceDescriptor, out info);

                        return new Node(this,
                            serviceDescriptor,
                            info,
                            Builder,
                            buildInfo);
                    });
            }

            public Dictionary<ServiceDescriptor, Node>? Children { get; private set; }
            public Dictionary<(Type ParameterType, object Key), ServiceDescriptor>?
                ChildServiceDescriptors { get; private set; }
            public UnaryExpression[]? 可选参数 { get; private set; }
            public (Type ParameterType, object Key)[]? 必选参数 { get; private set; }
            // ReSharper disable once MemberCanBePrivate.Local
            public ParameterInfo[]? ParameterInfos { get; set; }
            public ContainerInstanceBuildInfo? BuildInfo { get; internal set; } =
                BuildInfo;
        }

        public ContainerInstanceBuilder ContainerInstanceBuilder { get; }
        public ServiceDescriptor ServiceDescriptor { get; }
        public ConstructorInfo ConstructorInfo { get; }

        public 有依赖项类型的BuildInfoBuilder(
            ContainerInstanceBuilder containerInstanceBuilder,
            ServiceDescriptor serviceDescriptor,
            ConstructorInfo constructorInfo)
        {
            ContainerInstanceBuilder = containerInstanceBuilder;
            ServiceDescriptor = serviceDescriptor;
            ConstructorInfo = constructorInfo;
        }

        public ContainerInstanceBuildInfo Build()
        {
            var root = 构建依赖树();
            var dictionary = 将依赖树展开(root);
            使用依赖树构建BuildInfo(dictionary);

            return root.BuildInfo!;
        }

        private void 使用依赖树构建BuildInfo(Dictionary<int, List<Node>> dictionary)
        {
            var count = dictionary.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                foreach (var node in dictionary[i])
                {
                    构建节点BuildInfo(in node);
                }
            }
        }

        private void 构建节点BuildInfo(in Node node)
        {
            if (node.BuildInfo != null)
            {
                return;
            }

            var nodeServiceDescriptor = node.ServiceDescriptor;
            var value = Variable(nodeServiceDescriptor.ServiceType);
            List<Expression> keyExpressions = new(10);
            List<ParameterExpression> parameterExpressions = new(10);

            if (nodeServiceDescriptor.Lifetime is ServiceLifetime.Scoped or
                ServiceLifetime.Singleton)
            {
                var buff = Variable(typeof(object));
                parameterExpressions.Add(buff);
                IocGetFromCacheExpression getFromCache = (storage, descriptor) =>
                    storage.GetInstanceFromCache(descriptor);
                keyExpressions.Add(Assign(buff,
                    Invoke(getFromCache,
                        node.Builder.Storage.Constant(),
                        nodeServiceDescriptor.Constant())));
                IocAddToCacheExpression addToCache = (storage, descriptor, instance) =>
                    storage.AddInstanceToCache(descriptor, instance);
                keyExpressions.Add(IfThenElse(NotEqual(buff, Constant(null)),
                    Block(Assign(value, Convert(buff, value.Type))),
                    Block(获取所有子节点的关键表达式步骤(node)
                        .Concat(new Expression[]
                        {
                            Assign(value,
                                New(node.ConstructorInfo!, 获取节点的构造函数参数(node))),
                            Invoke(addToCache,
                                node.Builder.Storage.Constant(),
                                node.ServiceDescriptor.Constant(),
                                Convert(value, typeof(object)))
                        }))));
            }
            else
            {
                keyExpressions.AddRange(获取所有子节点的关键表达式步骤(node)
                    .Concat(new Expression[]
                    {
                        Assign(value,
                            New(node.ConstructorInfo!, 获取节点的构造函数参数(node))),
                    }));
            }

            parameterExpressions.AddRange(获取所有子节点的变量(node));
            var (@return, end) = node.Builder.CreateEndExpression(value);
            var block = Block(parameterExpressions.Concat(new[] {value,}),
                keyExpressions.Concat(new Expression[] {@return, end}));
            IocExpression expression = Lambda<Func<object>>(block);
            var compile = expression.Compile();
            node.BuildInfo =
                new ContainerInstanceBuildInfo(nodeServiceDescriptor,
                    node.Builder.Storage)
                {
                    Expression = expression,
                    Variable = value,
                    BuildFunc = compile,
                    InternalVariable = parameterExpressions.ToArray(),
                    KeyExpressions = keyExpressions.ToArray()
                };
            node.Builder.Storage.AddBuildInfo(nodeServiceDescriptor, node.BuildInfo);
        }

        private ParameterExpression[] 获取所有子节点的变量(in Node node)
        {
            var stack = new Stack<Node>();
            stack.Push(node);
            var nodes = new List<Node>();

            while (StackExtensions.TryPop(stack, out var value))
            {
                var value1 = value;
                var array = value!.必选参数?.Select(tuple =>
                        value1!.Children![value1.ChildServiceDescriptors![tuple]])
                    .ToArray();

                if (array != null)
                {
                    stack.PushArray(array);
                    nodes.AddRange(array);
                }
            }

            var parameterExpressions = new List<ParameterExpression>();
            parameterExpressions.AddRange(nodes.Select(node1 =>
                node1.BuildInfo!.Variable));
            parameterExpressions.AddRange(nodes.SelectMany(node1 =>
                node1.BuildInfo!.InternalVariable ??
                Enumerable.Empty<ParameterExpression>()));

            return parameterExpressions.ToArray();
        }

        private Expression[] 获取节点的构造函数参数(in Node node)
        {
            var node1 = node;
            // ReSharper disable once CoVariantArrayConversion
            Expression[]? parameterExpressions = node.必选参数?.Select(tuple =>
                    node1.Children![node1.ChildServiceDescriptors![tuple]].BuildInfo!
                        .Variable)
                .ToArray();
            var expressions = node.可选参数!
                .Concat(parameterExpressions ?? Enumerable.Empty<Expression>())
                .ToArray();

            return expressions;
        }

        private IEnumerable<Expression> 获取所有子节点的关键表达式步骤(in Node node)
        {
            var stack = new Stack<Node>();
            stack.Push(node);
            var nodes = new List<Node>();

            while (StackExtensions.TryPop(stack, out var value))
            {
                var value1 = value;
                var array = value!.必选参数?.Select(tuple =>
                        value1!.Children![value1.ChildServiceDescriptors![tuple]])
                    .ToArray();

                if (array != null)
                {
                    stack.PushArray(array);
                    nodes.AddRange(array);
                }
            }

            return nodes.SelectMany(node1 => node1.BuildInfo!.KeyExpressions);
        }

        private Dictionary<int, List<Node>> 将依赖树展开(Node root)
        {
            var dictionary = new Dictionary<int, List<Node>>();
            var stack = new Stack<Node>();
            stack.Push(root);

            while (StackExtensions.TryPop(stack, out var node))
            {
                if (dictionary.TryGetValue(node!.Depth, out var list))
                {
                    list.Add(node);
                }
                else
                {
                    dictionary.Add(node.Depth, new List<Node> {node});
                }

                if (node.Children != null) stack.PushArray(node.Children.Values);
            }

            return dictionary;
        }

        private Node 构建依赖树()
        {
            var node = new Node(default,
                ServiceDescriptor,
                ConstructorInfo,
                ContainerInstanceBuilder,
                null);
            var stack = new Stack<Node>();
            stack.Push(node);
            var set = new HashSet<(Type, object)>();

            while (StackExtensions.TryPop(stack, out var x))
            {
                var tuple = (x!.ServiceDescriptor.ServiceType,
                    x.ServiceDescriptor.ServiceKey);

                if (set.Contains(tuple))
                {
                    var list = new List<Node>();
                    Node buff = x;
                    list.Add(buff);
                    while (buff.Parent != null)
                    {
                        list.Add(buff.Parent);
                        buff = buff.Parent;
                    }

                    list.Reverse();
                    var s = string.Join("->",list.Select(node1 => $@"""{node1.ServiceDescriptor.ServiceType
                    }"":""{node1.ServiceDescriptor.ServiceKey}"""));

                    throw new InvalidOperationException($@"在构造""{tuple.ServiceType
                    }"":""{tuple.ServiceKey}""时产生了循环依赖.构造链为:{s}");
                }

                x!.Init();
                set.Add(tuple);

                if (x.Children == null) continue;

                stack.PushArray(x.Children.Values);
            }

            return node;
        }
    }
}
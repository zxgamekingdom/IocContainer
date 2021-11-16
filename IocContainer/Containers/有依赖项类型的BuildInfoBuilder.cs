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
    public readonly struct �����������͵�BuildInfoBuilder
    {
        record Node(Node? Parent,
            ServiceDescriptor ServiceDescriptor,
            ConstructorInfo? ConstructorInfo,
            ContainerInstanceBuilder Builder,
            ContainerInstanceBuildInfo? BuildInfo)
        {
            //��� �㼶
            public int Depth => Parent?.Depth + 1 ?? 0;

            public void Init()
            {
                ParameterInfos = ConstructorInfo?.GetParameters();
                ��ѡ���� = ParameterInfos?.Where(p => !p.IsOptional)
                    .Select(info => (info.ParameterType,
                        Key: info.GetCustomAttribute<KeyAttribute>()?.Key ??
                        NullKey.Instance))
                    .ToArray();
                ��ѡ���� = ParameterInfos?.Where(p => p.IsOptional)
                    .Select(info => Convert(info.RawDefaultValue.Constant(),
                        info.ParameterType))
                    .ToArray();
                ChildServiceDescriptors = ��ѡ����?.ToDictionary(tuple => tuple,
                    tuple => Builder.Ѱ�һ��ߴ���ServiceDescriptor(tuple.ParameterType,
                        tuple.Key));
                Children = ChildServiceDescriptors?.ToDictionary(pair => pair.Value,
                    pair =>
                    {
                        var serviceDescriptor = pair.Value;
                        ConstructorInfo? info = default;
                        var buildInfo =
                            Builder.Storage.GetBuildInfo(serviceDescriptor) ??
                            Builder.������ͨ���͵�BuildInfo(serviceDescriptor, out info);

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
            public UnaryExpression[]? ��ѡ���� { get; private set; }
            public (Type ParameterType, object Key)[]? ��ѡ���� { get; private set; }
            // ReSharper disable once MemberCanBePrivate.Local
            public ParameterInfo[]? ParameterInfos { get; set; }
            public ContainerInstanceBuildInfo? BuildInfo { get; internal set; } =
                BuildInfo;
        }

        public ContainerInstanceBuilder ContainerInstanceBuilder { get; }
        public ServiceDescriptor ServiceDescriptor { get; }
        public ConstructorInfo ConstructorInfo { get; }

        public �����������͵�BuildInfoBuilder(
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
            var root = ����������();
            var dictionary = ��������չ��(root);
            ʹ������������BuildInfo(dictionary);

            return root.BuildInfo!;
        }

        private void ʹ������������BuildInfo(Dictionary<int, List<Node>> dictionary)
        {
            var count = dictionary.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                foreach (var node in dictionary[i])
                {
                    �����ڵ�BuildInfo(in node);
                }
            }
        }

        private void �����ڵ�BuildInfo(in Node node)
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
                    Block(��ȡ�����ӽڵ�Ĺؼ����ʽ����(node)
                        .Concat(new Expression[]
                        {
                            Assign(value,
                                New(node.ConstructorInfo!, ��ȡ�ڵ�Ĺ��캯������(node))),
                            Invoke(addToCache,
                                node.Builder.Storage.Constant(),
                                node.ServiceDescriptor.Constant(),
                                Convert(value, typeof(object)))
                        }))));
            }
            else
            {
                keyExpressions.AddRange(��ȡ�����ӽڵ�Ĺؼ����ʽ����(node)
                    .Concat(new Expression[]
                    {
                        Assign(value,
                            New(node.ConstructorInfo!, ��ȡ�ڵ�Ĺ��캯������(node))),
                    }));
            }

            parameterExpressions.AddRange(��ȡ�����ӽڵ�ı���(node));
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

        private ParameterExpression[] ��ȡ�����ӽڵ�ı���(in Node node)
        {
            var stack = new Stack<Node>();
            stack.Push(node);
            var nodes = new List<Node>();

            while (StackExtensions.TryPop(stack, out var value))
            {
                var value1 = value;
                var array = value!.��ѡ����?.Select(tuple =>
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

        private Expression[] ��ȡ�ڵ�Ĺ��캯������(in Node node)
        {
            var node1 = node;
            // ReSharper disable once CoVariantArrayConversion
            Expression[]? parameterExpressions = node.��ѡ����?.Select(tuple =>
                    node1.Children![node1.ChildServiceDescriptors![tuple]].BuildInfo!
                        .Variable)
                .ToArray();
            var expressions = node.��ѡ����!
                .Concat(parameterExpressions ?? Enumerable.Empty<Expression>())
                .ToArray();

            return expressions;
        }

        private IEnumerable<Expression> ��ȡ�����ӽڵ�Ĺؼ����ʽ����(in Node node)
        {
            var stack = new Stack<Node>();
            stack.Push(node);
            var nodes = new List<Node>();

            while (StackExtensions.TryPop(stack, out var value))
            {
                var value1 = value;
                var array = value!.��ѡ����?.Select(tuple =>
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

        private Dictionary<int, List<Node>> ��������չ��(Node root)
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

        private Node ����������()
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

                    throw new InvalidOperationException($@"�ڹ���""{tuple.ServiceType
                    }"":""{tuple.ServiceKey}""ʱ������ѭ������.������Ϊ:{s}");
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
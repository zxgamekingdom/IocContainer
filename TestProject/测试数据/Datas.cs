using System;

namespace TestProject.测试数据
{
    public abstract class AbsA
    {
    }

    public interface IA
    {
    }

    public class A : IA
    {
    }

    public class 链式
    {
        public 链式(链式1 链式1)
        {
            this.链式1 = 链式1;
        }

        public 链式1 链式1 { get; init; }
    }

    public class 链式1Copy
    {
        public 链式1Copy(链式1 链式1)
        {
            this.链式1 = 链式1;
        }

        public 链式1 链式1 { get; init; }
    }

    public class 链式1
    {
        public 链式1(链式2 链式2)
        {
            this.链式2 = 链式2;
        }

        public 链式2 链式2 { get; init; }
    }

    public class CtorAllOption
    {
        public CtorAllOption(链式2? 链式2 = default,
            int i = 1,
            int? i1 = 11,
            int i2 = 12,
            int? iD = default)
        {
            this.链式2 = 链式2;
            I = i;
            I1 = i1;
            I2 = i2;
            ID = iD;
        }

        public 链式2? 链式2 { get; }
        public int I { get; }
        public int? I1 { get; }
        public int I2 { get; }
        public int? ID { get; }
    }

    public class 链式2
    {
    }

    public class 默认值
    {
        public 默认值(int I = int.MaxValue)
        {
            this.I = I;
        }

        public int I { get; init; }
    }

    public class 特殊类型
    {
        public 特殊类型(int I)
        {
            this.I = I;
        }

        public int I { get; init; }
    }

    public class LoopA
    {
        public LoopB LoopB { get; }

        public LoopA(LoopB loopB)
        {
            LoopB = loopB;
        }
    }

    public class LoopB
    {
        public LoopC LoopC { get; }

        public LoopB(LoopC loopC)
        {
            LoopC = loopC;
        }
    }

    public class LoopC
    {
        public LoopA LoopA { get; }

        public LoopC(LoopA loopA)
        {
            LoopA = loopA;
        }
    }
}

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    internal record IsExternalInit;
}
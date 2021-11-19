// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    internal record IsExternalInit;
}
namespace TestProject.测试数据
{
    public interface IA
    {
    }
    public class A : IA
    {
    }
    public abstract class AbsA
    {
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

        public int I { get; }
        public int? I1 { get; }
        public int I2 { get; }
        public int? ID { get; }
        public 链式2? 链式2 { get; }
    }
    public class LoopA
    {
        public LoopA(LoopB loopB)
        {
            LoopB = loopB;
        }
        public LoopB LoopB { get; }
    }
    public class LoopB
    {
        public LoopB(LoopC loopC)
        {
            LoopC = loopC;
        }
        public LoopC LoopC { get; }
    }
    public class LoopC
    {
        public LoopC(LoopA loopA)
        {
            LoopA = loopA;
        }
        public LoopA LoopA { get; }
    }
    public class 链式
    {
        public 链式(链式1 链式1)
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
    public class 链式1Copy
    {
        public 链式1Copy(链式1 链式1)
        {
            this.链式1 = 链式1;
        }

        public 链式1 链式1 { get; init; }
    }
    public class 链式2
    {
        public 链式2(链式3 链式3)
        {
            this.链式3 = 链式3;
        }
        public 链式3 链式3 { get; }
    }

    public class 链式3
    {
        public int I { get; set; }
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
}
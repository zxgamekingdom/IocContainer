using System;

namespace TestProject.测试数据
{
    public abstract class AbsA
    {
    }

    public interface IA
    {
    }

    public record A : IA;
    public record 链式(链式1 链式1);
    public record 链式1(链式2 链式2);
    public record 链式2();
    public record 默认值(int I = Int32.MaxValue);
    public record 特殊类型(int I);
}

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    internal record IsExternalInit;
}
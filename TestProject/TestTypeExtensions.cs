using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using TestProject.测试数据;
using Xunit;
using Zt.Containers.Logic.Extensions;

namespace TestProject
{
    public class TestTypeExtensions
    {
        [Fact]
        public void Test_IsNullable()
        {
            Assert.True(typeof(int?).IsNullable());
        }

        [Fact]
        public void Test_IsString()
        {
            Assert.True("123".GetType().IsString());
            Assert.False(123.GetType().IsString());
        }

        [Fact]
        public void Test_IsDelegate()
        {
            Action condition = () => { };
            Assert.True(condition.GetType().IsDelegate());
            Assert.False("123".GetType().IsDelegate());
        }

        [Fact]
        public void Test_IsTask()
        {
            Assert.True(typeof(Task).IsTask());
            Assert.True(typeof(Task<int>).IsTask());
            Assert.True(typeof(ValueTask).IsTask());
            Assert.True(typeof(ValueTask<>).IsTask());
            Assert.True(typeof(ValueTask<string>).IsTask());
            Assert.False(123.GetType().IsTask());
        }

        [Fact]
        public void Test_IsValueTuple()
        {
            Assert.True((123, 123).GetType().IsValueTuple());
            Assert.False(123.GetType().IsValueTuple());
        }

        [Fact]
        public void Test_IsTuple()
        {
            Assert.True(Tuple.Create(1, 1).GetType().IsTuple());
            Assert.False(123.GetType().IsTuple());
            Assert.False((123, 123).GetType().IsTuple());
        }

        [Fact]
        public void Test_IsCancellationToken()
        {
            Assert.True(typeof(CancellationToken).IsCancellationToken());
            Assert.False(typeof(int).IsCancellationToken());
            Assert.False(typeof(ValueTuple).IsCancellationToken());
        }

        [Fact]
        public void 是不是可以处理特殊类型()
        {
            Assert.True(typeof(int).是不是可以处理特殊类型());
            Assert.True(typeof(double).是不是可以处理特殊类型());
            Assert.True(typeof(nint).是不是可以处理特殊类型());
            Assert.True(typeof(nuint).是不是可以处理特殊类型());
            Assert.True(typeof(IntPtr).是不是可以处理特殊类型());
            Assert.True(typeof(decimal).是不是可以处理特殊类型());
            Assert.True(typeof(long).是不是可以处理特殊类型());
            Assert.True(typeof(BigInteger).是不是可以处理特殊类型());
        }

        [Fact]
        public void 是不是不能处理的特殊类型()
        {
            Assert.True(typeof(IA).是不是不能处理的特殊类型());
            Assert.True(typeof(AbsA).是不是不能处理的特殊类型());
        }
    }
}
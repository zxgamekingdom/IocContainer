using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using TestProject.��������;
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
        public void �ǲ��ǿ��Դ�����������()
        {
            Assert.True(typeof(int).�ǲ��ǿ��Դ�����������());
            Assert.True(typeof(double).�ǲ��ǿ��Դ�����������());
            Assert.True(typeof(nint).�ǲ��ǿ��Դ�����������());
            Assert.True(typeof(nuint).�ǲ��ǿ��Դ�����������());
            Assert.True(typeof(IntPtr).�ǲ��ǿ��Դ�����������());
            Assert.True(typeof(decimal).�ǲ��ǿ��Դ�����������());
            Assert.True(typeof(long).�ǲ��ǿ��Դ�����������());
            Assert.True(typeof(BigInteger).�ǲ��ǿ��Դ�����������());
        }

        [Fact]
        public void �ǲ��ǲ��ܴ������������()
        {
            Assert.True(typeof(IA).�ǲ��ǲ��ܴ������������());
            Assert.True(typeof(AbsA).�ǲ��ǲ��ܴ������������());
        }
    }
}
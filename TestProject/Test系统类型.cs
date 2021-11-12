using TestProject.测试数据;
using Xunit;

namespace TestProject
{
    public class Test系统类型
    {
        [Fact]
        public void Test_Type()
        {
            Assert.Equal(typeof(Assert), typeof(Assert));
        }

        [Fact]
        public void Test_值元组()
        {
            var o = new object();
            var type = typeof(A);
            Assert.Equal((o, type), (o, typeof(A)));
        }

        [Fact]
        public void Test_TestType()
        {
        }
    }
}
using TestProject.��������;
using Xunit;

namespace TestProject
{
    public class Testϵͳ����
    {
        [Fact]
        public void Test_Type()
        {
            Assert.Equal(typeof(Assert), typeof(Assert));
        }

        [Fact]
        public void Test_ֵԪ��()
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
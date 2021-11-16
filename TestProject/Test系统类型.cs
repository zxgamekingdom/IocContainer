using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TestProject.测试数据;
using Xunit;
using static System.Linq.Expressions.Expression;

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
        public void Test_ExpressionReturn()
        {
            var variable = Variable(typeof(int));
            var parameter = Parameter(typeof(int));
            var endTarget = Label(variable.Type);
            var expressions = new List<Expression>
            {
                Assign(variable, parameter),
                IfThen(Equal(variable, Constant(0)),
                    Block(Assign(variable, Constant(13)),
                        Return(endTarget, variable),
                        Assign(variable, Constant(14)))),
                Return(endTarget, variable),
                Label(endTarget, Default(variable.Type)),
            };
            var block = Block(new[] {variable,}, expressions);
            Expression<Func<int, int>> expression =
                Lambda<Func<int, int>>(block, parameter);
            var compile = expression.Compile();
            Assert.Equal(13, compile.Invoke(0));
            Assert.Equal(233, compile.Invoke(233));
        }

        [Fact]
        public void Test_参数()
        {
            var oneParameter =
                typeof(链式1).GetConstructors().Single().GetParameters()[0];
            Assert.Equal(oneParameter, oneParameter);
            var twoParameter =
                typeof(链式1Copy).GetConstructors().Single().GetParameters()[0];
            Assert.NotEqual(oneParameter, twoParameter);
            Assert.Equal((oneParameter, (object) 123), (oneParameter, 123));
            Assert.Equal((oneParameter, 123), (oneParameter, 123));
            Assert.NotEqual((object) (oneParameter, 123),
                (oneParameter, "123"));
        }
    }
}
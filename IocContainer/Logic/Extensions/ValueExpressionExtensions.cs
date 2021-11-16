using System.Linq.Expressions;

namespace IocContainer.Logic.Extensions
{
    internal static class ValueExpressionExtensions
    {
        public static Expression Constant<T>(this T t)
        {
            return Expression.Constant(t);
        }
    }
}
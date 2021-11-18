using System.Linq.Expressions;

namespace Zt.Containers.Logic.Extensions
{
    internal static class ValueExpressionExtensions
    {
        public static Expression Constant<T>(this T t)
        {
            return Expression.Constant(t);
        }
    }
}
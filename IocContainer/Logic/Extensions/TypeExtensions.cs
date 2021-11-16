using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace IocContainer.Logic.Extensions
{
    public static class TypeExtensions
    {
        public static bool 是不是不能处理的特殊类型(this Type type)
        {
            //类型是抽象类或者接口
            return type.IsAbstract || type.IsInterface;
        }

        public static bool 是不是可以处理特殊类型(this Type type)
        {
            return type.IsDelegate() ||
                type.IsDecimal() ||
                type.IsNullable() ||
                type.IsBigInteger() ||
                type.IsEnum ||
                type.IsPrimitive ||
                type.IsPointer ||
                type.IsString() ||
                type.IsTask() ||
                type.IsCancellationToken();
        }

        //IsDecimal
        public static bool IsDecimal(this Type type)
        {
            return typeof(decimal).IsAssignableFrom(type);
        }

        //IsTuple
        public static bool IsTuple(this Type type)
        {
            return type.FullName!.StartsWith(typeof(Tuple).FullName!) && type.IsClass;
        }

        //IsBigInteger
        public static bool IsBigInteger(this Type type)
        {
            return typeof(BigInteger).IsAssignableFrom(type);
        }

        //IsValueTuple
        public static bool IsValueTuple(this Type type)
        {
            return type.FullName!.StartsWith(typeof(ValueTuple).FullName!) &&
                type.IsValueType;
        }

        //IsNullAble
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType &&
                typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }

        //IsCancellationToken
        public static bool IsCancellationToken(this Type type)
        {
            return typeof(CancellationToken).IsAssignableFrom(type);
        }

        //IsDelegate
        public static bool IsDelegate(this Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type);
        }

        //IsString
        public static bool IsString(this Type type)
        {
            return typeof(string).IsAssignableFrom(type);
        }

        //IsTask
        public static bool IsTask(this Type type)
        {
            return typeof(Task).IsAssignableFrom(type) ||
                typeof(ValueTask).IsAssignableFrom(type) ||
                (type.IsGenericType &&
                    typeof(ValueTask<>).IsAssignableFrom(
                        type.GetGenericTypeDefinition())) ||
                (type.IsGenericType &&
                    typeof(Task<>).IsAssignableFrom(type.GetGenericTypeDefinition()));
        }
    }
}
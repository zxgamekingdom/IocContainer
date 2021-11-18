using System.Collections.Generic;

namespace Zt.Containers.Logic.Extensions
{
    public static class StackExtensions
    {
        public static void PushArray<T>(this Stack<T> stack, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                stack.Push(item);
            }
        }

        public static bool TryPop<T>(Stack<T> stack, out T? value)
        {
            var b = stack.Count != 0;
            value = b switch
            {
                true => stack.Pop(),
                false => default
            };

            return b;
        }
    }
}
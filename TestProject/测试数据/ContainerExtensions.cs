using System.Reflection;
using Zt.Containers;
using Zt.Containers.Logic.DataStructures;

namespace TestProject.≤‚ ‘ ˝æ›
{
    internal static class ContainerExtensions
    {
        public static ContainerStorage GetStorage(this Container container)
        {
            return (ContainerStorage)container.GetType()
                .GetProperty("Storage", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(container);
        }
    }
}
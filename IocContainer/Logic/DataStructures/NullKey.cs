namespace IocContainer.Logic.DataStructures
{
    public record NullKey
    {
        public static NullKey Instance { get; } = new NullKey();

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private NullKey()
        {
        }
    }
}
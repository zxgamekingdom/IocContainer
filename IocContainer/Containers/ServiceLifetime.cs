namespace IocContainer.Containers
{
    /// <summary>
    /// 生命周期
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// 瞬态
        /// </summary>
        Transient,
        /// <summary>
        ///局部
        ///</summary>
        Scoped,
        /// <summary>
        ///单例
        ///</summary>
        Singleton
    }
}
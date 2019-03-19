namespace NetCoreAopEssentials.Cache
{
    /// <summary>
    /// Cache action enum
    /// </summary>
    public enum EnumCacheAction
    {
        /// <summary>
        /// Cache is set if method is executed without error.
        /// Method will not execute if value is cached.
        /// </summary>
        Set,

        /// <summary>
        /// Cache is removed if method is executed without error.
        /// </summary>
        Remove,
    }
}

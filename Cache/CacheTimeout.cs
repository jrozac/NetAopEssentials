namespace NetCoreAopEssentials.Cache
{

    /// <summary>
    /// Cache timeout constants 
    /// </summary>
    public static class CacheTimeout
    {

        /// <summary>
        /// Second
        /// </summary>
        public const long Second = 1000;

        /// <summary>
        /// Minute 
        /// </summary>
        public const long Minute = Second * 60;

        /// <summary>
        /// Hour
        /// </summary>
        public const long Hour = Minute * 60;

        /// <summary>
        /// Day
        /// </summary>
        public const long Day = Hour * 24;

        /// <summary>
        /// Week
        /// </summary>
        public const long Week = Day * 7;

        /// <summary>
        /// Month
        /// </summary>
        public const long Month = Day * 31;

        /// <summary>
        /// Year
        /// </summary>
        public const long Year = Day * 365;

        /// <summary>
        /// Decade
        /// </summary>
        public const long Decade = Year * 10;

        /// <summary>
        /// Century
        /// </summary>
        public const long Century = Year * 1000;

    }
}

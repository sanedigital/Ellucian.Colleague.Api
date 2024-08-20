namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ColleaguePubSubOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public const string APPSETTINGS_KEY = "ColleaguePubSub";
        /// <summary>
        /// 
        /// </summary>
        public const string DEFAULT_NAMESPACE = "DefaultNamespace";
        /// <summary>
        /// 
        /// </summary>
        public const string DEFAULT_CONFIG_CHANNEL = "EACSS";
        /// <summary>
        /// 
        /// </summary>
        public const string DEFAULT_CACHE_CHANNEL = "CacheNotify";

        /// <summary>
        /// Whether or not pub/sub is enabled for configuration management (i.e. EACSS)
        /// </summary>
        public bool ConfigManagementEnabled { get; set; }
        /// <summary>
        /// Whether or not pub/sub is enabled for cache management 
        /// </summary>
        public bool CacheManagementEnabled { get; set; }
        /// <summary>
        /// The pub/sub Redis connection string.
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// The namespace to separate messages according to area/application.
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// The channel for configuration-related change notifications.
        /// </summary>
        public string ConfigChannel { get; set; }

        /// <summary>
        /// The channel for cache removal notifications.
        /// </summary>
        public string CacheChannel { get; set; }
    }
}

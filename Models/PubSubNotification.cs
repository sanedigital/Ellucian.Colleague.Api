using System.Collections;
using System.Collections.Generic;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// Base class for serialization
    /// </summary>
    public abstract class PubSubNotificationBase
    {
        private string _hostName = "--Not Specified--";
        /// <summary>
        /// Host name initiating the notification (i.e. the publisher)
        /// </summary>
        public string HostName
        {
            get { return _hostName; }
            set { _hostName = value ?? "--Not Specified--"; }
        }
    }
    /// <summary>
    /// Notification for cache removal.
    /// </summary>
    public class PubSubCacheNotification : PubSubNotificationBase
    {
        string[] _cacheKeys = Array.Empty<string>();
        /// <summary>
        /// Keys to remove
        /// </summary>
        public string[] CacheKeys
        {
            get { return _cacheKeys; }
            set { _cacheKeys = value ?? Array.Empty<string>(); }
        }
    }

    /// <summary>
    /// Notification for cache removal.
    /// </summary>
    public class PubSubConfigNotification : PubSubNotificationBase
    {
        string _checksum = string.Empty;

        /// <summary>
        /// Checksum of config backup
        /// </summary>
        public string Checksum
        {
            get { return _checksum; }
            set { _checksum = value ?? string.Empty; }
        }
    }

}

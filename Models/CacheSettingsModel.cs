// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// Cache settings model
    /// </summary>
    public class CacheSettingsModel
    {
        /// <summary>
        /// Gets or sets the list of supported Web API cache providers.
        /// </summary>
        /// <value>
        /// The list of supported Web API cache providers.
        /// </value>
        public List<KeyValuePair<string, string>> SupportedCacheProviders { get; set; }

        /// <summary>
        /// Gets or sets the cache provider to be used for this Colleague Web API definition.
        /// </summary>
        /// <value>
        /// The cache provider.
        /// </value>
        public KeyValuePair<string, string> SelectedCacheProvider { get; set; }

        /// <summary>
        /// Gets or sets the available trace level options for API cache diagnostics.
        /// </summary>
        /// <value>
        /// The trace level options.
        /// </value>
        public List<KeyValuePair<string, TraceLevel?>> TraceLevelOptions { get; set; }

        /// <summary>
        /// Gets or sets the cache trace level to be used for this Colleague Web API definition.
        /// </summary>
        /// <value>
        /// The cache trace level.
        /// </value>
        public KeyValuePair<string, TraceLevel?> SelectedCacheTraceLevel { get; set; }

        /// <summary>
        /// Gets or sets the cache host to be used for this Colleague Web API definition.
        /// </summary>
        /// <value>
        /// The cache host.
        /// </value>
        public string CacheHost { get; set; }

        /// <summary>
        /// Gets or sets the cache port to be used for this Colleague Web API definition.
        /// </summary>
        /// <value>
        /// The cache port.
        /// </value>
        public ushort? CachePort { get; set; }

        /// <summary>
        /// Gets or sets the name of the cache to be used for this Colleague Web API definition.
        /// </summary>
        /// <value>
        /// The name of the cache.
        /// </value>
        public string CacheName { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CacheSettingsModel()
        {
            SupportedCacheProviders = new List<KeyValuePair<string, string>>();
            TraceLevelOptions = new List<KeyValuePair<string, TraceLevel?>>();
            CacheHost = string.Empty;
            CachePort = null;
            CacheName = string.Empty;
        }
    }
}

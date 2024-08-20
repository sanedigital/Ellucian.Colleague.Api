// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// Web API settings model.
    /// </summary>
    public class WebApiSettings
    {
        /// <summary>
        /// Gets or sets the DMI account name.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the DMI IP address.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the DMI listener port number.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether or not the DMI connection is secure.
        /// </summary>
        public bool Secure { get; set; }

        /// <summary>
        /// Gets or sets the certificate host name override.
        /// </summary>
        public string HostNameOverride { get; set; }

        /// <summary>
        /// Gets or sets the size of the connection pool.
        /// </summary>
        public short ConnectionPoolSize { get; set; }

        /// <summary>
        /// Gets or sets the shared secret.
        /// </summary>
        public string SharedSecret1 { get; set; }

        /// <summary>
        /// Gets or sets the shared secret confirmation.
        /// </summary>
        public string SharedSecret2 { get; set; }

        /// <summary>
        /// Gets or sets the available log levels.
        /// </summary>
        public IEnumerable<SelectListItem> LogLevels { get; set; }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        public string LogLevel { get; set; }

        /// <summary>
        /// Gets or sets the API profile name.
        /// </summary>
        public string ProfileName { get; set; }

        /// <summary>
        /// Gets or sets the machine key setting error message.
        /// </summary>
        public string MachineKeySettingError { get; set; }

        /// <summary>
        /// Gets or sets the machine key setting warning message.
        /// </summary>
        public string MachineKeySettingWarning { get; set; }

        /// <summary>
        /// Gets or sets the DAS account name.
        /// </summary>
        public string DasAccountName { get; set; }

        /// <summary>
        /// Gets or sets the DAS IP address.
        /// </summary>
        public string DasIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the DAS listener port number.
        /// </summary>
        public int? DasPort { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether or not the DAS connection is secure.
        /// </summary>
        public bool DasSecure { get; set; }

        /// <summary>
        /// Gets or sets the certificate host name override for the secure DAS connection.
        /// </summary>
        public string DasHostNameOverride { get; set; }

        /// <summary>
        /// Gets or sets the size of the DAS connection pool.
        /// </summary>
        public int? DasConnectionPoolSize { get; set; }

        /// <summary>
        /// Gets or sets the DAS username.
        /// </summary>
        public string DasUsername { get; set; }

        /// <summary>
        /// Gets or sets the DAS password.
        /// </summary>
        public string DasPassword { get; set; }

        /// <summary>
        /// Gets or sets the DAS settings flag.
        /// </summary>
        public bool UseDasDatareader { get; set; }
        /// <summary>
        /// Gets or sets the Oauth Issuer Url.
        /// </summary>
        public string OauthIssuerUrl { get; set; }

        /// <summary>
        /// Gets or sets the Oauth Proxy login.
        /// </summary>
        public string OauthProxyUsername { get; set; }

        /// <summary>
        /// Gets or sets the Oauth Proxy password.
        /// </summary>
        public string OauthProxyPassword { get; set; }

        /// <summary>
        /// Gets or sets the available cultures.
        /// </summary>
        public IEnumerable<SelectListItem> SupportedCultures { get; set; }

        /// <summary>
        /// Gets or sets the available UI cultures.
        /// </summary>
        public IEnumerable<SelectListItem> SupportedUiCultures { get; set; }

        /// <summary>
        /// Gets or sets the default culture.
        /// </summary>
        public string DefaultCulture { get; set; }

        /// <summary>
        /// Gets or sets the default UI culture.
        /// </summary>
        public string DefaultUiCulture { get; set; }

    }
}

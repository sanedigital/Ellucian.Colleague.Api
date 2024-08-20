// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// SaaS Staging Configuration object
    /// More info here:
    /// https://confluence.ellucian.com/display/colleague/Config+Staging+File+Template
    /// </summary>
    public class SaasStagingConfiguration
    {
        /// <summary>
        /// Application's name (Web API / Self Service / UI)
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Application's config version must be same or higher than this minimum config version for the file to be applied.
        /// </summary>
        public string MinimumConfigVersion { get; set; }

        /// <summary>
        /// Collection of settings to be updated
        /// </summary>
        public UpdateSetting[] UpdateSettings { get; set; }
    }

    /// <summary>
    /// Object describing the setting to be updated
    /// </summary>
    public partial class UpdateSetting
    {
        /// <summary>
        /// Name of setting
        /// </summary>
        public string SettingName { get; set; }

        /// <summary>
        /// Secondary name of setting
        /// </summary>
        public string SettingSecondaryName { get; set; }

        /// <summary>
        /// New value of the setting
        /// </summary>
        public string SettingValue { get; set; }
    }
}

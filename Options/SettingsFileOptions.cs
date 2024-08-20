// Copyright 2023 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Api.Options
{
    /// <summary>
    /// Allows for the options for settings file.
    /// </summary>
    public class SettingsFileOptions
    {
        /// <summary>
        /// The key for the configuration file.
        /// </summary>
        public const string Key = "SettingsFile";

        /// <summary>
        /// Path to the settings.config file.
        /// </summary>
        public string Path { get; set; } = String.Empty;
        /// <summary>
        /// Backup path for the settings file.
        /// </summary>
        public string BackupPath { get; set; } = String.Empty;
        /// <summary>
        /// Are the paths present relative to the application content root.
        /// </summary>
        public bool IsRelative { get; set; } = true;
    }
}

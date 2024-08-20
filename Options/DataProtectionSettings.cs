// Copyright 2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Api.Options
{
    /// <summary>
    /// This allows for some configuration of dataprotection behavior
    /// </summary>
    public class DataProtectionSettings
    {
        /// <summary>
        /// This is the key for retrieval in te
        /// </summary>
        public const string SettingsKey = "EllucianColleagueDataProtectionSettings";

        /// <summary>
        /// This is the key to use for the FixedKeyManager should the option for using a FixedKey be used.
        /// </summary>
        public string FixedKeyManagerKey { get; set; }

        /// <summary>
        /// This is the network path for storing XML keys in a web farm environment. 
        /// Should be a UNC path or, if a a path that is mounted/mapped consistently between hosts and files, XML key 
        /// synchronization will be the responsibility of the system administrators.
        /// If empty, it defaults to the ProtectionKeys directory within the App_Data folder.
        /// </summary>
        public string NetworkPath { get; set; }

        /// <summary>
        /// Which protection mode is configured.
        /// </summary>
        public DataProtectionMode DataProtectionMode { get; set; }

        /// <summary>
        /// Path/key for AWS secrets manager for storing the key
        /// </summary>
        public string AwsKeyPath { get; set; }
    }

    /// <summary>
    /// Allows for an indicator of which provider to configure for the application
    /// </summary>
    public enum DataProtectionMode
    {
        /// <summary>
        /// Uses the FixedKeyManager to provide a static key for data encryption. This is similar to MachineKey.
        /// </summary>
        FixedKey = 0,
        /// <summary>
        /// Uses a network share path for data protection
        /// </summary>
        NetworkShare = 1,
        /// <summary>
        /// Uses the AWS secrets manager data protection provider.
        /// See https://github.com/aws/aws-ssm-data-protection-provider-for-aspnet
        /// </summary>
        AWS = 2
    }
}

// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.App.Config.Storage.Service.Client;
using Ellucian.Colleague.Api.Models;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Mvc.Install;
using Ellucian.Web.Mvc.Install.Backup;
using Ellucian.Web.Resource;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text.Json;
using System.Net;

namespace Ellucian.Colleague.Api.Utility
{
    /// <summary>
    /// Utility for managing configuration for this application
    /// </summary>
    public class AppConfigUtility
    {
        /// <summary>
        /// API config version
        /// MUST be incremented everytime any setting/property is added/removed/renamed in any of the setting groups.
        /// </summary>
        public const string ApiConfigVersion = "3.0";

        /// <summary>
        /// config service client settings
        /// </summary>
        public ConfigStorageServiceClientSettings ConfigServiceClientSettings
        {
            get { return configServiceClientSettings; }
            set { configServiceClientSettings = value; }
        }
        private ConfigStorageServiceClientSettings configServiceClientSettings = Utilities.GetConfigMonitorSettings();

        /// <summary>
        /// Config service client for sending requests to the storage service.
        /// This client gets set by the first call to GetApiConfigurationObject() below, 
        /// which provides this instance's namespace that the client requires.
        /// </summary>
        public ConfigStorageServiceHttpClient StorageServiceClient
        {
            get { return storageServiceClient; }
            set { storageServiceClient = value; }
        }
        private ConfigStorageServiceHttpClient storageServiceClient;

        /// <summary>
        /// Allows for knowing the host for building paths, etc.
        /// </summary>
        public IWebHostEnvironment HostEnvironment { get; set; }

        private readonly ISettingsRepository _settingsRepository;
        private readonly IApiSettingsRepository _apiSettingsRepository;
        private readonly IResourceRepository _resourceRepository;
        private readonly ILogger _logger;
        private ApiSettings _apiSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settingsRepository"></param>
        /// <param name="apiSettingsRepository"></param>
        /// <param name="resourceRepository"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public AppConfigUtility(ISettingsRepository settingsRepository,
                                IApiSettingsRepository apiSettingsRepository,
                                IResourceRepository resourceRepository,
                                ILogger logger,
                                ApiSettings apiSettings)
        {
            _settingsRepository = settingsRepository;
            _apiSettingsRepository = apiSettingsRepository;
            _resourceRepository = resourceRepository;
            _logger = logger;
            _apiSettings = apiSettings;
        }

        /// <summary>
        /// Get back the overall config object which contains all of API's various config data objects
        /// </summary>
        /// <returns></returns>
        public Domain.Base.Entities.BackupConfiguration GetApiConfigurationObject()
        {
            ApiBackupConfigData backupData = new ApiBackupConfigData();
            backupData.Settings = _settingsRepository.Get();
            var nameSpace = "Ellucian/" + ApiProductInfo.ProductId + '/' + backupData.Settings.ColleagueSettings.DmiSettings.AccountName;

            // Initialize the storage service client as soon as we have the namespace
            if (StorageServiceClient == null)
            {
                StorageServiceClient = new ConfigStorageServiceHttpClient(ConfigServiceClientSettings, ApiConfigVersion, nameSpace, _logger);
            }

            if (string.IsNullOrEmpty(backupData.Settings.ColleagueSettings.DmiSettings.IpAddress)
                || backupData.Settings.ColleagueSettings.DmiSettings.Port == 0)
            {
                // this is a brand new instance with no setting configured. 
                // Just return an empty config object. The monitor job will start and if there's any backup config data, it will be restored.
                // Note: this should not happen for SaaS as the basic connection parms are set as part of provisioning.
                var blankConfig = new Domain.Base.Entities.BackupConfiguration()
                {
                    Namespace = nameSpace,
                    ProductId = ApiProductInfo.ProductId,
                    ProductVersion = ApiProductInfo.ProductVersion,
                    ConfigVersion = ApiConfigVersion,
                };
                return blankConfig;
            }

            ApiSettings apiSettings = null;
            try
            {
                // always get the fresh apiSettings from the repo instead of the cached object in dependencyresolver, 
                // as its version number automatically increases every save, and may not match what's in memory.
                apiSettings = _apiSettingsRepository.Get(backupData.Settings.ProfileName);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Exception occurred reading API profile \"" + backupData.Settings.ProfileName + "\".");
            }

            backupData.ApiSettings = apiSettings;
            backupData.BinaryFiles = GetBinaryFiles(_logger, apiSettings);

            var changeLogPath = _resourceRepository.ChangeLogPath;
            if (string.IsNullOrWhiteSpace(changeLogPath))
            {
                _logger.LogWarning("Resource file change log path could not be determined. Skipping backing up of resource change log.");
            }
            else
            {
                if (File.Exists(changeLogPath))
                {
                    var resxChangeLogFileContent = File.ReadAllText(changeLogPath);
                    backupData.ResourceFileChangeLogContent = resxChangeLogFileContent;
                }
                else
                {
                    _logger.LogInformation("Resource file change log does not exist.");
                    backupData.ResourceFileChangeLogContent = null;
                }
            }

            backupData.ResourceFiles = GetResourceFiles(_logger, _resourceRepository.BaseResourcePath, changeLogPath);

            // backup the optional MaxQueryAttributeLimit setting
            backupData.WebConfigAppSettingsMaxQueryAttributeLimit = null;
            var MaxQueryAttributeLimitSetting = ConfigurationManager.AppSettings["MaxQueryAttributeLimit"];
            if (!string.IsNullOrEmpty(MaxQueryAttributeLimitSetting))
            {
                backupData.WebConfigAppSettingsMaxQueryAttributeLimit = MaxQueryAttributeLimitSetting;
            }

            // encrypt any secrets in this backup object before serializing it.
            backupData = EncryptSecrets(backupData);

            string configJson = JsonConvert.SerializeObject(backupData);

            var configData = new Domain.Base.Entities.BackupConfiguration()
            {
                // namespace e.g. "Ellucian/Colleague Web API/dvetk_wstst01_rt"
                // This is used as the "ID" for the set of config records that this instance consumes.

                // ***IMPORTANT*** do not include any kind of product version in the namespace. If you do,
                // later version cannot get config data provided by a previous version, and this would break
                // the very popular "upgrade" scenario, where an instance with a newer version spins up to replace the older one.

                Namespace = nameSpace,
                ProductId = ApiProductInfo.ProductId,
                ProductVersion = ApiProductInfo.ProductVersion,
                ConfigVersion = ApiConfigVersion,
                ConfigData = configJson
            };
            _logger.LogInformation("Successfully built the API configuration object for backup.");
            return configData;
        }

        /// <summary>
        /// Checks the EACSS health
        /// </summary>
        /// <returns></returns>
        public async Task<HttpStatusCode> CheckStorageServiceHealth()
        {
            ApiBackupConfigData backupData = new ApiBackupConfigData();
            backupData.Settings = _settingsRepository.Get();
            var nameSpace = "Ellucian/" + ApiProductInfo.ProductId + '/' + backupData.Settings.ColleagueSettings.DmiSettings.AccountName;

            // Initialize the storage service client as soon as we have the namespace
            if (StorageServiceClient == null)
            {
                StorageServiceClient = new ConfigStorageServiceHttpClient(ConfigServiceClientSettings, ApiConfigVersion, nameSpace, _logger);
            }

            return await StorageServiceClient.HealthCheckAsync();
        }

        /// <summary>
        /// Restores this API instance's config data using the latest backup retrieved from Colleague DB.
        /// An optional date time filter can be used.
        /// Also optionally perform merging of any applicable settings, such as the resource files. 
        /// </summary>
        /// <returns></returns>
        public ApiBackupConfigData RestoreApiBackupConfiguration(string configData, bool versionChanged, string newVersion)
        {
            ApiBackupConfigData apiBackupConfigData = JsonConvert.DeserializeObject<ApiBackupConfigData>(configData);

            // decrypt any secrets in this backup object before restoring it.
            apiBackupConfigData = DecryptSecrets(apiBackupConfigData);

            if (versionChanged)
            {
                _logger.LogInformation($"Upgrading config due to version changes: new version {newVersion}");
                ProcessUpgrade(apiBackupConfigData, newVersion);
            }


            // *** Restoring ***
            // restore api settings by simply replacing it with the backup data
            _settingsRepository.Update(apiBackupConfigData.Settings); // no merging logic available
            _logger.LogInformation("settings.config restored.");

            // API profile settings stored in WEB.API.CONFIG don't need to be restored 
            // since they are already centrally stored in Colleague DB.
            // ApiSettingsRepository.Update(apiBackupConfigData.ApiSettings);
            // However, we will always replace the binary files which don't get stored in Colleague:
            if (apiBackupConfigData.BinaryFiles != null && apiBackupConfigData.BinaryFiles.Any())
            {
                foreach (KeyValuePair<string, string> entry in apiBackupConfigData.BinaryFiles)
                {
                    var fileMapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", entry.Key); // these paths are relative paths
                    var fileBytes = Convert.FromBase64String(entry.Value);
                    try
                    {
                        var targetDirectory = Path.GetDirectoryName(fileMapPath).ToLower();
                        var fileName = Path.GetFileName(fileMapPath);
                        if (!Directory.Exists(targetDirectory))
                        {
                            Directory.CreateDirectory(targetDirectory);
                        }
                        File.WriteAllBytes(Path.Combine(targetDirectory, fileName), fileBytes); // this will overwrite existing file of same name.
                        _logger.LogInformation("Wrote bytes " + entry.Key);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Could not write bytes to file " + entry.Key);
                    }
                }
            }

            // however, we do want to restore the various appsettings in web.config
            if (apiBackupConfigData.ApiSettings != null)
            {
                try
                {
                    _apiSettings.BulkReadSize = apiBackupConfigData.ApiSettings.BulkReadSize;
                    _apiSettings.IncludeLinkSelfHeaders = apiBackupConfigData.ApiSettings.IncludeLinkSelfHeaders;
                    _apiSettings.EnableConfigBackup = apiBackupConfigData.ApiSettings.EnableConfigBackup;
                    _apiSettings.AttachRequestMaxSize = apiBackupConfigData.ApiSettings.AttachRequestMaxSize;
                    _apiSettings.DetailedHealthCheckApiEnabled = apiBackupConfigData.ApiSettings.DetailedHealthCheckApiEnabled;
                    _apiSettingsRepository.Update(_apiSettings);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Exception occurred restoring web.config appsettings.");
                }
            }

            _logger.LogInformation("AppSettings restored.");

            // *** Resource File Restoration ***
            /*
             *
             * Scenario 1: This is a load balancing instance that needs to sync with one where a config change was made.
             * Method 1 is used: All resource files and change log file will be replaced with the ones from the backup config data.
             * 
             * Scenario 2: This is a load balancing instance that needs to sync with one where the resource file customizations have been
             * undone and the resource changelog file deleted.
             * Method 2 is used: copy over the resource files and delete the changelog file.
             * 
             * 
             * Scenario 3: This is a new instance for scaling or for replacing a failed instance, or is an upgrade replacement.
             * Method 3 is used: The merge logic will apply changes from the change log to the uncustomized resource files on this new instance.
             * If the changelog file is not present in the backup config data, no merge will occur.
             * This merge will cover the upgrade scenario where delivered resource files have different entries.
             *  
             */
            var changeLogPath = _resourceRepository.ChangeLogPath;
            if (!string.IsNullOrWhiteSpace(changeLogPath) && File.Exists(changeLogPath))
            {
                // This instance's change log file already exists. If so, this is not a brand new instance, so we 
                // need to update all resource files and change log by overwriting them.

                if (apiBackupConfigData.ResourceFileChangeLogContent != null)
                {
                    // Scenario 1: This is a load balancing instance that needs to sync with one where a config change was made.
                    // Method 1: All resource files and change log file will be replaced with the ones from the backup config data.
                    _logger.LogInformation("Resource file change log exists, which means this API instance has modified resource files. Overwrite instead of merge resource files.");
                    if (apiBackupConfigData.ResourceFiles != null && apiBackupConfigData.ResourceFiles.Any())
                    {
                        foreach (KeyValuePair<string, string> entry in apiBackupConfigData.ResourceFiles)
                        {
                            try
                            {
                                var physicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, entry.Key);
                                var targetDirectory = Path.GetDirectoryName(physicalPath);
                                var targetFileName = Path.GetFileName(physicalPath);

                                if (!Directory.Exists(targetDirectory))
                                {
                                    Directory.CreateDirectory(targetDirectory);
                                }
                                File.WriteAllText(physicalPath, entry.Value);
                                _logger.LogInformation("Wrote file " + physicalPath);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Could not write to file " + entry.Key);
                            }
                        }
                        _logger.LogInformation("Resource files replaced.");
                    }
                    else
                    {
                        _logger.LogInformation("No action was taken for resource files since backup data contain no resource files.");
                    }
                }
                else
                {
                    // Scenario 2: This is a load balancing instance that needs to sync with one where all resource file customizations have been
                    // undone and the resource changelog file deleted.
                    // Method 2: overwrite the resource files with ones from backup data, and delete the instance's existing change log file.
                    if (apiBackupConfigData.ResourceFiles != null && apiBackupConfigData.ResourceFiles.Any())
                    {
                        foreach (KeyValuePair<string, string> entry in apiBackupConfigData.ResourceFiles)
                        {
                            try
                            {
                                var physicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, entry.Key);
                                var targetDirectory = Path.GetDirectoryName(physicalPath);
                                if (!Directory.Exists(targetDirectory))
                                {
                                    Directory.CreateDirectory(targetDirectory);
                                }
                                File.WriteAllText(physicalPath, entry.Value);
                                _logger.LogInformation("Wrote file " + physicalPath);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Could not write to file " + entry.Key);
                            }
                        }
                        File.Delete(changeLogPath);
                        _logger.LogInformation("Resource files replaced, and changelog file deleted.");
                    }
                    else
                    {
                        _logger.LogInformation("No action was taken for resource files since backup data contain no resource files.");
                    }
                }
            }
            else
            {
                // This instance's change log file does not exist. 
                // Scenario 3: This is a new instance for scaling or for replacing a failed instance, or is an upgrade replacement.
                // resource file customization.
                if (apiBackupConfigData.ResourceFileChangeLogContent != null)
                {
                    // There's resource changes from the backup config data to be applied.

                    // Method 3:
                    // Create a change log file with the log content from the backup data, and perform the merge using the existing
                    // resouce file merge utility.
                    // NOTE: the merge utility assumes the existing resource files are Ellucian-delivered / uncustomized
                    // and bases its logic accordingly. This is why this scenario is the 
                    var targetDirectory = Path.GetDirectoryName(changeLogPath);
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }
                    File.WriteAllText(changeLogPath, apiBackupConfigData.ResourceFileChangeLogContent);
                    var appPath = AppDomain.CurrentDomain.BaseDirectory;
                    var mergeLogger = new InstallLogger(Path.Combine(appPath, @"App_Data\Logs"), "resx_merge");
                    BackupUtility buUtil = new BackupUtility(appPath, "", mergeLogger);
                    buUtil.MergeJsonResources();
                    _logger.LogInformation("Resource file merging completed.");
                }
                else
                {
                    // do nothing, as there are no resource changes in either the instance or the backup config data
                    _logger.LogInformation("No action was taken for resource files, since there are no resource file changes in either the instance or the backup config data.");
                }
            }
            _logger.LogInformation("API configuration restored/updated successfully.");

            return apiBackupConfigData;

        }

        private void ProcessUpgrade(ApiBackupConfigData apiBackupConfigData, string configVersion)
        {
            switch (configVersion)
            {
                case "3.0":
                    // this upgrade is the .NET Framework => .NET 6 upgrade so some folder paths change 
                    // update the binary file paths
                    foreach (var originalKey in apiBackupConfigData.BinaryFiles.Keys.ToArray())
                    {
                        // create a new keyvalue pair based on the new path
                        var newKey = "images/" + originalKey.Split('\\', '/').Last();

                        // add the modified path entry
                        apiBackupConfigData.BinaryFiles.Add(newKey, apiBackupConfigData.BinaryFiles[originalKey]);

                        // remove the original from our existing dictionary
                        apiBackupConfigData.BinaryFiles.Remove(originalKey);
                    }

                    foreach (var resourceFile in apiBackupConfigData.ResourceFiles.Keys.ToArray())
                    {
                        _logger.LogInformation($"Converting resource file: {resourceFile}");
                        // convert the .resx to .json
                        var newKey = resourceFile.Replace("~/", "").Replace("App_GlobalResources", "Resources").Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace(".resx", ".json");
                        if (newKey.StartsWith("/") && newKey.Length > 1)
                        {
                            newKey = newKey.Substring(1);
                        }
                        _logger.LogInformation($"New resource file key: {newKey}");

                        apiBackupConfigData.ResourceFiles[newKey] = ConvertResxToJson(apiBackupConfigData.ResourceFiles[resourceFile]);
                        apiBackupConfigData.ResourceFiles.Remove(resourceFile);
                    }

                    break;
            }
        }

        private string EnsureLinuxFriendlyPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;

            var directory = Path.GetDirectoryName(path);
            var filename = Path.GetFileName(path);

            var returnPath = Path.Combine(directory.ToLower(), filename).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.IsNullOrWhiteSpace(filename) && !string.IsNullOrWhiteSpace(returnPath))
            {
                // end with /
                returnPath += "/";
            }
            // strip the ~ style (won't be using Server.MapPath) and don't start with /
            if (returnPath.StartsWith("~/") && returnPath.Length > 2)
            {
                returnPath = returnPath.Substring(2);
            }
            else if (returnPath.StartsWith("/") && returnPath.Length > 1)
            {
                returnPath = returnPath.Substring(1);
            }
            return returnPath;
        }

        private string ConvertResxToJson(string resxContent)
        {
            var xml = XDocument.Parse(resxContent);

            var possibilities = from el in xml.Descendants("data")
                                select new KeyValuePair<string, string>(el.Attribute("name").Value, el.Elements().First().Value);
            var keyValuePairs = new Dictionary<string, string>(possibilities.DistinctBy(p => p.Key));
            var jsonSerializerSettings = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            return System.Text.Json.JsonSerializer.Serialize(keyValuePairs, jsonSerializerSettings);
        }

        /// <summary>
        /// Apply config changes from a valid staging config file.
        /// This action will occur only once. The staging config file will be archived after it is processed and will not be processed again.
        /// </summary>
        /// <returns>True if changes occurred. False if no changes made.</returns>
        public bool ApplyStagingConfigFile()
        {
            var stagingConfig = GetStagingConfig(_logger);
            bool changesOccurred = false;
            if (stagingConfig != null)
            {
                _logger.LogInformation("Valid staging config file found, applying it...");
                try
                {
                    changesOccurred = UpdateConfigsWithStagingData(_logger, stagingConfig);
                    _logger.LogInformation("Staging config file has been processed.");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error occurred processing staging config data. ");
                };

                // archive the staging config file once it's processed, so it doesn't get reapplied evert startup.
                // this is done even in the event that the processing failed, so the app doesn't get stuck in a loop.
                ArchiveStagingConfigFile(_logger);
                _logger.LogInformation("Staging config file has been archived and will not be processed again on next startup.");
            }
            return changesOccurred;
        }

        /// <summary>
        /// Similar to the restore method, but the source is the staging config file.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="stagingConfig"></param>
        /// <returns>True if changes occurred. False if no changes made.</returns>
        private bool UpdateConfigsWithStagingData(ILogger logger, SaasStagingConfiguration stagingConfig)
        {
            // Go through each supported config for staging, and update it in the supplied config data if it is present in the staging file.
            // Document supported staging configs here: 
            // https://confluence.ellucian.com/display/colleague/Config+Staging+File+Template#ConfigStagingFileTemplate-Supportedstagingconfigsbyproduct

            bool changesOccurred = false;

            // ------- all general settings ------------

            Settings currentSettings = null;

            // Set log level
            string logLevelSettingName = "log level";
            var newLogLevelSetting = GetSettingFromStagingConfigFile(stagingConfig, logLevelSettingName);
            if (newLogLevelSetting != null && !string.IsNullOrWhiteSpace(newLogLevelSetting.SettingValue))
            {
                if (currentSettings == null)
                {
                    currentSettings = _settingsRepository.Get();
                }
                var newLogLevelString = newLogLevelSetting.SettingValue;
                var oldLogLevel = currentSettings.LogLevel.ToString();

                Serilog.Events.LogEventLevel newLogLevel;
                if (Enum.TryParse(newLogLevelString, true, out newLogLevel))
                {
                    var newSettings = new Settings(currentSettings.ColleagueSettings, currentSettings.OauthSettings, newLogLevel, currentSettings.DefaultCulture, currentSettings.DefaultUiCulture) { ProfileName = currentSettings.ProfileName };
                    currentSettings = newSettings;
                    changesOccurred = true;
                    _logger.LogInformation(string.Format("Staging file changes: {0}. Old value={1}; new value={2}", logLevelSettingName, oldLogLevel, newLogLevelString));
                }
                else
                {
                    _logger.LogError(string.Format("Staging file changes: Invalid input for setting {0}: {1}", logLevelSettingName, newLogLevelString));
                }
            }

            // Set api settings profile name
            string apiProfileNameSettingName = "api profile name";
            var newApiProfileNameSetting = GetSettingFromStagingConfigFile(stagingConfig, apiProfileNameSettingName);
            if (newApiProfileNameSetting != null && !string.IsNullOrWhiteSpace(newApiProfileNameSetting.SettingValue))
            {
                if (currentSettings == null)
                {
                    currentSettings = _settingsRepository.Get();
                }
                var newApiProfileNameString = newApiProfileNameSetting.SettingValue;
                var oldApiProfileName = currentSettings.ProfileName;

                if (!string.IsNullOrWhiteSpace(newApiProfileNameString))
                {
                    var newSettings = new Settings(currentSettings.ColleagueSettings, currentSettings.OauthSettings, currentSettings.LogLevel, currentSettings.DefaultCulture, currentSettings.DefaultUiCulture) { ProfileName = newApiProfileNameString };
                    currentSettings = newSettings;
                    changesOccurred = true;
                    _logger.LogInformation(string.Format("Staging file changes: {0}. Old value={1}; new value={2}", apiProfileNameSettingName, oldApiProfileName, newApiProfileNameString));
                }
                else
                {
                    _logger.LogError(string.Format("Staging file changes: Invalid input for setting {0}: {1}", apiProfileNameSettingName, newApiProfileNameString));
                }
            }

            // Set default culture
            string defaultCultureSettingName = "default culture";
            var defaultCultureSetting = GetSettingFromStagingConfigFile(stagingConfig, defaultCultureSettingName);
            if (defaultCultureSetting != null && !string.IsNullOrWhiteSpace(defaultCultureSetting.SettingValue))
            {
                if (currentSettings == null)
                {
                    currentSettings = _settingsRepository.Get();
                }
                var newDefaultCultureString = defaultCultureSetting.SettingValue;
                var oldDefaultCulture = currentSettings.DefaultCulture;

                if (!string.IsNullOrWhiteSpace(newDefaultCultureString))
                {
                    var newSettings = new Settings(currentSettings.ColleagueSettings, currentSettings.OauthSettings, currentSettings.LogLevel, newDefaultCultureString, currentSettings.DefaultUiCulture) { ProfileName = currentSettings.ProfileName };
                    currentSettings = newSettings;
                    changesOccurred = true;
                    _logger.LogInformation(string.Format("Staging file changes: {0}. Old value={1}; new value={2}", defaultCultureSettingName, oldDefaultCulture, newDefaultCultureString));
                }
                else
                {
                    _logger.LogError(string.Format("Staging file changes: Invalid input for setting {0}: {1}", defaultCultureSettingName, newDefaultCultureString));
                }
            }

            // Set default ui culture
            string defaultUiCultureSettingName = "default ui culture";
            var defaultUiCultureSetting = GetSettingFromStagingConfigFile(stagingConfig, defaultCultureSettingName);
            if (defaultUiCultureSetting != null && !string.IsNullOrWhiteSpace(defaultUiCultureSetting.SettingValue))
            {
                if (currentSettings == null)
                {
                    currentSettings = _settingsRepository.Get();
                }
                var newDefaultUiCultureString = defaultUiCultureSetting.SettingValue;
                var oldDefaultUiCulture = currentSettings.DefaultUiCulture;

                if (!string.IsNullOrWhiteSpace(newDefaultUiCultureString))
                {
                    var newSettings = new Settings(currentSettings.ColleagueSettings, currentSettings.OauthSettings, currentSettings.LogLevel, currentSettings.DefaultCulture, newDefaultUiCultureString) { ProfileName = currentSettings.ProfileName };
                    currentSettings = newSettings;
                    changesOccurred = true;
                    _logger.LogInformation(string.Format("Staging file changes: {0}. Old value={1}; new value={2}", defaultUiCultureSettingName, oldDefaultUiCulture, newDefaultUiCultureString));
                }
                else
                {
                    _logger.LogError(string.Format("Staging file changes: Invalid input for setting {0}: {1}", defaultUiCultureSettingName, newDefaultUiCultureString));
                }
            }


            // set shared secret
            string apiSharedSecretSettingName = "api shared secret";
            var newApiSharedSecretSetting = GetSettingFromStagingConfigFile(stagingConfig, apiSharedSecretSettingName);
            if (newApiSharedSecretSetting != null && !string.IsNullOrWhiteSpace(newApiSharedSecretSetting.SettingValue))
            {
                if (currentSettings == null)
                {
                    currentSettings = _settingsRepository.Get();
                }
                var newApiSharedSecretString = newApiSharedSecretSetting.SettingValue;
                string decryptedNewApiSharedSecret = null;
                try
                {
                    decryptedNewApiSharedSecret = Utilities.DecryptString(newApiSharedSecretString);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, string.Format("Staging file changes: Decryption failed for {0}, error: {1}", apiSharedSecretSettingName, e.Message));
                }

                if (!string.IsNullOrWhiteSpace(decryptedNewApiSharedSecret))
                {
                    var oldApiSharedSecret = currentSettings.ColleagueSettings.DmiSettings.SharedSecret;

                    var newColleagueSettings = currentSettings.ColleagueSettings;
                    newColleagueSettings.DmiSettings.SharedSecret = decryptedNewApiSharedSecret;
                    var newSettings = new Settings(currentSettings.ColleagueSettings, currentSettings.OauthSettings, currentSettings.LogLevel, currentSettings.DefaultCulture, currentSettings.DefaultUiCulture) { ProfileName = currentSettings.ProfileName };
                    currentSettings = newSettings;
                    changesOccurred = true;
                    _logger.LogInformation(string.Format("Staging file changes: {0}. Old value={1}; new value={2}", apiSharedSecretSettingName, "*notshown*", "*notshown*"));
                }
                else
                {
                    _logger.LogError(string.Format("Staging file changes: Invalid input for setting {0}: {1}", apiSharedSecretSettingName, "*not shown*"));
                }
            }

            if (currentSettings != null && changesOccurred)
            {
                // object currentSettings now has all the new settings applied. Update it.
                _settingsRepository.Update(currentSettings);
                _logger.LogInformation("Staging file changes: updated 'settings' object.");

                //_logger.LogInformation("new settings=" + JsonConvert.SerializeObject(currentSettings)); for debugging only, contains plaintext secrets.
            }

            // ------ all WEB.API.CONFIG related settings -----------

            ApiSettings currentApiSettings = null;

            // Note: the binary file paths are part of the profile setting, which is stored in data base table UT.PARMS WEB.API.CONFIG

            // set report logo file (encoded binary string)
            var newReportLogoFileSetting = GetSettingFromStagingConfigFile(stagingConfig, "report logo file");
            if (newReportLogoFileSetting != null && !string.IsNullOrWhiteSpace(newReportLogoFileSetting.SettingValue))
            {
                var newReportLogoFileString = newReportLogoFileSetting.SettingValue;
                if (string.IsNullOrWhiteSpace(newReportLogoFileString))
                {
                    _logger.LogError("Staging file changes: Invalid report logo file setting - value is empty.");
                }
                else
                {
                    // get report logo path
                    if (currentSettings == null)
                    {
                        currentSettings = _settingsRepository.Get();
                    }
                    if (currentApiSettings == null)
                    {
                        try
                        {
                            currentApiSettings = _apiSettingsRepository.Get(currentSettings.ProfileName);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Staging file changes: could not read API profile setting record.");
                        }
                    }
                    string reportLogoPath = null;
                    if (currentApiSettings != null)
                    {
                        reportLogoPath = currentApiSettings.ReportLogoPath;
                        if (!string.IsNullOrWhiteSpace(reportLogoPath) && reportLogoPath.StartsWith("~"))
                        {
                            // when converting to Linux, ensure the path is not rooted and no longer uses the tilde and the 
                            // directory is lowercase
                            reportLogoPath = reportLogoPath.Replace("~/", "");
                            var directory = Path.GetDirectoryName(reportLogoPath).ToLower();
                            var filename = Path.GetFileName(reportLogoPath);
                            reportLogoPath = Path.Combine(directory, filename);
                        }
                    }
                    if (string.IsNullOrWhiteSpace(reportLogoPath))
                    {
                        reportLogoPath = "images/report-logo.png";
                        _logger.LogInformation("Staging file changes: no current report logo file path is set, using SAAS default - " + reportLogoPath);
                    }

                    byte[] fileBytes = null;
                    try
                    {
                        fileBytes = Convert.FromBase64String(newReportLogoFileString);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Staging file changes: Could not base64-decode report logo file string.");
                    }

                    if (fileBytes != null)
                    {
                        try
                        {
                            var fileMapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", reportLogoPath); // these paths are relative paths
                            var targetDirectory = Path.GetDirectoryName(fileMapPath);
                            if (!Directory.Exists(targetDirectory))
                            {
                                Directory.CreateDirectory(targetDirectory);
                            }
                            File.WriteAllBytes(fileMapPath, fileBytes); // this will overwrite existing file of same name.
                            changesOccurred = true;
                            _logger.LogInformation("Staging file changes: Updated report logo file at " + reportLogoPath);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Staging file changes: Could not write bytes to report logo file path " + reportLogoPath);
                        }
                    }
                }
            }

            // set unofficial watermark file (encoded binary string)
            var newUnofficialWatermarkFileSetting = GetSettingFromStagingConfigFile(stagingConfig, "unofficial watermark file");
            if (newUnofficialWatermarkFileSetting != null && !string.IsNullOrWhiteSpace(newUnofficialWatermarkFileSetting.SettingValue))
            {
                var newUnofficialWatermarkFileString = newUnofficialWatermarkFileSetting.SettingValue;
                if (string.IsNullOrWhiteSpace(newUnofficialWatermarkFileString))
                {
                    _logger.LogError("Staging file changes: Invalid unofficial watermark file setting - value is empty.");
                }
                else
                {
                    // get unofficial watermark path
                    if (currentSettings == null)
                    {
                        currentSettings = _settingsRepository.Get();
                    }
                    if (currentApiSettings == null)
                    {
                        try
                        {
                            currentApiSettings = _apiSettingsRepository.Get(currentSettings.ProfileName);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Staging file changes: could not read API profile setting record.");
                        }
                    }
                    string unofficialWatermarkPath = null;
                    if (currentApiSettings != null)
                    {
                        unofficialWatermarkPath = currentApiSettings.UnofficialWatermarkPath;
                        if (!string.IsNullOrWhiteSpace(unofficialWatermarkPath) && unofficialWatermarkPath.StartsWith("~"))
                        {
                            // when converting to Linux, ensure the path is not rooted and no longer uses the tilde and the 
                            // directory is lowercase
                            unofficialWatermarkPath = unofficialWatermarkPath.Replace("~/", "");
                            var directory = Path.GetDirectoryName(unofficialWatermarkPath).ToLower();
                            var filename = Path.GetFileName(unofficialWatermarkPath);
                            unofficialWatermarkPath = Path.Combine(directory, filename);
                        }
                    }
                    if (string.IsNullOrWhiteSpace(unofficialWatermarkPath))
                    {
                        unofficialWatermarkPath = "images/unofficial-watermark.png";
                        _logger.LogInformation("Staging file changes: no current watermark file path is set, using SAAS default - " + unofficialWatermarkPath);
                    }

                    byte[] fileBytes = null;
                    try
                    {
                        fileBytes = Convert.FromBase64String(newUnofficialWatermarkFileString);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Staging file changes: Could not base64-decode unofficial watermark file string.");
                    }

                    if (fileBytes != null)
                    {
                        try
                        {
                            var fileMapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", unofficialWatermarkPath); // these paths are relative paths
                            var targetDirectory = Path.GetDirectoryName(fileMapPath);
                            if (!Directory.Exists(targetDirectory))
                            {
                                Directory.CreateDirectory(targetDirectory);
                            }
                            File.WriteAllBytes(fileMapPath, fileBytes); // this will overwrite existing file of same name.
                            changesOccurred = true;
                            _logger.LogInformation("Staging file changes: Updated unofficial watermark file at " + unofficialWatermarkPath);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Staging file changes: Could not write bytes to unofficial watermark file path " + unofficialWatermarkPath);
                        }
                    }
                }
            }

            return changesOccurred;
        }

        /// <summary>
        /// Return the UpdateSetting object from the staging config object with matching setting name
        /// </summary>
        /// <param name="stagingConfig"></param>
        /// <param name="primarySettingName"></param>
        /// <returns></returns>
        public UpdateSetting GetSettingFromStagingConfigFile(SaasStagingConfiguration stagingConfig, string primarySettingName)
        {
            UpdateSetting setting = null;
            if (string.IsNullOrWhiteSpace(primarySettingName))
            {
                return null;
            }
            setting = stagingConfig.UpdateSettings.Where(
                        s => s.SettingName.Equals(primarySettingName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
            return setting;
        }


        private string SaasStagingConfigFilePath = "App_Data/SaasStagingConfig.json";

        /// <summary>
        /// Return the staging config object, if a valid file exists.
        /// </summary>
        private SaasStagingConfiguration GetStagingConfig(ILogger logger)
        {
            SaasStagingConfiguration stagingConfig = null;
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SaasStagingConfigFilePath);
            if (!File.Exists(path))
            {
                _logger.LogInformation("No staging config file found at " + SaasStagingConfigFilePath);
                return null;
            }
            var json = File.ReadAllText(path);
            try
            {
                stagingConfig = JsonConvert.DeserializeObject<SaasStagingConfiguration>(json);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred serializing the staging config object.");
                return null;
            }

            if (!stagingConfig.ApplicationName.Equals("Web API", StringComparison.Ordinal))
            {
                _logger.LogError("Staging config: app mismatch - expected 'Web API', but found: " + stagingConfig.ApplicationName);
                return null;
            }

            if (!Utilities.VerifyNewConfigVersionOK(ApiConfigVersion, stagingConfig.MinimumConfigVersion))
            {
                // minimumConfigVersion <= ApiConfigVersion
                _logger.LogError("Staging config: minimum version " + stagingConfig.MinimumConfigVersion + " is not supported. App config version is : " + ApiConfigVersion);
                return null;
            }

            return stagingConfig;
        }

        /// <summary>
        /// Rename/archive the stagingconfigfile so that it doesn't get re-applied on next startup.
        /// </summary>
        private void ArchiveStagingConfigFile(ILogger logger)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SaasStagingConfigFilePath);
            if (!File.Exists(path))
            {
                return;
            }
            try
            {
                System.IO.File.Move(path, path + "_archived_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred archiving staging config file.");
            }
        }

        private ApiBackupConfigData EncryptSecrets(ApiBackupConfigData unencryptedConfigData)
        {
            var colleagueSettings = unencryptedConfigData.Settings.ColleagueSettings;

            // sharedsecret
            var dmiSettings = colleagueSettings.DmiSettings;
            var sharedSecret = dmiSettings.SharedSecret;
            var encryptedSharedSecret = Utilities.EncryptStringNoSalt(sharedSecret);
            unencryptedConfigData.Settings.ColleagueSettings.DmiSettings.SharedSecret = encryptedSharedSecret;

            // das password
            var dbPw = colleagueSettings.DasSettings.DbPassword;
            var encryptedDbPw = Utilities.EncryptStringNoSalt(dbPw);
            unencryptedConfigData.Settings.ColleagueSettings.DasSettings.DbPassword = encryptedDbPw;

            return unencryptedConfigData;
        }

        private ApiBackupConfigData DecryptSecrets(ApiBackupConfigData encryptedConfigData)
        {
            var colleagueSettings = encryptedConfigData.Settings.ColleagueSettings;

            // sharedsecret
            var dmiSettings = colleagueSettings.DmiSettings;
            var sharedSecret = dmiSettings.SharedSecret;
            var decryptedSharedSecret = Utilities.DecryptStringNoSalt(sharedSecret);
            encryptedConfigData.Settings.ColleagueSettings.DmiSettings.SharedSecret = decryptedSharedSecret;

            // das password
            var dbPw = colleagueSettings.DasSettings.DbPassword;
            var decryptedDbPw = Utilities.DecryptStringNoSalt(dbPw);
            encryptedConfigData.Settings.ColleagueSettings.DasSettings.DbPassword = decryptedDbPw;

            return encryptedConfigData;
        }

        private Dictionary<string, string> GetBinaryFiles(ILogger logger, ApiSettings apiSettings)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            try
            {
                string reportLogoFilePath = null;
                string unofficialWatermarkFilePath = null;

                if (apiSettings != null)
                {
                    reportLogoFilePath = apiSettings.ReportLogoPath;

                    unofficialWatermarkFilePath = apiSettings.UnofficialWatermarkPath;

                }
                else
                {
                    // No APISettings (likely because the specified profile record doesn't exists),
                    // that means the binary file paths are the hard-coded defaults SAAS uses, so use them instead.
                    reportLogoFilePath = "images/report-logo.png";
                    unofficialWatermarkFilePath = "images/unofficial-watermark.png";
                }

                if (!string.IsNullOrWhiteSpace(reportLogoFilePath))
                {
                    string reportLogoFilePhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", reportLogoFilePath);
                    if (File.Exists(reportLogoFilePhysicalPath))
                    {
                        var reportLogoBytes = File.ReadAllBytes(reportLogoFilePhysicalPath);
                        var reportLogoBase64String = Convert.ToBase64String(reportLogoBytes);
                        dict[reportLogoFilePath] = reportLogoBase64String;
                    }
                }

                if (!string.IsNullOrWhiteSpace(unofficialWatermarkFilePath))
                {
                    string UnofficialWatermarkFilePhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", unofficialWatermarkFilePath);
                    if (File.Exists(UnofficialWatermarkFilePhysicalPath))
                    {
                        var UnofficialWatermarkBytes = File.ReadAllBytes(UnofficialWatermarkFilePhysicalPath);
                        var UnofficialWatermarkBase64String = Convert.ToBase64String(UnofficialWatermarkBytes);
                        dict[unofficialWatermarkFilePath] = UnofficialWatermarkBase64String;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving binary files");
            }
            // dict contains relative paths only.
            return dict;
        }

        private Dictionary<string, string> GetResourceFiles(ILogger logger, string baseResourcePath, string changelogPath)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            try
            {
                List<string> allFilePaths = Directory.GetFiles(baseResourcePath, "*.json", SearchOption.AllDirectories).ToList();
                if (File.Exists(changelogPath))
                {
                    // also grab the changelog file as well.
                    allFilePaths.Add(changelogPath);
                }
                foreach (string filePath in allFilePaths)
                {
                    if (File.Exists(filePath))
                    {
                        string content = File.ReadAllText(filePath);
                        // Save the relative path only
                        var relativePath = GetRelativePath(filePath);
                        dict[relativePath] = content;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving resource files");
            }
            return dict;
        }

        private string GetRelativePath(string physicalPath)
        {
            var rootPhysicalPath = "";
            var relativePath = "";
            if (physicalPath.Contains("App_Data"))
            {
                rootPhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
                relativePath = "App_Data" + physicalPath.Replace(rootPhysicalPath, "").Replace(@"\", "/");
            }
            else if (physicalPath.Contains("Resources"))
            {
                rootPhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                relativePath = "Resources" + physicalPath.Replace(rootPhysicalPath, "").Replace(@"\", "/");
            }
            return relativePath;
        }
    }
}

// Copyright 2012-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.App.Config.Storage.Service.Client;
using Ellucian.Colleague.Api.Client;
using Ellucian.Colleague.Api.Helpers;
using Ellucian.Colleague.Api.Models;
using Ellucian.Colleague.Api.Options;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Client;
using Ellucian.Dmi.Client.Das;
using Ellucian.Dmi.Runtime;
using Ellucian.Logging;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Mvc.Filter;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Web API administration controller.
    /// </summary>
    [LocalRequest]
    public class AdminController : Microsoft.AspNetCore.Mvc.Controller
    {
        private const int DMI_ALSERVER = 2;
        private const string PasswordSecretPlaceholder = "*********";
        private ISettingsRepository settingsRepository;
        private IConfigurationService configurationService;
        private ILogger logger;
        private readonly Microsoft.AspNetCore.Hosting.Server.IServer server;
        private ApiSettings _ApiSettings;
        private ColleagueSettings _CollSettings;
        private DmiSettings _DmiSettings;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILoggerFactory loggerFactory;
        private readonly ICacheProvider cacheProvider;
        private readonly ApiSettingRepositorySettings apiSettingsRepositorySettings;
        private readonly IHostEnvironment hostEnvironment;
        private readonly AppConfigUtility appConfigUtility;
        private readonly JwtHelper jwtHelper;
        private readonly DataProtectionSettings _dataProtectionSettings;
        private readonly IApplicationLifetime _appLifetime;
        private readonly AuditLoggingAdapter _auditLoggingAdapter;
        private readonly ColleaguePubSubOptions _configManagementPubSubOptions;
        private readonly ISubscriber _pubSubSubscriber;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="settingsRepository">ISettingsRepository instance</param>
        /// <param name="configurationService">IConfigurationService instance</param>
        /// <param name="apiSettings">IConfigurationService instance</param>
        /// <param name="collSettings">IConfigurationService instance</param>
        /// <param name="dmiSettings">IConfigurationService instance</param>
        /// <param name="logger">ILogger instance</param>
        /// <param name="server"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="cacheProvider"></param>
        /// <param name="apiSettingsRepositorySettings"></param>
        /// <param name="hostEnvironment"></param>
        /// <param name="appConfigUtility"></param>
        /// <param name="jwtHelper"></param>
        /// <param name="dataProtectionSettings"></param>
        /// <param name="appLifetime"></param>
        /// <param name="auditLoggingAdapter"></param>
        /// <param name="configManagementPubSubOptions"></param>
        /// <param name="pubSubSubscriber"></param>
        public AdminController(ISettingsRepository settingsRepository, IConfigurationService configurationService,
            ApiSettings apiSettings, ColleagueSettings collSettings, DmiSettings dmiSettings, ILogger logger,
            Microsoft.AspNetCore.Hosting.Server.IServer server, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, ICacheProvider cacheProvider,
            IOptionsMonitor<ApiSettingRepositorySettings> apiSettingsRepositorySettings, IHostEnvironment hostEnvironment,
            AppConfigUtility appConfigUtility, JwtHelper jwtHelper, DataProtectionSettings dataProtectionSettings,
            IApplicationLifetime appLifetime, AuditLoggingAdapter auditLoggingAdapter,
            IOptions<ColleaguePubSubOptions> configManagementPubSubOptions, ISubscriber pubSubSubscriber)
        {
            if (settingsRepository == null)
            {
                throw new ArgumentNullException("settingsRepository");
            }
            this.settingsRepository = settingsRepository;
            this.logger = logger;
            this.server = server;
            this.configurationService = configurationService;
            _ApiSettings = apiSettings;
            _CollSettings = collSettings;
            _DmiSettings = dmiSettings;
            this.httpContextAccessor = httpContextAccessor;
            this.loggerFactory = loggerFactory;
            this.cacheProvider = cacheProvider;
            this.apiSettingsRepositorySettings = apiSettingsRepositorySettings.CurrentValue;
            this.hostEnvironment = hostEnvironment;
            this.appConfigUtility = appConfigUtility;
            this.jwtHelper = jwtHelper;
            _dataProtectionSettings = dataProtectionSettings;
            _appLifetime = appLifetime;
            _auditLoggingAdapter = auditLoggingAdapter;
            _pubSubSubscriber = pubSubSubscriber;
        }

        /// <summary>
        /// Gets the main API administration page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            FileVersionInfo assemblyFileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            if (assemblyFileVersion != null)
            {
                ViewBag.ApiVersionNumber = assemblyFileVersion.FileVersion;
            }
            return View();
        }

        #region local settings actions

        /// <summary>
        /// Gets the API connection settings page.
        /// </summary>
        /// <returns></returns>
        public ActionResult ConnectionSettings()
        {
            var domainSettings = settingsRepository.Get();
            var model = BuildSettingsModel(domainSettings);

            if (!string.IsNullOrEmpty(model.DasPassword))
                model.DasPassword = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret1))
                model.SharedSecret1 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret2))
                model.SharedSecret2 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.OauthProxyPassword))
                model.OauthProxyPassword = PasswordSecretPlaceholder;

            ViewBag.json = JsonConvert.SerializeObject(model);
            return View();
        }

        /// <summary>
        /// Post the API connection settings page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ConnectionSettings(string model)
        {
            var localSettingsModel = JsonConvert.DeserializeObject<WebApiSettings>(model);
            var domainSettings = settingsRepository.Get();
            var oldModel = BuildSettingsModel(domainSettings);

            if (localSettingsModel.SharedSecret1 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret1 = oldModel.SharedSecret1;

            if (localSettingsModel.SharedSecret2 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret2 = oldModel.SharedSecret2;

            if (localSettingsModel.DasPassword == PasswordSecretPlaceholder)
                localSettingsModel.DasPassword = oldModel.DasPassword;

            if (localSettingsModel.OauthProxyPassword == PasswordSecretPlaceholder)
                localSettingsModel.OauthProxyPassword = oldModel.OauthProxyPassword;

            try
            {
                var userPrincipal = LocalUserUtilities.GetCurrentUser(Request) as ClaimsPrincipal;
                var userId = userPrincipal?.FindFirstValue("pid") ?? "LocalAdmin";
                var auditLogProps = new AuditLogProperties(userId);
                localSettingsModel.AuditLogConfigurationChanges(oldModel, auditLogProps, _auditLoggingAdapter);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to audit log changes for web api connection settings.");
            }

            var settings = BuildSettingsDomain(localSettingsModel);
            settingsRepository.Update(settings);
            PerformBackupConfig();
            RecycleApp(settings);
            return RedirectToAction("SettingsConfirmation");
        }

        /// <summary>
        /// Gets the API culture settings page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Culture()
        {
            var domainSettings = settingsRepository.Get();
            var model = BuildSettingsModel(domainSettings);

            if (!string.IsNullOrEmpty(model.DasPassword))
                model.DasPassword = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret1))
                model.SharedSecret1 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret2))
                model.SharedSecret2 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.OauthProxyPassword))
                model.OauthProxyPassword = PasswordSecretPlaceholder;

            ViewBag.json = JsonConvert.SerializeObject(model);
            return View();
        }

        /// <summary>
        /// Post the API logging settings page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Culture(string model)
        {
            var localSettingsModel = JsonConvert.DeserializeObject<WebApiSettings>(model);
            var domainSettings = settingsRepository.Get();
            var oldModel = BuildSettingsModel(domainSettings);

            if (localSettingsModel.SharedSecret1 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret1 = oldModel.SharedSecret1;

            if (localSettingsModel.SharedSecret2 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret2 = oldModel.SharedSecret2;

            if (localSettingsModel.DasPassword == PasswordSecretPlaceholder)
                localSettingsModel.DasPassword = oldModel.DasPassword;

            if (localSettingsModel.OauthProxyPassword == PasswordSecretPlaceholder)
                localSettingsModel.OauthProxyPassword = oldModel.OauthProxyPassword;

            try
            {
                var userPrincipal = LocalUserUtilities.GetCurrentUser(Request) as ClaimsPrincipal;
                var userId = userPrincipal?.FindFirstValue("pid") ?? "LocalAdmin";
                var auditLogProps = new AuditLogProperties(userId);
                localSettingsModel.AuditLogConfigurationChanges(oldModel, auditLogProps, _auditLoggingAdapter);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to audit log changes for web api culture settings.");
            }

            var settings = BuildSettingsDomain(localSettingsModel);
            settingsRepository.Update(settings);
            PerformBackupConfig();
            RecycleApp(settings);
            return RedirectToAction("SettingsConfirmation");
        }

        /// <summary>
        /// Gets the API key management page.
        /// </summary>
        /// <returns></returns>
        public ActionResult KeyManagement()
        {
            var viewModel = new KeyManagementViewModel()
            {
                FixedKey = PasswordSecretPlaceholder,
                KeyPath = _dataProtectionSettings.NetworkPath,
                KeyStrategy = _dataProtectionSettings.DataProtectionMode.ToString(),
                KeyStrategies = Enum.GetNames<DataProtectionMode>()
                .Where(m => appConfigUtility.ConfigServiceClientSettings != null && appConfigUtility.ConfigServiceClientSettings.IsSaaSEnvironment ? m == DataProtectionMode.AWS.ToString() : m != DataProtectionMode.AWS.ToString()),
                ConfirmationUrl = Url.Action("SettingsConfirmation")
            };

            return View(viewModel);
        }

        /// <summary>
        /// Gets the API key management page.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult KeyManagement([FromBody] KeyManagementViewModel localSettingsModel)
        {
            if (ModelState.IsValid)
            {
                // update it, where appropriate
                var appSettingsPath = Path.Combine(hostEnvironment.ContentRootPath, "appsettings.json");
                var json = System.IO.File.ReadAllText(appSettingsPath);

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new ExpandoObjectConverter());
                jsonSettings.Converters.Add(new StringEnumConverter());

                dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(json, jsonSettings);

                config.EllucianColleagueDataProtectionSettings.DataProtectionMode = localSettingsModel.KeyStrategy.ToString();
                if (localSettingsModel.FixedKey != PasswordSecretPlaceholder)
                {
                    var keyToUse = string.IsNullOrEmpty(localSettingsModel.FixedKey) ? Environment.MachineName : localSettingsModel.FixedKey;
                    config.EllucianColleagueDataProtectionSettings.FixedKeyManagerKey = keyToUse;
                    _dataProtectionSettings.FixedKeyManagerKey = keyToUse;
                }

                config.EllucianColleagueDataProtectionSettings.NetworkPath = localSettingsModel.KeyPath ?? "";
                if (!string.IsNullOrEmpty(localSettingsModel.KeyPath) && !Directory.Exists(localSettingsModel.KeyPath))
                {
                    try
                    {
                        Directory.CreateDirectory(localSettingsModel.KeyPath);
                    }
                    catch (Exception ex)
                    {
                        localSettingsModel.ErrorMessage = "Error trying to create directory for keys: " + ex.GetBaseException().Message;
                        localSettingsModel.KeyStrategies = Enum.GetNames<DataProtectionMode>()
                .Where(m => appConfigUtility.ConfigServiceClientSettings != null && appConfigUtility.ConfigServiceClientSettings.IsSaaSEnvironment ? m == DataProtectionMode.AWS.ToString() : m != DataProtectionMode.AWS.ToString());

                        return View(localSettingsModel);
                    }
                }

                // if the network path changed, copy from the old path to the new one
                if (_dataProtectionSettings.NetworkPath != config.EllucianColleagueDataProtectionSettings.NetworkPath
                    && Directory.Exists(_dataProtectionSettings.NetworkPath))
                {
                    try
                    {
                        Copy(_dataProtectionSettings.NetworkPath, config.EllucianColleagueDataProtectionSettings.NetworkPath);
                    }
                    catch (Exception ex)
                    {
                        localSettingsModel.ErrorMessage = "Error trying to copy existing keys to new path: " + ex.GetBaseException().Message;
                        localSettingsModel.KeyStrategies = Enum.GetNames<DataProtectionMode>()
                .Where(m => appConfigUtility.ConfigServiceClientSettings != null && appConfigUtility.ConfigServiceClientSettings.IsSaaSEnvironment ? m == DataProtectionMode.AWS.ToString() : m != DataProtectionMode.AWS.ToString());
                        return View(localSettingsModel);
                    }
                }

                var newJson = JsonConvert.SerializeObject(config, Formatting.Indented, jsonSettings);
                System.IO.File.WriteAllText(appSettingsPath, newJson);

                // these are serious enough to warrant the application to restart, as it involves data protection providers
                _appLifetime.StopApplication();
            }
            else
            {
                return View(localSettingsModel);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Gets the API logging settings page.
        /// </summary>
        /// <returns></returns>
        public Microsoft.AspNetCore.Mvc.ActionResult Logging()
        {
            var domainSettings = settingsRepository.Get();
            var model = BuildSettingsModel(domainSettings);

            if (!string.IsNullOrEmpty(model.DasPassword))
                model.DasPassword = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret1))
                model.SharedSecret1 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret2))
                model.SharedSecret2 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.OauthProxyPassword))
                model.OauthProxyPassword = PasswordSecretPlaceholder;

            ViewBag.json = JsonConvert.SerializeObject(model);
            return View();
        }

        /// <summary>
        /// Post the API logging settings page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Logging(string model)
        {
            var localSettingsModel = JsonConvert.DeserializeObject<WebApiSettings>(model);
            var domainSettings = settingsRepository.Get();
            var oldModel = BuildSettingsModel(domainSettings);

            if (localSettingsModel.SharedSecret1 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret1 = oldModel.SharedSecret1;

            if (localSettingsModel.SharedSecret2 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret2 = oldModel.SharedSecret2;

            if (localSettingsModel.DasPassword == PasswordSecretPlaceholder)
                localSettingsModel.DasPassword = oldModel.DasPassword;

            if (localSettingsModel.OauthProxyPassword == PasswordSecretPlaceholder)
                localSettingsModel.OauthProxyPassword = oldModel.OauthProxyPassword;

            try
            {
                var userPrincipal = LocalUserUtilities.GetCurrentUser(Request) as ClaimsPrincipal;
                var userId = userPrincipal?.FindFirstValue("pid") ?? "LocalAdmin";
                var auditLogProps = new AuditLogProperties(userId);
                localSettingsModel.AuditLogConfigurationChanges(oldModel, auditLogProps, _auditLoggingAdapter);
                logger.LogInformation("Aduit Logging: Change log level");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to audit log changes for web api logging settings.");
            }

            var settings = BuildSettingsDomain(localSettingsModel);
            settingsRepository.Update(settings);
            PerformBackupConfig();
            RecycleApp(settings);
            return RedirectToAction("SettingsConfirmation");
        }

        /// <summary>
        /// Desiged as an API method to allow for a simple change for the log level.
        /// </summary>
        /// <param name="loggingLevel">One of the valid options: Off, Error, Warning, Information, Verbose</param>
        /// <returns>OK or BadRequest</returns>
        [HttpPut]
        public Microsoft.AspNetCore.Mvc.ActionResult LoggingLevel([FromBody] string loggingLevel)
        {
            var domainSettings = settingsRepository.Get();
            var model = BuildSettingsModel(domainSettings);

            var validLogLevel = model.LogLevels.FirstOrDefault(l => l.Value.Equals(loggingLevel, StringComparison.OrdinalIgnoreCase)
                                                                 || l.Text.Equals(loggingLevel, StringComparison.OrdinalIgnoreCase));
            if (validLogLevel != null)
            {
                model.LogLevel = validLogLevel.Text;

                var settings = BuildSettingsDomain(model);
                settingsRepository.Update(settings);
                PerformBackupConfig();
                RecycleApp(settings);
                return new Microsoft.AspNetCore.Mvc.StatusCodeResult((int)HttpStatusCode.OK);
            }
            else
            {
                return new Microsoft.AspNetCore.Mvc.StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region api settings profile actions

        /// <summary>
        /// Gets the API settings profile page.
        /// </summary>
        /// <returns></returns>
        public Microsoft.AspNetCore.Mvc.ActionResult ApiSettingsProfile()
        {
            if (LocalUserUtilities.GetCurrentUser(Request) == null)
            {
                var error = "You must login before accessing the API Settings Profile";
                string returnUrl = Url.Action("ApiSettings", "Admin");
                return RedirectToAction("Login", new { returnUrl = returnUrl, error = error });
            }

            ViewBag.json = JsonConvert.SerializeObject(GetApiSettingsProfileModel());
            return View();
        }

        /// <summary>
        /// Posts the API settings profile page.
        /// </summary>
        /// <param name="model">JSON string representing a <see cref="ApiSettingsProfileModel"/></param>
        /// <returns></returns>
        [HttpPost]
        public Microsoft.AspNetCore.Mvc.ActionResult ApiSettingsProfile(string model)
        {
            var apiSettingsProfileModel = JsonConvert.DeserializeObject<ApiSettingsProfileModel>(model);
            var settingsDomain = settingsRepository.Get();

            // 1 of 2 things will happen: 1) create new profile, 2) change the current profile name.
            string selectedExistingProfileName = null;
            if (apiSettingsProfileModel.SelectedExistingProfileName != null && !string.IsNullOrEmpty(apiSettingsProfileModel.SelectedExistingProfileName.Value))
            {
                selectedExistingProfileName = apiSettingsProfileModel.SelectedExistingProfileName.Value;
            }

            // new profile
            if (!string.IsNullOrEmpty(apiSettingsProfileModel.NewProfileName) && string.IsNullOrEmpty(selectedExistingProfileName))
            {
                try
                {
                    ApiSettings apiSettings = new ApiSettings(apiSettingsProfileModel.NewProfileName);
                    var apiSettingsRepo = CreateApiSettingsRepository();
                    apiSettingsRepo.Update(apiSettings);
                }
                catch (Exception e)
                {
                    throw e;
                }
                // set the new name in the xml
                settingsDomain.ProfileName = apiSettingsProfileModel.NewProfileName;
            }
            // existing, just set the new name
            else if (string.IsNullOrEmpty(apiSettingsProfileModel.NewProfileName) && !string.IsNullOrEmpty(selectedExistingProfileName))
            {
                settingsDomain.ProfileName = selectedExistingProfileName;
            }

            // update the xml
            settingsRepository.Update(settingsDomain);

            PerformBackupConfig();

            // recycle
            RecycleApp(settingsDomain);

            return RedirectToAction("ApiSettings");
        }

        #endregion

        #region api settings actions

        /// <summary>
        /// Processes a user's request to access API settings
        /// </summary>
        /// <returns>The ApiSettings view</returns>
        public Microsoft.AspNetCore.Mvc.ActionResult ApiSettings()
        {
            if (LocalUserUtilities.GetCurrentUser(Request) == null)
            {
                var error = "You must login before accessing the API Settings";
                string returnUrl = Url.Action("ApiSettings", "Admin");
                return RedirectToAction("Login", new { returnUrl = returnUrl, error = error });
            }

            var settingsDomain = settingsRepository.Get();
            if (settingsDomain != null && string.IsNullOrEmpty(settingsDomain.ProfileName))
            {
                // profile name not defined in local settings - force the admin to create or set one...
                return RedirectToAction("ApiSettingsProfile");
            }

            var apiSettingsRepo = CreateApiSettingsRepository();
            ApiSettings apiSettingsDomain = null;
            try
            {
                apiSettingsDomain = apiSettingsRepo.Get(settingsDomain.ProfileName);
            }
            catch (ArgumentException)
            {
                // profile name does not exist in colleague
                return RedirectToAction("ApiSettingsProfile");
            }

            // Instantiate an ApiSettingsModel
            var apiSettingsModel = new ApiSettingsModel();
            apiSettingsModel.Id = apiSettingsDomain.Id;
            apiSettingsModel.Version = apiSettingsDomain.Version;
            apiSettingsModel.ProfileName = settingsDomain.ProfileName;

            // Initialize the photo settings
            apiSettingsModel.PhotoSettings.ParseFormattedUrl(apiSettingsDomain.PhotoURL);
            var selectedImageType = apiSettingsModel.PhotoSettings.ImageTypes.Where(a => a.Key == apiSettingsDomain.PhotoType).FirstOrDefault();
            if (selectedImageType.Key != null)
            {
                apiSettingsModel.PhotoSettings.SelectedImageType = selectedImageType;
            }

            if (apiSettingsDomain.PhotoHeaders != null && apiSettingsDomain.PhotoHeaders.Count > 0) // name should be PhotoHeaders
            {
                apiSettingsModel.PhotoSettings.CustomHeaders = apiSettingsDomain.PhotoHeaders.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList();
            }

            // Initialize the report settings
            apiSettingsModel.ReportSettings.ReportLogoPath = apiSettingsDomain.ReportLogoPath;
            apiSettingsModel.ReportSettings.UnofficialWatermarkPath = apiSettingsDomain.UnofficialWatermarkPath;

            // Initialize the cache settings
            //   Set up the supported cache providers data for the drop-down
            apiSettingsModel.CacheSettings.SupportedCacheProviders = apiSettingsDomain.SupportedCacheProviders;

            // Set the currently-selected cache provider; if the setting from the domain is empty, default to in-process caching since that's the default setting
            string currentCacheProvider = apiSettingsDomain.CacheProvider;
            if (string.IsNullOrEmpty(currentCacheProvider))
            {
                currentCacheProvider = Ellucian.Web.Http.Configuration.ApiSettings.INPROC_CACHE;
            }
            var selectedCacheProvider = apiSettingsModel.CacheSettings.SupportedCacheProviders.Where(a => a.Value == apiSettingsDomain.CacheProvider).FirstOrDefault();
            if (selectedCacheProvider.Key != null)
            {
                apiSettingsModel.CacheSettings.SelectedCacheProvider = selectedCacheProvider;
            }

            if (apiSettingsDomain.CacheProvider == Ellucian.Web.Http.Configuration.ApiSettings.INPROC_CACHE)
            {
                // For in-process caching, the cache host, port, name, and trace levels do not apply
                apiSettingsModel.CacheSettings.CacheHost = string.Empty;
                apiSettingsModel.CacheSettings.CachePort = null;
            }
            else
            {
                // Set the cache host, port, and name from the values retrieved from Colleague
                apiSettingsModel.CacheSettings.CacheHost = apiSettingsDomain.CacheHost;
                apiSettingsModel.CacheSettings.CachePort = apiSettingsDomain.CachePort;
            }

            // Serialize the settings model and add to the view bag
            ViewBag.json = JsonConvert.SerializeObject(apiSettingsModel);
            return View();
        }

        /// <summary>
        /// Processes a user's request to update API settings
        /// </summary>
        /// <param name="model">The Api settings view model</param>
        /// <returns>The settings confirmation view</returns>
        [HttpPost]
        public async Task<ActionResult> ApiSettings(string model)
        {
            var apiSettingsModel = JsonConvert.DeserializeObject<ApiSettingsModel>(model);
            ApiSettings apiSettingsDomain = null;

            if (!string.IsNullOrEmpty(apiSettingsModel.ProfileName))
            {
                apiSettingsDomain = new ApiSettings(apiSettingsModel.Id, apiSettingsModel.ProfileName, apiSettingsModel.Version);
                if (apiSettingsModel.PhotoSettings != null)
                {
                    var photoSettings = apiSettingsModel.PhotoSettings;
                    apiSettingsDomain.PhotoURL = photoSettings.GetFormattedUrl();
                    apiSettingsDomain.PhotoType = photoSettings.SelectedImageType.Key;
                    if (photoSettings.CustomHeaders != null && photoSettings.CustomHeaders.Count > 0)
                    {
                        foreach (var header in photoSettings.CustomHeaders)
                        {
                            if (!string.IsNullOrEmpty(header.Key))
                            {
                                apiSettingsDomain.PhotoHeaders.Add(header.Key, header.Value ?? string.Empty);
                            }
                        }
                    }
                }

                if (apiSettingsModel.ReportSettings != null)
                {
                    var reportSettings = apiSettingsModel.ReportSettings;
                    apiSettingsDomain.ReportLogoPath = reportSettings.ReportLogoPath;
                    apiSettingsDomain.UnofficialWatermarkPath = reportSettings.UnofficialWatermarkPath;
                }

                if (apiSettingsModel.CacheSettings != null)
                {
                    var cacheSettings = apiSettingsModel.CacheSettings;
                    apiSettingsDomain.CacheProvider = cacheSettings.SelectedCacheProvider.Value;
                    if (apiSettingsDomain.CacheProvider == Ellucian.Web.Http.Configuration.ApiSettings.INPROC_CACHE)
                    {
                        // In-process caching does not use any of the other settings; so, even if set, disregard them
                        apiSettingsDomain.CacheHost = string.Empty;
                        apiSettingsDomain.CachePort = null;
                    }
                    else
                    {
                        // For all other providers, set all other settings
                        apiSettingsDomain.CacheHost = cacheSettings.CacheHost;
                        apiSettingsDomain.CachePort = cacheSettings.CachePort;
                    }
                }

                var apiSettingsRepo = CreateApiSettingsRepository();
                var oldApiSettings = apiSettingsRepo.Get(apiSettingsDomain.Name);
                apiSettingsRepo.Update(apiSettingsDomain);

                try
                {
                    var userPrincipal = LocalUserUtilities.GetCurrentUser(Request) as ClaimsPrincipal;
                    var userId = userPrincipal?.FindFirstValue("pid") ?? "LocalAdmin";
                    var auditLogProps = new AuditLogProperties(userId);
                    apiSettingsDomain.AuditLogConfigurationChanges(oldApiSettings, auditLogProps, _auditLoggingAdapter);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to audit log changes for web api logging settings.");
                }
            }

            // do a backup, even though API settings isn't included in the restore.
            PerformBackupConfig();

            // recycle
            RecycleApp(apiSettingsDomain);

            return RedirectToAction("SettingsConfirmation");
        }

        #endregion

        #region Oauth settings actions

        /// <summary>
        /// Gets the API connection settings page.
        /// </summary>
        /// <returns></returns>
        public ActionResult OauthSettings()
        {
            var domainSettings = settingsRepository.Get();
            var model = BuildSettingsModel(domainSettings);

            if (!string.IsNullOrEmpty(model.DasPassword))
                model.DasPassword = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret1))
                model.SharedSecret1 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret2))
                model.SharedSecret2 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.OauthProxyPassword))
                model.OauthProxyPassword = PasswordSecretPlaceholder;

            ViewBag.json = JsonConvert.SerializeObject(model);
            return View();
        }

        /// <summary>
        /// Post the API connection settings page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> OauthSettings(string model)
        {
            var localSettingsModel = JsonConvert.DeserializeObject<WebApiSettings>(model);
            var domainSettings = settingsRepository.Get();
            var oldModel = BuildSettingsModel(domainSettings);
            if (localSettingsModel.SharedSecret1 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret1 = oldModel.SharedSecret1;
            if (localSettingsModel.SharedSecret2 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret2 = oldModel.SharedSecret2;
            if (localSettingsModel.DasPassword == PasswordSecretPlaceholder)
                localSettingsModel.DasPassword = oldModel.DasPassword;
            if (localSettingsModel.OauthProxyPassword == PasswordSecretPlaceholder)
                localSettingsModel.OauthProxyPassword = oldModel.OauthProxyPassword;

            try
            {
                var userPrincipal = LocalUserUtilities.GetCurrentUser(Request) as ClaimsPrincipal;
                var userId = userPrincipal?.FindFirstValue("pid") ?? "LocalAdmin";
                var auditLogProps = new AuditLogProperties(userId);
                localSettingsModel.AuditLogConfigurationChanges(oldModel, auditLogProps, _auditLoggingAdapter);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to audit log changes for web api connection settings.");
            }

            var settings = BuildSettingsDomain(localSettingsModel);
            settingsRepository.Update(settings);
            PerformBackupConfig();
            RecycleApp(settings);
            return RedirectToAction("SettingsConfirmation");
        }

        #endregion

        #region login/out actions

        /// <summary>
        /// Gets the login page used by the API settings pages.
        /// </summary>
        /// <param name="returnUrl">URL to return to after login, if any.</param>
        /// <param name="error">Error message from a previous failed login.</param>
        /// <returns></returns>
        public Microsoft.AspNetCore.Mvc.ActionResult Login(string returnUrl, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                ModelState.AddModelError("", error);
            }
            if (!string.IsNullOrEmpty(returnUrl))
            {
                ViewBag.RouteValues = new { returnUrl = returnUrl };
            }

            return View();
        }

        /// <summary>
        /// Submits the login page used by the API settings pages.
        /// </summary>
        /// <param name="credentials"><see cref="TestLogin"/> model</param>
        /// <param name="returnUrl">URL to return to after login, if any.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(TestLogin credentials, string returnUrl)
        {
            try
            {
                string baseUrl = credentials.BaseUrl;
                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = GetWebAppRoot();

                }

                var client = new ColleagueApiClient(baseUrl, logger);
                var token = await client.Login2Async(credentials.UserId, credentials.Password);
                Response.Cookies.Append(LocalUserUtilities.CookieId, LocalUserUtilities.CreateCookie(baseUrl, token));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
                ViewBag.RouteValues = new { returnUrl = returnUrl };
                return View(credentials);
            }

            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Submits a logout for the login used by the API settings pages.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            var cookieValue = LocalUserUtilities.GetCookie(Request);
            if (string.IsNullOrEmpty(cookieValue))
            {
                throw new ColleagueWebApiException("Log in first");
            }
            var baseUrl = cookieValue.Split('*')[0];
            var token = cookieValue.Split('*')[1];
            var client = new ColleagueApiClient(baseUrl, logger);
            client.Credentials = token;
            await client.LogoutAsync(token);
            Response.Cookies.Append(LocalUserUtilities.CookieId, cookieValue, LocalUserUtilities.CreateExpiredCookieOptions());
            return RedirectToAction("Index", "Home");
        }
        #endregion

        /// <summary>
        /// Gets the API settings confirmation page.
        /// </summary>
        /// <returns></returns>
        public Microsoft.AspNetCore.Mvc.ActionResult SettingsConfirmation()
        {
            var domainSettings = settingsRepository.Get();
            var model = BuildSettingsModel(domainSettings);

            if (!string.IsNullOrEmpty(model.DasPassword))
                model.DasPassword = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret1))
                model.SharedSecret1 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret2))
                model.SharedSecret2 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.OauthProxyPassword))
                model.OauthProxyPassword = PasswordSecretPlaceholder;

            ViewBag.json = JsonConvert.SerializeObject(model);

            return View();
        }

        /// <summary>
        /// Submits a request to test the app listener connection setting to DMI.
        /// </summary>
        /// <param name="model"><see cref="TestConnection"/> model</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> TestAppConnectionAsync([FromBody] TestConnection model)
        {
            var collSettings = new ColleagueSettings();
            var dmiSettings = new DmiSettings();
            dmiSettings.AccountName = model.AccountName;
            dmiSettings.ConnectionPoolSize = model.ConnectionPoolSize;
            dmiSettings.HostNameOverride = model.HostNameOverride;
            dmiSettings.IpAddress = model.IpAddress;
            dmiSettings.Port = model.Port;
            dmiSettings.Secure = model.Secure;
            if (model.SharedSecret1 == PasswordSecretPlaceholder)
            {
                var domainSettings = settingsRepository.Get();
                var oldModel = BuildSettingsModel(domainSettings);
                dmiSettings.SharedSecret = oldModel.SharedSecret1;
            }
            else
                dmiSettings.SharedSecret = model.SharedSecret1;

            collSettings.DmiSettings = dmiSettings;

            var repoLogger = loggerFactory.CreateLogger<ColleagueSessionRepository>();
            var sessionRepo = new ColleagueSessionRepository(dmiSettings, repoLogger, jwtHelper, cacheProvider, httpContextAccessor);
            string token = null;
            try
            {
                token = await sessionRepo.LoginAsync(model.UserId, model.Password);
            }
            catch (LoginException lex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(lex.Message);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Login failed: " + ex.Message);
            }

            try
            {
                // try a data read to verify the shared secret
                var claims = JwtHelper.CreatePrincipal(token);
                var dataReader = CreateTransactionFactory(collSettings, claims).GetDataReader();
                await dataReader.SelectAsync("UT.PARMS", "");

                // Bye
                await sessionRepo.LogoutAsync(token);
                return Json("Success!");
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Data reader test failed. Check the Shared Secret on the API Connection Settings form. The error was: " + ex.Message);
            }
            finally
            {
                HttpContext.User = null;
            }
        }

        /// <summary>
        /// Submits a request to test the OAuth setting URL and Proxy UserID/Password.
        /// </summary>
        /// <param name="model"><see cref="TestConnection"/> model</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> TestOauthSettingsAsync([FromBody] TestConnection model)
        {
            var domainSettings = settingsRepository.Get();
            var dmiSettings = domainSettings.ColleagueSettings.DmiSettings;
            var myIssuer = model.OauthIssuerUrl;

            if (model.Password == PasswordSecretPlaceholder)
            {
                var oldModel = BuildSettingsModel(domainSettings);
                model.Password = oldModel.OauthProxyPassword;
            }

            try
            {
                IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{myIssuer}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                OpenIdConnectConfiguration openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("The OAUTH Issuer URL did not respond with the OpenID Configuration.");
            }

            var repoLogger = loggerFactory.CreateLogger<ColleagueSessionRepository>();
            var sessionRepo = new ColleagueSessionRepository(dmiSettings, repoLogger, jwtHelper, cacheProvider, httpContextAccessor);
            string token;
            try
            {
                token = await sessionRepo.LoginAsync(model.UserId, model.Password);
            }
            catch (LoginException lex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(lex.Message);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Login failed: " + ex.Message);
            }

            // Bye
            await sessionRepo.LogoutAsync(token);
            HttpContext.User = null;

            return Json("Success!");
        }

        /// <summary>
        /// Submits a request to test the connection setting to DAS.
        /// </summary>
        /// <param name="model"><see cref="TestDASConnectionAsync"/>model</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> TestDASConnectionAsync([FromBody] TestConnection model)
        {
            var collSettings = new ColleagueSettings();
            var dasSettings = new DasSettings();
            dasSettings.AccountName = model.DasAccountName;
            dasSettings.ConnectionPoolSize = model.DasConnectionPoolSize.HasValue ? model.DasConnectionPoolSize.Value : 1;
            dasSettings.HostNameOverride = model.DasHostNameOverride;
            dasSettings.IpAddress = model.DasIpAddress;
            dasSettings.Port = model.DasPort.HasValue ? model.DasPort.Value : 1;
            dasSettings.Secure = model.DasSecure;
            dasSettings.DbLogin = model.DasUsername;
            if (model.DasPassword == PasswordSecretPlaceholder)
            {
                var domainSettings = settingsRepository.Get();
                var oldModel = BuildSettingsModel(domainSettings);
                dasSettings.DbPassword = oldModel.DasPassword;
            }
            else
                dasSettings.DbPassword = model.DasPassword;

            collSettings.DasSettings = dasSettings;

            // Das session instantiation
            DasSession dasSession;
            try
            {
                dasSession = new DasSession(dasSettings);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                string errorMessage = "DAS session creation failed. The error was: " + ex.Message;
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    errorMessage += " (" + ex.InnerException.Message + ")";
                }
                return Json(errorMessage);
            }

            // Login
            try
            {
                if (model.DasPassword == PasswordSecretPlaceholder)
                {
                    var domainSettings = settingsRepository.Get();
                    var oldModel = BuildSettingsModel(domainSettings);
                    await dasSession.LoginAsync(model.DasUsername, oldModel.DasPassword);
                }
                else
                    await dasSession.LoginAsync(model.DasUsername, model.DasPassword);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Login failed: " + ex.Message);
            }

            // Data Reader test (to validate registry/account name)
            bool exceptionOccurred = false;
            try
            {
                // Take the specified account name from the settings page, and read from DMIACCTS
                var dmiacctsReadObject = await dasSession.ReadRecordAsync("DMIACCTS", dasSettings.AccountName);
                if (dmiacctsReadObject.ReadStatusCode == ReadStatus.Normal)
                {
                    string dmiacctsRawRecord = dmiacctsReadObject.Record;
                    string dmiAlserverValue = DasUtils.Field(dmiacctsRawRecord, DmiString._FM, DMI_ALSERVER);

                    if (!dmiAlserverValue.EndsWith("_database"))
                    {
                        // The pointer to the ALSERVRS record isn't referencing the database connection (never the case for
                        // the DAS registry/account name); throw exception
                        throw new ColleagueDataReaderException("This does not appear to be a Colleague DAS listener. Please check the DAS Registry Name on the API Connection Settings form.");
                    }
                }
                else
                {
                    // Failure to read record; exception
                    throw new ColleagueDataReaderException("This does not appear to be a Colleague DAS listener. Check the DAS Registry Name, Listener IP, and/or Listener Port on the API Connection Settings form.");
                }
            }
            catch (Exception ex)
            {
                exceptionOccurred = true;
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message);
            }
            finally
            {
                // Ensure that we log out if an exception occurred
                if (exceptionOccurred)
                {
                    try
                    {
                        await dasSession.LogoutAsync();
                    }
                    catch (Exception ex)
                    {
                        // Can ignore exceptions here
                        logger.LogError(ex.Message, "Error DAS session logout");
                    }
                    finally
                    {
                        HttpContext.User = null;
                    }
                }
            }

            // Logout
            try
            {
                await dasSession.LogoutAsync();
                return Json("Success!");
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Login was successful but error on logout. The error was: " + ex.Message);
            }
            finally
            {
                HttpContext.User = null;
            }
        }

        /// <summary>
        /// Backup all API configs
        /// </summary>
        public async void PerformBackupConfig()
        {

            // SaaS backup
            if (appConfigUtility.ConfigServiceClientSettings != null && appConfigUtility.ConfigServiceClientSettings.IsSaaSEnvironment)
            {
                string username = "unknown";
                try
                {
                    username = HttpContext.User.Identity.Name;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message, "Error at user identity name");
                }

                try
                {
                    // send a copy of this latest config data to the storage service.
                    var configObject = appConfigUtility.GetApiConfigurationObject();
                    var result = appConfigUtility.StorageServiceClient.PostConfigurationAsync(
                        configObject.Namespace, configObject.ConfigData, username,
                        configObject.ConfigVersion, configObject.ProductId, configObject.ProductVersion).GetAwaiter().GetResult();

                    // after submitting a new snapshot, set the lastrestoredchecksum to this new snapshot's checksum.
                    // This must be done to avoid a looping situation where instances keep performing merges
                    // in lock step with each other due to lastrestoredchecksum file containing an older checksum, when 
                    // there are changes that are repeated (e.g. logging toggled on/off).
                    var currentChecksum = Utilities.GetMd5ChecksumString(configObject.ConfigData);
                    Utilities.SetLastRestoredChecksum(currentChecksum);

                    // notify the pubsub if configured
                    if (_configManagementPubSubOptions.ConfigManagementEnabled)
                    {
                        var eacssChannel = new RedisChannel((_configManagementPubSubOptions.Namespace ?? ColleaguePubSubOptions.DEFAULT_NAMESPACE) + "/" + (_configManagementPubSubOptions.ConfigChannel ?? ColleaguePubSubOptions.DEFAULT_CONFIG_CHANNEL), RedisChannel.PatternMode.Literal);

                        var notification = new PubSubConfigNotification() { HostName = Environment.MachineName, Checksum = currentChecksum };
                        var json = System.Text.Json.JsonSerializer.Serialize(notification);

                        _pubSubSubscriber.Publish(eacssChannel, new RedisValue(json));
                    }

                }
                catch (Exception e)
                {
                    logger.LogError(e, "Configuration changes have been saved, but the backup to config storage service failed. See API log for more details.");
                }
            }
            else
            {
                // Colleague-based backup
                if (!_ApiSettings.EnableConfigBackup)
                {
                    return;
                }
                try
                {
                    var cookieValue = LocalUserUtilities.GetCookie(Request);
                    if (string.IsNullOrEmpty(cookieValue))
                    {
                        throw new ColleagueWebApiException("Log in first");
                    }
                    var baseUrl = cookieValue.Split('*')[0];
                    var token = cookieValue.Split('*')[1];
                    var client = new ColleagueApiClient(baseUrl, logger);
                    client.Credentials = token;
                    await client.PostBackupApiConfigDataAsync();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Configuration changes have been saved, but the backup action failed. See API log for more details.");
                    // throw;
                }
            }
        }

        private string GetWebAppRoot()
        {
            var urlBase = String.Format("{0}://{1}", Request.Scheme, Request.Host);
            var url = new Uri(urlBase);
            string host = (url.IsDefaultPort) ?
                url.Host :
                url.Authority;
            host = String.Format("{0}://{1}", url.Scheme, host);

            return host + Request.PathBase;
        }

        private async void RecycleApp(Settings settings)
        {
            if (settings == null || settings.ColleagueSettings == null || settings.OauthSettings == null ||
                settings.ColleagueSettings.DasSettings == null || settings.ColleagueSettings.DmiSettings == null ||
                settings.ColleagueSettings.GeneralSettings == null)
            {
                return;
            }

            var logLevel = settings.LogLevel;
            var collSettings = settings.ColleagueSettings;
            var oauthSettings = settings.OauthSettings;
            var dmiSettings = collSettings.DmiSettings;
            var dasSettings = collSettings.DasSettings;
            var generalSettings = collSettings.GeneralSettings;

            // Update log level switch in bootstrapper
            Bootstrapper.LoggingLevelSwitch.MinimumLevel = logLevel;

            // If logged in and the DMI or Das connections have changed then logout
            if (_DmiSettings.AccountName != dmiSettings.AccountName ||
                _DmiSettings.ConnectionPoolSize != dmiSettings.ConnectionPoolSize ||
                _DmiSettings.HostNameOverride != dmiSettings.HostNameOverride ||
                _DmiSettings.IpAddress != dmiSettings.IpAddress ||
                _DmiSettings.Port != dmiSettings.Port ||
                _DmiSettings.Secure != dmiSettings.Secure ||
                _DmiSettings.SharedSecret != dmiSettings.SharedSecret ||
                _CollSettings.DasSettings.AccountName != dasSettings.AccountName ||
                _CollSettings.DasSettings.IpAddress != dasSettings.IpAddress ||
                _CollSettings.DasSettings.Port != dasSettings.Port ||
                _CollSettings.DasSettings.Secure != dasSettings.Secure ||
                _CollSettings.DasSettings.HostNameOverride != dasSettings.HostNameOverride ||
                _CollSettings.DasSettings.ConnectionPoolSize != dasSettings.ConnectionPoolSize ||
                _CollSettings.DasSettings.DbLogin != dasSettings.DbLogin ||
                _CollSettings.DasSettings.DbPassword != dasSettings.DbPassword)
            {
                var cookieValue = LocalUserUtilities.GetCookie(Request);
                if (!string.IsNullOrEmpty(cookieValue))
                {
                    var baseUrl = cookieValue.Split('*')[0];
                    var token = cookieValue.Split('*')[1];
                    var client = new ColleagueApiClient(baseUrl, logger);
                    client.Credentials = token;
                    await client.LogoutAsync(token);
                    Response.Cookies.Append(LocalUserUtilities.CookieId, cookieValue, LocalUserUtilities.CreateExpiredCookieOptions());
                }

                // Update Dmi Connection Pool
                await DmiConnectionPool.CloseAllConnectionsAsync();
                await DmiConnectionPool.SetSizeAsync(
                    DmiConnectionPool.ConnectionPoolName(dmiSettings.IpAddress, dmiSettings.Port, dmiSettings.Secure),
                    dmiSettings.ConnectionPoolSize);
                await Ellucian.Dmi.Client.Das.DasSessionPool.SetSizeAsync(dasSettings.ConnectionPoolSize);

                // Update Colleague Settings
                _CollSettings.DmiSettings.AccountName = dmiSettings.AccountName;
                _CollSettings.DmiSettings.ConnectionPoolSize = dmiSettings.ConnectionPoolSize;
                _CollSettings.DmiSettings.HostNameOverride = dmiSettings.HostNameOverride;
                _CollSettings.DmiSettings.IpAddress = dmiSettings.IpAddress;
                _CollSettings.DmiSettings.Port = dmiSettings.Port;
                _CollSettings.DmiSettings.Secure = dmiSettings.Secure;
                _CollSettings.DmiSettings.SharedSecret = dmiSettings.SharedSecret;
                _CollSettings.GeneralSettings.UseDasDatareader = generalSettings.UseDasDatareader;
                _CollSettings.DasSettings.AccountName = dasSettings.AccountName;
                _CollSettings.DasSettings.IpAddress = dasSettings.IpAddress;
                _CollSettings.DasSettings.Port = dasSettings.Port;
                _CollSettings.DasSettings.Secure = dasSettings.Secure;
                _CollSettings.DasSettings.HostNameOverride = dasSettings.HostNameOverride;
                _CollSettings.DasSettings.ConnectionPoolSize = dasSettings.ConnectionPoolSize;
                _CollSettings.DasSettings.DbLogin = dasSettings.DbLogin;
                _CollSettings.DasSettings.DbPassword = dasSettings.DbPassword;

                // Update DMI Settings
                _DmiSettings.AccountName = dmiSettings.AccountName;
                _DmiSettings.ConnectionPoolSize = dmiSettings.ConnectionPoolSize;
                _DmiSettings.HostNameOverride = dmiSettings.HostNameOverride;
                _DmiSettings.IpAddress = dmiSettings.IpAddress;
                _DmiSettings.Port = dmiSettings.Port;
                _DmiSettings.Secure = dmiSettings.Secure;
                _DmiSettings.SharedSecret = dmiSettings.SharedSecret;

                // Clear the cache
                List<string> filter = new List<string>();
                UtilityCacheRepository cacheRepo = new UtilityCacheRepository(cacheProvider);
                cacheRepo.ClearCache(filter);
            }
        }

        /// <summary>
        /// Private repository class which extends the base caching repository; a bit of a hack so this controller
        /// can access methods in the abstract base caching repository
        /// </summary>
        private class UtilityCacheRepository : BaseCachingRepository
        {
            protected internal UtilityCacheRepository(ICacheProvider cacheProvider)
                : base(cacheProvider)
            {
            }
        }

        private void RecycleApp(ApiSettings apiSettings)
        {
            if (apiSettings == null)
            {
                return;
            }

            if (_ApiSettings.PhotoType != apiSettings.PhotoType ||
                _ApiSettings.PhotoURL != apiSettings.PhotoURL ||
                _ApiSettings.PhotoHeaders != apiSettings.PhotoHeaders ||
                _ApiSettings.ReportLogoPath != apiSettings.ReportLogoPath ||
                _ApiSettings.UnofficialWatermarkPath != apiSettings.UnofficialWatermarkPath ||
                _ApiSettings.CacheProvider != apiSettings.CacheProvider ||
                _ApiSettings.BulkReadSize != apiSettings.BulkReadSize ||
                _ApiSettings.IncludeLinkSelfHeaders != apiSettings.IncludeLinkSelfHeaders ||
                _ApiSettings.EnableConfigBackup != apiSettings.EnableConfigBackup ||
                _ApiSettings.AttachRequestMaxSize != apiSettings.AttachRequestMaxSize ||
                _ApiSettings.DetailedHealthCheckApiEnabled != apiSettings.DetailedHealthCheckApiEnabled)
            {
                _ApiSettings.PhotoType = apiSettings.PhotoType;
                _ApiSettings.PhotoURL = apiSettings.PhotoURL;
                _ApiSettings.PhotoHeaders = apiSettings.PhotoHeaders;
                _ApiSettings.ReportLogoPath = apiSettings.ReportLogoPath;
                _ApiSettings.UnofficialWatermarkPath = apiSettings.UnofficialWatermarkPath;
                _ApiSettings.CacheProvider = apiSettings.CacheProvider;
                _ApiSettings.BulkReadSize = apiSettings.BulkReadSize;
                _ApiSettings.IncludeLinkSelfHeaders = apiSettings.IncludeLinkSelfHeaders;
                _ApiSettings.EnableConfigBackup = apiSettings.EnableConfigBackup;
                _ApiSettings.AttachRequestMaxSize = apiSettings.AttachRequestMaxSize;
                _ApiSettings.DetailedHealthCheckApiEnabled = apiSettings.DetailedHealthCheckApiEnabled;

                // Clear the cache
                List<string> filter = new List<string>();
                UtilityCacheRepository cacheRepo = new UtilityCacheRepository(cacheProvider);
                cacheRepo.ClearCache(filter);
            }
        }

        private HttpContextTransactionFactory CreateTransactionFactory(ColleagueSettings collSettings = null, ClaimsPrincipal user = null)
        {
            if (collSettings == null)
            {
                var settings = settingsRepository.Get();
                if (settings != null)
                {
                    collSettings = settings.ColleagueSettings;
                }
            }
            if (user != null)
            {
                HttpContext.User = user;
            }

            return new HttpContextTransactionFactory(logger, collSettings, this.httpContextAccessor);
        }

        private ApiSettingsRepository CreateApiSettingsRepository()
        {
            var cookieValue = LocalUserUtilities.GetCookie(Request);
            if (string.IsNullOrEmpty(cookieValue))
            {
                throw new ColleagueWebApiException("Log in first");
            }
            var baseUrl = cookieValue.Split('*')[0];
            var token = cookieValue.Split('*')[1];

            var principal = JwtHelper.CreatePrincipal(token);

            var options = Microsoft.Extensions.Options.Options.Create<ApiSettingRepositorySettings>(apiSettingsRepositorySettings);
            return new ApiSettingsRepository(cacheProvider, CreateTransactionFactory(user: principal), logger, options);
        }

        private WebApiSettings BuildSettingsModel(Settings settings)
        {
            var model = new WebApiSettings();
            model.AccountName = settings.ColleagueSettings.DmiSettings.AccountName;
            model.ConnectionPoolSize = settings.ColleagueSettings.DmiSettings.ConnectionPoolSize;
            model.HostNameOverride = settings.ColleagueSettings.DmiSettings.HostNameOverride;
            model.IpAddress = settings.ColleagueSettings.DmiSettings.IpAddress;
            model.Port = settings.ColleagueSettings.DmiSettings.Port;
            model.Secure = settings.ColleagueSettings.DmiSettings.Secure;
            model.SharedSecret1 = settings.ColleagueSettings.DmiSettings.SharedSecret;
            model.SharedSecret2 = settings.ColleagueSettings.DmiSettings.SharedSecret;
            model.UseDasDatareader = settings.ColleagueSettings.GeneralSettings.UseDasDatareader;
            model.DasAccountName = settings.ColleagueSettings.DasSettings.AccountName;
            model.DasIpAddress = settings.ColleagueSettings.DasSettings.IpAddress;
            model.DasPort = settings.ColleagueSettings.DasSettings.Port;
            model.DasSecure = settings.ColleagueSettings.DasSettings.Secure;
            model.DasHostNameOverride = settings.ColleagueSettings.DasSettings.HostNameOverride;
            model.DasConnectionPoolSize = settings.ColleagueSettings.DasSettings.ConnectionPoolSize;
            model.DasUsername = settings.ColleagueSettings.DasSettings.DbLogin;
            model.DasPassword = settings.ColleagueSettings.DasSettings.DbPassword;
            model.OauthIssuerUrl = settings.OauthSettings.OauthIssuerUrl;
            model.OauthProxyUsername = settings.OauthSettings.OauthProxyLogin;
            model.OauthProxyPassword = settings.OauthSettings.OauthProxyPassword;

            string[] levels = new string[5];
            levels[0] = SourceLevels.Off.ToString(); //in serilog there is no off setting, this will be converted to fatal level
            levels[1] = SourceLevels.Error.ToString();
            levels[2] = SourceLevels.Warning.ToString();
            levels[3] = SourceLevels.Information.ToString();
            levels[4] = SourceLevels.Verbose.ToString();

            var selectList = levels.Select(x => new SelectListItem
            {
                Text = x,
                Value = x
            }).ToList();

            model.LogLevels = selectList;

            model.SupportedCultures = Bootstrapper.SupportedCultures.Select(c => new SelectListItem()
            {
                Text = c,
                Value = c
            });

            model.SupportedUiCultures = Bootstrapper.SupportedUICultures.Select(c => new SelectListItem()
            {
                Text = c,
                Value = c
            });

            model.DefaultCulture = settings.DefaultCulture;
            model.DefaultUiCulture = settings.DefaultUiCulture;

            if (settings.LogLevel.ToString() == "Fatal")
                model.LogLevel = "Off";
            else
                model.LogLevel = settings.LogLevel.ToString();

            model.ProfileName = settings.ProfileName;
            model.MachineKeySettingError = string.Empty;
            model.MachineKeySettingWarning = string.Empty;
            return model;
        }

        private Settings BuildSettingsDomain(WebApiSettings webApiSettings)
        {
            var collSettings = new ColleagueSettings();
            var dmiSettings = new DmiSettings();
            var dasSettings = new DasSettings();
            var generalSettings = new GeneralSettings();
            var oauthSettings = new OauthSettings();

            dmiSettings.AccountName = webApiSettings.AccountName;
            dmiSettings.ConnectionPoolSize = webApiSettings.ConnectionPoolSize;
            dmiSettings.HostNameOverride = webApiSettings.HostNameOverride;
            dmiSettings.IpAddress = webApiSettings.IpAddress;
            dmiSettings.Port = webApiSettings.Port;
            dmiSettings.Secure = webApiSettings.Secure;
            dmiSettings.SharedSecret = webApiSettings.SharedSecret2;
            collSettings.DmiSettings = dmiSettings;

            generalSettings.UseDasDatareader = webApiSettings.UseDasDatareader;
            collSettings.GeneralSettings = generalSettings;

            dasSettings.AccountName = webApiSettings.DasAccountName;
            dasSettings.IpAddress = webApiSettings.DasIpAddress;
            dasSettings.Port = webApiSettings.DasPort.HasValue ? webApiSettings.DasPort.Value : 0;
            dasSettings.Secure = webApiSettings.DasSecure;
            dasSettings.HostNameOverride = webApiSettings.DasHostNameOverride;
            dasSettings.ConnectionPoolSize = webApiSettings.DasConnectionPoolSize.HasValue ? webApiSettings.DasConnectionPoolSize.Value : 0;
            dasSettings.DbLogin = webApiSettings.DasUsername;
            dasSettings.DbPassword = webApiSettings.DasPassword;
            collSettings.DasSettings = dasSettings;

            oauthSettings.OauthIssuerUrl = webApiSettings.OauthIssuerUrl;
            oauthSettings.OauthProxyLogin = webApiSettings.OauthProxyUsername;
            oauthSettings.OauthProxyPassword = webApiSettings.OauthProxyPassword;

            return new Settings(collSettings, oauthSettings,
                SerilogLevelFromString(webApiSettings.LogLevel), webApiSettings.DefaultCulture, webApiSettings.DefaultUiCulture)
            { ProfileName = webApiSettings.ProfileName };
        }

        private Serilog.Events.LogEventLevel SerilogLevelFromString(string level)
        {
            switch (level?.ToLower())
            {
                case "error":
                    return Serilog.Events.LogEventLevel.Error;
                case "information":
                    return Serilog.Events.LogEventLevel.Information;
                case "warning":
                    return Serilog.Events.LogEventLevel.Warning;
                case "verbose":
                    return Serilog.Events.LogEventLevel.Verbose;
                default:
                    return Serilog.Events.LogEventLevel.Fatal;
            }
        }

        private ApiSettingsProfileModel GetApiSettingsProfileModel()
        {
            ApiSettingsProfileModel model = new ApiSettingsProfileModel();

            // read xml setting to get the current profile name...
            var domainSettings = settingsRepository.Get();
            model.CurrentProfileName = domainSettings.ProfileName;

            // get the existing profile names in Colleague
            var apiSettingsRepo = CreateApiSettingsRepository();
            IEnumerable<string> profileNames = apiSettingsRepo.GetNames();

            // if profile name...
            if (!string.IsNullOrEmpty(model.CurrentProfileName))
            {
                // see if the current profile exists in Colleague (if present)
                bool exists = false;
                if (profileNames != null && profileNames.Count() > 0 && !string.IsNullOrEmpty(model.CurrentProfileName))
                {
                    string current = model.CurrentProfileName.Replace(" ", "").ToUpper();
                    foreach (string name in profileNames)
                    {
                        if (name.Replace(" ", "").ToUpper() == current)
                        {
                            exists = true;
                            break;
                        }
                    }
                }
                if (!exists)
                {
                    // local name does not exist in colleague...
                    ViewBag.error = "The current profile name specified by the local configuration does not exist in Colleague - click save to create it.";
                    model.NewProfileName = model.CurrentProfileName;
                    model.CurrentProfileName = "";
                }
            }
            else
            {
                if (profileNames != null && profileNames.Count() > 0)
                {
                    // local name is not defined, and there are names in colleague...
                    ViewBag.error = "Please select either an existing profile or specify a new profile name to associate with this Web API.";
                }
                else
                {
                    // suggest a new one as none exist, anywhere...
                    ViewBag.error = "No profiles exist. A new profile name has been suggested based on the current website name.";
                    var suggestion = hostEnvironment.ApplicationName;
                    suggestion = suggestion.Trim().Replace(" ", string.Empty).ToUpper();
                    model.NewProfileName = suggestion;
                }
            }

            if (profileNames != null && profileNames.Count() > 0)
            {
                foreach (string profileName in profileNames)
                {
                    if (profileName.ToUpper() != model.CurrentProfileName.ToUpper())
                    {
                        (model.ExistingProfileNames as IList<SelectListItem>).Add(new SelectListItem() { Value = profileName, Text = profileName });
                    }
                }
            }

            var emptySelectListItem = new SelectListItem() { Text = "select...", Value = "" };
            (model.ExistingProfileNames as IList<SelectListItem>).Insert(0, emptySelectListItem);
            model.SelectedExistingProfileName = emptySelectListItem;

            return model;
        }

        /// <summary>
        /// Copies source directory contents to target
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="targetDirectory"></param>
        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        /// <summary>
        /// Copies all files from source directory contents to target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

       
    }
}

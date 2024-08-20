// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Middleware;
using Ellucian.Colleague.Api.Models;
using Ellucian.Colleague.Api.Options;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Client;
using Ellucian.Logging;
using Ellucian.Web.Adapters;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Bootstrapping;
using Ellucian.Web.Mvc.Filter;
using Ellucian.Web.Mvc.Session;
using Ellucian.Web.Resource;
using Ellucian.Web.Resource.Repositories;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Core;
using Serilog.Events;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ellucian.Colleague.Api
{
    /// <summary>
    /// Manages the initial configuration of the web API layer, including container registration.
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// When set true in appSettings, the logic to convert rules into .NET expression trees will be disabled.
        /// </summary>
        private static string ExecuteAllRulesInColleague = "ExecuteAllRulesInColleague";
        private static LoggingLevelSwitch _loggingLevelSwitch = new LoggingLevelSwitch() { MinimumLevel = LogEventLevel.Error };
        //private static string LogFile = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "Logs", "ColleagueWebApi.log");
        private static string LogCategory = "ColleagueAPIApplication";
        private static string LogComponentName = "ColleagueWebAPI";
        internal static string colleagueTimeZone = "";
        private static string baseResourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        private static string resourceCustomizationFilePath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + @"\ResourceCustomization.json";

        private const string HedtechIntegrationStudentUnverifiedGradesSubmissionsFormat = "application/vnd.hedtech.integration.student-unverified-grades-submissions.v{0}+json";
        private const string HedtechIntegrationStudentTranscriptGradesAdjustmentsFormat = "application/vnd.hedtech.integration.student-transcript-grades-adjustments.v{0}+json";

        /// <summary>
        /// This provides the application discriminator for data protection APIs.
        /// </summary>
        public const string DataProtectionApplicationDiscriminator = "Ellucian Colleague";


        /// <summary>
        /// This property set/get an instance of the Serilog LoggingLevelSwitch
        /// </summary>
        public static LoggingLevelSwitch LoggingLevelSwitch { get { return _loggingLevelSwitch; } }
        /// <summary>
        /// The setting for the Colleague Time Zone
        /// </summary>
        public static string ColleagueTimeZone { get { return colleagueTimeZone; } }

        /// <summary>
        /// For formatting purposes.
        /// </summary>
        public static readonly IEnumerable<string> SupportedCultures = new[] { "en-US", "en-CA", "fr-CA" };
        /// <summary>
        /// For language UI elements.
        /// </summary>
        public static readonly IEnumerable<string> SupportedUICultures = new[] { "en", "es" };

        /// <summary>
        /// For key management strategies.
        /// </summary>
        public static IEnumerable<string> KeyManagementStrategies { get; private set; }

        /// <summary>
        /// Default UI culture (i.e. prior to any user-specific settings)
        /// </summary>
        public static string DefaultUiCulture = "en";
        /// <summary>
        /// Default culture (i.e. prior to any user-specific settings)
        /// </summary>
        public static string DefaultCulture = "en-US";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="appConfiguration"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async static Task<IServiceCollection> ConfigureServices(IServiceCollection services, IConfiguration appConfiguration, Serilog.ILogger logger)
        {
            KeyManagementStrategies = Enum.GetNames<DataProtectionMode>();
            /*
             * NOTE: Order can be important when setting up the container, so be careful!
             */
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "X-CustomCredentials";
                options.DefaultAuthenticateScheme = "X-CustomCredentials";
                options.DefaultChallengeScheme = "X-CustomCredentials";
            })
            .AddScheme<ColleagueBasicAuthorizationOptions, ColleagueBasicAuthenticationHandler>(
                "Basic", options => { })
            .AddScheme<ColleagueBearerAuthorizationOptions, ColleagueBearerAuthenticationHandler>(
                "Bearer", options => { })
            .AddScheme<ColleagueJwtAuthorizationOptions, ColleagueJwtAuthenticationHandler>(
                "X-CustomCredentials", options => { });

            services.AddAuthorization(options =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder("Basic", "Bearer", "X-CustomCredentials");
                defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });


            // [1] setup connection settings
            // determine the manner of data protection
            var dpSettings = appConfiguration.GetSection(DataProtectionSettings.SettingsKey).Get<DataProtectionSettings>();
            if (dpSettings == null)
            {
                dpSettings = new DataProtectionSettings()
                {
                    DataProtectionMode = DataProtectionMode.FixedKey,
                    FixedKeyManagerKey = Environment.MachineName,
                    NetworkPath = string.Empty,
                    AwsKeyPath = string.Empty
                };
            }
            services.AddSingleton(dpSettings);

            switch (dpSettings.DataProtectionMode)
            {
                case DataProtectionMode.NetworkShare:
                    var keyDirectory = (string.IsNullOrEmpty(dpSettings.NetworkPath) || !Directory.Exists(dpSettings.NetworkPath))
                        ? Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory") as string, "ProtectionKeys")
                        : dpSettings.NetworkPath;
                    services.AddDataProtection(options =>
                    {
                        options.ApplicationDiscriminator = DataProtectionApplicationDiscriminator;

                    }).PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));
                    break;
                case DataProtectionMode.AWS:
                    // we can find the key path by building it from variables.json
                    var awsKeyPath = string.Empty;

                    // we prefer to get these from the variables.json
                    var variablesPath = "/ellucian/variables.json";
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        variablesPath = @"C:\ellucian\variables.json";
                    }
                    if (File.Exists(variablesPath))
                    {
                        var jsonSettings = new JsonSerializerSettings();
                        jsonSettings.Converters.Add(new ExpandoObjectConverter());
                        jsonSettings.Converters.Add(new StringEnumConverter());
                        var json = System.IO.File.ReadAllText(variablesPath);

                        dynamic variablesConfig = JsonConvert.DeserializeObject<ExpandoObject>(json, jsonSettings);

                        var variablesDictionary = ((IDictionary<string, object>)variablesConfig);

                        if (!variablesDictionary.TryGetValue("COLLEAGUE_ENV", out object clientId))
                        {
                            variablesDictionary.TryGetValue("colleague_env", out clientId);
                        }

                        if (!variablesDictionary.TryGetValue("COLLEAGUE_RUNTIME_ENV", out object environment))
                        {
                            variablesDictionary.TryGetValue("colleague_runtime_env", out environment);
                        }

                        awsKeyPath = $"/Colleague/SaaS/{clientId}/{environment}/Api/DataProtection";
                    }
                    // or directly in our appsettings.json
                    if (string.IsNullOrWhiteSpace(awsKeyPath))
                    {
                        awsKeyPath = dpSettings.AwsKeyPath;
                    }

                    services.AddDataProtection(options =>
                    {
                        options.ApplicationDiscriminator = DataProtectionApplicationDiscriminator;

                    }).PersistKeysToAWSSystemsManager(awsKeyPath);
                    break;
                default:
                    services.AddFixedKeyDataProtection(DataProtectionApplicationDiscriminator, settings =>
                    {
                        settings.Secret = dpSettings.FixedKeyManagerKey ?? Environment.MachineName;
                    });
                    break;
            }

            // we need an instance for the XmlSettingsRepository, so we build a service provider here to limit duplication of services
            var dataprotectionProvider = services.BuildServiceProvider().GetDataProtectionProvider();
            var protector = dataprotectionProvider.CreateProtector("Colleague");

            // set cookie suffix
            LocalUserUtilities.CookieId = appConfiguration.GetSection("ApiSettings")["CookieSuffix"];

            services.AddSingleton<JwtHelper, JwtHelper>();
            services.AddSingleton(typeof(ILogger), typeof(Logger<Startup>));
            services.AddSingleton(typeof(ILogger), typeof(Logger<HttpContextTransactionFactory>));
            services.AddHttpContextAccessor();
            services.AddTransient<ValidateAntiForgeryTokenCustomFilterAttribute, ValidateAntiForgeryTokenCustomFilterAttribute>();
            services.AddTransient<SessionCookieManager, SessionCookieManager>();
            services.AddTransient<CustomCredentialsResponseFilter, CustomCredentialsResponseFilter>();

            // pub/sub settings
            try
            {
                services.Configure<ColleaguePubSubOptions>(appConfiguration.GetSection(ColleaguePubSubOptions.APPSETTINGS_KEY));
                // Redis Server Configuration and Connection
                var cacheManagementPubSubOptions = appConfiguration.GetSection(ColleaguePubSubOptions.APPSETTINGS_KEY).Get<ColleaguePubSubOptions>();
                if (cacheManagementPubSubOptions.CacheManagementEnabled || cacheManagementPubSubOptions.ConfigManagementEnabled)
                {
                    logger.Debug("attempting to connect to redis");
                    var configOptions = new ConfigurationOptions();
                    configOptions.AbortOnConnectFail = false;
                    configOptions.ConnectRetry = 3;
                    configOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);
                    configOptions.EndPoints.Add(cacheManagementPubSubOptions.ConnectionString);
                    var connMultiplexer = ConnectionMultiplexer.Connect(configOptions);
                    var pubSubSubscriber = connMultiplexer.GetSubscriber();
                    services.AddSingleton(pubSubSubscriber);
                }
                else
                {
                    services.AddSingleton<ISubscriber>(new NullSubscriber());
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error connecting to pub/sub endpoint: " + ex.Message);
            }

            var settingsFileOptions = appConfiguration.GetSection(SettingsFileOptions.Key).Get<SettingsFileOptions>();

            var settingsFilePath = settingsFileOptions.Path;
            var settingsBackupFilePath = settingsFileOptions.BackupPath;
            if (settingsFileOptions.IsRelative)
            {
                settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsFilePath);
                settingsBackupFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsBackupFilePath);
            }

            var xmlSettingsRepository = new XmlSettingsRepository(settingsFilePath, settingsBackupFilePath, dataprotectionProvider);
            services.AddSingleton<ISettingsRepository>(xmlSettingsRepository);

            var saveSettings = false; // Settings that are changed at this stage, need to be saved 
            var settings = xmlSettingsRepository.Get();
            var collSettings = settings.ColleagueSettings;
            var dmiSettings = settings.ColleagueSettings.DmiSettings;
            services.AddSingleton<ColleagueSettings>(collSettings);
            services.AddSingleton<DmiSettings>(dmiSettings);
            await DmiConnectionPool.SetSizeAsync(
                DmiConnectionPool.ConnectionPoolName(collSettings.DmiSettings.IpAddress, collSettings.DmiSettings.Port, collSettings.DmiSettings.Secure),
                collSettings.DmiSettings.ConnectionPoolSize);
            await Ellucian.Dmi.Client.Das.DasSessionPool.SetSizeAsync(collSettings.DasSettings.ConnectionPoolSize);

            // update the log level switch
            _loggingLevelSwitch.MinimumLevel = settings.LogLevel;

            // setup audit log
            var auditLogFilePath = appConfiguration.GetSection("ApiSettings")["AuditLogPath"];
            var auditLog = new AuditLoggingAdapter(auditLogFilePath, "ColleagueWebAPI");
            services.AddSingleton(auditLog);

            // set UI culture
            var supportedUiCultures = new List<CultureInfo>();
            foreach (var supportedUiCulture in SupportedUICultures)
            {
                supportedUiCultures.Add(new CultureInfo(supportedUiCulture));
            };
            SetDefaultUiCulture(settings.DefaultUiCulture, supportedUiCultures);
            if (DefaultUiCulture != settings.DefaultUiCulture)
            {
                settings.DefaultUiCulture = DefaultUiCulture;
                saveSettings = true;
            }
            // set culture
            var supportedCultures = new List<CultureInfo>();
            foreach (var supportedCulture in SupportedCultures)
            {
                supportedCultures.Add(new CultureInfo(supportedCulture));
            };
            SetDefaultCulture(settings.DefaultCulture, supportedCultures);
            if (DefaultCulture != settings.DefaultCulture)
            {
                settings.DefaultCulture = DefaultCulture;
                saveSettings = true;
            }

            if (saveSettings) xmlSettingsRepository.Update(settings);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedUiCultures;
                options.DefaultRequestCulture = new RequestCulture(DefaultUiCulture);

                options.AddInitialRequestCultureProvider(new CustomRequestCultureProvider(async context =>
                {
                    var currentCulture = DefaultCulture;
                    var currentUiCulture = DefaultUiCulture;

                    // TODO: in the future, this can be where we resolve for a user their desired UI culture
                    // or it can be broken out into class

                    var requestCultureResult = new ProviderCultureResult(currentCulture, currentUiCulture);

                    return requestCultureResult;
                }));
            });

			// we need to explicitly register 1252 code page
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var cookiePolicySettings = appConfiguration.GetSection("CookiePolicySettings").Get<CookiePolicySettings>();
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.Secure = cookiePolicySettings.CookieSecurePolicy;
            });

            // [2] setup logging (depends on settings). Override default log template with one that has timestamp.
            // TODO: ensure the log format is as desired

            // [3] critical common components (depend on logging and settings)
            services.AddTransient<ISessionRepository, ColleagueSessionRepository>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IColleagueTransactionFactory, HttpContextTransactionFactory>();
            services.AddTransient<ICurrentUserFactory, ThreadCurrentUserFactory>();

            // Web API Cache Initialization
            //   This is no longer HTTP runtime supported
            services.AddSingleton<ICacheProvider, MemoryCacheProvider>();

            services.Configure<JwtHelperSettings>(appConfiguration.GetSection("JwtHelperSettings"));
            services.Configure<ApiSettingRepositorySettings>(appConfiguration.GetSection("ApiSettings"));
            services.Configure<MemoryCacheOptions>(appConfiguration.GetSection("CacheSettings"));
            // [4] setup api settings (depends on settings, logging, and common components)
            services.AddTransient<IApiSettingsRepository, ApiSettingsRepository>();
            services.AddTransient<AppConfigUtility, AppConfigUtility>();
            services.AddTransient<EedmResponseFilter, EedmResponseFilter>();

            // rules "engine"
            var ruleAdapterRegistry = new RuleAdapterRegistry();
            services.AddSingleton<RuleAdapterRegistry>(ruleAdapterRegistry);
            var rulesFlag = appConfiguration[ExecuteAllRulesInColleague];
            var config = new RuleConfiguration();
            if (!string.IsNullOrEmpty(rulesFlag) && "TRUE".Equals(rulesFlag.ToUpper()))
            {
                config.ExecuteAllRulesInColleague = true;
            }
            services.AddSingleton<RuleConfiguration>(config);

            // [5] required repository for extendedRouteContraint used for extensibility
            services.AddTransient<IExtendRepository, ExtendRepository>();

            var startupProvider = services.BuildServiceProvider();
            var apiSettingsInstance = startupProvider.GetService<IApiSettingsRepository>();

            var apiSettings = new ApiSettings("null");
            try
            {
                apiSettings = apiSettingsInstance.Get(settings.ProfileName);
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(e.Message))
                {
                    string m = e.Message.ToLower();
                    if (m.Contains("cannot access file") && m.Contains("web.api.config"))
                    {
                        logger.Error("WEB.API.CONFIG has not been configured for anonymous access on WSPD! Anything using the API settings may fail.");
                    }
                }
                logger.Error(e, "Unable to read API Settings from colleague. Profile Name: {0}", settings.ProfileName);
            }

            services.AddSingleton<ApiSettings>(apiSettings);
            colleagueTimeZone = apiSettings.ColleagueTimeZone;

            // [5] Resource Repository 
            var localResourceRepository = new LocalResourceRepository(baseResourcePath, resourceCustomizationFilePath);
            services.AddSingleton<IResourceRepository>(localResourceRepository);

            // the following calls all depend on all assemblies being loaded...
            LoadAllAssembliesIntoAppDomain(logger);

            var meLogger = startupProvider.GetService<ILogger>();
            var httpContextAccessor = startupProvider.GetService<IHttpContextAccessor>();

            SetIntlValues(dmiSettings, meLogger, httpContextAccessor);

            DependencyRegistration.AddRegisteredTypes(services);
            RegisterAdapters(services, meLogger);
            BootstrapModules(startupProvider, meLogger);

            return services;
        }

        internal static void SetDefaultCulture(string defaultCulture, IEnumerable<CultureInfo> supportedCultures)
        {
            Bootstrapper.DefaultCulture = (supportedCultures ?? new List<CultureInfo>()).Any(c => c.Name.Equals(defaultCulture, StringComparison.OrdinalIgnoreCase)) ? defaultCulture : supportedCultures.FirstOrDefault()?.Name;

        }

        internal static void SetDefaultUiCulture(string defaultUiCulture, IEnumerable<CultureInfo> supportedUiCultures)
        {
            DefaultUiCulture = (supportedUiCultures ?? new List<CultureInfo>()).Any(c => c.Name.Equals(defaultUiCulture, StringComparison.OrdinalIgnoreCase)) ? defaultUiCulture : supportedUiCultures.FirstOrDefault()?.Name;
        }

        private static void LoadAllAssembliesIntoAppDomain(Serilog.ILogger logger)
        {
            var binDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var files = Directory.GetFiles(binDirectory, "*.dll", SearchOption.AllDirectories);
            AssemblyName a = null;
            foreach (var s in files)
            {
                try
                {
                    a = AssemblyName.GetAssemblyName(s);
                    if (!AppDomain.CurrentDomain.GetAssemblies().Any(
                        assembly => AssemblyName.ReferenceMatchesDefinition(
                        assembly.GetName(), a)))
                    {
                        Assembly.LoadFrom(s);
                    }
                }
                catch (BadImageFormatException ex)
                {
                    // native assembly
                }
            }

            // debug
            if (logger.IsEnabled(LogEventLevel.Debug))
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                List<Assembly> sortedLoadedAssemblies = loadedAssemblies.OrderBy(x => x.FullName).ToList();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Loaded AppDomain Assemblies:");
                foreach (var asm in sortedLoadedAssemblies)
                {
                    sb.AppendLine(asm.FullName);
                }
                logger.Debug(sb.ToString());
            }
        }


        private static void RegisterAdapters(IServiceCollection services, ILogger logger)
        {
            try
            {
                var baseAdapterInterface = typeof(ITypeAdapter);
                var baseMappingProfileClass = typeof(AutoMapper.Profile);
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                // Select adapter types from this assembly that inherits from ITypeAdapter; the adapter interfaces are excluded
                // since concrete types are required
                var ellucianLoadedAssemblies = loadedAssemblies.Where(x => x.GetName().Name.StartsWith("ellucian", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                var adapterTypes = ellucianLoadedAssemblies.SelectMany(assembly => assembly.GetLoadableTypes().Where(x => baseAdapterInterface.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract && !x.ContainsGenericParameters));
                var profileTypes = ellucianLoadedAssemblies.SelectMany(assembly => assembly.GetLoadableTypes().Where(x => baseMappingProfileClass.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract && !x.ContainsGenericParameters));

                ISet<ITypeAdapter> adapterCollection = new HashSet<ITypeAdapter>();
                var mappingProfiles = new List<AutoMapper.Profile>();
                foreach (var profileType in profileTypes)
                {
                    mappingProfiles.Add(profileType.GetConstructor(Array.Empty<Type>()).Invoke(null) as AutoMapper.Profile);
                }
                // we need to find all profiles to put them in
                AdapterRegistry registry = new AdapterRegistry(adapterCollection, mappingProfiles, logger);

                StringBuilder debug = new StringBuilder();
                debug.AppendLine("RegisterAdapters:");

                foreach (var adapterType in adapterTypes)
                {
                    // Instantiate 
                    var adapterObject = adapterType.GetConstructor(new Type[] { typeof(IAdapterRegistry), typeof(ILogger) }).Invoke(new object[] { registry, logger }) as ITypeAdapter;
                    registry.AddAdapter(adapterObject);
                    debug.AppendLine("added: " + adapterObject.GetType().ToString());
                }

                logger.LogDebug(debug.ToString());

                // Register the adapter registry as a singleton instance
                services.AddSingleton<IAdapterRegistry>(registry);
            }
            catch (ReflectionTypeLoadException e)
            {
                logger.LogError("RegisterAdapters error(s)", e);
                if (e.LoaderExceptions != null)
                {
                    foreach (var le in e.LoaderExceptions)
                    {
                        logger.LogError("Loader Exception: " + le.Message);
                    }
                }
                throw e;
            }
        }

        /// <summary>
        /// Determines which assemblys are able to be loaded
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        private static void BootstrapModules(IServiceProvider serviceProvider, ILogger logger)
        {
            try
            {
                var moduleBootstrapperInterface = typeof(IModuleBootstrapper);
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();


                var ellucianLoadedAssemblies = loadedAssemblies.Where(x => x.GetName().Name.StartsWith("ellucian", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                var interfaceImplementers = ellucianLoadedAssemblies.SelectMany(assembly => assembly.GetLoadableTypes().Where(x => moduleBootstrapperInterface.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract && !x.ContainsGenericParameters));
                if (interfaceImplementers != null)
                {
                    StringBuilder debug = new StringBuilder();
                    debug.AppendLine("BootstrapModules:");

                    foreach (var module in interfaceImplementers)
                    {
                        try
                        {
                            var instance = Activator.CreateInstance(module) as IModuleBootstrapper;
                            instance.BootstrapModule(serviceProvider);
                            instance = null;
                            debug.AppendLine("executed: " + module.Name);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Module bootstrapping failed");
                        }
                    }

                    logger.LogDebug(debug.ToString());
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                logger.LogError("BootstrapModules error(s)", e);
                if (e.LoaderExceptions != null)
                {
                    foreach (var le in e.LoaderExceptions)
                    {
                        logger.LogError("Loader Exception: " + le.Message);
                    }
                }
                throw e;
            }
        }

        private static void SetIntlValues(DmiSettings dmiSettings, ILogger meLogger, IHttpContextAccessor httpContextAccessor)
        {
            try
            {
                var servMan = new ServerConfigurationManager(meLogger, ApplicationServerConfigurationManager.Instance);
                DmiConnectionFactory dmiConnectionFactory = new DmiConnectionFactory(dmiSettings, httpContextAccessor);
                var conn = dmiConnectionFactory.CreateServerConnection();
                if (conn != null)
                {
                    servMan.GetIntlSettings(conn);
                }
            }
            catch (Exception e)
            {
                meLogger.LogError(e, "Unexpected error while discovering INTL settings.");
            }
        }
    }
}

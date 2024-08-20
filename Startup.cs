// Copyright 2023-2024 Ellucian Company L.P. and its affiliates.
// This Startup file is based on ASP.NET Core new project templates and is included
// as a starting point for DI registration and HTTP request processing pipeline configuration.
// This file will need updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core startup files, see https://docs.microsoft.com/aspnet/core/fundamentals/startup
using Ellucian.App.Config.Storage.Service.Client;
using Ellucian.Colleague.Api.Converters;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Dmi.Client;
using Ellucian.Dmi.Client.Das;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Serilog;
using System.IO;
using System.Text.Json;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading;
using Microsoft.AspNetCore.ResponseCompression;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;
using Ellucian.Web.Http.Configuration;
using System.Reflection;
using Ellucian.Colleague.Configuration;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Ellucian.Colleague.Api.Models;
using Ellucian.Web.Cache;
using StackExchange.Redis;
using Ellucian.Colleague.Api.Middleware;

namespace Ellucian.Colleague.Api
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        private IApplicationBuilder _app;
        private IHostApplicationLifetime _lifetime;
        private static PosixSignalRegistration sigTermRegistration;
        private static PosixSignalRegistration sigIntRegistration;


        /// <summary>
        /// The app startup method.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data"));
        }

        /// <summary>
        /// The configuration for the application.
        /// </summary>
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary>
        /// Configures ASP.NET services
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            Bootstrapper.ConfigureServices(services, Configuration, Log.Logger);

            services.AddResponseCompression(options =>
            {
                // .Append(TItem) is only available on Core.
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
                ////Example of using excluded and wildcard MIME types:
                ////Compress all MIME types except various media types, but do compress SVG images.
                //options.MimeTypes = new[] { "*/*", "image/svg+xml" };
                options.ExcludedMimeTypes = new[] { "image/*", "audio/*", "video/*" };
            });

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<LoggingExceptionFilter>();
                options.Filters.Add<PagedActionResultFilter>(1);
                options.Filters.Add<GlobalPagingFilter>();
                options.Filters.Add<SortingFilter>();
                options.Filters.Add<GlobalFilteringFilter>();
                options.Filters.Add<CustomMediaTypeAttributeFilter>();
                options.Filters.Add<EthosModelBinderErrorFilter>();
            })
                // Newtonsoft.Json is added for compatibility reasons
                // The recommended approach is to use System.Text.Json for serialization
                // Visit the following link for more guidance about moving away from Newtonsoft.Json to System.Text.Json
                //// https://docs.microsoft.com/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to
                .AddNewtonsoftJson(options =>
                {
                    options.UseMemberCasing();
                    options.SerializerSettings.Error = delegate (object? sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                    {
                        var s = args;
                        args.ErrorContext.Handled = true;
                    };
                    // Sets the custom json date time converter to override how json date/time strings
                    // are handled using the ColleagueDateTimeConverter.
                    options.SerializerSettings.DateParseHandling = DateParseHandling.None;
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;

                    options.SerializerSettings.Converters.Add(new ColleagueDateTimeConverter(Bootstrapper.ColleagueTimeZone));
                })
                .AddXmlSerializerFormatters();
            //.AddJsonOptions(options =>
            //{
            //    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            //    // this is for System.Text.Json
            //});

            services.Configure<MvcOptions>(options =>
            {
                ConfigureMvcOptions(options);
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddMvc(options =>
            {
                options.MaxModelBindingCollectionSize = Int32.MaxValue;
            });
            services.AddAntiforgery(options =>
            {
                options.HeaderName = options.FormFieldName;
            });
            services.AddSwaggerGen();

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                options.AddServerHeader = false;
            });
            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });


            services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddJavaScriptBundle("/bundles/jquery", "/scripts/jquery-3.7.1.min.js");
                pipeline.AddJavaScriptBundle("/bundles/jqueryui", "/scripts/jquery-ui-1.32.2.min.js");
                pipeline.AddJavaScriptBundle("/bundles/jqueryval", "/scripts/jquery.unobtrusive*", "/scripts/jquery.validate*");

                pipeline.AddJavaScriptBundle("/bundles/globalscripts", "/scripts/knockout-3.4.0.js",
                    "/scripts/knockout.validation.js",
                    "/scripts/global.js",
                    "/scripts/jquery.ui.plugin_responsive-table.js");

                pipeline.AddCssBundle("/bundles/css", "/*.css");
                pipeline.AddCssBundle("/bundles/themes/base/css", "/themes/base/*.css", "/Site.css");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// Configures the ASP.NET application
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="lifetime"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            var useForwardedHeaders = Configuration.GetValue<bool>("UseForwardedHeaders");
            if (useForwardedHeaders)
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions()
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            var pathBase = Configuration.GetValue<string>("PathBase");
            if (!string.IsNullOrEmpty(pathBase))
            {
                // should start with /
                if (!pathBase.StartsWith("/"))
                {
                    pathBase = "/" + pathBase;
                }
                app.UsePathBase(pathBase);
            }

            app.UseResponseCompression();
            app.UseRequestLocalization();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCookiePolicy();

            app.UseStatusCodePages(async context =>
            {
                if (context.HttpContext.Response.StatusCode == 401)
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    var responseMessage = "{\n\"Message\": \"Authorization has been denied for this request.\"\n}";
                    context.HttpContext.Response.ContentLength = responseMessage.Length;
                    await context.HttpContext.Response.WriteAsync(responseMessage);
                }
            });

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseAuthentication();

            app.UseWebOptimizer();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "ColleagueAreas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToController("NotSupportedRoute", "Home");
            });

            _app = app;
            _lifetime = lifetime;
            lifetime.ApplicationStopping.Register(OnAppStopping);

            EllucianLicenseProvider.RefreshLicense(AppDomain.CurrentDomain.GetData("DataDirectory").ToString());

            // listen for POSIX commands to interrupt or terminate and do so promptly
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                sigTermRegistration = PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
                {
                    context.Cancel = false;
                });
                sigIntRegistration = PosixSignalRegistration.Create(PosixSignal.SIGINT, context =>
                {
                    context.Cancel = false;
                });
            }

            // cache management handling
            // set things up for listening
            var pubsubOptions = _app.ApplicationServices.GetService<IOptions<ColleaguePubSubOptions>>().Value ?? new ColleaguePubSubOptions();
            if (pubsubOptions.CacheManagementEnabled)
            {
                var pubsubSubscriber = _app.ApplicationServices.GetService<ISubscriber>();
                var cache = _app.ApplicationServices.GetService<ICacheProvider>();
                var cacheChannel = new RedisChannel((pubsubOptions.Namespace ?? ColleaguePubSubOptions.DEFAULT_NAMESPACE) + "/" + (pubsubOptions.CacheChannel ?? ColleaguePubSubOptions.DEFAULT_CACHE_CHANNEL), RedisChannel.PatternMode.Literal);
                var queue = pubsubSubscriber.Subscribe(cacheChannel);
                queue.OnMessage(async message =>
                {
                    if (message.Message.HasValue && cache != null)
                    {
                        try
                        {
                            var notification = System.Text.Json.JsonSerializer.Deserialize<PubSubCacheNotification>((string)message.Message);
                            if (notification.HostName != Environment.MachineName)
                            {
                                // it didn't originate here, let's remove the keys
                                try
                                {
                                    var itemsRemoved = new List<string>();
                                    foreach (var key in notification.CacheKeys ?? Array.Empty<string>())
                                    {
                                        if (cache.Contains(key))
                                        {
                                            _ = cache.Remove(key);
                                            itemsRemoved.Add(key);
                                        }
                                    }
                                }
                                finally
                                {
                                    _ = "This key had a removal issue";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _ = "Do we care?";
                        }
                    }
                });

            }

            ConfigUpdateAndMonitor(pubsubOptions);

        }

        private void ConfigureMvcOptions(MvcOptions mvcOptions)
        {
            mvcOptions.MaxModelBindingCollectionSize = Int32.MaxValue;
            mvcOptions.AllowEmptyInputInBodyModelBinding = true;

            var newtonsoft = mvcOptions.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();
            // make this the first formatter
            mvcOptions.OutputFormatters.Remove(newtonsoft);
            mvcOptions.OutputFormatters.Insert(0, newtonsoft);
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json")
            {
                Charset = Encoding.UTF8.WebName
            });
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v1+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v2+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v3+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v4+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v5+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v6+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v7+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v8+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v9+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v10+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v11+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v12+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v13+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue(string.Format(RouteConstants.HedtechIntegrationStudentUnverifiedGradesSubmissionsFormat, "1.0.0")));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue(string.Format(RouteConstants.HedtechIntegrationStudentTranscriptGradesAdjustmentsFormat, "1.0.0")));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.bulk-requests.v1.0.0+json"));
            newtonsoft.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.bulk-requests.v1+json"));

        }

        /// <summary>
        /// When app stops gracefully
        /// </summary>
        public async void OnAppStopping()
        {
            try
            {
                // DMI clean up
                await DmiConnectionPool.CloseAllConnectionsAsync();
            }
            catch (Exception ex)
            {
                _app.ApplicationServices.GetService<Microsoft.Extensions.Logging.ILogger>().LogError(ex, "Error during DMI clean up");
            }
            try
            {
                // DMI clean up
                await DasSessionPool.CloseAllConnectionsAsync();
            }
            catch (Exception ex)
            {
                _app.ApplicationServices.GetService<Microsoft.Extensions.Logging.ILogger>().LogError(ex, "Error during DMI clean up");
            }
        }

        #region config update / monitor

        private async void ConfigUpdateAndMonitor(ColleaguePubSubOptions pubsubOptions)
        {
            var logger = _app.ApplicationServices.GetService<Microsoft.Extensions.Logging.ILogger>();
            var appConfigUtility = _app.ApplicationServices.GetService<AppConfigUtility>();
            int atStep = 1;
            try
            {
                // Only execute this code block if config monitoring is configured and this is a SaaS environment
                if (appConfigUtility.ConfigServiceClientSettings != null && appConfigUtility.ConfigServiceClientSettings.IsSaaSEnvironment)
                {
                    // ***FIRST***, grab current config object and its checksum by calling this app's own "gather config data" method

                    var configObject = appConfigUtility.GetApiConfigurationObject();
                    logger.LogInformation("Config storage client created with namespace " + appConfigUtility.StorageServiceClient.NameSpace);
                    // *Required*: set the current config's checksum for the monitor client so it can do long polling later.
                    var currentChecksum = Utilities.GetMd5ChecksumString(configObject.ConfigData);
                    ConfigStorageServiceHttpClient.CurrentConfigChecksum = currentChecksum;


                    // ***SECOND***, send a GET /latest to get latest config record - not going to do long poll here to avoid delaying app start

                    atStep = 2;
                    logger.LogInformation("Getting latest config...");
                    App.Config.Storage.Service.Client.Models.Configuration latestConfig = null;
                    try
                    {
                        latestConfig = await appConfigUtility.StorageServiceClient.GetLatestAsync();
                    }
                    catch (InvalidCredentialException ice)
                    {
                        logger.LogError(ice, "Invalid credentials. Cannot start config monitor job.");
                        return;
                    }
                    catch (Exception e)
                    {
                        // ignore any http exception here in case the storage service is temporarily down, in which
                        // case we still want to start a monitor thread that will wait for the service to come back on.
                        logger.LogError(e, "Exception ocurred getting latest config record from storage service.");
                    }

                    // ***THIRD***, if new config data found, apply config update and shut down appdomain. Otherwise start the background monitor thread.

                    logger.LogInformation(string.Format("Current checksum: {0}; latest checksum: {1}",
                        currentChecksum,
                        (latestConfig == null) ? "null" : latestConfig.Checksum));

                    bool startMonitorThread = true;
                    if (latestConfig != null && latestConfig.Checksum != currentChecksum)
                    {
                        // Check whether this latest config has actually been restored before.
                        var lastRestoredChecksum = Utilities.GetLastRestoredChecksum();
                        if (!string.IsNullOrWhiteSpace(lastRestoredChecksum) && lastRestoredChecksum == latestConfig.Checksum)
                        {
                            logger.LogInformation("Latest config found, but it has already been restored previously. This means the last restore performed a merge. Sending a new backup...");
                            // This "latest" backup config has already been restored previously. The fact that this instance's 
                            // current checksum is different than the "latest's" means the last restore performed a merge of
                            // different config data versions, which resulted in a new unique checksum. 

                            // So, instead of restoring this obsolete "latest" config data, we will submit the instance's current
                            // config data as the new backup config data, which will serve as the new "latest".

                            // Note: we're NOT processing staging config file here, since it would have been
                            // already processed and became part of the new config data that's being sent to EACSS below. 

                            string username = "Application_Start";
                            try
                            {
                                var result = appConfigUtility.StorageServiceClient.PostConfigurationAsync(
                                    configObject.Namespace, configObject.ConfigData, username,
                                    configObject.ConfigVersion, configObject.ProductId, configObject.ProductVersion).GetAwaiter().GetResult();
                                logger.LogInformation("Post-merge backup sent to config storage.");

                                // after submitting the merged checksum, set the lastrestoredchecksum to this new checksum.
                                // This must be done to avoid a looping situation where instances keep performing merges
                                // in lock step with each other due to lastrestoredchecksum file containing an older checksum, when 
                                // there are changes that are repeated (e.g. logging toggled on/off).
                                Utilities.SetLastRestoredChecksum(currentChecksum);
                                ConfigStorageServiceHttpClient.CurrentConfigChecksum = currentChecksum;

                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, "Post-merge backup to config storage service failed.");
                            }
                        }
                        else
                        {
                            // This is new backup data. Verify its version is valid (same or lower than this instance's) and restore it.
                            // Note: the monitoring job also does the version check and will only issue an app shutdown if latest config version is valid.
                            // This version check logic below is necessary for the startup scenario.
                            if (Utilities.VerifyNewConfigVersionOK(configObject.ConfigVersion, latestConfig.ConfigVersion))
                            {
                                logger.LogInformation("Current config data:\n " + configObject.ConfigData);
                                logger.LogInformation("\nNew config data:\n " + latestConfig.ConfigData);

                                atStep = 3;
                                logger.LogInformation("New config data found, apply new data and shutting down...");
                                // We have new config to apply.
                                // Apply new config and shutdown app domain
                                RestoreFromConfiguration(latestConfig);

                                // Once restore/merge is done, process staging config file if it hasn't been done before...
                                // Apply the staging config file if it is present. The changes from this config file will be saved
                                // as part of the "merge". On next start up, the config snapshot that includes both the merge and the changes
                                // from the staging config file will be sent to EACSS as the latest snapshot.
                                //
                                // NOTE: this step MUST be done after the restore/merge, or the value from the staging confile file would get
                                // overwritten by the restore/merge operation.   
                                logger.LogInformation("Config restore completed. Processing staging config file...");
                                bool changesApplied = appConfigUtility.ApplyStagingConfigFile();

                                atStep = 4;
                                if (changesApplied)
                                {
                                    startMonitorThread = false;
                                    // restart the app to ensure all changes take effect. This should only occur once... when the staging config
                                    // file is present
                                    _lifetime.ApplicationStarted.Register(o => _lifetime.StopApplication(), null);
                                }
                            }
                            else
                            {
                                logger.LogInformation(string.Format(
                                    "New config's version '{0}' is higher than this instance's config version '{1}'. It will not be restored.",
                                    latestConfig.ConfigVersion, configObject.ConfigVersion));
                            }
                        }
                    }
                    else
                    {
                        // no new config returned from EACSS. 
                        // Process staging config file next.
                        atStep = 3;
                        logger.LogInformation("Processing staging config file...");
                        var changesOccurred = appConfigUtility.ApplyStagingConfigFile();
                        if (changesOccurred)
                        {
                            // If configs were updated as a result, send a new backup snapshot to EACSS.
                            // But first, rebuild the config object so that it contains the changes.
                            logger.LogInformation("Post-staging config object being rebuilt...");
                            configObject = appConfigUtility.GetApiConfigurationObject();

                            string username = "Application_Start";
                            try
                            {
                                logger.LogInformation("Post-staging backup being sent to config storage...");
                                var result = appConfigUtility.StorageServiceClient.PostConfigurationAsync(
                                    configObject.Namespace, configObject.ConfigData, username,
                                    configObject.ConfigVersion, configObject.ProductId, configObject.ProductVersion).GetAwaiter().GetResult();
                                logger.LogInformation("Post-staging backup sent to config storage. Bouncing app pool...");
                                
                                // In the case where this snapshot we just submitted is slightly different than
                                // the actual config that forms when this app restarts, due to certain web.config settings
                                // which don't reflect until a restart:
                                // Set the lastrestoredchecksum file to the checksum of the snapshot we just sent,
                                // to force a merge on restart, and ensure those new web.config settings are preserved.
                                // Otherwise, the snapshot we just sent will get restored and wipe out those updated web.config settings.
                                var updatedChecksum = Utilities.GetMd5ChecksumString(configObject.ConfigData);
                                Utilities.SetLastRestoredChecksum(updatedChecksum);

                                atStep = 4;
                                startMonitorThread = false;
                                // restart the app to ensure all changes take effect. This should only occur once... when the staging config
                                // file is present
                                _lifetime.ApplicationStarted.Register(o => _lifetime.StopApplication(), null);

                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, "Post staging backup to config storage service failed.");
                            }
                        }
                        else
                        {
                            logger.LogInformation("No staging config changes were made.");
                        }
                    }

                    if (startMonitorThread)
                    {
                        atStep = 5;
                        // No new update found at this time.
                        // Kick off monitor thread and pass it a callback delegate that will shutdown app domain to restart itself.
                        // If this thread gets killed mid operation, it actually doesn't matter.
                        logger.LogInformation("No new config data to apply. Kicking off monitor job...");

                        // see if we are using pubsub
                        // set things up for listening
                        if (pubsubOptions.ConfigManagementEnabled)
                        {
                            var pubsubSubscriber = _app.ApplicationServices.GetService<ISubscriber>();
                            var eacssChannel = new RedisChannel((pubsubOptions.Namespace ?? ColleaguePubSubOptions.DEFAULT_NAMESPACE) + "/" + (pubsubOptions.ConfigChannel ?? ColleaguePubSubOptions.DEFAULT_CONFIG_CHANNEL), RedisChannel.PatternMode.Literal);
                            var queue = pubsubSubscriber.Subscribe(eacssChannel);
                            queue.OnMessage(async message =>
                            {
                                if (message.Message.HasValue)
                                {
                                    var notification = System.Text.Json.JsonSerializer.Deserialize<PubSubConfigNotification>((string)message.Message);
                                    //logger.LogDebug("PubSub subsriber received message: " + checksumFromMessage);
                                    if (notification.HostName != Environment.MachineName && ConfigStorageServiceHttpClient.CurrentConfigChecksum != notification.Checksum)
                                    {
                                        var latestConfig = await appConfigUtility.StorageServiceClient.GetLatestAsync();
                                        if (latestConfig != null && latestConfig.Checksum != notification.Checksum)
                                        {
                                            // Check whether this latest config has actually been restored before.
                                            var lastRestoredChecksum = Utilities.GetLastRestoredChecksum();
                                            if (!string.IsNullOrWhiteSpace(lastRestoredChecksum) && lastRestoredChecksum == latestConfig.Checksum)
                                            {
                                                RestoreFromConfiguration(latestConfig);
                                            }
                                        }
                                    }

                                }
                            });

                        }
                        else
                        {
                            // use the pre-existing background monitor
                            var monitorJob = new BackgroundMonitorJob(appConfigUtility.StorageServiceClient, _lifetime, RestoreFromConfiguration);
                            ThreadStart threadDelegate = new ThreadStart(monitorJob.Start);
                            Thread newThread = new Thread(threadDelegate);
                            newThread.Start();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception ocurred updating/monitoring config data at step " + atStep);
            }
        }

        private void RestoreFromConfiguration(App.Config.Storage.Service.Client.Models.Configuration latestConfig)
        {
            var logger = _app.ApplicationServices.GetService<Microsoft.Extensions.Logging.ILogger>();
            var appConfigUtility = _app.ApplicationServices.GetRequiredService<AppConfigUtility>();
            var workerProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
            logger.LogInformation(DateTime.Now.ToString("hh:mm:ss.fff tt") + " (WPID " + workerProcessId + "): New config data found, apply new data without shutting down...");

            // We have new config to apply.
            // Apply new config 
            var restoredData = appConfigUtility.RestoreApiBackupConfiguration(latestConfig.ConfigData, versionChanged: AppConfigUtility.ApiConfigVersion != latestConfig.ConfigVersion, newVersion: AppConfigUtility.ApiConfigVersion);
            // Set the "last restored checksum" so on respin after a config merge 
            // (the merge is due to current config version being higher/different than the backup config version, which results in a new checksum)
            // we know not to restore the same backup config again and instead perform a backup.
            Utilities.SetLastRestoredChecksum(latestConfig.Checksum);
            ConfigStorageServiceHttpClient.CurrentConfigChecksum = latestConfig.Checksum;

            RefreshServiceInstances(restoredData, logger);
        }

        private async void RefreshServiceInstances(ApiBackupConfigData backupData, Microsoft.Extensions.Logging.ILogger logger)
        {
            try
            {
                logger.LogDebug("Updating service instances");
                // update the static properties
                var supportedUiCultures = new List<CultureInfo>();
                foreach (var supportedCulture in Bootstrapper.SupportedUICultures)
                {
                    supportedUiCultures.Add(new CultureInfo(supportedCulture));
                };
                Bootstrapper.SetDefaultUiCulture(backupData.Settings.DefaultUiCulture, supportedUiCultures);
                var supportedCultures = new List<CultureInfo>();
                foreach (var supportedCulture in Bootstrapper.SupportedCultures)
                {
                    supportedCultures.Add(new CultureInfo(supportedCulture));
                };
                Bootstrapper.SetDefaultCulture(backupData.Settings.DefaultCulture, supportedCultures);
                Bootstrapper.LoggingLevelSwitch.MinimumLevel = backupData.Settings.LogLevel;

                var injectedSettingsAsOptions = _app.ApplicationServices.GetService<IOptions<ApiSettingRepositorySettings>>();
                injectedSettingsAsOptions.Value.AttachRequestMaxSize = backupData.ApiSettings.AttachRequestMaxSize;
                injectedSettingsAsOptions.Value.BulkReadSize = backupData.ApiSettings.BulkReadSize;
                injectedSettingsAsOptions.Value.DetailedHealthCheckApiEnabled = backupData.ApiSettings.DetailedHealthCheckApiEnabled;
                injectedSettingsAsOptions.Value.EnableConfigBackup = backupData.ApiSettings.EnableConfigBackup;
                injectedSettingsAsOptions.Value.IncludeLinkSelfHeaders = backupData.ApiSettings.IncludeLinkSelfHeaders;

                var injectedColleagueSettings = _app.ApplicationServices.GetService<ColleagueSettings>();
                CopyPublicProperties(backupData.Settings.ColleagueSettings.DmiSettings, injectedColleagueSettings.DmiSettings);
                CopyPublicProperties(backupData.Settings.ColleagueSettings.DasSettings, injectedColleagueSettings.DasSettings);
                CopyPublicProperties(backupData.Settings.ColleagueSettings.GeneralSettings, injectedColleagueSettings.GeneralSettings);

                var injectedDmiSettings = _app.ApplicationServices.GetService<DmiSettings>();
                CopyPublicProperties(backupData.Settings.ColleagueSettings.DmiSettings, injectedDmiSettings);

                await DmiConnectionPool.SetSizeAsync(DmiConnectionPool.ConnectionPoolName(injectedColleagueSettings.DmiSettings.IpAddress, injectedColleagueSettings.DmiSettings.Port, injectedColleagueSettings.DmiSettings.Secure),
                        injectedColleagueSettings.DmiSettings.ConnectionPoolSize);
                await DasSessionPool.SetSizeAsync(injectedColleagueSettings.DasSettings.ConnectionPoolSize);

                var apiSettingsRepo = _app.ApplicationServices.GetService<IApiSettingsRepository>();

                var apiSettings = new ApiSettings("null");
                try
                {
                    apiSettings = apiSettingsRepo.Get(backupData.Settings.ProfileName);
                }
                catch (Exception e)
                {
                    if (!string.IsNullOrEmpty(e.Message))
                    {
                        string m = e.Message.ToLower();
                        if (m.Contains("cannot access file") && m.Contains("web.api.config"))
                        {
                            logger.LogError("WEB.API.CONFIG has not been configured for anonymous access on WSPD! Anything using the API settings may fail.");
                        }
                    }
                    logger.LogError(e, "Unable to read API Settings from colleague. Profile Name: {0}", backupData.Settings.ProfileName);
                }
                var injectedApiSettings = _app.ApplicationServices.GetService<ApiSettings>();
                CopyPublicProperties(apiSettings, injectedApiSettings);

                Bootstrapper.colleagueTimeZone = apiSettings.ColleagueTimeZone;

            }
            catch (Exception ex)
            {
                logger.LogError("Error when updating service instances: " + ex.ToString());
            }
        }

        private void CopyPublicProperties<T>(T source, T target)
        {
            var properties = typeof(T)
                                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(p => p.CanRead && p.CanWrite);

            foreach (var property in properties)
            {
                property.SetValue(target, property.GetValue(source));
            }
        }

        #endregion
    }
}

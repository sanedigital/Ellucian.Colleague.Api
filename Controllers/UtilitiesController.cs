// Copyright 2013-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Client;
using Ellucian.Colleague.Api.Models;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Mvc.Filter;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Services;
using StackExchange.Redis;
using Microsoft.Extensions.Options;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides a top-level controller for API utilities.
    /// </summary>
    [LocalRequest]
    public class UtilitiesController : Controller
    {
        private ISettingsRepository settingsRepository;
        private ILogger logger;
        private ICacheProvider cacheProvider;
        private ApiSettings apiSettings;
        private IWebHostEnvironment environment;
        private readonly ICacheManagementService cacheManagementService;
        private readonly ColleaguePubSubOptions _configManagementPubSubOptions;
        private readonly ISubscriber _pubSubSubscriber;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="settingsRepository">ISettingsRepository instance</param>
        /// <param name="logger">ILogger instance</param>
        /// <param name="cacheProvider">Cache provider instance</param>
        /// <param name="apiSettings">IConfigurationService instance</param>
        /// <param name="environment">IWebHostEnvironment instance</param>
        /// <param name="cacheManagementService"></param>
        /// <param name="configManagementPubSubOptions"></param>
        /// <param name="pubSubSubscriber"></param>
        public UtilitiesController(ISettingsRepository settingsRepository, ILogger logger, ICacheProvider cacheProvider, ApiSettings apiSettings, IWebHostEnvironment environment,
            ICacheManagementService cacheManagementService,
            IOptions<ColleaguePubSubOptions> configManagementPubSubOptions, ISubscriber pubSubSubscriber)
        {
            if (settingsRepository == null)
            {
                throw new ArgumentNullException("settingsRepository");
            }
            this.settingsRepository = settingsRepository;
            this.logger = logger;
            this.cacheProvider = cacheProvider;
            this.apiSettings = apiSettings;
            this.environment = environment;
            this.cacheManagementService = cacheManagementService;
            _configManagementPubSubOptions = configManagementPubSubOptions.Value;
            _pubSubSubscriber = pubSubSubscriber;
        }

        /// <summary>
        /// Gets the index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.EnableStudentTests = System.IO.File.Exists(System.IO.Path.Combine(environment.ContentRootPath, "Areas", "Student", "Views", "Test", "Index.cshtml"));
            ViewBag.EnablePlanningTests = System.IO.File.Exists(System.IO.Path.Combine(environment.ContentRootPath, "Areas", "Student", "Views", "Test", "Index.cshtml"));
            ViewBag.EnableConfigBackup = apiSettings.EnableConfigBackup;
            return View();
        }

        /// <summary>
        /// Allows for viewing of cache keys
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> CacheManagement()
        {
            var result = new CacheManagementViewModel()
            {
                CacheKeys = await cacheManagementService.GetKeys(),
                Host = Environment.MachineName,
                Application = Assembly.GetEntryAssembly().ToString()
            };

            return View(result);
        }

        /// <summary>
        /// Returns a YAML-like result with string values changed to "==NOTNULL==" or "null" to indicate properties that have values and those that don't.
        /// Must be a localhost request.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetSanitizedCacheValue([FromQuery] string key)
        {
            var sanitizedResult = string.Empty;
            try
            {
                sanitizedResult = await cacheManagementService.GetSanitizedCacheValue(key);
            }
            catch (KeyNotFoundException ex)
            {
                sanitizedResult = ex.Message;
            }

            return Json(new
            {
                Result = sanitizedResult
            });
        }

        /// <summary>
        /// Allows removal of cache elements based on the provided keys for localhost requests only.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> RemoveCacheValue([FromBody] IEnumerable<string> keys)
        {
            if (keys == null)
            {
                return Json(new
                {
                    Result = $"No keys provided.",
                    RemovedKeys = new List<string>()
                });
            }
            try
            {
                var itemsRemoved = await cacheManagementService.RemoveCacheValue(keys);

                if (itemsRemoved != null && itemsRemoved.Any())
                {
                    foreach ( var key in itemsRemoved)
                    {
                        if (_configManagementPubSubOptions.CacheManagementEnabled)
                        {
                            var cacheChannel = new RedisChannel((_configManagementPubSubOptions.Namespace ?? ColleaguePubSubOptions.DEFAULT_NAMESPACE) + "/" + (_configManagementPubSubOptions.CacheChannel ?? ColleaguePubSubOptions.DEFAULT_CACHE_CHANNEL), RedisChannel.PatternMode.Literal);

                            var notification = new PubSubCacheNotification() { HostName = Environment.MachineName, CacheKeys = keys.ToArray() };
                            var json = System.Text.Json.JsonSerializer.Serialize(notification);
                            _pubSubSubscriber.Publish(cacheChannel, new RedisValue(json));
                        }
                    }
                }

                return Json(new
                {
                    Result = $"Completed removing {itemsRemoved.Count()} items from cache.",
                    RemovedKeys = itemsRemoved
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing items from cache.");
                return Json(new
                {
                    Result = $"Error removing items from cache.",
                    RemovedKeys = new List<string>()
                });
            }
        }

        /// <summary>
        /// Gets the clear cache page. This action currently clears the cache.
        /// </summary>
        /// <returns></returns>
        public ActionResult ClearCache()
        {
            List<string> filter = new List<string>();

            UtilityCacheRepository cacheRepo = new UtilityCacheRepository(cacheProvider);
            cacheRepo.ClearCache(filter);

            return View();
        }

        /// <summary>
        /// Private repository class which extends the base caching repository; a bit of a hack so this controller
        /// can access methods in the abstract base caching repository
        /// </summary>
        private class UtilityCacheRepository : BaseCachingRepository
        {
            protected internal UtilityCacheRepository(ICacheProvider cacheProvider) : base(cacheProvider) { }
        }

        /// <summary>
        /// Backs up API config
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> BackupConfig()
        {
            if (LocalUserUtilities.GetCurrentUser(Request) == null)
            {
                var error = "You must login before backing up the API configuration.";
                string returnUrl = Url.Action("Index", "Utilities");
                return RedirectToAction("Login", "Admin", new { returnUrl = returnUrl, error = error });
            }
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
            return View();
        }

        /// <summary>
        /// Backs up API config
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> RestoreConfig()
        {
            if (LocalUserUtilities.GetCurrentUser(Request) == null)
            {
                var error = "You must login before restoring the API configuration.";
                string returnUrl = Url.Action("Index", "Utilities");
                return RedirectToAction("Login", "Admin", new { returnUrl = returnUrl, error = error });
            }
            var cookieValue = LocalUserUtilities.GetCookie(Request);
            if (string.IsNullOrEmpty(cookieValue))
            {
                throw new ColleagueWebApiException("Log in first");
            }
            var baseUrl = cookieValue.Split('*')[0];
            var token = cookieValue.Split('*')[1];
            var client = new ColleagueApiClient(baseUrl, logger);
            client.Credentials = token;
            await client.PostRestoreApiConfigDataAsync();
            return View();
        }
    }
}

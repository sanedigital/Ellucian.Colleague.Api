using Ellucian.Colleague.Api.Helpers;
using Ellucian.Colleague.Api.Models;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos;
using Ellucian.Logging;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Mvc.Filter;
using Ellucian.Web.Resource;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Allows for fine-tuned cache management (i.e. expiration of keys)
    /// </summary>
    public class CacheManagementController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly ICacheProvider _cacheProvider;
        private readonly ICacheManagementService _cacheManagementService;
        private readonly ColleaguePubSubOptions _configManagementPubSubOptions;
        private readonly ISubscriber _pubSubSubscriber;
        private readonly AuditLoggingAdapter _auditLoggingAdapter;

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cacheProvider"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        /// <param name="cacheManagementService"></param>
        /// <param name="configManagementPubSubOptions"></param>
        /// <param name="pubSubSubscriber"></param>
        /// <param name="auditLoggingAdapter"></param>
        public CacheManagementController(ILogger logger, ICacheProvider cacheProvider, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings, ICacheManagementService cacheManagementService,
            IOptions<ColleaguePubSubOptions> configManagementPubSubOptions, ISubscriber pubSubSubscriber, AuditLoggingAdapter auditLoggingAdapter) : base(actionContextAccessor, apiSettings)
        {
            _logger = logger;
            _cacheProvider = cacheProvider;
            _cacheManagementService = cacheManagementService;
            _configManagementPubSubOptions = configManagementPubSubOptions.Value;
            _pubSubSubscriber = pubSubSubscriber;
            _auditLoggingAdapter = auditLoggingAdapter;
        }

        /// <summary>
        /// Allows for returning all cache provider keys
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [PermissionsFilter(BasePermissionCodes.ViewCacheKeys)]
        [HeaderVersionRoute("/cache-management-list-keys", 1, true, Name = "ListKeys")]
        public async Task<ActionResult<IEnumerable<string>>> ListKeys()
        {
            try
            {
                _cacheManagementService.ValidatePermissions(GetPermissionsMetaData());
                return Ok(await _cacheManagementService.GetKeys());
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError("Can't list cache provider keys: " + ex.GetBaseException().Message);
                return BadRequest("Can't list cache provider keys, see error log for more details.");
            }
        }

        /// <summary>
        /// Returns a YAML-like result with string values changed to "==NOTNULL==" or "null" to indicate properties that have values and those that don't
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [PermissionsFilter(BasePermissionCodes.ViewCacheKeys)]
        [HeaderVersionRoute("/cache-management-safe-key-value", 1, true, Name = "ViewValue")]
        public async Task<ActionResult<CacheManagementResponse>> GetSanitizedCacheValue([FromQuery] string key)
        {
            try
            {
                _cacheManagementService.ValidatePermissions(GetPermissionsMetaData());

                var sanitizedResult = await _cacheManagementService.GetSanitizedCacheValue(key);

                return new CacheManagementResponse()
                {
                    Result = sanitizedResult
                };
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError("Can't view cache provider key details: " + ex.GetBaseException().Message);
                return BadRequest("Can't view cache provider key details, see error log for more details.");
            }
        }


        /// <summary>
        /// Allows removal of cache elements based on the provided keys.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionsFilter(BasePermissionCodes.DeleteCacheKeys)]
        [HeaderVersionRoute("/cache-management-remove-keys", 1, true, Name = "RemoveCacheValue")]
        public async Task<ActionResult<CacheManagementResponse>> RemoveCacheValue([FromBody] IEnumerable<string> keys)
        {
            try
            {
                _cacheManagementService.ValidatePermissions(GetPermissionsMetaData());

                var removedKeys = await _cacheManagementService.RemoveCacheValue(keys);

                if (removedKeys != null && removedKeys.Any())
                {
                    foreach (var key in removedKeys)
                    {
                        if (_configManagementPubSubOptions.CacheManagementEnabled)
                        {
                            var cacheChannel = new RedisChannel((_configManagementPubSubOptions.Namespace ?? ColleaguePubSubOptions.DEFAULT_NAMESPACE) + "/" + (_configManagementPubSubOptions.CacheChannel ?? ColleaguePubSubOptions.DEFAULT_CACHE_CHANNEL), RedisChannel.PatternMode.Literal);

                            var notification = new PubSubCacheNotification() { HostName = Environment.MachineName, CacheKeys = keys.ToArray() };
                            var json = System.Text.Json.JsonSerializer.Serialize(notification);
                            _pubSubSubscriber.Publish(cacheChannel, new RedisValue(json));
                        }
                    }
                    try
                    {
                        var userPrincipal = LocalUserUtilities.GetCurrentUser(Request) as ClaimsPrincipal;
                        var userId = userPrincipal?.FindFirstValue("pid") ?? "LocalAdmin";
                        var auditLogProps = new AuditLogProperties(userId);

                        using (Serilog.Context.LogContext.Push(auditLogProps.GetEnricherList().ToArray()))
                        {
                            _auditLoggingAdapter.Info($"Web API Admin Cache Management, Keys removed: {String.Join(',', removedKeys)}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to audit log changes for web api resource file editor.");
                    }
                }

                return new CacheManagementResponse()
                {
                    Result = $"Completed removing {removedKeys.Count()} items from cache.",
                    RemovedKeys = removedKeys.ToArray()
                };
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing items from cache.");
                return new JsonResult(new
                {
                    Result = $"Error removing items from cache.",
                    RemovedKeys = new List<string>()
                });
            }
        }



        


    }
}

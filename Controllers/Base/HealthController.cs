// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides specific status information
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class HealthController : BaseCompressedApiController
    {
        private readonly IHealthCheckService _healthCheckService;
        private readonly ILogger _logger;
        private readonly AppConfigUtility _appConfigUtility;

        /// <summary>
        /// Initializes a new instance of the HealthCheckController class.
        /// </summary>
        /// <param name="healthCheckService">Service of type <see cref="IHealthCheckService">IHealthCheckService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="apiSettings">API settings</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="appConfigUtility"></param>
        public HealthController(IHealthCheckService healthCheckService, ILogger logger, ApiSettings apiSettings,
            IActionContextAccessor actionContextAccessor, AppConfigUtility appConfigUtility) : base(actionContextAccessor, apiSettings)
        {
            _healthCheckService = healthCheckService;
            this._logger = logger;
            _appConfigUtility = appConfigUtility;
        }

        /// <summary>
        /// Retrieves status information for the Colleague Web API.
        /// </summary>
        /// <returns>Status information.</returns>
        [HttpGet]
        [HeaderVersionRoute("/health", 1, true, Name = "GetApplicationHealth", Order = -10)]
        public async Task<IActionResult> GetApplicationHealthCheckAsync([FromQuery(Name = "level")] string level = null)
        {
            if (level == "detailed")
            {
                if (_apiSettings != null && _apiSettings.DetailedHealthCheckApiEnabled)
                {
                    // detailed health check meant for reporting/monitoring without automated actions.  Check the
                    // application's connections to its dependencies
                    try
                    {
                        var healthCheckResult = await _healthCheckService.PerformDetailedHealthCheckAsync();
                        if (healthCheckResult.Status == Dtos.Base.HealthCheckStatusType.Unavailable)
                        {
                            // an error occurred during the health check, change the status code but return the JSON still
                            return StatusCode(StatusCodes.Status503ServiceUnavailable);
                        }
                        else
                        {
                            if (_appConfigUtility.ConfigServiceClientSettings != null && _appConfigUtility.ConfigServiceClientSettings.IsSaaSEnvironment)
                            {
                                // if we are in SaaS and the EACSS health check isn't clean, then this site will have issues
                                var eacssHealthCheckResponse = await _appConfigUtility.CheckStorageServiceHealth();
                                healthCheckResult.EACSSResult = eacssHealthCheckResponse.ToString();
                                if (!(eacssHealthCheckResponse == System.Net.HttpStatusCode.NoContent || eacssHealthCheckResponse == System.Net.HttpStatusCode.OK))
                                {
                                    healthCheckResult.Status = Dtos.Base.HealthCheckStatusType.Unavailable;
                                }
                            }

                            switch (healthCheckResult.Status)
                            {
                                case Dtos.Base.HealthCheckStatusType.Available:
                                    return NoContent();
                                case Dtos.Base.HealthCheckStatusType.Unavailable:
                                    return StatusCode(StatusCodes.Status503ServiceUnavailable);
                                default:
                                    return StatusCode(500);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        string error = "Exception occurred during detailed health check";
                        _logger.LogError(e, error);
                        return CreateHttpResponseException(error, HttpStatusCode.ServiceUnavailable);
                    }
                }
                else
                {
                    // detailed health check not enabled
                    return Forbid();
                }
            }
            else if (level == null)
            {
                if (_appConfigUtility.ConfigServiceClientSettings != null && _appConfigUtility.ConfigServiceClientSettings.IsSaaSEnvironment)
                {
                    // if we are in SaaS and the EACSS health check isn't clean, then this site will have issues
                    var eacssHealthCheckResponse = await _appConfigUtility.CheckStorageServiceHealth();
                    if (!(eacssHealthCheckResponse == HttpStatusCode.NoContent || eacssHealthCheckResponse == System.Net.HttpStatusCode.OK))
                    {
                        return StatusCode(StatusCodes.Status503ServiceUnavailable);
                    }
                }

                // basic health check meant for load balancers.  Return available to confirm the application
                // is running and accessible
                return NoContent();
            }
            else
            {
                return CreateHttpResponseException("Invalid health check level");
            }
        }
    }
}

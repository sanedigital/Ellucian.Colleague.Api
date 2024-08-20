// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.Reflection;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific status information
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class HealthCheckController : BaseCompressedApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public HealthCheckController(IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
        }


        /// <summary>
        /// Retrieves status information for the Colleague Web API.
        /// </summary>
        /// <returns>Status information.</returns>
        [HttpGet]
        [Route("/healthcheck", Name = "GetHealthCheck", Order = -10)]
        public async Task<ActionResult<IEnumerable<HealthCheck>>> GetHealthCheckAsync()
        {
            HealthCheck statusInfo = new HealthCheck(HealthCheckType.Available);

            switch (statusInfo.Status)
            {
                case HealthCheckType.Available:
                    return NoContent();
                case HealthCheckType.Unavailable:
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);
                default:
                    return StatusCode(500);
            }

        }
    }
}

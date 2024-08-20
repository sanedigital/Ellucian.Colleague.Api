// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.TimeManagement.Services;
using Ellucian.Colleague.Domain.TimeManagement.Entities;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;


namespace Ellucian.Colleague.Api.Controllers.TimeManagement
{
    /// <summary>
    /// Exposes access to employee time cards for time entry
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.TimeManagement)]
    public class OvertimeCalculationDefinitionsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IOvertimeCalculationDefinitionsService overtimeCalculationDefinitionsService;

        /// <summary>
        /// OvertimeCalculationDefinitionsController constructor
        /// </summary>
        /// 
        /// <param name="logger"></param>
        /// <param name="overtimeCalculationDefinitionsService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public OvertimeCalculationDefinitionsController(ILogger logger, IOvertimeCalculationDefinitionsService overtimeCalculationDefinitionsService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
                this.logger = logger;
                this.overtimeCalculationDefinitionsService = overtimeCalculationDefinitionsService;
        }

         /// <summary>
         /// Gets all overtime calculation definitions.
         /// See the OvertimeCalculationDefinition object description for information on its properties.
         /// This endpoint will return an error if:
         ///  1. 400 - An unhandled error occurs while getting overtime calculation definitions 
         /// </summary>
         /// <returns>A list of Overtime Calculation Definitions</returns>
        [HttpGet]
        [HeaderVersionRoute("/overtime-calculation-definitions", 1, true, Name = "GetOvertimeCalculationDefinitionsAsync")]
        public async Task<ActionResult<IEnumerable<Dtos.TimeManagement.OvertimeCalculationDefinition>>> GetOvertimeCalculationDefinitionsAsync()
        {
             try
             {
                  return Ok(await overtimeCalculationDefinitionsService.GetOvertimeCalculationDefinitionsAsync());
             }
             catch (PermissionsException pe)
             {
                  var message = "You do not have permission to GetOvertimeCalculationDefinitionsAsync";
                  logger.LogError(pe, message);
                  return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
             }
             catch (Exception e)
             {
                  logger.LogError(e, e.Message);
                  return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
             }

        }
    }
}

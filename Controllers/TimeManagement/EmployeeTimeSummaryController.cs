// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.TimeManagement.Services;
using Ellucian.Colleague.Dtos.TimeManagement;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.TimeManagement
{
    /// <summary>
    /// Exposes access to employee time summary information 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.TimeManagement)]
    public class EmployeeTimeSummaryController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IEmployeeTimeSummaryService employeeTimeSummaryService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";
        private const string invalidPermissionsErrorMessage = "The current user does not have the permissions to perform the requested operation.";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="employeeTimeSummaryService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmployeeTimeSummaryController(ILogger logger, IEmployeeTimeSummaryService employeeTimeSummaryService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.employeeTimeSummaryService = employeeTimeSummaryService;
        }

        /// <summary>
        /// Gets a summary of employee time information based on criteria provided.
        /// 
        /// The endpoint will not return the requested EmployeeTimeSummary if:
        ///     1.  400 - criteria was not provided
        ///     2.  403 - criteria contains Ids that do not have permission to get requested EmployeeTimeSummary
        ///     3.  404 - EmployeeTimeSummary resources requested do not exist
        /// </summary>
        /// <param name="criteria">Criteria used to select EmployeeTimeSummary objects <see cref="EmployeeTimeSummaryQueryCriteria">.</see></param>
        /// <returns>A list of <see cref="EmployeeTimeSummary"> objects.</see></returns>
        /// <accessComments>
        /// When a supervisor Id is provided as part of the criteria, the authenticated user must have supervisory permissions
        /// or be a proxy for supervisor. If no supervisor Id is provided, only EmployeeTimeSummary objects for the authenticated user
        /// may be requested
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/employee-time-summary", 1, true, Name = "QueryEmployeeTimeSummaryAsync")]
        public async Task<ActionResult<IEnumerable<EmployeeTimeSummary>>> QueryEmployeeTimeSummaryAsync([FromBody]EmployeeTimeSummaryQueryCriteria criteria)
        {
            if (criteria == null)
            {
                var message = string.Format("criteria is required for QuerySupervisorSummaryAsync.");
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(criteria.SupervisorId) && criteria.SuperviseeIds == null)
            {
                var message = string.Format("Criteria must include a supervisor Id or supervisee Id(s)");
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                logger.LogDebug("************Start - Process to get a summary of employee time information based on criteria provided - Start************");
                var employeeTimeSummary = await employeeTimeSummaryService.GetEmployeeTimeSummaryAsync(criteria);
                logger.LogDebug("************End - Process to get a summary of employee time information based on criteria provided is successful - End************");
                return Ok(employeeTimeSummary);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to QueryEmployeeTimeSummaryAsync - {0}", pe.Message);
                logger.LogError(pe, message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }
    }
}

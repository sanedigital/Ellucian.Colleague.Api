// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
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


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes access to employee summary information 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmployeeSummaryController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IEmployeeSummaryService employeeSummaryService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="employeeSummaryService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmployeeSummaryController(ILogger logger, IEmployeeSummaryService employeeSummaryService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.employeeSummaryService = employeeSummaryService;
        }

        /// <summary>
        /// Gets a summary of employee information based on criteria provided.
        /// 
        /// The endpoint will not return the requested EmployeeSummary if:
        ///     1.  400 - criteria was not provided
        ///     2.  403 - criteria contains Ids that do not have permission to get requested EmployeeSummary
        ///     3.  404 - EmployeeSummary resources requested do not exist
        /// </summary>
        /// <param name="criteria">Criteria used to select EmployeeSummary objects <see cref="EmployeeSummaryQueryCriteria">.</see></param>
        /// <returns>A list of <see cref="EmployeeSummary"> objects.</see></returns>
        /// <accessComments>
        /// When a supervisor Id is provided as part of the criteria, the authenticated user must have supervisory permissions
        /// or be a proxy for supervisor. If no supervisor Id is provided, only EmployeeSummary objects for the authenticated user
        /// may be requested
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/employee-summary", 1, true, Name = "QueryEmployeeSummaryAsync")]
        public async Task<ActionResult<IEnumerable<EmployeeSummary>>> QueryEmployeeSummaryAsync([FromBody]EmployeeSummaryQueryCriteria criteria)
        {
            if (criteria == null)
            {
                var message = string.Format("criteria is required for QueryEmployeeSummaryAsync");
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(criteria.EmployeeSupervisorId) && (criteria.EmployeeIds == null || criteria.EmployeeIds.Any()))
            {
                var message = string.Format("Criteria must include a supervisor Id or employee Id(s)");
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await employeeSummaryService.QueryEmployeeSummaryAsync(criteria));
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to QueryEmployeeSummaryAsync - {0}", pe.Message);
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

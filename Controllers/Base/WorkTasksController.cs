// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Work Task data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class WorkTasksController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IWorkTaskService workTaskService;

        /// <summary>
        /// Creates a work task controller object.
        /// </summary>
        /// <param name="workTaskService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public WorkTasksController(IWorkTaskService workTaskService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.workTaskService = workTaskService;
            this.logger = logger;
        }

        /// <summary>
        /// Get the list of open tasks for the indicated person
        /// </summary>
        /// <param name="personId">Required. Id of person to whom tasks are assigned. Retrieves tasks that are assigned to the person's Id or to any of the person's roles.</param>
        /// <returns></returns>
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "personId")]
        [HeaderVersionRoute("/work-tasks", 1, true, Name = "GetWorkTasks")]
        public async Task<ActionResult<IEnumerable<Dtos.Base.WorkTask>>> GetAsync([FromQuery] string personId)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    logger.LogError("A query parameter is required to retrieve work tasks");
                    throw new ArgumentNullException(personId);
                }
                return await workTaskService.GetAsync(personId);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(ex.ToString(), HttpStatusCode.BadRequest);
            }
        }
    }
}

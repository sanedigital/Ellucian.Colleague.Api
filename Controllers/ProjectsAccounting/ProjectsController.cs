// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ProjectsAccounting.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers.ProjectsAccounting
{
    /// <summary>
    /// This is the controller for projects accounting.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ProjectsAccounting)]
    [Authorize]
    public class ProjectsController : BaseCompressedApiController
    {
        private readonly IProjectService projectService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the ProjectsController object.
        /// </summary>
        /// <param name="projectService">Project service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor">injected</param>
        /// <param name="apiSettings"></param>
        public ProjectsController(IProjectService projectService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.projectService = projectService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all projects assigned to the user.
        /// </summary>
        /// <param name="filter">Filter criteria used to reduce the number of projects returned.</param>
        /// <param name="summaryOnly">Specify whether or not detailed information will be returned with the project.</param>
        /// <returns>List of project DTOs.</returns>
        [HttpGet]
        [HeaderVersionRoute("/projects", 1, true, Name = "GetProjects")]
        public async Task<ActionResult<IEnumerable<Project>>> GetAsync([FromQuery] IEnumerable<UrlFilter> filter, bool summaryOnly = true)
        {
            try
            {
                var projects = await projectService.GetProjectsAsync(summaryOnly, filter);
                return Ok(projects);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, "Invalid argument.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ConfigurationException ce)
            {
                logger.LogError(ce, "Invalid configuration.");
                return CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired. Could not get projects.");
                return CreateHttpResponseException("Session expired. Could not get projects.", HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not get projects.");
                return CreateHttpResponseException();
            }
        }


        /// <summary>
        /// Retrieves all projects for given criteria
        /// </summary>
        /// <param name="criteria"> Criteria object contains parameters for which projects are queried.</param>
        /// <accessComments>
        /// User requires access for atleast one of the general ledger numbers on the criteria.
        /// </accessComments>
        /// <returns>List of project DTOs.</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/projects", 1, true, Name = "QueryProjects")]
        public async Task<ActionResult<IEnumerable<Project>>> QueryProjectsAsync([FromBody] Dtos.ColleagueFinance.ProjectQueryCriteria criteria)
        {
            try
            {
                var projects = await projectService.QueryProjectsAsync(criteria);
                return Ok(projects);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument to query the projects.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Retrieves the project the user selected
        /// </summary>
        /// <param name="projectId">ID of the project requested.</param>
        /// <param name="summaryOnly">Specify whether or not detailed information will be returned with the project.</param>
        /// <param name="sequenceNumber">Specify a sequence number to return project totals for a single budget period.</param>
        /// <returns>Project DTO</returns>
        [HttpGet]
        [HeaderVersionRoute("/projects/{projectId}", 1, true, Name = "GetProject")]
        public async Task<ActionResult<Project>> GetProjectAsync(string projectId, bool summaryOnly = false, string sequenceNumber = null)
        {
            try
            {
                var project = await projectService.GetProjectAsync(projectId, summaryOnly, sequenceNumber);
                return project;
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, "Access denied.");
                return CreateHttpResponseException("Access denied.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, "Invalid argument.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ConfigurationException ce)
            {
                logger.LogError(ce, "Invalid configuration.");
                return CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "Session expired. Could not get project.");
                return CreateHttpResponseException("Session expired. Could not get project.", HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not get project.");
                return CreateHttpResponseException();
            }
        }
    }
}

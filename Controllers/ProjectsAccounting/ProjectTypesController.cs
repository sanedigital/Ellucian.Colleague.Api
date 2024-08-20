// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ProjectsAccounting.Services;
using Ellucian.Colleague.Dtos.ProjectsAccounting;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;

namespace Ellucian.Colleague.Api.Controllers.ProjectsAccounting
{
    /// <summary>
    /// Provides access to Project Types information.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ProjectsAccounting)]
    [Authorize]
    public class ProjectTypesController : BaseCompressedApiController
    {
        private readonly IProjectTypeService projectTypeService;

        /// <summary>
        /// This constructor initializes the ProjectsController object.
        /// </summary>
        /// <param name="projectTypeService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ProjectTypesController(IProjectTypeService projectTypeService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.projectTypeService = projectTypeService;
        }

        /// <summary>
        /// Get list of all project types.
        /// </summary>
        /// <returns></returns>
        /// <note>ProjectType is cached for 5 minutes.</note>
        [HttpGet]
        [HeaderVersionRoute("/project-types", 1, true, Name = "GetProjectTypes")]
        public async Task<ActionResult<IEnumerable<ProjectType>>> GetProjectTypesAsync()
        {
            var projectTypes = await projectTypeService.GetProjectTypesAsync();
            return Ok(projectTypes);
        }
    }
}

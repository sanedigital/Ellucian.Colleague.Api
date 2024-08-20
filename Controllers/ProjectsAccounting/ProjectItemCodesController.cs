// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ProjectsAccounting.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;

namespace Ellucian.Colleague.Api.Controllers.ProjectsAccounting
{
    /// <summary>
    /// Provides access to Item Code information
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ProjectsAccounting)]
    [Authorize]
    public class ProjectItemCodesController : BaseCompressedApiController
    {
        private readonly IProjectItemCodeService projectItemCodeService;

        /// <summary>
        /// Constructor to initialize ProjectItemCodesController object.
        /// </summary>
        /// <param name="projectItemCodeService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ProjectItemCodesController(IProjectItemCodeService projectItemCodeService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.projectItemCodeService = projectItemCodeService;
        }

        /// <summary>
        /// Get all of the item codes.
        /// </summary>
        /// <returns></returns>
        /// <note>ProjectItemCode is cached for 5 minutes.</note>
        [HttpGet]
        [HeaderVersionRoute("/project-item-codes", 1, true, Name = "GetProjectItemCodes")]
        public async Task<ActionResult<IEnumerable<ProjectItemCode>>> GetProjectItemCodesAsync()
        {
            var itemCodes = await projectItemCodeService.GetProjectItemCodesAsync();
            return Ok(itemCodes);
        }
    }
}

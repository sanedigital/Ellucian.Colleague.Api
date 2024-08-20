// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes student-specific PROFILE Application data 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ProfileApplicationsController : BaseCompressedApiController
    {
        private readonly IProfileApplicationService profileApplicationService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for ProfileApplicationsController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="profileApplicationService">ProfileApplicationService</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ProfileApplicationsController(
            IAdapterRegistry adapterRegistry,
            IProfileApplicationService profileApplicationService,
            ILogger logger,
            IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.profileApplicationService = profileApplicationService;
            this.logger = logger;
        }

        /// <summary>
        /// Get a student's profile applications for all award years
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student for whom to get ProfileApplications</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of the given student's profile applications for all award years</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/profile-applications", 1, true, Name = "AllProfileApplications")]
        public async Task<ActionResult<IEnumerable<ProfileApplication>>> GetProfileApplicationsAsync([FromRoute] string studentId, [FromQuery]bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId is required in Uri");
            }

            try
            {
                return Ok(await profileApplicationService.GetProfileApplicationsAsync(studentId, getActiveYearsOnly));
            }
            catch (PermissionsException pex)
            {
                var message = string.Format("User does not have access rights to student {0}", studentId);
                logger.LogError(pex, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred getting profile applications resources");
                return CreateHttpResponseException("Unknown error occurred getting profile applications resources");
            }
        }
    }
}

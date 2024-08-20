// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to JobApplicationSources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class JobApplicationSourcesController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the JobApplicationSourcesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public JobApplicationSourcesController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all job-application-sources
        /// </summary>
        /// <returns>All <see cref="Dtos.JobApplicationSources">JobApplicationSources</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/job-application-sources", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetJobApplicationSources", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<JobApplicationSources>>> GetJobApplicationSourcesAsync()
        {
            return new List<JobApplicationSources>();
        }

        /// <summary>
        /// Retrieve (GET) an existing job-application-sources
        /// </summary>
        /// <param name="guid">GUID of the job-application-sources to get</param>
        /// <returns>A jobApplicationSources object <see cref="Dtos.JobApplicationSources"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/job-application-sources/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetJobApplicationSourcesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.JobApplicationSources>> GetJobApplicationSourcesByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No job-application-sources was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new jobApplicationSources
        /// </summary>
        /// <param name="jobApplicationSources">DTO of the new jobApplicationSources</param>
        /// <returns>A jobApplicationSources object <see cref="Dtos.JobApplicationSources"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/job-application-sources", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostJobApplicationSourcesV10")]
        public async Task<ActionResult<Dtos.JobApplicationSources>> PostJobApplicationSourcesAsync([FromBody] Dtos.JobApplicationSources jobApplicationSources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing jobApplicationSources
        /// </summary>
        /// <param name="guid">GUID of the jobApplicationSources to update</param>
        /// <param name="jobApplicationSources">DTO of the updated jobApplicationSources</param>
        /// <returns>A jobApplicationSources object <see cref="Dtos.JobApplicationSources"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/job-application-sources/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutJobApplicationSourcesV10")]
        public async Task<ActionResult<Dtos.JobApplicationSources>> PutJobApplicationSourcesAsync([FromRoute] string guid, [FromBody] Dtos.JobApplicationSources jobApplicationSources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a jobApplicationSources
        /// </summary>
        /// <param name="guid">GUID to desired jobApplicationSources</param>
        [HttpDelete]
        [Route("/job-application-sources/{guid}", Name = "DefaultDeleteJobApplicationSources")]
        public async Task<IActionResult> DeleteJobApplicationSourcesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

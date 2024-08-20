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
    /// Provides access to JobApplicationStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class JobApplicationStatusesController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the JobApplicationStatusesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public JobApplicationStatusesController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all job-application-statuses
        /// </summary>
        /// <returns>All <see cref="Dtos.JobApplicationStatuses">JobApplicationStatuses</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/job-application-statuses", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetJobApplicationStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<JobApplicationStatuses>>> GetJobApplicationStatusesAsync()
        {
            return new List<JobApplicationStatuses>();
        }

        /// <summary>
        /// Retrieve (GET) an existing job-application-statuses
        /// </summary>
        /// <param name="guid">GUID of the job-application-statuses to get</param>
        /// <returns>A jobApplicationStatuses object <see cref="Dtos.JobApplicationStatuses"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/job-application-statuses/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetJobApplicationStatusesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.JobApplicationStatuses>> GetJobApplicationStatusesByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No job-application-statuses was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new jobApplicationStatuses
        /// </summary>
        /// <param name="jobApplicationStatuses">DTO of the new jobApplicationStatuses</param>
        /// <returns>A jobApplicationStatuses object <see cref="Dtos.JobApplicationStatuses"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/job-application-statuses", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostJobApplicationStatusesV10")]
        public async Task<ActionResult<Dtos.JobApplicationStatuses>> PostJobApplicationStatusesAsync([FromBody] Dtos.JobApplicationStatuses jobApplicationStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing jobApplicationStatuses
        /// </summary>
        /// <param name="guid">GUID of the jobApplicationStatuses to update</param>
        /// <param name="jobApplicationStatuses">DTO of the updated jobApplicationStatuses</param>
        /// <returns>A jobApplicationStatuses object <see cref="Dtos.JobApplicationStatuses"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/job-application-statuses/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutJobApplicationStatusesV10")]
        public async Task<ActionResult<Dtos.JobApplicationStatuses>> PutJobApplicationStatusesAsync([FromRoute] string guid, [FromBody] Dtos.JobApplicationStatuses jobApplicationStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a jobApplicationStatuses
        /// </summary>
        /// <param name="guid">GUID to desired jobApplicationStatuses</param>
        [HttpDelete]
        [Route("/job-application-statuses/{guid}", Name = "DefaultDeleteJobApplicationStatuses")]
        public async Task<IActionResult> DeleteJobApplicationStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

// Copyright 2024 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to batch jobs.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class BatchJobController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IBatchJobService _batchJobService;

        /// <summary>
        /// Default constructor for the batch job controller.
        /// </summary>
        /// <param name="batchJobService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BatchJobController(IBatchJobService batchJobService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _logger = logger;
            _batchJobService = batchJobService;
        }

        /// <summary>
        /// Retrieves batch job status information for the provided job ID.
        /// </summary>
        /// <param name="jobId">ID from an already submitted job.</param>
        /// <returns>An batch job state object <see cref="Dtos.Base.BatchJobState"/></returns>
        [HttpGet]
        [PermissionsFilter(BasePermissionCodes.GetJobStatus)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets the status of a batch job.", HttpMethodDescription = "Gets the status of a batch job by ID.")]
        [HeaderVersionRoute("/jobs/{jobId}/status", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetBatchJobState", IsEthosEnabled = true)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<Dtos.Base.BatchJobState>> GetBatchJobStateAsync(string jobId)
        {
            try
            {
                _batchJobService.ValidatePermissions(GetPermissionsMetaData());
                var batchJobState = await _batchJobService.GetBatchJobStateByIdAsync(jobId);

                return Ok(batchJobState);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Allows a user to submit a batch job request. Permissions are determined by the code of the requested job type
        /// </summary>
        /// <param name="jobSubmission"></param>
        /// <returns></returns>
        [HttpPost]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Creates a batch job.", HttpMethodDescription = "Creates a batch job for the JOB.SPEC specified.")]
        [HeaderVersionRoute("/jobs/submit", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "SubmitBatchJob", IsEthosEnabled = true)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<Dtos.Base.BatchJobSubmitResponse>> PostBatchJobAsync(BatchJobSubmit jobSubmission)
        {
            try
            {
                var jobSubmissionResponse = await _batchJobService.SubmitJobAsync(jobSubmission);

                return Ok(jobSubmissionResponse);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
    }
}

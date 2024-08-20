// Copyright 2012-2024 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.ComponentModel;
using System.Net;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Web.Http.ModelBinding;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to update Application status.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RecruiterController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IRecruiterService _recruiterService;

        /// <summary>
        /// Initializes a new instance of the RecruiterController class.
        /// </summary>
        /// <param name="recruiterService">Coordination service of type <see cref="IRecruiterService">IRecruiterService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RecruiterController(IRecruiterService recruiterService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _recruiterService = recruiterService;
            this._logger = logger;
        }

        /// <summary>
        /// Import a Recruiter application/prospect into Colleague.
        /// </summary>
        /// <param name="application">Application/prospect import data</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can import Recruiter applications/prospects into Colleague.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/recruiter-applications", 1, true, Name = "RecruiterApplications")]
        public async Task<IActionResult> PostApplicationAsync(Application application)
        {
            try
            {
                await _recruiterService.ImportApplicationAsync(application);
                return Ok();
            }
            catch (PermissionsException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates an existing application's status.
        /// </summary>
        /// <param name="application">Application update data</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can update an existing application's status.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/recruiter-application-statuses", 1, true, Name = "RecruiterApplicationStatuses")]
        public async Task<IActionResult> PostApplicationStatusAsync(Application application)
        {
            try
            {
                await _recruiterService.UpdateApplicationStatusAsync(application);
                return Ok();
            }
            catch (PermissionsException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Import a Recruiter test score into Colleague.
        /// </summary>
        /// <param name="testScore">Test score data</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can import a Recruiter test score into Colleague.
        /// </accessComments>
        [HttpPost]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [HeaderVersionRoute("/recruiter-test-scores", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "RecruiterTestScoresV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/recruiter-test-scores", 1, false, Name = "RecruiterTestScores")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Import a Recruiter test score into Colleague.",
            HttpMethodDescription = "Import a Recruiter test score into Colleague.")]
        public async Task<IActionResult> PostTestScoresAsync(TestScore testScore)
        {
            try
            {
                await _recruiterService.ImportTestScoresAsync(testScore);
                return Ok();
            }
            catch (PermissionsException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Import a Recruiter transcript course into Colleague.
        /// </summary>
        /// <param name="transcriptCourse">transcript course data</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can import a Recruiter transcript course into Colleague.
        /// </accessComments>
        [HttpPost]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [HeaderVersionRoute("/recruiter-transcript-courses", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "RecruiterTranscriptCoursesV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/recruiter-transcript-courses", 1, false, Name = "RecruiterTranscriptCourses")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Import a Recruiter transcript course into Colleague.",
            HttpMethodDescription = "Import a Recruiter transcript course into Colleague.")]
        public async Task<IActionResult> PostTranscriptCoursesAsync(TranscriptCourse transcriptCourse)
        {
            try
            {
                await _recruiterService.ImportTranscriptCoursesAsync(transcriptCourse);
                return Ok();
            }
            catch (PermissionsException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Import Recruiter communication history into Colleague.
        /// </summary>
        /// <param name="communicationHistory">communication history data</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can import Recruiter communication history into Colleague.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/recruiter-communication-history", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "RecruiterCommunicationHistoryV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/recruiter-communication-history", 1, false, Name = "RecruiterCommunicationHistory")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Import Recruiter communication history into Colleague.",
            HttpMethodDescription = "Import Recruiter communication history into Colleague.")]
        public async Task<IActionResult> PostCommunicationHistoryAsync(CommunicationHistory communicationHistory)
        {
            try
            {
                await _recruiterService.ImportCommunicationHistoryAsync(communicationHistory);
                return Ok();
            }
            catch (PermissionsException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Request communication history from Colleague.
        /// </summary>
        /// <param name="communicationHistory">communication history request</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can request Recruiter communication history into Colleague.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/recruiter-communication-history-request", 1, true, Name = "RecruiterCommunicationHistoryRequest")]
        public async Task<IActionResult> PostCommunicationHistoryRequestAsync(CommunicationHistory communicationHistory)
        {
            try
            {
                await _recruiterService.RequestCommunicationHistoryAsync(communicationHistory);
                return Ok();
            }
            catch (PermissionsException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Test connection from Colleague to Recruiter.
        /// </summary>
        /// <param name="connectionStatus">connection status request (empty or optional RecruiterOrganizationName)</param>
        /// <returns>Connection status response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can test the connection from Colleague to Recruiter.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/recruiter-connection-status", 1, true, Name = "RecruiterConnectionStatus")]
        public async Task<ActionResult<ConnectionStatus>> PostConnectionStatusAsync(ConnectionStatus connectionStatus)
        {
            try
            {
                ConnectionStatus resultDto = await _recruiterService.PostConnectionStatusAsync(connectionStatus);
                return resultDto;
            }
            catch (PermissionsException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Repositories;

using Newtonsoft.Json;
using Ellucian.Colleague.Api.Converters;
using Ellucian.Colleague.Domain.Base.Exceptions;
using System.Linq;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to student request data for transcript requests and enrollment verification requests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentRequestsController : BaseCompressedApiController
    {
        private readonly IStudentRequestService _requestService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initiatlize controller for StudentRequests.
        /// </summary>
        /// <param name="requestService">Student request service of type <see cref="IStudentRequestService"/></param>
        /// <param name="logger">logger of type <see cref="ILogger"/>ILogger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentRequestsController(IStudentRequestService requestService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _requestService = requestService;
            this._logger = logger;
        }


        /// <summary>
        /// Create a transcript request for a student
        /// </summary>
        /// <param name="transcriptRequest">The transcript request being added</param>
        /// <returns>Added <see cref="Dtos.Student.StudentTranscriptRequest">transcript request</see></returns>
        ///  <accessComments>
        /// Only the current user can create its own transcript request.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/students/{studentId}/student-transcript-request", 1, true, Name = "CreateStudentTranscriptRequests")]
        public async Task<ActionResult<Dtos.Student.StudentTranscriptRequest>> PostStudentTranscriptRequestAsync([FromBody]Dtos.Student.StudentTranscriptRequest transcriptRequest)
        {
            List<string> RequiredParametersNames = new List<string>();
            // Throw exception if incoming student transcript request is null
            if (transcriptRequest == null)
            {
                throw new ArgumentNullException("transcriptRequest", "Student transcript request object must be provided.");
            }

            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrEmpty(transcriptRequest.StudentId))
            {
                RequiredParametersNames.Add("StudentId");
            }
            if (string.IsNullOrEmpty(transcriptRequest.RecipientName))
            {
                RequiredParametersNames.Add("RecipientName");
            }
            if (transcriptRequest.MailToAddressLines == null || (transcriptRequest.MailToAddressLines != null && transcriptRequest.MailToAddressLines.Count <= 0))
            {
                RequiredParametersNames.Add("MailToAddressLines");

            }
            if (RequiredParametersNames != null && RequiredParametersNames.Count > 0)
            {
                string propertyNames = string.Join(",", RequiredParametersNames.ToArray());
                var message = string.Format("Student  Transcript request is missing {0}  required properties.", propertyNames.ToString());
                _logger.LogError(message);
                throw new ArgumentException("StudentTranscriptRequest", message);
            }
            try
            {
                Dtos.Student.StudentTranscriptRequest createdRequestDto = await _requestService.CreateStudentRequestAsync(transcriptRequest) as Dtos.Student.StudentTranscriptRequest;
                return Created(Url.Link("GetStudentTranscriptRequest", new { requestId = createdRequestDto.Id }), createdRequestDto);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException gaex)
            {
                _logger.LogInformation(gaex.ToString());
                SetResourceLocationHeader("GetStudentTranscriptRequest", new { id = gaex.ExistingResourceId });
                return CreateHttpResponseException(gaex.Message, HttpStatusCode.Conflict);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while creating student transcript request.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }


        }

        /// <summary>
        /// create enrollment verification requests
        /// </summary>
        /// <param name="enrollmentRequest"></param>
        /// <returns>Added <see cref="Dtos.Student.StudentEnrollmentRequest"/>Enrollment Request</returns>
        /// <accessComments>
        /// Only the current user can create its own enrollment request.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/students/{studentId}/student-enrollment-request", 1, true, Name = "CreateStudentEnrollmentRequests")]
        public async Task<ActionResult<Dtos.Student.StudentEnrollmentRequest>> PostStudentEnrollmentRequestAsync([FromBody]Dtos.Student.StudentEnrollmentRequest enrollmentRequest)
        {
            List<string> RequiredParametersNames = new List<string>();
            // Throw exception if incoming student enrollment verification request is nullenrollmentRequest
            if (enrollmentRequest == null)
            {
                throw new ArgumentNullException("enrollmentRequest", "Student enrollment verification request object must be provided.");
            }
            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrEmpty(enrollmentRequest.StudentId))
            {
                RequiredParametersNames.Add("StudentId");
            }
            if (string.IsNullOrEmpty(enrollmentRequest.RecipientName))
            {
                RequiredParametersNames.Add("RecipientName");
            }
            if (enrollmentRequest.MailToAddressLines == null || (enrollmentRequest.MailToAddressLines != null && enrollmentRequest.MailToAddressLines.Count <= 0))
            {
                RequiredParametersNames.Add("MailToAddressLines");

            }

            if (RequiredParametersNames != null && RequiredParametersNames.Count > 0)
            {
                string propertyNames = string.Join(",", RequiredParametersNames.ToArray());
                var message = string.Format("Student Enrollment  request is missing {0}  required properties.", propertyNames.ToString());
                _logger.LogError(message);
                throw new ArgumentException("StudentEnrollmentRequest", message);
            }
            try
            {
                Dtos.Student.StudentEnrollmentRequest createdRequestDto = await _requestService.CreateStudentRequestAsync(enrollmentRequest) as Dtos.Student.StudentEnrollmentRequest;
                return Created(Url.Link("GetStudentEnrollmentRequest", new { requestId = createdRequestDto.Id }), createdRequestDto);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while creating enrollment verification requests";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (ExistingResourceException gaex)
            {
                _logger.LogInformation(gaex.ToString());
                SetResourceLocationHeader("GetStudentEnrollmentRequest", new { id = gaex.ExistingResourceId });
                return CreateHttpResponseException(gaex.Message, HttpStatusCode.Conflict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Get student transcript request
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns> <see cref="Dtos.Student.StudentTranscriptRequest"/>Student Transcript Request</returns>
        ///  <accessComments>
        /// Only a student to which request applies can access its own transcript request.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/student-transcript-request/{requestId}", 1, true, Name = "GetStudentTranscriptRequest")]
        public async Task<ActionResult<Dtos.Student.StudentTranscriptRequest>> GetStudentTranscriptRequestAsync(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentException("Request id for student transcript request retrieval is not provided.");
            }
            try
            {
                return await _requestService.GetStudentRequestAsync(requestId) as Dtos.Student.StudentTranscriptRequest;
            }
            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to student transcript requests is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Invalid request Id specified to retrieve student transcript request", System.Net.HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving the student transcript request.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error occurred retrieving the student transcript request." + System.Net.HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Get student enrollment request
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns><see cref="Dtos.Student.StudentEnrollmentRequest"/> Student Enrollment Request </returns>
        /// <accessComments>
        /// Only a student to which request applies can access its own enrollment request.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/student-enrollment-request/{requestId}", 1, true, Name = "GetStudentEnrollmentRequest")]
        public async Task<ActionResult<Dtos.Student.StudentEnrollmentRequest>> GetStudentEnrollmentRequestAsync(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentException("Request id for student enrollment verification request retrieval is not provided.");
            }
            try
            {
                return await _requestService.GetStudentRequestAsync(requestId) as Dtos.Student.StudentEnrollmentRequest;
            }
            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to student enrollment verification request is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving student enrollment verification request.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Invalid request Id specified to retrieve enrollment verification request.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error occurred retrieving the student enrollment verification request." + System.Net.HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Get all student enrollment requests for a student
        /// </summary>
        /// <param name="studentId">ID of the student</param>
        /// <returns>List of <see cref="Dtos.Student.StudentEnrollmentRequest">Enrollment Verification Request</see> objects for the student</returns>
        ///  <accessComments>
        /// Only a student can retrieve its own enrollment requests.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/student-enrollment-requests", 1, true, Name = "GetStudentEnrollmentRequests")]
        public async Task<ActionResult<List<Dtos.Student.StudentEnrollmentRequest>>> GetStudentEnrollmentRequestsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("Student id for student enrollment verification requests retrieval is required.");
            }
            try
            {
                List<Dtos.Student.StudentEnrollmentRequest> enrollmentRequests = new List<Dtos.Student.StudentEnrollmentRequest>();
                var studentRequests = await _requestService.GetStudentRequestsAsync(studentId, "Enrollment");
                foreach (var req in studentRequests)
                {
                    enrollmentRequests.Add(req as Dtos.Student.StudentEnrollmentRequest);
                }
                return enrollmentRequests;
            }           
            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to student enrollment verification requests is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving the student enrollment verification requests.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error occurred retrieving the student enrollment verification requests." + System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all student transcript requests for a student
        /// </summary>
        /// <param name="studentId">ID of the student</param>
        /// <returns>List of <see cref="Dtos.Student.StudentTranscriptRequest">Transcript Request</see> objects for the student</returns>
        ///  <accessComments>
        /// Only a student can retrieve its own transcript requests.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/student-transcript-requests", 1, true, Name = "GetStudentTranscriptRequests")]
        public async Task<ActionResult<List<Dtos.Student.StudentTranscriptRequest>>> GetStudentTranscriptRequestsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("Student id for student transcript requests retrieval is required.");
            }
            try
            {
                List<Dtos.Student.StudentTranscriptRequest> transcriptRequests = new List<Dtos.Student.StudentTranscriptRequest>();
                var studentRequests = await _requestService.GetStudentRequestsAsync(studentId, "Transcript");
                foreach (var req in studentRequests)
                {
                    transcriptRequests.Add(req as Dtos.Student.StudentTranscriptRequest);
                }
                return transcriptRequests;
            }
            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to student transcript requests is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving student transcript requests for student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error occurred retrieving the student transcript requests." + System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves student request fee and distribution code for a specific student Id and request Id
        /// </summary>
        /// <param name="studentId">Id of the student to retrieve</param>
        /// <param name="requestId">Request Id for the specified student request</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.StudentRequestFee">Student Request Fee</see> object.</returns>
        /// <accessComments>
        /// Only a student to which request applies can access its own request fees. 
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/student-request/{requestId}/student-request-fees", 1, true, Name = "GetStudentRequestFee")]
        public async Task<ActionResult<Dtos.Student.StudentRequestFee>> GetStudentRequestFeeAsync(string studentId, string requestId)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentException("Student Id and Request Id are required to get student request fee information.");
            }
            try
            {
                return await _requestService.GetStudentRequestFeeAsync(studentId, requestId);
            }
            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to student transcript requests fees is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving the student request fee for this program.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error occurred retrieving the student request fee for this program." + System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}

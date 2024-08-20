// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Ellucian.Colleague.Api;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Graduation Application Information.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class GraduationApplicationsController : BaseCompressedApiController
    {
        private readonly IGraduationApplicationService _graduationApplicationService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the GraduationApplicationController class.
        /// </summary>
        /// <param name="graduationApplicationService">Graduation Application Service</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GraduationApplicationsController(IGraduationApplicationService graduationApplicationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _graduationApplicationService = graduationApplicationService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves a graduation application by student Id and program code asynchronously
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="programCode">Graduation Application program code</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">Graduation Application</see> object.</returns>
        /// <accessComments>
        /// A single graduation application can only be retrieved by the student who submitted the application.
        /// </accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "programCode" })]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/programs/{programCode}/graduation-application", 1, true, Name = "GetGraduationApplication")]
        public async Task<ActionResult<Dtos.Student.GraduationApplication>> GetGraduationApplicationAsync(string studentId, string programCode)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentException("Graduation Application is missing student Id and/or program Id required for retrieval.");
            }
            try
            {
                return await _graduationApplicationService.GetGraduationApplicationAsync(studentId, programCode);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to Graduation Application is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Invalid Graduation Application Id specified.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error occurred retrieving the requested graduation application.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a new Graduation Application asynchronously. Student must be pass any applicable graduation application eligibility rules and must not
        /// have previously submitted an application for the same program. 
        /// </summary>
        /// <param name="studentId">Student id passed through url</param>
        /// <param name="programCode">Program Code passed through url</param>
        /// <param name="graduationApplication">GraduationApplication dto object</param>
        /// <returns>
        /// If successful, returns the newly created Graduation Application in an http response with resource locator information. 
        /// If failure, returns the exception information. If failure due to existing Graduation Application already exists for the given student and program,
        /// it also returns resource locator to use to retrieve the existing item.
        /// </returns>
        /// <accessComments>
        /// A graduation application can only be created by the student applying for graduation.
        /// </accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "programCode" })]
        [HttpPost]
        [HeaderVersionRoute("/students/{studentId}/programs/{programCode}/graduation-application", 1, true, Name = "CreateGraduationApplication")]
        public async Task<ActionResult<GraduationApplication>> PostGraduationApplicationAsync(string studentId, string programCode, [FromBody]Dtos.Student.GraduationApplication graduationApplication)
        {
            // Throw exception if incoming graduation application is null
            if (graduationApplication == null)
            {
                throw new ArgumentNullException("graduationApplication", "Graduation Application object must be provided.");
            }

            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrEmpty(graduationApplication.StudentId) || string.IsNullOrEmpty(graduationApplication.ProgramCode))
            {
                throw new ArgumentException("Graduation Application is missing a required property.");
            }
            try
            {
                GraduationApplication createdApplicationDto = await _graduationApplicationService.CreateGraduationApplicationAsync(graduationApplication);
                return Created(Url.Link("GetGraduationApplication", new { studentId = createdApplicationDto.StudentId, programCode = createdApplicationDto.ProgramCode }), createdApplicationDto);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException gaex)
            {
                _logger.LogInformation(gaex.ToString());
                SetResourceLocationHeader("GetGraduationApplication", new { studentId = graduationApplication.StudentId, programCode = graduationApplication.ProgramCode });
                return CreateHttpResponseException(gaex.Message, HttpStatusCode.Conflict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieve list of all the graduation applications  submitted for  the student
        /// </summary>
        /// <param name="studentId">Id of the Student</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">List of Graduation Application</see></returns>
        /// <accessComments>
        /// Graduation Applications for the student can be retrieved only if:
        /// 1. A Student is accessing its own data.
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        ///  Privacy is enforced by this response. If any student has an assigned privacy code that the advisor is not authorized to access, the GraduaionApplication response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned. In this situation, 
        /// all details except the student Id are cleared from the specific GraduationApplication object.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/graduation-applications", 1, true, Name = "GetGraduationApplications")]
        public async Task<ActionResult<IEnumerable<Dtos.Student.GraduationApplication>>> GetGraduationApplicationsAsync(string studentId)
        {
            try
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    throw new ArgumentNullException("Student Id is required for retrieval.");
                }
                var privacyWrapper= await _graduationApplicationService.GetGraduationApplicationsAsync(studentId);
                var graduationApplication = privacyWrapper.Dto as IEnumerable<Ellucian.Colleague.Dtos.Student.GraduationApplication>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return Ok(graduationApplication);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to Graduation Application is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Invalid Student Id specified.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error occurred retrieving the graduation applications.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates an existing Graduation Application asynchronously.
        /// </summary>
        /// <param name="studentId">Student id passed through url</param>
        /// <param name="programCode">Program Code passed through url</param>
        /// <param name="graduationApplication">GraduationApplication dto object</param>
        /// <returns>
        /// If successful, returns the updated Graduation Application in an http response. 
        /// If failure, returns the exception information. 
        /// </returns>
        /// <accessComments>
        /// A graduation application can only be updated by the student of the application.
        /// </accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "programCode" })]
        [HttpPut]
        [HeaderVersionRoute("/students/{studentId}/programs/{programCode}/graduation-application", 1, true, Name = "UpdateGraduationApplication")]
        public async Task<ActionResult<GraduationApplication>> PutGraduationApplicationAsync(string studentId, string programCode, [FromBody]Dtos.Student.GraduationApplication graduationApplication)
        {
            // Throw exception if incoming graduation application is null
            if (graduationApplication == null)
            {
                throw new ArgumentNullException("graduationApplication", "Graduation Application object must be provided.");
            }
            graduationApplication.StudentId = studentId;
            graduationApplication.ProgramCode = programCode;
            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrWhiteSpace(graduationApplication.StudentId) || string.IsNullOrWhiteSpace(graduationApplication.ProgramCode))
            {
                throw new ArgumentException("Graduation Application is missing a required property.");
            }

            try
            {
                Dtos.Student.GraduationApplication updatedApplicationDto = await _graduationApplicationService.UpdateGraduationApplicationAsync(graduationApplication);
                return Ok(updatedApplicationDto);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Invalid Graduation Application Id specified.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves graduation application fee and payment information for a specific student Id and programCode
        /// </summary>
        /// <param name="studentId">Id of the student to retrieve</param>
        /// <param name="programCode">Program code for the specified graduation application</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplicationFee">Graduation Application Fee</see> object.</returns>
        /// <accessComments>Users may request their own data.</accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "programCode" })]
        [HttpGet]
        [HeaderVersionRoute("/graduation-application-fees/{studentId}/{programCode}", 1, true, Name = "GetGraduationApplicationFee")]
        public async Task<ActionResult<Dtos.Student.GraduationApplicationFee>> GetGraduationApplicationFeeAsync(string studentId, string programCode)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentException("Student Id and program Id are required to get application fee information.");
            }
            try
            {
                return await _graduationApplicationService.GetGraduationApplicationFeeAsync(studentId, programCode);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error occurred retrieving the graduation application fee for this program.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Checks to see if the student is eligible to apply for graduation in specific programs.
        /// </summary>
        /// <param name="criteria">Identifies the student and the programs for which eligibility is requested</param>
        /// <returns>A list of <see cref="GraduationApplicationProgramEligibility">Graduation Application Program Eligibility </see> items</returns>
        /// <accessComments>
        /// Graduation Application Eligibility for the student can be retrieved only if:
        /// 1. A Student is requesting their own eligibility or
        /// 2. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/graduation-application-eligibility", 1, true, Name = "QueryGraduationApplicationEligibility")]
        public async Task<ActionResult<IEnumerable<GraduationApplicationProgramEligibility>>> QueryGraduationApplicationEligibilityAsync(GraduationApplicationEligibilityCriteria criteria)
        {
            if (criteria == null)
            {
                _logger.LogError("Missing graduation application eligibility criteria");
                return CreateHttpResponseException("Missing graduation application eligibility criteria", HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await _graduationApplicationService.GetGraduationApplicationEligibilityAsync(criteria.StudentId, criteria.ProgramCodes));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

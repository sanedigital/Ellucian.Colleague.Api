// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provide access to faculty Consent and student petition data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentPetitionsController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly ISectionPermissionService _service;
        private readonly IStudentPetitionService _studentPetitionService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="studentPetitionService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentPetitionsController(ISectionPermissionService service, IStudentPetitionService studentPetitionService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _service = service;
            _studentPetitionService = studentPetitionService;
            this._logger = logger;
        }


        /// <summary>
        /// Creates a new Student Petition.
        /// </summary>
        /// <param name="studentPetition">StudentPetition dto object</param>
        /// <returns>
        /// If successful, returns the newly created Student Petition in an http response with resource locator information. 
        /// If failure, returns the exception information. If failure due to existing Student Petition found for the given student and section,
        /// also returns resource locator to use to retrieve the existing item.
        /// </returns>
        /// <accessComments>
        /// User must have correct permission code, depending on petition type:
        /// 1.A faculty member assigned to the section can add student petition with any of the following permission codes
        /// CREATE.STUDENT.PETITION
        /// CREATE.FACULTY.CONSENT
        /// 2.A departmental oversight member assigned to the section can add student petition with any of the following permission codes
        /// CREATE.SECTION.STUDENT.PETITION
        /// CREATE.SECTION.FACULTY.CONSENT
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/student-petitions", 1, true, Name = "AddStudentPetition")]
        public async Task<ActionResult<Dtos.Student.StudentPetition>> PostStudentPetitionAsync([FromBody] Dtos.Student.StudentPetition studentPetition)
        {
            try
            {
                Dtos.Student.StudentPetition createdPetitionDto = await _service.AddStudentPetitionAsync(studentPetition);
                return Created(Url.Link("GetStudentPetition", new { studentPetitionId = createdPetitionDto.Id, sectionId = createdPetitionDto.SectionId, type = createdPetitionDto.Type.ToString() }), createdPetitionDto);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingStudentPetitionException swex)
            {
                _logger.LogInformation(swex.ToString());

                // Create the get existing student petition by ID.
                SetResourceLocationHeader("GetStudentPetition", new { id = swex.ExistingStudentPetitionId, sectionId = swex.ExistingStudentPetitionSectionId, type = swex.ExistingStudentPetitionType });

                return CreateHttpResponseException(swex.Message, HttpStatusCode.Conflict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update Student Petition
        /// </summary>
        /// <param name="studentPetition">StudentPetition dto object</param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to update student petitions.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>
        /// If successful, returns the updated Student Petition in an http response with resource locator information. 
        /// If failure, returns the exception information. 
        /// </returns>
        /// <accessComments>
        /// User must have correct permission code, depending on petition type:
        /// 1.A faculty member assigned to the section can update student petition with any of the following permission codes
        /// CREATE.STUDENT.PETITION
        /// CREATE.FACULTY.CONSENT
        /// 2.A departmental oversight member assigned to the section can update student petition with any of the following permission codes
        /// CREATE.SECTION.STUDENT.PETITION
        /// CREATE.SECTION.FACULTY.CONSENT
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/student-petitions", 1, true, Name = "UpdateStudentPetition")]
        public async Task<ActionResult<StudentPetition>> PutStudentPetitionAsync([FromBody] Dtos.Student.StudentPetition studentPetition)
        {
            try
            {
                Dtos.Student.StudentPetition updatedPetitionDto = await _service.UpdateStudentPetitionAsync(studentPetition);
                return Created(Url.Link("GetStudentPetition", new { studentPetitionId = updatedPetitionDto.Id, sectionId = updatedPetitionDto.SectionId, type = updatedPetitionDto.Type.ToString() }), updatedPetitionDto);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the requested student petition based on the student petition Id, section Id, and type.
        /// The user making this request must be an instructor of the section for which the petition is being requested or it will generate a permission exception.
        /// </summary>
        /// <param name="studentPetitionId">Id of the student Petition (Required)</param>
        /// <param name="sectionId">Id of the section for which the petition is requested. (Required)</param>
        /// <param name="type">Type of student petition desired since same ID can yield either type. If not provided it will default to a petition of type StudentPetition.</param>
        /// <returns>Student Petition object</returns>
        /// <accessComments>
        /// 1.User must be faculty in specified section to get data.
        /// 2.A departmental oversight member assigned to the section may retrieve student petition with any of the following permission codes
        /// VIEW.SECTION.STUDENT.PETITIONS
        /// CREATE.SECTION.STUDENT.PETITION
        /// VIEW.SECTION.FACULTY.CONSENTS
        /// CREATE.SECTION.FACULTY.CONSENT
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/student-petitions/{studentPetitionId}/{sectionId}", 1, true, Name = "GetStudentPetition")]
        public async Task<ActionResult<Dtos.Student.StudentPetition>> GetAsync(string studentPetitionId, string sectionId, StudentPetitionType type)
        {
            try
            {
                return await _service.GetStudentPetitionAsync(studentPetitionId, sectionId, type);
            }
            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to Student Petition is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Invalid Student Petition Id specified.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error occurred retrieving the requested student petition." + System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the student petitions and faculty consents.
        /// </summary>
        /// <param name="studentId">Id of the student </param>
        /// <returns>Collection of Student Petition object</returns>
        /// <accessComments>
        /// 1. A student can access their own data
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
        [HttpGet]
        [HeaderVersionRoute("/student-petitions/{studentId}", 1, true, Name = "GetStudentPetitions")]
        public async Task<ActionResult<IEnumerable<Dtos.Student.StudentPetition>>> GetAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Unable to get student petitions. Invalid studentId " + studentId);
                return CreateHttpResponseException("Unable to get student petitions. Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _studentPetitionService.GetAsync(studentId));
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving student petitions";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                string message = "Access to Student Petition is forbidden.";
                _logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = "Error occurred retrieving the student petitions.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the student overload petitions
        /// </summary>
        /// <param name="studentId">Id of the student </param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve overload petitions.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>A list of <see cref="StudentOverloadPetition">StudentOverloadPetition</see> object</returns>
        /// <accessComments>
        /// 1. A student can access their own data
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
        [HttpGet]
        [HeaderVersionRoute("/student-overload-petitions/{studentId}", 1, true, Name = "GetStudentOverloadPetitions")]
        public async Task<ActionResult<IEnumerable<Dtos.Student.StudentOverloadPetition>>> GetStudentOverloadPetitionsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Unable to get student overload petitions. Invalid studentId " + studentId);
                return CreateHttpResponseException("Unable to get student overload petitions. Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _studentPetitionService.GetStudentOverloadPetitionsAsync(studentId));
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving student overload petitions";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                string message = "Access to Student Overload Petition is forbidden.";
                _logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = "Error occurred retrieving the student overload petitions.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.BadRequest);
            }
        }
    }

}

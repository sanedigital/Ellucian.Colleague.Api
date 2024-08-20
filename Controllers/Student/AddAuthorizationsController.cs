// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.
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


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Waiver data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AddAuthorizationsController : BaseCompressedApiController
    {
        private readonly IAddAuthorizationService _addAuthorizationService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Provides access to Student Waivers.
        /// </summary>
        /// <param name="addAuthorizationService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AddAuthorizationsController(IAddAuthorizationService addAuthorizationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _addAuthorizationService = addAuthorizationService;
            this._logger = logger;
        }

        /// <summary>
        /// Update an existing add authorization record.
        /// </summary>
        /// <param name="addAuthorization">Dto containing add authorization being updated.</param>
        /// <returns>Updated <see cref="AddAuthorization">AddAuthorization</see> DTO.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update an add authorization for this section.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if the record cannot be updated due to a lock.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if invalid student id or student locked or any other creation problem.</exception>
        /// <accessComments>
        /// This action can only be performed by:
        /// 1. a student who is assigning themselves to a previously unassigned add authorization code, or
        /// 2. a faculty member assigned to the section,or
        /// 3. a departmental oversight member assigned to the section may update an add authorization for a section with the following permission code 
        /// CREATE.SECTION.ADD.AUTHORIZATION
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/add-authorizations", 1, true, Name = "UpdateAddAuthorization")]
        public async Task<ActionResult<AddAuthorization>> PutAddAuthorizationAsync([FromBody] AddAuthorization addAuthorization)
        {
            if (addAuthorization == null)
            {
                string errorText = "Must provide the add authorization item to update.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }

            try
            {
                return await _addAuthorizationService.UpdateAddAuthorizationAsync(addAuthorization);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                _logger.LogInformation(pe.ToString());
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (RecordLockException re)
            {
                _logger.LogInformation(re.ToString());
                return CreateHttpResponseException(re.Message, HttpStatusCode.Conflict);
            }
            catch (System.Collections.Generic.KeyNotFoundException knfe)
            {
                _logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Add Authorization not found.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves Add Authorizations for a specific section
        /// </summary>
        /// <param name="sectionId">The id of the section</param>
        /// <returns>The <see cref="AddAuthorization">Add Authorizations</see> for the section.</returns>
        /// <accessComments>
        /// 1. Only permitted for faculty members assigned to the section.
        /// 2. A departmental oversight member assigned to the section may retrieve add authorization information with any of the following permission codes
        /// VIEW.SECTION.WAITLISTS
        /// VIEW.SECTION.ADD.AUTHORIZATIONS
        /// CREATE.SECTION.ADD.AUTHORIZATION
        /// </accessComments>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}/add-authorizations", 1, true, Name = "GetSectionAddAuthorizations")]
        public async Task<ActionResult<IEnumerable<AddAuthorization>>> GetSectionAddAuthorizationsAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                string errText = "Section Id must be provided to get section add authorizations.";
                _logger.LogError(errText);
                return CreateHttpResponseException(errText, HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await _addAuthorizationService.GetSectionAddAuthorizationsAsync(sectionId));
            }
            catch (ColleagueSessionExpiredException tex)
            {
                var message = "Session has expired while retrieving add autorizations for section " + sectionId;
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = "User is not authorized to retrieve add autorizations for section " + sectionId;
                _logger.LogInformation(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = "An error occurred while retrieving add autorizations for section " + sectionId;
                _logger.LogInformation(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create a new add authorization for a student in a section.
        /// </summary>
        /// <param name="addAuthorizationInput"><see cref="AddAuthorizationInput">Add Authorization Input</see> with information on creating a new authorization.</param>
        /// <returns>Newly created <see cref="AddAuthorization">Add Authorization</see>.</returns>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="AddAuthorization">add authorization</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create an add authorization for this section.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if an unrevoked authorization already exists for the student in the section.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if any other creation problem.</exception>
        /// <accessComments>
        /// This action can only be performed by:
        /// 1. a faculty member assigned to the section.
        /// 2. a departmental oversight member assigned to the section may create an add authorization for a section with the following permission code 
        /// CREATE.SECTION.ADD.AUTHORIZATION
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/add-authorizations", 1, true, Name = "CreateAddAuthorization")]
        public async Task<ActionResult<AddAuthorization>> PostAddAuthorizationAsync([FromBody] AddAuthorizationInput addAuthorizationInput)
        {
            if (addAuthorizationInput == null)
            {
                string errorText = "Must provide the add authorization input item to create a new authorization.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                AddAuthorization newAuthorization = await _addAuthorizationService.CreateAddAuthorizationAsync(addAuthorizationInput);
                return Created(Url.Link("GetAddAuthorization", new { id = newAuthorization.Id }), newAuthorization);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while creating add autorization.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = "User is not authorized to create add autorizations.";
                _logger.LogInformation(pe, message);
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException re)
            {
                var message = "Add authorization already exists.";
                _logger.LogInformation(re, message);
                return CreateHttpResponseException(re.Message, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                var message = "An error occurred while creating add autorization.";
                _logger.LogInformation(e, message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves information about a specific add authorization.
        /// </summary>
        /// <param name="id">Unique system Id of the add authorization</param>
        /// <returns>An <see cref="AddAuthorization">Add Authorization.</see></returns>
        /// <accessComments>
        /// This action can only be performed by the student who is assigned to the authorization, or by 
        /// a faculty member assigned to the section on the authorization.
        /// </accessComments>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/add-authorizations/{id}", 1, true, Name = "GetAddAuthorization")]
        public async Task<ActionResult<AddAuthorization>> GetAsync(string id)
        {
            try
            {
                var addAuthorization = await _addAuthorizationService.GetAsync(id);
                if (addAuthorization == null)
                {
                    return CreateNotFoundException("AddAuthorization", id);
                }
                return addAuthorization;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving add autorization.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                var message = "User is unable to retrieve add authorization Id " + id;
                this._logger.LogError(pex, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException kex)
            {
                var message = "Add Authorization not found for Id " + id;
                this._logger.LogError(kex, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                var message = "An error occurred while retrieving add autorization.";
                _logger.LogInformation(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieve add authorizations for a student
        /// </summary>
        /// <param name="studentId">ID of the student for whom add authorizations are being retrieved</param>
        /// <returns>Add Authorizations for the student</returns>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have at least one of the following permissions can request other users' data:
        /// ALL.ACCESS.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// VIEW.ANY.ADVISEE
        /// ALL.ACCESS.ASSIGNED.ADVISEES (the student must be an assigned advisee for the user)
        /// UPDATE.ASSIGNED.ADVISEES (the student must be an assigned advisee for the user)
        /// REVIEW.ASSIGNED.ADVISEES (the student must be an assigned advisee for the user)
        /// VIEW.ASSIGNED.ADVISEES (the student must be an assigned advisee for the user)
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/add-authorizations", 1, true, Name = "GetStudentAddAuthorizationsAsync")]
        public async Task<ActionResult<IEnumerable<AddAuthorization>>> GetStudentAddAuthorizationsAsync(string studentId)
        {
            try
            {
                var notices = await _addAuthorizationService.GetStudentAddAuthorizationsAsync(studentId);
                return Ok(notices);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving student's add autorizations";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string message = "User does not have permission to retrieve add authorizations for student.";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception exception)
            {
                string message = "Unable to retrieve add authorizations for student.";
                _logger.LogError(exception, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

    }
}

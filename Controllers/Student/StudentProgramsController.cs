// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to AcademicHistory data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentProgramsController : BaseCompressedApiController
    {
        private readonly IStudentProgramService _studentProgramService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CoursesController class.
        /// </summary>
        /// <param name="service">Service of type <see cref="IStudentProgramService">IStudentProgramService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentProgramsController(IStudentProgramService service, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentProgramService = service;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves <see cref="Dtos.Student.StudentProgram2">student programs</see> for the specified student IDs
        /// </summary>
        /// <param name="criteria">Criteria for retrieving student program information</param>
        /// <returns>List of <see cref="Dtos.Student.StudentProgram2">student programs</see></returns>
        /// <accessComments>
        /// An authenticated user (advisor) with any of the following permission codes may view student program information for any student:
        /// 
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// VIEW.STUDENT.INFORMATION
        /// 
        /// An authenticated user with any of the following permission codes who does not have one of the permission codes above may view student program information for assigned advisees only:
        /// 
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/student-programs", 1, true, Name = "GetStudentProgramsByIdList")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentProgram2>>> QueryStudentProgramsAsync([FromBody] StudentProgramsQueryCriteria criteria)
        {
            if (criteria.StudentIds == null)
            {
                _logger.LogError("Invalid studentIds");
                return CreateHttpResponseException("At least one Student Id is required.", HttpStatusCode.BadRequest);
            }
            if (!criteria.IncludeInactivePrograms && criteria.IncludeHistory)
            {
                _logger.LogError("Conflict between IncludeInactivePrograms and IncludeHistory parameters.");
                return CreateHttpResponseException("Cannot exclude inactive programs when requesting historical data.", HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _studentProgramService.GetStudentProgramsByIdsAsync(criteria.StudentIds, criteria.IncludeInactivePrograms, criteria.Term, criteria.IncludeHistory));
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving student programs";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Create a new academic program for a student.
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="studentAcademicProgram">Information for adding a academic program for student</param>
        /// <returns><see cref="Dtos.Student.StudentProgram2"> Newly created student program</see></returns>
        /// <accessComments>
        /// An authenticated user (advisor) with any of the following permission codes can add student program
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// VIEW.STUDENT.INFORMATION
        /// An authenticated user with any of the following permission codes who does not have one of the permission codes above can add student program:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/students/{studentId}/programs", 1, true, Name = "AddStudentProgram")]
        public async Task<ActionResult<Dtos.Student.StudentProgram2>> AddStudentProgramAsync([FromRoute] string studentId, [FromBody] StudentAcademicProgram studentAcademicProgram)
        {
            if (studentId == null)
            {
                string errorText = "Must provide the studentId to create a new program for student.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (studentAcademicProgram == null)
            {
                string errorText = "Must provide the studentAcademicProgram item to create a new program for student.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentProgramService.AddStudentProgram(studentAcademicProgram);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while adding new  program for student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Update (PUT) an existing student academic program.
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="studentAcademicProgram">Information for updating a academic program for student</param>
        /// <returns><see cref="Dtos.Student.StudentProgram2"> Updated student program</see></returns>
        /// <accessComments>
        /// An authenticated user (advisor) with any of the following permission codes can update student program
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// VIEW.STUDENT.INFORMATION
        /// An authenticated user with any of the following permission codes who does not have one of the permission codes above can update student program:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/students/{studentId}/programs", 1, true, Name = "UpdateStudentProgram")]
        public async Task<ActionResult<Dtos.Student.StudentProgram2>> UpdateStudentProgramAsync([FromRoute] string studentId, [FromBody] StudentAcademicProgram studentAcademicProgram)
        {
            if (studentId == null)
            {
                string errorText = "Must provide the studentId to update academic program for student.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (studentAcademicProgram == null)
            {
                string errorText = "Must provide the studentAcademicProgram to update academic program for student.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentProgramService.UpdateStudentProgram(studentAcademicProgram);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while updating program for student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }
    }
}

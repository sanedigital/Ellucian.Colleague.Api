// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
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


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides information about student attendances data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAttendanceController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IStudentAttendanceService _studentAttendanceService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="studentAttendanceService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentAttendanceController(IStudentAttendanceService studentAttendanceService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentAttendanceService = studentAttendanceService;
            this._logger = logger;
        }

        /// <summary>
        /// Query by post method used to get student attendance information based on criteria
        /// </summary>
        /// <remarks>If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database; otherwise, cached data is returned from the repository.</remarks>
        /// <param name="criteria">Object containing the section for which attendances are requested and other parameter choices.</param>
        /// <returns><see cref="StudentAttendance">Student Attendance</see> DTOs.</returns>
        /// <accessComments>
        /// 1. Only a faculty user who is assigned to the requested course section can view student attendance data for that course section
        /// 2. A departmental oversight member assigned to the section may retrieve student attendance information with any of the following permission code
        /// VIEW.SECTION.ATTENDANCE
        /// CREATE.SECTION.ATTENDANCE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/student-attendances", 1, true, Name = "QueryStudentAttendances")]
        public async Task<ActionResult<IEnumerable<StudentAttendance>>> QueryStudentAttendancesAsync(StudentAttendanceQueryCriteria criteria)
        {
            if (criteria == null || string.IsNullOrEmpty(criteria.SectionId))
            {
                string errorText = "criteria must contain a SectionId";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                bool useCache = true;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        useCache = false;
                    }
                }
                return Ok(await _studentAttendanceService.QueryStudentAttendancesAsync(criteria, useCache));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving student attendances.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                _logger.LogInformation(pe.ToString());
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Add new or update an existing student attendance for a student, section and meeting instance.
        /// </summary>
        /// <param name="studentAttendance">Object containing the section for which attendances are requested and other parameter choices.</param>
        /// <returns>Updated <see cref="StudentAttendance">Student Attendance</see> DTO.</returns>
        /// <accessComments>1) faculty user who is assigned to the requested course section can update student attendance data for that course section
        /// 2)A departmental oversight person for this section who has CREATE.SECTION.ATTENDANCE permission</accessComments>
        [HttpPut]
        [HeaderVersionRoute("/student-attendances", 1, true, Name = "PutStudentAttendance")]
        public async Task<ActionResult<StudentAttendance>> PutStudentAttendanceAsync([FromBody] StudentAttendance studentAttendance)
        {
            if (studentAttendance == null)
            {
                string errorText = "Must provide the student attendance to update.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentAttendanceService.UpdateStudentAttendanceAsync(studentAttendance);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while updating student attendances.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
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
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }
    }

}

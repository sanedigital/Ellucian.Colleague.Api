// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

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
    public class StudentSectionAttendancesController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IStudentSectionAttendancesService _studentSectionAttendancesService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="studentSectionAttendancesService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentSectionAttendancesController(IStudentSectionAttendancesService studentSectionAttendancesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentSectionAttendancesService = studentSectionAttendancesService;
            this._logger = logger;
        }

        /// <summary>
        /// Query by post method used to get student attendance information based on criteria.
        /// Criteria must have a studentId. SectionIds in criteria is optional.
        ///This returns student attendances for the given studentId and for the sections provided.
        ///If no section is provided in criteria then by default attendances for all the sections that belong to given studentId are returned.
        /// </summary>
        /// <param name="criteria">Object containing the studentId and then section for which attendances are requested.</param>
        /// <returns><see cref="StudentAttendance">Student Attendance</see> DTOs.</returns>
        /// <accessComments>Only an authenticated user can retrieve its own attendances.</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/student-section-attendances", 1, true, Name = "QueryStudentSectionAttendances")]
        public async Task<ActionResult<StudentSectionsAttendances>> QueryStudentSectionAttendancesAsync(StudentSectionAttendancesQueryCriteria criteria)
        {
            if (criteria == null || string.IsNullOrEmpty(criteria.StudentId))
            {
                string errorText = "criteria must contain a StudentId";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            
            try
            {
                return await _studentSectionAttendancesService.QueryStudentSectionAttendancesAsync(criteria);
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
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

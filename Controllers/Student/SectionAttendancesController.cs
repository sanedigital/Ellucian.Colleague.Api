// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Information about student attendances data for particular sections.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionAttendancesController : BaseCompressedApiController
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
        public SectionAttendancesController(IStudentAttendanceService studentAttendanceService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentAttendanceService = studentAttendanceService;
            this._logger = logger;
        }

        /// <summary>
        /// Update attendance information for a particular section and meeting instance.
        /// </summary>
        /// <param name="sectionAttendance"><see cref="SectionAttendance">Section Attendance</see> DTO that contains the section and the attendance information to be updated.</param>
        /// <returns><see cref="SectionAttendanceResponse">SectionAttendanceResponse</see> DTO.</returns>
        /// <accessComments>1) A faculty user who is assigned to the associated course section can update student attendance data for that course section.
        /// 2) A departmental oversight person for this section who has CREATE.SECTION.ATTENDANCE permission
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/section-attendances", 2, true, Name = "PutSectionAttendances2")]
        public async Task<ActionResult<SectionAttendanceResponse>> PutSectionAttendances2Async([FromBody] SectionAttendance sectionAttendance)
        {
            if (sectionAttendance == null)
            {
                string errorText = "Must provide the section attendance to update.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }

            try
            {
                return await _studentAttendanceService.UpdateSectionAttendance2Async(sectionAttendance);
            }
            catch (PermissionsException pe)
            {
                var message = "User is not authorized to update the section attendance for section id " + sectionAttendance.SectionId;
                _logger.LogError(pe, message);
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = "Unable to updaet the section attendance for section id " + sectionAttendance.SectionId;
                _logger.LogError(e, message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update attendance information for a particular section and meeting instance.
        /// </summary>
        /// <param name="sectionAttendance"><see cref="SectionAttendance">Section Attendance</see> DTO that contains the section and the attendance information to be updated.</param>
        /// <returns><see cref="SectionAttendanceResponse">SectionAttendanceResponse</see> DTO.</returns>
        /// <accessComments>1) A faculty user who is assigned to the associated course section can update student attendance data for that course section.
        /// 2)A departmental oversight person for this section who has CREATE.SECTION.ATTENDANCE permission
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/section-attendances", 1, false, Name = "PutSectionAttendances")]
        public async Task<ActionResult<SectionAttendanceResponse>> PutSectionAttendancesAsync([FromBody] SectionAttendance sectionAttendance)
        {

            if (sectionAttendance == null)
            {
                string errorText = "Must provide the section attendance to update.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentAttendanceService.UpdateSectionAttendanceAsync(sectionAttendance);
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

// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
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


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to nonacademic attendance requirement data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class NonAcademicAttendanceRequirementsController : BaseCompressedApiController
    {
        private readonly INonAcademicAttendanceService _nonAcademicAttendanceService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the NonAcademicAttendanceRequirementsController class.
        /// </summary>
        /// <param name="nonAcademicAttendanceService">Service of type <see cref="INonAcademicAttendanceService">INonAcademicAttendanceService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public NonAcademicAttendanceRequirementsController(INonAcademicAttendanceService nonAcademicAttendanceService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _nonAcademicAttendanceService = nonAcademicAttendanceService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person
        /// </summary>
        /// <param name="studentId">Unique identifier for the student whose requirements are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person</returns>
        /// <accessComments> 
        /// Student must be requesting their own attendance data
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/nonacademic-attendance-requirements", 1, true, Name = "GetNonAcademicAttendanceRequirements")]
        public async Task<ActionResult<IEnumerable<NonAcademicAttendanceRequirement>>> GetNonAcademicAttendanceRequirementsAsync(string studentId)
        {
            try
            {
                return Ok(await _nonAcademicAttendanceService.GetNonAcademicAttendanceRequirementsAsync(studentId));
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.ToString());
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ColleagueDataReaderException cdre)
            {
                string message = "An error occurred while trying to read nonacademic attendance requirement data from the database.";
                _logger.LogError(message, cdre.ToString());
                return CreateHttpResponseException(message);
            }
            catch (Exception ex)
            {
                string message = "An error occurred while trying to retrieve nonacademic attendance requirement information.";
                _logger.LogError(message, ex.ToString());
                return CreateHttpResponseException(message);
            }
        }
    }
}

// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.ComponentModel;
using System.Linq;
using System.Net;

using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Api.Licensing;
using Microsoft.Extensions.Logging;

using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Address data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentStandingsController : BaseCompressedApiController
    {
        private readonly IStudentStandingService _studentStandingService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentStandingsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentStandingService">Service of type <see cref="IStudentStandingService">IStudentStandingService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentStandingsController(IAdapterRegistry adapterRegistry, IStudentStandingService studentStandingService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentStandingService = studentStandingService;
            this._logger = logger;
        }

        /// <summary>
        /// Get a list of Student Standings from a list of Student keys
        /// </summary>
        /// <param name="criteria">DTO Object containing List of Student Keys and Term.</param>
        /// <returns>List of StudentStanding Objects <see cref="Ellucian.Colleague.Dtos.Student.StudentStanding">StudentStanding</see></returns>
        /// <accessComments>
        /// API endpoint is secured with VIEW.STUDENT.INFORMATION permission.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/student-standings", 1, true, Name = "GetStudentStandingsByIdList")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentStanding>>> QueryStudentStandingsAsync([FromBody] StudentStandingsQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            string term = criteria.Term;
            string currentTerm = criteria.CurrentTerm;

            if (studentIds == null || studentIds.Count() <= 0)
            {
                _logger.LogError("Invalid studentIds parameter. List was empty or null.");
                return CreateHttpResponseException("List of student ids is empty or null.", HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _studentStandingService.GetAsync(studentIds, term, currentTerm));
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                string message = "Session has expired while retrieving list of student standings from a list of student keys";
                _logger.LogError(csee, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "QueryStudentStandings error");
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// This retrieves student's academic standings.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>List of Student's Academic Standings</returns>
        ///<accessComments>
        /// Student Academic Standings can be retrieved only if:
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
        ///</accessComments>
        [HttpGet]
        [HeaderVersionRoute("/student-standings/{studentId}", 1, true, Name = "GetStudentStandings")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentStanding>>> GetStudentAcademicStandingsAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id passed to retrieve student's academic standing cannot be null or empty");
            }

            try
            {
                IEnumerable<Ellucian.Colleague.Dtos.Student.StudentStanding> studentAcademicStandings = await _studentStandingService.GetStudentAcademicStandingsAsync(studentId);
                return Ok(studentAcademicStandings);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving academic standings for student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string message = "A user needs to be self or an advisor in order to view student's academic standings";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = "An exception occured while retrieving student's academic standings for student with id- " + studentId;
                _logger.LogError(e, message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

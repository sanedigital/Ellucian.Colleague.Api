// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

using Ellucian.Web.Http.Filters;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Faculty data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FacultyController : BaseCompressedApiController
    {
        private readonly IFacultyService _facultyService;
        private readonly IFacultyRestrictionService _facultyRestrictionService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the FacultyController class.
        /// </summary>
        /// <param name="facultyService">Service of type <see cref="IFacultyService">IFacultyService</see></param>
        /// <param name="facultyRestrictionService">Service of type <see cref="IFacultyRestrictionService">IFacultyRestrictionService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FacultyController(IFacultyService facultyService, IFacultyRestrictionService facultyRestrictionService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _facultyService = facultyService;
            _facultyRestrictionService = facultyRestrictionService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves sections taught by faculty member.
        /// </summary>
        /// <param name="facultyId">A faculty ID</param>
        /// <param name="startDate">Optional, default to current date.</param>
        /// <param name="endDate">Optional, default to current date+90 days. Must be greater than start date if specified.</param>
        /// <param name="bestFit">Optional, true assigns a term to any non-term section based on the section start date. Defaults to false.</param>
        /// <returns>List of <see cref="Section">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve faculty course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section cannot retrieve list of active students Ids and
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        /// <note>Section is cached for 24 hours.</note>
        [Obsolete("Obsolete as of Api version 1.3, use version 3 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/faculty/{facultyId}/sections", 1, false, Name = "GetFacultySections")]
        public async Task<ActionResult<IEnumerable<Section>>> GetFacultySectionsAsync(string facultyId, DateTime? startDate = null, DateTime? endDate = null, bool bestFit = false)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                return new List<Section>();
            }
            try
            {
                var privacyWrapper = await _facultyService.GetFacultySectionsAsync(facultyId, startDate, endDate, bestFit);
                var sectionDtos = privacyWrapper.Dto as IEnumerable<Ellucian.Colleague.Dtos.Student.Section>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    SetContentRestrictedHeader("partial");
                }
                return Ok(sectionDtos);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves sections taught by faculty member.
        /// </summary>
        /// <param name="facultyId">A faculty ID</param>
        /// <param name="startDate">Optional, default to current date.</param>
        /// <param name="endDate">Optional, default to current date+90 days. Must be greater than start date if specified.</param>
        /// <param name="bestFit">Optional, true assigns a term to any non-term section based on the section start date. Defaults to false.</param>
        /// <returns>List of <see cref="Section">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve faculty course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section cannot retrieve list of active students Ids and
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        /// <note>Section is cached for 24 hours.</note>
        [Obsolete("Obsolete as of Api version 1.5, use version 3 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/faculty/{facultyId}/sections", 2, false, Name = "GetFacultySections2")]
        public async Task<ActionResult<IEnumerable<Section2>>> GetFacultySections2Async(string facultyId, DateTime? startDate = null, DateTime? endDate = null, bool bestFit = false)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                return new List<Section2>();
            }
            try
            {
                var privacyWrapper = await _facultyService.GetFacultySections2Async(facultyId, startDate, endDate, bestFit);
                var sectionDtos = privacyWrapper.Dto as IEnumerable<Ellucian.Colleague.Dtos.Student.Section2>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    SetContentRestrictedHeader("partial");
                }
                return Ok(sectionDtos);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves sections taught by faculty member.
        /// </summary>
        /// <param name="facultyId">A faculty ID - if not supplied an empty list of sections is returned.</param>
        /// <param name="startDate">Optional, ISO-8601 short date format, yyyy-mm-dd, default to current date.</param>
        /// <param name="endDate">Optional, ISO-8601 short date format, yyyy-mm-dd, default to current date+90 days. Must be greater than start date if specified.</param>
        /// <param name="bestFit">Optional, true assigns a term to any non-term section based on the section start date. Defaults to false.</param>
        /// <returns>List of <see cref="Section3">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve faculty course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section cannot retrieve list of active students Ids and
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [Obsolete("Obsolete as of Api version 1.13.1, use version 4 of this API")]
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "bestFit", "startDate", "endDate" })]
        [HttpGet]
        [HeaderVersionRoute("/faculty/{facultyId}/sections", 3, false, Name = "GetFacultySections3")]
        public async Task<ActionResult<IEnumerable<Section3>>> GetFacultySections3Async(string facultyId, DateTime? startDate = null, DateTime? endDate = null, bool bestFit = false)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                return new List<Section3>();
            }
            try
            {
                var privacyWrapper = await _facultyService.GetFacultySections3Async(facultyId, startDate, endDate, bestFit);
                var sectionDtos = privacyWrapper.Dto as IEnumerable<Ellucian.Colleague.Dtos.Student.Section3>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    SetContentRestrictedHeader("partial");
                }
                return Ok(sectionDtos);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a list of sections taught by faculty ID based on a date range or system parameters. If a start date is not provided sections will be returned based on 
        /// the allowed terms defined on Registration Web Parameters (RGWP), Class Schedule Web Parameters (CSWP) and Grading Web Parameters (GRWP).
        /// </summary>
        /// <param name="facultyId">A faculty ID. If not supplied, an empty list of sections is returned.</param>
        /// <param name="startDate">Optional, startDate, ISO-8601, yyyy-mm-dd.</param>
        /// <param name="endDate">Optional, endDate, ISO-8601, yyyy-mm-dd. If a start date is specified but end date is not, it will default to 90 days past start date. It must be greater than start date if specified, otherwise it will default to 90 days past start.</param>
        /// <param name="bestFit">Optional, true assigns a term to any non-term section based on the section start date. Defaults to false.</param>
        /// <returns>List of <see cref="Section3">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve faculty course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section cannot retrieve list of active students Ids and
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        [Obsolete("Obsolete as of Api version 1.31, use version 5 of this API")]
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "bestFit", "startDate", "endDate" })]
        [HttpGet]
        [HeaderVersionRoute("/faculty/{facultyId}/sections", 4, false, Name = "GetFacultySections4")]
        public async Task<ActionResult<IEnumerable<Section3>>> GetFacultySections4Async(string facultyId, DateTime? startDate = null, DateTime? endDate = null, bool bestFit = false)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                return new List<Section3>();
            }
            bool useCache = true;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                var privacyWrapper = await _facultyService.GetFacultySections4Async(facultyId, startDate, endDate, bestFit, useCache);
                var sectionDtos = privacyWrapper.Dto as IEnumerable<Ellucian.Colleague.Dtos.Student.Section3>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    SetContentRestrictedHeader("partial");
                }
                return Ok(sectionDtos);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a list of sections taught by faculty ID based on a date range or system parameters. If a start date is not provided sections will be returned based on 
        /// the allowed terms defined on Registration Web Parameters (RGWP), Class Schedule Web Parameters (CSWP) and Grading Web Parameters (GRWP).
        /// </summary>
        /// <param name="facultyId">A faculty ID. If not supplied, an empty list of sections is returned.</param>
        /// <param name="startDate">Optional, startDate, ISO-8601, yyyy-mm-dd.</param>
        /// <param name="endDate">Optional, endDate, ISO-8601, yyyy-mm-dd. If a start date is specified but end date is not, it will default to 90 days past start date. It must be greater than start date if specified, otherwise it will default to 90 days past start.</param>
        /// <param name="bestFit">Optional, true assigns a term to any non-term section based on the section start date. Defaults to false.</param>
        /// <returns>List of <see cref="Section3">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve faculty course section information; however,
        /// only an assigned faculty or departmental oversight user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty or departmental oversight to a course section cannot retrieve list of active students Ids and
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "bestFit", "startDate", "endDate" })]
        [HttpGet]
        [HeaderVersionRoute("/faculty/{facultyId}/sections", 5, true, Name = "GetFacultySections5")]
        public async Task<ActionResult<IEnumerable<Section4>>> GetFacultySections5Async(string facultyId, DateTime? startDate = null, DateTime? endDate = null, bool bestFit = false)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                return new List<Section4>();
            }
            bool useCache = true;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                var privacyWrapper = await _facultyService.GetFacultySections5Async(facultyId, startDate, endDate, bestFit, useCache);
                var sectionDtos = privacyWrapper.Dto as IEnumerable<Ellucian.Colleague.Dtos.Student.Section4>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    SetContentRestrictedHeader("partial");
                }
                return Ok(sectionDtos);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, "Session has expired while retrieving faculty details.");
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception exception)
            {
                var message = "An error occurred while retrieving faculty details";
                _logger.LogError(exception, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Retrieves many faculty members at once.
        /// </summary>
        /// <param name="ids">comma delimited list of IDs from request body</param>
        /// <returns>List of <see cref="Faculty">Faculty</see></returns>
        /// <accessComments>Any authenticated user can request faculty information.</accessComments>
        /// <note>Faculty data is cached for 24 hours.</note>
        [Obsolete("Obsolete as of API version 1.2, use the GET faculty/{id} API")]
        [HttpPost]
        [HeaderVersionRoute("/faculty", 1, true, Name = "GetFaculty")]
        public async Task<ActionResult<IEnumerable<Faculty>>> PostFacultyAsync([FromBody] string ids)
        {
            try
            {
                if (string.IsNullOrEmpty(ids))
                {
                    return new List<Faculty>();
                }
                var idList = ids.Trim().Split(',');
                var facultyList = new List<Faculty>();
                foreach (var id in idList)
                {
                    facultyList.Add(await _facultyService.GetAsync(id));
                }
                return facultyList;
            }
            catch (Exception ex)
            {
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a faculty member by ID.
        /// </summary>
        /// <param name="id">The ID of the faculty member to retrieve</param>
        /// <returns>The <see cref="Faculty">Faculty</see> data.</returns>
        /// <accessComments>Any authenticated user can request faculty information.</accessComments>
        /// <note>Faculty data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/faculty/{id}", 1, true, Name = "GetFaculty2")]
        public async Task<ActionResult<Faculty>> GetFacultyAsync(string id)
        {
            try
            {
                return await _facultyService.GetAsync(id);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving faculty data";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                var message = "An error occurred while retrieving faculty details";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the restrictions for the indicated faculty.
        /// </summary>
        /// <param name="facultyId">ID if the faculty</param>
        /// <returns>The list of <see cref="PersonRestriction">StudentRestrictions</see> found for this faculty.</returns>
        /// <accessComments>Users may retrieve their own restriction information</accessComments>
        [HttpGet]
        [HeaderVersionRoute("/faculty/{facultyId}/restrictions", 1, true, Name = "GetFacultyRestrictions")]
        public async Task<ActionResult<IEnumerable<PersonRestriction>>> GetFacultyRestrictionsAsync(string facultyId)
        {
            try
            {
                return Ok(await _facultyRestrictionService.GetFacultyRestrictionsAsync(facultyId));
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves many faculty members at once.
        /// </summary>
        /// <param name="criteria">criteria object including a comma delimited list of IDs from request body</param>
        /// <returns>List of <see cref="Faculty">Faculty</see></returns>
        ///<accessComments>Any authenticated user can request faculty information.</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/faculty", 1, true, Name = "GetFacultyByIds")]
        public async Task<ActionResult<IEnumerable<Faculty>>> QueryFacultyByPostAsync([FromBody] FacultyQueryCriteria criteria)
        {
            try
            {
                return Ok(await _facultyService.QueryFacultyAsync(criteria));
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving faculty details";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string message = "User does not have appropriate permissions to retrieve faculty details";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = "Exception occurred while retrieving faculty details";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Return a list of Faculty IDs for either Advisor Only or Faculty Only. Leave blank for all faculty.
        /// </summary>
        /// <param name="criteria">Contains flags for Faculty only and Advisor only.</param>
        /// <returns>List of faculty IDs</returns>
        /// <accessComments>Only users with any of the following permissions can retrieve list of Faculty IDs.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/query-faculty-ids", 1, true, Name = "PostFacultyIds")]
        public async Task<ActionResult<IEnumerable<string>>> PostFacultyIdsAsync([FromBody] FacultyQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    return Ok(await _facultyService.SearchFacultyIdsAsync(false, true));
                }
                return Ok(await _facultyService.SearchFacultyIdsAsync(criteria.IncludeFacultyOnly, criteria.IncludeAdvisorOnly));
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                string message = "Session has expired while retrieving list of faculty ids";
                _logger.LogError(csee, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves permissions for the current user to determine which faculty functions the user is allowed, such as ability to create a prerequisite waiver.
        /// </summary>
        /// <returns>List of strings representing the faculty permissions of this user</returns>
        /// <accessComments>Users may retrieve their own faculty permissions</accessComments>
        [Obsolete("Obsolete as of Colleague Web API 1.21. Use version 2 instead.")]
        [HttpGet]
        [HeaderVersionRoute("/faculty/permissions", 1, false, Name = "FacultyPermissions")]
        public async Task<ActionResult<IEnumerable<string>>> GetPermissionsAsync()
        {
            try
            {
                return Ok(await _facultyService.GetFacultyPermissionsAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Returns the faculty permissions for the authenticated user.
        /// </summary>
        /// <returns>The <see cref="FacultyPermissions">Faculty Permission</see> data.</returns>
        /// <accessComments>Users may retrieve their own faculty permissions</accessComments>
        [HttpGet]
        [HeaderVersionRoute("/faculty/permissions", 2, true, Name = "GetFacultyPermissions2")]
        public async Task<ActionResult<FacultyPermissions>> GetFacultyPermissions2Async()
        {
            try
            {
                return await _facultyService.GetFacultyPermissions2Async();
            }
            catch (ColleagueSessionExpiredException csee)
            {
                var message = "Session has expired while retrieving list of faculty permissions.";
                _logger.LogError(csee, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                var message = "An error occurred while retrieving faculty permissions.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the faculty office hours for the list of faculty ids.
        /// </summary>
        /// <returns>A list of FacultyOfficeHours for each faculty id</returns>
        /// <accessComments>Any authenticated user can request faculty information.</accessComments>
        /// <note>Faculty Office Hours for the specific faculty id is cached for 24 hours.</note>
        [HttpPost]
        [HeaderVersionRoute("/qapi/faculty/office-hours", 1, true, Name = "QueryFacultyOfficeHours")]
        public async Task<ActionResult<IEnumerable<FacultyOfficeHours>>> GetFacultyOfficeHoursAsync([FromBody] IEnumerable<string> facultyIds)
        {
            try
            {
                if (facultyIds != null)
                {
                    return Ok(await _facultyService.GetFacultyOfficeHoursAsync(facultyIds));
                }
                else
                {
                    throw new ArgumentNullException("facultyIds", "IDs cannot be empty/null for Faculty office hours retrieval.");
                }
            }
            catch (ColleagueSessionExpiredException csee)
            {
                string message = "Session has expired while retrieving list of faculty office hours";
                _logger.LogError(csee, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException("An error occurred while retrieving faculty office hours", HttpStatusCode.BadRequest);
            }
        }
    }
}

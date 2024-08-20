// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using Section = Ellucian.Colleague.Dtos.Student.Section;
using Section2 = Ellucian.Colleague.Dtos.Student.Section2;
using Section3 = Ellucian.Colleague.Dtos.Student.Section3;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to course Section data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionsController : BaseCompressedApiController
    {
        private readonly ISectionCoordinationService _sectionCoordinationService;
        private readonly ISectionRegistrationService _sectionRegistrationService;
        private readonly IRegistrationGroupService _registrationGroupService;
        private readonly ICourseService _courseService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the SectionsController class.
        /// </summary>
        /// <param name="sectionCoordinationService">Service of type <see cref="ISectionCoordinationService">ISectionCoordinationService</see></param>
        /// <param name="sectionRegistrationService">Service of type <see cref="ISectionRegistrationService">ISectionRegistrationService</see></param>
        /// <param name="registrationGroupService">Service of type <see cref="IRegistrationGroupService">IRegistrationGroupService</see></param>
        /// <param name="courseService">Service of type <see cref="ICourseService">courseService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionsController(ISectionCoordinationService sectionCoordinationService,
            ISectionRegistrationService sectionRegistrationService,
            IRegistrationGroupService registrationGroupService,
            ICourseService courseService,
            ILogger logger,
            IActionContextAccessor actionContextAccessor,
            ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _sectionCoordinationService = sectionCoordinationService;
            _sectionRegistrationService = sectionRegistrationService;
            _registrationGroupService = registrationGroupService;
            _courseService = courseService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves information about a specific course section. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionId">Id of the section desired</param>
        /// <returns>The requested <see cref="Dtos.Student.Section">Section</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section a list of active students Ids is not retrieved and
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        ///[CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [Obsolete("Obsolete as of Api version 1.3, use version 2 of this API")]
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}", 1, false, Name = "GetSection")]
        public async Task<ActionResult<Section>> GetSectionAsync(string sectionId)
        {
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
                var privacyWrapper = await _sectionCoordinationService.GetSectionAsync(sectionId, useCache);
                var sectionDto = privacyWrapper.Dto as Ellucian.Colleague.Dtos.Student.Section;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return Ok(sectionDto);
            }
            catch (KeyNotFoundException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateNotFoundException("Section", sectionId);
            }
            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Retrieves information about a specific course section. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionId">Id of the section desired</param>
        /// <returns>The requested <see cref="Dtos.Student.Section2">Section</see></returns>
        ///  <accessComments>
        /// Any authenticated user can retrieve course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section cannot retrieve list of active students Ids  and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        ///[CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [Obsolete("Obsolete as of Api version 1.5, use version 3 of this API")]
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}", 2, false, Name = "GetSection2")]
        public async Task<ActionResult<Section2>> GetSection2Async(string sectionId)
        {
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
                var privacyWrapper = await _sectionCoordinationService.GetSection2Async(sectionId, useCache);
                var sectionDto = privacyWrapper.Dto as Ellucian.Colleague.Dtos.Student.Section2;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return sectionDto;
            }
            catch (KeyNotFoundException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateNotFoundException("Section", sectionId);
            }
            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Retrieves information about a specific course section. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionId">Id of the section desired</param>
        /// <returns>The requested <see cref="Dtos.Student.Section3">Section3</see></returns>
        ///  <accessComments>
        /// Any authenticated user can retrieve course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section a list of active students Ids is not retrieved and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [Obsolete("Obsolete as of Api version 1.31, use version 4 of this API")]
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}", 3, false, Name = "GetSection3")]
        public async Task<ActionResult<Section3>> GetSection3Async(string sectionId)
        {
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
                var privacyWrapper = await _sectionCoordinationService.GetSection3Async(sectionId, useCache);
                var sectionDto = privacyWrapper.Dto as Ellucian.Colleague.Dtos.Student.Section3;

                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return sectionDto;
            }
            catch (KeyNotFoundException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateNotFoundException("Section", sectionId);
            }
            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves information about a specific course section. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionId">Id of the section desired</param>
        /// <returns>The requested <see cref="Dtos.Student.Section4">Section</see></returns>
        ///  <accessComments>
        /// Any authenticated user can retrieve course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section a list of active students Ids is not retrieved and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}", 4, true, Name = "GetSection4")]
        public async Task<ActionResult<Dtos.Student.Section4>> GetSection4Async(string sectionId)
        {
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
                var privacyWrapper = await _sectionCoordinationService.GetSection4Async(sectionId, useCache);
                var sectionDto = privacyWrapper.Dto as Ellucian.Colleague.Dtos.Student.Section4;

                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return Ok(sectionDto);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
                _logger.LogError(csse, invalidSessionErrorMessage);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException exception)
            {
                var message = "The section was not found for section " + sectionId;
                _logger.LogError(exception, message);
                return CreateNotFoundException("Section", sectionId);
            }
            catch (ArgumentNullException exception)
            {
                var message = "A section Id was not provided.";
                _logger.LogError(exception, message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                var message = "An error occurred while retrieving section details for section " + sectionId;
                _logger.LogError(exception, message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves information about faculty member indications that grading is complete for the section.
        /// </summary>
        /// <param name="sectionId">Id of the section</param>
        /// <returns>The requested <see cref="Dtos.Student.SectionMidtermGradingComplete">section grading completion indication information</see></returns>
        ///  <accessComments>
        /// 1. Only a faculty member assigned to the section may retrieve midterm grading completion information for a section.
        /// 2. A departmental oversight member assigned to the section may retrieve midterm grading completion information with the following permission code
        /// VIEW.SECTION.GRADING
        /// CREATE.SECTION.GRADING
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}/midterm-grading-complete", 1, true, Name = "GetMidtermSectionGradingComplete")]
        public async Task<ActionResult<SectionMidtermGradingComplete>> GetSectionMidtermGradingCompleteAsync([FromRoute] string sectionId)
        {
            try
            {
                var sectionCompleteDto = await _sectionCoordinationService.GetSectionMidtermGradingCompleteAsync(sectionId);
                return sectionCompleteDto;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving section midterm grading complete information.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Adds an indication that midterm grading is complete for one section and midterm grade number.
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="postInfo">Attributes of the midterm grading complete indication to be posted</param>
        /// <returns>The requested <see cref="Dtos.Student.SectionMidtermGradingComplete">section grading completion indication information</see></returns>        
        /// <accessComments>
        /// 1. A user with UPDATE.GRADES permission or assigned faculty on a section can indicate that midterm grading is complete.
        /// 2. A departmental oversight member assigned to the section with CREATE.SECTION.GRADING can indicate that midterm grading is complete.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/sections/{sectionId}/midterm-grading-complete", 1, true, Name = "PostMidtermSectionGradingComplete")]
        public async Task<ActionResult<SectionMidtermGradingComplete>> PostSectionMidtermGradingCompleteAsync([FromRoute] string sectionId, [FromBody] SectionMidtermGradingCompleteForPost postInfo)
        {
            try
            {
                var sectionCompleteDto = await _sectionCoordinationService.PostSectionMidtermGradingCompleteAsync(sectionId, postInfo);
                return sectionCompleteDto;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while posting section midterm grading complete information.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (ArgumentException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the grading status for a course section
        /// </summary>
        /// <param name="sectionId">Unique identifier for the course section</param>
        /// <returns>Grading status for the specified course section</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if a course section is not specified, or if there was a Colleage data or configuration error.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not authorized to retrieve section grading status information for the specified course section.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if data for a course section could not be retrieved.</exception>
        /// <accessComments>
        /// 1. The authenticated user must be an assigned faculty member for the specified course section in order to retrieve course section grading status information for that course section.
        /// 2. A departmental oversight member assigned to the section may retrieve course section grading status information with the following permission code
        /// VIEW.SECTION.GRADING
        /// CREATE.SECTION.GRADING
        /// </accessComments>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}/grading-status", 1, true, Name = "GetSectionGradingStatusAsync")]
        public async Task<ActionResult<Dtos.Student.SectionGradingStatus>> GetSectionGradingStatusAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                string sectionIdRequiredMessage = "A course section ID is required when retrieving course section grading status information.";
                _logger.LogError(sectionIdRequiredMessage);
                return CreateHttpResponseException(sectionIdRequiredMessage, HttpStatusCode.BadRequest);
            }
            try
            {
                var sectionPreliminaryAnonymousGrading = await _sectionCoordinationService.GetSectionGradingStatusAsync(sectionId);
                return sectionPreliminaryAnonymousGrading;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving course section grading status information for course section " + sectionId;
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string unauthorizedMessage = string.Format("Authenticated user is not an authorized to retrieve course section grading status information for course section {0}.", sectionId);
                _logger.LogError(pex, unauthorizedMessage);
                return CreateHttpResponseException(unauthorizedMessage, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                string notFoundMessage = string.Format("Could not retrieve information for course section {0}. Course section grading status information cannot be retrieved.", sectionId);
                _logger.LogError(knfex, notFoundMessage);
                return CreateHttpResponseException(notFoundMessage, HttpStatusCode.NotFound);
            }
            catch (ConfigurationException confe)
            {
                string configurationMessage = string.Format("A configuration error was encountered while retrieving course section grading status information for course section {0}.", sectionId);
                _logger.LogError(confe, configurationMessage);
                return CreateHttpResponseException(configurationMessage, HttpStatusCode.BadRequest);
            }
            catch (ColleagueException ce)
            {
                string dataErrorMessage = string.Format("A data error was encountered while retrieving course section grading status information for course section {0}.", sectionId);
                _logger.LogError(ce, dataErrorMessage);
                return CreateHttpResponseException(dataErrorMessage, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                string genericErrorMessage = string.Format("An error was encountered while retrieving course section grading status information for course section {0}.", sectionId);
                _logger.LogError(ex, genericErrorMessage);
                return CreateHttpResponseException(genericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves roster information for a course section.
        /// </summary>
        /// <param name="sectionId">ID of the course section for which roster students will be retrieved</param>
        /// <returns>All <see cref="RosterStudent">students</see> in the course section</returns>
        /// <accessComments>
        /// Requestor must be registered student or assigned faculty member for section.
        /// </accessComments>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [ParameterSubstitutionFilter]
        [Obsolete("Obsolete as of Api version 1.19, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}/roster", 1, false, Name = "GetSectionRoster")]
        public async Task<ActionResult<IEnumerable<RosterStudent>>> GetSectionRosterAsync(string sectionId)
        {
            try
            {
                return Ok(await _sectionCoordinationService.GetSectionRosterAsync(sectionId));
            }
            catch (ArgumentNullException e)
            {
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (ApplicationException e)
            {
                return CreateHttpResponseException(e.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Get a course roster for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A course roster</returns>
        /// <accessComments>
        /// 1. The requestor must be a registered student or assigned faculty member for the section.
        /// 2. A departmental oversight member assigned to the section may retrieve section roster information with any of the following permission codes
        /// VIEW.SECTION.ROSTER
        /// VIEW.SECTION.GRADING
        /// CREATE.SECTION.GRADING
        /// VIEW.SECTION.ADD.AUTHORIZATIONS
        /// CREATE.SECTION.ADD.AUTHORIZATION
        /// </accessComments>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}/roster", 2, true, Name = "GetSectionRoster2")]
        public async Task<ActionResult<SectionRoster>> GetSectionRoster2Async(string sectionId)
        {
            try
            {
                return await _sectionCoordinationService.GetSectionRoster2Async(sectionId);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
                _logger.LogError(csse, invalidSessionErrorMessage);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException e)
            {
                var message = "Cannot build a section roster without a course section Id.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException e)
            {
                var message = "Couldn't retrieve section information for given Section Id.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                var message = "An error occurred while retrieving section roster information.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(e.Message);
            }
        }


        /// <summary>
        /// Retrieves the waitlists for a given course sections ID. 
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>Section waitlist</returns>
        /// <accessComments>
        /// You must be an assigned faculty for the course section to retrieve section waitlist information. 
        /// </accessComments>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}/waitlist", 1, false, Name = "GetSectionWaitlist")]
        public async Task<ActionResult<SectionWaitlist>> GetSectionWaitlistAsync(string sectionId)
        {
            try
            {
                return await _sectionCoordinationService.GetSectionWaitlistAsync(sectionId);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return CreateHttpResponseException("Error retrieving section waitlists.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the waitlist section setting for a given course sections ID. 
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>Section waitlist config</returns>
        /// <accessComments>
        /// 1. The user must be an assigned faculty for the course section to retrieve section waitlist information. 
        /// 2. A departmental oversight member for the course section can retrieve section waitlist information with any of the following permission codes 
        /// VIEW.SECTION.ROSTER
        /// CREATE.SECTION.ADD.AUTHORIZATION
        /// VIEW.SECTION.WAITLISTS
        /// </accessComments>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}/waitlist-config", 1, true, Name = "GetSectionWaitlistConfig")]
        public async Task<ActionResult<SectionWaitlistConfig>> GetSectionWaitlistConfigAsync(string sectionId)
        {
            try
            {
                return await _sectionCoordinationService.GetSectionWaitlistConfigAsync(sectionId);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, "Session has expired while retrieving section waitlist configuration details");
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException e)
            {
                var message = "Cannot get a section waitlist config without a course section ID.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pex)
            {
                var message = "Access to Section waitlist settings is forbidden.";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = "Error retrieving section waitlist settings.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the waitlist details and student id for a given course sections ID. 
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>List of Section waitlist student</returns>
        /// <accessComments>
        /// You must be an assigned faculty for the course section to retrieve section waitlist information. 
        /// </accessComments>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}/waitlist", 2, true, Name = "GetSectionWaitlist2")]
        public async Task<ActionResult<IEnumerable<SectionWaitlistStudent>>> GetSectionWaitlist2Async(string sectionId)
        {
            try
            {
                return Ok(await _sectionCoordinationService.GetSectionWaitlist2Async(sectionId));
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return CreateHttpResponseException("Error retrieving section waitlists.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the waitlist details for a given course sections IDs. 
        /// </summary>
        /// <param name="criteria">This holds the section ids and a boolean to indicate if the cross listed section waitlist details are to be included or not</param>
        /// <returns>A list of student waitlist information</returns>
        /// <accessComments>
        /// 1. You must be an assigned faculty for the course section to retrieve section waitlist information. 
        /// 2. A departmental oversight member assigned to the section may retrieve section waitlist information with any of the following permission codes
        /// VIEW.SECTION.ROSTER
        /// CREATE.SECTION.ADD.AUTHORIZATION
        /// VIEW.SECTION.WAITLISTS
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/section-waitlist", 1, true, Name = "QuerySectionWaitlistAsync")]
        public async Task<ActionResult<IEnumerable<SectionWaitlistStudent>>> QuerySectionWaitlistAsync([FromBody] SectionWaitlistQueryCriteria criteria)
        {
            try
            {
                return Ok(await _sectionCoordinationService.GetSectionWaitlist3Async(criteria));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving section waitlists.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, pex.ToString());
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return CreateHttpResponseException("Error retrieving section waitlists.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the various waitlist statuses 
        /// </summary>    
        /// <returns>List of StudentWaitlistStatus</returns>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/waitlist-statuses", 1, true, Name = "GetStudentWaitlistStatuses")]
        public async Task<ActionResult<IEnumerable<StudentWaitlistStatus>>> GetStudentWaitlistStatusesAsync()
        {
            try
            {
                return Ok(await _sectionCoordinationService.GetStudentWaitlistStatusesAsync());
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving waitlist statuses.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error retrieving waitlist statuses.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the waitlist details for a given course Section Id and Student Id. 
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <param name="studentId">student ID</param>
        /// <returns><see cref="StudentSectionWaitlistInfo"> StudentSectionWaitlistInfo </see> object</returns> 
        /// <accessComments>
        /// Section waitlist information can only be retrieved by the student.
        /// 1. A Student is accessing their own data,
        /// 3. An Advisor with any of the following permissions is accessing any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following permissions is accessing one of his or her assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "studentId")]
        [HeaderVersionRoute("/sections/{sectionId}/waitlist-info", 1, true, Name = "GetStudentSectionWaitlistsByStudentAndSectionId")]
        public async Task<ActionResult<StudentSectionWaitlistInfo>> GetStudentSectionWaitlistsByStudentAndSectionIdAsync(string sectionId, string studentId)
        {
            try
            {
                return await _sectionCoordinationService.GetStudentSectionWaitlistsByStudentAndSectionIdAsync(sectionId, studentId);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving section waitlist information";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (ArgumentNullException e)
            {
                string message = "Arguments passed is null  while retrieving section waitlist information";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pex)
            {
                string message = "User does not have apporpriate permissions to retrieve section waitlist information";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = "Error retrieving student section waitlist details.";
                _logger.LogError(e, message);
                return CreateHttpResponseException("Error retrieving student section waitlist details.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the sections for the given section Ids. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionIds">comma delimited list of section IDs</param>
        /// <returns>The requested <see cref="Section">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course sections information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a given course section.
        /// For all other users that are not assigned faculty to a given course section a list of active students Ids is not retrieved and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        /// <note>Section is cached for 24 hours.</note>
        [Obsolete("Obsolete as of Api version 1.3, use version 2 of this API")]
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "sectionIds")]
        [HeaderVersionRoute("/sections", 1, false, Name = "GetSections")]
        public async Task<ActionResult<IEnumerable<Section>>> GetSectionsAsync(string sectionIds)
        {
            if (string.IsNullOrEmpty(sectionIds))
            {

                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);

            }
            var lstOfSectionIds = sectionIds.Trim().Split(',').ToList();

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
                var privacyWrapper = await _sectionCoordinationService.GetSectionsAsync(lstOfSectionIds, useCache);
                var sectionsDto = privacyWrapper.Dto as List<Ellucian.Colleague.Dtos.Student.Section>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return sectionsDto;
            }

            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Retrieves the sections for the given section Ids. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionIds">comma delimited list of section IDs</param>
        /// <returns>The requested <see cref="Section2">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course sections information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a given course section.
        /// For all other users that are not assigned faculty to a given course section a list of active students Ids is not retrieved and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        /// <note>Section is cached for 24 hours.</note>
        [Obsolete("Obsolete as of Api version 1.4, use endpoint POST qapi/sections")]
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "sectionIds")]
        [HeaderVersionRoute("/sections", 2, true, Name = "GetSections2")]
        public async Task<ActionResult<IEnumerable<Section2>>> GetSections2Async(string sectionIds)
        {
            if (string.IsNullOrEmpty(sectionIds))
            {

                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);

            }
            var lstOfSectionIds = sectionIds.Trim().Split(',').ToList();
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
                var privacyWrapper = await _sectionCoordinationService.GetSections2Async(lstOfSectionIds, useCache);
                var sectionsDto = privacyWrapper.Dto as List<Ellucian.Colleague.Dtos.Student.Section2>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return sectionsDto;
            }

            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query by post method used to get the sections for the given section Ids. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionIds">list of section IDs</param>
        /// <returns>The requested <see cref="Section2">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course sections information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a given course section.
        /// For all other users that are not assigned faculty to a given course section a list of active students Ids is not retrieved and  
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        /// <note>Section is cached for 24 hours.</note>
        [Obsolete("Obsolete as of Api version 1.5, use version 2 of this API")]
        [HttpPost]
        [HeaderVersionRoute("/qapi/sections", 1, false, Name = "QuerySectionsByPost")]
        public async Task<ActionResult<IEnumerable<Section2>>> QuerySectionsByPostAsync([FromBody] IEnumerable<string> sectionIds)
        {
            bool useCache = true;
            if (sectionIds == null)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                var privacyWrapper = await _sectionCoordinationService.GetSections2Async(sectionIds, useCache);
                var sectionsDto = privacyWrapper.Dto as List<Ellucian.Colleague.Dtos.Student.Section2>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return sectionsDto;
            }

            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Query by post method used to get the sections for the given section Ids. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionIds">list of section IDs</param>
        /// <returns>The requested <see cref="Section3">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course sections information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a given course section.
        /// For all other users that are not assigned faculty to a given course section a list of active students Ids is not retrieved and  
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        /// <note>Section is cached for 24 hours but newly added sections and those with a changed status are updated in the cache every 10 minutes.</note>
        [Obsolete("Obsolete as of Api version 1.6, use version 3 of this API")]
        [HttpPost]
        [HeaderVersionRoute("/qapi/sections", 2, false, Name = "QuerySectionsByPost2")]
        public async Task<ActionResult<IEnumerable<Section3>>> QuerySectionsByPost2Async([FromBody] IEnumerable<string> sectionIds)
        {
            bool useCache = true;
            if (sectionIds == null)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }

            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                var privacyWrapper = await _sectionCoordinationService.GetSections3Async(sectionIds, useCache);
                var sectionsDto = privacyWrapper.Dto as List<Ellucian.Colleague.Dtos.Student.Section3>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return sectionsDto;
            }

            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        #region HeDM Methods

        /// <summary>
        /// Update (PUT) section registrations
        /// </summary>
        /// <param name="guid">GUID of the Section</param>
        /// <param name="sectionRegistration">DTO of the SectionRegistration</param>
        /// <returns>A registration response object</returns>
        [Obsolete("Obsolete as of HeDM Version 4, use Accept Header Version 4 instead.")]
        [HttpPut]
        public async Task<ActionResult<Dtos.SectionRegistration>> PutSectionRegistrationAsync([FromQuery] string guid, [FromBody] Dtos.SectionRegistration sectionRegistration)
        {
            if (sectionRegistration == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null section argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(sectionRegistration.Guid))
            {
                sectionRegistration.Guid = sectionRegistration.Section.Guid.ToLowerInvariant();
            }
            else if (guid.ToLowerInvariant() != sectionRegistration.Guid.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                return await _sectionRegistrationService.UpdateRegistrationAsync(sectionRegistration);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        #endregion

        #region EEDM V6 Methods

        /// <summary>
        /// Return a list of Sections objects based on selection criteria.
        /// </summary>
        /// <param name="page">Section page Contains ...page...</param>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevels">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningInstitutionUnits">Section Department equal to (guid)</param>
        /// <returns>List of Section2 <see cref="Dtos.Section3"/> objects representing matching sections</returns>
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter(new string[] { "title", "startOn", "endOn", "code", "number", "instructionalPlatform", "academicPeriod", "academicLevels", "course", "site", "status", "owningInstitutionUnits" }, false, true)]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/sections", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionsV6", IsEedmSupported = true)]
        public async Task<IActionResult> GetHedmSections2Async(Paging page, [FromQuery] string title = "", [FromQuery] string startOn = "", [FromQuery] string endOn = "",
            [FromQuery] string code = "", [FromQuery] string number = "", [FromQuery] string instructionalPlatform = "", [FromQuery] string academicPeriod = "",
            [FromQuery] string academicLevels = "", [FromQuery] string course = "", [FromQuery] string site = "", [FromQuery] string status = "", [FromQuery] string owningInstitutionUnits = "")
        {
            if (page == null)
            {
                page = new Paging(100, 0);
            }
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (title == null || startOn == null || endOn == null || code == null || number == null || instructionalPlatform == null || academicPeriod == null || academicLevels == null || course == null || site == null || status == null || owningInstitutionUnits == null)
            {
                return new PagedActionResult<IEnumerable<Dtos.Section3>>(new List<Dtos.Section3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

            try
            {
                if ((!string.IsNullOrEmpty(status)) && (!ValidEnumerationValue(typeof(SectionStatus2), status)))
                {
                    throw new ColleagueWebApiException(string.Concat("'", status, "' is an invalid enumeration value. "));
                }

                var pageOfItems = await _sectionCoordinationService.GetSections3Async(page.Offset, page.Limit, title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, academicLevels, course, site, status, owningInstitutionUnits);

                AddEthosContextProperties(
                  await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Section3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A section object <see cref="Dtos.Section3"/> in HeDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/sections/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionByGuidV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Section3>> GetHedmSectionByGuid2Async(string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                   await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _sectionCoordinationService.GetSection3ByGuidAsync(id);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new section
        /// </summary>
        /// <param name="section">DTO of the new section</param>
        /// <returns>A section object <see cref="Dtos.Section3"/> in HeDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection)]
        [HeaderVersionRoute("/sections", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmSectionV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Section3>> PostHedmSection2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Section3 section)
        {
            if (section == null)
            {
                return CreateHttpResponseException("Request body must contain a valid Section.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null section id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (section.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }
            try
            {
                _sectionCoordinationService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section
                var sectionReturn = await _sectionCoordinationService.PostSection3Async(section);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionReturn.Id }));

                return sectionReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing section
        /// </summary>
        /// <param name="id">GUID of the section to update</param>
        /// <param name="section">DTO of the updated section</param>
        /// <returns>A section object <see cref="Dtos.Section3"/> in HeDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection)]
        [HttpPut]
        [HeaderVersionRoute("/sections/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmSectionV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Section3>> PutHedmSection2Async([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Section3 section)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (section == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null section argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                section.Id = id.ToLowerInvariant();
            }
            else if (id.ToLowerInvariant() != section.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _sectionCoordinationService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionReturn = await _sectionCoordinationService.PutSection3Async(
                    await PerformPartialPayloadMerge(section, async () => await _sectionCoordinationService.GetSection3ByGuidAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return sectionReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Delete (DELETE) a section
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>Nothing</returns>
        [HttpDelete]
        [Route("/sections/{id}", Name = "DeleteHedmSectionDefault", Order = -10)]
        public async Task<IActionResult> DeleteHedmSectionByGuid2Async(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region EEDM V8 Methods

        /// <summary>
        /// Return a list of Sections objects based on selection criteria.
        /// </summary>
        /// <param name="page"> - Section page Contains ...page...</param>
        /// <param name="criteria"> - JSON formatted selection criteria.  Can contain:</param>
        /// <param name="searchable"></param>
        /// <param name="keywordSearch"></param>
        /// <param name="subject"></param>
        /// <param name="instructor"></param>
        /// "title" - Section Title Contains ...title...
        /// "startOn" - Section starts on or after this date
        /// "endOn" - Section ends on or before this date
        /// "code" - Section Name Contains ...code...
        /// "number" - Section Number equal to
        /// "instructionalPlatform" - Learning Platform equal to (guid)
        /// "academicPeriod" - Section Term equal to (guid)
        /// "academicLevels" - Section Academic Level equal to (guid)
        /// "course" - Section Course equal to (guid)
        /// "site" - Section Location equal to (guid)
        /// "status" - Section Status matches closed, open, pending, or cancelled
        /// "owningInstitutionUnits" - Section Department equal to (guid) [renamed from owningOrganizations in v8]
        /// <returns>List of Section4 <see cref="Dtos.Section4"/> objects representing matching sections</returns>
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.SectionFilter))]
        [QueryStringFilterFilter("searchable", typeof(Dtos.Filters.SearchableFilter))]
        [QueryStringFilterFilter("keywordSearch", typeof(Dtos.Filters.KeywordSearchFilter))]
        [QueryStringFilterFilter("subject", typeof(Dtos.Filters.SubjectFilter))]
        [QueryStringFilterFilter("instructor", typeof(Dtos.Filters.InstructorFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/sections", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionsV8", IsEedmSupported = true)]
        public async Task<IActionResult> GetHedmSections4Async(Paging page, QueryStringFilter criteria,
           QueryStringFilter searchable, QueryStringFilter keywordSearch, QueryStringFilter subject,
           QueryStringFilter instructor)
        {
            string title = string.Empty, startOn = string.Empty, endOn = string.Empty, code = string.Empty,
                   number = string.Empty, instructionalPlatform = string.Empty, academicPeriod = string.Empty,
                   course = string.Empty, site = string.Empty, status = string.Empty,
                   instructorId = string.Empty, subjectName = string.Empty, keyword = string.Empty;

            var bypassCache = false;

            List<string> academicLevels = new List<string>(), owningOrganizations = new List<string>();
            SectionsSearchable search = SectionsSearchable.NotSet;

            try
            {
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var keywordSearchObj = GetFilterObject<Dtos.Filters.KeywordSearchFilter>(_logger, "keywordSearch");
                if (keywordSearchObj != null)
                {
                    keyword = keywordSearchObj.Search;
                }
                var searchableObj = GetFilterObject<Dtos.Filters.SearchableFilter>(_logger, "searchable");
                if (searchableObj != null)
                {
                    search = searchableObj.Search;
                }
                var subjectObj = GetFilterObject<Dtos.Filters.SubjectFilter>(_logger, "subject");
                if (subjectObj != null)
                {
                    subjectName = subjectObj.SubjectName != null && !string.IsNullOrEmpty(subjectObj.SubjectName.Id) ? subjectObj.SubjectName.Id : string.Empty;
                }
                var instructorObj = GetFilterObject<Dtos.Filters.InstructorFilter>(_logger, "instructor");
                if (instructorObj != null)
                {
                    instructorId = instructorObj.InstructorId != null && !string.IsNullOrEmpty(instructorObj.InstructorId.Id) ? instructorObj.InstructorId.Id : string.Empty;
                }
                var criteriaObj = GetFilterObject<Dtos.Filters.SectionFilter>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    title = criteriaObj.Title != null ? criteriaObj.Title : string.Empty;
                    startOn = criteriaObj.StartOn != null ? criteriaObj.StartOn.ToString() : string.Empty;
                    endOn = criteriaObj.EndOn != null ? criteriaObj.EndOn.ToString() : string.Empty;
                    code = criteriaObj.Code != null ? criteriaObj.Code : string.Empty;
                    number = criteriaObj.Number != null ? criteriaObj.Number : string.Empty;
                    instructionalPlatform = criteriaObj.InstructionalPlatform != null && !(string.IsNullOrEmpty(criteriaObj.InstructionalPlatform.Id))
                        ? criteriaObj.InstructionalPlatform.Id : string.Empty;
                    academicPeriod = criteriaObj.AcademicPeriod != null ? criteriaObj.AcademicPeriod.Id : string.Empty;
                    academicLevels = criteriaObj.AcademicLevels != null ? ConvertGuidObject2ListToStringList(criteriaObj.AcademicLevels) : new List<string>();
                    course = criteriaObj.Course != null && !(string.IsNullOrEmpty(criteriaObj.Course.Id)) ? criteriaObj.Course.Id : string.Empty;
                    site = criteriaObj.Site != null && !(string.IsNullOrEmpty(criteriaObj.Site.Id)) ? criteriaObj.Site.Id : string.Empty;
                    status = ((criteriaObj.Status != null) && (criteriaObj.Status.Category != SectionStatus2.NotSet))
                        ? criteriaObj.Status.Category.ToString() : string.Empty;

                    if ((criteriaObj.OwningInstitutionUnits != null) && (criteriaObj.OwningInstitutionUnits.Any()))
                    {
                        var organizations = new List<string>();
                        foreach (var owningInstitutionUnit in criteriaObj.OwningInstitutionUnits)
                        {
                            if ((owningInstitutionUnit != null) && (owningInstitutionUnit.InstitutionUnit != null))
                            {
                                organizations.Add(owningInstitutionUnit.InstitutionUnit.Id);
                            }
                        }
                        owningOrganizations = organizations;
                    }
                    // Subject needs to be supported in criteria object
                    if (string.IsNullOrEmpty(subjectName))
                    {
                        subjectName = criteriaObj.Subject != null && !string.IsNullOrEmpty(criteriaObj.Subject.Id) ? criteriaObj.Subject.Id : string.Empty;
                    }
                    if (string.IsNullOrEmpty(instructorId))
                    {
                        instructorId = criteriaObj.Instructor != null && !string.IsNullOrEmpty(criteriaObj.Instructor.Id) ? criteriaObj.Instructor.Id : string.Empty;
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Section4>>(new List<Dtos.Section4>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _sectionCoordinationService.GetSections4Async(page.Offset, page.Limit,
                    title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, academicLevels,
                    course, site, status, owningOrganizations, subjectName, instructorId, search, keyword);

                AddEthosContextProperties(
                  await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Section4>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (JsonReaderException e)
            {
                _logger.LogError(e.ToString());

                return CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                    IntegrationApiUtility.GetDefaultApiError("Error parsing JSON section search request.")));
            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e.ToString());

                return CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                    IntegrationApiUtility.GetDefaultApiError("Error parsing JSON section search request.")));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A section object <see cref="Dtos.Section4"/> in HeDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/sections/{id}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionByGuidV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Section4>> GetHedmSectionByGuid3Async(string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                   await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _sectionCoordinationService.GetSection4ByGuidAsync(id);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new section
        /// </summary>
        /// <param name="section">DTO of the new section</param>
        /// <returns>A section object <see cref="Dtos.Section4"/> in HeDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection)]
        [HttpPost]
        [HeaderVersionRoute("/sections", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmSectionV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Section4>> PostHedmSection4Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Section4 section)
        {
            if (section == null)
            {
                return CreateHttpResponseException("Request body must contain a valid Section.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null section id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (section.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            try
            {
                _sectionCoordinationService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section
                var sectionReturn = await _sectionCoordinationService.PostSection4Async(section);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionReturn.Id }));

                return sectionReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing section
        /// </summary>
        /// <param name="id">GUID of the section to update</param>
        /// <param name="section">DTO of the updated section</param>
        /// <returns>A section object <see cref="Dtos.Section4"/> in HeDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection)]
        [HttpPut]
        [HeaderVersionRoute("/sections/{id}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmSectionV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Section4>> PutHedmSection4Async([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Section4 section)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (section == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null section argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                section.Id = id.ToLowerInvariant();
            }
            else if (id.ToLowerInvariant() != section.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _sectionCoordinationService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionReturn = await _sectionCoordinationService.PutSection4Async(
                    await PerformPartialPayloadMerge(section, async () => await _sectionCoordinationService.GetSection4ByGuidAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return sectionReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Delete (DELETE) a section
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A section object <see cref="Dtos.Section4"/> in HeDM format</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteHedmSectionByGuid4Async(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region EEDM V11 Methods

        /// <summary>
        /// Return a list of Sections objects based on selection criteria.
        /// </summary>
        /// <param name="page"> - Section page Contains ...page...</param>
        /// <param name="criteria"> - JSON formatted selection criteria.  Can contain:</param>
        /// <param name="searchable"></param>
        /// <param name="keywordSearch"></param>
        /// <param name="subject"></param>
        /// <param name="instructor"></param>
        /// "title" - Section Title Contains ...title...
        /// "startOn" - Section starts on or after this date
        /// "endOn" - Section ends on or before this date
        /// "code" - Section Name Contains ...code...
        /// "number" - Section Number equal to
        /// "instructionalPlatform" - Learning Platform equal to (guid)
        /// "academicPeriod" - Section Term equal to (guid)
        /// "academicLevels" - Section Academic Level equal to (guid)
        /// "course" - Section Course equal to (guid)
        /// "site" - Section Location equal to (guid)
        /// "status" - Section Status matches closed, open, pending, or cancelled
        /// "owningInstitutionUnits" - Section Department equal to (guid) [renamed from owningOrganizations in v8]
        /// <returns>List of Section5 <see cref="Dtos.Section5"/> objects representing matching sections</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.SectionFilter2))]
        [QueryStringFilterFilter("searchable", typeof(Dtos.Filters.SearchableFilter))]
        [QueryStringFilterFilter("keywordSearch", typeof(Dtos.Filters.KeywordSearchFilter))]
        [QueryStringFilterFilter("subject", typeof(Dtos.Filters.SubjectFilter))]
        [QueryStringFilterFilter("instructor", typeof(Dtos.Filters.InstructorFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/sections", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionV11Default", IsEedmSupported = true, Order = -1)]
        public async Task<IActionResult> GetHedmSections5Async(Paging page, QueryStringFilter criteria,
           QueryStringFilter searchable, QueryStringFilter keywordSearch, QueryStringFilter subject,
           QueryStringFilter instructor)
        {
            string title = string.Empty, startOn = string.Empty, endOn = string.Empty, code = string.Empty,
                   number = string.Empty, instructionalPlatform = string.Empty, academicPeriod = string.Empty,
                   course = string.Empty, site = string.Empty, status = string.Empty,
                   instructorId = string.Empty, subjectName = string.Empty, keyword = string.Empty;

            var bypassCache = false;

            List<string> academicLevels = new List<string>(), owningOrganizations = new List<string>();
            SectionsSearchable search = SectionsSearchable.NotSet;

            try
            {
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var keywordSearchObj = GetFilterObject<Dtos.Filters.KeywordSearchFilter>(_logger, "keywordSearch");
                if (keywordSearchObj != null)
                {
                    keyword = keywordSearchObj.Search;
                }
                var searchableObj = GetFilterObject<Dtos.Filters.SearchableFilter>(_logger, "searchable");
                if (searchableObj != null)
                {
                    search = searchableObj.Search;
                }
                var subjectObj = GetFilterObject<Dtos.Filters.SubjectFilter>(_logger, "subject");
                if (subjectObj != null)
                {
                    subjectName = subjectObj.SubjectName != null && !string.IsNullOrEmpty(subjectObj.SubjectName.Id) ? subjectObj.SubjectName.Id : string.Empty;
                }
                var instructorObj = GetFilterObject<Dtos.Filters.InstructorFilter>(_logger, "instructor");
                if (instructorObj != null)
                {
                    instructorId = instructorObj.InstructorId != null && !string.IsNullOrEmpty(instructorObj.InstructorId.Id) ? instructorObj.InstructorId.Id : string.Empty;
                }
                var criteriaObj = GetFilterObject<Dtos.Filters.SectionFilter2>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    title = criteriaObj.Title != null ? criteriaObj.Title : string.Empty;
                    startOn = criteriaObj.StartOn != null ? criteriaObj.StartOn.ToString() : string.Empty;
                    endOn = criteriaObj.EndOn != null ? criteriaObj.EndOn.ToString() : string.Empty;
                    code = criteriaObj.Code != null ? criteriaObj.Code : string.Empty;
                    number = criteriaObj.Number != null ? criteriaObj.Number : string.Empty;
                    instructionalPlatform = criteriaObj.InstructionalPlatform != null && !(string.IsNullOrEmpty(criteriaObj.InstructionalPlatform.Id))
                        ? criteriaObj.InstructionalPlatform.Id : string.Empty;
                    academicPeriod = criteriaObj.AcademicPeriod != null ? criteriaObj.AcademicPeriod.Id : string.Empty;
                    academicLevels = criteriaObj.AcademicLevels != null ? ConvertGuidObject2ListToStringList(criteriaObj.AcademicLevels) : new List<string>();
                    course = criteriaObj.Course != null && !(string.IsNullOrEmpty(criteriaObj.Course.Id)) ? criteriaObj.Course.Id : string.Empty;
                    site = criteriaObj.Site != null && !(string.IsNullOrEmpty(criteriaObj.Site.Id)) ? criteriaObj.Site.Id : string.Empty;
                    status = ((criteriaObj.Status != null) && (criteriaObj.Status.Category != SectionStatus2.NotSet))
                        ? criteriaObj.Status.Category.ToString() : string.Empty;

                    if ((criteriaObj.OwningInstitutionUnits != null) && (criteriaObj.OwningInstitutionUnits.Any()))
                    {
                        var organizations = new List<string>();
                        foreach (var owningInstitutionUnit in criteriaObj.OwningInstitutionUnits)
                        {
                            if ((owningInstitutionUnit != null) && (owningInstitutionUnit.InstitutionUnit != null))
                            {
                                organizations.Add(owningInstitutionUnit.InstitutionUnit.Id);
                            }
                        }
                        owningOrganizations = organizations;
                    }
                    if (string.IsNullOrEmpty(subjectName))
                    {
                        subjectName = criteriaObj.SubjectName != null && !string.IsNullOrEmpty(criteriaObj.SubjectName.Id) ? criteriaObj.SubjectName.Id : string.Empty;
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Section5>>(new List<Dtos.Section5>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _sectionCoordinationService.GetSections5Async(page.Offset, page.Limit,
                    title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, academicLevels,
                    course, site, status, owningOrganizations, subjectName, instructorId, search, keyword);

                AddEthosContextProperties(
                  await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Section5>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired section</param>
        /// <returns>A section object <see cref="Dtos.Section5"/> in HeDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/sections/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionByGuidV11Default", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Section5>> GetHedmSectionByGuid5Async(string guid)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                   await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _sectionCoordinationService.GetSection5ByGuidAsync(guid);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new section
        /// </summary>
        /// <param name="section">DTO of the new section</param>
        /// <returns>A section object <see cref="Dtos.Section5"/> in HeDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection)]
        [HeaderVersionRoute("/sections", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmSectionV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Section5>> PostHedmSection5Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Section5 section)
        {
            if (section == null)
            {
                return CreateHttpResponseException("Request body must contain a valid Section.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null section id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (section.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }
            try
            {
                _sectionCoordinationService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section
                var sectionReturn = await _sectionCoordinationService.PostSection5Async(section);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionReturn.Id }));

                return sectionReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing section
        /// </summary>
        /// <param name="guid">GUID of the section to update</param>
        /// <param name="section">DTO of the updated section</param>
        /// <returns>A section object <see cref="Dtos.Section5"/> in HeDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection)]
        [HeaderVersionRoute("/sections/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmSectionV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Section5>> PutHedmSection5Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Section5 section)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (section == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null section argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                section.Id = guid.ToLowerInvariant();
            }
            else if (guid.ToLowerInvariant() != section.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _sectionCoordinationService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                //
                // Call an alternate version of Get by Id that excludes default census dates from  term 
                // or term/location so that subsequent Put conversion will only try to write census dates
                // that came from request body or the original section override census dates on disk.
                //
                var sectionReturn = await _sectionCoordinationService.PutSection5Async(
                    await PerformPartialPayloadMerge(section, async () => await _sectionCoordinationService.GetSection5ByGuidFilterCensusDatesAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return sectionReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }


        #endregion EEDM V11 Methods

        #region EEDM V16 Methods
        /// <summary>
        /// Return a list of Sections objects based on selection criteria.
        /// </summary>
        /// <param name="page"> - Section page Contains ...page...</param>
        /// <param name="criteria">filter criteria</param>
        /// <param name="searchable">named query</param>
        /// <param name="keywordSearch">named query</param>
        /// <param name="subject">named query</param>
        /// <param name="instructor">named query</param>
        /// "title" - Section Title Contains ...title...
        /// "startOn" - Section starts on or after this date
        /// "endOn" - Section ends on or before this date
        /// "code" - Section Name Contains ...code...
        /// "number" - Section Number equal to
        /// "instructionalPlatform" - Learning Platform equal to (guid)
        /// "academicPeriod" - Section Term equal to (guid)
        /// "academicLevels" - Section Academic Level equal to (guid)
        /// "course" - Section Course equal to (guid)
        /// "site" - Section Location equal to (guid)
        /// "status" - Section Status matches closed, open, pending, or cancelled
        /// "owningInstitutionUnits" - Section Department equal to (guid) [renamed from owningOrganizations in v8]
        /// <returns>List of Section6 <see cref="Dtos.Section6"/> objects representing matching sections</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Section6))]
        [QueryStringFilterFilter("searchable", typeof(Dtos.Filters.SearchableFilter))]
        [QueryStringFilterFilter("keywordSearch", typeof(Dtos.Filters.KeywordSearchFilter))]
        [QueryStringFilterFilter("subject", typeof(Dtos.Filters.SubjectFilter))]
        [QueryStringFilterFilter("instructor", typeof(Dtos.Filters.InstructorFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/sections", "16.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionV16Default", IsEedmSupported = true, Order = -20)]
        public async Task<IActionResult> GetHedmSections6Async(Paging page, QueryStringFilter criteria,
           QueryStringFilter searchable, QueryStringFilter keywordSearch, QueryStringFilter subject,
           QueryStringFilter instructor)
        {
            string title = string.Empty, startOn = string.Empty, endOn = string.Empty, code = string.Empty,
                   number = string.Empty, instructionalPlatform = string.Empty, academicPeriod = string.Empty,
                   reportingAcademicPeriod = string.Empty, course = string.Empty, site = string.Empty, status = string.Empty,
                   instructorId = string.Empty, subjectName = string.Empty, keyword = string.Empty;

            var bypassCache = false;

            List<string> academicLevels = new List<string>(), owningOrganizations = new List<string>();
            SectionsSearchable search = SectionsSearchable.NotSet;

            try
            {
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var keywordSearchObj = GetFilterObject<Dtos.Filters.KeywordSearchFilter>(_logger, "keywordSearch");
                if (keywordSearchObj != null)
                {
                    keyword = keywordSearchObj.Search;
                }
                var searchableObj = GetFilterObject<Dtos.Filters.SearchableFilter>(_logger, "searchable");
                if (searchableObj != null)
                {
                    search = searchableObj.Search;
                }
                var subjectObj = GetFilterObject<Dtos.Filters.SubjectFilter>(_logger, "subject");
                if (subjectObj != null)
                {
                    subjectName = subjectObj.SubjectName != null && !string.IsNullOrEmpty(subjectObj.SubjectName.Id) ? subjectObj.SubjectName.Id : string.Empty;
                }
                var instructorObj = GetFilterObject<Dtos.Filters.InstructorFilter>(_logger, "instructor");
                if (instructorObj != null)
                {
                    instructorId = instructorObj.InstructorId != null && !string.IsNullOrEmpty(instructorObj.InstructorId.Id) ? instructorObj.InstructorId.Id : string.Empty;
                }
                var criteriaObj = GetFilterObject<Dtos.Section6>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    title = criteriaObj.Titles != null && !string.IsNullOrEmpty(criteriaObj.Titles[0].Value) ? criteriaObj.Titles[0].Value : string.Empty;
                    startOn = criteriaObj.StartOn != null ? criteriaObj.StartOn.ToString() : string.Empty;
                    endOn = criteriaObj.EndOn != null ? criteriaObj.EndOn.ToString() : string.Empty;
                    code = criteriaObj.Code != null ? criteriaObj.Code : string.Empty;
                    number = criteriaObj.Number != null ? criteriaObj.Number : string.Empty;
                    instructionalPlatform = criteriaObj.InstructionalPlatform != null && !(string.IsNullOrEmpty(criteriaObj.InstructionalPlatform.Id))
                        ? criteriaObj.InstructionalPlatform.Id : string.Empty;
                    academicPeriod = criteriaObj.AcademicPeriod != null ? criteriaObj.AcademicPeriod.Id : string.Empty;
                    reportingAcademicPeriod = criteriaObj.ReportingAcademicPeriod != null ? criteriaObj.ReportingAcademicPeriod.Id : string.Empty;
                    academicLevels = criteriaObj.AcademicLevels != null ? ConvertGuidObject2ListToStringList(criteriaObj.AcademicLevels) : new List<string>();
                    course = criteriaObj.Course != null && !(string.IsNullOrEmpty(criteriaObj.Course.Id)) ? criteriaObj.Course.Id : string.Empty;
                    site = criteriaObj.Site != null && !(string.IsNullOrEmpty(criteriaObj.Site.Id)) ? criteriaObj.Site.Id : string.Empty;
                    status = ((criteriaObj.Status != null) && (criteriaObj.Status.Category != SectionStatus2.NotSet))
                        ? criteriaObj.Status.Category.ToString() : string.Empty;

                    if ((criteriaObj.OwningInstitutionUnits != null) && (criteriaObj.OwningInstitutionUnits.Any()))
                    {
                        var organizations = new List<string>();
                        foreach (var owningInstitutionUnit in criteriaObj.OwningInstitutionUnits)
                        {
                            if ((owningInstitutionUnit != null) && (owningInstitutionUnit.InstitutionUnit != null))
                            {
                                organizations.Add(owningInstitutionUnit.InstitutionUnit.Id);
                            }
                        }
                        owningOrganizations = organizations;
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Section6>>(new List<Dtos.Section6>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _sectionCoordinationService.GetSections6Async(page.Offset, page.Limit,
                    title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, reportingAcademicPeriod, academicLevels,
                    course, site, status, owningOrganizations, subjectName, instructorId, search, keyword, bypassCache);

                AddEthosContextProperties(
                  await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Section6>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (JsonReaderException e)
            {
                _logger.LogError(e.ToString());

                return CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                    IntegrationApiUtility.GetDefaultApiError("Error parsing JSON section search request.")));
            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e.ToString());

                return CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                    IntegrationApiUtility.GetDefaultApiError("Error parsing JSON section search request.")));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired section</param>
        /// <returns>A section object <see cref="Dtos.Section6"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/sections/{guid}", "16.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionByGuidV16Default", IsEedmSupported = true, Order = -20)]
        public async Task<ActionResult<Dtos.Section6>> GetHedmSectionByGuid6Async(string guid)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                   await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _sectionCoordinationService.GetSection6ByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new section
        /// </summary>
        /// <param name="section">DTO of the new section</param>
        /// <returns>A section object <see cref="Dtos.Section6"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection)]
        [HeaderVersionRoute("/sections", "16.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmSectionV16", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Section6>> PostHedmSection6Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Section6 section)
        {
            if (section == null)
            {
                return CreateHttpResponseException("Request body must contain a valid Section.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null section id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (section.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            try
            {
                _sectionCoordinationService.ValidatePermissions(GetPermissionsMetaData());
                // Don't allow update to alternate ID field
                if (section.AlternateIds != null && section.AlternateIds.Any())
                {
                    foreach (var altIds in section.AlternateIds)
                    {
                        if (!string.IsNullOrEmpty(altIds.Value))
                        {
                            throw new ArgumentException("alternateIds cannot be assigned in a POST request. ", "alternateIds");
                        }
                    }
                }
                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section
                var sectionReturn = await _sectionCoordinationService.PostSection6Async(section);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionReturn.Id }));

                return sectionReturn;
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing section
        /// </summary>
        /// <param name="guid">GUID of the section to update</param>
        /// <param name="section">DTO of the updated section</param>
        /// <returns>A section object <see cref="Dtos.Section6"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection)]
        [HeaderVersionRoute("/sections/{guid}", "16.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmSectionV16", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Section6>> PutHedmSection6Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Section6 section)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (section == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null section argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                section.Id = guid.ToLowerInvariant();
            }
            else if (guid.ToLowerInvariant() != section.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
            Guid guidOutput;
            if (!Guid.TryParse(guid, out guidOutput) || guid == Guid.Empty.ToString())
            {
                return CreateHttpResponseException("Input ID is not a valid GUID.", HttpStatusCode.BadRequest);
            }

            try
            {
                _sectionCoordinationService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var origSectionData = new Dtos.Section6();
                try
                {
                    // Call an alternate version of Get by Id that excludes default census dates from  term 
                    // or term/location so that subsequent Put conversion will only try to write census dates
                    // that came from request body or the original section override census dates on disk.
                    origSectionData = await _sectionCoordinationService.GetSection6ByGuidFilterCensusDatesAsync(guid);
                }
                catch (KeyNotFoundException)
                {
                    origSectionData = null;
                }

                if (origSectionData != null)
                {
                    if (origSectionData.Credits != null && origSectionData.Credits.Any() && section.Credits != null && section.Credits.Count() < 1)
                    {
                        // Put attempt is trying to explicitly clear credits with an empty object.  Do not allow.
                        // Optional in sections API, but required in Colleague.
                        throw new ArgumentException("A section must have either credits or CEUs defined");
                    }
                    if (section.Waitlist != null && section.Waitlist.Eligible == SectionWaitlistEligible.NotEligible)
                    {
                        // Replace waitlist object with the incoming object and ignore for partial merge.
                        origSectionData.Waitlist = section.Waitlist;
                    }
                    // Don't allow a change to the charge assessment method
                    if (origSectionData.ChargeAssessmentMethod != null && !string.IsNullOrEmpty(origSectionData.ChargeAssessmentMethod.Id))
                    {
                        if (section.ChargeAssessmentMethod != null && !string.IsNullOrEmpty(section.ChargeAssessmentMethod.Id))
                        {
                            if (origSectionData.ChargeAssessmentMethod.Id != section.ChargeAssessmentMethod.Id)
                            {
                                throw new ArgumentException("The charge assessment method cannot be changed on a PUT request.", "chargeAssessmentMethod.id");
                            }
                        }
                    }
                    // Don't allow update/change to alternate ID field
                    if (section.AlternateIds != null && section.AlternateIds.Any())
                    {
                        foreach (var altIds in section.AlternateIds)
                        {
                            if (altIds != null)
                            {
                                if (altIds.Title != "Source Key")
                                {
                                    throw new ArgumentException("The only alternateIds.title supported is 'Source Key'. ", "alternateIds.title");
                                }
                                var origAltIdsObject = origSectionData.AlternateIds.Where(sk => sk.Title == "Source Key").FirstOrDefault();
                                if (origAltIdsObject == null || altIds.Value != origAltIdsObject.Value)
                                {
                                    throw new ArgumentException("The 'Source Key' cannot be changed in a PUT request. ", "alternateIds.value");
                                }
                            }
                        }
                    }
                }
                //do update with partial logic
                var sectionReturn = await _sectionCoordinationService.PutSection6Async(
                    await PerformPartialPayloadMerge(section, origSectionData,
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return sectionReturn;
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        #endregion EEDM V13 Methods

        /// <summary>
        /// Query by post method used to get the sections for the given section Ids. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="criteria">DTO Object with a list of Section keys</param>
        /// <returns>The requested <see cref="Section3">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course sections information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a given course section.
        ///For all other users that are not assigned faculty to a given course section a list of active students Ids is not retrieved and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        /// <note>Section is cached for 24 hours but newly added sections and those with a changed status are updated in the cache every 10 minutes.</note>
        [Obsolete("Obsolete as of Api version 1.31, use version 4 of this API")]
        [HttpPost]
        [HeaderVersionRoute("/qapi/sections", 3, false, Name = "QuerySectionsByPost3")]
        public async Task<ActionResult<IEnumerable<Section3>>> QuerySectionsByPost3Async([FromBody] SectionsQueryCriteria criteria)
        {
            if (criteria == null || criteria.SectionIds == null)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            bool bestFit = criteria.BestFit;
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
                var privacyWrapper = await _sectionCoordinationService.GetSections3Async(criteria.SectionIds, useCache, bestFit);
                var sectionsDto = privacyWrapper.Dto as List<Ellucian.Colleague.Dtos.Student.Section3>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return sectionsDto;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving sections";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query by post method used to get the sections for the given section Ids. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="criteria">DTO Object with a list of Section keys</param>
        /// <returns>The requested <see cref="Dtos.Student.Section4">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course sections information; however,
        /// only an assigned faculty or departmental oversight user may retrieve list of active students Ids in a given course section.
        /// For all other users that are not assigned faculty or departmental oversight to a given course section a list of active students Ids is not retrieved and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        /// <note>Course section information is cached for 24 hours but newly added sections and those with a changed status are updated in the cache every 10 minutes.</note>
        [HttpPost]
        [HeaderVersionRoute("/qapi/sections", 4, true, Name = "QuerySectionsByPost4")]
        public async Task<ActionResult<IEnumerable<Dtos.Student.Section4>>> QuerySectionsByPost4Async([FromBody] SectionsQueryCriteria criteria)
        {
            if (criteria == null || criteria.SectionIds == null)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            bool bestFit = criteria.BestFit;
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
                var privacyWrapper = await _sectionCoordinationService.GetSections4Async(criteria.SectionIds, useCache, bestFit);
                var sectionsDto = privacyWrapper.Dto as List<Ellucian.Colleague.Dtos.Student.Section4>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return sectionsDto;
            }

            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, exception.Message);
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Puts a collection of student section grades.
        /// </summary>
        /// <returns><see cref="SectionGradeResponse">StudentSectionGradeResponse</see></returns>
        /// <accessComments>
        /// A user with UPDATE.GRADES permission can update student grades for the given section.
        /// </accessComments>
        [Obsolete("Obsolete , use version 2 of this API")]
        [HttpPut]
        [HeaderVersionRoute("/sections/{sectionId}/grades", 1, false, Name = "PutCollectionOfStudentGrades")]
        public async Task<ActionResult<IEnumerable<SectionGradeResponse>>> PutCollectionOfStudentGradesAsync([FromRoute] string sectionId, [FromBody] SectionGrades sectionGrades)
        {
            try
            {
                if (ModelState != null && !ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                    if (modelErrors != null && modelErrors.Count() > 0)
                    {
                        var formatExceptions = modelErrors.Where(x => x.Exception is System.FormatException).Select(x => x.Exception as System.FormatException).ToList();

                        if (formatExceptions != null && formatExceptions.Count() > 0)
                        {
                            throw formatExceptions.First();
                        }
                    }
                }

                if (string.IsNullOrEmpty(sectionGrades.SectionId))
                {
                    throw new ArgumentException("SectionId", "Section Id must be provided.");
                }

                // Compare uri value to body value for section Id
                if (!sectionId.Equals(sectionGrades.SectionId))
                {
                    throw new ArgumentException("sectionId", "Section Ids do not match in the request.");
                }

                if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                {
                    throw new ArgumentException("StudentGrades", "At least one student grade must be provided.");
                }

                var returnDto = await _sectionCoordinationService.ImportGradesAsync(sectionGrades);
                return Ok(returnDto);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Puts a collection of student section grades.
        /// </summary>
        /// <returns><see cref="SectionGradeResponse">StudentSectionGradeResponse</see></returns>
        /// <accessComments>
        /// A user with UPDATE.GRADES permission can update student grades for the given section.
        /// </accessComments>
        [Obsolete("Obsolete as of Api version 1.12, use version 3 of this API")]
        [HttpPut]
        [HeaderVersionRoute("/sections/{sectionId}/grades", 2, false, Name = "PutCollectionOfStudentGrades2")]
        public async Task<ActionResult<IEnumerable<SectionGradeResponse>>> PutCollectionOfStudentGrades2Async([FromRoute] string sectionId, [FromBody] SectionGrades2 sectionGrades)
        {
            try
            {
                if (ModelState != null && !ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                    if (modelErrors != null && modelErrors.Count() > 0)
                    {
                        var formatExceptions = modelErrors.Where(x => x.Exception is System.FormatException).Select(x => x.Exception as System.FormatException).ToList();

                        if (formatExceptions != null && formatExceptions.Count() > 0)
                        {
                            throw formatExceptions.First();
                        }
                    }
                }

                if (string.IsNullOrEmpty(sectionGrades.SectionId))
                {
                    throw new ArgumentException("SectionId", "Section Id must be provided.");
                }

                // Compare uri value to body value for section Id
                if (!sectionId.Equals(sectionGrades.SectionId))
                {
                    throw new ArgumentException("sectionId", "Section Ids do not match in the request.");
                }

                if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                {
                    throw new ArgumentException("StudentGrades", "At least one student grade must be provided.");
                }

                var returnDto = await _sectionCoordinationService.ImportGrades2Async(sectionGrades);
                return Ok(returnDto);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Puts a collection of student section grades.
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <returns><see cref="Dtos.Student.Grade">StudentSectionGradeResponse</see></returns>
        /// <accessComments>
        /// A user with UPDATE.GRADES permission or assigned faculty on a section can update students grades for the given section.
        /// </accessComments>
        [Obsolete("Obsolete as of Api version 1.13, use version 4 for non-ILP callers, or version 1 of the json ILP header for ILP callers")]
        [HttpPut]
        [HeaderVersionRoute("/sections/{sectionId}/grades", 3, false, Name = "PutCollectionOfStudentGrades3")]
        public async Task<ActionResult<IEnumerable<SectionGradeResponse>>> PutCollectionOfStudentGrades3Async([FromRoute] string sectionId, [FromBody] SectionGrades3 sectionGrades)
        {
            try
            {
                if (ModelState != null && !ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                    if (modelErrors != null && modelErrors.Count() > 0)
                    {
                        var formatExceptions = modelErrors.Where(x => x.Exception is System.FormatException).Select(x => x.Exception as System.FormatException).ToList();

                        if (formatExceptions != null && formatExceptions.Count() > 0)
                        {
                            throw formatExceptions.First();
                        }
                    }
                }

                if (string.IsNullOrEmpty(sectionGrades.SectionId))
                {
                    throw new ArgumentException("SectionId", "Section Id must be provided.");
                }

                // Compare uri value to body value for section Id
                if (!sectionId.Equals(sectionGrades.SectionId))
                {
                    throw new ArgumentException("sectionId", "Section Ids do not match in the request.");
                }

                if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                {
                    throw new ArgumentException("StudentGrades", "At least one student grade must be provided.");
                }

                var returnDto = await _sectionCoordinationService.ImportGrades3Async(sectionGrades);
                return Ok(returnDto);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Puts a collection of student section grades from a standard non-ILP caller.
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <returns><see cref="Dtos.Student.Grade">StudentSectionGradeResponse</see></returns>
        /// <accessComments>
        /// A user with UPDATE.GRADES permission or assigned faculty on a section can update students grades for the given section.
        /// </accessComments>
        [Obsolete("Obsolete as of Api version 1.33, use version 5")]
        [HttpPut]
        [HeaderVersionRoute("/sections/{sectionId}/grades", 4, false, Name = "PutCollectionOfStudentGrades4")]
        public async Task<ActionResult<IEnumerable<SectionGradeResponse>>> PutCollectionOfStudentGrades4Async([FromRoute] string sectionId, [FromBody] SectionGrades3 sectionGrades)
        {
            try
            {
                if (ModelState != null && !ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                    if (modelErrors != null && modelErrors.Count() > 0)
                    {
                        var formatExceptions = modelErrors.Where(x => x.Exception is System.FormatException).Select(x => x.Exception as System.FormatException).ToList();

                        if (formatExceptions != null && formatExceptions.Count() > 0)
                        {
                            throw formatExceptions.First();
                        }
                    }
                }

                if (string.IsNullOrEmpty(sectionGrades.SectionId))
                {
                    throw new ArgumentException("SectionId", "Section Id must be provided.");
                }

                // Compare uri value to body value for section Id
                if (!sectionId.Equals(sectionGrades.SectionId))
                {
                    throw new ArgumentException("sectionId", "Section Ids do not match in the request.");
                }

                if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                {
                    throw new ArgumentException("StudentGrades", "At least one student grade must be provided.");
                }

                var returnDto = await _sectionCoordinationService.ImportGrades4Async(sectionGrades);
                return Ok(returnDto);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Puts a collection of student section grades from a standard non-ILP caller.
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <returns><see cref="Dtos.Student.Grade">StudentSectionGradeResponse</see></returns>
        /// <accessComments>
        /// 1. A user with UPDATE.GRADES permission or assigned faculty on a section can update students grades for the given section.
        /// 2. A departmental oversight member assigned to the section with CREATE.SECTION.GRADING can update students grades for the given section.
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/sections/{sectionId}/grades", 5, true, Name = "PutCollectionOfStudentGrades5")]
        public async Task<ActionResult<SectionGradeSectionResponse>> PutCollectionOfStudentGrades5Async([FromRoute] string sectionId, [FromBody] SectionGrades4 sectionGrades)
        {
            try
            {
                if (ModelState != null && !ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                    if (modelErrors != null && modelErrors.Count() > 0)
                    {
                        var formatExceptions = modelErrors.Where(x => x.Exception is System.FormatException).Select(x => x.Exception as System.FormatException).ToList();

                        if (formatExceptions != null && formatExceptions.Count() > 0)
                        {
                            throw formatExceptions.First();
                        }
                    }
                }

                if (string.IsNullOrEmpty(sectionGrades.SectionId))
                {
                    throw new ArgumentException("SectionId", "Section Id must be provided.");
                }

                // Compare uri value to body value for section Id
                if (!sectionId.Equals(sectionGrades.SectionId))
                {
                    throw new ArgumentException("sectionId", "Section Ids do not match in the request.");
                }

                if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                {
                    throw new ArgumentException("StudentGrades", "At least one student grade must be provided.");
                }

                var returnDto = await _sectionCoordinationService.ImportGrades5Async(sectionGrades);
                return returnDto;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while updating student grade information.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Puts a collection of student section grades from an ILP caller.
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <returns><see cref="Dtos.Student.Grade">StudentSectionGradeResponse</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/sections/{sectionId}/grades", 1, false, RouteConstants.EllucianJsonIlpMediaTypeFormat, Name = "PutIlpCollectionOfStudentGrades1")]
        public async Task<ActionResult<IEnumerable<SectionGradeResponse>>> PutIlpCollectionOfStudentGrades1Async([FromRoute] string sectionId, [FromBody] SectionGrades3 sectionGrades)
        {
            try
            {
                if (ModelState != null && !ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                    if (modelErrors != null && modelErrors.Count() > 0)
                    {
                        var formatExceptions = modelErrors.Where(x => x.Exception is System.FormatException).Select(x => x.Exception as System.FormatException).ToList();

                        if (formatExceptions != null && formatExceptions.Count() > 0)
                        {
                            throw formatExceptions.First();
                        }
                    }
                }

                if (string.IsNullOrEmpty(sectionGrades.SectionId))
                {
                    throw new ArgumentException("SectionId", "Section Id must be provided.");
                }

                // Compare uri value to body value for section Id
                if (!sectionId.Equals(sectionGrades.SectionId))
                {
                    throw new ArgumentException("sectionId", "Section Ids do not match in the request.");
                }

                if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                {
                    throw new ArgumentException("StudentGrades", "At least one student grade must be provided.");
                }

                var returnDto = await _sectionCoordinationService.ImportIlpGrades1Async(sectionGrades);
                return Ok(returnDto);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query by post method used to get the section registration date overrides for any of the specified section Ids based on the registration group of the person making the request. 
        /// </summary>
        /// <param name="criteria">DTO Object that contains the list of Section ids for which registration dates are requested and the considerUsersgroup boolean variable to decide if the persons registration group should be considered or not</param>
        /// <returns><see cref="SectionRegistrationDate">SectionRegistrationDate</see> DTOs.</returns> 
        /// <accessComments>
        /// 1.Requestor must be assigned faculty member for section.   
        /// 2. A Student is accessing their own data,
        /// 3. An Advisor with any of the following permissions is accessing any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following permissions is accessing one of his or her assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        /// <note>Section and Registration Group information used to calculate the dates are cached for 24 hours.</note>
        [HttpPost]
        [HeaderVersionRoute("/qapi/sections/registration-dates", 1, true, Name = "QuerySectionsRegistrationDates")]
        public async Task<ActionResult<IEnumerable<SectionRegistrationDate>>> QuerySectionRegistrationDatesAsync([FromBody] SectionDateQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentException("SectionDateQueryCriteria", "Section Date Query Criteria cannot be null");
            }
            IEnumerable<string> sectionIds = criteria.SectionIds;
            bool considerUsersGroup = criteria.ConsiderUsersGroup;

            if (sectionIds == null || sectionIds.Count() == 0)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _registrationGroupService.GetSectionRegistrationDatesAsync(sectionIds, considerUsersGroup));

            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving section's registration dates";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Exception occurred while retrieving section's registration dates";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Get all section meeting instances for a specific section id
        /// </summary>
        /// <param name="sectionId">Id of Section. (Required)</param>
        /// <returns>The requested section <see cref="SectionMeetingInstance">meeting instances</see></returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>NotFound.</exception>
        [ParameterSubstitutionFilter]
        [HttpGet]
        //"ExternalEducation", action = "GetExternalEducations2Async", isEedmSupported = true }-meeting-instances", Name = "GetSectionMeetingInstancesAsync")]
        //"ExternalEducation", action = "GetExternalEducations2Async", isEedmSupported = true }
        [HeaderVersionRoute("/sections/{sectionId}/section-meeting-instances", 1, true, Name = "GetSectionMeetingInstancesAsync")]
        public async Task<ActionResult<IEnumerable<SectionMeetingInstance>>> GetSectionMeetingInstancesAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                string errText = "Section ID must be provided to get section meeting instances.";
                _logger.LogError(errText);
                return CreateHttpResponseException(errText, HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await _sectionCoordinationService.GetSectionMeetingInstancesAsync(sectionId));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving student attendances.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving student attendances.");
                return CreateNotFoundException("section events", sectionId);
            }
        }

        /// <summary>
        /// Query by post method to retrieve section events or section calendar schedules in ICal format.
        /// For unscheduled cross-listed sections, it will retrieve calendar schedules for associated primary section if parameter on CPWP/SXRF allows to do so.
        /// </summary>
        /// <param name="criteria">DTO Object that contains list of sectionIds and date range to query section calendar schedules</param>
        /// <returns><see cref="EventsICal"> EventsICal</see> DTO</returns>
        /// <accessComments>Any authenticated user can retrieve sections calendar schedules in iCal format.</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/sections/section-events-ical", 1, true, Name = "QuerySectionEventsICal")]
        public async Task<ActionResult<EventsICal>> QuerySectionEventsICalAsync([FromBody] SectionEventsICalQueryCriteria criteria)
        {
            if (criteria == null || criteria.SectionIds == null || !criteria.SectionIds.Any())
            {
                string errorText = "Criteria must be provided and at least one item in list of SectionIds must be provided.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                var result = await _sectionCoordinationService.GetSectionEventsICalAsync(criteria.SectionIds, criteria.StartDate, criteria.EndDate);
                return result;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving section events calendar";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failure to retrieve section events Ical");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Performs a search of sections in Colleague that are available for registration. 
        /// The criteria supplies a keyword, course Ids, section Id and various filters which may be used to search and narrow a list of sections.
        ///     If keyword is null or empty and there are no course Ids or section Ids, then no sections will be returned.
        /// </summary> 
        /// <param name="criteria"><see cref="SectionSearchCriteria">Section search criteria</see></param>
        /// <param name="pageSize">integer page size</param>
        /// <param name="pageIndex">integer page index</param>
        /// <returns>A <see cref="SectionPage">page</see> of sections matching criteria with totals and filter information.</returns>
        /// <accessComments>Section search can be accessed by any authenticated user or guest user.</accessComments>
        /// <note>Section is cached for 24 hours. New sections and sections with a status change are updated to the cache every 10 minutes.</note>
        [Obsolete("Obsolete as of API version 1.32. Use the latest version of this method.")]
        [HttpPost]
        [HeaderVersionRoute("/sections/search", 1, false, Name = "SectionsSearch")]
        public async Task<ActionResult<SectionPage>> PostSectionSearchAsync([FromBody] SectionSearchCriteria criteria, int pageSize, int pageIndex)
        {
            criteria.Keyword = criteria.Keyword != null ? criteria.Keyword.Replace("_~", "/") : null;

            try
            {
                SectionPage sectionPage = await _courseService.SectionSearchAsync(criteria, pageSize, pageIndex);
                return sectionPage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString() + ex.StackTrace);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Performs a search of sections in Colleague that are available for registration. 
        /// The criteria supplies a keyword, course Ids, section Id and various filters which may be used to search and narrow a list of sections.
        ///     If keyword is null or empty and there are no course Ids or section Ids, then no sections will be returned.
        /// </summary> 
        /// <param name="criteria"><see cref="SectionSearchCriteria">Section search criteria</see></param>
        /// <param name="pageSize">integer page size</param>
        /// <param name="pageIndex">integer page index</param>
        /// <returns>A <see cref="SectionPage2">page</see> of sections matching criteria with totals and filter information.</returns>
        /// <accessComments>Section search can be accessed by any authenticated user or guest user.</accessComments>
        /// <note>Section is cached for 24 hours. New sections and sections with a status change are updated to the cache every 10 minutes.</note>
        [HttpPost]
        [HeaderVersionRoute("/sections/search", 2, true, Name = "SectionsSearch2")]
        public async Task<ActionResult<SectionPage2>> PostSectionSearch2Async([FromBody] SectionSearchCriteria2 criteria, int pageSize, int pageIndex)
        {
            criteria.Keyword = criteria.Keyword != null ? criteria.Keyword.Replace("_~", "/") : null;

            try
            {
                SectionPage2 sectionPage = await _courseService.SectionSearch2Async(criteria, pageSize, pageIndex);
                return sectionPage;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while searching of sections in Colleague that are available for registration";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString() + ex.StackTrace);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Certify Census Date for the given section.
        /// </summary>
        /// <param name="sectionId">Section Id of the section for which census needs to be certified</param>
        /// <param name="sectionCensusToCertify"><see cref="SectionCensusToCertify"></see>Census information to certify</param>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="SectionCensusCertification">section's census certification</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have appropriate permission</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if the census is already certified</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.KeyNotFound returned if section si not found or census date for the section does not exist.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if there are other creation problem.</exception>
        /// <accessComments>
        /// 1. You must be an assigned faculty for the course section. 
        /// 2. A departmental oversight member assigned to the section may certify census with any of the following permission codes
        /// CREATE.SECTION.CENSUS
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/sections/{sectionId}/certify-census", 1, true, Name = "PostSectionCensusCertification")]
        public async Task<ActionResult<SectionCensusCertification>> PostSectionCensusCertificationAsync([FromRoute] string sectionId, [FromBody] SectionCensusToCertify sectionCensusToCertify)
        {
            SectionCensusCertification updatedSectionCertCertification = null;
            string sectionIdForRequest = sectionId;
            try
            {
                if (sectionIdForRequest == null)
                {
                    throw new ArgumentNullException("sectionId", "Section Id must be provided to update its census certs");
                }
                if (sectionCensusToCertify == null)
                {
                    throw new ArgumentNullException("sectionCensusToCertify", "Section Census Certification  details for the census must be provided");
                }
                if (sectionCensusToCertify.CensusCertificationDate == null)
                {
                    throw new ArgumentNullException("CensusCertificationDate", "Section Census date must be provided");
                }
                if (string.IsNullOrWhiteSpace(sectionCensusToCertify.CensusCertificationPosition))
                {
                    throw new ArgumentNullException("CensusCertificationPosition", "Section Census position must be provided");
                }
                if (sectionCensusToCertify.CensusCertificationRecordedDate == null)
                {
                    throw new ArgumentNullException("CensusCertificationRecordedDate", "Section Census recorded date must be provided");
                }
                if (sectionCensusToCertify.CensusCertificationRecordedTime == null)
                {
                    throw new ArgumentNullException("CensusCertificationRecordedTime", "Section Census recorded time must be provided");
                }

                //retrieve section's census dates is valid census date for the section- from ACTM/TLOC/SRGD
                SectionRegistrationDate sectionRegistrationDates = (await _registrationGroupService.GetSectionRegistrationDatesAsync(new List<string>() { sectionIdForRequest }, false)).FirstOrDefault();

                updatedSectionCertCertification = await _sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionIdForRequest, sectionCensusToCertify, sectionRegistrationDates);
                return Created(Url.Link("GetSection4", new { sectionId = sectionIdForRequest }), updatedSectionCertCertification);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while adding census certification details for the section Id " + sectionId;
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);//when user is not one of the assigned faculty of the section
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.NotFound);//when section or census date is not found
            }
            catch (ExistingResourceException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Conflict);//when census date is already certified
            }
            catch (Exception exception)
            {
                string message = "An exception occurred while adding a new census certification details for the given section Id " + sectionId;
                _logger.LogError(exception, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieve a collection of course section seat counts for the given section ids and any of their cross-listed sections
        /// </summary>
        /// <param name="sectionIds">Unique identifiers for the course sections in which to retrieve seat counts</param>
        /// <returns>Collection of <see cref="SectionSeats"/></returns>
        /// <accessComments>Any authenticated user can retrieve course sections seat count information.</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/sections-seats", 1, true, Name = "GetSectionsSeats")]
        public async Task<ActionResult<IEnumerable<SectionSeats>>> QuerySectionsSeatsAsync([FromBody] IEnumerable<string> sectionIds)
        {
            if (sectionIds == null || !sectionIds.Any())
            {
                return CreateHttpResponseException("At least one course section ID is required when retrieving seat counts for sections.");
            }
            try
            {
                var sectionsSeatsDtos = await _sectionCoordinationService.GetSectionsSeatsAsync(sectionIds);
                return Ok(sectionsSeatsDtos);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = "Information for one or more section seat counts could not be retrieved.";
                _logger.LogError(knfe, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while trying to retrieve seat counts";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                var message = string.Format("An error occurred while trying to retrieve seat counts for sections for IDs {0}.", string.Join(",", sectionIds));
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message);
            }
        }
    }
}

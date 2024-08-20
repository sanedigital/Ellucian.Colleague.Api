// Copyright 2021-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Student.AnonymousGrading;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Controller for actions related to course section preliminary anonymous grading
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class PreliminaryAnonymousGradesController : BaseCompressedApiController
    {
        private readonly IPreliminaryAnonymousGradeService _preliminaryAnonymousGradeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AcademicCatalogController class.
        /// </summary>
        /// <param name="preliminaryAnonymousGradeService">Interface to preliminary anonymous grade service</param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PreliminaryAnonymousGradesController(IPreliminaryAnonymousGradeService preliminaryAnonymousGradeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _preliminaryAnonymousGradeService = preliminaryAnonymousGradeService;
            this._logger = logger;
        }

        /// <summary>
        /// Get preliminary anonymous grade information for the specified course section
        /// </summary>
        /// <param name="sectionId">ID of the course section for which to retrieve preliminary anonymous grade information</param>
        /// <returns>Preliminary anonymous grade information for the specified course section</returns>
        /// <accessComments>
        /// 1. The authenticated user must be an assigned faculty member for the specified course section to retrieve preliminary anonymous grade information for that course section.
        /// 2. A departmental oversight member assigned to the section may retrieve preliminary anonymous grade information with the following permission code
        /// VIEW.SECTION.GRADING
        /// CREATE.SECTION.GRADING
        /// </accessComments>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if a course section is not specified, or if there was a Colleage data or configuration error.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not authorized to retrieve preliminary anonymous grade information for the specified course section.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if data for a course section could not be retrieved.</exception>
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}/preliminary-anonymous-grades", 1, true, Name = "GetPreliminaryAnonymousGradesBySectionIdAsync")]
        public async Task<ActionResult<SectionPreliminaryAnonymousGrading>> GetPreliminaryAnonymousGradesBySectionIdAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                string sectionIdRequiredMessage = "A course section ID is required when retrieving preliminary anonymous grade information.";
                _logger.LogError(sectionIdRequiredMessage);
                return CreateHttpResponseException(sectionIdRequiredMessage, HttpStatusCode.BadRequest);
            }
            try
            {
                var sectionPreliminaryAnonymousGrading = await _preliminaryAnonymousGradeService.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
                return sectionPreliminaryAnonymousGrading;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving preliminary anonymous grade information.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string unauthorizedMessage = string.Format("Authenticated user is not an authorized to retrieve preliminary anonymous grade information for course section {0}.", sectionId);
                _logger.LogError(pex, unauthorizedMessage);
                return CreateHttpResponseException(unauthorizedMessage, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                string notFoundMessage = string.Format("Could not retrieve information for course section {0}. Preliminary anonymous grade information cannot be retrieved.", sectionId);
                _logger.LogError(knfex, notFoundMessage);
                return CreateHttpResponseException(notFoundMessage, HttpStatusCode.NotFound);
            }
            catch (ConfigurationException confe)
            {
                string configurationMessage = string.Format("A configuration error was encountered while retrieving preliminary anonymous grade information for course section {0}.", sectionId);
                _logger.LogError(confe, configurationMessage);
                return CreateHttpResponseException(configurationMessage, HttpStatusCode.BadRequest);
            }
            catch (ColleagueException ce)
            {
                string dataErrorMessage = string.Format("A data error was encountered while retrieving preliminary anonymous grade information for course section {0}.", sectionId);
                _logger.LogError(ce, dataErrorMessage);
                return CreateHttpResponseException(dataErrorMessage, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                string genericErrorMessage = string.Format("An error was encountered while retrieving preliminary anonymous grade information for course section {0}.", sectionId);
                _logger.LogError(ex, genericErrorMessage);
                return CreateHttpResponseException(genericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update preliminary anonymous grade information for the specified course section
        /// </summary>
        /// <param name="sectionId">ID of the course section for which to process preliminary anonymous grade updates</param>
        /// <param name="preliminaryAnonymousGrades">Preliminary anonymous grade updates to process</param>
        /// <returns>Preliminary anonymous grade update results</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if a course section is not specified, if no grades are specified, or if there was a Colleage data or configuration error.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not authorized to update preliminary anonymous grade information for the specified course section.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if data for a course section could not be retrieved.</exception>
        [HttpPut]
        [HeaderVersionRoute("/sections/{sectionId}/preliminary-anonymous-grades", 1, true, Name = "UpdatePreliminaryAnonymousGradesBySectionIdAsync")]
        public async Task<ActionResult<IEnumerable<PreliminaryAnonymousGradeUpdateResult>>> UpdatePreliminaryAnonymousGradesBySectionIdAsync([FromRoute] string sectionId,
            [FromBody] IEnumerable<PreliminaryAnonymousGrade> preliminaryAnonymousGrades)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                string sectionIdRequiredMessage = "A course section ID is required when updating preliminary anonymous grade information.";
                _logger.LogError(sectionIdRequiredMessage);
                return CreateHttpResponseException(sectionIdRequiredMessage, HttpStatusCode.BadRequest);
            }
            if (preliminaryAnonymousGrades == null || !preliminaryAnonymousGrades.Any())
            {
                string gradesRequiredMessage = "At least one grade is required when updating preliminary anonymous grade information.";
                _logger.LogError(gradesRequiredMessage);
                return CreateHttpResponseException(gradesRequiredMessage, HttpStatusCode.BadRequest);
            }
            try
            {
                var updatedPreliminaryAnonymousGrades = await _preliminaryAnonymousGradeService.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
                return Ok(updatedPreliminaryAnonymousGrades);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while updating preliminary anonymous grade information for course section " + sectionId;
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string unauthorizedMessage = string.Format("Authenticated user is not an authorized to update preliminary anonymous grade information for course section {0}.", sectionId);
                _logger.LogError(pex, unauthorizedMessage);
                return CreateHttpResponseException(unauthorizedMessage, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                string notFoundMessage = string.Format("Could not retrieve information for course section {0}. Preliminary anonymous grade information cannot be updated.", sectionId);
                _logger.LogError(knfex, notFoundMessage);
                return CreateHttpResponseException(notFoundMessage, HttpStatusCode.NotFound);
            }
            catch (ConfigurationException confe)
            {
                string configurationMessage = string.Format("A configuration error was encountered while updating preliminary anonymous grade information for course section {0}.", sectionId);
                _logger.LogError(confe, configurationMessage);
                return CreateHttpResponseException(configurationMessage, HttpStatusCode.BadRequest);
            }
            catch (ColleagueException ce)
            {
                string dataErrorMessage = string.Format("A data error was encountered while updating preliminary anonymous grade information for course section {0}.", sectionId);
                _logger.LogError(ce, dataErrorMessage);
                return CreateHttpResponseException(dataErrorMessage, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                string genericErrorMessage = string.Format("An error was encountered while updating preliminary anonymous grade information for course section {0}.", sectionId);
                _logger.LogError(ex, genericErrorMessage);
                return CreateHttpResponseException(genericErrorMessage, HttpStatusCode.BadRequest);
            }
        }
    }
}

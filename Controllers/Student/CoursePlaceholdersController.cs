// Copyright 2021-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
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
    /// Controller for course placeholder data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CoursePlaceholdersController : BaseCompressedApiController
    {
        private readonly ICoursePlaceholderService _coursePlaceholderService;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="CoursePlaceholdersController"/> class.
        /// </summary>
        /// <param name="coursePlaceholderService">Interface to course placeholder coordination service</param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CoursePlaceholdersController(ICoursePlaceholderService coursePlaceholderService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _coursePlaceholderService = coursePlaceholderService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieve a collection of course placeholders by ID
        /// </summary>
        /// <param name="coursePlaceholderIds">Unique identifiers for course placeholders to retrieve</param>
        /// <returns>Collection of <see cref="CoursePlaceholder"/></returns>
        /// <accessComments>Any authenticated user can retrieve course placeholder information.</accessComments>
        /// <note>Course placeholder information is cached for 24 hours.</note>
        [HttpPost]
        [HeaderVersionRoute("/qapi/course-placeholders", 1, true, Name = "QueryCoursePlaceholdersByPost")]
        public async Task<ActionResult<IEnumerable<CoursePlaceholder>>> QueryCoursePlaceholdersByIdsAsync([FromBody] IEnumerable<string> coursePlaceholderIds)
        {
            if (coursePlaceholderIds == null || !coursePlaceholderIds.Any())
            {
                return CreateHttpResponseException("At least one course placeholder ID is required when retrieving course placeholders by ID.");
            }
            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                var coursePlaceholderDtos = await _coursePlaceholderService.GetCoursePlaceholdersByIdsAsync(coursePlaceholderIds, bypassCache);
                return Ok(coursePlaceholderDtos);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while querying placeholder";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (KeyNotFoundException knfe)
            {
                var message = "Information for one or more course placeholders could not be retrieved.";
                _logger.LogError(knfe, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                var message = string.Format("An error occurred while trying to retrieve course placeholder data for IDs {0}.", string.Join(",", coursePlaceholderIds));
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message);
            }
        }
    }
}

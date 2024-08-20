// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student.Portal;
using Ellucian.Web.Security;
using System.Net;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Portal Controller is introduced to replace Portal Web Part of WebAdvisor. 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Route("/[controller]/[action]")]
    public class PortalController : BaseCompressedApiController
    {
        private readonly IPortalService _portalService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PortalWebController class.
        /// </summary>
        /// <param name="service">Service of type <see cref="IPortalService">IPortalService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PortalController(IPortalService service, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _portalService = service;
            this._logger = logger;
        }

        /// <summary>
        /// This returns the total courses and the list of course ids  that are applicable for deletion from Portal.
        /// </summary>
        /// <accessComments>Any authenticated user with PORTAL.CATALOG.ADMIN permissions is allowed to get the list of courses that are applicable for deletion from Portal. </accessComments>
        public async Task<ActionResult<PortalDeletedCoursesResult>> GetCoursesForDeletionAsync()
        {
            try
            {
                PortalDeletedCoursesResult portalDeletedCoursesDto = await _portalService.GetCoursesForDeletionAsync();
                return portalDeletedCoursesDto;
            }
            catch (PermissionsException pex)
            {
                string error = "The current user  does not have appropriate permissions to access deleted courses for Portal";
                _logger.LogError(pex, error);
                return CreateHttpResponseException("The user does not have appropriate permissions to access deleted courses for Portal", HttpStatusCode.Forbidden);
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, "Repository level exception occured while retrieving deleted courses for Portal");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving deleted courses for Portal";
                _logger.LogError(e, error);
                return CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This returns the list of sections that are applicable for updating from Portal.
        /// </summary>
        /// <accessComments>Any authenticated user with PORTAL.CATALOG.ADMIN permissions is allowed to get the list of sections that are applicable for updated from Portal.</accessComments>
        public async Task<ActionResult<PortalUpdatedSectionsResult>> GetSectionsForUpdateAsync()
        {
            try
            {
                var portalUpdatedSectionsResultDto = await _portalService.GetSectionsForUpdateAsync();
                return portalUpdatedSectionsResultDto;
            }
            catch (PermissionsException pex)
            {
                string error = "The current user does not have appropriate permissions to access updated sections for Portal";
                _logger.LogError(pex, error);
                return CreateHttpResponseException("The user does not have appropriate permissions to access updated sections for Portal", HttpStatusCode.Forbidden);
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, "Repository level exception occured while retrieving updated sections for Portal");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving updated sections for Portal";
                _logger.LogError(e, error);
                return CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This returns the total sections and the list of section ids that are applicable for deletion from Portal.
        /// </summary>
        /// <accessComments>Any authenticated user with PORTAL.CATALOG.ADMIN permissions is allowed to get the list of sections that are applicable for deletion from Portal.</accessComments>
        public async Task<ActionResult<PortalDeletedSectionsResult>> GetSectionsForDeletionAsync()
        {
            try
            {
                PortalDeletedSectionsResult portalDeletedSectionsResultDto = await _portalService.GetSectionsForDeletionAsync();
                return portalDeletedSectionsResultDto;
            }
            catch (PermissionsException pex)
            {
                string error = "The current user  does not have appropriate permissions to access deleted sections for Portal";
                _logger.LogError(pex, error);
                return CreateHttpResponseException("The user does not have appropriate permissions to access deleted course sections for Portal", HttpStatusCode.Forbidden);
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, "Repository level exception occured while retrieving deleted sections for Portal");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving deleted sections for Portal";
                _logger.LogError(e, error);
                return CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns event and reminders to be displayed in the Portal for the authenticated user.
        /// </summary>
        /// <param name="criteria">Event and reminder selection criteria</param>
        /// <accessComments>Any authenticated user may retrieve events and reminders to be displayed in the Portal for themselves</accessComments>
        public async Task<ActionResult<Dtos.Student.Portal.PortalEventsAndReminders>> QueryEventsAndRemindersAsync([FromBody]PortalEventsAndRemindersQueryCriteria criteria)
        {
            try
            {
                PortalEventsAndReminders portalEventsAndRemindersDto = await _portalService.GetEventsAndRemindersAsync(criteria);
                return portalEventsAndRemindersDto;
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, "Repository level exception occured while retrieving Portal events and reminders.");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving Portal events and reminders.";
                _logger.LogError(e, error);
                return CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }

        }


        /// <summary>
        /// This returns the courses that are applicable for updating from Portal.
        /// </summary>
        /// <accessComments>Any authenticated user with PORTAL.CATALOG.ADMIN permissions is allowed to get the list of courses that are applicable for updated from Portal.</accessComments>
        public async Task<ActionResult<PortalUpdatedCoursesResult>> GetCoursesForUpdateAsync()
        {
            try
            {
                var portalUpdatedCoursesResultDto = await _portalService.GetCoursesForUpdateAsync();
                return portalUpdatedCoursesResultDto;
            }
            catch (PermissionsException pex)
            {
                string error = "The current user does not have appropriate permissions to access updated courses for Portal";
                _logger.LogError(pex, error);
                return CreateHttpResponseException("The user does not have appropriate permissions to access updated courses for Portal", HttpStatusCode.Forbidden);
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, "Repository level exception occured while retrieving updated courses for Portal");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving updated courses for Portal";
                _logger.LogError(e, error);
                return CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates a student's list of preferred course sections
        /// </summary>
        /// <param name="studentId">ID of the student whose list of preferred course sections is being updated</param>
        /// <param name="courseSectionIds">IDs of the course sections to be added to the student's list of preferred course sections</param>
        /// <returns>Collection of <see cref="PortalStudentPreferredCourseSectionUpdateResult"/></returns>
        /// <accessComments>Authenticated users may update their own preferred course sections.</accessComments>
        public async Task<ActionResult<IEnumerable<PortalStudentPreferredCourseSectionUpdateResult>>> UpdateStudentPreferredCourseSectionsAsync([FromQuery] string studentId, [FromBody]IEnumerable<string> courseSectionIds)
        {
            try
            {
                var result = await _portalService.UpdateStudentPreferredCourseSectionsAsync(studentId, courseSectionIds);
                return Ok(result);
            }
            catch (PermissionsException pex)
            {
                string error = "The current user does not have appropriate permissions to access updated courses for Portal";
                _logger.LogError(pex, error);
                return CreateHttpResponseException("The user does not have appropriate permissions to access updated courses for Portal", HttpStatusCode.Forbidden);
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, "Repository level exception occured while retrieving updated courses for Portal");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving updated courses for Portal";
                _logger.LogError(e, error);
                return CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }
        }
    }
}

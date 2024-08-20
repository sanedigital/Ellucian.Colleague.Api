// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Security;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Preferred Section data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class PreferredSectionsController : BaseCompressedApiController
    {
        private readonly IPreferredSectionService _preferredSectionService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentPreferredSectionsRepository class.
        /// </summary>
        /// <param name="preferredSectionService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PreferredSectionsController(IPreferredSectionService preferredSectionService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _preferredSectionService = preferredSectionService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves the student's list of preferred sections.
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        /// <returns>List of student's current Preferred Sections and applicable messages.</returns>
        /// <accessComments>
        /// Student must be requesting their own sections
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/preferred-sections", 1, true, Name = "GetPreferredSections")]
        public async Task<ActionResult<Dtos.Student.PreferredSectionsResponse>> GetPreferredSectionsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Invalid studentId");
                return CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _preferredSectionService.GetAsync(studentId);
            }
            catch (PermissionsException pex)
            {
                _logger.LogInformation(pex.ToString());
                return CreateHttpResponseException(pex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return CreateHttpResponseException(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update (create new and/or update existing) the student's list of preferred sections.
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        /// <param name="preferredSections">List of preferred sections to create and/or update.</param>
        /// <returns>List of student's current Preferred Sections and applicable messages.</returns>
        /// <accessComments>
        /// Student must be updating their own section
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/students/{studentId}/preferred-sections", 1, true, Name = "UpdatePreferredSections")]
        public async Task<ActionResult<IEnumerable<Dtos.Student.PreferredSectionMessage>>> UpdatePreferredSectionsAsync(string studentId, [FromBody] IEnumerable<Dtos.Student.PreferredSection> preferredSections)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Invalid studentId");
                return CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            if (preferredSections == null || preferredSections.Count() <= 0)
            {
                _logger.LogError("Invalid preferredSections");
                return CreateHttpResponseException("Invalid preferredSections. Must provide at least one.", System.Net.HttpStatusCode.BadRequest);
            }
            try
            {
                IEnumerable<Dtos.Student.PreferredSectionMessage> response = await _preferredSectionService.UpdateAsync(studentId, preferredSections);
                return Ok(response);
            }
            catch (PermissionsException pex)
            {
                _logger.LogInformation(pex.ToString());
                return CreateHttpResponseException(pex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return CreateHttpResponseException(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Delete the indicated section from the student's preferred sections list.
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        /// <param name="sectionId">The Section Id to delete.</param>
        /// <returns></returns>
        /// <accessComments>
        /// Student must be deleting their own section
        /// </accessComments>
        [HttpDelete]
        [HeaderVersionRoute("/students/{studentId}/preferred-sections/{sectionId}", 1, true, Name = "DeletePreferredSections")]
        public async Task<ActionResult<IEnumerable<Dtos.Student.PreferredSectionMessage>>> DeletePreferredSectionsAsync(string studentId, string sectionId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Invalid studentId");
                return CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(sectionId))
            {
                _logger.LogError("Invalid sectionId");
                return CreateHttpResponseException("Invalid sectionId", HttpStatusCode.BadRequest);
            }
            try
            {
                IEnumerable<Dtos.Student.PreferredSectionMessage> response = await _preferredSectionService.DeleteAsync(studentId, sectionId);
                return Ok(response);
            }
            catch (PermissionsException pex)
            {
                _logger.LogInformation(pex.ToString());
                return CreateHttpResponseException(pex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return CreateHttpResponseException(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }


/*        /// <summary>
        /// Deletes the indicated sections from the student's preferred sections list.
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        /// <param name="sectionIds">List of Preferred Section IDs to delete.</param>
        /// <returns></returns>
        [HttpPost]
        public IEnumerable<Dtos.Student.PreferredSectionMessage> DeletePreferredSections(string studentId, [FromBody]List<string> sectionIds)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Invalid studentId");
                return CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            if (sectionIds == null || sectionIds.Count() == 0)
            {
                _logger.LogError("Invalid sectionIds");
                return CreateHttpResponseException("Invalid sectionIds. Must provide at least one.", System.Net.HttpStatusCode.BadRequest);
            }
            try
            {
                IEnumerable<Dtos.Student.PreferredSectionMessage> response = _preferredSectionService.Delete(studentId, sectionIds);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.LogInformation(pex.ToString());
                return CreateHttpResponseException(pex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return CreateHttpResponseException(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }
*/
    }
}

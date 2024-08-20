// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
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
    /// Provide access to faculty Consent and student petition data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionPermissionsController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly ISectionPermissionService _service;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionPermissionsController(ISectionPermissionService service, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _service = service;
            this._logger = logger;
        }
        /// <summary>
        /// Returns the student petitions and faculty consents found for the specified section. Requestor must be an instructor of the section to access the information.
        /// </summary>
        /// <param name="sectionId">The section Id to use to retrieve student petitions and faculty consents.</param>
        /// <returns>The <see cref="Dtos.Student.SectionPermission">SectionPermission</see> object</returns>
        /// <accessComments>
        /// 1. Only the faculty of a section can retrieve the permissions for the section.
        /// 2. A departmental oversight member assigned to the section can retrieve the permissions with any of the following permission code
        /// VIEW.SECTION.STUDENT.PETITIONS
        /// CREATE.SECTION.STUDENT.PETITION
        /// VIEW.SECTION.FACULTY.CONSENTS
        /// CREATE.SECTION.FACULTY.CONSENT
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/sections/{sectionId}/section-permission", 1, true, Name = "GetSectionPermission")]
        public async Task<ActionResult<SectionPermission>> GetSectionPermissionAsync(string sectionId)
        {
            try
            {
                var sectionPermission = await _service.GetAsync(sectionId);
                return sectionPermission;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                string message = "Session has expired while retrieving list of faculty office hours";
                _logger.LogError(csee, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = "Access to Section Permission is forbidden.";
                _logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = "Invalid section specified.";
                _logger.LogError(knfe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                var message = "Error occurred retrieving permissions for section.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.BadRequest);
            }
        }
    }

}

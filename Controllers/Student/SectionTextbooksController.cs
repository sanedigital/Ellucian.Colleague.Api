// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
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
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to course Section data for textbooks
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionTextbooksController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly ISectionCoordinationService _sectionCoordinationService;

        /// <summary>
        /// Initializes a new instance of the SectionsController class.
        /// </summary>
        /// <param name="sectionCoordinationService">Service of type <see cref="ISectionCoordinationService">ISectionCoordinationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionTextbooksController(
            ISectionCoordinationService sectionCoordinationService,
            ILogger logger,
            IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _sectionCoordinationService = sectionCoordinationService;
            this._logger = logger;
        }

        /// <summary>
        /// Update a book assignment for a section.
        /// </summary>
        /// <param name="textbook">The textbook whose assignment to a specific section is being updated.</param>
        /// <returns>An updated <see cref="Section3"/> object.</returns>
        /// <accessComments>
        /// 1. Only an assigned faculty user may update book assignments for a course section.
        /// 2. A departmental oversight member assigned to the section may update book assignments for a course section with the following permission code
        /// CREATE.SECTION.BOOKS
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/section-textbooks", 1, true, Name = "PutSectionBook")]
        public async Task<ActionResult<Section3>> UpdateSectionBookAsync(SectionTextbook textbook)
        {
            try
            {
                return await _sectionCoordinationService.UpdateSectionBookAsync(textbook);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while updating section textbook information.";
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException e)
            {
                var message = "User does not have appropriate permissions to update section textbook information.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = "An error occurred while updating section textbook information.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}

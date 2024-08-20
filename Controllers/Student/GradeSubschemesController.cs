// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Grade Subscheme data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class GradeSubschemesController : BaseCompressedApiController
    {
        private readonly IGradeSchemeService _gradeSchemeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GradeSubschemesController class.
        /// </summary>
        /// <param name="gradeSchemeService">Service of type <see cref="IGradeSchemeService">IGradeSchemeService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GradeSubschemesController(IGradeSchemeService gradeSchemeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _gradeSchemeService = gradeSchemeService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves a Grade Subscheme by ID
        /// </summary>
        /// <param name="id">ID of the Grade Subscheme</param>
        /// <returns>A Grade Subscheme</returns>
        /// <accessComments>Any authenticated user can retrieve Grade Subscheme information.</accessComments>
        /// <note>Grade subscheme information is cached for 24 hours.</note>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/grade-subschemes/{id}", 1, true, Name = "GetGradeSubschemeByIdAsync")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Student.GradeSubscheme>> GetGradeSubschemeByIdAsync([FromRoute]string id)
        {
            try
            {
                return await _gradeSchemeService.GetGradeSubschemeByIdAsync(id);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving grade subscheme information.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, string.Format("Could not retrieve a Grade Subscheme with ID {0}.", id));
                return CreateHttpResponseException(string.Format("Could not retrieve a Grade Subscheme with ID {0}.", id), System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }
    }
}

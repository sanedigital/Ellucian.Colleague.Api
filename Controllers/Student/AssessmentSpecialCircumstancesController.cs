// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using System.Threading.Tasks;
using System;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AssessmentSpecialCircumstances data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AssessmentSpecialCircumstancesController : BaseCompressedApiController
    {
        private readonly ICurriculumService _curriculumService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AssessmentSpecialCircumstancesController class.
        /// </summary>
        /// <param name="curriculumService">Repository of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AssessmentSpecialCircumstancesController(ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _curriculumService = curriculumService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all assessment special circumstances.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All AssessmentSpecialCircumstance objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/assessment-special-circumstances", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAssessmentSpecialCircumstances", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AssessmentSpecialCircumstance>>> GetAssessmentSpecialCircumstancesAsync()
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                return Ok(await _curriculumService.GetAssessmentSpecialCircumstancesAsync(bypassCache));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Retrieves an Assessment special circumstances by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.AssessmentSpecialCircumstance">AssessmentSpecialCircumstance</see>object.</returns>
        [HttpGet]
        [HeaderVersionRoute("/assessment-special-circumstances/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAssessmentSpecialCircumstanceById", IsEedmSupported = true)]
        public async Task<ActionResult<AssessmentSpecialCircumstance>> GetAssessmentSpecialCircumstanceByIdAsync(string id)
        {
            try
            {
                return await _curriculumService.GetAssessmentSpecialCircumstanceByGuidAsync(id);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Creates a Assessment Special Circumstance.
        /// </summary>
        /// <param name="assessmentSpecialCircumstance"><see cref="Dtos.AssessmentSpecialCircumstance">AssessmentSpecialCircumstance</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AssessmentSpecialCircumstance">AssessmentSpecialCircumstance</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/assessment-special-circumstances", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAssessmentSpecialCircumstancesV6")]
        public async Task<ActionResult<Dtos.AssessmentSpecialCircumstance>> PostAssessmentSpecialCircumstanceAsync([FromBody] Dtos.AssessmentSpecialCircumstance assessmentSpecialCircumstance)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Assessment Special Circumstance.
        /// </summary>
        /// <param name="id">Id of the Assessment Special Circumstance to update</param>
        /// <param name="assessmentSpecialCircumstance"><see cref="Dtos.AssessmentSpecialCircumstance">AssessmentSpecialCircumstance</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AssessmentSpecialCircumstance">AssessmentSpecialCircumstance</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/assessment-special-circumstances/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAssessmentSpecialCircumstancesV6")]
        public async Task<ActionResult<Dtos.AssessmentSpecialCircumstance>> PutAssessmentSpecialCircumstanceAsync([FromRoute] string id, [FromBody] Dtos.AssessmentSpecialCircumstance assessmentSpecialCircumstance)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Assessment Special Circumstance.
        /// </summary>
        /// <param name="id">ID of the Assessment Special Circumstance to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/assessment-special-circumstances/{id}", Name = "DeleteAssessmentSpecialCircumstances")]
        public async Task<IActionResult> DeleteAssessmentSpecialCircumstanceAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

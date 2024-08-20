// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AssessmentCalculationMethods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AssessmentCalculationMethodsController : BaseCompressedApiController
    {
        private readonly IAssessmentCalculationMethodsService _assessmentCalculationMethodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AssessmentCalculationMethodsController class.
        /// </summary>
        /// <param name="assessmentCalculationMethodsService">Service of type <see cref="IAssessmentCalculationMethodsService">IAssessmentCalculationMethodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AssessmentCalculationMethodsController(IAssessmentCalculationMethodsService assessmentCalculationMethodsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _assessmentCalculationMethodsService = assessmentCalculationMethodsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all assessmentCalculationMethods
        /// </summary>
        /// <returns>List of AssessmentCalculationMethods <see cref="Dtos.AssessmentCalculationMethods"/> objects representing matching assessmentCalculationMethods</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/assessment-calculation-methods", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAssessmentCalculationMethods", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AssessmentCalculationMethods>>> GetAssessmentCalculationMethodsAsync()
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                return Ok(await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsAsync(bypassCache));
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
        /// Read (GET) a assessmentCalculationMethods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired assessmentCalculationMethods</param>
        /// <returns>A assessmentCalculationMethods object <see cref="Dtos.AssessmentCalculationMethods"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/assessment-calculation-methods/{guid}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAssessmentCalculationMethodsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AssessmentCalculationMethods>> GetAssessmentCalculationMethodsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsByGuidAsync(guid);
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
        /// Create (POST) a new assessmentCalculationMethods
        /// </summary>
        /// <param name="assessmentCalculationMethods">DTO of the new assessmentCalculationMethods</param>
        /// <returns>A assessmentCalculationMethods object <see cref="Dtos.AssessmentCalculationMethods"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/assessment-calculation-methods", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAssessmentCalculationMethodsV6")]
        public async Task<ActionResult<Dtos.AssessmentCalculationMethods>> PostAssessmentCalculationMethodsAsync([FromBody] Dtos.AssessmentCalculationMethods assessmentCalculationMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing assessmentCalculationMethods
        /// </summary>
        /// <param name="guid">GUID of the assessmentCalculationMethods to update</param>
        /// <param name="assessmentCalculationMethods">DTO of the updated assessmentCalculationMethods</param>
        /// <returns>A assessmentCalculationMethods object <see cref="Dtos.AssessmentCalculationMethods"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/assessment-calculation-methods/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAssessmentCalculationMethodsV6")]
        public async Task<ActionResult<Dtos.AssessmentCalculationMethods>> PutAssessmentCalculationMethodsAsync([FromRoute] string guid, [FromBody] Dtos.AssessmentCalculationMethods assessmentCalculationMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a assessmentCalculationMethods
        /// </summary>
        /// <param name="guid">GUID to desired assessmentCalculationMethods</param>
        [HttpDelete]
        [Route("/assessment-calculation-methods/{guid}", Name = "DefaultDeleteAssessmentCalculationMethods")]
        public async Task<IActionResult> DeleteAssessmentCalculationMethodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

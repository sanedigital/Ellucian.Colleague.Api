// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AptitudeAssessments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AptitudeAssessmentsController : BaseCompressedApiController
    {
        private readonly IAptitudeAssessmentsService _aptitudeAssessmentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AptitudeAssessmentsController class.
        /// </summary>
        /// <param name="aptitudeAssessmentsService">Service of type <see cref="IAptitudeAssessmentsService">IAptitudeAssessmentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AptitudeAssessmentsController(IAptitudeAssessmentsService aptitudeAssessmentsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _aptitudeAssessmentsService = aptitudeAssessmentsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all aptitudeAssessments
        /// </summary>
        /// <returns>List of AptitudeAssessments <see cref="Dtos.AptitudeAssessment"/> objects representing matching aptitudeAssessments</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/aptitude-assessments", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAptitudeAssessments", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.AptitudeAssessment>>> GetAptitudeAssessmentsAsync()
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
                var aptitudeDtos =  await _aptitudeAssessmentsService.GetAptitudeAssessmentsAsync(bypassCache);
                AddEthosContextProperties(
                    await _aptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _aptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        aptitudeDtos.Select(i => i.Id).ToList()));
                return Ok(aptitudeDtos);

            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a aptitudeAssessments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired aptitudeAssessments</param>
        /// <returns>A aptitudeAssessments object <see cref="Dtos.AptitudeAssessment"/> in EEDM format</returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/aptitude-assessments/{guid}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAptitudeAssessmentsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AptitudeAssessment>> GetAptitudeAssessmentsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                await _aptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                 await _aptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                new List<string>() { guid }));

                return await _aptitudeAssessmentsService.GetAptitudeAssessmentsByGuidAsync(guid);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new aptitudeAssessment
        /// </summary>
        /// <param name="aptitudeAssessment">DTO of the new aptitudeAssessment</param>
        /// <returns>A aptitudeAssessments object <see cref="Dtos.AptitudeAssessment"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/aptitude-assessments", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAptitudeAssessmentsV6")]
        public async Task<ActionResult<Dtos.AptitudeAssessment>> PostAptitudeAssessmentsAsync([FromBody] Dtos.AptitudeAssessment aptitudeAssessment)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing aptitudeAssessments
        /// </summary>
        /// <param name="guid">GUID of the aptitudeAssessments to update</param>
        /// <param name="aptitudeAssessment">DTO of the updated aptitudeAssessments</param>
        /// <returns>A aptitudeAssessments object <see cref="Dtos.AptitudeAssessments"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/aptitude-assessments/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAptitudeAssessmentsV6")]
        public async Task<ActionResult<Dtos.AptitudeAssessment>> PutAptitudeAssessmentsAsync([FromRoute] string guid, [FromBody] Dtos.AptitudeAssessment aptitudeAssessment)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a aptitudeAssessment
        /// </summary>
        /// <param name="guid">GUID to desired aptitudeAssessment</param>
        [HttpDelete]
        [Route("/aptitude-assessments/{guid}", Name = "DefaultDeleteAptitudeAssessments")]
        public async Task<IActionResult> DeleteAptitudeAssessmentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

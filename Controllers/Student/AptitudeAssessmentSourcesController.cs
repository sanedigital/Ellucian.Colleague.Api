// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
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
    /// Provides access to AptitudeAssessmentSources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AptitudeAssessmentSourcesController : BaseCompressedApiController
    {
        private readonly IAptitudeAssessmentSourcesService _aptitudeAssessmentSourcesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AptitudeAssessmentSourcesController class.
        /// </summary>
        /// <param name="aptitudeAssessmentSourcesService">Service of type <see cref="IAptitudeAssessmentSourcesService">IAptitudeAssessmentSourcesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AptitudeAssessmentSourcesController(IAptitudeAssessmentSourcesService aptitudeAssessmentSourcesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _aptitudeAssessmentSourcesService = aptitudeAssessmentSourcesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all aptitudeAssessmentSources
        /// </summary>
        /// <returns>List of AptitudeAssessmentSources <see cref="Dtos.AptitudeAssessmentSources"/> objects representing matching aptitudeAssessmentSources</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/aptitude-assessment-sources", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAptitudeAssessmentSources", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AptitudeAssessmentSources>>> GetAptitudeAssessmentSourcesAsync()
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
                var aptitudeAssessmentSources = await _aptitudeAssessmentSourcesService.GetAptitudeAssessmentSourcesAsync(bypassCache);

                if (aptitudeAssessmentSources != null && aptitudeAssessmentSources.Any())
                {
                    AddEthosContextProperties(await _aptitudeAssessmentSourcesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _aptitudeAssessmentSourcesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              aptitudeAssessmentSources.Select(a => a.Id).ToList()));
                }
                return Ok(aptitudeAssessmentSources);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a aptitudeAssessmentSources using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired aptitudeAssessmentSources</param>
        /// <returns>A aptitudeAssessmentSources object <see cref="Dtos.AptitudeAssessmentSources"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/aptitude-assessment-sources/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAptitudeAssessmentSourcesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AptitudeAssessmentSources>> GetAptitudeAssessmentSourcesByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                   await _aptitudeAssessmentSourcesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _aptitudeAssessmentSourcesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _aptitudeAssessmentSourcesService.GetAptitudeAssessmentSourcesByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Create (POST) a new aptitudeAssessmentSources
        /// </summary>
        /// <param name="aptitudeAssessmentSources">DTO of the new aptitudeAssessmentSources</param>
        /// <returns>A aptitudeAssessmentSources object <see cref="Dtos.AptitudeAssessmentSources"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/aptitude-assessment-sources", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAptitudeAssessmentSourcesV1.0.0")]
        public async Task<ActionResult<Dtos.AptitudeAssessmentSources>> PostAptitudeAssessmentSourcesAsync([FromBody] Dtos.AptitudeAssessmentSources aptitudeAssessmentSources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing aptitudeAssessmentSources
        /// </summary>
        /// <param name="guid">GUID of the aptitudeAssessmentSources to update</param>
        /// <param name="aptitudeAssessmentSources">DTO of the updated aptitudeAssessmentSources</param>
        /// <returns>A aptitudeAssessmentSources object <see cref="Dtos.AptitudeAssessmentSources"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/aptitude-assessment-sources/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAptitudeAssessmentSourcesV1.0.0")]
        public async Task<ActionResult<Dtos.AptitudeAssessmentSources>> PutAptitudeAssessmentSourcesAsync([FromRoute] string guid, [FromBody] Dtos.AptitudeAssessmentSources aptitudeAssessmentSources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a aptitudeAssessmentSources
        /// </summary>
        /// <param name="guid">GUID to desired aptitudeAssessmentSources</param>
        [HttpDelete]
        [Route("/aptitude-assessment-sources/{guid}", Name = "DefaultDeleteAptitudeAssessmentSources")]
        public async Task<IActionResult> DeleteAptitudeAssessmentSourcesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

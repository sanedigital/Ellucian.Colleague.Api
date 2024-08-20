// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
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


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PositionClassifications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PositionClassificationsController : BaseCompressedApiController
    {
        private readonly IPositionClassificationsService _positionClassificationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PositionClassificationsController class.
        /// </summary>
        /// <param name="positionClassificationsService">Service of type <see cref="IPositionClassificationsService">IPositionClassificationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PositionClassificationsController(IPositionClassificationsService positionClassificationsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _positionClassificationsService = positionClassificationsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all positionClassifications
        /// </summary>
        /// <returns>List of PositionClassifications <see cref="Dtos.PositionClassification"/> objects representing matching positionClassifications</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/position-classifications", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPositionClassifications", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PositionClassification>>> GetPositionClassificationsAsync()
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
                var positionClassifications = await _positionClassificationsService.GetPositionClassificationsAsync(bypassCache);

                if (positionClassifications != null && positionClassifications.Any())
                {
                    AddEthosContextProperties(await _positionClassificationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _positionClassificationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              positionClassifications.Select(a => a.Id).ToList()));
                }
                return Ok(positionClassifications);
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
        /// Read (GET) a positionClassifications using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired positionClassifications</param>
        /// <returns>A positionClassifications object <see cref="Dtos.PositionClassification"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/position-classifications/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPositionClassificationsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PositionClassification>> GetPositionClassificationsByGuidAsync(string guid)
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
                    await _positionClassificationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(),bypassCache),
                    await _positionClassificationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _positionClassificationsService.GetPositionClassificationsByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new positionClassifications
        /// </summary>
        /// <param name="positionClassifications">DTO of the new positionClassifications</param>
        /// <returns>A positionClassifications object <see cref="Dtos.PositionClassification"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/position-classifications", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPositionClassificationsV12")]
        public async Task<ActionResult<Dtos.PositionClassification>> PostPositionClassificationsAsync([FromBody] Dtos.PositionClassification positionClassifications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing positionClassifications
        /// </summary>
        /// <param name="guid">GUID of the positionClassifications to update</param>
        /// <param name="positionClassifications">DTO of the updated positionClassifications</param>
        /// <returns>A positionClassifications object <see cref="Dtos.PositionClassification"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/position-classifications/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPositionClassificationsV12")]
        public async Task<ActionResult<Dtos.PositionClassification>> PutPositionClassificationsAsync([FromRoute] string guid, [FromBody] Dtos.PositionClassification positionClassifications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a positionClassifications
        /// </summary>
        /// <param name="guid">GUID to desired positionClassifications</param>
        [HttpDelete]
        [Route("/position-classifications/{guid}", Name = "DefaultDeletePositionClassifications")]
        public async Task<IActionResult> DeletePositionClassificationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

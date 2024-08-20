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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to ExternalEmploymentPositions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ExternalEmploymentPositionsController : BaseCompressedApiController
    {
        private readonly IExternalEmploymentPositionsService _externalEmploymentPositionsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ExternalEmploymentPositionsController class.
        /// </summary>
        /// <param name="externalEmploymentPositionsService">Service of type <see cref="IExternalEmploymentPositionsService">IExternalEmploymentPositionsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ExternalEmploymentPositionsController(IExternalEmploymentPositionsService externalEmploymentPositionsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _externalEmploymentPositionsService = externalEmploymentPositionsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all externalEmploymentPositions
        /// </summary>
        /// <returns>List of ExternalEmploymentPositions <see cref="Dtos.ExternalEmploymentPositions"/> objects representing matching externalEmploymentPositions</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/external-employment-positions", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetExternalEmploymentPositions", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmploymentPositions>>> GetExternalEmploymentPositionsAsync()
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
                var externalEmploymentPositions = await _externalEmploymentPositionsService.GetExternalEmploymentPositionsAsync(bypassCache);

                if (externalEmploymentPositions != null && externalEmploymentPositions.Any())
                {
                    AddEthosContextProperties(await _externalEmploymentPositionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _externalEmploymentPositionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              externalEmploymentPositions.Select(a => a.Id).ToList()));
                }

                return Ok(externalEmploymentPositions);                
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
        /// Read (GET) a externalEmploymentPositions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired externalEmploymentPositions</param>
        /// <returns>A externalEmploymentPositions object <see cref="Dtos.ExternalEmploymentPositions"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/external-employment-positions/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetExternalEmploymentPositionsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ExternalEmploymentPositions>> GetExternalEmploymentPositionsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _externalEmploymentPositionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _externalEmploymentPositionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _externalEmploymentPositionsService.GetExternalEmploymentPositionsByGuidAsync(guid);
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
        /// Create (POST) a new externalEmploymentPositions
        /// </summary>
        /// <param name="externalEmploymentPositions">DTO of the new externalEmploymentPositions</param>
        /// <returns>A externalEmploymentPositions object <see cref="Dtos.ExternalEmploymentPositions"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/external-employment-positions", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostExternalEmploymentPositionsV10")]
        public async Task<ActionResult<Dtos.ExternalEmploymentPositions>> PostExternalEmploymentPositionsAsync([FromBody] Dtos.ExternalEmploymentPositions externalEmploymentPositions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing externalEmploymentPositions
        /// </summary>
        /// <param name="guid">GUID of the externalEmploymentPositions to update</param>
        /// <param name="externalEmploymentPositions">DTO of the updated externalEmploymentPositions</param>
        /// <returns>A externalEmploymentPositions object <see cref="Dtos.ExternalEmploymentPositions"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/external-employment-positions/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutExternalEmploymentPositionsV10")]
        public async Task<ActionResult<Dtos.ExternalEmploymentPositions>> PutExternalEmploymentPositionsAsync([FromRoute] string guid, [FromBody] Dtos.ExternalEmploymentPositions externalEmploymentPositions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a externalEmploymentPositions
        /// </summary>
        /// <param name="guid">GUID to desired externalEmploymentPositions</param>
        [HttpDelete]
        [Route("/external-employment-positions/{guid}", Name = "DefaultDeleteExternalEmploymentPositions", Order = -10)]
        public async Task<IActionResult> DeleteExternalEmploymentPositionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
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


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to FixedAssetTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class FixedAssetTypesController : BaseCompressedApiController
    {
        private readonly IFixedAssetTypesService _fixedAssetTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FixedAssetTypesController class.
        /// </summary>
        /// <param name="fixedAssetTypesService">Service of type <see cref="IFixedAssetTypesService">IFixedAssetTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FixedAssetTypesController(IFixedAssetTypesService fixedAssetTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _fixedAssetTypesService = fixedAssetTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all fixedAssetTypes
        /// </summary>
        /// <returns>List of FixedAssetTypes <see cref="Dtos.FixedAssetType"/> objects representing matching fixedAssetTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/fixed-asset-types", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFixedAssetTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.FixedAssetType>>> GetFixedAssetTypesAsync()
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
                var items = await _fixedAssetTypesService.GetFixedAssetTypesAsync(bypassCache);

                AddEthosContextProperties(
                 await _fixedAssetTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _fixedAssetTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     items.Select(i => i.Id).Distinct().ToList()));

                return Ok(items);
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
        /// Read (GET) a fixedAssetTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssetTypes</param>
        /// <returns>A fixedAssetTypes object <see cref="Dtos.FixedAssetType"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/fixed-asset-types/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFixedAssetTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FixedAssetType>> GetFixedAssetTypesByGuidAsync(string guid)
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
                   await _fixedAssetTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _fixedAssetTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _fixedAssetTypesService.GetFixedAssetTypesByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new fixedAssetTypes
        /// </summary>
        /// <param name="fixedAssetTypes">DTO of the new fixedAssetTypes</param>
        /// <returns>A fixedAssetTypes object <see cref="Dtos.FixedAssetType"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/fixed-asset-types", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFixedAssetTypesV12")]
        public async Task<ActionResult<Dtos.FixedAssetType>> PostFixedAssetTypesAsync([FromBody] Dtos.FixedAssetType fixedAssetTypes)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing fixedAssetTypes
        /// </summary>
        /// <param name="guid">GUID of the fixedAssetTypes to update</param>
        /// <param name="fixedAssetTypes">DTO of the updated fixedAssetTypes</param>
        /// <returns>A fixedAssetTypes object <see cref="Dtos.FixedAssetType"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/fixed-asset-types/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFixedAssetTypesV12")]
        public async Task<ActionResult<Dtos.FixedAssetType>> PutFixedAssetTypesAsync([FromRoute] string guid, [FromBody] Dtos.FixedAssetType fixedAssetTypes)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a fixedAssetTypes
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssetTypes</param>
        [HttpDelete]
        [Route("/fixed-asset-types/{guid}", Name = "DefaultDeleteFixedAssetTypes", Order = -10)]
        public async Task<IActionResult> DeleteFixedAssetTypesAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }       
    }
}

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
    /// Provides access to FixedAssetCategories
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class FixedAssetCategoriesController : BaseCompressedApiController
    {
        private readonly IFixedAssetCategoriesService _fixedAssetCategoriesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FixedAssetCategoriesController class.
        /// </summary>
        /// <param name="fixedAssetCategoriesService">Service of type <see cref="IFixedAssetCategoriesService">IFixedAssetCategoriesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FixedAssetCategoriesController(IFixedAssetCategoriesService fixedAssetCategoriesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _fixedAssetCategoriesService = fixedAssetCategoriesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all fixedAssetCategories
        /// </summary>
        /// <returns>List of FixedAssetCategories <see cref="Dtos.FixedAssetCategory"/> objects representing matching fixedAssetCategories</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/fixed-asset-categories", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFixedAssetCategories", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.FixedAssetCategory>>> GetFixedAssetCategoriesAsync()
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
                var items = await _fixedAssetCategoriesService.GetFixedAssetCategoriesAsync(bypassCache);

                AddEthosContextProperties(await _fixedAssetCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache), 
                    await _fixedAssetCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), 
                    items.Select(a => a.Id).ToList()));

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
        /// Read (GET) a fixedAssetCategories using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssetCategories</param>
        /// <returns>A fixedAssetCategories object <see cref="Dtos.FixedAssetCategory"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/fixed-asset-categories/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFixedAssetCategoriesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FixedAssetCategory>> GetFixedAssetCategoriesByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _fixedAssetCategoriesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                var item = await _fixedAssetCategoriesService.GetFixedAssetCategoriesByGuidAsync(guid);

                if (item != null)
                {
                    AddEthosContextProperties(await _fixedAssetCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _fixedAssetCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { item.Id }));
                }

                return item;
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
        /// Create (POST) a new fixedAssetCategories
        /// </summary>
        /// <param name="fixedAssetCategories">DTO of the new fixedAssetCategories</param>
        /// <returns>A fixedAssetCategories object <see cref="Dtos.FixedAssetCategory"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/fixed-asset-categories", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFixedAssetCategoriesV12")]
        public async Task<ActionResult<Dtos.FixedAssetCategory>> PostFixedAssetCategoriesAsync([FromBody] Dtos.FixedAssetCategory fixedAssetCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing fixedAssetCategories
        /// </summary>
        /// <param name="guid">GUID of the fixedAssetCategories to update</param>
        /// <param name="fixedAssetCategories">DTO of the updated fixedAssetCategories</param>
        /// <returns>A fixedAssetCategories object <see cref="Dtos.FixedAssetCategory"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/fixed-asset-categories/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFixedAssetCategoriesV12")]
        public async Task<ActionResult<Dtos.FixedAssetCategory>> PutFixedAssetCategoriesAsync([FromRoute] string guid, [FromBody] Dtos.FixedAssetCategory fixedAssetCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a fixedAssetCategories
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssetCategories</param>
        [HttpDelete]
        [Route("/fixed-asset-categories/{guid}", Name = "DefaultDeleteFixedAssetCategories", Order = -10)]
        public async Task<IActionResult> DeleteFixedAssetCategoriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
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
    /// Provides access to FixedAssets
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class FixedAssetsController : BaseCompressedApiController
    {
        private readonly IFixedAssetsService _fixedAssetsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FixedAssetsController class.
        /// </summary>
        /// <param name="fixedAssetsService">Service of type <see cref="IFixedAssetsService">IFixedAssetsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to action context accessor</param>
        /// <param name="apiSettings"></param>
        public FixedAssetsController(IFixedAssetsService fixedAssetsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _fixedAssetsService = fixedAssetsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all fixed-assets
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of FixedAssets <see cref="Dtos.FixedAssets"/> objects representing matching fixedAssets</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewFixedAssets })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/fixed-assets", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFixedAssets", IsEedmSupported = true)]
        public async Task<IActionResult> GetFixedAssetsAsync(Paging page)
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
                _fixedAssetsService.ValidatePermissions(GetPermissionsMetaData());

                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _fixedAssetsService.GetFixedAssetsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                  await _fixedAssetsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _fixedAssetsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.FixedAssets>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a fixed-assets using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssets</param>
        /// <returns>A fixedAssets object <see cref="Dtos.FixedAssets"/> in EEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewFixedAssets }) ]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/fixed-assets/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFixedAssetsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FixedAssets>> GetFixedAssetsByGuidAsync(string guid)
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
                _fixedAssetsService.ValidatePermissions(GetPermissionsMetaData());

                AddEthosContextProperties(
                   await _fixedAssetsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _fixedAssetsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));

                return await _fixedAssetsService.GetFixedAssetsByGuidAsync(guid);
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
        /// Create (POST) a new fixedAssets
        /// </summary>
        /// <param name="fixedAssets">DTO of the new fixedAssets</param>
        /// <returns>A fixedAssets object <see cref="Dtos.FixedAssets"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/fixed-assets", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFixedAssetsV12")]
        public async Task<ActionResult<Dtos.FixedAssets>> PostFixedAssetsAsync([FromBody] Dtos.FixedAssets fixedAssets)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing fixedAssets
        /// </summary>
        /// <param name="guid">GUID of the fixedAssets to update</param>
        /// <param name="fixedAssets">DTO of the updated fixedAssets</param>
        /// <returns>A fixedAssets object <see cref="Dtos.FixedAssets"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/fixed-assets/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFixedAssetsV12")]
        public async Task<ActionResult<Dtos.FixedAssets>> PutFixedAssetsAsync([FromRoute] string guid, [FromBody] Dtos.FixedAssets fixedAssets)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a fixedAssets
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssets</param>
        [HttpDelete]
        [Route("/fixed-assets/{guid}", Name = "DefaultDeleteFixedAssets", Order = -10)]
        public async Task<IActionResult> DeleteFixedAssetsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Return all Fixed asset transfer flags
        /// </summary>
        /// <returns>List of Fixed asset transfer flag <see cref="Dtos.ColleagueFinance.FixedAssetsFlag"/> objects representing matching FixedAssetsFlag</returns>
        /// <accessComments>
        /// Any authenticated user can get the Fixed asset transfer flags
        /// </accessComments>
        /// <note>FixedAssetsFlag is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/fixed-asset-transfer-flags", 1, true, Name = "GetFixedAssetTransferFlags")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.FixedAssetsFlag>>> GetFixedAssetTransferFlagsAsync()
        {
            try
            {
                var fixedAssetFlags = await _fixedAssetsService.GetFixedAssetTransferFlagsAsync();
                return Ok(fixedAssetFlags);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get Fixed asset transfer flags.", HttpStatusCode.BadRequest);
            }
        }

    }
}

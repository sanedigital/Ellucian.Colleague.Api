// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to VendorAddressUsages
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class VendorAddressUsagesController : BaseCompressedApiController
    {
        private readonly IVendorAddressUsagesService _vendorAddressUsagesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the VendorAddressUsagesController class.
        /// </summary>
        /// <param name="vendorAddressUsagesService">Service of type <see cref="IVendorAddressUsagesService">IVendorAddressUsagesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public VendorAddressUsagesController(IVendorAddressUsagesService vendorAddressUsagesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _vendorAddressUsagesService = vendorAddressUsagesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all vendorAddressUsages
        /// </summary>
        /// <returns>List of VendorAddressUsages <see cref="Dtos.VendorAddressUsages"/> objects representing matching vendorAddressUsages</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/vendor-address-usages", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVendorAddressUsages", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.VendorAddressUsages>>> GetVendorAddressUsagesAsync()
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
                var vendorAddressUsages = await _vendorAddressUsagesService.GetVendorAddressUsagesAsync(bypassCache);

                if (vendorAddressUsages != null && vendorAddressUsages.Any())
                {
                    AddEthosContextProperties(await _vendorAddressUsagesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _vendorAddressUsagesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              vendorAddressUsages.Select(a => a.Id).ToList()));
                }
                return Ok(vendorAddressUsages);
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
        /// Read (GET) a vendorAddressUsages using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired vendorAddressUsages</param>
        /// <returns>A vendorAddressUsages object <see cref="Dtos.VendorAddressUsages"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/vendor-address-usages/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVendorAddressUsagesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.VendorAddressUsages>> GetVendorAddressUsagesByGuidAsync(string guid)
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
                   await _vendorAddressUsagesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _vendorAddressUsagesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _vendorAddressUsagesService.GetVendorAddressUsagesByGuidAsync(guid);
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
        /// Create (POST) a new vendorAddressUsages
        /// </summary>
        /// <param name="vendorAddressUsages">DTO of the new vendorAddressUsages</param>
        /// <returns>A vendorAddressUsages object <see cref="Dtos.VendorAddressUsages"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/vendor-address-usages", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostVendorAddressUsagesV1.0.0")]
        public async Task<ActionResult<Dtos.VendorAddressUsages>> PostVendorAddressUsagesAsync([FromBody] Dtos.VendorAddressUsages vendorAddressUsages)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing vendorAddressUsages
        /// </summary>
        /// <param name="guid">GUID of the vendorAddressUsages to update</param>
        /// <param name="vendorAddressUsages">DTO of the updated vendorAddressUsages</param>
        /// <returns>A vendorAddressUsages object <see cref="Dtos.VendorAddressUsages"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/vendor-address-usages/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutVendorAddressUsagesV1.0.0")]
        public async Task<ActionResult<Dtos.VendorAddressUsages>> PutVendorAddressUsagesAsync([FromRoute] string guid, [FromBody] Dtos.VendorAddressUsages vendorAddressUsages)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a vendorAddressUsages
        /// </summary>
        /// <param name="guid">GUID to desired vendorAddressUsages</param>
        [HttpDelete]
        [Route("/vendor-address-usages/{guid}", Name = "DefaultDeleteVendorAddressUsages")]
        public async Task<IActionResult> DeleteVendorAddressUsagesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

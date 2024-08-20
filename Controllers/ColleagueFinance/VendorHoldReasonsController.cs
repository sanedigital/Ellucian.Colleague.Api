// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Colleague.Dtos;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to VendorHoldReasons
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class VendorHoldReasonsController : BaseCompressedApiController
    {
        private readonly IVendorHoldReasonsService _vendorHoldReasonsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the VendorHoldReasonsController class.
        /// </summary>
        /// <param name="vendorHoldReasonsService">Service of type <see cref="IVendorHoldReasonsService">IVendorHoldReasonsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public VendorHoldReasonsController(IVendorHoldReasonsService vendorHoldReasonsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _vendorHoldReasonsService = vendorHoldReasonsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all vendorHoldReasons
        /// </summary>
        /// <returns>List of VendorHoldReasons <see cref="VendorHoldReasons"/> objects representing matching vendorHoldReasons</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/vendor-hold-reasons", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVendorHoldReasons", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<VendorHoldReasons>>> GetVendorHoldReasonsAsync()
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
                var vendorHoldReasons = await _vendorHoldReasonsService.GetVendorHoldReasonsAsync(bypassCache);

                if (vendorHoldReasons != null && vendorHoldReasons.Any())
                {
                    AddEthosContextProperties(await _vendorHoldReasonsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _vendorHoldReasonsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              vendorHoldReasons.Select(a => a.Id).ToList()));
                }

                return Ok(vendorHoldReasons);                
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
        /// Read (GET) a vendorHoldReasons using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired vendorHoldReasons</param>
        /// <returns>A vendorHoldReasons object <see cref="VendorHoldReasons"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/vendor-hold-reasons/{guid}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVendorHoldReasonsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<VendorHoldReasons>> GetVendorHoldReasonsByIdAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _vendorHoldReasonsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _vendorHoldReasonsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _vendorHoldReasonsService.GetVendorHoldReasonsByGuidAsync(guid);
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
        /// Create (POST) a new vendorHoldReasons
        /// </summary>
        /// <param name="vendorHoldReasons">DTO of the new vendorHoldReasons</param>
        /// <returns>A vendorHoldReasons object <see cref="VendorHoldReasons"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/vendor-hold-reasons", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostVendorHoldReasonsV8Async")]
        public async Task<ActionResult<VendorHoldReasons>> PostVendorHoldReasonsAsync([FromBody] VendorHoldReasons vendorHoldReasons)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing vendorHoldReasons
        /// </summary>
        /// <param name="guid">GUID of the vendorHoldReasons to update</param>
        /// <param name="vendorHoldReasons">DTO of the updated vendorHoldReasons</param>
        /// <returns>A vendorHoldReasons object <see cref="Dtos.VendorHoldReasons"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/vendor-hold-reasons/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutVendorHoldReasonsV8Async")]
        public async Task<ActionResult<VendorHoldReasons>> PutVendorHoldReasonsAsync([FromRoute] string guid, [FromBody] VendorHoldReasons vendorHoldReasons)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a vendorHoldReasons
        /// </summary>
        /// <param name="guid">GUID to desired vendorHoldReasons</param>
        [HttpDelete]
        [Route("/vendor-hold-reasons/{guid}", Name = "DefaultDeleteVendorHoldReasonsAsync")]
        public async Task<IActionResult> DeleteVendorHoldReasonsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

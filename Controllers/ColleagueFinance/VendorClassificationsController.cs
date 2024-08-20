// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to Vendor Types data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class VendorClassificationsController : BaseCompressedApiController
    {
        private readonly IVendorTypesService _vendorTypesService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the VendorTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="vendorTypesService">Service of type <see cref="IVendorTypesService">Vendor type service</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public VendorClassificationsController(IAdapterRegistry adapterRegistry, IVendorTypesService vendorTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _vendorTypesService = vendorTypesService;
            _adapterRegistry = adapterRegistry;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all Vendor types.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All vendor types objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/vendor-classifications", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVendorTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.VendorType>>> GetVendorTypesAsync()
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var items = await _vendorTypesService.GetVendorTypesAsync(bypassCache);

                AddEthosContextProperties(
                  await _vendorTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _vendorTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a vendor type by ID.
        /// </summary>
        /// <param name="guid">Guid of vendor type to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.VendorType">vendor type.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/vendor-classifications/{guid}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVendorTypesById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.VendorType>> GetVendorTypeByIdAsync(string guid)
        {
            try
            {
                AddEthosContextProperties(
                    await _vendorTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), true),
                    await _vendorTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _vendorTypesService.GetVendorTypeByIdAsync(guid);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Creates a Vendor type.
        /// </summary>
        /// <param name="vendorType"><see cref="Dtos.VendorType">VendorType</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.VendorType">vendor type</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/vendor-classifications", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostVendorTypes")]
        public async Task<ActionResult<Dtos.VendorType>> PostVendorTypeAsync([FromBody] Dtos.VendorType vendorType)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Updates a vendor type.
        /// </summary>
        /// <param name="guid">Guid of the vendor to update</param>
        /// <param name="vendorType"><see cref="Dtos.VendorType">vendor type</see> to update</param>
        /// <returns>Updated <see cref="Dtos.VendorType">vendor type</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/vendor-classifications/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutVendorTypes")]
        public async Task<ActionResult<Dtos.VendorType>> PutVendorTypeAsync([FromRoute] string guid, [FromBody] Dtos.VendorType vendorType)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing vendor type.
        /// </summary>
        /// <param name="guid">Guid of the vendor type to delete</param>
        [HttpDelete]
        [Route("/vendor-classifications/{guid}", Name = "DeleteVendorTypes", Order = -10)]
        public async Task<IActionResult> DeleteVendorTypeAsync([FromRoute] string guid)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

    }
}

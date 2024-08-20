// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Routes;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to AddressType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AddressTypesController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IAddressTypeService _addressTypeService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the AddressTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="addressTypeService">Service of type <see cref="IAddressTypeService">IAddressTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AddressTypesController(IAdapterRegistry adapterRegistry, IAddressTypeService addressTypeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _addressTypeService = addressTypeService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all address types.
        /// </summary>
        /// <returns>All AddressType objects.</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/address-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHeDMAddressTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AddressType2>>> GetAddressTypesAsync()
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
                var addressType = await _addressTypeService.GetAddressTypesAsync(bypassCache);

                if (addressType != null && addressType.Any())
                {
                    AddEthosContextProperties(await _addressTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _addressTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              addressType.Select(a => a.Id).ToList()));
                }

                return Ok(await _addressTypeService.GetAddressTypesAsync(bypassCache));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves an address type by GUID.
        /// </summary>
        /// /// <param name="id">Unique ID representing the Address Type to get</param>
        /// <returns>An AddressType object.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/address-types/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAddressTypeByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.AddressType2>> GetAddressTypeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _addressTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _addressTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _addressTypeService.GetAddressTypeByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Delete an existing Address type in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Unique ID representing the Address Type to delete</param>
        [HttpDelete]
        [Route("/address-types/{id}", Name = "DeleteHeDMAddressTypes", Order = -10)]
        public async Task<IActionResult> DeleteAddressTypesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Update a Address Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="AddressType"><see cref="AddressType2">AddressType</see> to update</param>
        [HttpPut]
        [HeaderVersionRoute("/address-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHeDMAddressTypes")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.AddressType2>> PutAddressTypesAsync([FromBody] Ellucian.Colleague.Dtos.AddressType2 AddressType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Post Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Create a Address Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="AddressType"><see cref="AddressType2">AddressType</see> to create</param>
        [HttpPost]
        [HeaderVersionRoute("/address-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHeDMAddressTypes")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.AddressType2>> PostAddressTypesAsync([FromBody] Ellucian.Colleague.Dtos.AddressType2 AddressType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}

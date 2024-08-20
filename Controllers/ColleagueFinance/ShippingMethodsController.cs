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
    /// Provides access to ShippingMethods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class ShippingMethodsController : BaseCompressedApiController
    {
        private readonly IShippingMethodsService _shippingMethodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ShippingMethodsController class.
        /// </summary>
        /// <param name="shippingMethodsService">Service of type <see cref="IShippingMethodsService">IShippingMethodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ShippingMethodsController(IShippingMethodsService shippingMethodsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _shippingMethodsService = shippingMethodsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all shippingMethods
        /// </summary>
        /// <returns>List of ShippingMethods <see cref="Dtos.ShippingMethods"/> objects representing matching shippingMethods</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/shipping-methods", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetShippingMethods", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ShippingMethods>>> GetShippingMethodsAsync()
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
                var pageOfItems = await _shippingMethodsService.GetShippingMethodsAsync(bypassCache);

                AddEthosContextProperties(
                    await _shippingMethodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _shippingMethodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Select(i => i.Id).Distinct().ToList()));

                return Ok(pageOfItems);
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
        /// Read (GET) a ShippingMethods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired ShippingMethods</param>
        /// <returns>A ShippingMethods object <see cref="Dtos.ShippingMethods"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/shipping-methods/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetShippingMethodsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ShippingMethods>> GetShippingMethodsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

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
                AddEthosContextProperties(
                  await _shippingMethodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _shippingMethodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));

                return await _shippingMethodsService.GetShippingMethodsByGuidAsync(guid);
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
        /// Create (POST) a new ShippingMethods
        /// </summary>
        /// <param name="shippingMethods">DTO of the new ShippingMethods</param>
        /// <returns>A ShippingMethods object <see cref="Dtos.ShippingMethods"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/shipping-methods", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostShippingMethodsV11")]
        public async Task<ActionResult<Dtos.ShippingMethods>> PostShippingMethodsAsync([FromBody] Dtos.ShippingMethods shippingMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing ShippingMethods
        /// </summary>
        /// <param name="guid">GUID of the ShippingMethods to update</param>
        /// <param name="shippingMethods">DTO of the updated ShippingMethods</param>
        /// <returns>A ShippingMethods object <see cref="Dtos.ShippingMethods"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/shipping-methods/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutShippingMethodsV11")]
        public async Task<ActionResult<Dtos.ShippingMethods>> PutShippingMethodsAsync([FromRoute] string guid, [FromBody] Dtos.ShippingMethods shippingMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a ShippingMethods
        /// </summary>
        /// <param name="guid">GUID to desired ShippingMethods</param>
        [HttpDelete]
        [Route("/shipping-methods/{guid}", Name = "DefaultDeleteShippingMethods")]
        public async Task<IActionResult> DeleteShippingMethodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Return all ShipViaCode
        /// </summary>
        /// <returns>List of ShipViaCodes <see cref="Dtos.ColleagueFinance.ShipViaCode"/> objects representing matching ShipToCode</returns>
        [HttpGet]
        [HeaderVersionRoute("/ship-via-codes", 1, false, Name = "GetShipViaCodes")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.ShipViaCode>>> GetShipViaCodesAsync()
        {
            try
            {
                var dtos = await _shippingMethodsService.GetShipViaCodesAsync();
                return Ok(dtos);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get Ship Via Codes.", HttpStatusCode.BadRequest);
            }
        }
    }
}

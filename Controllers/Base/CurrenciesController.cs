// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Web.Http.Controllers;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Security;
using System.Net;
using Ellucian.Colleague.Coordination.Base.Services;

using Ellucian.Web.Http.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides a API controller for fetching country codes.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CurrenciesController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly ICurrenciesService _currenciesService;

        /// <summary>
        /// Initializes a new instance of the CurrenciesController class.
        /// </summary>
        /// <param name="currenciesService">Service of type <see cref="ICurrenciesService">ICurrenciesService</see></param>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CurrenciesController(ICurrenciesService currenciesService, IAdapterRegistry adapterRegistry, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._adapterRegistry = adapterRegistry;
            this._logger = logger;
            this._currenciesService = currenciesService;
        }

        #region currencies
        /// <summary>
        /// Return all currencies
        /// </summary>
        /// <returns>List of Currencies <see cref="Dtos.Currencies"/> objects representing matching currencies</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/currencies", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCurrencies", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Currencies>>> GetCurrenciesAsync()
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
                var currencies = await _currenciesService.GetCurrenciesAsync(bypassCache);

                if (currencies != null && currencies.Any())
                {
                    AddEthosContextProperties(await _currenciesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _currenciesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              currencies.Select(a => a.Id).ToList()));
                }
                return Ok(currencies);
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
        /// Read (GET) a currencies using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired currencies</param>
        /// <returns>A currencies object <see cref="Dtos.Currencies"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/currencies/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCurrenciesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Currencies>> GetCurrenciesByGuidAsync(string guid)
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
                   await _currenciesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _currenciesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _currenciesService.GetCurrenciesByGuidAsync(guid);
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
        /// Create (POST) a new currencies
        /// </summary>
        /// <param name="currencies">DTO of the new currencies</param>
        /// <returns>A currencies object <see cref="Dtos.Currencies"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/currencies", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCurrenciesV1.0.0")]
        public async Task<ActionResult<Dtos.Currencies>> PostCurrenciesAsync([FromBody] Dtos.Currencies currencies)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing currencies
        /// </summary>
        /// <param name="guid">GUID of the currencies to update</param>
        /// <param name="currencies">DTO of the updated currencies</param>
        /// <returns>A currencies object <see cref="Dtos.Currencies"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPut]
        [HeaderVersionRoute("/currencies/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCurrenciesV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Currencies>> PutCurrenciesAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Currencies currencies)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (currencies == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null comment argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(currencies.Id))
            {
                currencies.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(currencies.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != currencies.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }


            try
            {
                //get Data Privacy List
                var dpList = await _currenciesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _currenciesService.ImportExtendedEthosData(await ExtractExtendedData(await _currenciesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var currenciesReturn = await _currenciesService.PutCurrenciesAsync(guid,
                    await PerformPartialPayloadMerge(currencies, async () => await _currenciesService.GetCurrenciesByGuidAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _currenciesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return currenciesReturn;
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
        /// Delete (DELETE) a currencies
        /// </summary>
        /// <param name="guid">GUID to desired currencies</param>
        [HttpDelete]
        [Route("/currencies/{guid}", Name = "DefaultDeleteCurrencies", Order = -10)]
        public async Task<IActionResult> DeleteCurrenciesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
        #endregion

        #region currency-iso-codes

        /// <summary>
        /// Return all currencyIsoCodes
        /// </summary>
        /// <returns>List of CurrencyIsoCodes <see cref="Dtos.CurrencyIsoCodes"/> objects representing matching currencyIsoCodes</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/currency-iso-codes", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCurrencyIsoCodes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CurrencyIsoCodes>>> GetCurrencyIsoCodesAsync()
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
                var currencyIsoCodes = await _currenciesService.GetCurrencyIsoCodesAsync(bypassCache);

                if (currencyIsoCodes != null && currencyIsoCodes.Any())
                {
                    AddEthosContextProperties(await _currenciesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _currenciesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              currencyIsoCodes.Select(a => a.Id).ToList()));
                }
                return Ok(currencyIsoCodes);
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
        /// Read (GET) a currencyIsoCodes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired currencyIsoCodes</param>
        /// <returns>A currencyIsoCodes object <see cref="Dtos.CurrencyIsoCodes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/currency-iso-codes/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCurrencyIsoCodesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.CurrencyIsoCodes>> GetCurrencyIsoCodesByGuidAsync(string guid)
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
                   await _currenciesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _currenciesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _currenciesService.GetCurrencyIsoCodesByGuidAsync(guid);
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
        /// Create (POST) a new currencyIsoCodes
        /// </summary>
        /// <param name="currencyIsoCodes">DTO of the new currencyIsoCodes</param>
        /// <returns>A currencyIsoCodes object <see cref="Dtos.CurrencyIsoCodes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/currency-iso-codes", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCurrencyIsoCodesV1.0.0")]
        public async Task<ActionResult<Dtos.CurrencyIsoCodes>> PostCurrencyIsoCodesAsync([FromBody] Dtos.CurrencyIsoCodes currencyIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing currencyIsoCodes
        /// </summary>
        /// <param name="guid">GUID of the currencyIsoCodes to update</param>
        /// <param name="currencyIsoCodes">DTO of the updated currencyIsoCodes</param>
        /// <returns>A currencyIsoCodes object <see cref="Dtos.CurrencyIsoCodes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/currency-iso-codes/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCurrencyIsoCodesV1.0.0")]
        public async Task<ActionResult<Dtos.CurrencyIsoCodes>> PutCurrencyIsoCodesAsync([FromRoute] string guid, [FromBody] Dtos.CurrencyIsoCodes currencyIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a currencyIsoCodes
        /// </summary>
        /// <param name="guid">GUID to desired currencyIsoCodes</param>
        [HttpDelete]
        [Route("/currency-iso-codes/{guid}", Name = "DefaultDeleteCurrencyIsoCodes", Order = -10)]
        public async Task<IActionResult> DeleteCurrencyIsoCodesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
        #endregion
    }
}

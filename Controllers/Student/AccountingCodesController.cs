// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to AccountingCodes data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AccountingCodesController : BaseCompressedApiController
    {
        private readonly IAccountingCodesService _accountingCodesService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AccountingCodesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="accountingCodesService">Service of type <see cref="IAccountingCodesService">IAccountingCodesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AccountingCodesController(IAdapterRegistry adapterRegistry, IAccountingCodesService accountingCodesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _accountingCodesService = accountingCodesService;
            _adapterRegistry = adapterRegistry;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all accounting codes.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All accounting codes objects.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/accounting-codes", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountingCodesV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AccountingCode>>> GetAccountingCodesAsync()
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
                var items = await _accountingCodesService.GetAccountingCodesAsync(bypassCache);

                AddEthosContextProperties(
                    await _accountingCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _accountingCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all accounting codes.
        /// </summary>
        /// <param name="criteria">filters</param>
        /// <returns></returns>
        [HttpGet, ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AccountingCode2))]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { false, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/accounting-codes", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAccountingCodes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AccountingCode2>>> GetAccountingCodes2Async(QueryStringFilter criteria)
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
                AccountingCodeCategoryDtoProperty category = null;
                var accountingCodeFilter = GetFilterObject<Dtos.AccountingCode2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.AccountingCode2>(new List<Dtos.AccountingCode2>());

                if (accountingCodeFilter != null)
                {
                    category = accountingCodeFilter.AccountingCodeCategory;
                }
              
                var items = await _accountingCodesService.GetAccountingCodes2Async(category, bypassCache);

                AddEthosContextProperties(
                    await _accountingCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _accountingCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (JsonReaderException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                   IntegrationApiUtility.GetDefaultApiError("Error parsing JSON course search request.")));
            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                   IntegrationApiUtility.GetDefaultApiError("Error parsing JSON course search request.")));
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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves a accounting code by ID.
        /// </summary>
        /// <param name="id">Id of accounting code to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.AccountingCode">accounting code.</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/accounting-codes/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountingCodeByIdV6", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.AccountingCode>> GetAccountingCodeByIdAsync(string id)
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
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Accounting code id is required.");
                }

                AddEthosContextProperties(
                    await _accountingCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _accountingCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _accountingCodesService.GetAccountingCodeByIdAsync(id);
            }
            catch (KeyNotFoundException e)
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves a accounting code2 by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/accounting-codes/{id}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAccountingCodeById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.AccountingCode2>> GetAccountingCodeById2Async(string id)
        {
            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Accounting code id is required.");
                }

                AddEthosContextProperties(
                    await _accountingCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _accountingCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                
                return await _accountingCodesService.GetAccountingCode2ByIdAsync(id, bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Creates a AccountingCode.
        /// </summary>
        /// <param name="accountingCode"><see cref="Dtos.AccountingCode">AccountingCode</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AccountingCode">AccountingCode</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/accounting-codes", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountingCodesV6")]
        [HeaderVersionRoute("/accounting-codes", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountingCodesV11")]
        public async Task<ActionResult<Dtos.AccountingCode>> PostAccountingCode([FromBody] Dtos.AccountingCode accountingCode)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Updates a accounting code.
        /// </summary>
        /// <param name="id">Id of the AccountingCode to update</param>
        /// <param name="accountingCode"><see cref="Dtos.AccountingCode">AccountingCode</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AccountingCode">AccountingCode</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/accounting-codes/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountingCodesV6")]
        [HeaderVersionRoute("/accounting-codes/{id}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountingCodesV11")]
        public async Task<ActionResult<Dtos.AccountingCode>> PutAccountingCode([FromRoute] string id, [FromBody] Dtos.AccountingCode accountingCode)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing accountingCode
        /// </summary>
        /// <param name="id">Id of the accountingCode to delete</param>
        [HttpDelete]
        [Route("/accounting-codes/{id}", Name = "DefaultDeleteAccountingCodes")]
        public async Task<IActionResult> DeleteAccountingCode([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

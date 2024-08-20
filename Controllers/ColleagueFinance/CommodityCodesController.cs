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
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to CommodityCodes data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class CommodityCodesController : BaseCompressedApiController
    {
        private readonly ICommodityCodesService _commodityCodesService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CommodityCodesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="commodityCodesService">Service of type <see cref="ICommodityCodesService">ICommodityCodesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CommodityCodesController(IAdapterRegistry adapterRegistry, ICommodityCodesService commodityCodesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _commodityCodesService = commodityCodesService;
            _adapterRegistry = adapterRegistry;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all commodity codes.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All commodity codes objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/commodity-codes", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmCommodityCodes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CommodityCode>>> GetCommodityCodesAsync()
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
                var items = await _commodityCodesService.GetCommodityCodesAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _commodityCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _commodityCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(a => a.Id).ToList()));
                }

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
        /// Retrieves a commodity code by ID.
        /// </summary>
        /// <param name="id">Id of commodity code to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CommodityCode">commodity code.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/commodity-codes/{id}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmCommodityCodesById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.CommodityCode>> GetCommodityCodeByIdAsync(string id)
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
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

            try
            {
                var item = await _commodityCodesService.GetCommodityCodeByIdAsync(id);

                if (item != null)
                {
                    AddEthosContextProperties(await _commodityCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _commodityCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { item.Id }));
                }

                return item;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Creates a CommodityCode.
        /// </summary>
        /// <param name="commodityCode"><see cref="Dtos.CommodityCode">CommodityCode</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.CommodityCode">CommodityCode</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/commodity-codes", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmCommodityCodes")]
        public async Task<ActionResult<Dtos.CommodityCode>> PostCommodityCodeAsync([FromBody] Dtos.CommodityCode commodityCode)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Updates a commodity code.
        /// </summary>
        /// <param name="id">Id of the CommodityCode to update</param>
        /// <param name="commodityCode"><see cref="Dtos.CommodityCode">CommodityCode</see> to create</param>
        /// <returns>Updated <see cref="Dtos.CommodityCode">CommodityCode</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/commodity-codes/{id}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmCommodityCodes")]
        public async Task<ActionResult<Dtos.CommodityCode>> PutCommodityCodeAsync([FromRoute] string id, [FromBody] Dtos.CommodityCode commodityCode)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing commodityCode
        /// </summary>
        /// <param name="id">Id of the commodityCode to delete</param>
        [HttpDelete]
        [Route("/commodity-codes/{id}", Name = "DeleteHedmCommodityCodes", Order = -10)]
        public async Task<IActionResult> DeleteCommodityCodeAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


        /// <summary>
        /// Returns all CommodityCodes
        /// </summary>
        /// <returns>List of CommodityCodes DTO objects </returns>
        /// <accessComments>
        /// No permission is needed.
        /// </accessComments>
        /// <note>ComodityCodes is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/commodity-codes", 1, false, Name = "GetAllCommodityCodes")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode>>> GetAllCommodityCodesAsync()
        {
            try
            {
                var dtos = await _commodityCodesService.GetAllCommodityCodesAsync();
                return Ok(dtos);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get Commodity Codes.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns a commodity code
        /// </summary>
        /// <param name="commodityCode">commodity code</param>
        /// <returns>Procurement commodity code DTO</returns>
        /// <accessComments>
        /// No permission is needed.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/commodity-codes/{commodityCode}", 1, false, Name = "GetCommodityCodeAsync")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode>> GetCommodityCodeAsync(string commodityCode)
        {
            if (string.IsNullOrEmpty(commodityCode))
            {
                string message = "commodityCode must be specified.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                var dto = await _commodityCodesService.GetCommodityCodeByCodeAsync(commodityCode);
                return dto;
            }
            catch (ArgumentNullException anex)
            {
                _logger.LogError(anex, "Invalid argument to get Commodity Code.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.LogError(knfex, "Commodity Code not found.");
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get Commodity Code.");
                return CreateHttpResponseException("Unable to get Commodity Code.", HttpStatusCode.BadRequest);
            }
        }
    }
}

// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to Bargaining Units data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class BargainingUnitsController : BaseCompressedApiController
    {
        private readonly IBargainingUnitsService _bargainingUnitsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BargainingUnitsController class.
        /// </summary>
        /// <param name="bargainingUnitsService">Service of type <see cref="IBargainingUnitsService">IBargainingUnitsService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BargainingUnitsController(IBargainingUnitsService bargainingUnitsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _bargainingUnitsService = bargainingUnitsService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves all bargaining units.
        /// </summary>
        /// <returns>All BargainingUnit objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/bargaining-units", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmBargainingUnits", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.BargainingUnit>>> GetBargainingUnitsAsync()
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

                var items = await _bargainingUnitsService.GetBargainingUnitsAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _bargainingUnitsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _bargainingUnitsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves a bargaining unit by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.BargainingUnit">BargainingUnit.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/bargaining-units/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmBargainingUnitsById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.BargainingUnit>> GetBargainingUnitByIdAsync(string id)
        {
            var bypassCache = false;
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
                var item = await _bargainingUnitsService.GetBargainingUnitsByGuidAsync(id);

                if (item != null)
                {
                    AddEthosContextProperties(await _bargainingUnitsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _bargainingUnitsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { item.Id }));
                }

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates a BargainingUnit.
        /// </summary>
        /// <param name="bargainingUnit"><see cref="BargainingUnit">BargainingUnit</see> to update</param>
        /// <returns>Newly updated <see cref="BargainingUnit">BargainingUnit</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/bargaining-units/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmBargainingUnits")]
        public async Task<ActionResult<Dtos.BargainingUnit>> PutBargainingUnitAsync([FromBody] Dtos.BargainingUnit bargainingUnit)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a BargainingUnit.
        /// </summary>
        /// <param name="bargainingUnit"><see cref="BargainingUnit">BargainingUnit</see> to create</param>
        /// <returns>Newly created <see cref="BargainingUnit">BargainingUnit</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/bargaining-units", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmBargainingUnits")]
        public async Task<ActionResult<Dtos.BargainingUnit>> PostBargainingUnitAsync([FromBody] Dtos.BargainingUnit bargainingUnit)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing BargainingUnit
        /// </summary>
        /// <param name="id">Id of the BargainingUnit to delete</param>
        [HttpDelete]
        [Route("/bargaining-units/{id}", Name = "DeleteHedmBargainingUnits")]
        public async Task<IActionResult> DeleteBargainingUnitAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

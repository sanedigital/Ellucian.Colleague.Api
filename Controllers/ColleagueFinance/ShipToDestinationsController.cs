// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to ShipToDestinations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class ShipToDestinationsController : BaseCompressedApiController
    {
        private readonly IShipToDestinationsService _shipToDestinationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ShipToDestinationsController class.
        /// </summary>
        /// <param name="shipToDestinationsService">Service of type <see cref="IShipToDestinationsService">IShipToDestinationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ShipToDestinationsController(IShipToDestinationsService shipToDestinationsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _shipToDestinationsService = shipToDestinationsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all shipToDestinations
        /// </summary>
        /// <returns>List of ShipToDestinations <see cref="Dtos.ShipToDestinations"/> objects representing matching shipToDestinations</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/ship-to-destinations", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetShipToDestinations", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ShipToDestinations>>> GetShipToDestinationsAsync()
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
                var dtos = await _shipToDestinationsService.GetShipToDestinationsAsync(bypassCache);

                AddEthosContextProperties(
                    await _shipToDestinationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _shipToDestinationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        dtos.Select(i => i.Id).ToList()));

                return Ok(dtos);
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
        /// Return all ShipToCodes
        /// </summary>
        /// <returns>List of ShipToCodes <see cref="Dtos.ColleagueFinance.ShipToCode"/> objects representing matching ShipToCode</returns>
        /// <note>ShipToCode is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/ship-to-codes", 1, false, Name = "GetShipToCodes")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.ShipToCode>>> GetShipToCodesAsync()
        {
            try
            {
                var dtos = await _shipToDestinationsService.GetShipToCodesAsync();
                return Ok(dtos);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get Ship to Codes.", HttpStatusCode.BadRequest);
            }
        }



        /// <summary>
        /// Read (GET) a shipToDestinations using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired shipToDestinations</param>
        /// <returns>A shipToDestinations object <see cref="Dtos.ShipToDestinations"/> in EEDM format</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/ship-to-destinations/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetShipToDestinationsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ShipToDestinations>> GetShipToDestinationsByGuidAsync(string guid)
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
                    await _shipToDestinationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _shipToDestinationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _shipToDestinationsService.GetShipToDestinationsByGuidAsync(guid);
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
        /// Create (POST) a new shipToDestinations
        /// </summary>
        /// <param name="shipToDestinations">DTO of the new shipToDestinations</param>
        /// <returns>A shipToDestinations object <see cref="Dtos.ShipToDestinations"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/ship-to-destinations", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostShipToDestinationsV10")]
        public async Task<ActionResult<Dtos.ShipToDestinations>> PostShipToDestinationsAsync([FromBody] Dtos.ShipToDestinations shipToDestinations)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing shipToDestinations
        /// </summary>
        /// <param name="guid">GUID of the shipToDestinations to update</param>
        /// <param name="shipToDestinations">DTO of the updated shipToDestinations</param>
        /// <returns>A shipToDestinations object <see cref="Dtos.ShipToDestinations"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/ship-to-destinations/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutShipToDestinationsV10")]
        public async Task<ActionResult<Dtos.ShipToDestinations>> PutShipToDestinationsAsync([FromRoute] string guid, [FromBody] Dtos.ShipToDestinations shipToDestinations)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a shipToDestinations
        /// </summary>
        /// <param name="guid">GUID to desired shipToDestinations</param>
        [HttpDelete]
        [Route("/ship-to-destinations/{guid}", Name = "DefaultDeleteShipToDestinations")]
        public async Task<IActionResult> DeleteShipToDestinationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

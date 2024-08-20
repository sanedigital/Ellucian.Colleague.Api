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
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to RoomRates
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class RoomRatesController : BaseCompressedApiController
    {
        private readonly IRoomRatesService _roomRatesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RoomRatesController class.
        /// </summary>
        /// <param name="roomRatesService">Service of type <see cref="IRoomRatesService">IRoomRatesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RoomRatesController(IRoomRatesService roomRatesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _roomRatesService = roomRatesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all roomRates
        /// </summary>
        /// <returns>List of RoomRates <see cref="Dtos.RoomRates"/> objects representing matching roomRates</returns>
        [HttpGet]       
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/room-rates", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRoomRates", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.RoomRates>>> GetRoomRatesAsync()
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
                var roomRates = await _roomRatesService.GetRoomRatesAsync(bypassCache);

                if (roomRates != null && roomRates.Any())
                {

                    AddEthosContextProperties(await _roomRatesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _roomRatesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              roomRates.Select(a => a.Id).ToList()));
                }
                return Ok(roomRates);
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
        /// Read (GET) a roomRates using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired roomRates</param>
        /// <returns>A roomRates object <see cref="Dtos.RoomRates"/> in EEDM format</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/room-rates/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRoomRatesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.RoomRates>> GetRoomRatesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                var rate = await _roomRatesService.GetRoomRatesByGuidAsync(guid);

                if (rate != null)
                {

                    AddEthosContextProperties(await _roomRatesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _roomRatesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { rate.Id }));
                }


                return rate;
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
        /// Create (POST) a new roomRates
        /// </summary>
        /// <param name="roomRates">DTO of the new roomRates</param>
        /// <returns>A roomRates object <see cref="Dtos.RoomRates"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/room-rates", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRoomRatesV10")]
        public async Task<ActionResult<Dtos.RoomRates>> PostRoomRatesAsync([FromBody] Dtos.RoomRates roomRates)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing roomRates
        /// </summary>
        /// <param name="guid">GUID of the roomRates to update</param>
        /// <param name="roomRates">DTO of the updated roomRates</param>
        /// <returns>A roomRates object <see cref="Dtos.RoomRates"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/room-rates/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRoomRatesV10")]
        public async Task<ActionResult<Dtos.RoomRates>> PutRoomRatesAsync([FromRoute] string guid, [FromBody] Dtos.RoomRates roomRates)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a roomRates
        /// </summary>
        /// <param name="guid">GUID to desired roomRates</param>
        [HttpDelete]
        [Route("/room-rates/{guid}", Name = "DefaultDeleteRoomRates", Order = -10)]
        public async Task<IActionResult> DeleteRoomRatesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

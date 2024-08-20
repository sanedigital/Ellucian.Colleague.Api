// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to room characteristic data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RoomCharacteristicsController : BaseCompressedApiController
    {
        private readonly IRoomCharacteristicService _roomCharacteristicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RoomCharacteristicsController class.
        /// </summary>
        /// <param name="roomCharacteristicService">Service of type <see cref="IRoomCharacteristicService">IRoomCharacteristicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RoomCharacteristicsController(IRoomCharacteristicService roomCharacteristicService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _roomCharacteristicService = roomCharacteristicService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model VERSION 6</remarks>
        /// <summary>
        /// Retrieves all room characteristics.
        /// </summary>
        /// <returns>All RoomCharacteristics objects.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/room-characteristics", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRoomCharacteristics", IsEedmSupported = true)]
        [HeaderVersionRoute("/room-characteristics", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRoomCharacteristics", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.RoomCharacteristic>>> GetRoomCharacteristicsAsync()
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
                var roomCharacteristic = await _roomCharacteristicService.GetRoomCharacteristicsAsync(bypassCache);

                if (roomCharacteristic != null && roomCharacteristic.Any())
                {
                    AddEthosContextProperties(await _roomCharacteristicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _roomCharacteristicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              roomCharacteristic.Select(a => a.Id).ToList()));
                }
                return Ok(roomCharacteristic);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model VERSION 6</remarks>
        /// <summary>
        /// Retrieves a room characteristic by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.RoomCharacteristic">RoomCharacteristic.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/room-characteristics/{id}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRoomCharacteristicByGuid", IsEedmSupported = true)]
        [HeaderVersionRoute("/room-characteristics/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRoomCharacteristicByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.RoomCharacteristic>> GetRoomCharacteristicByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Must provide a room characteristic id.");
                }

                AddEthosContextProperties(
                    await _roomCharacteristicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _roomCharacteristicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _roomCharacteristicService.GetRoomCharacteristicByGuidAsync(id);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Updates a RoomCharacteristic.
        /// </summary>
        /// <param name="roomCharacteristic">RoomCharacteristic to update</param>
        /// <returns>Newly updated RoomCharacteristic</returns>
        [HttpPut]
        [HeaderVersionRoute("/room-characteristics/{id}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRoomCharacteristicV8")]
        [HeaderVersionRoute("/room-characteristics/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRoomCharacteristic")]
        public async Task<ActionResult<Dtos.RoomCharacteristic>> PutRoomCharacteristicAsync([FromBody] Dtos.RoomCharacteristic roomCharacteristic)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a RoomCharacteristic.
        /// </summary>
        /// <param name="roomCharacteristic">RoomCharacteristic to create</param>
        /// <returns>Newly created RoomCharacteristic</returns>
        [HttpPost]
        [HeaderVersionRoute("/room-characteristics", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRoomCharacteristicV8")]
        [HeaderVersionRoute("/room-characteristics", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRoomCharacteristic")]
        public async Task<ActionResult<Dtos.RoomCharacteristic>> PostRoomCharacteristicAsync([FromBody] Dtos.RoomCharacteristic roomCharacteristic)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing RoomCharacteristic
        /// </summary>
        /// <param name="id">Id of the RoomCharacteristic to delete</param>
        [HttpDelete]
        [Route("/room-characteristics/{id}", Name = "DeleteRoomCharacteristic", Order = -10)]
        public async Task<IActionResult> DeleteRoomCharacteristicAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

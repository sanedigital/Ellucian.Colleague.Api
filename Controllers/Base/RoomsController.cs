// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Room = Ellucian.Colleague.Dtos.Base.Room;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Room data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RoomsController : BaseCompressedApiController
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IFacilitiesService _institutionService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// RoomsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="roomRepository">Repository of type <see cref="IRoomRepository">IReferenceDataRepository</see></param>
        /// <param name="institutionService">Service of type <see cref="IFacilitiesService">IInstitutionService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor">Interface to action context accessor</param>
        /// <param name="apiSettings"></param>
        public RoomsController(IAdapterRegistry adapterRegistry, IRoomRepository roomRepository,
            IFacilitiesService institutionService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _roomRepository = roomRepository;
            _institutionService = institutionService;
            this._logger = logger;
        }

        #region Rooms
        /// <summary>
        /// Retrieves all Rooms
        /// </summary>
        /// <returns>All <see cref="Dtos.Base.Room">Room building codes, codes, and descriptions.</see></returns>
        /// <note>Room is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/rooms", 1, false, Name = "GetRooms")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoomsAsync()
        {
            try
            {
                var bypassCache = false;
                if ((Request != null) && (Request.GetTypedHeaders().CacheControl != null))
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                var roomCollection = await _roomRepository.GetRoomsAsync(bypassCache);

                // Get the right adapter for the type mapping
                var roomDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Room, Room>();

                // Map the room entity to the program DTO
                return roomCollection.Select(room => roomDtoAdapter.MapToType(room)).ToList();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Your previous session has expired and is no longer valid.";
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///  Retrieves all rooms.
        /// </summary>
        /// <returns>All <see cref="Dtos.Room3">Rooms.</see></returns>
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/rooms", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmRoomsV6", IsEedmSupported = true)]
        public async Task<IActionResult> GetHedmRooms3Async(Paging page)
        {
            var bypassCache = false;
            if ((Request != null) && (Request.GetTypedHeaders().CacheControl != null))
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _institutionService.GetRooms3Async(page.Offset, page.Limit, bypassCache);
              
                AddEthosContextProperties(
                  await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Room3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all rooms.
        /// </summary>
        /// <param name="page">Paging offset and limit.</param>
        /// <param name="criteria">Filter Criteria, includes building, roomTypes.</param>
        /// <returns>All <see cref="Dtos.Room3">Rooms.</see></returns>
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Room3))]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/rooms", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmRooms", IsEedmSupported = true)]
        public async Task<IActionResult> GetHedmRooms4Async(Paging page, QueryStringFilter criteria)
        {
            string building = string.Empty;
            string roomType = string.Empty;

            var bypassCache = false;
            if ((Request != null) && (Request.GetTypedHeaders().CacheControl != null))
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var criteriaObj = GetFilterObject<Dtos.Room3>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    building = criteriaObj.BuildingGuid != null ? criteriaObj.BuildingGuid.Id : string.Empty;
                    var roomTypesArray = criteriaObj.RoomTypes != null ? ConvertRoomTypesTypeListToStringList(criteriaObj.RoomTypes) : new List<string>();
                    if (roomTypesArray != null && roomTypesArray.Any())
                    {
                        if (roomTypesArray.Count() > 1)
                        {
                            return new PagedActionResult<IEnumerable<Dtos.Room3>>(new List<Dtos.Room3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                        }
                        roomType = roomTypesArray[0];
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Room3>>(new List<Dtos.Room3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                
                var pageOfItems = await _institutionService.GetRooms4Async(page.Offset, page.Limit, bypassCache, building, roomType);
               
                AddEthosContextProperties(
                  await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Room3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves a room by Id.
        /// </summary>
        /// <returns>A <see cref="Dtos.Room3">Room.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/rooms/{id}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmRoomById", IsEedmSupported = true)]
        [HeaderVersionRoute("/rooms/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmRoomByIdV6", IsEedmSupported = true)]
        public async Task<ActionResult<Room3>> GetHedmRoomById2Async(string id)
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
                AddEthosContextProperties(
                 await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { id }));
                return await _institutionService.GetRoomById3Async(id);
                
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Creates a Room.
        /// </summary>
        /// <param name="room"><see cref="Room">Room</see> to create</param>
        /// <returns>Newly created <see cref="Room">Room</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/rooms", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRoomV6")]
        [HeaderVersionRoute("/rooms", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRoomV10")]
        public IActionResult PostRoom([FromBody] Dtos.Room room)
        {
            //Create is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///  Updates a Room.
        /// </summary>
        /// <param name="id">Id of the Room to update</param>
        /// <param name="room"><see cref="Room">Room</see> to create</param>
        /// <returns>Updated <see cref="Room">Room</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/rooms/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRoomV6")]
        [HeaderVersionRoute("/rooms/{id}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRoomV10")]
        public IActionResult PutRoom([FromRoute] string id, [FromBody] Dtos.Room room)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///     Delete (DELETE) an existing Room
        /// </summary>
        /// <param name="id">Id of the Room to delete</param>
        [HttpDelete]
        [Route("/rooms/{id}", Name = "DeleteRoom", Order = -10)]
        public IActionResult DeleteRoom([FromRoute] string id)
        {
            //Delete is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Building-Wings

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all Building Wings
        /// </summary>
        /// <returns>All <see cref="Dtos.BuildingWing">BuildingWings.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/building-wings", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetBuildingWings", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.BuildingWing>>> GetBuildingWingsAsync()
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
                var buildingWings = await _institutionService.GetBuildingWingsAsync(bypassCache);

                if (buildingWings != null && buildingWings.Any())
                {
                    AddEthosContextProperties(await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              buildingWings.Select(a => a.Id).ToList()));
                }

                return Ok(await _institutionService.GetBuildingWingsAsync(bypassCache));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves an Building Wing by guid.
        /// </summary>
        /// <returns>A <see cref="Dtos.BuildingWing">BuildingWings.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/building-wings/{guid}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetBuildingWingsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.BuildingWing>> GetBuildingWingsByGuidAsync(string guid)
        {
            try
            {
                AddEthosContextProperties(
                   await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _institutionService.GetBuildingWingsByGuidAsync(guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a Building Wing.
        /// </summary>
        /// <param name="buildingWings"><see cref="Dtos.BuildingWing">Building Wing</see> to update</param>
        /// <returns>Newly updated <see cref="Dtos.BuildingWing">Building Wing</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/building-wings/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutBuildingWingsV6")]
        public async Task<ActionResult<Dtos.BuildingWing>> PutBuildingWingsAsync([FromBody] Dtos.BuildingWing buildingWings)
        {
            //Create is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a Building Wing.
        /// </summary>
        /// <param name="buildingWings"><see cref="Dtos.BuildingWing">Building Wings</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.BuildingWing">Building Wings</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/building-wings", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostBuildingWingsV6")]
        public async Task<ActionResult<Dtos.BuildingWing>> PostBuildingWingsAsync([FromBody] Dtos.BuildingWing buildingWings)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Building Wings
        /// </summary>
        /// <param name="guid">Guid of the Building Wings to delete</param>
        [HttpDelete]
        [Route("/building-wings/{guid}", Name = "DeleteBuildingWings", Order = -10)]
        public async Task<ActionResult<Dtos.BuildingWing>> DeleteBuildingWingsAsync(string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Qapi        

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Check for room availability for a given date range, start and end time, and frequency
        /// </summary>
        /// <param name="request">Room availability request</param>
        /// <returns>Collection of Room DTO objects</returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPost]
        
        [HeaderVersionRoute("/qapi/rooms", 6, false, RouteConstants.HedtechIntegrationRoomAvailabilityQapiMediaTypeFormat, Name = "QueryRoomAvailabilityByPostV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Room2>>> QueryAvailableRoomsByPost3Async(RoomsAvailabilityRequest2 request)
        {

            var validationResult = ValidateRoomsAvailabilityRequest2(request);
            if (validationResult != null)
            {
                return Ok(validationResult);
            }

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

                var roomsReturn = await _institutionService.CheckRoomAvailability3Async(request);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(
                    await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        roomsReturn.Select(i => i.Id).ToList()));

                return Created("", roomsReturn);
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException aex)
            {
                _logger.LogError(aex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(aex));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Check for room availability for a given date range, start and end time, and frequency
        /// </summary>
        /// <param name="request">Room availability request</param>
        /// <returns>Collection of Room DTO objects</returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPost]
        [HeaderVersionRoute("/qapi/rooms", "10", false, RouteConstants.HedtechIntegrationRoomAvailabilityQapiMediaTypeFormat, Name = "QueryRoomAvailabilityByPostV10", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Room3>>> QueryAvailableRoomsByPost4Async(RoomsAvailabilityRequest3 request)
        {

            var validationResult = ValidateRoomsAvailabilityRequest3(request);
            if (validationResult != null)
            {
                return Ok(validationResult);
            }

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

                var roomsReturn = await _institutionService.CheckRoomAvailability4Async(request, bypassCache);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(
                    await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        roomsReturn.Select(i => i.Id).ToList()));

                return Ok(roomsReturn);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NoContent);
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException aex)
            {
                _logger.LogError(aex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(aex));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        ///  Check for room availability for a given date range, start and end time, and frequency.   Return GUID
        /// </summary>
        /// <returns>A <see cref="Dtos.RoomsMinimumResponse">RoomsMinimumResponse.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPost]
        [HeaderVersionRoute("/qapi/rooms", 6, false, RouteConstants.HedtechIntegrationRoomMinimumQapiMediaTypeFormat, Name = "QueryRoomsMinimumByPostV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<RoomsMinimumResponse>>> QueryRoomsMinimumByPostAsync(RoomsAvailabilityRequest2 request)
        {
            var validationResult = ValidateRoomsAvailabilityRequest2(request);
            if (validationResult != null)
            {
                return Ok(validationResult);
            }

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
                var roomsReturn = await _institutionService.GetRoomsMinimumAsync(request);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(
                    await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        roomsReturn.Select(i => i.Id).ToList()));

                return Ok(roomsReturn);
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException aex)
            {
                _logger.LogError(aex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(aex));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        #endregion

        #region Helper Methods

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Validate RoomsAvailabilityRequest2 DTO
        /// </summary>
        private IActionResult ValidateRoomsAvailabilityRequest2(RoomsAvailabilityRequest2 request)
        {
            if ((request == null) || (request.Recurrence == null))
                throw new ArgumentNullException("RoomsAvailabilityRequest",
                    "Must provide a recurrence");
            if ((request.Recurrence.TimePeriod != null) &&
                ((request.Recurrence.TimePeriod.StartOn == DateTimeOffset.MinValue) ||
                 (request.Recurrence.TimePeriod.EndOn == DateTimeOffset.MinValue)))
                throw new ArgumentNullException("RoomsAvailabilityRequest.Recurrence.TimePeriod",
                    "Must provide both a start and end date");
            if (request.Recurrence.TimePeriod.StartOn > request.Recurrence.TimePeriod.EndOn)
                throw new ArgumentNullException("RoomsAvailabilityRequest.Recurrence.TimePeriod",
                    "Start date must be prior to the end date");
            if (request.Recurrence.RepeatRule == null)
                throw new ArgumentNullException("RoomsAvailabilityRequest.Recurrence.RepeatRule",
                    "Must provide a repeat rule");
            if ((request.Site != null) && (request.Site.Id == null))
                throw new ArgumentNullException("RoomsAvailabilityRequest.Site",
                    "If providing a site, then id is required");
            if ((request.Building != null) && (request.Building.Id == null))
                throw new ArgumentNullException("RoomsAvailabilityRequest.Building",
                    "If providing a building, then id is required");
            if (request.Occupancies == null || !(request.Occupancies.Any()))
                throw new ArgumentNullException("RoomsAvailabilityRequest.Occupancies.NotSpecified",
                    "Occupancies cannot be null or empty.");
            if (request.Occupancies.Any(o => o.MaximumOccupancy <= 0))
                throw new ArgumentNullException(
                    "RoomsAvailabilityRequest.Occupancies If providing a occupancy, then MaximumOccupancy is required");
            if (request.Occupancies != null &&
                (request.Occupancies.Any(x => x.RoomLayoutType != RoomLayoutType2.Default)))
                throw new ArgumentException(
                    "RoomsAvailabilityRequest.Occupancies.Type Occupancies type must be 'Default'.");
            if (request.RoomType == null)
                throw new ArgumentNullException("RoomsAvailabilityRequest.RoomTypes", "Must provide a roomType");
            if ((request.RoomType != null) && !(request.RoomType.Any()))
                throw new ArgumentNullException("RoomsAvailabilityRequest.RoomTypes", "Must provide a roomType");

            foreach (var roomType in request.RoomType)
            {
                if (roomType.RoomTypesGuid != null && string.IsNullOrEmpty(roomType.RoomTypesGuid.Id))
                    throw new ArgumentNullException("RoomsAvailabilityRequest.RoomTypes", "If providing a roomType detail must provide a detail id.");
            }

            switch (request.Recurrence.RepeatRule.Type)
            {
                case FrequencyType2.Daily:
                    var repeatRuleDaily = (RepeatRuleDaily)request.Recurrence.RepeatRule;
                    if (repeatRuleDaily == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleDaily",
                            "Unable to determine Recurrence.RepeatRule");
                    if ((repeatRuleDaily != null) && (repeatRuleDaily.Ends != null) &&
                        ((repeatRuleDaily.Ends.Date == null) && (repeatRuleDaily.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleDaily",
                            "If providing a value for ends, must provide either a date or repetition value");
                    break;
                case FrequencyType2.Weekly:
                    var repeatRuleWeekly = (RepeatRuleWeekly)request.Recurrence.RepeatRule;
                    if (repeatRuleWeekly == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleWeekly",
                            "Unable to determine Recurrence.RepeatRule");
                    if ((repeatRuleWeekly.Ends != null) &&
                        ((repeatRuleWeekly.Ends.Date == null) && (repeatRuleWeekly.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleWeekly",
                            "If providing a value for ends, must provide either a date or repetition value");
                    break;
                case FrequencyType2.Monthly:
                    var repeatRuleMonthly = (RepeatRuleMonthly)request.Recurrence.RepeatRule;
                    if ((repeatRuleMonthly) == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly",
                            "Unable to determine Recurrence.RepeatRule");
                    if (repeatRuleMonthly.RepeatBy == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly",
                            "Must provide a repeatBy rule");
                    if ((repeatRuleMonthly.RepeatBy.DayOfMonth == 0) && (repeatRuleMonthly.RepeatBy.DayOfWeek == null))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly",
                            "Must provide either a value for dayOfMonth or dayOfWeek");
                    if ((repeatRuleMonthly.Ends != null) &&
                        ((repeatRuleMonthly.Ends.Date == null) && (repeatRuleMonthly.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly",
                            "If providing a value for ends, must provide either a date or repetition value");
                    break;
                case FrequencyType2.Yearly:
                    var repeatRuleYearly = (RepeatRuleYearly)request.Recurrence.RepeatRule;
                    if ((repeatRuleYearly) == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleYearly",
                            "Unable to determine Recurrence.RepeatRule");
                    if ((repeatRuleYearly.Ends != null) &&
                        ((repeatRuleYearly.Ends.Date == null) && (repeatRuleYearly.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleYearly",
                            "If providing a value for ends, must provide either a date or repetition value");
                    break;
                default:
                    return CreateHttpResponseException("Unable to validate recurrence repeat rule.");
            }

            return null;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Validate RoomsAvailabilityRequest3 DTO
        /// </summary>
        private IActionResult ValidateRoomsAvailabilityRequest3(RoomsAvailabilityRequest3 request)
        {
            if ((request == null) || (request.Recurrence == null))
                throw new ArgumentNullException("RoomsAvailabilityRequest",
                    "Must provide a recurrence");
            if ((request.Recurrence.TimePeriod != null)
                && (request.Recurrence.TimePeriod.StartOn == DateTimeOffset.MinValue))
                throw new ArgumentNullException("RoomsAvailabilityRequest.Recurrence.TimePeriod",
                    "Must provide a start date");
            if ((request.Recurrence.TimePeriod != null)
            && (request.Recurrence.TimePeriod.EndOn == null))
                throw new ArgumentNullException("RoomsAvailabilityRequest.Recurrence.TimePeriod",
                    "Must provide an endOn");
            if ((request.Recurrence.TimePeriod.EndOn != DateTimeOffset.MinValue)
                && (request.Recurrence.TimePeriod.StartOn > request.Recurrence.TimePeriod.EndOn))
                throw new ArgumentNullException("RoomsAvailabilityRequest.Recurrence.TimePeriod",
                    "Start date must be prior to the end date");
            if (request.Recurrence.RepeatRule == null)
                throw new ArgumentNullException("RoomsAvailabilityRequest.Recurrence.RepeatRule",
                    "Must provide a repeat rule");
            if ((request.Site != null) && (request.Site.Id == null))
                throw new ArgumentNullException("RoomsAvailabilityRequest.Site",
                    "If providing a site, then id is required");
            if ((request.Building != null) && (request.Building.Id == null))
                throw new ArgumentNullException("RoomsAvailabilityRequest.Building",
                    "If providing a building, then id is required");
            if ((request.Occupancies != null) && (request.Occupancies.Any(o => o.MaximumOccupancy <= 0)))
                throw new ArgumentNullException(
                    "RoomsAvailabilityRequest.Occupancies If providing a occupancy, then MaximumOccupancy is required");

            if (request.RoomType != null)
            {
                foreach (var roomType in request.RoomType)
                {
                    if (roomType.RoomTypesGuid != null && string.IsNullOrEmpty(roomType.RoomTypesGuid.Id))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RoomTypes", "If providing a roomType detail must provide a detail id.");
                }
            }

            switch (request.Recurrence.RepeatRule.Type)
            {
                case FrequencyType2.Daily:
                    var repeatRuleDaily = (RepeatRuleDaily)request.Recurrence.RepeatRule;
                    if (repeatRuleDaily == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleDaily",
                            "Unable to determine Recurrence.RepeatRule");
                    if ((repeatRuleDaily != null) && (repeatRuleDaily.Ends != null) &&
                        ((repeatRuleDaily.Ends.Date == null) && (repeatRuleDaily.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleDaily",
                            "If providing a value for ends, must provide either a date or repetition value");
                    break;
                case FrequencyType2.Weekly:
                    var repeatRuleWeekly = (RepeatRuleWeekly)request.Recurrence.RepeatRule;
                    if (repeatRuleWeekly == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleWeekly",
                            "Unable to determine Recurrence.RepeatRule");
                    if ((repeatRuleWeekly.Ends != null) &&
                        ((repeatRuleWeekly.Ends.Date == null) && (repeatRuleWeekly.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleWeekly",
                            "If providing a value for ends, must provide either a date or repetition value");
                    break;
                case FrequencyType2.Monthly:
                    var repeatRuleMonthly = (RepeatRuleMonthly)request.Recurrence.RepeatRule;
                    if ((repeatRuleMonthly) == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly",
                            "Unable to determine Recurrence.RepeatRule");
                    if (repeatRuleMonthly.RepeatBy == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly",
                            "Must provide a Monthly repeatBy rule");
                    if ((repeatRuleMonthly.RepeatBy.DayOfMonth == 0) && (repeatRuleMonthly.RepeatBy.DayOfWeek == null))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly",
                            "Must provide either a value for dayOfMonth or dayOfWeek");
                    if ((repeatRuleMonthly.Ends != null) &&
                        ((repeatRuleMonthly.Ends.Date == null) && (repeatRuleMonthly.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly",
                            "If providing a value for ends, must provide either a date or repetition value");
                    break;
                case FrequencyType2.Yearly:
                    var repeatRuleYearly = (RepeatRuleYearly)request.Recurrence.RepeatRule;
                    if ((repeatRuleYearly) == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleYearly",
                            "Unable to determine Recurrence.RepeatRule");
                    if ((repeatRuleYearly.Ends != null) &&
                        ((repeatRuleYearly.Ends.Date == null) && (repeatRuleYearly.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleYearly",
                            "If providing a value for ends, must provide either a date or repetition value");
                    break;
                default:
                    return CreateHttpResponseException("Unable to validate recurrence repeat rule.");
            }
            return null;
        }
        #endregion

        /// <summary>
        /// Convert list of GUID object Ids to list of strings
        /// </summary>
        /// <param name="roomTypeList">Guid Object of Ids</param>
        /// <returns>List of strings</returns>

        public List<string> ConvertRoomTypesTypeListToStringList(IEnumerable<RoomType> roomTypeList)
        {
            var retval = new List<string>();
            if (roomTypeList != null & roomTypeList.Any())
            {
                foreach (var roomType in roomTypeList)
                {
                    if (roomType.Type != null)
                    {
                        retval.Add(roomType.Type.ToString());
                    }

                }

            }
            return retval;
        }
    }
}

// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Controller for Room Types
    /// </summary>
     [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RoomTypesController : BaseCompressedApiController
    {
        private readonly IRoomTypesService _roomTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RoomTypeController class.
        /// </summary>
        /// <param name="roomTypesService">Service of type <see cref="IRoomTypesService">IRoomTypesService</see></param>
        /// <param name="logger">Interface to Logger</param>    
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RoomTypesController(IRoomTypesService roomTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _roomTypesService = roomTypesService;
            this._logger = logger;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Room Types
        /// </summary>
        /// <returns>All <see cref="Dtos.RoomTypes">RoomTypes.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/room-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmRoomTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.RoomTypes>>> GetRoomTypesAsync()
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
                var roomTypes = await _roomTypesService.GetRoomTypesAsync(bypassCache);

                if (roomTypes != null && roomTypes.Any())
                {
                    AddEthosContextProperties(await _roomTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _roomTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              roomTypes.Select(a => a.Id).ToList()));
                }

                return Ok(roomTypes);
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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Room Type by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.RoomTypes">RoomTypes.</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/room-types/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRoomTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.RoomTypes>> GetRoomTypeByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                var type = await _roomTypesService.GetRoomTypesByGuidAsync(id);

                if (type != null)
                {

                    AddEthosContextProperties(await _roomTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _roomTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { type.Id }));
                }
                
                return type;
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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a RoomTypes.
        /// </summary>
        /// <param name="roomTypes"><see cref="Dtos.RoomTypes">RoomTypes</see> to update</param>
        /// <returns>Newly updated <see cref="Dtos.RoomTypes">RoomTypes</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/room-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRoomTypeV6")]
        public ActionResult<Dtos.RoomTypes> PutRoomType([FromBody] Dtos.RoomTypes roomTypes)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a RoomTypes.
        /// </summary>
        /// <param name="roomTypes"><see cref="Dtos.RoomTypes">RoomTypes</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.RoomTypes">RoomTypes</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/room-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRoomTypeV6")]
        public ActionResult<Dtos.RoomTypes> PostRoomType([FromBody] Dtos.RoomTypes roomTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing RoomTypes
        /// </summary>
        /// <param name="id">Id of the RoomTypes to delete</param>
        [HttpDelete]
        //"Schemas", action = "GetSchemas"
        //UrlParameter.Optional, selectedSchema3 = UrlParameter.Optiona
        //"Schemas", action = "PutSchemas"
        //"Schemas", action = "PostSchemas"
        //"Schemas", action = "DeleteSchemas"
        //"EthosApiBuilder", action = "GetAlternativeRouteOrNotAcceptable"
        //"Schemas", action = "GetSchemas"
        //true, isAdministrative = true
        //UrlParameter.Optional
        //UrlParameter.Optional, selectedSchema3 = UrlParameter.Optiona
        //"EthosApiBuilder", action = "GetAlternativeRouteOrNotAcceptable"
        [HeaderVersionRoute("/room-types/{id}", "*", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultDeleteRoomType")]
        public ActionResult<Dtos.RoomTypes> DeleteRoomType(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

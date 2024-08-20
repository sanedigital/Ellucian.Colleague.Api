// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
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
using System.Net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Building data.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class BuildingsController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IFacilitiesService _institutionService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// BuildingsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Reference data repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="institutionService">Service of type <see cref="IFacilitiesService">IInstitutionService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BuildingsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IFacilitiesService institutionService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _institutionService = institutionService;
            this._logger = logger;
        }

        //[CacheControlFilter(MaxAgeHours = 1, Public = true, Revalidate = true)]
        /// <summary>
        /// Retrieves all Buildings.
        /// </summary>
        /// <returns>All <see cref="Building">Building codes and descriptions.</see></returns>
        /// <note>This request supports anonymous access. The BUILDINGS entity in Colleague must have public access enabled for this endpoint to function anonymously. See :ref:`anonymousapis` for additional information.</note>
        [HttpGet]
        [HeaderVersionRoute("/buildings", 1, false, Name = "GetBuildings")]
        public async Task<ActionResult<IEnumerable<Building>>> GetBuildingsAsync()
        {
            try
            {
                var buildingCollection = await _referenceDataRepository.BuildingsAsync();

                // Get the right adapter for the type mapping
                var buildingDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Building, Building>();

                // Map the building entity to the building DTO
                var buildingDtoCollection = new List<Building>();
                foreach (var bldg in buildingCollection)
                {
                    buildingDtoCollection.Add(buildingDtoAdapter.MapToType(bldg));
                }

                return Ok(buildingDtoCollection.OrderBy(s => s.Description));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Your previous session has expired and is no longer valid.";
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString() + ex.StackTrace);
                throw;
            }
        }

        //[CacheControlFilter(MaxAgeHours = 1, Public = true, Revalidate = true)]
        /// <summary>
        /// Retrieves all Building Types.
        /// </summary>
        /// <returns>All <see cref="BuildingType">Building Type codes and descriptions.</see></returns>
        /// <note>This request supports anonymous access. The BUILDING.TYPES (CORE.VALCODES) valcode in Colleague must have public access enabled for this endpoint to function anonymously. See :ref:`anonymousapis` for additional information.</note>
        [HttpGet]
        [HeaderVersionRoute("/building-types", 1, true, Name = "GetBuildingTypes")]
        public IEnumerable<BuildingType> GetBuildingTypes()
        {
            var buildingTypes = _referenceDataRepository.BuildingTypes;
            var buildingTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.BuildingType, BuildingType>();
            var buildingTypeDtoCollection = new List<BuildingType>();
            foreach (var bldgType in buildingTypes)
            {
                buildingTypeDtoCollection.Add(buildingTypeDtoAdapter.MapToType(bldgType));
            }
            return buildingTypeDtoCollection.OrderBy(s => s.Code);
        }

        ///// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        ///// <summary>
        ///// Retrieves all buildings.
        ///// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        ///// </summary>
        ///// <returns>All <see cref="Building">Buildings.</see></returns>
        //[HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        //[ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        //[Authorize]
        //[HttpGet]
        //[HeaderVersionRoute("/buildings", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmBuildings", IsEedmSupported = true)]
        //public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Building3>>> GetHedmBuildings3Async([FromUri] string mapVisibility = "")
        //{
        //    bool bypassCache = false;
        //    if (Request.GetTypedHeaders().CacheControl != null)
        //    {
        //        if (Request.GetTypedHeaders().CacheControl.NoCache)
        //        {
        //            bypassCache = true;
        //        }
        //    }
        //    string mapVisibilityFilter = string.Empty;
        //    if (!string.IsNullOrEmpty(mapVisibility))
        //    {
        //        var criteriaValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(mapVisibility);

        //        foreach (var value in criteriaValues)
        //        {
        //            switch (value.Key.ToLower())
        //            {
        //                case "mapvisibility":
        //                    mapVisibilityFilter = string.IsNullOrWhiteSpace(value.Value) ? string.Empty : value.Value;
        //                    break;
        //                default:
        //                    throw new ArgumentException(string.Concat("Invalid filter value received: ", value.Key));
        //            }
        //        }
        //    }
        //    try
        //    {
        //        var items = await _institutionService.GetBuildings3Async(bypassCache, mapVisibilityFilter);

        //        AddEthosContextProperties(
        //            await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
        //            await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
        //                items.Select(i => i.Id).ToList()));

        //        return items;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.ToString());
        //        return CreateHttpResponseException(ex.Message);
        //    }
        //}

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all buildings.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="mapVisibility"></param>
        /// <returns>All <see cref="Building">Buildings.</see></returns>
        [Authorize]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("mapVisibility", typeof(Dtos.Filters.MapVisibilityFilter))]
        [HeaderVersionRoute("/buildings", 11, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmBuildingsV11")]
        public async Task<ActionResult<IEnumerable<Dtos.Building3>>> GetHedmBuildings3Async(QueryStringFilter mapVisibility)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            string mapVisibilityFilter = string.Empty;
            BuildingMapVisibility visibility = BuildingMapVisibility.NotSet;

            try
            {
                var mapVisibilityObj = GetFilterObject<Dtos.Filters.MapVisibilityFilter>(_logger, "mapVisibility");
                if (mapVisibilityObj != null)
                {
                    visibility = mapVisibilityObj.Visibility;
                    mapVisibilityFilter = !visibility.Equals(BuildingMapVisibility.NotSet) ? visibility.ToString() : null;
                }

                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.Building3>();

                var items = await _institutionService.GetBuildings3Async(bypassCache, mapVisibilityFilter);

                AddEthosContextProperties(
                    await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }

        }


        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all buildings.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Building">Buildings.</see></returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [Authorize]
        [HttpGet]
        [HeaderVersionRoute("/buildings", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmBuildingsV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.Building2>>> GetHedmBuildings2Async()
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
                var items = await _institutionService.GetBuildings2Async(bypassCache);

                AddEthosContextProperties(
                    await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a building by ID.
        /// </summary>
        /// <returns>A <see cref="Dto.Building2">Building.</see></returns>
        [Authorize]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/buildings/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmBuildingByIdV6")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Building2>> GetHedmBuildingByIdAsync(string id)
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
                var building = await _institutionService.GetBuilding2Async(id);

                if (building != null)
                {

                    AddEthosContextProperties(await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { building.Id }));
                }

                return building;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a building by ID.
        /// </summary>
        /// <returns>A <see cref="Dto.Building3">Building.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [Authorize]
        [HttpGet]
        [HeaderVersionRoute("/buildings/{id}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmBuildingById")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Building3>> GetHedmBuildingById2Async(string id)
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
                var building = await _institutionService.GetBuilding3Async(id);

                if (building != null)
                {

                    AddEthosContextProperties(await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { building.Id }));
                }

                return building;
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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>        
        /// Creates a Building.
        /// </summary>
        /// <param name="building"><see cref="Dtos.Building2">Building</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.Building2">Building</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/buildings", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostBuildingV11")]
        [HeaderVersionRoute("/buildings", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostBuildingV6")]
        public async Task<ActionResult<Dtos.Building2>> PostBuildingAsync([FromBody] Dtos.Building2 building)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Updates a Building.
        /// </summary>
        /// <param name="id">Id of the Building to update</param>
        /// <param name="building"><see cref="Dtos.Building2">Building</see> to create</param>
        /// <returns>Updated <see cref="Dtos.Building2">Building</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/buildings/{id}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutBuildingV11")]
        [HeaderVersionRoute("/buildings/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutBuildingV6")]
        public async Task<ActionResult<Dtos.Building2>> PutBuildingAsync([FromRoute] string id, [FromBody] Dtos.Building2 building)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing Building
        /// </summary>
        /// <param name="id">Id of the Building to delete</param>
        [HttpDelete]
        [Route("/buildings/{id}", Name = "DeleteBuilding", Order = -10)]
        public async Task<ActionResult<Dtos.Building2>> DeleteBuildingAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

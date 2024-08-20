// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Address data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AddressesController : BaseCompressedApiController
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly IAddressService _addressService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the AddressesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="addressService">Service of type <see cref="IAddressService">IAddressService</see></param>
        /// <param name="addressRepository">Repository of type <see cref="IAddressRepository">IAddressRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AddressesController(IAdapterRegistry adapterRegistry, IAddressService addressService, IAddressRepository addressRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _addressService = addressService;
            _addressRepository = addressRepository;
            this._logger = logger;
        }

        #region Get Methods
        /// <summary>
        /// Get all current addresses for a person
        /// </summary>
        /// <param name="personId">Person to get addresses for</param>
        /// <returns>List of Address Objects <see cref="Ellucian.Colleague.Dtos.Base.Address">Address</see></returns>
        /// <accessComments>Authenticated users can retrieve their own address information; authenticated users with the VIEW.PERSON.INFORMATION or EDIT.VENDOR.BANKING.INFORMATION permission code can retrieve address information for others.</accessComments>
        [HttpGet]
        [HeaderVersionRoute("/addresses/{personId}", 2, true, Name = "GetAddressByPersonId2Async", Order = -9)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.Address>>> GetPersonAddresses2Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                _logger.LogError("Invalid personId parameter while retrieving person's addresses");
                return CreateHttpResponseException("The personId is required to retrieve person's addresses.", HttpStatusCode.BadRequest);
            }
            try
            {
                var addressDtoCollection = await _addressService.GetPersonAddresses2Async(personId);
                return Ok(addressDtoCollection);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = string.Format("Timeout exception occurred while retrieving addresses for the person {0}", personId);
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string message = "Either user is not self or does not have appropriate permissions to retrieve addresses for given person";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = "An exception occurred while retrieving addresses for the given person Id";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Get a list of Addresses from a list of Person keys
        /// </summary>
        /// <param name="criteria">Address Query Criteria including PersonIds list.</param>
        /// <returns>List of Address Objects <see cref="Ellucian.Colleague.Dtos.Base.Address">Address</see></returns>
        /// <accessComments>User must have VIEW.ADDRESS permission or search for their own address(es)</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/addresses", 1, true, Name = "GetAddressesByIdList")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.Address>>> QueryAddressesAsync(AddressQueryCriteria criteria)
        {
            if (criteria.PersonIds == null || criteria.PersonIds.Count() <= 0)
            {
                _logger.LogError("Invalid personIds parameter: null or empty.");
                return CreateHttpResponseException("No person IDs provided.", HttpStatusCode.BadRequest);
            }
            try
            {
                await _addressService.QueryAddressPermissionAsync(criteria.PersonIds);

                var addressDtoCollection = new List<Ellucian.Colleague.Dtos.Base.Address>();
                var addressCollection = _addressRepository.GetPersonAddressesByIds(criteria.PersonIds.ToList());
                // Get the right adapter for the type mapping
                var addressDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Address, Ellucian.Colleague.Dtos.Base.Address>();
                // Map the Address entity to the Address DTO
                foreach (var address in addressCollection)
                {
                    addressDtoCollection.Add(addressDtoAdapter.MapToType(address));
                }

                return addressDtoCollection;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message, "QueryAddresses error");
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "QueryAddresses error");
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Read (GET) a Address using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired Address</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress })]
        [HeaderVersionRoute("/addresses/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAddressByGuidV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Addresses>> GetAddressByGuidAsync(string guid)
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
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                var address = await _addressService.GetAddressesByGuidAsync(guid);

                if (address != null)
                {

                    AddEthosContextProperties(await _addressService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { address.Id }));
                }


                return address;
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
        /// Read (GET) a Address using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired Address</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress })]
        [HeaderVersionRoute("/addresses/{guid}", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAddressByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Addresses>> GetAddressByGuid2Async(string guid)
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
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                var address = await _addressService.GetAddressesByGuid2Async(guid);

                if (address != null)
                {

                    AddEthosContextProperties(await _addressService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { address.Id }));
                }
                return address;
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
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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

        /// <summary>
        /// Get all addresses with paging
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/addresses", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAddressesV6", IsEedmSupported = true)]
        public async Task<IActionResult> GetAddressesAsync(Paging page)
        {
            try
            {
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                var bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _addressService.GetAddressesAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _addressService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Addresses>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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

        /// <summary>
        /// Get all addresses with paging
        /// </summary>
        /// <param name="page"></param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <returns></returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/addresses", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAddressesDefault", IsEedmSupported = true)]
        public async Task<IActionResult> GetAddresses2Async(Paging page, QueryStringFilter personFilter)
        {
            try
            {
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                var bypassCache = false;
                if (!StringValues.IsNullOrEmpty(Request.Headers.CacheControl))
                {
                    
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Addresses>>(new List<Dtos.Addresses>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }

                var pageOfItems = await _addressService.GetAddresses2Async(page.Offset, page.Limit, personFilterValue, bypassCache);

                AddEthosContextProperties(await _addressService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));
                return new PagedActionResult<IEnumerable<Dtos.Addresses>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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

        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Update a Address Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Guid to Address in Colleague</param>
        /// <param name="address"><see cref="Dtos.Addresses">Address</see> to update</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        [HttpPut, PermissionsFilter(BasePermissionCodes.UpdateAddress)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPut]
        [HeaderVersionRoute("/addresses/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAddressV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Addresses>> PutAddressAsync([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Addresses address)
        {
            if (address == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null request body",
                    IntegrationApiUtility.GetDefaultApiError("The request body must be specified in the request.")));
            }
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (id != address.Id)
            {
                return CreateHttpResponseException(new IntegrationApiException("Incorrect id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID in the URL doesn't match the GUID in the body of the request.")));
            }
            try
            {
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _addressService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _addressService.ImportExtendedEthosData(await ExtractExtendedData(await _addressService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var addressReturn = await _addressService.PutAddressesAsync(id,
                    await PerformPartialPayloadMerge(address, async () => await _addressService.GetAddressesByGuidAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return addressReturn;
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
        /// Update a Address Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Guid to Address in Colleague</param>
        /// <param name="address"><see cref="Dtos.Addresses">Address</see> to update</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, PermissionsFilter(BasePermissionCodes.UpdateAddress)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPut]
        [HeaderVersionRoute("/addresses/{id}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAddressV11.1.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Addresses>> PutAddress2Async([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Addresses address)
        {
            if (address == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null request body",
                    IntegrationApiUtility.GetDefaultApiError("The request body must be specified in the request.")));
            }
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (id != address.Id)
            {
                return CreateHttpResponseException(new IntegrationApiException("Incorrect id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID in the URL doesn't match the GUID in the body of the request.")));
            }
            try
            {
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _addressService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _addressService.ImportExtendedEthosData(await ExtractExtendedData(await _addressService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var addressReturn = await _addressService.PutAddresses2Async(id,
                    await PerformPartialPayloadMerge(address, async () => await _addressService.GetAddressesByGuid2Async(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return addressReturn;
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
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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

        #endregion

        #region Post Methods


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Create a Address Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="address"><see cref="Dtos.Addresses">Address</see> to create</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/addresses", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAddressV11.1.0")]
        [HeaderVersionRoute("/addresses", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAddressV6")]
        public async Task<ActionResult<Dtos.Addresses>> PostAddressAsync([FromBody] Dtos.Addresses address)
        {        
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));        
        }
        #endregion

        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Delete an existing Address in Colleague 
        /// </summary>
        /// <param name="id">Unique ID representing the Address to delete</param>
        [HttpDelete]
        [Route("/addresses/{id}", Name = "DefaultDeleteAddresses", Order = -10)]
        public async Task<IActionResult> DeleteAddressAsync(string id)
        {
            try
            {
                await _addressService.DeleteAddressesAsync(id);
                return NoContent();
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

        #endregion
    }
}

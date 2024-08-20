// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Web.Http;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;

using Ellucian.Web.Security;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonVisas data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class PersonVisasController : BaseCompressedApiController
    {
        private readonly IPersonVisasService _personVisasService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        #region ..ctor
        /// <summary>
        /// Initializes a new instance of the PersonVisasController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="personVisasService">Service of type <see cref="IPersonVisasService">IPersonVisasService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to action context accessor</param>
        /// <param name="apiSettings"></param>
        public PersonVisasController(IAdapterRegistry adapterRegistry, IPersonVisasService personVisasService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _personVisasService = personVisasService;
            _adapterRegistry = adapterRegistry;
            this._logger = logger;
        }
        #endregion

        #region GET

        /// <summary>
        /// Gets all person visa information
        /// </summary>
        /// <param name="page"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPersonVisa, BasePermissionCodes.UpdateAnyPersonVisa })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ServiceFilter(typeof(FilteringFilter))]
        [ValidateQueryStringFilter(new string[] { "person" }, false, true)]
        [HttpGet]
        [HeaderVersionRoute("/person-visas", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllPersonVisasAsync", IsEedmSupported = true)]
        public async Task<IActionResult> GetAllPersonVisasAsync(Paging page, [FromQuery] string person = "")
        {
            try
            {
                _personVisasService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                if (person == null)
                {
                    return new PagedActionResult<IEnumerable<Dtos.PersonVisa>>(new List<Dtos.PersonVisa>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                }

                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _personVisasService.GetAllAsync(page.Offset, page.Limit, person, bypassCache);

                AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonVisa>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Gets all person visa information
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPersonVisa, BasePermissionCodes.UpdateAnyPersonVisa })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PersonVisa))]
        [HttpGet]
        [HeaderVersionRoute("/person-visas", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAllPersonVisasAsync", IsEedmSupported = true)]
        public async Task<IActionResult> GetAllPersonVisas2Async(Paging page, QueryStringFilter criteria)
        {
            string person = string.Empty, visaTypeDetail = string.Empty, visaTypeCategory = string.Empty;

            try
            {
                _personVisasService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
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

                var criteriaObj = GetFilterObject<Dtos.PersonVisa>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    person = criteriaObj.Person != null ? criteriaObj.Person.Id : string.Empty;
                    visaTypeDetail = (criteriaObj.VisaType != null && criteriaObj.VisaType.Detail != null) ? criteriaObj.VisaType.Detail.Id : string.Empty;
                    visaTypeCategory = criteriaObj.VisaType != null ? 
                        (criteriaObj.VisaType.VisaTypeCategory.Equals(Dtos.VisaTypeCategory.Immigrant) ? 
                        "immigrant" : (criteriaObj.VisaType.VisaTypeCategory.Equals(Dtos.VisaTypeCategory.NonImmigrant) ? 
                        "nonImmigrant" : string.Empty)) : string.Empty;
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.PersonVisa>>(new List<Dtos.PersonVisa>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                
                var pageOfItems = await _personVisasService.GetAll2Async(page.Offset, page.Limit, person, visaTypeCategory, visaTypeDetail, bypassCache);

                AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonVisa>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves a person visa by ID.
        /// </summary>
        /// <param name="id">Id of person visa to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.PersonVisa">person visa</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPersonVisa, BasePermissionCodes.UpdateAnyPersonVisa })]
        [HttpGet]
        [HeaderVersionRoute("/person-visas/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonVisaByIdV6", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PersonVisa>> GetPersonVisaByIdAsync([FromRoute] string id)
        {            
            try
            {
                _personVisasService.ValidatePermissions(GetPermissionsMetaData());
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
                    throw new ArgumentNullException("Must provide an id for person-visas.");
                }

                var personVisa = await _personVisasService.GetPersonVisaByIdAsync(id);

                if (personVisa != null)
                {

                    AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { personVisa.Id }));
                }


                return personVisa;
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Retrieves a person visa by ID.
        /// </summary>
        /// <param name="id">Id of person visa to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.PersonVisa">person visa</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter)),CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPersonVisa, BasePermissionCodes.UpdateAnyPersonVisa })]
        [HttpGet]
        [HeaderVersionRoute("/person-visas/{id}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonVisaById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PersonVisa>> GetPersonVisaById2Async([FromRoute] string id)
        {
            try
            {
                _personVisasService.ValidatePermissions(GetPermissionsMetaData());
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
                    // throw new ArgumentNullException("Must provide an id for person-visas.");
                    var exception = new IntegrationApiException();
                    exception.AddError(new IntegrationApiError("Missing.Guid",
                            "The GUID must be provided in the URL.",
                                "Please provide a GUID in the URL."));
                    throw exception;
                }

                var personVisa = await _personVisasService.GetPersonVisaById2Async(id);

                if (personVisa != null)
                {

                    AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { personVisa.Id }));
                }


                return personVisa;
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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

        #region POST
        /// <summary>
        /// Creates a PersonVisa.
        /// </summary>
        /// <param name="personVisa"><see cref="Dtos.PersonVisa">personVisa</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.PersonVisa">PersonVisa</see></returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.UpdateAnyPersonVisa)]
        [HttpPost]
        [HeaderVersionRoute("/person-visas", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "POSTPostPersonVisaV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonVisa>> PostPersonVisaAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.PersonVisa personVisa)
        {
            try
            {
                _personVisasService.ValidatePermissions(GetPermissionsMetaData());
                if (personVisa == null)
                {
                    CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentNullException("Must provide a person-visas.")));
                }

                if (string.IsNullOrEmpty(personVisa.Id))
                {
                    CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentNullException("Must provide an id for person-visas in request body.")));
                }

                if (personVisa.Id != Guid.Empty.ToString())
                {
                    CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentNullException("person-visas", "Nil GUID must be used in POST operation.")));
                }
                
                TryValidateModel(personVisa);
                
                //call import extend method that needs the extracted extension data and the config
                await _personVisasService.ImportExtendedEthosData(await ExtractExtendedData(await _personVisasService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var personVisaReturn  = await _personVisasService.PostPersonVisaAsync(personVisa);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personVisaReturn.Id }));

                return Ok(personVisaReturn);

            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a PersonVisa.
        /// </summary>
        /// <param name="personVisa"><see cref="Dtos.PersonVisa">personVisa</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.PersonVisa">PersonVisa</see></returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.UpdateAnyPersonVisa)]
        [HttpPost]
        [HeaderVersionRoute("/person-visas", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "POSTPostPersonVisaV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonVisa>> PostPersonVisa2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.PersonVisa personVisa)
        {
            try
            {
                _personVisasService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _personVisasService.ImportExtendedEthosData(await ExtractExtendedData(await _personVisasService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var personVisaReturn = await _personVisasService.PostPersonVisa2Async(personVisa);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personVisaReturn.Id }));

                return personVisaReturn;

            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (RepositoryException e)
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
        #endregion

        #region PUT
        /// <summary>
        /// Updates a person visa.
        /// </summary>
        /// <param name="id">id of the personVisa to update</param>
        /// <param name="personVisa"><see cref="Dtos.PersonVisa">personVisa</see> to create</param>
        /// <returns>Updated <see cref="Dtos.PersonVisa">Dtos.PersonVisa</see></returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.UpdateAnyPersonVisa)]
        [HttpPut]
        [HeaderVersionRoute("/person-visas/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PUTPersonVisaV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonVisa>> PutPersonVisaAsync([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonVisa personVisa)
        {
            try
            {
                _personVisasService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Must provide an id for person-visas.");
                }

                if (personVisa == null)
                {
                    throw new ArgumentNullException("Must provide a person-visas.");
                }

                if (string.IsNullOrEmpty(personVisa.Id))
                {
                    throw new ArgumentNullException("Must provide an id for person-visas in request body.");
                }

                if (!id.Equals(personVisa.Id, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException("id in URL is not the same as in request body.");
                }

                //get Data Privacy List
                var dpList = await _personVisasService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _personVisasService.ImportExtendedEthosData(await ExtractExtendedData(await _personVisasService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var personVisaReturn = await _personVisasService.PutPersonVisaAsync(id,
                  await PerformPartialPayloadMerge(personVisa, async () => await _personVisasService.GetPersonVisaByIdAsync(id),
                  dpList, _logger));

                AddEthosContextProperties(dpList,
                    await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return personVisaReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a person visa.
        /// </summary>
        /// <param name="id">id of the personVisa to update</param>
        /// <param name="personVisa"><see cref="Dtos.PersonVisa">personVisa</see> to create</param>
        /// <returns>Updated <see cref="Dtos.PersonVisa">Dtos.PersonVisa</see></returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.UpdateAnyPersonVisa)]
        [HttpPut]
        [HeaderVersionRoute("/person-visas/{id}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PUTPersonVisaV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonVisa>> PutPersonVisa2Async([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonVisa personVisa)
        {
            try
            {
                _personVisasService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _personVisasService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _personVisasService.ImportExtendedEthosData(await ExtractExtendedData(await _personVisasService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var personVisaReturn = await _personVisasService.PutPersonVisa2Async(id,
                  await PerformPartialPayloadMerge(personVisa, async () => await _personVisasService.GetPersonVisaById2Async(id),
                  dpList, _logger));

                AddEthosContextProperties(dpList,
                    await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return personVisaReturn;
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }           
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (RepositoryException e)
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

        #endregion

        #region DELETE
        /// <summary>
        /// Delete (DELETE) an existing PersonVisa
        /// </summary>
        /// <param name="id">id of the PersonVisa to delete</param>
        [HttpDelete]
        [Route("/person-visas/{id}", Name = "DefaultDeletePersonVisa", Order = -10)]
        public async Task<IActionResult> DeletePersonVisaAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

      
    }
}

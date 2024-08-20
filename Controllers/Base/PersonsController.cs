// Copyright 2014-2024 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Http.ModelBinding;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to person data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonsController : BaseCompressedApiController
    {
        private readonly IPersonRestrictionTypeService _personRestrictionTypeService;
        private readonly IPersonService _personService;
        private readonly IEmergencyInformationService _emergencyInformationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private const string permissionExceptionMessage = "User does not have permission to access the requested information";
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";
        /// <summary>
        /// Initializes a new instance of the PersonsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="personService">Service of type <see cref="IPersonService">IPersonService</see></param>
        /// <param name="personRestrictionTypeService">Service of type <see cref="IPersonRestrictionTypeService">IPersonRestrictionTypeService</see></param>
        /// <param name="emergencyInformationService">Service of type <see cref="IEmergencyInformationService">IEmergencyInformationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonsController(IAdapterRegistry adapterRegistry, IPersonService personService, IPersonRestrictionTypeService personRestrictionTypeService, IEmergencyInformationService emergencyInformationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _personService = personService;
            _personRestrictionTypeService = personRestrictionTypeService;
            _emergencyInformationService = emergencyInformationService;
            this._logger = logger;
        }

        #region Get Methods

        /// <summary>
        /// Gets a subset of person credentials.
        /// </summary>
        /// <returns>The requested <see cref="Dtos.PersonCredential">PersonsCredentials</see></returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/persons-credentials", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPersonCredentialsV6", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonCredentialsAsync(Paging page)
        {
            try
            {
                _personService.ValidatePermissions(GetPermissionsMetaData());
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

                var pageOfItems = await _personService.GetAllPersonCredentialsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonCredential>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Gets a subset of person credentials.
        /// </summary>
        /// <returns>The requested <see cref="Dtos.PersonCredential">PersonsCredentials</see></returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/persons-credentials", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPersonCredentialsV8", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonCredentials2Async(Paging page)
        {
            try
            {
                _personService.ValidatePermissions(GetPermissionsMetaData());
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

                var pageOfItems = await _personService.GetAllPersonCredentials2Async(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonCredential2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
            catch (RepositoryException rex)
            {
                _logger.LogError(rex.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(rex));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Gets a subset of person credentials.
        /// </summary>
        /// <returns>The requested <see cref="Dtos.PersonCredential2">PersonsCredentials</see></returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PersonCredential2))]
        [HeaderVersionRoute("/persons-credentials", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPersonCredentialsV11", IsEedmSupported = true, Order = -5)]
        public async Task<IActionResult> GetPersonCredentials3Async(Paging page, QueryStringFilter criteria)
        {
            try
            {
                _personService.ValidatePermissions(GetPermissionsMetaData());
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

                var criteriaObject = GetFilterObject<Dtos.PersonCredential2>(_logger, "criteria");

                //we need to validate the credentials
                if (criteriaObject.Credentials != null && criteriaObject.Credentials.Any())
                {
                    foreach (var cred in criteriaObject.Credentials)
                    {
                        switch (cred.Type)
                        {
                            case CredentialType2.BannerId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerId' is not supported."))));
                            case CredentialType2.BannerSourcedId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerSourcedId' is not supported."))));
                            case CredentialType2.BannerUdcId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUdcId' is not supported."))));
                            case CredentialType2.BannerUserName:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUserName' is not supported."))));
                            case CredentialType2.Ssn:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'ssn' is not supported."))));
                            case CredentialType2.Sin:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'sin' is not supported."))));
                        }
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.PersonCredential2>>(new List<Dtos.PersonCredential2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>();


                var personCredentials = criteriaObject.Credentials != null ?

                criteriaObject.Credentials.ToString() : string.Empty;

                var pageOfItems = await _personService.GetAllPersonCredentials3Async(page.Offset, page.Limit, criteriaObject, bypassCache);

                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonCredential2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
            catch (RepositoryException rex)
            {
                _logger.LogError(rex.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(rex));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Gets a subset of person credentials.
        /// </summary>
        /// <returns>The requested <see cref="Dtos.PersonCredential3">PersonsCredentials</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PersonCredential3))]
        [HeaderVersionRoute("/persons-credentials", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmPersonCredentials", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonCredentials4Async(Paging page, QueryStringFilter criteria)
        {
            try
            {
                _personService.ValidatePermissions(GetPermissionsMetaData());
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

                var criteriaObject = GetFilterObject<Dtos.PersonCredential3>(_logger, "criteria");

                //we need to validate the credentials
                if (criteriaObject.Credentials != null && criteriaObject.Credentials.Any())
                {
                    foreach (var cred in criteriaObject.Credentials)
                    {
                        switch (cred.Type)
                        {
                            case Credential3Type.BannerId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerId' is not supported."))));
                            case Credential3Type.BannerSourcedId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerSourcedId' is not supported."))));
                            case Credential3Type.BannerUdcId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUdcId' is not supported."))));
                            case Credential3Type.BannerUserName:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUserName' is not supported."))));
                            case Credential3Type.Ssn:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'ssn' is not supported."))));
                            case Credential3Type.Sin:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'sin' is not supported."))));
                        }
                    }
                }

                //Discussed with Vickie & Kelly to add exception similar to alternateCredentials. If only type is provided & value is not provided.
                if (criteriaObject.Credentials != null && criteriaObject.Credentials.Any())
                {
                    foreach (var cred in criteriaObject.Credentials)
                    {
                        if (cred.Type != null && string.IsNullOrEmpty(cred.Value))
                        {
                            return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentials.type", string.Concat("credentials.type.id filter requires credentials.value filter."))));
                        }
                    }
                }

                //we need to validate the alternative credentials
                if (criteriaObject.AlternativeCredentials != null && criteriaObject.AlternativeCredentials.Any())
                {
                    foreach (var cred in criteriaObject.AlternativeCredentials)
                    {
                        if (cred.Type != null && string.IsNullOrEmpty(cred.Value))
                        {
                            return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("alternativeCredentials.type", string.Concat("alternativeCredentials.type.id filter requires alternativeCredentials.value filter."))));
                        }
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.PersonCredential3>>(new List<Dtos.PersonCredential3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _personService.GetAllPersonCredentials4Async(page.Offset, page.Limit, criteriaObject, bypassCache);

                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonCredential3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Get a subset of person's data, including only their credentials.
        /// </summary>
        /// <param name="id">A global identifier of a person.</param>
        /// <returns>The requested <see cref="Dtos.PersonCredential">PersonsCredentials</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [HttpGet]
        [HeaderVersionRoute("/persons-credentials/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPersonCredentialsByGuidV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonCredential>> GetPersonCredentialByGuidAsync(string id)
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
                _personService.ValidatePermissions(GetPermissionsMetaData());
                var credential = await _personService.GetPersonCredentialByGuidAsync(id);

                if (credential != null)
                {

                    AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { credential.Id }));
                }


                return credential;

            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateNotFoundException("person", id);
            }
        }

        /// <summary>
        /// Get a subset of person's data, including only their credentials.
        /// </summary>
        /// <param name="id">A global identifier of a person.</param>
        /// <returns>The requested <see cref="Dtos.PersonCredential2">PersonsCredentials</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [HttpGet]
        [HeaderVersionRoute("/persons-credentials/{id}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPersonCredentialsByGuidV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonCredential2>> GetPersonCredential2ByGuidAsync(string id)
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
                _personService.ValidatePermissions(GetPermissionsMetaData());
                var credential = await _personService.GetPersonCredential2ByGuidAsync(id);

                if (credential != null)
                {

                    AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { credential.Id }));
                }


                return credential;

            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateNotFoundException("person", id);
            }
        }

        /// <summary>
        /// Get a subset of person's data, including only their credentials.
        /// </summary>
        /// <param name="id">A global identifier of a person.</param>
        /// <returns>The requested <see cref="Dtos.PersonCredential2">PersonsCredentials</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [HeaderVersionRoute("/persons-credentials/{id}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPersonCredentialsByGuidV11", IsEedmSupported = true, Order = -5)]
        public async Task<ActionResult<Dtos.PersonCredential2>> GetPersonCredential3ByGuidAsync(string id)
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
                _personService.ValidatePermissions(GetPermissionsMetaData());
                var credential = await _personService.GetPersonCredential3ByGuidAsync(id);

                if (credential != null)
                {

                    AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { credential.Id }));
                }


                return credential;

            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateNotFoundException("person", id);
            }
        }

        #region persons credentials v11.1.10
        /// <summary>
        /// Get a subset of person's data, including only their credentials.
        /// </summary>
        /// <param name="id">A global identifier of a person.</param>
        /// <returns>The requested <see cref="Dtos.PersonCredential3">PersonsCredentials</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [HeaderVersionRoute("/persons-credentials/{id}", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmPersonCredentialsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonCredential3>> GetPersonCredential4ByGuidAsync(string id)
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
                _personService.ValidatePermissions(GetPermissionsMetaData());
                var credential = await _personService.GetPersonCredential4ByGuidAsync(id);

                if (credential != null)
                {

                    AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { credential.Id }));
                }


                return credential;

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

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Retrieves all active person restriction types for a student
        /// </summary>
        /// <returns>PersonRestrictionType object for a student.</returns>
        [Obsolete("Obsolete as of HeDM Version 4.5, use person-holds API instead.")]
        [HttpGet]
        [HeaderVersionRoute("/persons/{guid}/restriction-types", 1, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetActivePersonRestrictionTypes")]
        public async Task<ActionResult<List<Dtos.GuidObject>>> GetActivePersonRestrictionTypesAsync(string guid)
        {
            try
            {
                return await _personRestrictionTypeService.GetActivePersonRestrictionTypesAsync(guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Get a person's profile information.
        /// </summary>
        /// <param name="personId">Id of the person to get</param>
        /// <returns>The requested <see cref="Dtos.Base.Profile">Profile</see></returns>
        /// <accessComments>
        /// Only the current user or their proxies can view their profile.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}", 1, false, "application/vnd.ellucian-person-profile.v1+json", Name = "GetPersonProfile")]
        public async Task<ActionResult<Dtos.Base.Profile>> GetProfileAsync(string personId)
        {
            try
            {
                bool useCache = true;
                if (Request != null && Request.Headers != null && Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        useCache = false;
                    }
                }
                return await _personService.GetProfileAsync(personId, useCache);
            }
            catch (ArgumentNullException anex)
            {
                _logger.LogError(anex.ToString());
                return CreateHttpResponseException(anex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving person profile information.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateNotFoundException("person", personId);
            }
        }

        /// <summary>
        /// Get only the information required to create a proxy
        /// </summary>
        /// <param name="personId">Id of the person to get</param>
        /// <returns>The requested <see cref="Dtos.Base.PersonProxyDetails"/> information </returns>
        /// <accessComments>
        /// Any logged in user can access their own proxy's details.
        /// A user with the permission ADD.ALL.HR.PROXY is considered as an admin and can access details of any employee's proxy.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}", 1, false, RouteConstants.EllucianProxyUserFormat, Name = "GetPersonProxyDetails")]
        public async Task<ActionResult<Dtos.Base.PersonProxyDetails>> GetPersonProxyDetailsAsync(string personId)
        {
            try
            {
                return await _personService.GetPersonProxyDetailsAsync(personId);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException anex)
            {
                _logger.LogError(anex.ToString());
                return CreateHttpResponseException("Person ID cannot be null", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.ToString());
                return CreateHttpResponseException(permissionExceptionMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all the emergency information for a person.
        /// </summary>
        /// <param name="personId">Pass in a person ID</param>
        /// <returns>Returns all the emergency information for the specified person</returns>
        /// <accessComments>
        /// Only the current user can get their emergency information.
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.16. Use GetEmergencyInformation2Async instead.")]
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/emergency-information", 1, false, Name = "GetEmergencyInformation")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Base.EmergencyInformation>> GetEmergencyInformationAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                return CreateHttpResponseException("Invalid person ID", HttpStatusCode.BadRequest);
            }

            try
            {
                return await _emergencyInformationService.GetEmergencyInformationAsync(personId);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception)
            {
                return CreateNotFoundException("person", personId);
            }
        }

        /// <summary>
        /// Get all the emergency information for a person.
        /// </summary>
        /// <param name="personId">Pass in a person ID</param>
        /// <returns>Returns all the emergency information for the specified person</returns>
        /// <accessComments>
        /// The current user can access all their emergency information.
        ///
        /// Permissions can be granted to allow full or partial access to others' information:
        /// * Users with VIEW.PERSON.EMERGENCY.CONTACTS can see others' Emergency Contacts and Opt Out status
        /// * Users with VIEW.PERSON.HEALTH.CONDITIONS can see others' Health Conditions
        /// * Users with VIEW.PERSON.OTHER.EMERGENCY.INFORMATION can see others' Insurance Information, Hospital Preference, and Additional Information
        ///
        /// Users with one or more of the above permissions can see other properties not listed as well (such as name and person ID).
        ///
        /// If the requested person has a privacy code restriction, the requesting user will need the corresponding privacy code access in addition
        /// to the permission(s).
        ///
        /// Lacking one or more of the above permissions or privacy code access will result in some properties being null and the
        /// "X-Content-Restricted" header being set to "partial"
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/emergency-information", 2, true, Name = "GetEmergencyInformation2Async")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Base.EmergencyInformation>> GetEmergencyInformation2Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                return CreateHttpResponseException("Invalid person ID", HttpStatusCode.BadRequest);
            }

            try
            {
                var privacyWrappedEmerInfo = await _emergencyInformationService.GetEmergencyInformation2Async(personId);
                var emergencyInformation = privacyWrappedEmerInfo.Dto;
                if (privacyWrappedEmerInfo.HasPrivacyRestrictions)
                {
                    SetContentRestrictedHeader("partial");
                }
                return emergencyInformation;
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception)
            {
                return CreateNotFoundException("person", personId);
            }
        }

        #endregion

        #region Get Methods for HEDM v6

        /// <summary>
        /// Get a person.
        ///  If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="guid">Guid of the person to get</param>
        /// <returns>The requested <see cref="Person2">Person</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/persons/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPerson", IsEedmSupported = true)]
        public async Task<ActionResult<Person2>> GetPerson2ByGuidAsync(string guid)
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
                AddEthosContextProperties(
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _personService.GetPerson2ByGuidAsync(guid, bypassCache);
            }
            catch (PermissionsException e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
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
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Return a list of Person objects based on selection criteria.
        /// </summary>
        /// <param name="page">Person page to retrieve</param>
        /// <param name="title">Person Title Contains ...title...</param>
        /// <param name="firstName">Person First Name equal to</param>
        /// <param name="middleName">Person Middle Name equal to</param>
        /// <param name="lastNamePrefix">Person Last Name begins with 'code...'</param>
        /// <param name="lastName">Person Last Name equal to</param>
        /// <param name="pedigree">Person Suffixe Contains ...pedigree... (guid)</param>
        /// <param name="preferredName">Person Preferred Name equal to (guid)</param>
        /// <param name="role">Person Role equal to (guid)</param>
        /// <param name="credentialType">Person Credential Type (colleagueId or ssn)</param>
        /// <param name="credentialValue">Person Credential equal to</param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of Person2 <see cref="Dtos.Person2"/> objects representing matching persons</returns>
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [ValidateQueryStringFilter(new string[] { "title", "firstName", "middleName", "lastNamePrefix", "lastName", "pedigree", "preferredName", "role", "credentialType", "credentialValue", "personFilter" }, false, true)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/persons", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmFilteredPerson", IsEedmSupported = true)]
        public async Task<IActionResult> GetPerson2Async(Paging page, [FromQuery] string title = "", [FromQuery] string firstName = "", [FromQuery] string middleName = "",
            [FromQuery] string lastNamePrefix = "", [FromQuery] string lastName = "", [FromQuery] string pedigree = "", [FromQuery] string preferredName = "",
            [FromQuery] string role = "", [FromQuery] string credentialType = "", [FromQuery] string credentialValue = "", [FromQuery] string personFilter = "")
        {
            if (title == null || firstName == null || middleName == null || lastNamePrefix == null || lastName == null || pedigree == null || preferredName == null || role == null || credentialType == null || credentialValue == null || personFilter == null)
            {
                return new PagedActionResult<IEnumerable<Dtos.Person2>>(new List<Dtos.Person2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            string criteria = string.Concat(title, firstName, middleName, lastNamePrefix, lastName, pedigree, preferredName,
                    role, credentialType, credentialValue, personFilter);

            //valid query parameter but empty argument
            if ((!string.IsNullOrEmpty(criteria)) && (string.IsNullOrEmpty(criteria.Replace("\"", ""))))
            {
                return new PagedActionResult<IEnumerable<Dtos.Person2>>(new List<Dtos.Person2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            //validate the crendentials
            if (!string.IsNullOrEmpty(credentialType) && string.IsNullOrEmpty(credentialValue))
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialValue", "credentialValue is required when requesting a credentialType")));
            }
            if (string.IsNullOrEmpty(credentialType) && !string.IsNullOrEmpty(credentialValue))
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", "credentialType is required when requesting a credentialValue")));
            }
            if (!string.IsNullOrEmpty(credentialType))
            {
                var credentialTypeValue = GetEnumFromEnumMemberAttribute(credentialType, Dtos.EnumProperties.CredentialType.NotSet);
                if (credentialTypeValue == Dtos.EnumProperties.CredentialType.NotSet)
                {
                    return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialValue", "credentialType is not valid")));
                }
                if (credentialTypeValue == Dtos.EnumProperties.CredentialType.Sin || credentialTypeValue == Dtos.EnumProperties.CredentialType.Ssn || credentialTypeValue == Dtos.EnumProperties.CredentialType.BannerId
                            || credentialTypeValue == Dtos.EnumProperties.CredentialType.BannerSourcedId || credentialTypeValue == Dtos.EnumProperties.CredentialType.BannerUdcId || credentialTypeValue == Dtos.EnumProperties.CredentialType.BannerSourcedId ||
                            credentialTypeValue == Dtos.EnumProperties.CredentialType.BannerUserName)
                {
                    return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("Credential Type filter of '", credentialTypeValue, "' is not supported."))));
                }
            }
            //validate role
            if (!string.IsNullOrEmpty(role))
            {
                var roleTypeValue = GetEnumFromEnumMemberAttribute(role, Dtos.EnumProperties.PersonRoleType.NotSet);
                if (roleTypeValue == Dtos.EnumProperties.PersonRoleType.NotSet)
                    return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("role", string.Concat(role, " is not a valid role."))));
            }
            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.Person2>>(new List<Dtos.Person2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            try
            {
                _personService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _personService.GetPerson2NonCachedAsync(page.Offset, page.Limit, bypassCache,
                    title, firstName, middleName, lastNamePrefix, lastName, pedigree, preferredName,
                    role, credentialType, credentialValue, personFilter);

                AddEthosContextProperties(
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Person2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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

        #region Get Methods for HEDM v8

        /// <summary>
        /// Get a person.
        ///  If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="guid">Guid of the person to get</param>
        /// <returns>The requested <see cref="Person3">Person</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/persons/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPersonByGuidV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Person3>> GetPersonByGuid3Async(string guid)
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
                AddEthosContextProperties(
                     await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         new List<string>() { guid }));

                return await _personService.GetPerson3ByGuidAsync(guid, bypassCache);
            }
            catch (PermissionsException e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
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
        /// Return a list of Person objects based on selection criteria.
        /// </summary>
        /// <param name="page">Person page to retrieve</param>
        /// <param name="criteria">Person search criteria in JSON format
        /// <param name="personFilter">Person filter search criteria</param>
        /// <param name="preferredName">Person filter search criteria</param>
        /// Can contain:
        /// title - Person title equal to
        /// firstName - Person First Name equal to
        /// middleName - Person Middle Name equal to
        /// lastNamePrefix - Person Last Name begins with 'code...'
        /// lastName- -Person Last Name equal to
        /// pedigree- -Person Suffix Contains pedigree (guid)
        /// preferredName - Person Preferred Name equal to (guid)
        /// role - Person Role equal to (guid)
        /// credentialType - Person Credential Type (colleagueId - SSN/SIN not supported here)
        /// credentialValue - Person Credential equal to
        /// personFilter - Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of Person2 <see cref="Dtos.Person3"/> objects representing matching persons</returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.PersonFilter))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter))]
        [QueryStringFilterFilter("preferredName", typeof(Dtos.Filters.PreferredNameFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/persons", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPersonV8", IsEedmSupported = true)]
        public async Task<IActionResult> GetPerson3Async(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter, QueryStringFilter preferredName)
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
                _personService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    personFilterValue = personFilterObj.personFilterId != null ? personFilterObj.personFilterId : null;
                }

                string preferredNameValue = string.Empty;
                var preferredNameFilterObj = GetFilterObject<Dtos.Filters.PreferredNameFilter>(_logger, "preferredName");
                if (preferredNameFilterObj != null)
                {
                    preferredNameValue = preferredNameFilterObj.PreferredName != null ? preferredNameFilterObj.PreferredName : null;
                }

                var criteriaObj = GetFilterObject<Dtos.Filters.PersonFilter>(_logger, "criteria");

                if (criteriaObj != null)
                {
                    //check for old filter and convert them to new format
                    if (!string.IsNullOrEmpty(criteriaObj.Title) || !string.IsNullOrEmpty(criteriaObj.FirstName) || !string.IsNullOrEmpty(criteriaObj.MiddleName) || !string.IsNullOrEmpty(criteriaObj.LastName)
                        || !string.IsNullOrEmpty(criteriaObj.LastNamePrefix) || !string.IsNullOrEmpty(criteriaObj.Pedigree))
                    {
                        var personName = new PersonNameDtoProperty();
                        personName.Title = criteriaObj.Title != null ? criteriaObj.Title : string.Empty;
                        personName.FirstName = criteriaObj.FirstName != null ? criteriaObj.FirstName : string.Empty;
                        personName.MiddleName = criteriaObj.MiddleName != null ? criteriaObj.MiddleName : string.Empty;
                        personName.LastNamePrefix = criteriaObj.LastNamePrefix != null ? criteriaObj.LastNamePrefix : string.Empty;
                        personName.LastName = criteriaObj.LastName != null ? criteriaObj.LastName : string.Empty;
                        personName.Pedigree = criteriaObj.Pedigree != null ? criteriaObj.Pedigree : string.Empty;
                        criteriaObj.PersonNames = new List<PersonNameDtoProperty> { personName };
                    }
                    if (!string.IsNullOrEmpty(criteriaObj.PreferredName))
                    {
                        preferredNameValue = criteriaObj.PreferredName;
                    }
                    if (!string.IsNullOrEmpty(criteriaObj.PersonFilterFilter))
                    {
                        personFilterValue = criteriaObj.PersonFilterFilter;
                    }
                    if (criteriaObj.Role != null)
                    {
                        criteriaObj.Roles = new List<PersonRoleDtoProperty> { new PersonRoleDtoProperty { RoleType = criteriaObj.Role } };
                    }
                    if ((criteriaObj.CredentialType != null) || !string.IsNullOrEmpty(criteriaObj.CredentialValue))
                    {
                        if (!string.IsNullOrEmpty(criteriaObj.CredentialType.ToString()) && string.IsNullOrEmpty(criteriaObj.CredentialValue))
                        {
                            return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialValue", "credentialValue is required when requesting a credentialType")));
                        }
                        if (string.IsNullOrEmpty(criteriaObj.CredentialType.ToString()) && !string.IsNullOrEmpty(criteriaObj.CredentialValue))
                        {
                            return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", "credentialType is required when requesting a credentialValue")));
                        }
                        criteriaObj.Credentials = new List<CredentialDtoProperty2> { new CredentialDtoProperty2 { Type = criteriaObj.CredentialType, Value = criteriaObj.CredentialValue } };
                    }

                    // do filter validation
                    //we need to validate the credentials
                    if (criteriaObj.Credentials != null && criteriaObj.Credentials.Any())
                    {
                        foreach (var cred in criteriaObj.Credentials)
                        {
                            if (cred.Type == CredentialType2.Sin || cred.Type == CredentialType2.Ssn || cred.Type == CredentialType2.BannerId
                                || cred.Type == CredentialType2.BannerSourcedId || cred.Type == CredentialType2.BannerUdcId || cred.Type == CredentialType2.BannerSourcedId ||
                                cred.Type == CredentialType2.BannerUserName || cred.Type == CredentialType2.NotSet)
                            {
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("Credential Type filter of '", cred.Type, "' is not supported."))));
                            }
                            if (!string.IsNullOrEmpty(cred.Type.ToString()) && string.IsNullOrEmpty(cred.Value))
                            {
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialValue", "credentialValue is required when requesting a credentialType")));
                            }
                            if (string.IsNullOrEmpty(cred.Type.ToString()) && !string.IsNullOrEmpty(cred.Value))
                            {
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", "credentialType is required when requesting a credentialValue")));
                            }
                        }
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Person3>>(new List<Dtos.Person3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _personService.GetPerson3NonCachedAsync(page.Offset, page.Limit, bypassCache, criteriaObj, personFilterValue, preferredNameValue);

                AddEthosContextProperties(
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Person3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (JsonReaderException e)
            {
                _logger.LogError(e.ToString());

                return CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                   IntegrationApiUtility.GetDefaultApiError("Error parsing JSON person search request.")));
            }
            catch
                (KeyNotFoundException e)
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

        #region GET METHODS for EEDM V12

        /// <summary>
        /// Get a person.
        ///  If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="guid">Guid of the person to get</param>
        /// <returns>The requested <see cref="Person4">Person</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/persons/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPersonByGuidV12", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Person4>> GetPerson4ByIdAsync(string guid)
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
                AddEthosContextProperties(
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _personService.GetPerson4ByGuidAsync(guid, bypassCache);
            }
            catch (PermissionsException e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
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
        /// Return a list of Person objects based on selection criteria.
        /// </summary>
        /// <param name="page">Person page to retrieve</param>
        /// <param name="personFilter">Person filter search criteria</param>
        /// <param name="criteria">Person search criteria in JSON format
        /// Can contain:
        /// title - Person title equal to
        /// firstName - Person First Name equal to
        /// middleName - Person Middle Name equal to
        /// lastNamePrefix - Person Last Name begins with 'code...'
        /// lastName- -Person Last Name equal to
        /// role - Person Role equal to (guid)
        /// credentialType - Person Credential Type (colleagueId - SSN/SIN not supported here)
        /// credentialValue - Person Credential equal to
        /// personFilter - Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of Person3 <see cref="Dtos.Person3"/> objects representing matching persons</returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Person4))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/persons", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPersonV12", IsEedmSupported = true)]
        public async Task<IActionResult> GetPerson4Async(Paging page, QueryStringFilter personFilter, QueryStringFilter criteria)
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
                _personService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    personFilterValue = personFilterObj.personFilterId != null ? personFilterObj.personFilterId : null;
                }
                var criteriaObject = GetFilterObject<Dtos.Person4>(_logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Person4>>(new List<Dtos.Person4>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                //we need to validate the credentials
                if (criteriaObject.Credentials != null && criteriaObject.Credentials.Any())
                {
                    foreach (var cred in criteriaObject.Credentials)
                    {
                        switch (cred.Type)
                        {
                            case Credential3Type.BannerId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerId' is not supported."))));
                            case Credential3Type.BannerSourcedId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerSourcedId' is not supported."))));
                            case Credential3Type.BannerUdcId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUdcId' is not supported."))));
                            case Credential3Type.BannerUserName:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUserName' is not supported."))));
                            case Credential3Type.Ssn:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'ssn' is not supported."))));
                            case Credential3Type.Sin:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'sin' is not supported."))));
                        }
                    }
                }

                var pageOfItems = await _personService.GetPerson4NonCachedAsync(page.Offset, page.Limit, bypassCache, criteriaObject, personFilterValue);
                AddEthosContextProperties(
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Person4>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e.ToString());

                return CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                   IntegrationApiUtility.GetDefaultApiError("Error parsing JSON person search request.")));
            }
            catch (JsonReaderException e)
            {
                _logger.LogError(e.ToString());

                return CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                   IntegrationApiUtility.GetDefaultApiError("Error parsing JSON person search request.")));
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

        #region GET METHODS for EEDM V12.1.0

        /// <summary>
        /// Get a person.
        ///  If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="guid">Guid of the person to get</param>
        /// <returns>The requested <see cref="Person5">Person</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/persons/{guid}", "12.6.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmPersonByGuid", IsEedmSupported = true, Order = -20)]
        public async Task<ActionResult<Dtos.Person5>> GetPerson5ByIdAsync(string guid)
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
                AddEthosContextProperties(
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _personService.GetPerson5ByGuidAsync(guid, bypassCache);
            }
            catch (PermissionsException e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
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
        /// Return a list of Person objects based on selection criteria.
        /// </summary>
        /// <param name="page">Person page to retrieve</param>
        /// <param name="personFilter">Person filter search criteria</param>
        /// <param name="criteria">Person search criteria in JSON format
        /// Can contain:
        /// title - Person title equal to
        /// firstName - Person First Name equal to
        /// middleName - Person Middle Name equal to
        /// lastNamePrefix - Person Last Name begins with 'code...'
        /// lastName- -Person Last Name equal to
        /// role - Person Role equal to (guid)
        /// credentialType - Person Credential Type (colleagueId - SSN/SIN not supported here)
        /// credentialValue - Person Credential equal to
        /// alternativeCredentialsType - alternativeCredentials Type 
        /// alternativeCredentialsValue -alternativeCredentials equal to
        /// personFilter - Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of Person5 <see cref="Dtos.Person5"/> objects representing matching persons</returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Person5))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/persons", "12.6.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmPerson", IsEedmSupported = true, IsBulkSupported = true, Order = -20)]
        public async Task<IActionResult> GetPerson5Async(Paging page, QueryStringFilter personFilter, QueryStringFilter criteria)
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
                _personService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null && personFilterObj.personFilter != null)
                {
                    personFilterValue = personFilterObj.personFilter.Id != null ? personFilterObj.personFilter.Id : null;
                }

                var criteriaObject = GetFilterObject<Dtos.Person5>(_logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Person5>>(new List<Dtos.Person5>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                //we need to validate the credentials
                if (criteriaObject.Credentials != null && criteriaObject.Credentials.Any())
                {
                    foreach (var cred in criteriaObject.Credentials)
                    {
                        switch (cred.Type)
                        {
                            case Credential3Type.BannerId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerId' is not supported."))));
                            case Credential3Type.BannerSourcedId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerSourcedId' is not supported."))));
                            case Credential3Type.BannerUdcId:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUdcId' is not supported."))));
                            case Credential3Type.BannerUserName:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUserName' is not supported."))));
                            case Credential3Type.Ssn:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'ssn' is not supported."))));
                            case Credential3Type.Sin:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'sin' is not supported."))));
                            case Credential3Type.TaxIdentificationNumber:
                                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentialType", string.Concat("credentials.type filter of 'taxIdentificationNumber' is not supported."))));
                        }
                    }
                }

                //Discussed with Vickie & Kelly to add exception similar to alternateCredentials. If only type is provided & value is not provided.
                if (criteriaObject.Credentials != null && criteriaObject.Credentials.Any())
                {
                    foreach (var cred in criteriaObject.Credentials)
                    {
                        if (cred.Type != null && string.IsNullOrEmpty(cred.Value))
                        {
                            return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("credentials.type", string.Concat("credentials.type.id filter requires credentials.value filter."))));
                        }
                    }
                }

                //we need to validate the alternative credentials
                if (criteriaObject.AlternativeCredentials != null && criteriaObject.AlternativeCredentials.Any())
                {
                    foreach (var cred in criteriaObject.AlternativeCredentials)
                    {
                        if (cred.Type != null && string.IsNullOrEmpty(cred.Value))
                        {
                            return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("alternativeCredentials.type", string.Concat("alternativeCredentials.type.id filter requires alternativeCredentials.value filter."))));
                        }
                    }
                }

                var pageOfItems = await _personService.GetPerson5NonCachedAsync(page.Offset, page.Limit, bypassCache, criteriaObject, personFilterValue);
                AddEthosContextProperties(
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Person5>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e.ToString());

                return CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                   IntegrationApiUtility.GetDefaultApiError("Error parsing JSON person search request.")));
            }
            catch (JsonReaderException e)
            {
                _logger.LogError(e.ToString());

                return CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                   IntegrationApiUtility.GetDefaultApiError("Error parsing JSON person search request.")));
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

        #region Post Methods

        /// <summary>
        /// Create (POST) a new person
        /// </summary>
        /// <param name="person">DTO of the new person</param>
        /// <returns>A person object <see cref="Person2"/> in HeDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.CreatePerson)]
        [HeaderVersionRoute("/persons", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "CreateHedmPersonV6", IsEedmSupported = true)]
        public async Task<ActionResult<Person2>> PostPerson2Async([ModelBinder(typeof(EedmModelBinder))] Person2 person)
        {
            if (person == null)
            {
                return CreateHttpResponseException("Request body must contain a valid Person.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null person id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                _personService.ValidatePermissions(GetPermissionsMetaData());
                // check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(person.CitizenshipStatus, person.CitizenshipCountry, null, null);

                //call import extend method that needs the extracted extension data and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the person
                var personReturn = await _personService.CreatePerson2Async(person);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personReturn.Id }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(csse), HttpStatusCode.Unauthorized);
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
            catch (ConfigurationException e)
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
        /// Create (POST) a new person
        /// </summary>
        /// <param name="person">DTO of the new person</param>
        /// <returns>A person object <see cref="Person3"/> in HeDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.CreatePerson)]
        [HeaderVersionRoute("/persons", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "CreateHedmPersonV8", IsEedmSupported = true)]
        public async Task<ActionResult<Person3>> PostPerson3Async([ModelBinder(typeof(EedmModelBinder))] Person3 person)
        {
            if (person == null)
            {
                return CreateHttpResponseException("Request body must contain a valid Person.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null person id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                _personService.ValidatePermissions(GetPermissionsMetaData());
                // check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(person.CitizenshipStatus, person.CitizenshipCountry, null, null);

                //call import extend method that needs the extracted extension data and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the person
                var personReturn = await _personService.CreatePerson3Async(person);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personReturn.Id }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(csse), HttpStatusCode.Unauthorized);
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
            catch (ConfigurationException e)
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
        /// Create (POST) a new person
        /// </summary>
        /// <param name="person">DTO of the new person</param>
        /// <returns>A person object <see cref="Person4"/> in HeDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.CreatePerson)]
        [HeaderVersionRoute("/persons", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "CreateHedmPersonV12", IsEedmSupported = true, Order = -5)]
        public async Task<ActionResult<Person4>> PostPerson4Async([ModelBinder(typeof(EedmModelBinder))] Person4 person)
        {
            if (person == null)
            {
                return CreateHttpResponseException("Request body must contain a valid Person.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null person id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (person.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentNullException("person", "Nil GUID must be used in POST operation.")));
            }

            try
            {
                _personService.ValidatePermissions(GetPermissionsMetaData());
                // Check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(person.CitizenshipStatus, person.CitizenshipCountry, null, null);

                //call import extend method that needs the extracted extension data and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the person
                var personReturn = await _personService.CreatePerson4Async(person);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personReturn.Id }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(csse), HttpStatusCode.Unauthorized);
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
            catch (ConfigurationException e)
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
        /// Create (POST) a new person for v12.1.0
        /// </summary>
        /// <param name="person">DTO of the new person</param>
        /// <returns>A person object <see cref="Person5"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.CreatePerson)]
        [HeaderVersionRoute("/persons", "12.6.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "CreateHedmPersonV12.3.0", IsEedmSupported = true)]
        public async Task<ActionResult<Person5>> PostPerson5Async([ModelBinder(typeof(EedmModelBinder))] Person5 person)
        {
            if (person == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("person",
                     IntegrationApiUtility.GetDefaultApiError("Request body must contain a valid Person.")));
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null person id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (person.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException(new IntegrationApiException("person",
                    IntegrationApiUtility.GetDefaultApiError("Nil GUID must be used in POST operation.")));
            }

            try
            {
                _personService.ValidatePermissions(GetPermissionsMetaData());
                // Check citizenship fields
                await _personService.CheckCitizenshipfields2(person.CitizenshipStatus, person.CitizenshipCountry, null, null);

                //call import extend method that needs the extracted extension data and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                // Set person gender properties so that service can share ConvertPerson5GenderAsync for put/post
                var personDtoGender = person.GenderType;
                var personDtoGenderMarker = person.GenderMarker;

                //create the person
                var personReturn = await _personService.CreatePerson5Async(person, personDtoGender, personDtoGenderMarker);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personReturn.Id }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(csse), HttpStatusCode.Unauthorized);
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
            catch (ConfigurationException e)
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
        /// Create person credentials.
        /// </summary>
        /// <param name="personCredential"><see cref="Dtos.PersonCredential">PersonCredential</see> to update</param>
        /// <returns>Newly created <see cref="Dtos.PersonCredential">PersonCredential</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/persons-credentials", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmPersonCredentialsV11.1.0")]
        [HeaderVersionRoute("/persons-credentials", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmPersonCredentialsV8")]
        [HeaderVersionRoute("/persons-credentials", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmPersonCredentialsV6")]
        public async Task<ActionResult<Dtos.PersonCredential>> PostPersonCredentialAsync([FromBody] Dtos.PersonCredential personCredential)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods

        /// <summary>
        /// Update (PUT) an existing person
        /// </summary>
        /// <param name="guid">GUID of the person to update</param>
        /// <param name="person">DTO of the updated person</param>
        /// <returns>A Person2 object <see cref="Dtos.Person2"/> in HeDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/persons/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "UpdateHedmPersonV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Person2>> PutPerson2Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Person2 person)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (person == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null person argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                person.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, person.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));


                // do partial merge
                var originalPerson = new Dtos.Person2();
                try
                {
                    originalPerson = await _personService.GetPerson2ByGuidAsync(guid, true);
                }
                catch (KeyNotFoundException)
                {
                    originalPerson = null;
                }

                var mergedPerson = await PerformPartialPayloadMerge(person, originalPerson, dpList, _logger);

                PersonCitizenshipDtoProperty originalPersonCitizenshipStatus = null;
                string originalPersonCitizenshipCountry = null;
                if (originalPerson != null)
                {
                    originalPersonCitizenshipStatus = originalPerson.CitizenshipStatus;
                    originalPersonCitizenshipCountry = originalPerson.CitizenshipCountry;
                }

                // check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(mergedPerson.CitizenshipStatus, mergedPerson.CitizenshipCountry, originalPersonCitizenshipStatus, originalPersonCitizenshipCountry);


                var personReturn = await _personService.UpdatePerson2Async(mergedPerson);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(csse), HttpStatusCode.Unauthorized);
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
            catch (ConfigurationException e)
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
        /// Update (PUT) an existing person
        /// </summary>
        /// <param name="guid">GUID of the person to update</param>
        /// <param name="person">DTO of the updated person</param>
        /// <returns>A Person2 object <see cref="Dtos.Person3"/> in HeDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/persons/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "UpdateHedmPersonV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Person3>> PutPerson3Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Person3 person)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (person == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null person argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                person.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, person.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                var dpList = await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                // do partial merge
                var originalPerson = new Dtos.Person3();
                try
                {
                    originalPerson = await _personService.GetPerson3ByGuidAsync(guid, true);
                }
                catch (KeyNotFoundException)
                {
                    originalPerson = null;
                }

                var mergedPerson = await PerformPartialPayloadMerge(person, originalPerson, dpList, _logger);

                PersonCitizenshipDtoProperty originalPersonCitizenshipStatus = null;
                string originalPersonCitizenshipCountry = null;
                if (originalPerson != null)
                {
                    originalPersonCitizenshipStatus = originalPerson.CitizenshipStatus;
                    originalPersonCitizenshipCountry = originalPerson.CitizenshipCountry;
                }

                // check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(mergedPerson.CitizenshipStatus, mergedPerson.CitizenshipCountry, originalPersonCitizenshipStatus, originalPersonCitizenshipCountry);

                var personReturn = await _personService.UpdatePerson3Async(mergedPerson);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(csse), HttpStatusCode.Unauthorized);
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
            catch (ConfigurationException e)
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
        /// Update (PUT) an existing person
        /// </summary>
        /// <param name="guid">GUID of the person to update</param>
        /// <param name="person">DTO of the updated person</param>
        /// <returns>A Person2 object <see cref="Dtos.Person4"/> in HeDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/persons/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "UpdateHedmPersonV12", IsEedmSupported = true, Order = -5)]
        public async Task<ActionResult<Dtos.Person4>> PutPerson4Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Person4 person)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (person == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null person argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                person.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, person.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));


                // Check for {} sent in for Citizenship Status
                if (person.CitizenshipStatus != null)
                {
                    if (person.CitizenshipStatus.Category == null && (person.CitizenshipStatus.Detail == null || string.IsNullOrEmpty(person.CitizenshipStatus.Detail.Id)))
                    {
                        person.CitizenshipStatus = null;
                    }
                }
                if (string.IsNullOrEmpty(person.CitizenshipCountry)) person.CitizenshipCountry = null;

                // do partial merge
                var originalPerson = new Dtos.Person4();
                try
                {
                    originalPerson = await _personService.GetPerson4ByGuidAsync(guid, true);
                }
                catch (KeyNotFoundException)
                {
                    originalPerson = null;
                }

                var mergedPerson = await PerformPartialPayloadMerge(person, originalPerson, dpList, _logger);

                PersonCitizenshipDtoProperty originalPersonCitizenshipStatus = null;
                string originalPersonCitizenshipCountry = null;
                if (originalPerson != null)
                {
                    originalPersonCitizenshipStatus = originalPerson.CitizenshipStatus;
                    originalPersonCitizenshipCountry = originalPerson.CitizenshipCountry;
                }

                // Check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(mergedPerson.CitizenshipStatus, mergedPerson.CitizenshipCountry, originalPersonCitizenshipStatus, originalPersonCitizenshipCountry);


                //do update with partial logic
                var personReturn = await _personService.UpdatePerson4Async(mergedPerson);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return personReturn;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(csse), HttpStatusCode.Unauthorized);
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
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
            catch (ConfigurationException e)
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
        /// Update (PUT) an existing person for v12.1.0
        /// </summary>
        /// <param name="guid">GUID of the person to update</param>
        /// <param name="person">DTO of the updated person</param>
        /// <returns>A Person2 object <see cref="Dtos.Person5"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/persons/{guid}", "12.6.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "UpdateHedmPersonV12.3.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Person5>> PutPerson5Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Person5 person)
        {
            // For gender with 2 source properties, we need override logic in the service to calculate gender instead
            // of default partial put merge logic.  Save whether or not 2 source properties were explicitly included
            // in the request body or not.
            var updateRequest = GetUpdateRequest();
            var containsGenderMarkerName = ((Newtonsoft.Json.Linq.JObject)updateRequest).ContainsKey("genderMarker");
            var containsGenderName = ((Newtonsoft.Json.Linq.JObject)updateRequest).ContainsKey("gender");

            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null person.id",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (person == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("person",
                    IntegrationApiUtility.GetDefaultApiError("Request body must contain a valid Person.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("person.id",
                    IntegrationApiUtility.GetDefaultApiError("Nil GUID cannot be used in PUT operation.")));
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                person.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, person.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("person.id",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));


                // Check for {} sent in for Citizenship Status
                if (person.CitizenshipStatus != null)
                {
                    if (person.CitizenshipStatus.Category == null && (person.CitizenshipStatus.Detail == null || string.IsNullOrEmpty(person.CitizenshipStatus.Detail.Id)))
                    {
                        person.CitizenshipStatus = null;
                    }
                }
                if (string.IsNullOrEmpty(person.CitizenshipCountry)) person.CitizenshipCountry = null;

                // do partial merge
                var originalPerson = new Dtos.Person5();
                try
                {
                    originalPerson = await _personService.GetPerson5ByGuidAsync(guid, true);
                }
                catch (KeyNotFoundException)
                {
                    originalPerson = null;
                }

                var personDtoGender = person.GenderType;
                var personDtoGenderMarker = person.GenderMarker;
                var mergedPerson = await PerformPartialPayloadMerge(person, originalPerson, dpList, _logger);

                PersonCitizenshipDtoProperty originalPersonCitizenshipStatus = null;
                string originalPersonCitizenshipCountry = null;
                if (originalPerson != null)
                {
                    originalPersonCitizenshipStatus = originalPerson.CitizenshipStatus;
                    originalPersonCitizenshipCountry = originalPerson.CitizenshipCountry;
                }

                // Check citizenship fields
                await _personService.CheckCitizenshipfields2(mergedPerson.CitizenshipStatus, mergedPerson.CitizenshipCountry, originalPersonCitizenshipStatus, originalPersonCitizenshipCountry, guid);

                //do update with partial logic
                var personReturn = await _personService.UpdatePerson5Async(mergedPerson, originalPerson, personDtoGender, personDtoGenderMarker, containsGenderName, containsGenderMarkerName);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(csse), HttpStatusCode.Unauthorized);
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
            catch (ConfigurationException e)
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
        /// Get the request body.  Because gender has 2 source values (gender and genderMarker),
        /// it needs unique override logic beyond what is done by PerformPartialPayloadMerge.
        /// And needs to determine if those property names were explicitly included in the
        /// request body.
        /// </summary>
        /// <returns>request body</returns>
        private object GetUpdateRequest()
        {
            object updateRequest;
            _actionContextAccessor.ActionContext.HttpContext.Items.TryGetValue("PartialInputJsonObject", out updateRequest);
            return updateRequest;
        }

        /// <summary>
        /// Updates certain person profile information: AddressConfirmationDateTime, EmailAddressConfirmationDateTime,
        /// PhoneConfirmationDateTime, EmailAddresses, Personal Phones and Addresses. LastChangedDateTime must match the last changed timestamp on the database
        /// Person record to ensure updates not occurring from two different sources at the same time. If no changes are found, a NotModified Http status code
        /// is returned. If required by configuration, users must be set up with permissions to perform these updates: UPDATE.OWN.EMAIL, UPDATE.OWN.PHONE, and 
        /// UPDATE.OWN.ADDRESS. 
        /// </summary>
        /// <param name="personId">The ID of the person profile to update.</param>
        /// <param name="profile"><see cref="Dtos.Base.Profile">Profile</see> to use to update</param>
        /// <returns>Newly updated <see cref="Dtos.Base.Profile">Profile</see></returns>
        /// <accessComments>
        /// Only the current user can update their profile.
        /// </accessComments>
        [HttpPut]
        [Obsolete("Obsolete as of API 1.16. Use version 2 of this action instead.")]
        [HeaderVersionRoute("/persons/{personId}", 1, false, "application/vnd.ellucian-person-profile.v1+json", Name = "UpdatePersonProfile")]
        public async Task<ActionResult<Dtos.Base.Profile>> PutProfileAsync([FromRoute] string personId, [FromBody] Dtos.Base.Profile profile)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    _logger.LogError("PersonsController-PutProfileAsync: Must provide a person id in the request uri");
                    return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ColleagueWebApiException()));
                }
                if (profile == null)
                {
                    _logger.LogError("PersonsController-PutProfileAsync: Must provide a profile in the request body");
                    return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ColleagueWebApiException()));
                }
                if (string.IsNullOrEmpty(profile.Id))
                {
                    _logger.LogError("PersonsController-PutProfileAsync: Must provide a person Id in the request body");
                    return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ColleagueWebApiException()));
                }
                if (personId != profile.Id)
                {
                    _logger.LogError("PersonsController-PutProfileAsync: PersonID in URL is not the same as in request body");
                    return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ColleagueWebApiException()));
                }

                return await _personService.UpdateProfileAsync(profile);
            }
            catch (PermissionsException permissionException)
            {
                return CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception)
            {
                return CreateHttpResponseException("Unable to update profile information", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates certain person profile information: AddressConfirmationDateTime, EmailAddressConfirmationDateTime,
        /// PhoneConfirmationDateTime, EmailAddresses, Personal Phones and Addresses. LastChangedDateTime must match the last changed timestamp on the database
        /// Person record to ensure updates not occurring from two different sources at the same time. If no changes are found, a NotModified Http status code
        /// is returned. If required by configuration, users must be set up with permissions to perform these updates: UPDATE.OWN.EMAIL, UPDATE.OWN.PHONE, and 
        /// UPDATE.OWN.ADDRESS. 
        /// </summary>
        /// <param name="personId">The ID of the person profile to update.</param>
        /// <param name="profile"><see cref="Dtos.Base.Profile">Profile</see> to use to update</param>
        /// <returns>Newly updated <see cref="Dtos.Base.Profile">Profile</see></returns>
        /// <accessComments>
        /// Only the current user can update their profile.
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/persons/{personId}", 2, false, "application/vnd.ellucian-person-profile.v2+json", Name = "UpdatePersonProfile2")]
        public async Task<ActionResult<Dtos.Base.Profile>> PutProfile2Async([FromRoute] string personId, [FromBody] Dtos.Base.Profile profile)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    _logger.LogError("PersonsController-PutProfile2Async: Must provide a person id in the request uri");
                    return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ColleagueWebApiException()));
                }
                if (profile == null)
                {
                    _logger.LogError("PersonsController-PutProfile2Async: Must provide a profile in the request body");
                    return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ColleagueWebApiException()));
                }
                if (string.IsNullOrEmpty(profile.Id))
                {
                    _logger.LogError("PersonsController-PutProfile2Async: Must provide a person Id in the request body");
                    return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ColleagueWebApiException()));
                }
                if (personId != profile.Id)
                {
                    _logger.LogError("PersonsController-PutProfile2Async: PersonID in URL is not the same as in request body");
                    return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ColleagueWebApiException()));
                }

                return await _personService.UpdateProfile2Async(profile);
            }
            catch (PermissionsException permissionException)
            {
                return CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception)
            {
                return CreateHttpResponseException("Unable to update profile information", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a PersonCredential.
        /// </summary>
        /// <param name="personCredential"><see cref="Dtos.PersonCredential">PersonCredential</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.PersonCredential">PersonCredential</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/persons-credentials/{id}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmPersonCredentialsV11.1.0")]
        [HeaderVersionRoute("/persons-credentials/{id}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmPersonCredentialsV8")]
        [HeaderVersionRoute("/persons-credentials/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmPersonCredentialsV6")]
        public async Task<ActionResult<Dtos.PersonCredential>> PutPersonCredentialAsync([FromBody] Dtos.PersonCredential personCredential)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update a person's emergency information.
        /// </summary>
        /// <param name="emergencyInformation">An emergency information object</param>
        /// <returns>The updated emergency information object</returns>
        /// <accessComments>
        /// Only the current user can update their own emergency information.
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/persons/{personId}/emergency-information", 1, true, Name = "PutEmergencyInformation")]
        public async Task<IActionResult> PutEmergencyInformation(Dtos.Base.EmergencyInformation emergencyInformation)
        {
            if (emergencyInformation == null)
            {
                return CreateHttpResponseException("Request missing emergency information", HttpStatusCode.BadRequest);
            }
            try
            {
                var updatedEmergencyInformation = await _emergencyInformationService.UpdateEmergencyInformation(emergencyInformation);

                return Ok(updatedEmergencyInformation);
            }
            catch (PermissionsException permissionException)
            {
                return CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception)
            {
                return CreateHttpResponseException("Unable to update emergency information", HttpStatusCode.BadRequest);
            }
        }
        #endregion

        #region Delete Methods

        /// <summary>
        /// Delete (DELETE) an existing Person
        /// </summary>
        /// <param name="id">Id of the Person to delete</param>
        [HttpDelete]
        [Route("/persons/{id}", Name = "DeleteHedmPerson", Order = -10)]
        public async Task<IActionResult> DeletePersonAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing PersonCredential
        /// </summary>
        /// <param name="id">Id of the PersonCredential to delete</param>
        [HttpDelete]
        [Route("/persons-credentials/{id}", Name = "DefaultDeleteHedmPersonCredentials", Order = -10)]
        public async Task<IActionResult> DeletePersonCredentialAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Query Methods

        /// <summary>
        /// Queries a person by post.
        /// </summary>
        /// <param name="person"><see cref="Person2">Person</see> to use for querying</param>
        /// <returns>List of matching <see cref="Person2">persons</see></returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [HttpPost]
        [HeaderVersionRoute("/qapi/persons", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetMatchingPersons2", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Person2>>> QueryPerson2ByPostAsync([FromBody] Person2 person)
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
                _personService.ValidatePermissions(GetPermissionsMetaData());
                AddDataPrivacyContextProperty((await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return Ok(await _personService.QueryPerson2ByPostAsync(person));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Queries a person by post.
        /// </summary>
        /// <param name="person"><see cref="Person3">Person</see> to use for querying</param>
        /// <returns>List of matching <see cref="Person3">persons</see></returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [HttpPost]
        [HeaderVersionRoute("/qapi/persons", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmMatchingPersonsV8", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Person3>>> QueryPerson3ByPostAsync([FromBody] Person3 person)
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
                _personService.ValidatePermissions(GetPermissionsMetaData());
                AddDataPrivacyContextProperty((await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return Ok(await _personService.QueryPerson3ByPostAsync(person));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Queries a person by post.
        /// </summary>
        /// <param name="person"><see cref="Person4">Person</see> to use for querying</param>
        /// <returns>List of matching <see cref="Person4">persons</see></returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [HeaderVersionRoute("/qapi/persons", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmMatchingPersonsV12", IsEedmSupported = true, Order = -5)]
        public async Task<ActionResult<IEnumerable<Person4>>> QueryPerson4ByPostAsync([FromBody] Person4 person)
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
                _personService.ValidatePermissions(GetPermissionsMetaData());
                var personDtos = await _personService.QueryPerson4ByPostAsync(person, bypassCache);

                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              personDtos.Select(a => a.Id).ToList()));

                return Ok(personDtos);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Queries a person by post.
        /// </summary>
        /// <param name="person"><see cref="Person5">Person</see> to use for querying</param>
        /// <returns>List of matching <see cref="Person5">persons</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson, BasePermissionCodes.CreatePerson, BasePermissionCodes.UpdatePerson })]
        [HeaderVersionRoute("/qapi/persons", "12.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmMatchingPersonsV12.1.0", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Person5>>> QueryPerson5ByPostAsync([FromBody] Person5 person)
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
                _personService.ValidatePermissions(GetPermissionsMetaData());
                var personDtos = await _personService.QueryPerson5ByPostAsync(person, bypassCache);

                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              personDtos.Select(a => a.Id).ToList()));

                return Ok(personDtos);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
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
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query person by criteria and return the results of the matching algorithm
        /// </summary>
        /// <param name="criteria">The <see cref="Dtos.Base.PersonMatchCriteria">criteria</see> to query by.</param>
        /// <returns>List of matching <see cref="Dtos.Base.PersonMatchResult">results</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/persons", 1, false, Name = "GetPersonMatchingResults")]
        public async Task<ActionResult<IEnumerable<Dtos.Base.PersonMatchResult>>> QueryPersonMatchResultsByPostAsync([FromBody] Dtos.Base.PersonMatchCriteria criteria)
        {
            try
            {
                return Ok(await _personService.QueryPersonMatchResultsByPostAsync(criteria));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the matching Persons for the ids provided or searches keyword
        /// for the matching Persons if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">Keyword can be either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="Dtos.Base.Person">Person</see> with populated ID and first, middle and last names</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/persons", 1, false, "application/vnd.ellucian-person-name-search.v{0}+json", Name = "QueryPersonNames")]
        public async Task<ActionResult<IEnumerable<Dtos.Base.Person>>> QueryPersonNamesByPostAsync([FromBody] Dtos.Base.PersonNameQueryCriteria criteria)
        {
            try
            {
                return Ok(await _personService.QueryPersonNamesByPostAsync(criteria));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [HeaderVersionRoute("/persons/{id}/guid", 1, true, Name = "GetPersonGuidById")]
        public async Task<ActionResult<GuidObject2>> GetPersonGuidByIdAsync([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException("Person ID is required", HttpStatusCode.BadRequest);
            }
            try
            {
                var personGuid = await _personService.GetPersonGuidByIdAsync(id);
                return personGuid;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving person GUID by ID");
                return CreateHttpResponseException("Person GUID could not be found.", HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Adds an alternative identifier to an existing colleague person record.
        /// </summary>
        /// <param name="alternateIdentifier">AlternateIdentifier DTO</param>
        /// <accessComments>
        /// Logged-in user must have the permission 'UPDATE.PERSON' to add an alternative identifer to an existing colleague person.
        /// </accessComments>
        /// <returns>Newly added alternative identifier.</returns>
        [HttpPost]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/person-alt-ids", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "AddAlternateIdAsyncV1.0.0", IsEthosEnabled = true)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Adds an alternate id to a person record.",
           HttpMethodDescription = "Adds an alternate id to a person record.")]
        public async Task<ActionResult<AlternateIdentifier>> AddAlternateIdAsync([ModelBinder(typeof(EedmModelBinder))] AlternateIdentifier alternateIdentifier)
        {
            if (alternateIdentifier == null) 
            {
                _logger.LogError("alternateIdentifier DTO is required in body of request.");
                return CreateHttpResponseException("alternateIdentifier DTO is required in body of request.", HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _personService.AddAlternateIdAsync(alternateIdentifier));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to add an alternative identifier";
                _logger.LogError(pe, message);
                return CreateHttpResponseException(permissionExceptionMessage, HttpStatusCode.Forbidden);
            }          
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

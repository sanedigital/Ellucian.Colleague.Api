// Copyright 2021-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;

using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;
using System.Configuration;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonEmergencyContacts
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonEmergencyContactsController : BaseCompressedApiController
    {
        private readonly IEmergencyInformationService _personEmergencyContactsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonEmergencyContactsController class.
        /// </summary>
        /// <param name="personEmergencyContactsService">Service of type <see cref="IEmergencyInformationService">IEmergencyInformationService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonEmergencyContactsController(IEmergencyInformationService personEmergencyContactsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _personEmergencyContactsService = personEmergencyContactsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all personEmergencyContacts
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        ///  <param name="criteria">Selection criteria</param>
        ///  <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of PersonEmergencyContacts <see cref="Dtos.PersonEmergencyContacts"/> objects representing matching personEmergencyContacts</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPersonContact, BasePermissionCodes.UpdatePersonContact })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PersonEmergencyContacts))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 })]
        [HttpGet]
        [HeaderVersionRoute("/person-emergency-contacts", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonEmergencyContacts", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonEmergencyContactsAsync(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
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
                _personEmergencyContactsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.PersonEmergencyContacts>>(new List<Dtos.PersonEmergencyContacts>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }

                var criteriaObj = GetFilterObject<Dtos.PersonEmergencyContacts>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.PersonEmergencyContacts>>(new List<Dtos.PersonEmergencyContacts>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _personEmergencyContactsService.GetPersonEmergencyContacts2Async(page.Offset, page.Limit, criteriaObj, personFilterValue, bypassCache);

                AddEthosContextProperties(
                  await _personEmergencyContactsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personEmergencyContactsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonEmergencyContacts>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a personEmergencyContacts using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personEmergencyContacts</param>
        /// <returns>A personEmergencyContacts object <see cref="Dtos.PersonEmergencyContacts"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPersonContact, BasePermissionCodes.UpdatePersonContact })]
        [HttpGet]
        [HeaderVersionRoute("/person-emergency-contacts/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonEmergencyContactsByGuid2", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonEmergencyContacts>> GetPersonEmergencyContactsByGuidAsync(string guid)
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
                _personEmergencyContactsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _personEmergencyContactsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personEmergencyContactsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _personEmergencyContactsService.GetPersonEmergencyContactsByGuid2Async(guid);
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
        /// Update (PUT) an existing PersonEmergencyContacts
        /// </summary>
        /// <param name="guid">GUID of the personEmergencyContacts to update</param>
        /// <param name="personEmergencyContacts">DTO of the updated personEmergencyContacts</param>
        /// <returns>A PersonEmergencyContacts object <see cref="Dtos.PersonEmergencyContacts"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.UpdatePersonContact)]
        [HttpPut]
        [HeaderVersionRoute("/person-emergency-contacts/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonEmergencyContactsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonEmergencyContacts>> PutPersonEmergencyContactsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonEmergencyContacts personEmergencyContacts)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (personEmergencyContacts == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null personEmergencyContacts argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(personEmergencyContacts.Id))
            {
                personEmergencyContacts.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, personEmergencyContacts.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _personEmergencyContactsService.ValidatePermissions(GetPermissionsMetaData());
                return await _personEmergencyContactsService.UpdatePersonEmergencyContactsAsync(
                  await PerformPartialPayloadMerge(personEmergencyContacts, async () => await _personEmergencyContactsService.GetPersonEmergencyContactsByGuid2Async(guid, true),
                  await _personEmergencyContactsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                  _logger));
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
        /// Create (POST) a new personEmergencyContacts
        /// </summary>
        /// <param name="personEmergencyContacts">DTO of the new personEmergencyContacts</param>
        /// <returns>A personEmergencyContacts object <see cref="Dtos.PersonEmergencyContacts"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, PermissionsFilter(BasePermissionCodes.UpdatePersonContact)]
        [HttpPost]
        [HeaderVersionRoute("/person-emergency-contacts", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonEmergencyContactsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonEmergencyContacts>> PostPersonEmergencyContactsAsync(Dtos.PersonEmergencyContacts personEmergencyContacts)
        {
            try
            {
                _personEmergencyContactsService.ValidatePermissions(GetPermissionsMetaData());
                return await _personEmergencyContactsService.CreatePersonEmergencyContactsAsync(personEmergencyContacts);
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
        /// Delete (DELETE) a personEmergencyContacts
        /// </summary>
        /// <param name="guid">GUID to desired personEmergencyContacts</param>
        /// <returns>IActionResult</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete, PermissionsFilter(BasePermissionCodes.DeletePersonContact)]
        [Route("/person-emergency-contacts/{guid}", Name = "DefaultDeletePersonEmergencyContacts", Order = -10)]
        public async Task<IActionResult> DeletePersonEmergencyContactsAsync([FromRoute] string guid)
        {
            try
            {
                _personEmergencyContactsService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("id", "guid is a required for delete");
                }
                await _personEmergencyContactsService.DeletePersonEmergencyContactsAsync(guid);
                return NoContent();
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
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

    }
}

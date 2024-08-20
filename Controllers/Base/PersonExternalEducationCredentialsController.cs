// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to PersonExternalEducationCredentials
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonExternalEducationCredentialsController : BaseCompressedApiController
    {
        private readonly IPersonExternalEducationCredentialsService _personExternalEducationCredentialsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonExternalEducationCredentialsController class.
        /// </summary>
        /// <param name="personExternalEducationCredentialsService">Service of type <see cref="IPersonExternalEducationCredentialsService">IPersonExternalEducationCredentialsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonExternalEducationCredentialsController(IPersonExternalEducationCredentialsService personExternalEducationCredentialsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _personExternalEducationCredentialsService = personExternalEducationCredentialsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all personExternalEducationCredentials
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="personFilter">Person filter options</param>
        /// <param name="criteria">Standard filter options</param>
        /// <param name="person">person GUID filter option</param>
        /// <returns>List of PersonExternalEducationCredentials <see cref="Dtos.PersonExternalEducationCredentials"/> objects representing matching personExternalEducationCredentials</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewPersonExternalEducationCredentials, BasePermissionCodes.UpdatePersonExternalEducationCredentials })]
        [QueryStringFilterFilter("criteria", typeof(Ellucian.Colleague.Dtos.PersonExternalEducationCredentials))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [QueryStringFilterFilter("person", typeof(Dtos.Filters.PersonGuidFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]   
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HeaderVersionRoute("/person-external-education-credentials", "1.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonExternalEducationCredentials", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonExternalEducationCredentialsAsync(Paging page, QueryStringFilter criteria,
                QueryStringFilter personFilter, QueryStringFilter person)
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
                _personExternalEducationCredentialsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if ((personFilterObj != null) && (personFilterObj.personFilter != null))
                {
                    personFilterValue = personFilterObj.personFilter.Id;
                    if (string.IsNullOrEmpty(personFilterValue))
                    {
                        return new PagedActionResult<IEnumerable<Dtos.PersonExternalEducationCredentials>>(new List<Dtos.PersonExternalEducationCredentials>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }
                }
            
                string personGuid = string.Empty;
                var personGuidFilterObj = GetFilterObject<Dtos.Filters.PersonGuidFilter>(_logger, "person");
                if ((personGuidFilterObj != null) && (personGuidFilterObj.Person != null))
                {
                    personGuid = personGuidFilterObj.Person.Id;
                    if (string.IsNullOrEmpty(personGuid))
                    {
                        return new PagedActionResult<IEnumerable<Dtos.PersonExternalEducationCredentials>>(new List<Dtos.PersonExternalEducationCredentials>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }
                }

                var personExternalEducationFilter = GetFilterObject<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.PersonExternalEducationCredentials>>(new List<Dtos.PersonExternalEducationCredentials>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsAsync(page.Offset, page.Limit, personFilterValue,
                    personExternalEducationFilter, personGuid, bypassCache);

                AddEthosContextProperties(
                  await _personExternalEducationCredentialsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personExternalEducationCredentialsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonExternalEducationCredentials>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
   
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
        /// Read (GET) a personExternalEducationCredentials using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personExternalEducationCredentials</param>
        /// <returns>A personExternalEducationCredentials object <see cref="Dtos.PersonExternalEducationCredentials"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewPersonExternalEducationCredentials, BasePermissionCodes.UpdatePersonExternalEducationCredentials })]
        [HeaderVersionRoute("/person-external-education-credentials/{guid}", "1.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonExternalEducationCredentialsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonExternalEducationCredentials>> GetPersonExternalEducationCredentialsByGuidAsync(string guid)
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
                _personExternalEducationCredentialsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                  await _personExternalEducationCredentialsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personExternalEducationCredentialsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsByGuidAsync(guid);
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
        /// Update (PUT) an existing PersonExternalEducationCredentials
        /// </summary>
        /// <param name="guid">GUID of the personExternalEducationCredentials to update</param>
        /// <param name="personExternalEducationCredentials">DTO of the updated personExternalEducationCredentials</param>
        /// <returns>A PersonExternalEducationCredentials object <see cref="Dtos.PersonExternalEducationCredentials"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.UpdatePersonExternalEducationCredentials)]
        [HeaderVersionRoute("/person-external-education-credentials/{guid}", "1.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonExternalEducationCredentialsV1.1.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonExternalEducationCredentials>> PutPersonExternalEducationCredentialsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonExternalEducationCredentials personExternalEducationCredentials)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (personExternalEducationCredentials == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null personExternalEducationCredentials argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(personExternalEducationCredentials.Id))
            {
                personExternalEducationCredentials.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, personExternalEducationCredentials.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _personExternalEducationCredentialsService.ValidatePermissions(GetPermissionsMetaData());
                return await _personExternalEducationCredentialsService.UpdatePersonExternalEducationCredentialsAsync(
                  await PerformPartialPayloadMerge(personExternalEducationCredentials, async () => await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsByGuidAsync(guid, true),
                  await _personExternalEducationCredentialsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
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
        /// Create (POST) a new personExternalEducationCredentials
        /// </summary>
        /// <param name="personExternalEducationCredentials">DTO of the new personExternalEducationCredentials</param>
        /// <returns>A personExternalEducationCredentials object <see cref="Dtos.PersonExternalEducationCredentials"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, PermissionsFilter(BasePermissionCodes.UpdatePersonExternalEducationCredentials)]
        [HeaderVersionRoute("/person-external-education-credentials", "1.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonExternalEducationCredentialsV1.1.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonExternalEducationCredentials>> PostPersonExternalEducationCredentialsAsync(Dtos.PersonExternalEducationCredentials personExternalEducationCredentials)
        {
            if (personExternalEducationCredentials == null)
            {
                return CreateHttpResponseException("Request body must contain a valid personExternalEducationCredentials.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(personExternalEducationCredentials.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null personExternalEducationCredentials id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (personExternalEducationCredentials.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException(new IntegrationApiException("Must provide nil GUID when creating a new Person External Education Credentials.",
                    IntegrationApiUtility.GetDefaultApiError("Must provide nil GUID for create.")));
            }

            try
            {
                _personExternalEducationCredentialsService.ValidatePermissions(GetPermissionsMetaData());
                return await _personExternalEducationCredentialsService.CreatePersonExternalEducationCredentialsAsync(personExternalEducationCredentials);
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
        /// Delete (DELETE) a personExternalEducationCredentials
        /// </summary>
        /// <param name="guid">GUID to desired personExternalEducationCredentials</param>
        /// <returns>IActionResult</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/person-external-education-credentials/{guid}", Name = "DefaultDeletePersonExternalEducationCredentials", Order = -10)]
        public async Task<IActionResult> DeletePersonExternalEducationCredentialsAsync([FromRoute] string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        
    }
}

// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonMatchingRequests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonMatchingRequestsController : BaseCompressedApiController
    {
        private readonly IPersonMatchingRequestsService _personMatchingRequestsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonMatchingRequestsController class.
        /// </summary>
        /// <param name="personMatchingRequestsService">Service of type <see cref="IPersonMatchingRequestsService">IPersonMatchingRequestsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonMatchingRequestsController(IPersonMatchingRequestsService personMatchingRequestsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _personMatchingRequestsService = personMatchingRequestsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all personMatchingRequests
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">JSON formatted selection criteria.</param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters.</param>
        /// <returns>List of PersonMatchingRequests <see cref="Dtos.PersonMatchingRequests"/> objects representing matching personMatchingRequests</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewPersonMatchRequest, BasePermissionCodes.CreatePersonMatchRequestProspects })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PersonMatchingRequests))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HeaderVersionRoute("/person-matching-requests", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonMatchingRequests", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonMatchingRequestsAsync(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
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
                _personMatchingRequestsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }

                var criteriaObject = GetFilterObject<Dtos.PersonMatchingRequests>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.PersonMatchingRequests>>(new List<Dtos.PersonMatchingRequests>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _personMatchingRequestsService.GetPersonMatchingRequestsAsync(page.Offset, page.Limit, criteriaObject, personFilterValue, bypassCache);

                AddEthosContextProperties(
                  await _personMatchingRequestsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personMatchingRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonMatchingRequests>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a personMatchingRequests using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personMatchingRequests</param>
        /// <returns>A personMatchingRequests object <see cref="Dtos.PersonMatchingRequests"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewPersonMatchRequest, BasePermissionCodes.CreatePersonMatchRequestProspects })]
        [HeaderVersionRoute("/person-matching-requests/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonMatchingRequestsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonMatchingRequests>> GetPersonMatchingRequestsByGuidAsync(string guid)
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
                _personMatchingRequestsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _personMatchingRequestsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personMatchingRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _personMatchingRequestsService.GetPersonMatchingRequestsByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new personMatchingRequests
        /// </summary>
        /// <param name="personMatchingRequests">DTO of the new personMatchingRequests</param>
        /// <returns>A personMatchingRequests object <see cref="Dtos.PersonMatchingRequests"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/person-matching-requests", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonMatchingRequestsV1.0.0")]
        public async Task<ActionResult<Dtos.PersonMatchingRequests>> PostPersonMatchingRequestsAsync([FromBody] Dtos.PersonMatchingRequests personMatchingRequests)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personMatchingRequests
        /// </summary>
        /// <param name="guid">GUID of the personMatchingRequests to update</param>
        /// <param name="personMatchingRequests">DTO of the updated personMatchingRequests</param>
        /// <returns>A personMatchingRequests object <see cref="Dtos.PersonMatchingRequests"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/person-matching-requests/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonMatchingRequestsV1.0.0")]
        public async Task<ActionResult<Dtos.PersonMatchingRequests>> PutPersonMatchingRequestsAsync([FromRoute] string guid, [FromBody] Dtos.PersonMatchingRequests personMatchingRequests)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #region initiationsProspects

        /// <summary>
        /// Create (POST) a new personMatchingRequests
        /// </summary>
        /// <param name="personMatchingRequests">DTO of the new personMatchingRequests</param>
        /// <returns>A personMatchingRequests object <see cref="Dtos.PersonMatchingRequests"/> in EEDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.CreatePersonMatchRequestProspects)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationPersonMatchingRequestsInitiationsProspectsFormat },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/person-matching-requests", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultPostPersonMatchingRequestsInitiationsProspectsV1.0.0", Order = -20)]
        public async Task<ActionResult<Dtos.PersonMatchingRequests>> PostPersonMatchingRequestsInitiationsProspectsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.PersonMatchingRequestsInitiationsProspects personMatchingRequests)
        {
            if (personMatchingRequests == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null personMatchingRequests argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(personMatchingRequests.Id) || !personMatchingRequests.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            try
            {
                _personMatchingRequestsService.ValidatePermissions(GetPermissionsMetaData());
                var dpList = await _personMatchingRequestsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);
                await _personMatchingRequestsService.ImportExtendedEthosData(await ExtractExtendedData(await _personMatchingRequestsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var personMatchingRequestsReturn = await _personMatchingRequestsService.CreatePersonMatchingRequestsInitiationsProspectsAsync(personMatchingRequests);

                AddEthosContextProperties(dpList,
                    await _personMatchingRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personMatchingRequestsReturn.Id }));

                return personMatchingRequestsReturn;
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
        /// Update (PUT) an existing personMatchingRequests
        /// </summary>
        /// <param name="guid">GUID of the personMatchingRequests to update</param>
        /// <param name="personMatchingRequests">DTO of the updated personMatchingRequests</param>
        /// <returns>A personMatchingRequests object <see cref="Dtos.PersonMatchingRequests"/> in EEDM format</returns>
        [HttpPut]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationPersonMatchingRequestsInitiationsProspectsFormat },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/person-matching-requests/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultPutPersonMatchingRequestsInitiationsProspectsV1.0.0", Order = -20)]
        public async Task<ActionResult<Dtos.PersonMatchingRequests>> PutPersonMatchingRequestsInitiationsProspectsAsync([FromRoute] string guid, [FromBody] Dtos.PersonMatchingRequestsInitiationsProspects personMatchingRequests)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        /// <summary>
        /// Delete (DELETE) a personMatchingRequests
        /// </summary>
        /// <param name="guid">GUID to desired personMatchingRequests</param>
        [HttpDelete]
        [Route("/person-matching-requests/{guid}", Name = "DefaultDeletePersonMatchingRequests", Order = -10)]
        public async Task<IActionResult> DeletePersonMatchingRequestsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

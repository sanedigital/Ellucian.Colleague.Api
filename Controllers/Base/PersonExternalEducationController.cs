// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using System.Configuration;

using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to PersonExternalEducation
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonExternalEducationController : BaseCompressedApiController
    {
        private readonly IPersonExternalEducationService _personExternalEducationService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonExternalEducationController class.
        /// </summary>
        /// <param name="externalEducationService">Service of type <see cref="IPersonExternalEducationService">IPersonExternalEducationService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonExternalEducationController(IPersonExternalEducationService externalEducationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _personExternalEducationService = externalEducationService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all personExternalEducation
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">filter criteria</param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <param name="personByInstitutionType">Retrieve information for a specific person at institution's of a specific type</param>
        /// <returns>List of PersonExternalEducation <see cref="Dtos.PersonExternalEducation"/> objects representing matching personExternalEducation</returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewExternalEducation, BasePermissionCodes.CreateExternalEducation })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [QueryStringFilterFilter("criteria", typeof(Ellucian.Colleague.Dtos.PersonExternalEducation))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [QueryStringFilterFilter("personByInstitutionType", typeof(Dtos.Filters.PersonByInstitutionType))]
        [ServiceFilter(typeof(EedmResponseFilter)),TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HeaderVersionRoute("/person-external-education", "1.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonExternalEducation", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonExternalEducationAsync(Paging page, QueryStringFilter criteria, 
                QueryStringFilter personFilter, QueryStringFilter personByInstitutionType)
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
                _personExternalEducationService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                string personFilterGuid = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if ((personFilterObj != null) && (personFilterObj.personFilter != null))
                {
                    personFilterGuid = personFilterObj.personFilter.Id;
                }
                
                var personByInstitutionTypeFilterObj = GetFilterObject<Dtos.Filters.PersonByInstitutionType>(_logger, "personByInstitutionType");
                
                var personExternalEducationFilter = GetFilterObject<Ellucian.Colleague.Dtos.PersonExternalEducation>(_logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.PersonExternalEducation>>(new List<Dtos.PersonExternalEducation>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _personExternalEducationService.GetPersonExternalEducationAsync(page.Offset, page.Limit, 
                    personExternalEducationFilter, personFilterGuid, personByInstitutionTypeFilterObj, bypassCache);

                AddEthosContextProperties(
                  await _personExternalEducationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personExternalEducationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonExternalEducation>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a personExternalEducation using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personExternalEducation</param>
        /// <returns>A personExternalEducation object <see cref="Dtos.PersonExternalEducation"/> in EEDM format</returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewExternalEducation, BasePermissionCodes.CreateExternalEducation })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/person-external-education/{guid}", "1.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonExternalEducationByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonExternalEducation>> GetPersonExternalEducationByGuidAsync(string guid)
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
                _personExternalEducationService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                  await _personExternalEducationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personExternalEducationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _personExternalEducationService.GetPersonExternalEducationByGuidAsync(guid, bypassCache);
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
        /// Update (PUT) an existing PersonExternalEducation
        /// </summary>
        /// <param name="guid">GUID of the personExternalEducation to update</param>
        /// <param name="personExternalEducation">DTO of the updated personExternalEducation</param>
        /// <returns>A PersonExternalEducation object <see cref="Dtos.PersonExternalEducation"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.CreateExternalEducation)]
        [HeaderVersionRoute("/person-external-education/{guid}", "1.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonExternalEducationV1.2.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonExternalEducation>> PutPersonExternalEducationAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonExternalEducation personExternalEducation)
        {
            if (personExternalEducation == null)
            {
                return CreateHttpResponseException("Request body must contain a valid personExternalEducation.", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (personExternalEducation == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null personExternalEducation argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(personExternalEducation.Id))
            {
                personExternalEducation.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, personExternalEducation.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _personExternalEducationService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _personExternalEducationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _personExternalEducationService.ImportExtendedEthosData(await ExtractExtendedData(await _personExternalEducationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var personExternalEducationReturn = await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(
                   await PerformPartialPayloadMerge(personExternalEducation, async () => await _personExternalEducationService.GetPersonExternalEducationByGuidAsync(guid, true),
                dpList, _logger), true);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _personExternalEducationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return personExternalEducationReturn;
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
        /// Create (POST) a new personExternalEducation
        /// </summary>
        /// <param name="personExternalEducation">DTO of the new personExternalEducation</param>
        /// <returns>A personExternalEducation object <see cref="Dtos.PersonExternalEducation"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.CreateExternalEducation)]
        [HeaderVersionRoute("/person-external-education", "1.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonExternalEducationV1.2.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonExternalEducation>> PostPersonExternalEducationAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.PersonExternalEducation personExternalEducation)
        {
            if (personExternalEducation == null)
            {
                return CreateHttpResponseException("Request body must contain a valid personExternalEducation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(personExternalEducation.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null personExternalEducation id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                _personExternalEducationService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _personExternalEducationService.ImportExtendedEthosData(await ExtractExtendedData(await _personExternalEducationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create 
                var personExternalEducationReturn = await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(personExternalEducation, false);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personExternalEducationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personExternalEducationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personExternalEducationReturn.Id }));

                return personExternalEducationReturn;

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
        /// Delete (DELETE) a personExternalEducation
        /// </summary>
        /// <param name="guid">GUID to desired personExternalEducation</param>
        [HttpDelete]
        [Route("/person-external-education/{guid}", Name = "DefaultDeletePersonExternalEducation", Order = -10)]
        public async Task<IActionResult> DeletePersonExternalEducationAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

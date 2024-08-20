// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Educational Institutions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EducationalInstitutionsController : BaseCompressedApiController
    {
        private readonly IEducationalInstitutionsService _educationalInstitutionsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EducationalInstitutionsController class.
        /// </summary>
        /// <param name="educationalInstitutionsService">Service of type <see cref="IEducationalInstitutionsService">IEducationalInstitutionsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EducationalInstitutionsController(IEducationalInstitutionsService educationalInstitutionsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _educationalInstitutionsService = educationalInstitutionsService;
            this._logger = logger;
        }


        /// <summary>
        /// Return Educational-Institutions 
        /// </summary>
        /// <param name="page">paging information</param>
        /// <param name="type">Type of Educational-Institution ex:"secondary" or "postSecondary"</param>
        /// <param name="criteria">criteria</param>
        /// <returns>List of EducationalInstitutions <see cref="Dtos.EducationalInstitution"/> objects representing matching educationalInstitutions</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), PermissionsFilter(BasePermissionCodes.ViewEducationalInstitution)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.EducationalInstitution))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter(new string[] { "type", "credentials.type", "credentials.value",  }, false, true)]
        [HeaderVersionRoute("/educational-institutions", "6.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEducationalInstitutionsDefault", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.EducationalInstitution>>> GetEducationalInstitutionsAsync(Paging page, [FromQuery] string type = "", QueryStringFilter criteria = null)
        {
           
            var bypassCache = false;
            if (type == null || type == "null")
            {
                return new PagedActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                _educationalInstitutionsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var criteriaObject = GetFilterObject<Dtos.EducationalInstitution>(_logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);



                Dtos.EnumProperties.EducationalInstitutionType? typeFilter = null;

                if (!string.IsNullOrEmpty(type))
                {
                    switch (type.ToLower())
                    {
                        case "postsecondaryschool":
                            typeFilter = EducationalInstitutionType.PostSecondarySchool;
                            break;
                        case "secondaryschool":
                            typeFilter = EducationalInstitutionType.SecondarySchool;
                            break;
                        default:
                            return new PagedActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders); ;
                    }
                }
                else if (criteriaObject.Type != EducationalInstitutionType.NotSet)
                {

                    switch (criteriaObject.Type)
                    {
                        case EducationalInstitutionType.PostSecondarySchool:
                            typeFilter = EducationalInstitutionType.PostSecondarySchool;
                            break;
                        case EducationalInstitutionType.SecondarySchool:
                            typeFilter = EducationalInstitutionType.SecondarySchool;
                            break;
                        default:
                            return new PagedActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }
                }

                //we need to validate the credentials
                if (criteriaObject.Credentials != null && criteriaObject.Credentials.Any())
                {
                    if (criteriaObject.Credentials.Count() > 1)
                    {
                        return new PagedActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }
                    var credential = criteriaObject.Credentials.FirstOrDefault();

                    if ((credential.Type != null) || !string.IsNullOrEmpty(credential.Value))
                    {
                        if (!string.IsNullOrEmpty(credential.Type.ToString()) && string.IsNullOrEmpty(credential.Value))
                        {
                            throw new ArgumentException("credentialValue", "credentialValue is required when requesting a credentialType");
                        }
                        if (string.IsNullOrEmpty(credential.Type.ToString()) && !string.IsNullOrEmpty(credential.Value))
                        {
                            throw new ArgumentException("credentialType", "credentialType is required when requesting a credentialValue");
                        }
                    }
                    if (credential.Type != Credential3Type.ColleaguePersonId)
                    {
                        return new PagedActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }
                }

                //we need to validate the system Codes
                if (criteriaObject.SystemCodes != null && criteriaObject.SystemCodes.Any())
                {
                    if (criteriaObject.SystemCodes.Count() > 2)
                    {
                        return new PagedActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }
                    if (criteriaObject.SystemCodes.Where(code => code.Name == InstitutionSystemCodes.LocalId).Count() == 2 || criteriaObject.SystemCodes.Where(code => code.Name == InstitutionSystemCodes.OtherId).Count() == 2)
                    {
                        return new PagedActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }
                    foreach (var systemCode in criteriaObject.SystemCodes)
                    {
                        if ((systemCode.Name != null) || !string.IsNullOrEmpty(systemCode.Value))
                        {
                            if (!string.IsNullOrEmpty(systemCode.Name.ToString()) && string.IsNullOrEmpty(systemCode.Value))
                            {
                                throw new ArgumentException("SystemCodeValue is required when requesting a SystemCodeName");
                            }
                            if (string.IsNullOrEmpty(systemCode.Name.ToString()) && !string.IsNullOrEmpty(systemCode.Value))
                            {
                                throw new ArgumentException("SystemCodeName is required when requesting a SystemCodeValue");
                            }
                        }
                    }
                }

                var pageOfItems = await _educationalInstitutionsService.GetEducationalInstitutionsByTypeAsync(page.Offset, page.Limit, criteriaObject, typeFilter, bypassCache);

                AddEthosContextProperties(
                    await _educationalInstitutionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _educationalInstitutionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.EducationalInstitution>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            
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
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) an Educational-Institution-Unit using a GUID
        /// </summary>
        /// <param name="id">GUID to desired educationalInstitution</param>
        /// <returns>An EducationalInstitutions object <see cref="Dtos.EducationalInstitution"/> in DataModel format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.ViewEducationalInstitution)]
        [HeaderVersionRoute("/educational-institutions/{id}", "6.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEducationalInstitutionByGuidDefault", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EducationalInstitution>> GetEducationalInstitutionsByGuidAsync(string id)
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
                _educationalInstitutionsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _educationalInstitutionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _educationalInstitutionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _educationalInstitutionsService.GetEducationalInstitutionByGuidAsync(id);
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
        /// Create (POST) a new Educational-Institution
        /// </summary>
        /// <param name="educationalInstitution">DTO of the new educationalInstitutionUnits</param>
        /// <returns>A educationalInstitutionUnits object <see cref="Dtos.EducationalInstitution"/> in Data Model format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/educational-institutions", "6.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEducationalInstitutionV6_1_0")]
        public async Task<ActionResult<Dtos.EducationalInstitution>> PostEducationalInstitutionsAsync([FromBody] Dtos.EducationalInstitution educationalInstitution)
        {
            //Post is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError), HttpStatusCode.MethodNotAllowed);

        }

        /// <summary>
        /// Update (PUT) an existing Educational-Institution
        /// </summary>
        /// <param name="id">GUID of the EducationalInstitutions to update</param>
        /// <param name="educationalInstitution">DTO of the updated EducationalInstitutions</param>
        /// <returns>A EducationalInstitutions object <see cref="Dtos.EducationalInstitution"/> in Data Model format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/educational-institutions/{id}", "6.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEducationalInstitutionV6_1_0")]
        public async Task<ActionResult<Dtos.EducationalInstitution>> PutEducationalInstitutionsAsync([FromRoute] string id, [FromBody] Dtos.EducationalInstitution educationalInstitution)
        {
            //Put is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError), HttpStatusCode.MethodNotAllowed);

        }

        /// <summary>
        /// Delete (DELETE) a Educational-Institution
        /// </summary>
        /// <param name="id">GUID to desired EducationalInstitutions</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [Route("/educational-institutions/{id}", Name = "DeleteEducationalInstitutionByGuid", Order = -10)]
        public async Task<IActionResult> DeleteEducationalInstitutionByGuidAsync(string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError), HttpStatusCode.MethodNotAllowed);

        }
    }
}

// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.Base.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to NonPersonRelationships
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class NonPersonRelationshipsController : BaseCompressedApiController
    {
        private readonly INonPersonRelationshipsService _nonPersonRelationshipsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the NonPersonRelationshipsController class.
        /// </summary>
        /// <param name="personRelationshipsService">Service of type <see cref="INonPersonRelationshipsService">INonPersonRelationshipsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public NonPersonRelationshipsController(INonPersonRelationshipsService personRelationshipsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _nonPersonRelationshipsService = personRelationshipsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all personRelationships
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="organization">organization Filter</param>
        /// <param name="institution">institution Filter</param>
        /// <param name="criteria">NonPerson Filter</param>
        /// <param name="relationshipType">relatuonship type Filter</param>
        /// <returns>List of NonPersonRelationships <see cref="Dtos.NonPersonRelationships"/> objects representing matching personRelationships</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 })]
        public async Task<IActionResult> GetNonPersonRelationshipsAsync(Paging page, [FromQuery] string organization = "", [FromQuery] string institution = "", [FromQuery] string criteria = "", [FromQuery] string relationshipType = "")
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
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                string personValue = string.Empty, relationshipTypeValue = string.Empty;
                string organizationValue = string.Empty, institutionValue = string.Empty;
                if (!string.IsNullOrEmpty(organization))
                {
                    var organizationObject = JObject.Parse(organization);
                    if (organizationObject != null)
                    {
                        organizationValue = GetValueFromJsonObjectToken("organization.id", organizationObject);
                    }
                }

                if (!string.IsNullOrEmpty(institution))
                {
                    var institutionObject = JObject.Parse(institution);
                    if (institutionObject != null)
                    {
                        institutionValue = GetValueFromJsonObjectToken("institution.id", institutionObject);
                    }
                }

                if (!string.IsNullOrWhiteSpace(criteria))
                {
                    var criteriaObject = JObject.Parse(criteria);
                    if (criteriaObject != null)
                    {
                         personValue = GetValueFromJsonObjectToken("related.person.id", criteriaObject);
                    }
                }

                if (!string.IsNullOrEmpty(relationshipType))
                {
                    var relationshipTypeObject = JObject.Parse(relationshipType);
                    if (relationshipTypeObject != null)
                    {
                        relationshipTypeValue = GetValueFromJsonObjectToken("relationshipType.id", relationshipTypeObject);
                    }
                }
                
                var pageOfItems = await _nonPersonRelationshipsService.GetNonPersonRelationshipsAsync(page.Offset, page.Limit, organizationValue, institutionValue, personValue, relationshipTypeValue, bypassCache);

                AddEthosContextProperties(
                  await _nonPersonRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _nonPersonRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.NonPersonRelationships>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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

        /// <summary>
        /// Read (GET) a personRelationships using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personRelationships</param>
        /// <returns>A personRelationships object <see cref="Dtos.NonPersonRelationships"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        public async Task<ActionResult<Dtos.NonPersonRelationships>> GetNonPersonRelationshipsByGuidAsync(string guid)
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
                //AddDataPrivacyContextProperty((await _personRelationshipsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                   await _nonPersonRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _nonPersonRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _nonPersonRelationshipsService.GetNonPersonRelationshipsByGuidAsync(guid);
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

        /// <summary>
        /// Create (POST) a new personRelationships
        /// </summary>
        /// <param name="personRelationships">DTO of the new personRelationships</param>
        /// <returns>A personRelationships object <see cref="Dtos.NonPersonRelationships"/> in EEDM format</returns>
        [HttpPost]
        public async Task<ActionResult<Dtos.NonPersonRelationships>> PostNonPersonRelationshipsAsync([FromBody] Dtos.NonPersonRelationships personRelationships)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personRelationships
        /// </summary>
        /// <param name="guid">GUID of the personRelationships to update</param>
        /// <param name="personRelationships">DTO of the updated personRelationships</param>
        /// <returns>A personRelationships object <see cref="Dtos.NonPersonRelationships"/> in EEDM format</returns>
        [HttpPut]
        public async Task<ActionResult<Dtos.NonPersonRelationships>> PutNonPersonRelationshipsAsync([FromQuery] string guid, [FromBody] Dtos.NonPersonRelationships personRelationships)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personRelationships
        /// </summary>
        /// <param name="guid">GUID to desired personRelationships</param>
        [HttpDelete]
        public async Task<IActionResult> DeleteNonPersonRelationshipsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
     }
}

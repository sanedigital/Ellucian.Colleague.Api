// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

using System.Net;
using Ellucian.Colleague.Dtos;
using Newtonsoft.Json.Linq;

using Ellucian.Web.Http.ModelBinding;
using System.Configuration;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides personal relationships data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonalRelationshipsController : BaseCompressedApiController
    {
        private readonly IPersonalRelationshipsService _personalRelationshipsService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Personal relationships constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="personalRelationshipsService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonalRelationshipsController(IAdapterRegistry adapterRegistry, IPersonalRelationshipsService personalRelationshipsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _personalRelationshipsService = personalRelationshipsService;
            this._logger = logger;
        }

        #region GET Methods v6
        
        /// <summary>
        /// Retrieves all active personal relationships for relationship id
        /// </summary>
        /// <returns>PersonalRelationship object for a personal relationship.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyRelationship, BasePermissionCodes.UpdatePersonalRelationship })]
        [HttpGet]
        [HeaderVersionRoute("/personal-relationships/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonalRelationshipsByIdV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonalRelationship>> GetPersonalRelationshipByIdAsync([FromRoute] string id)
        {
            try
            {
                _personalRelationshipsService.ValidatePermissions(GetPermissionsMetaData());
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
                    throw new ArgumentNullException("Id cannot be null.");
                }

               AddEthosContextProperties(
                    await _personalRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personalRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _personalRelationshipsService.GetPersonalRelationshipByIdAsync(id);
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
            catch (InvalidOperationException e)
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
        /// Return a list of Personal Relationships objects based on selection criteria.
        /// </summary>
        /// <param name="page">Personal Relationship page Contains ...page...</param>
        /// <param name="subjectPerson">Personal Relationship subjectPerson Contains ...subjectPerson...</param>
        /// <param name="relatedPerson">Personal Relationship relatedPerson Contains ...relatedPerson...</param>
        /// <param name="directRelationshipType">Personal Relationship directRelationship Contains ...directRelationship...</param>
        /// <param name="directRelationshipDetailId">Personal Relationship reciprocalRelationship Contains ...reciprocalRelationship...</param>
        // <returns>List of PersonalRelationship <see cref="Dtos.PersonalRelationship"/> objects representing matching personal relationships</returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyRelationship, BasePermissionCodes.UpdatePersonalRelationship })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(new string[] { "subjectPerson", "relatedPerson", "directRelationshipType", "directRelationshipDetailId" }, false, true)]
        [HttpGet]
        [HeaderVersionRoute("/personal-relationships", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonalRelationshipsV6", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonalRelationshipsAsync(Paging page, [FromQuery] string subjectPerson = "",
            [FromQuery] string relatedPerson = "", [FromQuery] string directRelationshipType = "", [FromQuery] string directRelationshipDetailId = "")
        {
            if (subjectPerson == null || relatedPerson == null || directRelationshipType == null || directRelationshipDetailId == null)
            {
                return new PagedActionResult<IEnumerable<Dtos.PersonalRelationship>>(new List<Dtos.PersonalRelationship>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

            string criteria = string.Concat(subjectPerson,relatedPerson,directRelationshipType,directRelationshipDetailId);
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

            Tuple<IEnumerable<Dtos.PersonalRelationship>, int> pageOfItems = null;

            try
            {
                _personalRelationshipsService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(criteria))
                {
                    pageOfItems = await _personalRelationshipsService.GetAllPersonalRelationshipsAsync(page.Offset, page.Limit, bypassCache);

                }
                else
                {
                    //valid query parameter but empty argument
                    if ((string.IsNullOrEmpty(criteria.Replace("\"", ""))) || (string.IsNullOrEmpty(criteria.Replace("'", ""))))
                    {
                        return new PagedActionResult<IEnumerable<Dtos.PersonalRelationship>>(new List<Dtos.PersonalRelationship>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }

                    if ((!string.IsNullOrEmpty(directRelationshipType)) && (!ValidEnumerationValue(typeof(PersonalRelationshipType), directRelationshipType)))
                    {
                        throw new ColleagueWebApiException(string.Concat("'", directRelationshipType, "' is an invalid enumeration value. "));
                    }
                   
                    pageOfItems = await _personalRelationshipsService.GetPersonalRelationshipsByFilterAsync(page.Offset, page.Limit, subjectPerson, relatedPerson, directRelationshipType, directRelationshipDetailId);
                }

                AddEthosContextProperties(
                   await _personalRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personalRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       pageOfItems.Item1.Select(i => i.Id).ToList()));
                return new PagedActionResult<IEnumerable<Dtos.PersonalRelationship>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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

        #endregion

        #region PUT method v6
        /// <summary>
        /// Updates personal relationship
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personalRelationship"></param>
        /// <returns></returns>
        [HttpPut]
        [HeaderVersionRoute("/personal-relationships/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmPersonalRelationshipsV6")]
        public async Task<ActionResult<Dtos.PersonalRelationship>> PutPersonalRelationshipAsync([FromRoute] string id, [FromBody] Dtos.PersonalRelationship personalRelationship)
        {
            //PUT is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region POST method v6
        /// <summary>
        /// Create new personal relationship
        /// </summary>
        /// <param name="personRelationship">personRelationship</param>
        /// <returns></returns>
        //[HttpPost]
        [HttpPost]
        [HeaderVersionRoute("/personal-relationships", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmPersonalRelationshipsV6")]
        public async Task<ActionResult<Dtos.PersonalRelationship>> PostPersonalRelationshipAsync([FromBody] Dtos.PersonalRelationship personRelationship)
        {
            //POST is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));        
        }
        #endregion

        #region Get Methods v16.0.0

        /// <summary>
        /// Return all personalRelationships2
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="personFilter">person filter criteria</param>
        /// <param name="person">person filter criteria</param>
        /// <param name="relationshipType">person filter criteria</param>
        /// <returns>List of PersonRelationships <see cref="Dtos.PersonalRelationships2"/> objects representing matching personalRelationships2</returns>
        /// <accessComments>
        /// Only the current user can get their own person relationships.
        /// </accessComments>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyRelationship, BasePermissionCodes.UpdatePersonalRelationship })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonalRelationships2Filter))]
        [QueryStringFilterFilter("person", typeof(Dtos.Filters.PersonalRelationships2Filter))]
        [QueryStringFilterFilter("relationshipType", typeof(Dtos.Filters.PersonalRelationships2Filter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 })]
        [HttpGet]
        [HeaderVersionRoute("/personal-relationships", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonalRelationships", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonalRelationships2Async(Paging page, QueryStringFilter personFilter = null, QueryStringFilter person = null, QueryStringFilter relationshipType = null)
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
                _personalRelationshipsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonalRelationships2Filter>(_logger, "personFilter");
                if ((personFilterObj != null) && (personFilterObj.personFilter != null))
                {
                    personFilterValue = personFilterObj.personFilter.Id;
                }

                string personValue = string.Empty;
                var personObj = GetFilterObject<Dtos.Filters.PersonalRelationships2Filter>(_logger, "person");
                if ((personObj != null) && (personObj.person != null))
                {
                    personValue = personObj.person.Id;
                }

                string relationshipTypeValue = string.Empty;
                var relaObj = GetFilterObject<Dtos.Filters.PersonalRelationships2Filter>(_logger, "relationshipType");
                if ((relaObj != null) && (relaObj.relationshipType != null))
                {
                    relationshipTypeValue = relaObj.relationshipType.Id;
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentAcademicPrograms4>>(new List<Dtos.StudentAcademicPrograms4>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _personalRelationshipsService.GetPersonalRelationships2Async(page.Offset, page.Limit, personValue, relationshipTypeValue, personFilterValue, bypassCache);

                AddEthosContextProperties(
                  await _personalRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personalRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonalRelationships2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// <summary>
        /// Read (GET) a personalRelationships2 using a GUID
        /// </summary>
        /// <param name="id">GUID to desired personalRelationships2</param>
        /// <returns>A personalRelationships2 object <see cref="Dtos.PersonalRelationships2"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyRelationship, BasePermissionCodes.UpdatePersonalRelationship })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/personal-relationships/{id}", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonalRelationshipsById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonalRelationships2>> GetPersonalRelationships2ByGuidAsync(string id)
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
                _personalRelationshipsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _personalRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personalRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _personalRelationshipsService.GetPersonalRelationships2ByGuidAsync(id);
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
        /// Update (PUT) an existing PersonRelationships
        /// </summary>
        /// <param name="id">GUID of the personalRelationships2 to update</param>
        /// <param name="personalRelationships2">DTO of the updated personalRelationships2</param>
        /// <returns>A PersonRelationships object <see cref="Dtos.PersonalRelationships2"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.UpdatePersonalRelationship)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/personal-relationships/{id}", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmPersonalRelationshipsV16.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonalRelationships2>> PutPersonalRelationships2Async([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonalRelationships2 personalRelationships2)
        {            
            try
            {
                _personalRelationshipsService.ValidatePermissions(GetPermissionsMetaData());
                return await _personalRelationshipsService.UpdatePersonalRelationships2Async(
                  await PerformPartialPayloadMerge(personalRelationships2, async () => await _personalRelationshipsService.GetPersonalRelationships2ByGuidAsync(id, true),
                  await _personalRelationshipsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
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
        /// Create (POST) a new personalRelationships2
        /// </summary>
        /// <param name="personalRelationships2">DTO of the new personalRelationships2</param>
        /// <returns>A personalRelationships2 object <see cref="Dtos.PersonalRelationships2"/> in HeDM format</returns>
        [HttpPost, PermissionsFilter(BasePermissionCodes.UpdatePersonalRelationship)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/personal-relationships", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmPersonalRelationshipsV16.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonalRelationships2>> PostPersonalRelationships2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.PersonalRelationships2 personalRelationships2)
        {
            try
            {
                _personalRelationshipsService.ValidatePermissions(GetPermissionsMetaData());
                return await _personalRelationshipsService.CreatePersonalRelationships2Async(personalRelationships2);
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
        /// Delete (DELETE) a personalRelationships2
        /// </summary>
        /// <param name="id">GUID to desired personalRelationships2</param>
        /// <returns>IActionResult</returns>
        [HttpDelete, PermissionsFilter(BasePermissionCodes.DeletePersonalRelationship)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [Route("/personal-relationships/{id}", Name = "DefaultDeleteHedmPersonalRelationships", Order = -10)]
        public async Task<IActionResult> DeletePersonalRelationshipsAsync([FromRoute] string id)
        {
            try
            {
                _personalRelationshipsService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "guid is a required for delete");
                }
                await _personalRelationshipsService.DeletePersonalRelationshipsAsync(id);
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

        #region personal-relationship-initiation-process

        /// <summary>
        /// Create (POST) a new personalRelationships2
        /// </summary>
        /// <param name="personalRelationships">DTO of the new personalRelationships2</param>
        /// <returns>A personalRelationships2 object <see cref="Dtos.PersonalRelationshipInitiationProcess"/> in HeDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.ProcessRelationshipRequest)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/personal-relationship-initiation-process", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmPersonalRelationshipInitiationProcessV1_0_0", IsEedmSupported = true)]
        public async Task<ActionResult<object>> PostPersonalRelationshipInitiationProcessAsync([ModelBinder(typeof(EedmModelBinder))] PersonalRelationshipInitiationProcess personalRelationships)
        {
            try
            {
                _personalRelationshipsService.ValidatePermissions(GetPermissionsMetaData());
                var returnObject = await _personalRelationshipsService.CreatePersonalRelationshipInitiationProcessAsync(personalRelationships);

                var resourceName = string.Empty;
                var resourceGuid = string.Empty;
                var version = string.Empty;

                var type = returnObject.GetType();
                if (type == typeof(Dtos.PersonalRelationships2))
                {
                    resourceName = "personal-relationships";
                    resourceGuid = (returnObject as Dtos.PersonalRelationships2).Id;
                    version = "16.0.0";
                }
                else
                {
                    resourceName = "person-matching-requests";
                    resourceGuid = (returnObject as Dtos.PersonMatchingRequests).Id;
                    version = "1.0.0";
                }
                string customMediaType = string.Format(IntegrationCustomMediaType, resourceName, version);
                CustomMediaTypeAttributeFilter.SetCustomMediaType(customMediaType);

                //store dataprivacy list and get the extended data to store 
                var resource = new Web.Http.EthosExtend.EthosResourceRouteInfo()
                {
                    ResourceName = resourceName,
                    ResourceVersionNumber = version,
                    BypassCache = true
                };

                AddEthosContextProperties(await _personalRelationshipsService.GetDataPrivacyListByApi(resourceName, true),
                   await _personalRelationshipsService.GetExtendedEthosDataByResource(resource, new List<string>() { resourceGuid }));
                
                return returnObject;
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

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Update a Relationship Initiation PRocess in Colleague (Not Supported)
        /// </summary>
        /// <param name="guid">Unique ID representing the Personal Relation Type to update</param>
        /// <param name="personalRelationshipDto"><see cref="PersonalRelationships2">RelationType</see> to update</param>
        [HttpPut]
        [HeaderVersionRoute("/personal-relationship-initiation-process/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmPersonalRelationshipInitiationProcessV1_0_0")]
        public ActionResult<Ellucian.Colleague.Dtos.RelationType> PutPersonalRelationshipInitiationProcess([FromRoute] string guid, [FromBody] PersonalRelationships2 personalRelationshipDto)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a Relationship Initiation PRocess in Colleague (Not Supported)
        /// </summary>
        /// <param name="guid">Unique ID representing the Personal Relation Type to update</param>
        [HttpGet]
        [HeaderVersionRoute("/personal-relationship-initiation-process/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmPersonalRelationshipInitiationProcessV1_0_0")]
        public ActionResult<Ellucian.Colleague.Dtos.RelationType> GetPersonalRelationshipInitiationProcess([FromRoute] string guid = null)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing PersonalRelationshipInitiationProcess
        /// </summary>
        /// <param name="guid">Id of the PersonalRelationshipStatus to delete</param>
        [HttpDelete]
        [HeaderVersionRoute("/personal-relationship-initiation-process/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DeleteHedmPersonalRelationshipInitiationProcessV1_0_0")]
        public IActionResult DeletePersonalRelationshipInitiationProcess([FromRoute] string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

    }


}

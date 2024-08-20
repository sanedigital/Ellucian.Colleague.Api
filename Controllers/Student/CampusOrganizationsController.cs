// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Ellucian.Web.Http.Filters;
using System.Linq;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to CampusOrganization data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.CampusOrgs)]
    public class CampusOrganizationsController : BaseCompressedApiController
    {
        private readonly ICampusOrganizationService _campusOrganizationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the CampusOrganizationController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="campusOrganizationService">Service of type <see cref="ICampusOrganizationService">ICampusOrganizationService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CampusOrganizationsController(IAdapterRegistry adapterRegistry, ICampusOrganizationService campusOrganizationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _campusOrganizationService = campusOrganizationService;
            _adapterRegistry = adapterRegistry;
            this._logger = logger;
        }

        /// <summary>
        /// A qapi that retrieves CampusOrganization2 records matching the query criteria.
        /// <param name="criteria">CampusOrganizationQueryCriteria criteria</param>
        /// </summary>
        /// <returns>CampusOrganization2 objects.</returns>
        /// <remarks>This method is NOT intended for use with ELLUCIAN DATA MODEL and hence doesn't deal with GUIDs</remarks>
        [HttpPost]
        [HeaderVersionRoute("/qapi/campus-organization", 1, true, Name = "GetCampusOrganizations2Async")]
        public async Task<ActionResult<IEnumerable<CampusOrganization2>>> GetCampusOrganizations2Async([FromBody]CampusOrganizationQueryCriteria criteria)
        {
            if(criteria == null)
            {
                var message = "CampusOrganizationQueryCriteria object should not be null.";
                _logger.LogError(message);               
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.BadRequest);
            }

            if (criteria.CampusOrganizationIds == null || !criteria.CampusOrganizationIds.Any())
            {
                var message = "CampusOrganizationIds must contain at least one campus organization id.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await _campusOrganizationService.GetCampusOrganizations2ByCampusOrgIdsAsync(criteria.CampusOrganizationIds));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                var message = "Unexpected error occurred while fetching the requested CampusOrganization2 records.";
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Retrieves all campus organizations.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All campus organizations objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/campus-organizations", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCampusOrganizations", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CampusOrganization>>> GetCampusOrganizationsAsync()
        {
            bool bypassCache = false; 
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var items = await _campusOrganizationService.GetCampusOrganizationsAsync(bypassCache);

                AddEthosContextProperties(
                    await _campusOrganizationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _campusOrganizationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Retrieves a campus organization by ID.
        /// </summary>
        /// <param name="id">Id of campus organization to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CampusOrganization">campus organization.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/campus-organizations/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCampusOrganizationById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.CampusOrganization>> GetCampusOrganizationByIdAsync(string id)
        {
            bool bypassCache = false;
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
                    await _campusOrganizationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _campusOrganizationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));
                return await _campusOrganizationService.GetCampusOrganizationByGuidAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Creates a CampusOrganization.
        /// </summary>
        /// <param name="campusOrganization"><see cref="Dtos.CampusOrganization">CampusOrganization</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.CampusOrganizationType">CampusOrganization</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/campus-organizations", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCampusOrganization")]
        public async Task<ActionResult<Dtos.CampusOrganization>> PostCampusOrganizationAsync([FromBody] Dtos.CampusOrganization campusOrganization)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Updates a campus organization.
        /// </summary>
        /// <param name="id">Id of the CampusOrganization to update</param>
        /// <param name="campusOrganization"><see cref="Dtos.CampusOrganization">CampusOrganization</see> to create</param>
        /// <returns>Updated <see cref="Dtos.CampusOrganization">CampusOrganization</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/campus-organizations/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCampusOrganization")]
        public async Task<ActionResult<Dtos.CampusOrganization>> PutCampusOrganizationAsync([FromRoute] string id, [FromBody] Dtos.CampusOrganization campusOrganization)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Delete (DELETE) an existing campusOrganization
        /// </summary>
        /// <param name="id">Id of the campusOrganization to delete</param>
        [HttpDelete]
        [Route("/campus-organizations/{id}", Name = "DefaultDeleteCampusOrganization", Order = -10)]
        public async Task<IActionResult> DeleteCampusOrganizationAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

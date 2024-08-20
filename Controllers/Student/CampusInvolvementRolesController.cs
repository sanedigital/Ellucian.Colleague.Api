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

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to CampusInvolvementRoles data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.CampusOrgs)]
    public class CampusInvolvementRolesController : BaseCompressedApiController
    {
        private readonly ICampusOrganizationService _campusOrganizationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CampusInvolvementRolesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="campusOrganizationService">Service of type <see cref="ICampusOrganizationService">ICampusOrganizationService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CampusInvolvementRolesController(IAdapterRegistry adapterRegistry, ICampusOrganizationService campusOrganizationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _campusOrganizationService = campusOrganizationService;
            _adapterRegistry = adapterRegistry;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all campus involvement roles.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All campus involvement roles objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/campus-involvement-roles", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCampusInvolvementRoles", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CampusInvolvementRole>>> GetCampusInvolvementRolesAsync()
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
                var items = await _campusOrganizationService.GetCampusInvolvementRolesAsync(bypassCache);

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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a campus involvement role by ID.
        /// </summary>
        /// <param name="id">Id of campus involvement role to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CampusInvolvementRole">campus involvement role.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/campus-involvement-roles/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCampusInvolvementRoleById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.CampusInvolvementRole>> GetCampusInvolvementRoleByIdAsync(string id)
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
                return await _campusOrganizationService.GetCampusInvolvementRoleByGuidAsync(id);
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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Creates a CampusInvolvementRole.
        /// </summary>
        /// <param name="campusInvolvementRole"><see cref="Dtos.CampusInvolvementRole">CampusInvolvementRole</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.CampusInvolvementRole">CampusInvolvementRole</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/campus-involvement-roles", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCampusInvolvementRoles")]
        public async Task<ActionResult<Dtos.CampusInvolvementRole>> PostCampusInvolvementRoleAsync([FromBody] Dtos.CampusInvolvementRole campusInvolvementRole)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Updates a accounting code.
        /// </summary>
        /// <param name="id">Id of the CampusInvolvementRole to update</param>
        /// <param name="campusInvolvementRole"><see cref="Dtos.CampusInvolvementRole">CampusInvolvementRole</see> to create</param>
        /// <returns>Updated <see cref="Dtos.CampusInvolvementRole">CampusInvolvementRole</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/campus-involvement-roles/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCampusInvolvementRoles")]
        public async Task<ActionResult<Dtos.CampusInvolvementRole>> PutCampusInvolvementRoleAsync([FromRoute] string id, [FromBody] Dtos.CampusInvolvementRole campusInvolvementRole)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing campusInvolvementRole
        /// </summary>
        /// <param name="id">Id of the campusInvolvementRole to delete</param>
        [HttpDelete]
        [Route("/campus-involvement-roles/{id}", Name = "DefaultDeleteCampusInvolvementRoles", Order = -10)]
        public async Task<IActionResult> DeleteCampusInvolvementRoleAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

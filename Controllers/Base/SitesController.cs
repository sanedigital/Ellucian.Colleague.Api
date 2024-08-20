// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Sites
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class SitesController : BaseCompressedApiController
    {
        private readonly IFacilitiesService _institutionService;
       private readonly ILogger _logger;

        /// <summary>
        /// SitesController constructor
        /// </summary>
        /// <param name="institutionService">Service of type <see cref="IFacilitiesService">IInstitutionService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SitesController(IFacilitiesService institutionService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _institutionService = institutionService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all sites.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All Site objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/sites", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmSites", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Site2>>> GetHedmSitesAsync()
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
                var items = await _institutionService.GetSites2Async(bypassCache);

                AddEthosContextProperties(
                    await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a site by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.Site">Site.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/sites/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmSiteById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Site2>> GetHedmSiteByIdAsync(string id)
        {
            var bypassCache = true;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var item = await _institutionService.GetSite2Async(id);

                if (item != null)
                {

                    AddEthosContextProperties(await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { item.Id }));
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a Site.
        /// </summary>
        /// <param name="site"><see cref="Dtos.Site2">Site</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.Site2">Site</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/sites", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSitesV6")]
        public async Task<ActionResult<Dtos.Site2>> PostSiteAsync([FromBody] Dtos.Site2 site)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a Site.
        /// </summary>
        /// <param name="id">Id of the Site to update</param>
        /// <param name="site"><see cref="Dtos.Site2">Site</see> to create</param>
        /// <returns>Updated <see cref="Dtos.Site2">Site</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/sites/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSitesV6")]
        public async Task<ActionResult<Dtos.Site2>> PutSiteAsync([FromRoute] string id, [FromBody] Dtos.Site2 site)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Site
        /// </summary>
        /// <param name="id">Id of the Site to delete</param>
        [HttpDelete]
        [Route("/sites/{id}", Name = "DefaultDeleteSite", Order = -10)]
        public async Task<ActionResult<Dtos.Site2>> DeleteSiteAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


    }
}

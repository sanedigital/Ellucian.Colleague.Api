// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Security;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Linq;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to RegionIsoCodes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RegionIsoCodesController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IRegionIsoCodesService _regionIsoCodesService;

        /// <summary>
        /// Initializes a new instance of the RegionIsoCodesController class.
        /// </summary>
        /// <param name="regionIsoCodesService">Service of type <see cref="IRegionIsoCodesService">IRegionIsoCodesService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>      
        /// <param name="apiSettings"></param>
        public RegionIsoCodesController(IRegionIsoCodesService regionIsoCodesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
            this._regionIsoCodesService = regionIsoCodesService;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all region-iso-codes
        /// </summary>
        /// <returns>All region ISO codes<see cref="Dtos.RegionIsoCodes">RegionIsoCodes</see></returns>               
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.RegionIsoCodes))]
        [HttpGet]
        [HeaderVersionRoute("/region-iso-codes", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRegionIsoCodes", IsEedmSupported = true)]
        //public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.RegionIsoCodes>>> GetRegionIsoCodesAsync()
        //public async Task<IActionResult> GetRegionIsoCodesAsync(QueryStringFilter criteria)
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.RegionIsoCodes>>> GetRegionIsoCodesAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var criteriaFilter = GetFilterObject<Dtos.RegionIsoCodes>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return null;
                //return new PagedActionResult<IEnumerable<Dtos.RegionIsoCodes>>(new List<Dtos.RegionIsoCodes>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);


            try
            {
                var regionIsoCodes = await _regionIsoCodesService.GetRegionIsoCodesAsync(criteriaFilter, bypassCache);

                if (regionIsoCodes != null && regionIsoCodes.Any())
                {
                    AddEthosContextProperties(await _regionIsoCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _regionIsoCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              regionIsoCodes.Select(a => a.Id).ToList()));
                }
                return Ok(regionIsoCodes);
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
        /// Retrieve (GET) an existing region-iso-codes
        /// </summary>
        /// <param name="guid">GUID of the region-iso-codes to get</param>
        /// <returns>A regionIsoCodes object <see cref="Dtos.RegionIsoCodes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/region-iso-codes/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRegionIsoCodesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.RegionIsoCodes>> GetRegionIsoCodesByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _regionIsoCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _regionIsoCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _regionIsoCodesService.GetRegionIsoCodesByGuidAsync(guid);
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
        /// Create (POST) a new regionIsoCodes
        /// </summary>
        /// <param name="regionIsoCodes">DTO of the new regionIsoCodes</param>
        /// <returns>A regionIsoCodes object <see cref="Dtos.RegionIsoCodes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/region-iso-codes", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRegionIsoCodesV100")]
        public async Task<ActionResult<Dtos.RegionIsoCodes>> PostRegionIsoCodesAsync([FromBody] Dtos.RegionIsoCodes regionIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing regionIsoCodes
        /// </summary>
        /// <param name="guid">GUID of the regionIsoCodes to update</param>
        /// <param name="regionIsoCodes">DTO of the updated regionIsoCodes</param>
        /// <returns>A regionIsoCodes object <see cref="Dtos.RegionIsoCodes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/region-iso-codes/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRegionIsoCodesV100")]
        public async Task<ActionResult<Dtos.RegionIsoCodes>> PutRegionIsoCodesAsync([FromRoute] string guid, [FromBody] Dtos.RegionIsoCodes regionIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a regionIsoCodes
        /// </summary>
        /// <param name="guid">GUID to desired regionIsoCodes</param>
        [HttpDelete]
        [Route("/region-iso-codes/{guid}", Name = "DefaultDeleteRegionIsoCodes", Order = -10)]
        public async Task<IActionResult> DeleteRegionIsoCodesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

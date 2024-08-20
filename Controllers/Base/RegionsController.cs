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
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Regions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RegionsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RegionsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RegionsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all regions
        /// </summary>
        /// <returns>All <see cref="Dtos.Regions">Regions</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/regions", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRegions", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Regions>>> GetRegionsAsync()
        {
            return new List<Regions>();
        }

        /// <summary>
        /// Retrieve (GET) an existing regions
        /// </summary>
        /// <param name="guid">GUID of the regions to get</param>
        /// <returns>A regions object <see cref="Dtos.Regions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/regions/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRegionsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Regions>> GetRegionsByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No regions was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new regions
        /// </summary>
        /// <param name="regions">DTO of the new regions</param>
        /// <returns>A regions object <see cref="Dtos.Regions"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/regions", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRegionsV1.0.0")]
        public async Task<ActionResult<Dtos.Regions>> PostRegionsAsync([FromBody] Dtos.Regions regions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing regions
        /// </summary>
        /// <param name="guid">GUID of the regions to update</param>
        /// <param name="regions">DTO of the updated regions</param>
        /// <returns>A regions object <see cref="Dtos.Regions"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/regions/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRegionsV1.0.0")]
        public async Task<ActionResult<Dtos.Regions>> PutRegionsAsync([FromRoute] string guid, [FromBody] Dtos.Regions regions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a regions
        /// </summary>
        /// <param name="guid">GUID to desired regions</param>
        [HttpDelete]
        [Route("/regions/{guid}", Name = "DefaultDeleteRegions", Order = -10)]
        public async Task<IActionResult> DeleteRegionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

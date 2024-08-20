// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to ProspectOpportunitySources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ProspectOpportunitySourcesController : BaseCompressedApiController
    {
        
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ProspectOpportunitySourcesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ProspectOpportunitySourcesController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all prospect-opportunity-sources
        /// </summary>
        /// <returns>All <see cref="Dtos.ProspectOpportunitySources">ProspectOpportunitySources</see></returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/prospect-opportunity-sources", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetProspectOpportunitySources", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.ProspectOpportunitySources>>> GetProspectOpportunitySourcesAsync()
        {
            return new List<Dtos.ProspectOpportunitySources>();
        }

        /// <summary>
        /// Retrieve (GET) an existing prospect-opportunity-sources
        /// </summary>
        /// <param name="guid">GUID of the prospect-opportunity-sources to get</param>
        /// <returns>A prospectOpportunitySources object <see cref="Dtos.ProspectOpportunitySources"/> in EEDM format</returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/prospect-opportunity-sources/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetProspectOpportunitySourcesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ProspectOpportunitySources>> GetProspectOpportunitySourcesByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No prospect-opportunity-sources was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new prospectOpportunitySources
        /// </summary>
        /// <param name="prospectOpportunitySources">DTO of the new prospectOpportunitySources</param>
        /// <returns>A prospectOpportunitySources object <see cref="Dtos.ProspectOpportunitySources"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/prospect-opportunity-sources", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostProspectOpportunitySourcesV100")]
        public async Task<ActionResult<Dtos.ProspectOpportunitySources>> PostProspectOpportunitySourcesAsync([FromBody] Dtos.ProspectOpportunitySources prospectOpportunitySources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing prospectOpportunitySources
        /// </summary>
        /// <param name="guid">GUID of the prospectOpportunitySources to update</param>
        /// <param name="prospectOpportunitySources">DTO of the updated prospectOpportunitySources</param>
        /// <returns>A prospectOpportunitySources object <see cref="Dtos.ProspectOpportunitySources"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/prospect-opportunity-sources/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutProspectOpportunitySourcesV100")]
        public async Task<ActionResult<Dtos.ProspectOpportunitySources>> PutProspectOpportunitySourcesAsync([FromRoute] string guid, [FromBody] Dtos.ProspectOpportunitySources prospectOpportunitySources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a prospectOpportunitySources
        /// </summary>
        /// <param name="guid">GUID to desired prospectOpportunitySources</param>
        [HttpDelete]
        [Route("/prospect-opportunity-sources/{guid}", Name = "DefaultDeleteProspectOpportunitySources", Order = -10)]
        public async Task<IActionResult> DeleteProspectOpportunitySourcesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

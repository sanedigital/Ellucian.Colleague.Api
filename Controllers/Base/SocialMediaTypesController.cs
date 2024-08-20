// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;

using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Threading.Tasks;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Controller for Social Media Types
    /// </summary>
     [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class SocialMediaTypesController : BaseCompressedApiController
    {
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the  Social Media Types Controller class.
        /// </summary>
        /// <param name="socialMediaTypeService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SocialMediaTypesController(IDemographicService socialMediaTypeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _demographicService = socialMediaTypeService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Social Media Types
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Dtos.SocialMediaType">Social Media Types.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/social-media-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetSocialMediaTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.SocialMediaType>>> GetSocialMediaTypesAsync()
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
                var socialMediaTypes = await _demographicService.GetSocialMediaTypesAsync(bypassCache);

                if (socialMediaTypes != null && socialMediaTypes.Any())
                {
                    AddEthosContextProperties(await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              socialMediaTypes.Select(a => a.Id).ToList()));
                }
                return Ok(socialMediaTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Social Media Type by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.SocialMediaType">Social Media Type.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/social-media-types/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetSocialMediaTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SocialMediaType>> GetSocialMediaTypeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _demographicService.GetSocialMediaTypeByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>        
        /// Creates an Social Media Type
        /// </summary>
        /// <param name="socialMediaType"><see cref="Dtos.SocialMediaType">SocialMediaType</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.SocialMediaType">SocialMediaType</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/social-media-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSocialMediaTypeV6")]
        public async Task<ActionResult<Dtos.SocialMediaType>> PostSocialMediaTypeAsync([FromBody] Dtos.SocialMediaType socialMediaType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>        
        /// Updates an SocialMediaType.
        /// </summary>
        /// <param name="id">Id of the Social Media Type to update</param>
        /// <param name="socialMediaType"><see cref="Dtos.SocialMediaType">SocialMediaType</see> to create</param>
        /// <returns>Updated <see cref="Dtos.SocialMediaType">SocialMediaType</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/social-media-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSocialMediaTypeV6")]
        public async Task<ActionResult<Dtos.SocialMediaType>> PutSocialMediaTypeAsync([FromRoute] string id, [FromBody] Dtos.SocialMediaType socialMediaType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Social Media Type
        /// </summary>
        /// <param name="id">Id of the  Social Media Type to delete</param>
        [HttpDelete]
        [Route("/social-media-types/{id}", Name = "DeleteSocialMediaType", Order = -10)]
        public async Task<IActionResult> DeleteSocialMediaTypeAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;

using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to GenderIdentityType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class GenderIdentityTypesController : BaseCompressedApiController
    {
        private readonly IGenderIdentityTypeService _genderIdentityTypeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GenderIdentityTypesController class.
        /// </summary>
        /// <param name="genderIdentityTypeService">Service of type <see cref="IGenderIdentityTypeService">IGenderIdentityTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GenderIdentityTypesController(IGenderIdentityTypeService genderIdentityTypeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _genderIdentityTypeService = genderIdentityTypeService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves gender identity types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.GenderIdentityType">GenderIdentityType</see> objects></returns>
        [HttpGet]
        [HeaderVersionRoute("/gender-identity-types", 1, true, Name = "GetGenderIdentityTypes")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.GenderIdentityType>>> GetAsync()
        {
            try
            {
                bool ignoreCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        ignoreCache = true;
                    }
                }
                return Ok(await _genderIdentityTypeService.GetBaseGenderIdentityTypesAsync(ignoreCache));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Return all genderIdentities
        /// </summary>
        /// <returns>List of GenderIdentities <see cref="Dtos.GenderIdentities"/> objects representing matching genderIdentities</returns>         
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/gender-identities", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetGenderIdentities", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.GenderIdentities>>> GetGenderIdentitiesAsync()
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
                var genderIdentities = await _genderIdentityTypeService.GetGenderIdentitiesAsync(bypassCache);

                if (genderIdentities != null && genderIdentities.Any())
                {
                    AddEthosContextProperties(await _genderIdentityTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _genderIdentityTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              genderIdentities.Select(a => a.Id).ToList()));
                }
                return Ok(genderIdentities);
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
            catch (IntegrationApiException e)
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

        /// <summary>
        /// Read (GET) a genderIdentities using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired genderIdentities</param>
        /// <returns>A genderIdentities object <see cref="Dtos.GenderIdentities"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/gender-identities/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetGenderIdentitiesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GenderIdentities>> GetGenderIdentitiesByGuidAsync(string guid)
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
                   await _genderIdentityTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _genderIdentityTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _genderIdentityTypeService.GetGenderIdentitiesByGuidAsync(guid);
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
            catch (IntegrationApiException e)
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

        /// <summary>
        /// Create (POST) a new genderIdentities
        /// </summary>
        /// <param name="genderIdentities">DTO of the new genderIdentities</param>
        /// <returns>A genderIdentities object <see cref="Dtos.GenderIdentities"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/gender-identities", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGenderIdentitiesV1.0.0")]
        public async Task<ActionResult<Dtos.GenderIdentities>> PostGenderIdentitiesAsync([FromBody] Dtos.GenderIdentities genderIdentities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing genderIdentities
        /// </summary>
        /// <param name="guid">GUID of the genderIdentities to update</param>
        /// <param name="genderIdentities">DTO of the updated genderIdentities</param>
        /// <returns>A genderIdentities object <see cref="Dtos.GenderIdentities"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/gender-identities/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGenderIdentitiesV1.0.0")]
        public async Task<ActionResult<Dtos.GenderIdentities>> PutGenderIdentitiesAsync([FromRoute] string guid, [FromBody] Dtos.GenderIdentities genderIdentities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a genderIdentities
        /// </summary>
        /// <param name="guid">GUID to desired genderIdentities</param>
        [HttpDelete]
        [Route("/gender-identities/{guid}", Name = "DefaultDeleteGenderIdentities", Order = -10)]
        public async Task<IActionResult> DeleteGenderIdentitiesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

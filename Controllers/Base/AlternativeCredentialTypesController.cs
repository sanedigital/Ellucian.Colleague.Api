// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to AlternativeCredentialTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AlternativeCredentialTypesController : BaseCompressedApiController
    {
        private readonly IAlternativeCredentialTypesService _alternativeCredentialTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AlternativeCredentialTypesController class.
        /// </summary>
        /// <param name="alternativeCredentialTypesService">Service of type <see cref="IAlternativeCredentialTypesService">IAlternativeCredentialTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AlternativeCredentialTypesController(IAlternativeCredentialTypesService alternativeCredentialTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _alternativeCredentialTypesService = alternativeCredentialTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all alternativeCredentialTypes
        /// </summary>
        /// <returns>List of AlternativeCredentialTypes <see cref="Dtos.AlternativeCredentialTypes"/> objects representing matching alternativeCredentialTypes</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/alternative-credential-types", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAlternativeCredentialTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.AlternativeCredentialTypes>>> GetAlternativeCredentialTypesAsync()
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
                var alternativeCredentialTypes = await _alternativeCredentialTypesService.GetAlternativeCredentialTypesAsync(bypassCache);

                if (alternativeCredentialTypes != null && alternativeCredentialTypes.Any())
                {
                    AddEthosContextProperties(await _alternativeCredentialTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _alternativeCredentialTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              alternativeCredentialTypes.Select(a => a.Id).ToList()));
                }
                return Ok(alternativeCredentialTypes);
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
        /// Read (GET) a alternativeCredentialTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired alternativeCredentialTypes</param>
        /// <returns>A alternativeCredentialTypes object <see cref="Dtos.AlternativeCredentialTypes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/alternative-credential-types/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAlternativeCredentialTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AlternativeCredentialTypes>> GetAlternativeCredentialTypesByGuidAsync(string guid)
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
                   await _alternativeCredentialTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _alternativeCredentialTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return Ok(await _alternativeCredentialTypesService.GetAlternativeCredentialTypesByGuidAsync(guid));
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
        /// Create (POST) a new alternativeCredentialTypes
        /// </summary>
        /// <param name="alternativeCredentialTypes">DTO of the new alternativeCredentialTypes</param>
        /// <returns>A alternativeCredentialTypes object <see cref="Dtos.AlternativeCredentialTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/alternative-credential-types", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAlternativeCredentialTypesV100")]
        public async Task<ActionResult<Dtos.AlternativeCredentialTypes>> PostAlternativeCredentialTypesAsync([FromBody] Dtos.AlternativeCredentialTypes alternativeCredentialTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing alternativeCredentialTypes
        /// </summary>
        /// <param name="guid">GUID of the alternativeCredentialTypes to update</param>
        /// <param name="alternativeCredentialTypes">DTO of the updated alternativeCredentialTypes</param>
        /// <returns>A alternativeCredentialTypes object <see cref="Dtos.AlternativeCredentialTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/alternative-credential-types/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAlternativeCredentialTypesV100")]
        public async Task<ActionResult<Dtos.AlternativeCredentialTypes>> PutAlternativeCredentialTypesAsync([FromRoute] string guid, [FromBody] Dtos.AlternativeCredentialTypes alternativeCredentialTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a alternativeCredentialTypes
        /// </summary>
        /// <param name="guid">GUID to desired alternativeCredentialTypes</param>
        [HttpDelete]
        [Route("/alternative-credential-types/{guid}", Name = "DefaultDeleteAlternativeCredentialTypes", Order = -10)]
        public async Task<IActionResult> DeleteAlternativeCredentialTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

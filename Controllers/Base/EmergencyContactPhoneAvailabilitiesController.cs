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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to EmergencyContactPhoneAvailabilities
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EmergencyContactPhoneAvailabilitiesController : BaseCompressedApiController
    {
        private readonly IEmergencyContactPhoneAvailabilitiesService _emergencyContactPhoneAvailabilitiesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmergencyContactPhoneAvailabilitiesController class.
        /// </summary>
        /// <param name="emergencyContactPhoneAvailabilitiesService">Service of type <see cref="IEmergencyContactPhoneAvailabilitiesService">IEmergencyContactPhoneAvailabilitiesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmergencyContactPhoneAvailabilitiesController(IEmergencyContactPhoneAvailabilitiesService emergencyContactPhoneAvailabilitiesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _emergencyContactPhoneAvailabilitiesService = emergencyContactPhoneAvailabilitiesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all emergencyContactPhoneAvailabilities
        /// </summary>
        /// <returns>List of EmergencyContactPhoneAvailabilities <see cref="Dtos.EmergencyContactPhoneAvailabilities"/> objects representing matching emergencyContactPhoneAvailabilities</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/emergency-contact-phone-availabilities", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmergencyContactPhoneAvailabilities", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.EmergencyContactPhoneAvailabilities>>> GetEmergencyContactPhoneAvailabilitiesAsync()
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
                var emergencyContactPhoneAvailabilities = await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesAsync(bypassCache);

                if (emergencyContactPhoneAvailabilities != null && emergencyContactPhoneAvailabilities.Any())
                {
                    AddEthosContextProperties(await _emergencyContactPhoneAvailabilitiesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _emergencyContactPhoneAvailabilitiesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              emergencyContactPhoneAvailabilities.Select(a => a.Id).ToList()));
                }
                return Ok(emergencyContactPhoneAvailabilities);
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
        /// Read (GET) a emergencyContactPhoneAvailabilities using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired emergencyContactPhoneAvailabilities</param>
        /// <returns>A emergencyContactPhoneAvailabilities object <see cref="Dtos.EmergencyContactPhoneAvailabilities"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/emergency-contact-phone-availabilities/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmergencyContactPhoneAvailabilitiesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmergencyContactPhoneAvailabilities>> GetEmergencyContactPhoneAvailabilitiesByGuidAsync(string guid)
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
                   await _emergencyContactPhoneAvailabilitiesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _emergencyContactPhoneAvailabilitiesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(guid);
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
        /// Create (POST) a new emergencyContactPhoneAvailabilities
        /// </summary>
        /// <param name="emergencyContactPhoneAvailabilities">DTO of the new emergencyContactPhoneAvailabilities</param>
        /// <returns>A emergencyContactPhoneAvailabilities object <see cref="Dtos.EmergencyContactPhoneAvailabilities"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/emergency-contact-phone-availabilities", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEmergencyContactPhoneAvailabilitiesV1.0.0")]
        public async Task<ActionResult<Dtos.EmergencyContactPhoneAvailabilities>> PostEmergencyContactPhoneAvailabilitiesAsync([FromBody] Dtos.EmergencyContactPhoneAvailabilities emergencyContactPhoneAvailabilities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing emergencyContactPhoneAvailabilities
        /// </summary>
        /// <param name="guid">GUID of the emergencyContactPhoneAvailabilities to update</param>
        /// <param name="emergencyContactPhoneAvailabilities">DTO of the updated emergencyContactPhoneAvailabilities</param>
        /// <returns>A emergencyContactPhoneAvailabilities object <see cref="Dtos.EmergencyContactPhoneAvailabilities"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/emergency-contact-phone-availabilities/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmergencyContactPhoneAvailabilitiesV1.0.0")]
        public async Task<ActionResult<Dtos.EmergencyContactPhoneAvailabilities>> PutEmergencyContactPhoneAvailabilitiesAsync([FromRoute] string guid, [FromBody] Dtos.EmergencyContactPhoneAvailabilities emergencyContactPhoneAvailabilities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a emergencyContactPhoneAvailabilities
        /// </summary>
        /// <param name="guid">GUID to desired emergencyContactPhoneAvailabilities</param>
        [HttpDelete]
        [Route("/emergency-contact-phone-availabilities/{guid}", Name = "DefaultDeleteEmergencyContactPhoneAvailabilities", Order = -10)]
        public async Task<IActionResult> DeleteEmergencyContactPhoneAvailabilitiesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

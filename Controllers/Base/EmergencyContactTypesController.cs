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
    /// Provides access to EmergencyContactTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EmergencyContactTypesController : BaseCompressedApiController
    {
        private readonly IEmergencyContactTypesService _emergencyContactTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmergencyContactTypesController class.
        /// </summary>
        /// <param name="emergencyContactTypesService">Service of type <see cref="IEmergencyContactTypesService">IEmergencyContactTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmergencyContactTypesController(IEmergencyContactTypesService emergencyContactTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _emergencyContactTypesService = emergencyContactTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all emergencyContactTypes
        /// </summary>
        /// <returns>List of EmergencyContactTypes <see cref="Dtos.EmergencyContactTypes"/> objects representing matching emergencyContactTypes</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/emergency-contact-types", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmergencyContactTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.EmergencyContactTypes>>> GetEmergencyContactTypesAsync()
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
                var emergencyContactTypes = await _emergencyContactTypesService.GetEmergencyContactTypesAsync(bypassCache);

                if (emergencyContactTypes != null && emergencyContactTypes.Any())
                {
                    AddEthosContextProperties(await _emergencyContactTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _emergencyContactTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              emergencyContactTypes.Select(a => a.Id).ToList()));
                }
                return Ok(emergencyContactTypes);
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
        /// Read (GET) a emergencyContactTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired emergencyContactTypes</param>
        /// <returns>A emergencyContactTypes object <see cref="Dtos.EmergencyContactTypes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/emergency-contact-types/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmergencyContactTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmergencyContactTypes>> GetEmergencyContactTypesByGuidAsync(string guid)
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
                   await _emergencyContactTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _emergencyContactTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _emergencyContactTypesService.GetEmergencyContactTypesByGuidAsync(guid);
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
        /// Create (POST) a new emergencyContactTypes
        /// </summary>
        /// <param name="emergencyContactTypes">DTO of the new emergencyContactTypes</param>
        /// <returns>A emergencyContactTypes object <see cref="Dtos.EmergencyContactTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/emergency-contact-types", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEmergencyContactTypesV1.0.0")]
        public async Task<ActionResult<Dtos.EmergencyContactTypes>> PostEmergencyContactTypesAsync([FromBody] Dtos.EmergencyContactTypes emergencyContactTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing emergencyContactTypes
        /// </summary>
        /// <param name="guid">GUID of the emergencyContactTypes to update</param>
        /// <param name="emergencyContactTypes">DTO of the updated emergencyContactTypes</param>
        /// <returns>A emergencyContactTypes object <see cref="Dtos.EmergencyContactTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/emergency-contact-types/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmergencyContactTypesV1.0.0")]
        public async Task<ActionResult<Dtos.EmergencyContactTypes>> PutEmergencyContactTypesAsync([FromRoute] string guid, [FromBody] Dtos.EmergencyContactTypes emergencyContactTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a emergencyContactTypes
        /// </summary>
        /// <param name="guid">GUID to desired emergencyContactTypes</param>
        [HttpDelete]
        [Route("/emergency-contact-types/{guid}", Name = "DefaultDeleteEmergencyContactTypes", Order = -10)]
        public async Task<IActionResult> DeleteEmergencyContactTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

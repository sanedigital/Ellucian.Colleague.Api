// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionApplicationStatusTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationStatusTypesController : BaseCompressedApiController
    {
        private readonly IAdmissionDecisionTypesService _admissionApplicationStatusTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationStatusTypesController class.
        /// </summary>
        /// <param name="admissionApplicationStatusTypesService">Service of type <see cref="IAdmissionDecisionTypesService">IAdmissionApplicationStatusTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdmissionApplicationStatusTypesController(IAdmissionDecisionTypesService admissionApplicationStatusTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _admissionApplicationStatusTypesService = admissionApplicationStatusTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all admissionApplicationStatusTypes v6.
        /// </summary>
        /// <returns>List of AdmissionApplicationStatusTypes <see cref="Dtos.AdmissionApplicationStatusType"/> objects representing matching admissionApplicationStatusTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/admission-application-status-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAdmissionApplicationStatusTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationStatusType>>> GetAdmissionApplicationStatusTypesAsync()
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
                var statusTypeEntities = await _admissionApplicationStatusTypesService.GetAdmissionApplicationStatusTypesAsync(bypassCache);

                AddEthosContextProperties(
                    await _admissionApplicationStatusTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _admissionApplicationStatusTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        statusTypeEntities.Select(i => i.Id).ToList()));

                return Ok(statusTypeEntities);
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
        /// Read (GET) a admissionApplicationStatusTypes using a GUID v6.
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationStatusTypes</param>
        /// <returns>A admissionApplicationStatusTypes object <see cref="Dtos.AdmissionApplicationStatusType"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/admission-application-status-types/{guid}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplicationStatusTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplicationStatusType>> GetAdmissionApplicationStatusTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
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
                AddEthosContextProperties(
                    await _admissionApplicationStatusTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _admissionApplicationStatusTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _admissionApplicationStatusTypesService.GetAdmissionApplicationStatusTypesByGuidAsync(guid);
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
        /// Create (POST) a new admissionApplicationStatusTypes
        /// </summary>
        /// <param name="admissionApplicationStatusTypes">DTO of the new admissionApplicationStatusTypes</param>
        /// <returns>A admissionApplicationStatusTypes object <see cref="Dtos.AdmissionApplicationStatusType"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/admission-application-status-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionApplicationStatusTypesV6")]
        public async Task<ActionResult<Dtos.AdmissionApplicationStatusType>> PostAdmissionApplicationStatusTypesAsync([FromBody] Dtos.AdmissionApplicationStatusType admissionApplicationStatusTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionApplicationStatusTypes
        /// </summary>
        /// <param name="guid">GUID of the admissionApplicationStatusTypes to update</param>
        /// <param name="admissionApplicationStatusTypes">DTO of the updated admissionApplicationStatusTypes</param>
        /// <returns>A admissionApplicationStatusTypes object <see cref="Dtos.AdmissionApplicationStatusType"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/admission-application-status-types/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionApplicationStatusTypesV6")]
        public async Task<ActionResult<Dtos.AdmissionApplicationStatusType>> PutAdmissionApplicationStatusTypesAsync([FromRoute] string guid, [FromBody] Dtos.AdmissionApplicationStatusType admissionApplicationStatusTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionApplicationStatusTypes
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationStatusTypes</param>
        [HttpDelete]
        [Route("/admission-application-status-types/{guid}", Name = "DefaultDeleteAdmissionApplicationStatusTypes")]
        public async Task<IActionResult> DeleteAdmissionApplicationStatusTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

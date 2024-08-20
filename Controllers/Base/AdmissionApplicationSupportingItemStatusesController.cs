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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to AdmissionApplicationSupportingItemStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AdmissionApplicationSupportingItemStatusesController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationSupportingItemStatusesService _admissionApplicationSupportingItemStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationSupportingItemStatusesController class.
        /// </summary>
        /// <param name="admissionApplicationSupportingItemStatusesService">Service of type <see cref="IAdmissionApplicationSupportingItemStatusesService">IAdmissionApplicationSupportingItemStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdmissionApplicationSupportingItemStatusesController(IAdmissionApplicationSupportingItemStatusesService admissionApplicationSupportingItemStatusesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _admissionApplicationSupportingItemStatusesService = admissionApplicationSupportingItemStatusesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all admissionApplicationSupportingItemStatuses
        /// </summary>
        /// <returns>List of AdmissionApplicationSupportingItemStatuses <see cref="Dtos.AdmissionApplicationSupportingItemStatus"/> objects representing matching admissionApplicationSupportingItemStatuses</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/admission-application-supporting-item-statuses", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAdmissionApplicationSupportingItemStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemStatus>>> GetAdmissionApplicationSupportingItemStatusesAsync()
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
                var admissionApplicationSupportingItemStatuses = await _admissionApplicationSupportingItemStatusesService.GetAdmissionApplicationSupportingItemStatusesAsync(bypassCache);

                if (admissionApplicationSupportingItemStatuses != null && admissionApplicationSupportingItemStatuses.Any())
                {
                    AddEthosContextProperties(await _admissionApplicationSupportingItemStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _admissionApplicationSupportingItemStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              admissionApplicationSupportingItemStatuses.Select(a => a.Id).ToList()));
                }
                return Ok(admissionApplicationSupportingItemStatuses);
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
        /// Read (GET) a admissionApplicationSupportingItemStatus using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItemStatus</param>
        /// <returns>A admissionApplicationSupportingItemStatus object <see cref="Dtos.AdmissionApplicationSupportingItemStatus"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/admission-application-supporting-item-statuses/{guid}", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplicationSupportingItemStatusesByGuid")]
        public async Task<ActionResult<Dtos.AdmissionApplicationSupportingItemStatus>> GetAdmissionApplicationSupportingItemStatusByGuidAsync(string guid)
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
                   await _admissionApplicationSupportingItemStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _admissionApplicationSupportingItemStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return Ok(await _admissionApplicationSupportingItemStatusesService.GetAdmissionApplicationSupportingItemStatusByGuidAsync(guid));
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
        /// Create (POST) a new admissionApplicationSupportingItemStatus
        /// </summary>
        /// <param name="admissionApplicationSupportingItemStatus">DTO of the new admissionApplicationSupportingItemStatus</param>
        /// <returns>A admissionApplicationSupportingItemStatus object <see cref="Dtos.AdmissionApplicationSupportingItemStatus"/> in EEDM format</returns>
        [HttpPost]
        [Route("/admission-application-supporting-item-statuses", Name = "PostAdmissionApplicationSupportingItemStatusesV9", Order = -10)]
        public async Task<ActionResult<Dtos.AdmissionApplicationSupportingItemStatus>> PostAdmissionApplicationSupportingItemStatusAsync([FromBody] Dtos.AdmissionApplicationSupportingItemStatus admissionApplicationSupportingItemStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionApplicationSupportingItemStatus
        /// </summary>
        /// <param name="guid">GUID of the admissionApplicationSupportingItemStatus to update</param>
        /// <param name="admissionApplicationSupportingItemStatus">DTO of the updated admissionApplicationSupportingItemStatus</param>
        /// <returns>A admissionApplicationSupportingItemStatus object <see cref="Dtos.AdmissionApplicationSupportingItemStatus"/> in EEDM format</returns>
        [HttpPut]
        [Route("/admission-application-supporting-item-statuses/{guid}", Name = "PutAdmissionApplicationSupportingItemStatusesV9", Order = -10)]
        public async Task<ActionResult<Dtos.AdmissionApplicationSupportingItemStatus>> PutAdmissionApplicationSupportingItemStatusAsync([FromRoute] string guid, [FromBody] Dtos.AdmissionApplicationSupportingItemStatus admissionApplicationSupportingItemStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionApplicationSupportingItemStatus
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItemStatus</param>
        [HttpDelete]
        [Route("/admission-application-supporting-item-statuses/{guid}", Name = "DefaultDeleteAdmissionApplicationSupportingItemStatuses", Order = -10)]
        public async Task<IActionResult> DeleteAdmissionApplicationSupportingItemStatusAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

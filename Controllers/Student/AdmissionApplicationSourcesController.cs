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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;

using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionApplicationSources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationSourcesController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationSourcesService _admissionApplicationSourcesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationSourcesController class.
        /// </summary>
        /// <param name="admissionApplicationSourcesService">Service of type <see cref="IAdmissionApplicationSourcesService">IAdmissionApplicationSourcesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdmissionApplicationSourcesController(IAdmissionApplicationSourcesService admissionApplicationSourcesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _admissionApplicationSourcesService = admissionApplicationSourcesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all admissionApplicationSources
        /// </summary>
        /// <returns>List of AdmissionApplicationSources <see cref="Dtos.AdmissionApplicationSources"/> objects representing matching admissionApplicationSources</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/admission-application-sources", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplicationSources", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSources>>> GetAdmissionApplicationSourcesAsync()
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
                var admissionApplicationSources = await _admissionApplicationSourcesService.GetAdmissionApplicationSourcesAsync(bypassCache);

                if (admissionApplicationSources != null && admissionApplicationSources.Any())
                {
                    AddEthosContextProperties(await _admissionApplicationSourcesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _admissionApplicationSourcesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              admissionApplicationSources.Select(a => a.Id).ToList()));
                }
                return Ok(admissionApplicationSources);
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
        /// Read (GET) a admissionApplicationSources using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSources</param>
        /// <returns>A admissionApplicationSources object <see cref="Dtos.AdmissionApplicationSources"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/admission-application-sources/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplicationSourcesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplicationSources>> GetAdmissionApplicationSourcesByGuidAsync(string guid)
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
                   await _admissionApplicationSourcesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _admissionApplicationSourcesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _admissionApplicationSourcesService.GetAdmissionApplicationSourcesByGuidAsync(guid);
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
        /// Create (POST) a new admissionApplicationSources
        /// </summary>
        /// <param name="admissionApplicationSources">DTO of the new admissionApplicationSources</param>
        /// <returns>A admissionApplicationSources object <see cref="Dtos.AdmissionApplicationSources"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/admission-application-sources", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionApplicationSourcesV1.0.0")]
        public async Task<ActionResult<Dtos.AdmissionApplicationSources>> PostAdmissionApplicationSourcesAsync([FromBody] Dtos.AdmissionApplicationSources admissionApplicationSources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionApplicationSources
        /// </summary>
        /// <param name="guid">GUID of the admissionApplicationSources to update</param>
        /// <param name="admissionApplicationSources">DTO of the updated admissionApplicationSources</param>
        /// <returns>A admissionApplicationSources object <see cref="Dtos.AdmissionApplicationSources"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/admission-application-sources/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionApplicationSourcesV1.0.0")]
        public async Task<ActionResult<Dtos.AdmissionApplicationSources>> PutAdmissionApplicationSourcesAsync([FromRoute] string guid, [FromBody] Dtos.AdmissionApplicationSources admissionApplicationSources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionApplicationSources
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSources</param>
        [HttpDelete]
        [Route("/admission-application-sources/{guid}", Name = "DefaultDeleteAdmissionApplicationSources")]
        public async Task<IActionResult> DeleteAdmissionApplicationSourcesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
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


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionResidencyTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionResidencyTypesController : BaseCompressedApiController
    {
        private readonly IAdmissionResidencyTypesService _admissionResidencyTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionResidencyTypesController class.
        /// </summary>
        /// <param name="admissionResidencyTypesService">Service of type <see cref="IAdmissionResidencyTypesService">IAdmissionResidencyTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdmissionResidencyTypesController(IAdmissionResidencyTypesService admissionResidencyTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _admissionResidencyTypesService = admissionResidencyTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all admissionResidencyTypes
        /// </summary>
        /// <returns>List of AdmissionResidencyTypes <see cref="Dtos.AdmissionResidencyTypes"/> objects representing matching admissionResidencyTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/admission-residency-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAdmissionResidencyTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AdmissionResidencyTypes>>> GetAdmissionResidencyTypesAsync()
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
                var pageOfItems = await _admissionResidencyTypesService.GetAdmissionResidencyTypesAsync(bypassCache);

                AddEthosContextProperties(
                  await _admissionResidencyTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionResidencyTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Select(i => i.Id).Distinct().ToList()));

                return Ok(pageOfItems);
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
        /// Read (GET) a admissionResidencyTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionResidencyTypes</param>
        /// <returns>A admissionResidencyTypes object <see cref="Dtos.AdmissionResidencyTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/admission-residency-types/{guid}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionResidencyTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionResidencyTypes>> GetAdmissionResidencyTypesByGuidAsync(string guid)
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
                  await _admissionResidencyTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionResidencyTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));

                return await _admissionResidencyTypesService.GetAdmissionResidencyTypesByGuidAsync(guid);
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
        /// Create (POST) a new admissionResidencyTypes
        /// </summary>
        /// <param name="admissionResidencyTypes">DTO of the new admissionResidencyTypes</param>
        /// <returns>A admissionResidencyTypes object <see cref="Dtos.AdmissionResidencyTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/admission-residency-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionResidencyTypesV6")]
        public async Task<ActionResult<Dtos.AdmissionResidencyTypes>> PostAdmissionResidencyTypesAsync([FromBody] Dtos.AdmissionResidencyTypes admissionResidencyTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionResidencyTypes
        /// </summary>
        /// <param name="guid">GUID of the admissionResidencyTypes to update</param>
        /// <param name="admissionResidencyTypes">DTO of the updated admissionResidencyTypes</param>
        /// <returns>A admissionResidencyTypes object <see cref="Dtos.AdmissionResidencyTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/admission-residency-types/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionResidencyTypesV6")]
        public async Task<ActionResult<Dtos.AdmissionResidencyTypes>> PutAdmissionResidencyTypesAsync([FromRoute] string guid, [FromBody] Dtos.AdmissionResidencyTypes admissionResidencyTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionResidencyTypes
        /// </summary>
        /// <param name="guid">GUID to desired admissionResidencyTypes</param>
        [HttpDelete]
        [Route("/admission-residency-types/{guid}", Name = "DefaultDeleteAdmissionResidencyTypes")]
        public async Task<IActionResult> DeleteAdmissionResidencyTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

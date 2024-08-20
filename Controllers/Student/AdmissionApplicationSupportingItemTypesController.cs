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

using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionApplicationSupportingItemTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationSupportingItemTypesController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationSupportingItemTypesService _admissionApplicationSupportingItemTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationSupportingItemTypesController class.
        /// </summary>
        /// <param name="admissionApplicationSupportingItemTypesService">Service of type <see cref="IAdmissionApplicationSupportingItemTypesService">IAdmissionApplicationSupportingItemTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdmissionApplicationSupportingItemTypesController(IAdmissionApplicationSupportingItemTypesService admissionApplicationSupportingItemTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _admissionApplicationSupportingItemTypesService = admissionApplicationSupportingItemTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all admissionApplicationSupportingItemTypes
        /// </summary>
        /// <returns>List of AdmissionApplicationSupportingItemTypes <see cref="Dtos.AdmissionApplicationSupportingItemTypes"/> objects representing matching admissionApplicationSupportingItemTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/admission-application-supporting-item-types", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAdmissionApplicationSupportingItemTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes>>> GetAdmissionApplicationSupportingItemTypesAsync()
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
                var items = await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesAsync(bypassCache);

                AddEthosContextProperties(await _admissionApplicationSupportingItemTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _admissionApplicationSupportingItemTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              items.Select(a => a.Id).ToList()));

                return Ok(items);
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
        /// Read (GET) a admissionApplicationSupportingItemTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItemTypes</param>
        /// <returns>A admissionApplicationSupportingItemTypes object <see cref="Dtos.AdmissionApplicationSupportingItemTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/admission-application-supporting-item-types/{guid}", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplicationSupportingItemTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplicationSupportingItemTypes>> GetAdmissionApplicationSupportingItemTypesByGuidAsync(string guid)
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
                var admissionApplication = await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesByGuidAsync(guid);

                if (admissionApplication != null)
                {

                    AddEthosContextProperties(await _admissionApplicationSupportingItemTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _admissionApplicationSupportingItemTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { admissionApplication.Id }));
                }

                return admissionApplication;
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
        /// Create (POST) a new admissionApplicationSupportingItemTypes
        /// </summary>
        /// <param name="admissionApplicationSupportingItemTypes">DTO of the new admissionApplicationSupportingItemTypes</param>
        /// <returns>A admissionApplicationSupportingItemTypes object <see cref="Dtos.AdmissionApplicationSupportingItemTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/admission-application-supporting-item-types", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionApplicationSupportingItemTypesV9")]
        public async Task<ActionResult<Dtos.AdmissionApplicationSupportingItemTypes>> PostAdmissionApplicationSupportingItemTypesAsync([FromBody] Dtos.AdmissionApplicationSupportingItemTypes admissionApplicationSupportingItemTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionApplicationSupportingItemTypes
        /// </summary>
        /// <param name="guid">GUID of the admissionApplicationSupportingItemTypes to update</param>
        /// <param name="admissionApplicationSupportingItemTypes">DTO of the updated admissionApplicationSupportingItemTypes</param>
        /// <returns>A admissionApplicationSupportingItemTypes object <see cref="Dtos.AdmissionApplicationSupportingItemTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/admission-application-supporting-item-types/{guid}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionApplicationSupportingItemTypesV9")]
        public async Task<ActionResult<Dtos.AdmissionApplicationSupportingItemTypes>> PutAdmissionApplicationSupportingItemTypesAsync([FromRoute] string guid, [FromBody] Dtos.AdmissionApplicationSupportingItemTypes admissionApplicationSupportingItemTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionApplicationSupportingItemTypes
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItemTypes</param>
        [HttpDelete]
        [Route("/admission-application-supporting-item-types/{guid}", Name = "DefaultDeleteAdmissionApplicationSupportingItemTypes")]
        public async Task<IActionResult> DeleteAdmissionApplicationSupportingItemTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

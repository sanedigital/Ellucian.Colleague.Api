// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using System.Linq;
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
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionApplicationWithdrawalReasons
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationWithdrawalReasonsController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationWithdrawalReasonsService _admissionApplicationWithdrawalReasonsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationWithdrawalReasonsController class.
        /// </summary>
        /// <param name="admissionApplicationWithdrawalReasonsService">Service of type <see cref="IAdmissionApplicationWithdrawalReasonsService">IAdmissionApplicationWithdrawalReasonsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdmissionApplicationWithdrawalReasonsController(IAdmissionApplicationWithdrawalReasonsService admissionApplicationWithdrawalReasonsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _admissionApplicationWithdrawalReasonsService = admissionApplicationWithdrawalReasonsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all admissionApplicationWithdrawalReasons
        /// </summary>
        /// <returns>List of AdmissionApplicationWithdrawalReasons <see cref="Dtos.AdmissionApplicationWithdrawalReasons"/> objects representing matching admissionApplicationWithdrawalReasons</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/admission-application-withdrawal-reasons", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAdmissionApplicationWithdrawalReasons", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationWithdrawalReasons>>> GetAdmissionApplicationWithdrawalReasonsAsync()
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
                var reasons = await _admissionApplicationWithdrawalReasonsService.GetAdmissionApplicationWithdrawalReasonsAsync(bypassCache);
                AddEthosContextProperties(await _admissionApplicationWithdrawalReasonsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _admissionApplicationWithdrawalReasonsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          reasons.Select(dd => dd.Id).ToList()));
                return Ok(reasons);
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
        /// Read (GET) a admissionApplicationWithdrawalReasons using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationWithdrawalReasons</param>
        /// <returns>A admissionApplicationWithdrawalReasons object <see cref="Dtos.AdmissionApplicationWithdrawalReasons"/> in EEDM format</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/admission-application-withdrawal-reasons/{guid}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplicationWithdrawalReasonsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplicationWithdrawalReasons>> GetAdmissionApplicationWithdrawalReasonsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _admissionApplicationWithdrawalReasonsService.GetAdmissionApplicationWithdrawalReasonsByGuidAsync(guid);
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
        /// Create (POST) a new admissionApplicationWithdrawalReasons
        /// </summary>
        /// <param name="admissionApplicationWithdrawalReasons">DTO of the new admissionApplicationWithdrawalReasons</param>
        /// <returns>A admissionApplicationWithdrawalReasons object <see cref="Dtos.AdmissionApplicationWithdrawalReasons"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/admission-application-withdrawal-reasons", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionApplicationWithdrawalReasonsV6")]
        public async Task<ActionResult<Dtos.AdmissionApplicationWithdrawalReasons>> PostAdmissionApplicationWithdrawalReasonsAsync([FromBody] Dtos.AdmissionApplicationWithdrawalReasons admissionApplicationWithdrawalReasons)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionApplicationWithdrawalReasons
        /// </summary>
        /// <param name="guid">GUID of the admissionApplicationWithdrawalReasons to update</param>
        /// <param name="admissionApplicationWithdrawalReasons">DTO of the updated admissionApplicationWithdrawalReasons</param>
        /// <returns>A admissionApplicationWithdrawalReasons object <see cref="Dtos.AdmissionApplicationWithdrawalReasons"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/admission-application-withdrawal-reasons/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionApplicationWithdrawalReasonsV6")]
        public async Task<ActionResult<Dtos.AdmissionApplicationWithdrawalReasons>> PutAdmissionApplicationWithdrawalReasonsAsync([FromRoute] string guid, [FromBody] Dtos.AdmissionApplicationWithdrawalReasons admissionApplicationWithdrawalReasons)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionApplicationWithdrawalReasons
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationWithdrawalReasons</param>
        [HttpDelete]
        [Route("/admission-application-withdrawal-reasons/{guid}", Name = "DefaultDeleteAdmissionApplicationWithdrawalReasons")]
        public async Task<IActionResult> DeleteAdmissionApplicationWithdrawalReasonsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

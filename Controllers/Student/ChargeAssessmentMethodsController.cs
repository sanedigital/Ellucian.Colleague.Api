// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to ChargeAssessmentMethods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ChargeAssessmentMethodsController : BaseCompressedApiController
    {
        private readonly IChargeAssessmentMethodsService _chargeAssessmentMethodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ChargeAssessmentMethodsController class.
        /// </summary>
        /// <param name="chargeAssessmentMethodsService">Service of type <see cref="IChargeAssessmentMethodsService">IChargeAssessmentMethodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ChargeAssessmentMethodsController(IChargeAssessmentMethodsService chargeAssessmentMethodsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _chargeAssessmentMethodsService = chargeAssessmentMethodsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all chargeAssessmentMethods
        /// </summary>
        /// <returns>List of ChargeAssessmentMethods <see cref="Dtos.ChargeAssessmentMethods"/> objects representing matching chargeAssessmentMethods</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/charge-assessment-methods", "13", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetChargeAssessmentMethods", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ChargeAssessmentMethods>>> GetChargeAssessmentMethodsAsync()
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
                AddDataPrivacyContextProperty((await _chargeAssessmentMethodsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return Ok(await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsAsync(bypassCache));
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
        /// Read (GET) a chargeAssessmentMethods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired chargeAssessmentMethods</param>
        /// <returns>A chargeAssessmentMethods object <see cref="Dtos.ChargeAssessmentMethods"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/charge-assessment-methods/{guid}", "13", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetChargeAssessmentMethodsByGuid")]
        public async Task<ActionResult<Dtos.ChargeAssessmentMethods>> GetChargeAssessmentMethodsByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _chargeAssessmentMethodsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsByGuidAsync(guid);
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
        /// Create (POST) a new chargeAssessmentMethods
        /// </summary>
        /// <param name="chargeAssessmentMethods">DTO of the new chargeAssessmentMethods</param>
        /// <returns>A chargeAssessmentMethods object <see cref="Dtos.ChargeAssessmentMethods"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/charge-assessment-methods", "13", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostChargeAssessmentMethodsV13")]
        public async Task<ActionResult<Dtos.ChargeAssessmentMethods>> PostChargeAssessmentMethodsAsync([FromBody] Dtos.ChargeAssessmentMethods chargeAssessmentMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing chargeAssessmentMethods
        /// </summary>
        /// <param name="guid">GUID of the chargeAssessmentMethods to update</param>
        /// <param name="chargeAssessmentMethods">DTO of the updated chargeAssessmentMethods</param>
        /// <returns>A chargeAssessmentMethods object <see cref="Dtos.ChargeAssessmentMethods"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/charge-assessment-methods/{guid}", "13", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutChargeAssessmentMethodsV13")]
        public async Task<ActionResult<Dtos.ChargeAssessmentMethods>> PutChargeAssessmentMethodsAsync([FromRoute] string guid, [FromBody] Dtos.ChargeAssessmentMethods chargeAssessmentMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a chargeAssessmentMethods
        /// </summary>
        /// <param name="guid">GUID to desired chargeAssessmentMethods</param>
        [HttpDelete]
        [Route("/charge-assessment-methods/{guid}", Name = "DefaultDeleteChargeAssessmentMethods", Order = -10)]
        public async Task<IActionResult> DeleteChargeAssessmentMethodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

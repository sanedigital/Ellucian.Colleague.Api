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
    /// Provides access to BillingOverrideReasons
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class BillingOverrideReasonsController : BaseCompressedApiController
    {
        private readonly IBillingOverrideReasonsService _billingOverrideReasonsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BillingOverrideReasonsController class.
        /// </summary>
        /// <param name="billingOverrideReasonsService">Service of type <see cref="IBillingOverrideReasonsService">IBillingOverrideReasonsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BillingOverrideReasonsController(IBillingOverrideReasonsService billingOverrideReasonsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _billingOverrideReasonsService = billingOverrideReasonsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all billingOverrideReasons
        /// </summary>
        /// <returns>List of BillingOverrideReasons <see cref="Dtos.BillingOverrideReasons"/> objects representing matching billingOverrideReasons</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/billing-override-reasons", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetBillingOverrideReasons", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.BillingOverrideReasons>>> GetBillingOverrideReasonsAsync()
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
                var billingOverrideReasons =  await _billingOverrideReasonsService.GetBillingOverrideReasonsAsync(bypassCache);

                AddEthosContextProperties(
                    await _billingOverrideReasonsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _billingOverrideReasonsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        billingOverrideReasons.Select(i => i.Id).ToList()));

                return Ok(billingOverrideReasons);
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
        /// Read (GET) a billingOverrideReasons using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired billingOverrideReasons</param>
        /// <returns>A billingOverrideReasons object <see cref="Dtos.BillingOverrideReasons"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/billing-override-reasons/{guid}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetBillingOverrideReasonsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.BillingOverrideReasons>> GetBillingOverrideReasonsByGuidAsync(string guid)
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
                var billingOverRideReason = await _billingOverrideReasonsService.GetBillingOverrideReasonsByGuidAsync(guid);

                AddEthosContextProperties(
                    await _billingOverrideReasonsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _billingOverrideReasonsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return billingOverRideReason;
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
        /// Create (POST) a new billingOverrideReasons
        /// </summary>
        /// <param name="billingOverrideReasons">DTO of the new billingOverrideReasons</param>
        /// <returns>A billingOverrideReasons object <see cref="Dtos.BillingOverrideReasons"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/billing-override-reasons", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostBillingOverrideReasonsV8")]
        public async Task<ActionResult<Dtos.BillingOverrideReasons>> PostBillingOverrideReasonsAsync([FromBody] Dtos.BillingOverrideReasons billingOverrideReasons)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing billingOverrideReasons
        /// </summary>
        /// <param name="guid">GUID of the billingOverrideReasons to update</param>
        /// <param name="billingOverrideReasons">DTO of the updated billingOverrideReasons</param>
        /// <returns>A billingOverrideReasons object <see cref="Dtos.BillingOverrideReasons"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/billing-override-reasons/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutBillingOverrideReasonsV8")]
        public async Task<ActionResult<Dtos.BillingOverrideReasons>> PutBillingOverrideReasonsAsync([FromRoute] string guid, [FromBody] Dtos.BillingOverrideReasons billingOverrideReasons)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a billingOverrideReasons
        /// </summary>
        /// <param name="guid">GUID to desired billingOverrideReasons</param>
        [HttpDelete]
        [Route("/billing-override-reasons/{guid}", Name = "DefaultDeleteBillingOverrideReasons")]
        public async Task<IActionResult> DeleteBillingOverrideReasonsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

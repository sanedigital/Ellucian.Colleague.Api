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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;

using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PayScales
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayScalesController : BaseCompressedApiController
    {
        private readonly IPayScalesService _payScalesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PayScalesController class.
        /// </summary>
        /// <param name="payScalesService">Service of type <see cref="IPayScalesService">IPayScalesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PayScalesController(IPayScalesService payScalesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _payScalesService = payScalesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all payScales
        /// </summary>
        /// <returns>List of PayScales <see cref="Dtos.PayScales"/> objects representing matching payScales</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/pay-scales", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPayScales", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PayScales>>> GetPayScalesAsync()
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
                var payScalesEntities = await _payScalesService.GetPayScalesAsync(bypassCache);

                AddEthosContextProperties(
                    await _payScalesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _payScalesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        payScalesEntities.Select(i => i.Id).ToList()));

                return Ok(payScalesEntities);
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
        /// Read (GET) a payScales using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired payScales</param>
        /// <returns>A payScales object <see cref="Dtos.PayScales"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/pay-scales/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPayScalesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PayScales>> GetPayScalesByGuidAsync(string guid)
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
                     await _payScalesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _payScalesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         new List<string>() { guid }));

                return await _payScalesService.GetPayScalesByGuidAsync(guid);
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
        /// Create (POST) a new payScales
        /// </summary>
        /// <param name="payScales">DTO of the new payScales</param>
        /// <returns>A payScales object <see cref="Dtos.PayScales"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/pay-scales", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPayScalesV11")]
        public async Task<ActionResult<Dtos.PayScales>> PostPayScalesAsync([FromBody] Dtos.PayScales payScales)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing payScales
        /// </summary>
        /// <param name="guid">GUID of the payScales to update</param>
        /// <param name="payScales">DTO of the updated payScales</param>
        /// <returns>A payScales object <see cref="Dtos.PayScales"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/pay-scales/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPayScalesV11")]
        public async Task<ActionResult<Dtos.PayScales>> PutPayScalesAsync([FromRoute] string guid, [FromBody] Dtos.PayScales payScales)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a payScales
        /// </summary>
        /// <param name="guid">GUID to desired payScales</param>
        [HttpDelete]
        [Route("/pay-scales/{guid}", Name = "DefaultDeletePayScales")]
        public async Task<IActionResult> DeletePayScalesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

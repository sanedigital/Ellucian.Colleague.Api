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
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PayPeriods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayPeriodsController : BaseCompressedApiController
    {
        private readonly IPayPeriodsService _payPeriodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PayPeriodsController class.
        /// </summary>
        /// <param name="payPeriodsService">Service of type <see cref="IPayPeriodsService">IPayPeriodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PayPeriodsController(IPayPeriodsService payPeriodsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _payPeriodsService = payPeriodsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all payPeriods
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">API criteria info to filter on.</param>
        /// <returns>List of PayPeriods <see cref="Dtos.PayPeriods"/> objects representing matching payPeriods</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PayPeriods)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HttpGet]
        [HeaderVersionRoute("/pay-periods", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPayPeriods", IsEedmSupported = true)]
        public async Task<IActionResult> GetPayPeriodsAsync(Paging page, QueryStringFilter criteria)
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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                string payCycle = string.Empty, startOn = string.Empty, endOn = string.Empty;
                var rawFilterData = GetFilterObject<Dtos.PayPeriods>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.PayPeriods>>(new List<Dtos.PayPeriods>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                if (rawFilterData != null)
                {
                    payCycle = rawFilterData.PayCycle != null ? rawFilterData.PayCycle.Id : null;
                    startOn = rawFilterData.StartOn != null ? rawFilterData.StartOn.ToString() : string.Empty;
                    endOn = rawFilterData.EndOn != null ? rawFilterData.EndOn.ToString() : string.Empty;
                }
                
                var pageOfItems = await _payPeriodsService.GetPayPeriodsAsync(page.Offset, page.Limit, payCycle, startOn, endOn, bypassCache);

                AddEthosContextProperties(
                    await _payPeriodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _payPeriodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PayPeriods>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a payPeriods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired payPeriods</param>
        /// <returns>A payPeriods object <see cref="Dtos.PayPeriods"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/pay-periods/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPayPeriodsByGuid")]
        public async Task<ActionResult<Dtos.PayPeriods>> GetPayPeriodsByGuidAsync(string guid)
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
                    await _payPeriodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _payPeriodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _payPeriodsService.GetPayPeriodsByGuidAsync(guid);
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
        /// Create (POST) a new payPeriods
        /// </summary>
        /// <param name="payPeriods">DTO of the new payPeriods</param>
        /// <returns>A payPeriods object <see cref="Dtos.PayPeriods"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/pay-periods", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPayPeriodsV12")]
        public async Task<ActionResult<Dtos.PayPeriods>> PostPayPeriodsAsync([FromBody] Dtos.PayPeriods payPeriods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing payPeriods
        /// </summary>
        /// <param name="guid">GUID of the payPeriods to update</param>
        /// <param name="payPeriods">DTO of the updated payPeriods</param>
        /// <returns>A payPeriods object <see cref="Dtos.PayPeriods"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/pay-periods/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPayPeriodsV12")]
        public async Task<ActionResult<Dtos.PayPeriods>> PutPayPeriodsAsync([FromRoute] string guid, [FromBody] Dtos.PayPeriods payPeriods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a payPeriods
        /// </summary>
        /// <param name="guid">GUID to desired payPeriods</param>
        [HttpDelete]
        [Route("/pay-periods/{guid}", Name = "DefaultDeletePayPeriods")]
        public async Task<IActionResult> DeletePayPeriodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

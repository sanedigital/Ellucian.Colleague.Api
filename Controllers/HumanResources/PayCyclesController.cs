// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
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
using System.Web;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Expose Human Resources Pay Cycles data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayCyclesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IPayCycleService payCycleService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// PayCyclesController constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="payCycleService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PayCyclesController(ILogger logger, IAdapterRegistry adapterRegistry, IPayCycleService payCycleService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.adapterRegistry = adapterRegistry;
            this.payCycleService = payCycleService;
        }

        /// <summary>
        /// Gets all the pay cycles available for an institution.
        /// A pay cycle describes a date interval to which employee time worked is applied and processed
        /// Results can be limited by passing in a lookback date. Pay Periods with end dates before the specified lookback date will be omitted.
        /// </summary>
        /// <accessComments>Any authenticated user can get these resources</accessComments>
        /// <param name="lookbackDate">A optional date which is used to filter previous pay periods with end dates prior to this date.</param>
        /// <returns>A List of pay cycle dtos</returns>
        /// <note>PayCycle is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/pay-cycles", 1, false, Name = "GetPayCycles")]
        public async Task<ActionResult<IEnumerable<PayCycle>>> GetPayCyclesAsync(DateTime? lookbackDate = null)
        {
            try
            {
                return Ok(await payCycleService.GetPayCyclesAsync(lookbackDate));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Return all payCycles
        /// </summary>
        /// <returns>List of PayCycles <see cref="Dtos.PayCycles"/> objects representing matching payCycles</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/pay-cycles", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPayCycles", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PayCycles>>> GetPayCycles2Async()
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
                var items = await payCycleService.GetPayCyclesAsync(bypassCache);

                AddEthosContextProperties(
                    await payCycleService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await payCycleService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a payCycles using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired payCycles</param>
        /// <returns>A payCycles object <see cref="Dtos.PayCycles"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/pay-cycles/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPayCyclesByGuid")]
        public async Task<ActionResult<Dtos.PayCycles>> GetPayCyclesByGuidAsync(string guid)
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
                //AddDataPrivacyContextProperty((await payCycleService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                //return await payCycleService.GetPayCyclesByGuidAsync(guid);
                AddEthosContextProperties(
                    await payCycleService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await payCycleService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await payCycleService.GetPayCyclesByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new payCycles
        /// </summary>
        /// <param name="payCycles">DTO of the new payCycles</param>
        /// <returns>A payCycles object <see cref="Dtos.PayCycles"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/pay-cycles", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPayCyclesV12")]
        public async Task<ActionResult<Dtos.PayCycles>> PostPayCyclesAsync([FromBody] Dtos.PayCycles payCycles)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing payCycles
        /// </summary>
        /// <param name="guid">GUID of the payCycles to update</param>
        /// <param name="payCycles">DTO of the updated payCycles</param>
        /// <returns>A payCycles object <see cref="Dtos.PayCycles"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/pay-cycles/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPayCyclesV12")]
        public async Task<ActionResult<Dtos.PayCycles>> PutPayCyclesAsync([FromRoute] string guid, [FromBody] Dtos.PayCycles payCycles)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a payCycles
        /// </summary>
        /// <param name="guid">GUID to desired payCycles</param>
        [HttpDelete]
        [Route("/pay-cycles/{guid}", Name = "DefaultDeletePayCycles")]
        public async Task<IActionResult> DeletePayCyclesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

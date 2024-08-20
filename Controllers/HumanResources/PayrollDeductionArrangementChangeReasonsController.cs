// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes payroll deduction arrangement change reasons data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayrollDeductionArrangementChangeReasonsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPayrollDeductionArrangementChangeReasonsService _payrollDeductionArrangementChangeReasonsService;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="payrollDeductionArrangementChangeReasonsService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PayrollDeductionArrangementChangeReasonsController(ILogger logger, IPayrollDeductionArrangementChangeReasonsService payrollDeductionArrangementChangeReasonsService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this._payrollDeductionArrangementChangeReasonsService = payrollDeductionArrangementChangeReasonsService;
        }

        /// <summary>
        /// Returns all payroll deduction arrangement change reasons.
        /// </summary>
        /// <returns></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/payroll-deduction-arrangement-change-reasons", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmPayrollDeductionArrangementChangeReasons", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.PayrollDeductionArrangementChangeReason>>> GetAllPayrollDeductionArrangementChangeReasonsAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                var payrollDeductionArrangementChangeReasons = await _payrollDeductionArrangementChangeReasonsService.GetPayrollDeductionArrangementChangeReasonsAsync(bypassCache);

                if (payrollDeductionArrangementChangeReasons != null && payrollDeductionArrangementChangeReasons.Any())
                {
                    AddEthosContextProperties(await _payrollDeductionArrangementChangeReasonsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _payrollDeductionArrangementChangeReasonsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              payrollDeductionArrangementChangeReasons.Select(a => a.Id).ToList()));
                }
                return Ok(payrollDeductionArrangementChangeReasons);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting payroll deduction arrangement change reasons.");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns a payroll deduction arrangement change reason.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/payroll-deduction-arrangement-change-reasons/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPayrollDeductionArrangementChangeReasonById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PayrollDeductionArrangementChangeReason>> GetPayrollDeductionArrangementChangeReasonByIdAsync([FromRoute] string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _payrollDeductionArrangementChangeReasonsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _payrollDeductionArrangementChangeReasonsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _payrollDeductionArrangementChangeReasonsService.GetPayrollDeductionArrangementChangeReasonByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e, "No payroll deduction arrangement change reasons was found for guid " + id + ".");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting payroll deduction arrangement change reason.");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// PutPayrollDeductionArrangementChangeReasonAsync
        /// </summary>
        /// <param name="payrollDeductionArrangementChangeReason"></param>
        /// <returns></returns>
        [HttpPut]
        [HeaderVersionRoute("/payroll-deduction-arrangement-change-reasons/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmPayrollDeductionArrangementChangeReason")]
        public async Task<ActionResult<Dtos.PayrollDeductionArrangementChangeReason>> PutPayrollDeductionArrangementChangeReasonAsync([FromBody] Dtos.PayrollDeductionArrangementChangeReason payrollDeductionArrangementChangeReason)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// PostPayrollDeductionArrangementChangeReasonAsync
        /// </summary>
        /// <param name="payrollDeductionArrangementChangeReason"></param>
        /// <returns></returns>
        [HttpPost]
        [HeaderVersionRoute("/payroll-deduction-arrangement-change-reasons", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmPayrollDeductionArrangementChangeReason")]
        public async Task<ActionResult<Dtos.PayrollDeductionArrangementChangeReason>> PostPayrollDeductionArrangementChangeReasonAsync([FromBody] Dtos.PayrollDeductionArrangementChangeReason payrollDeductionArrangementChangeReason)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// DeletePayrollDeductionArrangementChangeReasonAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/payroll-deduction-arrangement-change-reasons/{id}", Name = "DeletePayrollDeductionArrangementChangeReason")]
        public async Task<IActionResult> DeletePayrollDeductionArrangementChangeReasonAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

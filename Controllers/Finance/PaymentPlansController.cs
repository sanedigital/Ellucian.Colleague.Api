// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Http.Controllers;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Filters;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get and update payment plan information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    [Metadata(ApiDescription = "Provides access to get and update payment plan information", ApiDomain = "Finance")]
    public class PaymentPlansController : BaseCompressedApiController
    {
        private readonly IPaymentPlanService _service;
        private readonly ILogger _logger;

        /// <summary>
        /// PaymentPlansController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IPaymentPlanService">IPaymentPlanService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PaymentPlansController(IPaymentPlanService service, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _service = service;
            this._logger = logger;
        }

        /// <summary>
        /// Gets all payment plan templates
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of PaymentPlanTemplate DTOs</returns>
        /// <note>PaymentPlanTemplate is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/payment-plans/templates", 1, true, Name = "GetPaymentPlanTemplates")]
        [HeaderVersionRoute("/payment-plans/templates", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetPaymentPlanTemplates", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<IEnumerable<PaymentPlanTemplate>> GetPaymentPlanTemplates()
        {
            try
            {
                return Ok(_service.GetPaymentPlanTemplates());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Get the specified payment plan
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="paymentPlanId">ID of the payment plan</param>
        /// <returns>A PaymentPlan DTO</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the payment plan ID is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to get the payment plan</exception>
        [HttpGet]
        [HeaderVersionRoute("/payment-plans/{paymentPlanId}", 1, true, Name = "GetPaymentPlan")]
        [HeaderVersionRoute("/payment-plans/{paymentPlanId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetPaymentPlan", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<PaymentPlan> GetPaymentPlan(string paymentPlanId)
        {
            if (string.IsNullOrEmpty(paymentPlanId))
            {
                string message = "Payment plan ID must be specified.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(_service.GetPaymentPlan(paymentPlanId));
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Gets the specified payment plan template
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <param name="templateId">ID of the payment plan template</param>
        /// <returns>A PaymentPlanTemplate DTO</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the payment plan is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create the payment plan</exception>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/payment-plans/templates/{templateId}", 1, true, Name = "GetPaymentPlanTemplate")]
        [HeaderVersionRoute("/payment-plans-templates/{templateId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetPaymentPlanTemplate", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<PaymentPlanTemplate> GetPaymentPlanTemplate(string templateId)
        {
            if (string.IsNullOrEmpty(templateId))
            {
                string message = "Payment plan template ID must be specified.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(_service.GetPaymentPlanTemplate(templateId));
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogError(knfe.ToString());
                return CreateNotFoundException("Payment Plan Template", templateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Post the approval of a payment plan's terms and conditions
        /// </summary>
        /// <accessComments>
        /// Users may change their own data only
        /// </accessComments>
        /// <param name="approval">The payment plan approval information</param>
        /// <returns>The updated <see cref="PaymentPlanApproval">Payment Plan approval</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the payment plan is not provided.</exception>
        [HttpPost]
        [HeaderVersionRoute("/payment-plans/accept-terms", 1, true, Name = "PostPaymentPlanTermsAcceptance")]
        public ActionResult<PaymentPlanApproval> PostAcceptTerms([FromBody]PaymentPlanTermsAcceptance approval)
        {
            try
            {
                return Ok(_service.ApprovePaymentPlanTerms(approval));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Get an approval for a payment plan
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="approvalId">Approval ID</param>
        /// <returns>A PaymentPlanApproval DTO</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the payment plan approval is not provided.</exception>
        [HttpGet]
        [HeaderVersionRoute("/payment-plans/approvals/{approvalId}", 1, true, Name = "GetPaymentPlanApproval")]
        [HeaderVersionRoute("/payment-plans-approvals/{approvalId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetPaymentPlanApproval", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<PaymentPlanApproval> GetPaymentPlanApproval(string approvalId)
        {
            if (string.IsNullOrEmpty(approvalId))
            {
                string message = "Payment plan approval ID must be specified.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(_service.GetPaymentPlanApproval(approvalId));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }


        /// <summary>
        /// Retrieves the down payment information for a payment control, payment plan, pay method and amount
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="planId">Payment plan ID</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="amount">Total payment amount</param>
        /// <param name="payControlId">Registration payment control ID</param>
        /// <returns>List of payments to be made</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "planId" })]
        [HttpGet]
        [HeaderVersionRoute("/payment-plans/{planId}/payment-summary", 1, true, Name = "GetPlanPaymentSummary")]
        [HeaderVersionRoute("/payment-plans/{planId}/payment-summary", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetPlanPaymentSummary", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<Payment> GetPlanPaymentSummary(string planId, string payMethod, decimal amount, string payControlId)
        {
            try
            {
                return Ok(_service.GetPlanPaymentSummary(planId, payMethod, amount, payControlId));
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return Forbid(peex.Message);
            }
        }

        /// <summary>
        /// Gets a proposed payment plan for a given person for a given term and receivable type with total charges
        /// no greater than the stated amount
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="personId">Proposed plan owner ID</param>
        /// <param name="termId">Billing term ID</param>
        /// <param name="receivableTypeCode">Receivable Type Code</param>
        /// <param name="planAmount">Maximum total payment plan charges</param>
        /// <returns>Proposed payment plan</returns>
        [HttpGet]
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "termId" })]
        [HeaderVersionRoute("/payment-plans/proposed-plan/{personId}", 1, true, Name = "GetProposedPaymentPlanAsync")]
        [HeaderVersionRoute("/payment-plans-proposed-plan/{personId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetProposedPaymentPlanAsync", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<PaymentPlan>> GetProposedPaymentPlanAsync([FromRoute] string personId, [FromQuery] string termId,
            [FromQuery]string receivableTypeCode, [FromQuery] decimal planAmount)
        {
            try
            {
                return await _service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }
    }
}

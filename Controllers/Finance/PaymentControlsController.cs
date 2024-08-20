// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get and update registration billing information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    [Metadata(ApiDescription = "Provides access to get and update registration billing information", ApiDomain = "Finance")]
    public class PaymentControlsController : BaseCompressedApiController
    {
        private readonly IRegistrationBillingService _service;
        private readonly ILogger _logger;

        /// <summary>
        /// RegistrationBillingController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IRegistrationBillingService">IRegistrationBillingService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PaymentControlsController(IRegistrationBillingService service, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _service = service;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves a specified registration payment control.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="id">Registration payment control ID</param>
        /// <returns>The <see cref="RegistrationPaymentControl">Registration Payment Control</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpGet]
        [HeaderVersionRoute("/payment-controls/{id}", 1, true, Name = "GetRegistrationPaymentControl")]
        [HeaderVersionRoute("/payment-controls/{id}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetRegistrationPaymentControl", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<RegistrationPaymentControl> Get(string id)
        {
            try
            {
                return Ok(_service.GetPaymentControl(id));
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
        }

        /// <summary>
        /// Retrieves the incomplete payment controls for a student.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Student ID</param>
        /// <returns>The list of <see cref="RegistrationPaymentControl">Registration Payment Control</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpGet]
        [HeaderVersionRoute("/payment-controls/student/{studentId}", 1, true, Name = "GetStudentPaymentControls")]
        [HeaderVersionRoute("/payment-controls-student/{studentId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetStudentPaymentControls", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<IEnumerable<RegistrationPaymentControl>> GetStudent(string studentId)
        {
            try
            {
                return Ok(_service.GetStudentPaymentControls(studentId));
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
        }

        /// <summary>
        /// Retrieves a registration payment control document.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="id">Registration payment control ID</param>
        /// <param name="documentId">Document ID</param>
        /// <returns>The <see cref="TextDocument">Text Document</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "documentId")]
        [HeaderVersionRoute("/payment-controls/{id}", 1, true, Name = "GetRegistrationPaymentControlDocument", Order = -20)]
        public ActionResult<TextDocument> GetDocument(string id, string documentId)
        {
            try
            {
                return Ok(_service.GetPaymentControlDocument(id, documentId));
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
        }

        /// <summary>
        /// Post the approval of a registration's terms and conditions
        /// </summary>
        /// <accessComments>
        /// Users may change their own data only
        /// </accessComments>
        /// <param name="approval">The registration approval information</param>
        /// <returns>The updated <see cref="RegistrationTermsApproval">registration approval</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete endpoint as of API 1.5: DTO has changed; use PostAcceptTerms2 instead.", false)]
        [HttpPost]
        [HeaderVersionRoute("/payment-controls/accept-terms", 1, false, Name = "PostRegistrationTermsAcceptance")]
        public ActionResult<RegistrationTermsApproval> PostAcceptTerms(PaymentTermsAcceptance approval)
        {
            try
            {
                return Ok(_service.ApproveRegistrationTerms(approval));
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
        }

        /// <summary>
        /// Post the approval of a registration's terms and conditions
        /// </summary>
        /// <accessComments>
        /// Users may change their own data only
        /// </accessComments>
        /// <param name="approval">The registration approval information</param>
        /// <returns>The updated <see cref="RegistrationTermsApproval">registration approval</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpPost]
        [HeaderVersionRoute("/payment-controls/accept-terms", 2, true, Name = "PostRegistrationTermsAcceptance2")]
        public ActionResult<RegistrationTermsApproval2> PostAcceptTerms2(PaymentTermsAcceptance2 approval)
        {
            try
            {
                return Ok(_service.ApproveRegistrationTerms2(approval));
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
        }

        /// <summary>
        /// Retrieves payment options for a student for a term.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="id">Registration payment control ID</param>
        /// <returns>The <see cref="ImmediatePaymentOptions">Immediate Payment Options</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpGet]
        [HeaderVersionRoute("/payment-controls/options/{id}", 1, true, Name = "GetPaymentControlOptions")]
        [HeaderVersionRoute("/payment-controls-options/{id}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetStudentPaymentControlsOptions", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<ImmediatePaymentOptions> GetOptions(string id)
        {
            try
            {
                return Ok(_service.GetPaymentOptions(id));
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
        }

        /// <summary>
        /// Updates a payment control
        /// </summary>
        /// <accessComments>
        /// Users may change their own data only
        /// </accessComments>
        /// <param name="rpcDto"><see cref="RegistrationPaymentControl">Registration Payment Control</see> DTO to update</param>
        /// <returns>The updated <see cref="RegistrationPaymentControl">Registration Payment Control</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpPut]
        [HeaderVersionRoute("/payment-controls", 1, true, Name = "PutRegistrationPaymentControl")]
        public ActionResult<RegistrationPaymentControl> Put(RegistrationPaymentControl rpcDto)
        {
            try
            {
                return Ok(_service.UpdatePaymentControl(rpcDto));
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
        }

        /// <summary>
        /// Retrieves the payment summary for a payment control, pay method, and payment amount.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="id">Registration payment control ID</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="amount">Total payment amount</param>
        /// <returns>The List of <see cref="Payment">payments</see> to be made</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpGet]
        [HeaderVersionRoute("/payment-controls/summary/{id}", 1, true, Name = "GetRegistrationPaymentSummary")]
        [HeaderVersionRoute("/payment-controls-summary/{id}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetRegistrationPaymentSummary", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<IEnumerable<Payment>> GetSummary(string id, string payMethod, decimal amount)
        {
            try
            {
                return Ok(_service.GetPaymentSummary(id, payMethod, amount));
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
        }

        /// <summary>
        /// Starts a registration payment
        /// </summary>
        /// <accessComments>
        /// Users may create their own data only
        /// </accessComments>
        /// <param name="payment">The registration payment</param>
        /// <returns>Payment provider information to start a payment</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpPost]
        [HeaderVersionRoute("/payment-controls/start-payment", 1, true, Name = "PostStartRegistrationPayment")]
        public ActionResult<PaymentProvider> PostStartPayment(Payment payment)
        {
            try
            {
                return Ok(_service.StartRegistrationPayment(payment));
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
        }

        /// <summary>
        /// Retrieves the registration terms approval.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="id">Terms approval ID</param>
        /// <returns>The <see cref="RegistrationTermsApproval">terms approval</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete as of API version 1.5, use GetRegistrationTermsApproval2 instead.")]
        [HttpGet]
        [HeaderVersionRoute("/payment-controls/terms-approval/{id}", 1, false, Name = "GetRegistrationTermsApproval")]
        public ActionResult<RegistrationTermsApproval> GetTermsApproval(string id)
        {
            try
            {
                return Ok(_service.GetTermsApproval(id));
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the registration terms approval.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="id">Terms approval ID</param>
        /// <returns>The <see cref="RegistrationTermsApproval2">terms approval</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpGet]
        [HeaderVersionRoute("/payment-controls/terms-approval/{id}", 2, true, Name = "GetRegistrationTermsApproval2")]
        [HeaderVersionRoute("/payment-controls-terms-approval/{id}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetRegistrationTermsApproval", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<RegistrationTermsApproval2> GetTermsApproval2(string id)
        {
            try
            {
                return Ok(_service.GetTermsApproval2(id));
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Get a proposed payment plan
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="payControlId">ID of a payment control record</param>
        /// <param name="receivableType">Receivable Type for proposed payment plan</param>
        /// <returns>The proposed<see cref="PaymentPlan">Payment Plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to get proposed payment plan</exception>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/payment-controls/proposed-plan/{payControlId}/{receivableType}", 1, true, Name = "GetPaymentControlProposedPaymentPlan")]
        public ActionResult<PaymentPlan> GetProposedPaymentPlan(string payControlId, string receivableType)
        {
            try
            {
                return Ok(_service.GetProposedPaymentPlan(payControlId, receivableType));
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
        }
    }
}

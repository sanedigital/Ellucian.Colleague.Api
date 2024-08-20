// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to process student payments.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    [Metadata(ApiDescription = "Provides access to process student payments.", ApiDomain = "Finance")]
    public class PaymentController : BaseCompressedApiController
    {
        private readonly IPaymentService _service;
        private readonly IAccountsReceivableService _arService;
        private readonly IFinanceConfigurationService _financeConfigService;
        private readonly ILogger _logger;

        /// <summary>
        /// PaymentController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IPaymentService">IPaymentService</see></param>
        /// <param name="arService">Service of type <see cref="IAccountsReceivableService">IAccountsReceivableService</see></param>
        /// <param name="financeConfigService"></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PaymentController(IPaymentService service, IAccountsReceivableService arService, IFinanceConfigurationService financeConfigService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _service = service;
            _arService = arService;
            _financeConfigService = financeConfigService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves information required to process a student payment.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <param name="distribution">Distribution ID</param>
        /// <param name="paymentMethod">Payment Method</param>
        /// <param name="amountToPay">Amount being paid</param>
        /// <returns>The <see cref="PaymentConfirmation">Payment Confirmation</see> information</returns>
        [HttpGet]
        [HeaderVersionRoute("/payment/confirm", 1, true, Name = "ConfirmStudentPayment")]
        [HeaderVersionRoute("/payment-confirm", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, IsEthosEnabled = true, Name = "EthosConfirmStudentPayment", Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<PaymentConfirmation> GetPaymentConfirmation(string distribution, string paymentMethod, string amountToPay)
        {
            try
            {
                return Ok(_service.GetPaymentConfirmation(distribution, paymentMethod, amountToPay));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Process a student payment using a credit card
        /// </summary>
        /// <accessComments>
        /// Users may change their own data. Additionally, users who have proxy permissions can
        /// change other users' data
        /// </accessComments>
        /// <param name="paymentDetails">The <see cref="Payment">Payment</see> information</param>
        /// <returns>The <see cref="PaymentProvider">Payment Provider</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to make this payment</exception>
        [HttpPost]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [HeaderVersionRoute("/payment/process", 1, true, Name = "PostProcessStudentPayment")]
        [HeaderVersionRoute("/payment-process", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosPostPaymentProvider", IsEthosEnabled = true, IsAdministrative = true, Order = -10000)]
        public ActionResult<PaymentProvider> PostPaymentProvider([ModelBinder(typeof(EthosEnabledBinder))] Payment paymentDetails)
        {
            try
            {
                return Ok(_service.PostPaymentProvider(paymentDetails));
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
        /// Retrieves the information needed to acknowledge a payment.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="transactionId">e-Commerce Transaction ID</param>
        /// <param name="cashReceiptId">Cash Receipt ID</param>
        /// <returns>The <see cref="PaymentReceipt">Payment Receipt</see> information</returns>
        [HttpGet]
        [HeaderVersionRoute("/payment/receipt", 1, true, Name = "GetCashReceipt")]
        [HeaderVersionRoute("/payment-receipt", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, IsEthosEnabled = true, Name = "EthosGetCashReceipt", Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<PaymentReceipt> GetPaymentReceipt(string transactionId, string cashReceiptId)
        {
            try
            {
                return Ok(_service.GetPaymentReceipt(transactionId, cashReceiptId));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, pex.Message);
                return CreateHttpResponseException("Permission denied to access receipt information. See log for details.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                if (ex.Message == "Your payment has been canceled.")
                {
                    return CreateHttpResponseException("Your payment has been canceled. Contact the system administrator if you are receiving this message in error.");
                }
                else
                {
                    return CreateHttpResponseException("Could not retrieve payment receipt with the specified information. See log for details.");
                }
            }
        }

        /// <summary>
        /// Process a student payment using an electronic check
        /// </summary>
        /// <accessComments>
        /// Users may change their own data. Additionally, users who have proxy permissions can
        /// change other users' data
        /// </accessComments>
        /// <param name="paymentDetails">The <see cref="Payment">Payment</see> information</param>
        /// <returns>The <see cref="ElectronicCheckProcessingResult">Electronic Check Processing Result</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to make this payment</exception>
        [HttpPost]
        [HeaderVersionRoute("/payment/echeck", 1, true, Name = "PostProcessElectronicCheck")]
        [HeaderVersionRoute("/payment-echeck", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosPostProcessElectronicCheck", IsEthosEnabled = true, IsAdministrative = true, Order = -10000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<ElectronicCheckProcessingResult> PostProcessElectronicCheck([ModelBinder(typeof(EthosEnabledBinder))] Payment paymentDetails)
        {
            try
            {
                return Ok(_service.PostProcessElectronicCheck(paymentDetails));
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
        /// Retrieves the payer information needed to process an e-check.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have proxy permissions can
        /// request other users' data
        /// </accessComments>
        /// <param name="personId">Payer ID</param>
        /// <returns>The <see cref="ElectronicCheckPayer">Electronic Check Payer</see> information</returns>
        [HttpGet]
        [HeaderVersionRoute("/payment/echeck/payer/{personId}", 1, true, Name = "GetCheckPayerInformation")]
        [HeaderVersionRoute("/payment-echeck-payer/{personId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, IsEthosEnabled = true, Name = "EthosGetCheckPayerInformation", Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<ElectronicCheckPayer> GetCheckPayerInformation(string personId)
        {
            try
            {
                return Ok(_service.GetCheckPayerInformation(personId));
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
        /// Retrieves the payment distributions for a student, account types, and payment process.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY 
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Student ID</param>
        /// <param name="accountTypes">Comma-delimited list of account type codes</param>
        /// <param name="paymentProcess">Code of payment process</param>
        /// <returns>List of payment distributions</returns>
        [HttpGet]
        [HeaderVersionRoute("/payment/distributions/{studentId}", 1, true, Name = "GetPaymentDistributions")]
        [HeaderVersionRoute("/payment-distributions/{studentId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, IsEthosEnabled = true, Name = "EthosGetPaymentDistributions", Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<string>> GetPaymentDistributions(string studentId, string accountTypes, string paymentProcess)
        {
            var types = (string.IsNullOrEmpty(accountTypes)) ? new List<string>() : new List<string>(accountTypes.Split(','));
            try
            {
                return Ok(_arService.GetDistributions(studentId, types, paymentProcess));
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
        /// Retrieves a list of restricted payment methods
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>List of Available Payment Methods</returns>
        [HttpGet]
        [HeaderVersionRoute("/restricted-payments/{studentId}", 1, true, Name = "GetRestrictedPaymentMethodsAsync")]
        [HeaderVersionRoute("/restricted-payments/{studentId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, IsEthosEnabled = true, Name = "EthosGetRestrictedPaymentMethodsAsync", Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Finance.Configuration.AvailablePaymentMethod>>> GetRestrictedPaymentMethodsAsync(string studentId)
        {
            try
            {
                var config = _financeConfigService.GetFinanceConfiguration();
                var allPaymentMethods = new List<AvailablePaymentMethod>();
                foreach (var paymentMethod in config.PaymentMethods)
                {
                    var singlePayMethod = new AvailablePaymentMethod();
                    singlePayMethod.Description = paymentMethod.Description;
                    singlePayMethod.InternalCode = paymentMethod.InternalCode;
                    singlePayMethod.Type = paymentMethod.Type;
                    allPaymentMethods.Add(singlePayMethod);
                }
                var restrictedPaymentMethods = await _service.GetRestrictedPaymentMethodsAsync(studentId, allPaymentMethods);
                return Ok(restrictedPaymentMethods);
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

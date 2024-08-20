// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get and update Accounts Receivable information.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    [Metadata(ApiDescription = "Provides access to get and update Accounts Receivable information.", ApiDomain = "Finance")]
    public class ReceivablesController : BaseCompressedApiController
    {
        private readonly IAccountsReceivableService _service;
        private readonly IPaymentPlanService _payPlanService;
        private readonly ILogger _logger;
        private const string _restrictedHeaderName = "X-Content-Restricted";
        private const string _restrictedHeaderValue = "partial";

        /// <summary>
        /// AccountsReceivableController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IAccountsReceivableService">IAccountsReceivableService</see></param>
        /// <param name="payPlanService">Service of type <see cref="IPaymentPlanService">IPaymentPlanService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ReceivablesController(IAccountsReceivableService service, IPaymentPlanService payPlanService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _service = service;
            _payPlanService = payPlanService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves the account holder information for a specified person.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="personId">Person ID</param>
        /// <returns>The <see cref="AccountHolder">AccountHolder</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        /// <note>AccountHolder is cached for 1 minute.</note>
        [Obsolete("Obsolete as of API version 1.3, use GetAccountHolder2 instead.")]
        [HttpGet]
        [HeaderVersionRoute("/account-holders/{personId}", 1, true, Name = "GetAccountHolderObs")]
        public ActionResult<AccountHolder> GetAccountHolder(string personId)
        {
            try
            {
                return Ok(_service.GetAccountHolder(personId));
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the privacy-restricted account holder information for a specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="bypassCache">bypassCache</param>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <returns>The <see cref="AccountHolder">AccountHolder</see> information. Account Holder privacy is enforced by this 
        /// response. If any Account Holder has an assigned privacy code that the user is not authorized to access, the AccountHolder response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of account holders. In this situation, 
        /// all details except the advisee name are cleared from the specific AccountHolder object.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        /// <note>AccountHolder is cached for 1 minute.</note>
        [HttpGet]
        [HeaderVersionRoute("/receivables/account-holder/{personId}", 2, true, Name = "GetAccountHolder2")]
        [HeaderVersionRoute("/receivables-account-holder/{personId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, IsEthosEnabled = true, Name = "EthosGetAccountHolder2", IsAdministrative = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<AccountHolder>> GetAccountHolder2Async(string personId, bool bypassCache)
        {
            try
            {
                var privacyWrapper = await _service.GetAccountHolder2Async(personId, bypassCache);
                var accountHolder = privacyWrapper.Dto as AccountHolder;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append(_restrictedHeaderName, _restrictedHeaderValue);
                }
                return accountHolder;
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving account holder.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the information for a single accountholder if an id is provided,
        /// or the matching accountholders if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpPost]
        public async Task<ActionResult<IEnumerable<AccountHolder>>> QueryAccountHoldersByPostAsync([FromBody] string criteria)
        {
            try
            {
                return Ok(await _service.SearchAccountHoldersAsync(criteria));
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the privacy-restricted information for a single accountholder if an id is provided,
        /// or the matching accountholders if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <accessComments>
        /// Users who have VIEW.STUDENT.ACCOUNT.ACTIVITY permission may request this data
        /// </accessComments>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information. Account Holder privacy is enforced by this 
        /// response. If any Account Holder has an assigned privacy code that the user is not authorized to access, the AccountHolder response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of account holders. In this situation, 
        /// all details except the advisee name are cleared from the specific AccountHolder object.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpPost]
        [Obsolete("Obsolete as of API version 1.16, use QueryAccountHoldersByPost3Async instead.")]
        [HeaderVersionRoute("/qapi/receivables/account-holder", 2, false, Name = "QueryAccountHoldersByPost2")]
        public async Task<ActionResult<IEnumerable<AccountHolder>>> QueryAccountHoldersByPostAsync2([FromBody] string criteria)
        {
            if (string.IsNullOrEmpty(criteria))
            {
                return CreateHttpResponseException("criteria cannot be null");
            }
            try
            {
                var privacyWrapper = await _service.SearchAccountHoldersAsync2(criteria);
                var accountHolders = privacyWrapper.Dto as List<AccountHolder>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append(_restrictedHeaderName, _restrictedHeaderValue);
                }
                return Ok((IEnumerable<AccountHolder>)accountHolders);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the privacy-restricted information for a single accountholder if an id is provided,
        /// or the matching accountholders if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <accessComments>
        /// Users who have VIEW.STUDENT.ACCOUNT.ACTIVITY permission may request this data
        /// </accessComments>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">either a Person ID or a first and last name, or a list of Person Ids.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information. Account Holder privacy is enforced by this 
        /// response. If any Account Holder has an assigned privacy code that the user is not authorized to access, the AccountHolder response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of account holders. In this situation, 
        /// all details except the advisee name are cleared from the specific AccountHolder object.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpPost]
        [HeaderVersionRoute("/qapi/receivables/account-holder", 3, true, Name = "QueryAccountHoldersByPost3")]
        [HeaderVersionRoute("/qapi/receivables-account-holder", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosQueryAccountHoldersByPost", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]

        public async Task<ActionResult<IEnumerable<AccountHolder>>> QueryAccountHoldersByPost3Async([FromBody] AccountHolderQueryCriteria criteria)
        {
            if (criteria == null)
            {
                return CreateHttpResponseException("criteria cannot be null");
            }
            if ((criteria.Ids == null || !criteria.Ids.Any())
                && string.IsNullOrEmpty(criteria.QueryKeyword))
            {
                return CreateHttpResponseException("criteria must contain either a list of account holder ids or a query keyword");
            }
            try
            {
                var privacyWrapper = await _service.SearchAccountHolders3Async(criteria);
                var accountHolders = privacyWrapper.Dto as List<AccountHolder>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append(_restrictedHeaderName, _restrictedHeaderValue);
                }
                return Ok((IEnumerable<AccountHolder>)accountHolders);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a set of specified invoices.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="invoiceIds">Comma-delimited list of invoice IDs</param>
        /// <returns>The collection of <see cref="Invoice">Invoice</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete as of API version 1.12, use QueryInvoicesByPostAsync instead.")]
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "invoiceIds")]
        [HeaderVersionRoute("/receivables/invoices", 1, true, Name = "GetInvoices")]
        public ActionResult<IEnumerable<Invoice>> GetInvoices(string invoiceIds)
        {
            if (String.IsNullOrEmpty(invoiceIds))
            {
                return new List<Invoice>();
            }

            var ids = new List<string>(invoiceIds.Split(','));
            try
            {
                return Ok(_service.GetInvoices(ids));
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return Forbid(peex.Message);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while requesting invoices.";
                _logger.LogError(tex, message);
                return Unauthorized(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Cannot retrieve invoices with the specified information. See log for details.");
            }
        }

        /// <summary>
        /// Accepts a list invoice Ids and will post a query against invoices.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        ///</accessComments>
        /// <param name="criteria"><see cref="InvoiceQueryCriteria">Query Criteria</see> including the list of Invoice Ids to use to retrieve invoices.</param>
        /// <returns>List of <see cref="Invoice">Invoices</see> objects. </returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/receivable-invoices", 1, true, Name = "QueryReceivableInvoices")]
        public async Task<ActionResult<IEnumerable<Invoice>>> QueryInvoicesByPostAsync([FromBody] InvoiceQueryCriteria criteria)
        {
            try
            {
                return Ok(await _service.QueryInvoicesAsync(criteria.InvoiceIds));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException aex)
            {
                _logger.LogError(aex.Message);
                return CreateHttpResponseException(aex.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Accepts a list invoice Ids and will post a query against invoice payments.
        /// Retrieves a set of specified invoice payment items which inherits from invoice but has the addition of an amount paid on the invoice
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="criteria"><see cref="InvoiceQueryCriteria">Query Criteria</see> including the list of Invoice Ids. At least 1 invoice Id must be specified.</param>
        /// <returns>List of <see cref="InvoicePayment">InvoicePayments</see> objects. </returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/receivable-invoices", 1, false, "application/vnd.ellucian-invoice-payment.v{0}+json", Name = "QueryReceivableInvoicesPayments")]
        public async Task<ActionResult<IEnumerable<InvoicePayment>>> QueryInvoicePaymentsByPostAsync([FromBody] InvoiceQueryCriteria criteria)
        {
            try
            {
                return Ok(await _service.QueryInvoicePaymentsAsync(criteria.InvoiceIds));
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException aex)
            {
                _logger.LogError(aex.Message);
                return CreateHttpResponseException(aex.Message, HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Timeout exception has occurred while requesting InvoicePayment objects.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get a set of specified payments
        /// </summary>
        /// <param name="paymentIds">Comma-delimited list of payment IDs</param>
        /// <returns>The collection of <see cref="ReceivablePayment">ReceivablePayment</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public ActionResult<IEnumerable<ReceivablePayment>> GetPayments(string paymentIds)
        {
            if (String.IsNullOrEmpty(paymentIds))
            {
                return new List<ReceivablePayment>();
            }

            var ids = new List<string>(paymentIds.Split(','));
            try
            {
                return Ok(_service.GetPayments(ids));
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
        /// OBSOLETE - Use GetDepositsDue() in DepositsController to get deposits due for a student.
        /// Get the deposits due for a specified student
        /// </summary>
        /// <param name="id">Student ID</param>
        /// <returns>A list of <see cref="DepositDue">deposits due</see> for the student</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete as of API version 1.3")]
        public ActionResult<IEnumerable<DepositDue>> GetDepositsDue(string id)
        {
            try
            {
                return Ok(_service.GetDepositsDue(id));
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// OBSOLETE - Use GetDepositTypes() in DepositsController to get all deposit types.
        /// Get all deposit types
        /// </summary>
        /// <returns>A list of all <see cref="DepositType">deposit types</see></returns>
        [Obsolete("Obsolete as of API version 1.3")]
        public IEnumerable<DepositType> GetDepositTypes()
        {
            return _service.GetDepositTypes();
        }

        /// <summary>
        /// Retrieves all receivable types.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of all <see cref="ReceivableType">receivable types</see></returns>
        /// <note>ReceivableType is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/receivables/receivable-types", 1, true, Name = "GetReceivableTypes")]
        [HeaderVersionRoute("/receivables-receivable-types", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetReceivableTypes", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public IEnumerable<ReceivableType> GetReceivableTypes()
        {
            return _service.GetReceivableTypes();
        }

        /// <summary>
        /// Gets valid billing term payment plan information from a proposed billing term payment plan information collection
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="billingTerms">List of payment items</param>
        /// <returns>Valid billing term payment plan information from a proposed billing term payment plan information collection</returns>
        [HttpPost]
        [Obsolete("Obsolete as of API version 1.16, use QueryAccountHolderPaymentPlanOptions2Async instead.")]
        [HeaderVersionRoute("/qapi/receivables/account-holder/payment-plan-options", 1, false, Name = "QueryAccountHolderPaymentPlanOptionsAsync")]
        public async Task<ActionResult<PaymentPlanEligibility>> QueryAccountHolderPaymentPlanOptionsAsync([FromBody] IEnumerable<BillingTermPaymentPlanInformation> billingTerms)
        {
            if (billingTerms == null || !billingTerms.Any())
            {
                return CreateHttpResponseException("billingTerms cannot be null or empty");
            }
            try
            {
                return await _payPlanService.GetBillingTermPaymentPlanInformationAsync(billingTerms);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Gets valid billing term payment plan information from a proposed billing term payment plan information collection
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="criteria">payment plan query criteria</param>
        /// <returns>Valid billing term payment plan information from a proposed billing term payment plan information collection</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/receivables/account-holder/payment-plan-options", 2, true, Name = "QueryAccountHolderPaymentPlanOptions2Async")]
        public async Task<ActionResult<PaymentPlanEligibility>> QueryAccountHolderPaymentPlanOptions2Async([FromBody] PaymentPlanQueryCriteria criteria)
        {
            if (criteria == null)
            {
                return CreateHttpResponseException("criteria cannot be null");
            }
            try
            {
                return await _payPlanService.GetBillingTermPaymentPlanInformation2Async(criteria);
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
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves all charge codes.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of all <see cref="ChargeCode">receivable types</see></returns>
        /// <note>Charge codes are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/receivables/charge-codes", 1, true, Name = "GetChargeCodes")]
        [HeaderVersionRoute("/receivables-charge-codes", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetChargeCodes", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<IEnumerable<ChargeCode>>> GetChargeCodesAsync()
        {
            try
            {
                return Ok(await _service.GetChargeCodesAsync());
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

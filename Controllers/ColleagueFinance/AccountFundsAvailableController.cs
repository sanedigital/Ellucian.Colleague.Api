// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to Accounting Strings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class AccountFundsAvailableController : BaseCompressedApiController
    {
        private readonly IAccountFundsAvailableService _accountingFundsAvailableService;
        private readonly ILogger _logger;

        /// <summary>
        /// The primary use of the Account Funds Available entity is to validate the status of funds (available, not available, and not available, but may be overridden) 
        /// for an accounting string as of a given date. Initializes a new instance of the AccountFundsAvailableController class.
        /// </summary>
        /// <param name="accountingFundsAvailableService">Service of type <see cref="IAccountFundsAvailableService">IAccountFundsAvailableService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AccountFundsAvailableController(IAccountFundsAvailableService accountingFundsAvailableService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _accountingFundsAvailableService = accountingFundsAvailableService;
            this._logger = logger;
        }

        #region accounting funds available

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria">Filter Criteria</param>
        /// <param name="accountSpecification">Filter criteria for Named Query</param>
        /// <returns></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.AccountFundsAvailableFilter))]
        [QueryStringFilterFilter("accountSpecification", typeof(Dtos.Filters.AccountFundsAvailableFilter))]
        [HttpGet]
        [HeaderVersionRoute("/account-funds-available", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountFundsAvailableDefault", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AccountFundsAvailable>> GetAccountFundsAvailableAsync(QueryStringFilter criteria, QueryStringFilter accountSpecification)
        {
            if (criteria == null && accountSpecification == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null arguments",
                    IntegrationApiUtility.GetDefaultApiError("Both the accountingString and amount must be specified in the request URL when a GET operation is requested.")));
            }

            string accountingString = string.Empty, amount = string.Empty, balanceOn = string.Empty, submittedBy = string.Empty;

            var criteriaValues = GetFilterObject<Dtos.Filters.AccountFundsAvailableFilter>(_logger, "criteria");
            var accountSpecificationFilter = GetFilterObject<Dtos.Filters.AccountFundsAvailableFilter>(_logger, "accountSpecification");

            if (CheckForEmptyFilterParameters())
                return CreateHttpResponseException(new IntegrationApiException("Null arguments",
                    IntegrationApiUtility.GetDefaultApiError("Null or empty arguments are not allowed in the request URL when a GET operation is requested.")));

            // Filters or Named query can be used and they both have the same properties
            if (criteriaValues != null)
            {
                accountingString = !string.IsNullOrEmpty(criteriaValues.AccountingString) ? criteriaValues.AccountingString : accountingString;
                amount = criteriaValues.Amount != null ? criteriaValues.Amount.ToString() : amount;
                balanceOn = criteriaValues.BalanceOn != null ? criteriaValues.BalanceOn.ToString() : balanceOn;
                submittedBy = criteriaValues.SubmittedBy != null && !string.IsNullOrEmpty(criteriaValues.SubmittedBy.Id) ? criteriaValues.SubmittedBy.Id : submittedBy;
            }

            if (accountSpecificationFilter != null)
            {
                accountingString = !string.IsNullOrEmpty(accountSpecificationFilter.AccountingString) ? accountSpecificationFilter.AccountingString : accountingString;
                amount = accountSpecificationFilter.Amount != null ? accountSpecificationFilter.Amount.ToString() : amount;
                balanceOn = accountSpecificationFilter.BalanceOn != null ? accountSpecificationFilter.BalanceOn.ToString() : balanceOn;
                submittedBy = accountSpecificationFilter.SubmittedBy != null && !string.IsNullOrEmpty(accountSpecificationFilter.SubmittedBy.Id) ? accountSpecificationFilter.SubmittedBy.Id : submittedBy;
            }
            try
            {

                //values passed to the service
                decimal amountValue;
                DateTime? balanceOnDateValue;
                string submittedByValue = string.Empty;

                //validate before calling the service
                Validate(accountingString, amount, balanceOn, submittedBy, out balanceOnDateValue, out amountValue, out submittedByValue);

                return await _accountingFundsAvailableService.GetAccountFundsAvailableByFilterCriteriaAsync(accountingString, amountValue, balanceOnDateValue, submittedByValue);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }

            catch (JsonSerializationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException e)
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
        /// Parses and validates the input parameters
        /// </summary>
        /// <param name="accountingString"></param>
        /// <param name="amount"></param>
        /// <param name="balanceOn"></param>
        /// <param name="submittedBy"></param>
        /// <param name="balanceOnDateValue"></param>
        /// <param name="amountValue"></param>
        /// <param name="submittedByValue"></param>
        private void Validate(string accountingString, string amount, string balanceOn, string submittedBy, out DateTime? balanceOnDateValue, out decimal amountValue, out string submittedByValue)
        {
            if (string.IsNullOrEmpty(accountingString))
            {
                throw new ArgumentNullException("Null accountingString argument", "Accounting string is required when a GET operation is requested.");
            }
            if (string.IsNullOrEmpty(amount))
            {
                throw new ArgumentNullException("Null amount argument", "Amount is required when a GET operation is requested.");
            }

            balanceOnDateValue = null;
            submittedByValue = string.Empty;

            if (!string.IsNullOrEmpty(balanceOn))
            {
                DateTime outDate;
                if (DateTime.TryParse(balanceOn, out outDate))
                {
                    balanceOnDateValue = outDate;
                }
                else
                {
                    throw new ArgumentException(
                        "The value provided for balanceOn filter could not be converted into a date.");
                }
            }
            else
            {
                balanceOnDateValue = DateTime.Today.Date;
            }

            decimal outAmount = 0;
            if (decimal.TryParse(amount, out outAmount))
            {
                // A zero amount is a valid amount to send to the API
                //if (outAmount == 0)
                //{
                //    throw new ArgumentException("Amount is required when a GET operation is requested.");
                //}
                amountValue = outAmount;
            }
            else
            {
                throw new ArgumentException(
                    "The value provided for amount filter could not be converted into a number.");
            }

            Guid outGuid;
            if (!string.IsNullOrEmpty(submittedBy))
            {
                if (Guid.TryParse(submittedBy, out outGuid))
                {
                    if (outGuid.Equals(Guid.Empty))
                    {
                        throw new ArgumentException("The empty guid is not a valid search criteria parameter.", "submittedBy");
                    }
                    submittedByValue = outGuid.ToString();
                }
                else
                {
                    throw new ArgumentException(
                        "The value provided for submittedBy filter could not be converted into a guid.", "submittedBy");
                }
            }
        }

        /// <summary>
        /// AccountFundsAvailable Post
        /// </summary>
        /// <returns>MethodNotAllowed error</returns>
        [HttpPost]
        [HeaderVersionRoute("/account-funds-available", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountFundsAvailable")]
        public async Task<ActionResult<Dtos.AccountFundsAvailable>> PostAccountFundsAvailableAsync([FromBody] Dtos.AccountFundsAvailable accountingString)
        {
            //Post is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError), HttpStatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Update (PUT) AccountFundsAvailable
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="accountFundsAvailable"></param>
        /// <returns></returns>
        [HttpPut]
        [HeaderVersionRoute("/account-funds-available/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountFundsAvailable")]
        public async Task<ActionResult<Dtos.AccountFundsAvailable>> PutAccountFundsAvailableAsync([FromRoute] string guid, [FromBody] Dtos.AccountFundsAvailable accountFundsAvailable)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) AccountFundsAvailable
        /// </summary>
        /// <param name="guid">GUID to desired vendor</param>
        [HttpDelete]
        [Route("/account-funds-available/{guid}", Name = "DefaultDeleteAccountFundsAvailable", Order = -10)]
        public async Task<IActionResult> DeleteAccountFundsAvailableAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        #endregion accounting funds available

        #region Account funds available transactions

        /// <summary>
        /// Return all accountFundsAvailable_Transactions
        /// </summary>
        /// <returns>List of AccountFundsAvailable_Transactions <see cref="Dtos.AccountFundsAvailable_Transactions"/> objects representing matching accountFundsAvailable_Transactions</returns>
        [HttpGet]
        [HeaderVersionRoute("/account-funds-available-transactions", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountFundsAvailableTransactionsDefault")]
        [HeaderVersionRoute("/account-funds-available_transactions", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountFundsAvailableTransactionsV8")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AccountFundsAvailable_Transactions>>> GetAccountFundsAvailable_TransactionsAsync()
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Read (GET) a accountFundsAvailable_Transactions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountFundsAvailable_Transactions</param>
        /// <returns>A accountFundsAvailable_Transactions object <see cref="Dtos.AccountFundsAvailable_Transactions"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/account-funds-available-transactions/{guid}", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountFundsAvailableTransactionsbyIDDefault")]
        [HeaderVersionRoute("/account-funds-available_transactions/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountFundsAvailableTransactionsByGuidV8")]
        public async Task<ActionResult<Dtos.AccountFundsAvailable_Transactions>> GetAccountFundsAvailable_TransactionsByGuidAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create (POST) a new accountFundsAvailable_Transactions
        /// </summary>
        /// <param name="accountFundsAvailable_Transactions">DTO of the new accountFundsAvailable_Transactions</param>
        /// <returns>A accountFundsAvailable_Transactions object <see cref="Dtos.AccountFundsAvailable_Transactions"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/account-funds-available_transactions", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountFundsAvailableTransactionsV8")]
        public async Task<ActionResult<Dtos.AccountFundsAvailable_Transactions>> PostAccountFundsAvailable_TransactionsAsync([FromBody] Dtos.AccountFundsAvailable_Transactions accountFundsAvailable_Transactions)
        {

            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountFundsAvailable_Transactions
        /// </summary>
        /// <param name="guid">GUID of the accountFundsAvailable_Transactions to update</param>
        /// <param name="accountFundsAvailable_Transactions">DTO of the updated accountFundsAvailable_Transactions</param>
        /// <returns>A accountFundsAvailable_Transactions object <see cref="Dtos.AccountFundsAvailable_Transactions"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/account-funds-available-transactions/{guid}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountFundsAvailableTransactionsV11.1.0")]
        [HeaderVersionRoute("/account-funds-available_transactions/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountFundsAvailableTransactionsV8")]
        public async Task<ActionResult<Dtos.AccountFundsAvailable_Transactions>> PutAccountFundsAvailable_TransactionsAsync([FromRoute] string guid, [FromBody] Dtos.AccountFundsAvailable_Transactions accountFundsAvailable_Transactions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a accountFundsAvailable_Transactions
        /// </summary>
        /// <param name="guid">GUID to desired accountFundsAvailable_Transactions</param>
        [HttpDelete]
        [Route("/account-funds-available-transactions/{guid}", Name = "DefaultDeleteAccountFundsTransactionsAvailable", Order = -10)]
        public async Task<IActionResult> DeleteAccountFundsAvailable_TransactionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create (POST) a new accountFundsAvailable_Transactions
        /// </summary>
        /// <param name="accountFundsAvailable_Transactions">DTO of the new accountFundsAvailable_Transactions</param>
        /// <returns>A accountFundsAvailable_Transactions object <see cref="Dtos.AccountFundsAvailable_Transactions"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/account-funds-available_transactions", 8, false, RouteConstants.HedtechIntegrationAfaTransactionsQapiMediaTypeFormat, Name = "QueryAccountsFundsAvailableTransMinimumByPostV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AccountFundsAvailable_Transactions>> QueryAccountFundsAvailable_TransactionsAsync([FromBody] Dtos.AccountFundsAvailable_Transactions accountFundsAvailable_Transactions)
        {
            try
            {
                if ((accountFundsAvailable_Transactions == null) || accountFundsAvailable_Transactions.Transactions == null)
                    throw new ArgumentNullException("AccountsFundsAvailableTransactions",
                        "Must provide a request body.");
                return await _accountingFundsAvailableService.CheckAccountFundsAvailable_Transactions2Async(accountFundsAvailable_Transactions);
            }
          
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Ellucian.Colleague.Domain.Exceptions.RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }


        }


        /// <summary>
        /// QAPI accountFundsAvailable_Transactions
        /// </summary>
        /// <param name="accountFundsAvailable_Transactions">DTO of the new accountFundsAvailable_Transactions</param>
        /// <returns>A accountFundsAvailable_Transactions object <see cref="Dtos.AccountFundsAvailable_Transactions"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/account-funds-available-transactions", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "QueryAccountsFundsAvailableTransMinimumByPostV11.1.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AccountFundsAvailable_Transactions2>> QueryAccountFundsAvailable_Transactions2Async([FromBody] Dtos.AccountFundsAvailable_Transactions2 accountFundsAvailable_Transactions)
        {
            try
            {
                if ((accountFundsAvailable_Transactions == null) || accountFundsAvailable_Transactions.Transactions == null)
                    throw new ArgumentNullException("AccountsFundsAvailableTransactions",
                        "Must provide a request body.");
                return Ok(await _accountingFundsAvailableService.CheckAccountFundsAvailable_Transactions3Async(accountFundsAvailable_Transactions));
            }

            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Ellucian.Colleague.Domain.Exceptions.RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(e.Message);
            }


        }

        #endregion account funds available transactions
    }
}

// Copyright 2024 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller class for financial statements methods.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class FinancialStatementsController : BaseCompressedApiController
    {
        private readonly IFinancialStatementsService financialStatementsService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// This constructor initializes the GL Finance query service object.
        /// </summary>
        /// <param name="financialStatementsService">GL Finance query service object</param>
        /// <param name="_logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialStatementsController(IFinancialStatementsService financialStatementsService, ILogger _logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.financialStatementsService = financialStatementsService;
            this._logger = _logger;
        }

        /// <summary>
        /// Retrieves the filtered GL Accounts list
        /// </summary>
        /// <param name="reportConfig">Financial statement report configuration object.</param>
        /// <returns>GL accounts that match the filter criteria.</returns>
        /// <accessComments>
        /// Requires permission USE.FINANCIAL.STATEMENTS.
        /// </accessComments>
        [HttpPost]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "financial statements report query api.", HttpMethodDescription = "financial statements report query api.")]
        [HeaderVersionRoute("/qapi/financial-statements", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "QueryFinancialStatementsByPostAsyncV1.0.0", IsEthosEnabled = true)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<FinancialStatementsReportResponse>> QueryFinancialStatementsByPostAsync(FinancialStatementsReportConfiguration reportConfig)
        {
            try
            {
                if (reportConfig == null)
                {
                    throw new ArgumentNullException("reportConfig", "The report configuration must be specified.");
                }
                var response = await financialStatementsService.QueryFinancialStatementsByPostAsync(reportConfig);
                return Ok(response);
            }
            catch (ColleagueSessionExpiredException e)
            {
                _logger.LogError(e, "QueryFinancialStatementsByPostAsync ColleagueSessionExpiredException");
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "QueryFinancialStatementsByPostAsync ArgumentException");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, "QueryFinancialStatementsByPostAsync PermissionException");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "QueryFinancialStatementsByPostAsync Exception");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves All the Gl component details
        /// </summary>
        /// <returns>gl components with description.</returns>
        [HttpGet]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets all the gl components with description.", HttpMethodDescription = "Gets all the gl components with description.")]
        [HeaderVersionRoute("/glcomp-details", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllGlComponentDetailsAsyncV1.0.0", IsEthosEnabled = true)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<IEnumerable<GlComponentDetails>>> GetAllGlComponentDetailsAsync()
        {
            try
            {
                var response = await financialStatementsService.GetAllGlComponentDetailsAsync();
                return Ok(response);
            }
            catch (ColleagueSessionExpiredException e)
            {
                _logger.LogError(e, "GetAllGlComponentDetailsAsync ColleagueSessionExpiredException");
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "GetAllGlComponentDetailsAsync ArgumentException");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetAllGlComponentDetailsAsync Exception");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves the list of GL account DTOs.
        /// </summary>
        /// <returns>A collection of expense GL account DTOs.</returns>
        /// <accessComments>
        /// Requires permission USE.FINANCIAL.STATEMENTS.
        /// </accessComments>
        [HttpGet]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets all the gl numbers for financial statements usage.", HttpMethodDescription = "Gets all the gl numbers for financial statements usage.")]
        [HeaderVersionRoute("/financial-statements-accounts", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFinancialStatementsGeneralLedgerAccounts", IsEthosEnabled = true)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<IEnumerable<GlAccount>>> GetFinancialStatementsGeneralLedgerAccountsAsync()
        {
            try
            {
                Stopwatch watch = null;
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    watch = new Stopwatch();
                    watch.Start();
                }

                var financialStatementsAccounts = await financialStatementsService.GetFinancialStatementsGeneralLedgerAccountsAsync();

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    watch.Stop();
                    _logger.LogInformation("Financial statements GL account CONTROLLER timing: GetFinancialStatementsGeneralLedgerAccountsAsync completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
                }

                return Ok(financialStatementsAccounts);
            }
            catch (ColleagueSessionExpiredException e)
            {
                _logger.LogError(e, "GetFinancialStatementsGeneralLedgerAccountsAsync ColleagueSessionExpiredException");
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ConfigurationException cnex)
            {
                _logger.LogError(cnex, "GetFinancialStatementsGeneralLedgerAccounts ConfigurationException");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(cnex), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                _logger.LogError(anex, "GetFinancialStatementsGeneralLedgerAccounts ArgumentNullException");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(anex), HttpStatusCode.BadRequest);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, "GetFinancialStatementsGeneralLedgerAccounts PermissionException");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFinancialStatementsGeneralLedgerAccounts Exception");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all the budget, actuals and encumbrances activity detail for the GL account and the fiscal year.
        /// This API does not use GL account access security. It is permission based.
        /// </summary>
        /// <param name="criteria"><see cref="GlActivityDetailQueryCriteria">Query criteria</see>includes the GL account and the fiscal year for the query.</param>
        /// <returns>List of GL activity detail DTOs for the specified GL account for the specified fiscal year.</returns>
        /// <accessComments>
        /// Requires permission USE.FINANCIAL.STATEMENTS.
        /// </accessComments>
        [HttpPost]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Financial Statements GL activity detail query API.", HttpMethodDescription = "Financial Statements GL activity detail query API.")]
        [HeaderVersionRoute("/qapi/financial-statement-activities", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "QueryFinancialStatementsActivityByPostAsyncV1.0.0", IsEthosEnabled = true)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<GlAccountActivityDetail>> QueryFinancialStatementsActivityByPostAsync([FromBody] GlActivityDetailQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }

                if (string.IsNullOrEmpty(criteria.GlAccount))
                {
                    throw new ArgumentNullException("GlAccount", "A GL account must be specified.");
                }

                if (string.IsNullOrEmpty(criteria.FiscalYear))
                {
                    throw new ArgumentNullException("FiscalYear", "A fiscal year must be specified.");
                }


                return await financialStatementsService.QueryFinancialStatementsActivityByPostAsync(criteria.GlAccount, criteria.FiscalYear);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to access the GL account activity.", HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException cnex)
            {
                _logger.LogError(cnex, cnex.Message);
                return CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                _logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogDebug(csee, "Colleague session expired - unable to get activity for the GL account");
                return CreateHttpResponseException("Colleague session expired - unable to get activity for the GL account", HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get activity for the GL account and Fiscal Year provided.", HttpStatusCode.BadRequest);
            }
        }
    }
}


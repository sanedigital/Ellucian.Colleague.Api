// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to general ledger objects.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class GeneralLedgerAccountsController : BaseCompressedApiController
    {
        private readonly IGeneralLedgerAccountService generalLedgerAccountService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the GL account controller.
        /// </summary>
        /// <param name="generalLedgerAccountService">General ledger account service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GeneralLedgerAccountsController(IGeneralLedgerAccountService generalLedgerAccountService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.generalLedgerAccountService = generalLedgerAccountService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves the list of active expense GL account DTOs for which the user has access.
        /// </summary>
        /// <param name="glClass">Optional: null for all the user GL accounts, expense for only the expense type GL accounts.</param>
        /// <returns>A collection of expense GL account DTOs for the user.</returns>
        /// <accessComments>
        /// No permission is needed. The user can only access those GL accounts
        /// for which they have GL account security access granted.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/general-ledger-accounts", 1, true, Name = "GetUserGeneralLedgerAccounts")]
        public async Task<ActionResult<IEnumerable<GlAccount>>> GetUserGeneralLedgerAccountsAsync([FromQuery(Name = "glClass")] string glClass)
        {
            {
                try
                {
                    Stopwatch watch = null;
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        watch = new Stopwatch();
                        watch.Start();
                    }

                    var glUserAccounts = await generalLedgerAccountService.GetUserGeneralLedgerAccountsAsync(glClass);

                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        watch.Stop();
                        logger.LogInformation("GL account LookUp CONTROLLER timing: GetUserGeneralLedgerAccountsAsync completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
                    }

                    return Ok(glUserAccounts);
                }
                catch (ConfigurationException cnex)
                {
                    logger.LogError(cnex, cnex.Message);
                    return CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
                }
                catch (ArgumentNullException anex)
                {
                    logger.LogError(anex, anex.Message);
                    return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
                }
                // Application exceptions will be caught below.
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    return CreateHttpResponseException("Unable to get the GL accounts.", HttpStatusCode.BadRequest);
                }
            }
        }

        /// <summary>
        /// Retrieves a single general ledger object using the supplied GL account ID.
        /// </summary>
        /// <param name="generalLedgerAccountId">General ledger account ID.</param>
        /// <returns>General ledger account DTO.</returns>
        /// <accessComments>
        /// The user can only access those GL accounts for which they have
        /// GL account security access granted.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/general-ledger-accounts/{generalLedgerAccountId}", 1, true, Name = "GetGeneralLedgerAccount")]
        public async Task<ActionResult<GeneralLedgerAccount>> GetAsync(string generalLedgerAccountId)
        {
            try
            {
                return await generalLedgerAccountService.GetAsync(generalLedgerAccountId);
            }
            catch (ConfigurationException cnex)
            {
                logger.LogError(cnex, cnex.Message);
                return CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to get the GL account.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to get the GL account.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Validate a GL account. 
        /// If there is a fiscal year, it will also validate it for that year.
        /// </summary>
        /// <param name="generalLedgerAccountId">GL account ID.</param>
        /// <param name="fiscalYear">Optional; General Ledger fiscal year.</param>
        /// <returns>A <see cref="GlAccountValidationResponse">DTO.</see>/></returns>
        /// <accessComments>
        /// The user can only access those GL accounts for which they have
        /// GL account security access granted.
        /// </accessComments>     
        [HttpGet]
        [HeaderVersionRoute("/general-ledger-account-validation/{generalLedgerAccountId}", 1, true, Name = "GetGlAccountValidation")]
        public async Task<ActionResult<GlAccountValidationResponse>> GetGlAccountValidationAsync(string generalLedgerAccountId,
            [FromQuery(Name = "fiscalYear")] string fiscalYear)
        {
            try
            {
                return await generalLedgerAccountService.ValidateGlAccountAsync(generalLedgerAccountId, fiscalYear);
            }
            catch (ConfigurationException cnex)
            {
                logger.LogError(cnex, cnex.Message);
                return CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "==> generalLedgerAccountService.ValidateGlAccountAsync session expired <==");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to validate the GL account.", HttpStatusCode.BadRequest);
            }
        }
    }
}

// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for GL finance query.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class FinanceQueryController : BaseCompressedApiController
    {
        private readonly IFinanceQueryService financeQueryService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the GL Finance query service object.
        /// </summary>
        /// <param name="financeQueryService">GL Finance query service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinanceQueryController(IFinanceQueryService financeQueryService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.financeQueryService = financeQueryService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves the filtered GL Accounts list
        /// </summary>
        /// <param name="criteria">Finance query filter criteria.</param>
        /// <returns>GL accounts that match the filter criteria.</returns>
        /// <accessComments>
        /// The user can only access those GL accounts for which they have
        /// GL account security access granted.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/finance-query", 1, true, Name = "QueryFinanceQuerySelectionByPostAsync")]
        public async Task<ActionResult<IEnumerable<FinanceQuery>>> QueryFinanceQuerySelectionByPostAsync([FromBody]FinanceQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }                

                return Ok(await financeQueryService.QueryFinanceQuerySelectionByPostAsync(criteria));
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get finance query results", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the filtered GL Account detail data.
        /// </summary>
        /// <param name="criteria">Finance query filter criteria.</param>
        /// <returns>GL account data that match the filter criteria.</returns>
        /// <accessComments>
        /// The user can only access those GL accounts for which they have
        /// GL account security access granted.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/finance-query-detail", 1, true, Name = "QueryFinanceQueryDetailSelectionByPostAsync")]
        public async Task<ActionResult<IEnumerable<FinanceQueryActivityDetail>>> QueryFinanceQueryDetailSelectionByPostAsync([FromBody] FinanceQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }

                return Ok(await financeQueryService.QueryFinanceQueryDetailSelectionByPostAsync(criteria));
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get finance query detail results.", HttpStatusCode.BadRequest);
            }
        }
    }
}


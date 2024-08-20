// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Dtos.BudgetManagement;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;


namespace Ellucian.Colleague.Api.Controllers.BudgetManagement
{
    /// <summary>
    /// Budget Development Configuration controller.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.BudgetManagement)]
    [Authorize]
    public class BudgetDevelopmentController : BaseCompressedApiController
    {
        private readonly IBudgetDevelopmentService budgetDevelopmentService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the Budget Development controller.
        /// </summary>
        /// <param name="budgetDevelopmentService">BudgetDevelopment service object.</param>
        /// <param name="logger">Logger object.</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BudgetDevelopmentController(IBudgetDevelopmentService budgetDevelopmentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.budgetDevelopmentService = budgetDevelopmentService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns the filtered line items in the working budget with or without subtotals.
        /// </summary>
        /// <returns>The working budget line items that match the filtered criteria with or without subtotals.</returns>
        /// <param name="criteria">Working budget filter criteria.</param>
        /// <accessComments>
        /// No permission is needed. A user has access based on what budget officers they are assigned.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/working-budget", 2, true, Name = "QueryWorkingBudget2")]
        public async Task<ActionResult<WorkingBudget2>> QueryWorkingBudgetByPost2Async([FromBody]WorkingBudgetQueryCriteria criteria)
        {
            logger.LogDebug(string.Format("==> QueryWorkingBudgetByPost2Async criteria {0}. <==", Newtonsoft.Json.JsonConvert.SerializeObject(criteria)));

            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }

                var budgetDevelopmentWorkingBudget = await budgetDevelopmentService.QueryWorkingBudget2Async(criteria);
                return budgetDevelopmentWorkingBudget;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the working budget.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates the BudgetDevelopment working budget.
        /// </summary>
        /// <param name="budgetLineItemsDto">A list of budget line items for the working budget.</param>
        /// <returns>A list of updated budget line items for the working budget.</returns>
        /// <accessComments>
        /// No permission is needed. A user may only update budget line items based on what budget officers that they are assigned.
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/budget-development/working-budget", 1, true, Name = "UpdateBudgetDevelopmentWorkingBudget")]
        public async Task<ActionResult<List<BudgetLineItem>>> UpdateBudgetDevelopmentWorkingBudgetAsync([FromBody] List<BudgetLineItem> budgetLineItemsDto)
        {
            logger.LogDebug(string.Format("==> UpdateBudgetDevelopmentWorkingBudgetAsync (budgetLineItemsDto.BudgetAccountId in a list) {0}. <==", budgetLineItemsDto.Select(gl => gl.BudgetAccountId).ToList()));

            try
            {
                return await budgetDevelopmentService.UpdateBudgetDevelopmentWorkingBudgetAsync(budgetLineItemsDto);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Colleague session has expired. Could not update the working budget.");
                return CreateHttpResponseException("Colleague session has expired. Could not update the working budget.", HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to update the working budget.");
                return CreateHttpResponseException("Unable to update the working budget.", HttpStatusCode.BadRequest);
            }
        }



        //////////////////////////////////////////////////////
        //                                                  //
        //               DEPRECATED / OBSOLETE              //
        //                                                  //
        //////////////////////////////////////////////////////


        #region DEPRECATED / OBSOLETE


        #endregion
    }
}

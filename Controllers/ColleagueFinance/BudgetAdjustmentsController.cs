// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Exceptions;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Controls actions for budget adjustments
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class BudgetAdjustmentsController : BaseCompressedApiController
    {
        private IBudgetAdjustmentService budgetAdjustmentService;
        private readonly ILogger logger;

        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="budgetAdjustmentService">Budget adjustment service</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BudgetAdjustmentsController(IBudgetAdjustmentService budgetAdjustmentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.budgetAdjustmentService = budgetAdjustmentService;
            this.logger = logger;
        }

        /// <summary>
        /// Creates a new budget adjustment.
        /// </summary>
        /// <param name="budgetAdjustmentDto">Budget adjustment DTO</param>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/budget-adjustments", 1, true, Name = "CreateBudgetAdjustment")]
        public async Task<ActionResult<Dtos.ColleagueFinance.BudgetAdjustment>> PostAsync([FromBody] Dtos.ColleagueFinance.BudgetAdjustment budgetAdjustmentDto)
        {
            try
            {
                return Ok(await budgetAdjustmentService.CreateBudgetAdjustmentAsync(budgetAdjustmentDto));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to create the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException cex)
            {
                logger.LogError(cex.Message);
                return CreateHttpResponseException("Unable to get budget adjustment configuration.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "==> PostAsync session expired <==");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to create a budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates an existing budget adjustment.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment that will be updated.</param>
        /// <param name="budgetAdjustmentDto">The budget adjustment content that will be updated.</param>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        /// <returns>The updated budget adjustment as it was stored.</returns>
        [HttpPut]
        [HeaderVersionRoute("/budget-adjustments/{id}", 1, true, Name = "UpdateBudgetAdjustment")]
        public async Task<ActionResult<Dtos.ColleagueFinance.BudgetAdjustment>> PutAsync(string id, [FromBody] Dtos.ColleagueFinance.BudgetAdjustment budgetAdjustmentDto)
        {
            try
            {
                return Ok(await budgetAdjustmentService.UpdateBudgetAdjustmentAsync(id, budgetAdjustmentDto));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to update the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException cex)
            {
                logger.LogError(cex.Message);
                return CreateHttpResponseException("Unable to get budget adjustment configuration.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "==> PutAsync session expired <==");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to update the budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a budget adjustment.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment.</param>
        /// <returns>A budget adjustment</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUDGET.ADJUSTMENT.
        /// The current user must be the user that created the budget adjustment.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/budget-adjustments/{id}", 1, true, Name = "GetBudgetAdjustment")]
        public async Task<ActionResult<BudgetAdjustment>> GetBudgetAdjustmentAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                string message = "A budget adjustment number must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await budgetAdjustmentService.GetBudgetAdjustmentAsync(id));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException cex)
            {
                logger.LogError(cex.Message);
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ArgumentException agex)
            {
                logger.LogError(agex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "==> GetBudgetAdjustmentAsync session expired <==");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to get the budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a budget adjustment that is awaiting the user's approval.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment.</param>
        /// <returns>A budget adjustment</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUD.ADJ.PENDING.APPR.
        /// The current user must be one of the users listed as a "next approver" on the budget adjustment.
        /// Adjustment Line data will only be returned for GL accounts that the user has access to based on their GL user access.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/budget-adjustments-pending-approval-detail/{id}", 1, true, Name = "GetBudgetAdjustmentPendingApprovalDetail")]
        public async Task<ActionResult<BudgetAdjustment>> GetBudgetAdjustmentPendingApprovalDetailAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                string message = "A budget adjustment number must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await budgetAdjustmentService.GetBudgetAdjustmentPendingApprovalDetailAsync(id));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException cex)
            {
                logger.LogError(cex.Message);
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ArgumentException agex)
            {
                logger.LogError(agex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "==> GetBudgetAdjustmentPendingApprovalDetailAsync session expired <==");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to get the budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// A user approves an existing budget adjustment.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment that will get the approval ID.</param>
        /// <param name="budgetAdjustmentApprovalDto">The budget adjustment approval data.</param>
        /// <returns>The budget adjustment approval dto.</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUD.ADJ.PENDING.APPR.
        /// The current user must be one of the users listed as a "next approver" on the budget adjustment,
        /// and they must have not approved this budget adjustment already.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/budget-adjustments/{id}/approvals", 1, true, Name = "PostBudgetAdjustmentApproval")]
        public async Task<ActionResult<Dtos.ColleagueFinance.BudgetAdjustmentApproval>> PostBudgetAdjustmentApprovalAsync(string id, [FromBody] Dtos.ColleagueFinance.BudgetAdjustmentApproval budgetAdjustmentApprovalDto)
        {
            try
            {
                return Ok(await budgetAdjustmentService.PostBudgetAdjustmentApprovalAsync(id, budgetAdjustmentApprovalDto));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to approve the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (AlreadyApprovedByUserException appx)
            {
                logger.LogError(appx.Message);
                return CreateHttpResponseException("You have already approved this budget adjustment.", HttpStatusCode.BadRequest);
            }
            catch (NotApprovedStatusException naex)
            {
                logger.LogError(naex.Message);
                return CreateHttpResponseException("The budget adjustment does not have a not approved status.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "==> PostBudgetAdjustmentApprovalAsync session expired <==");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to approve the budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all the budget adjustments summary for a user.
        /// </summary>
        /// <returns>List of budget adjustment summary DTOs for the current user.</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUDGET.ADJUSTMENT.
        /// A user can only get a list of draft and non-draft budget adjustments that they created.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/budget-adjustments-summary", 1, true, Name = "GetBudgetAdjustments")]
        public async Task<ActionResult<IEnumerable<BudgetAdjustmentSummary>>> GetBudgetAdjustmentsSummaryAsync()
        {
            try
            {
                return Ok(await budgetAdjustmentService.GetBudgetAdjustmentsSummaryAsync());
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the budget adjustment summary.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to get budget adjustments summary", HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Get all the budget adjustments pending approval summaries for a user.
        /// </summary>
        /// <returns>List of budget adjustment pending approval summary DTOs for the current user.</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUD.ADJ.PENDING.APPR.
        /// A user can only get a list of budget adjustments pending approval 
        /// where they are a next approver.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/budget-adjustments-pending-approval-summary", 1, true, Name = "GetBudgetAdjustmentsPendingApprovalSummary")]
        public async Task<ActionResult<IEnumerable<BudgetAdjustmentPendingApprovalSummary>>> GetBudgetAdjustmentsPendingApprovalSummaryAsync()
        {
            try
            {
                return Ok(await budgetAdjustmentService.GetBudgetAdjustmentsPendingApprovalSummaryAsync());
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the budget adjustment pending approval summary.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "==> GetBudgetAdjustmentsPendingApprovalSummaryAsync session expired <==");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to get budget adjustments pending approval summary", HttpStatusCode.BadRequest);
            }
        }
    }
}

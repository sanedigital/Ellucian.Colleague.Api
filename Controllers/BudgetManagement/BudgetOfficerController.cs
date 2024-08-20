// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Dtos.BudgetManagement;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.BudgetManagement
{
    /// <summary>
    /// Budget Development Configuration controller.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.BudgetManagement)]
    [Authorize]
    public class BudgetOfficerController : BaseCompressedApiController
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
        public BudgetOfficerController(IBudgetDevelopmentService budgetDevelopmentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.budgetDevelopmentService = budgetDevelopmentService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns the budget officers for the working budget.
        /// </summary>
        /// <returns>The budget officers for the working budget.</returns>
        /// <param name="isInWorkingBudget">Indicates whether to get budget officers for the working budget for a user.</param>
        /// <accessComments>
        /// No permission is needed. A user has access based on what budget officers they and their reporting units are assigned.
        /// </accessComments>
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "isInWorkingBudget")]
        [HeaderVersionRoute("/budget-officers", 1, true, Name = "GetBudgetOfficers")]
        public async Task<ActionResult<List<BudgetOfficer>>> GetBudgetOfficersAsync(bool isInWorkingBudget)
        {
            if (isInWorkingBudget)
            {
                try
                {
                    // Call the service method to return the budget officers for the working budget for the user.
                    var workingBudgetBudgetOfficers = await budgetDevelopmentService.GetBudgetDevelopmentBudgetOfficersAsync();
                    return workingBudgetBudgetOfficers;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    return CreateHttpResponseException("The budget officers for the working budget are not available.", HttpStatusCode.BadRequest);
                }
            }
            else
            {
                logger.LogError("Getting budget officers that are not associated with the working budget is not available.");
                return CreateHttpResponseException("Getting budget officers that are not associated with the working budget is not available.", HttpStatusCode.NotImplemented);
            }
        }
    }
}

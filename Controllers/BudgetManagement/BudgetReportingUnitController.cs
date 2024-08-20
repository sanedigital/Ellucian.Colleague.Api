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
    /// Reporting Unit controller.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.BudgetManagement)]
    [Authorize]
    public class BudgetReportingUnitController : BaseCompressedApiController
    {
        private readonly IBudgetDevelopmentService budgetDevelopmentService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the Reporting Unit controller.
        /// </summary>
        /// <param name="budgetDevelopmentService">BudgetDevelopment service object.</param>
        /// <param name="logger">Logger object.</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BudgetReportingUnitController(IBudgetDevelopmentService budgetDevelopmentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.budgetDevelopmentService = budgetDevelopmentService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns the reporting units for the user in the working budget.
        /// </summary>
        /// <returns>The reporting units for the user in the working budget.</returns>
        /// <param name="isInWorkingBudget">Indicates whether to get reporting units in the working budget for a user.</param>
        /// <accessComments>
        /// No permission is needed. A user has access based on what budget officers they and their reporting units are assigned.
        /// </accessComments>
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "isInWorkingBudget")]
        [HeaderVersionRoute("/budget-reporting-units", 1, true, Name = "GetBudgetReportingUnits")]
        public async Task<ActionResult<List<BudgetReportingUnit>>> GetBudgetReportingUnitsAsync(bool isInWorkingBudget)
        {
            if (isInWorkingBudget)
            {
                try
                {
                    // Call the service method to return the reporting units for the user in the working budget.
                    var workingBudgetReportingUnits = await budgetDevelopmentService.GetBudgetDevelopmentReportingUnitsAsync();
                    return workingBudgetReportingUnits;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    return CreateHttpResponseException("The reporting units for the user for the working budget are not available.", HttpStatusCode.BadRequest);
                }
            }
            else
            {
                logger.LogError("Getting reporting units that are not associated with the working budget is not available.");
                return CreateHttpResponseException("Getting reporting units that are not associated with the working budget is not available.", HttpStatusCode.NotImplemented);
            }
        }
    }
}

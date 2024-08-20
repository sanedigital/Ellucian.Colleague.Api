// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Controls actions for budget adjustments
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class DraftBudgetAdjustmentsController : BaseCompressedApiController
    {
        private IDraftBudgetAdjustmentService draftBudgetAdjustmentService;
        private readonly ILogger logger;

        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="draftBudgetAdjustmentService">Draft Budget adjustment service</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DraftBudgetAdjustmentsController(IDraftBudgetAdjustmentService draftBudgetAdjustmentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.draftBudgetAdjustmentService = draftBudgetAdjustmentService;
            this.logger = logger;
        }

        /// <summary>
        /// Creates a new draft budget adjustment.
        /// </summary>
        /// <param name="draftBudgetAdjustmentDto">Draft Budget adjustment DTO</param>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/draft-budget-adjustments", 1, true, Name = "CreateDraftBudgetAdjustment")]
        public async Task<ActionResult<Dtos.ColleagueFinance.DraftBudgetAdjustment>> PostAsync([FromBody] Dtos.ColleagueFinance.DraftBudgetAdjustment draftBudgetAdjustmentDto)
        {
            try
            {
                return await draftBudgetAdjustmentService.SaveDraftBudgetAdjustmentAsync(draftBudgetAdjustmentDto);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to create the draft budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException cex)
            {
                logger.LogError(cex.Message);
                return CreateHttpResponseException("Unable to create draft budget adjustment - configuration exception.", HttpStatusCode.NotFound);
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
                return CreateHttpResponseException("Unable to create a draft budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates the requested draft budget adjustment.
        /// </summary>
        /// <param name="id">A draft budget adjustment id.</param>
        /// <param name="draftBudgetAdjustmentDto">Draft Budget adjustment DTO</param>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// A user can only update a draft budget adjustments that they created.
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/draft-budget-adjustments/{id}", 1, true, Name = "UpdateDraftBudgetAdjustment")]

        public async Task<ActionResult<Dtos.ColleagueFinance.DraftBudgetAdjustment>> UpdateAsync(string id, [FromBody] Dtos.ColleagueFinance.DraftBudgetAdjustment draftBudgetAdjustmentDto)
        {
            if (id == null)
            {
                return CreateHttpResponseException("id is required in body of request", HttpStatusCode.BadRequest);
            }
            if (draftBudgetAdjustmentDto == null)
            {
                return CreateHttpResponseException("draftBudgetAdjustmentDto is required in body of request", HttpStatusCode.BadRequest);
            }
            try
            {
                draftBudgetAdjustmentDto.Id = id;
                return await draftBudgetAdjustmentService.SaveDraftBudgetAdjustmentAsync(draftBudgetAdjustmentDto);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to update the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException cex)
            {
                logger.LogError(cex.Message);
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ConfigurationException cex)
            {
                logger.LogError(cex.Message);
                return CreateHttpResponseException("Unable to update draft budget adjustment - configuration exception.", HttpStatusCode.BadRequest);
            }
            catch (ApplicationException aex)
            {
                logger.LogError(aex.Message);
                return CreateHttpResponseException("Unable to update draft budget adjustment - application exception.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "==> UpdateAsync session expired <==");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to update draft budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the requested draft budget adjustment.
        /// </summary>
        /// <param name="id">A draft budget adjustment id.</param>
        /// <returns>A draft budget adjustment DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUDGET.ADJUSTMENT
        /// A user can only get a draft budget adjustment that they have created.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/draft-budget-adjustments/{id}", 1, true, Name = "GetDraftBudgetAdjustment")]

        public async Task<ActionResult<Dtos.ColleagueFinance.DraftBudgetAdjustment>> GetAsync(string id)
        {
            if (id == null)
            {
                return CreateHttpResponseException("id is required in body of request", HttpStatusCode.BadRequest);
            }

            try
            {
                return await draftBudgetAdjustmentService.GetAsync(id);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the draft budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException argex)
            {
                logger.LogError(argex.Message);
                return CreateHttpResponseException("Unable to get the draft budget adjustment.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to get the draft budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Delete the requested draft budget adjustment.
        /// </summary>
        /// <param name="id">The draft budget adjustment ID to delete.</param>
        /// <returns>nothing</returns>
        /// <accessComments>
        /// Requires permission DELETE.BUDGET.ADJUSTMENT
        /// A user can only delete a draft budget adjustment that they have created.
        /// </accessComments>
        [HttpDelete]
        [Route("/draft-budget-adjustments/{id}", Name = "DeleteDraftBudgetAdjustment", Order = -10)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                logger.LogError("A draft budget adjustment ID is required.");
                return CreateHttpResponseException("A draft budget adjustment ID is required.", HttpStatusCode.BadRequest);
            }
            try
            {
                await draftBudgetAdjustmentService.DeleteAsync(id);
                return NoContent();
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to delete the draft budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to delete draft budget adjustment.", HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException aex)
            {
                logger.LogError(aex, aex.Message);
                return CreateHttpResponseException("Unable to delete draft budget adjustment - application exception", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "==> DeleteAsync session expired <==");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to delete the draft budget adjustment.", HttpStatusCode.BadRequest);
            }
        }
    }
}

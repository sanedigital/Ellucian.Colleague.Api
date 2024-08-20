// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
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
    /// Provides access to approver objects.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class ApproversController : BaseCompressedApiController
    {
        private readonly IApproverService approverService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the approver service controller.
        /// </summary>
        /// <param name="approverService">Approves service object.</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ApproversController(IApproverService approverService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.approverService = approverService;
            this.logger = logger;
        }

        /// <summary>
        /// Validate an approver ID. 
        /// A next approver ID and an approver ID are the same. They are just
        /// populated under different circumstances.
        /// </summary>
        /// <param name="nextApproverId">Next approver ID.</param>
        /// <returns>A <see cref="NextApproverValidationResponse">DTO.</see>/></returns>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/next-approvers/{nextApproverId}", 1, true, Name = "GetNextApproverValidation")]
        public async Task<ActionResult<NextApproverValidationResponse>> GetNextApproverValidationAsync(string nextApproverId)
        {
            try
            {
                return await approverService.ValidateApproverAsync(nextApproverId);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to validate the next approver.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to validate a next approver.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the list of next apporvers based on keyword search.
        /// </summary>
        /// <param name="queryKeyword">parameter for passing search keyword</param>
        /// <returns>The Next approver search results</returns>      
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.ANY.PERSON or CREATE.UPDATE.REQUISITION or CREATE.UPDATE.PURCHASE.ORDER or CREATE.UPDATE.VOUCHER or CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.34. Use QueryNextApproverByKeywordAsync.")]
        [HttpGet]
        [HeaderVersionRoute("/next-approvers-search/{queryKeyword}", 1, true, Name = "SearchNextApprover")]
        public async Task<ActionResult<IEnumerable<NextApprover>>> GetNextApproverByKeywordAsync(string queryKeyword)
        {
            if (string.IsNullOrEmpty(queryKeyword))
            {
                string message = "query keyword is required to query.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var nextApproverSearchResults = await approverService.QueryNextApproverByKeywordAsync(queryKeyword);
                return Ok(nextApproverSearchResults);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, "Invalid argument.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, "Insufficient permissions to get the approver info.");
                return CreateHttpResponseException("Insufficient permissions to get the approver info.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, "Record not found.");
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to search approver");
                return CreateHttpResponseException("Unable to search approver", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the list of Next Approvers based on keyword search.
        /// </summary>
        /// <param name="criteria">KeywordSearchCriteria parameter for passing search keyword</param>
        /// <returns> The Next approver search results</returns>      
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.ANY.PERSON or CREATE.UPDATE.REQUISITION or CREATE.UPDATE.PURCHASE.ORDER or CREATE.UPDATE.VOUCHER or CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/next-approvers-search", 1, true, Name = "QuerySearchNextApprover")]
        public async Task<ActionResult<IEnumerable<NextApprover>>> QueryNextApproverByKeywordAsync([FromBody] KeywordSearchCriteria criteria)
        {

            if (criteria == null || string.IsNullOrEmpty(criteria.Keyword))
            {
                string message = "query keyword is required to query.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var nextApproverSearchResults = await approverService.QueryNextApproverByKeywordAsync(criteria.Keyword);
                return Ok(nextApproverSearchResults);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, "Invalid argument.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, "Insufficient permissions to get the approver info.");
                return CreateHttpResponseException("Insufficient permissions to get the approver info.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, "Record not found.");
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to search approver");
                return CreateHttpResponseException("Unable to search approver", HttpStatusCode.BadRequest);
            }
        }
    }
}

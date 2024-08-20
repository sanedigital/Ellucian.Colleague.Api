// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    public class DocumentApprovalController : BaseCompressedApiController
    {
        private IDocumentApprovalService documentApprovalService;
        private readonly ILogger logger;

        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="documentApprovalService">Document approval service</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DocumentApprovalController(IDocumentApprovalService documentApprovalService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.documentApprovalService = documentApprovalService;
            this.logger = logger;
        }

        /// <summary>
        /// Get the document approval for the user.
        /// </summary>
        /// <returns>A document approval DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.DOCUMENT.APPROVAL
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/document-approval", 1, true, Name = "GetDocumentApproval")]
        public async Task<ActionResult<Dtos.ColleagueFinance.DocumentApproval>> GetAsync()
        {
            try
            {
                return await documentApprovalService.GetAsync();
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, "Insufficient permissions to get the document approval.");
                return CreateHttpResponseException("Insufficient permissions to get the document approval.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException argex)
            {
                logger.LogError(argex, "Unable to get the document approval.");
                return CreateHttpResponseException("Unable to get the document approval.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "Colleague session timeout to get the document approval.");
                return CreateHttpResponseException("Unable to get the document approval.", HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to get the document approval.");
                return CreateHttpResponseException("Unable to get the document approval.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update document approvals.
        /// </summary>
        /// <param name="documentApprovalUpdateRequest">The document approval update request DTO.</param>        
        /// <returns>The document approval update response DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.DOCUMENT.APPROVAL.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/document-approval", 1, true, Name = "UpdateDocumentApproval")]
        public async Task<ActionResult<Dtos.ColleagueFinance.DocumentApprovalResponse>> PostDocumentApprovalAsync([FromBody] Dtos.ColleagueFinance.DocumentApprovalRequest documentApprovalUpdateRequest)
        {
            // The document approval request cannot be null.
            if (documentApprovalUpdateRequest == null)
            {
                return CreateHttpResponseException("Request body cannot be null.", HttpStatusCode.BadRequest);
            }

            // The list of approval document requests in the document approval request must have objects.
            if (documentApprovalUpdateRequest.ApprovalDocumentRequests == null || !(documentApprovalUpdateRequest.ApprovalDocumentRequests.Any()))
            {
                return CreateHttpResponseException("Request body must have documents to approve.", HttpStatusCode.BadRequest);
            }

            try
            {
                return await documentApprovalService.UpdateDocumentApprovalRequestAsync(documentApprovalUpdateRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to update document approvals.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument to update a document approval.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "Colleague session timeout to update a document approval.");
                return CreateHttpResponseException("Unable to update a document approval.", HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to update a document approval.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves documents approved by the user.
        /// </summary>
        /// <param name="filterCriteria">Approved documents filter criteria.</param>
        /// <returns>List of document approved DTOs.</returns>
        /// <accessComments>
        /// Requires permission VIEW.DOCUMENT.APPROVAL.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/approved-documents", 1, true, Name = "QueryApprovedDocuments")]
        public async Task<ActionResult<IEnumerable<Dtos.ColleagueFinance.ApprovedDocument>>> QueryApprovedDocumentsAsync([FromBody] Dtos.ColleagueFinance.ApprovedDocumentFilterCriteria filterCriteria)
        {
            try
            {
                return Ok(await documentApprovalService.QueryApprovedDocumentsAsync(filterCriteria));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, "Insufficient permissions to get the approved documents.");
                return CreateHttpResponseException("Insufficient permissions to get the approved documents.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException argex)
            {
                logger.LogError(argex, "Unable to get approved documents.");
                return CreateHttpResponseException("Unable to get approved documents.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "Colleague session timeout to get approved documents.");
                return CreateHttpResponseException("Unable to get approved documents.", HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to get approved documents.");
                return CreateHttpResponseException("Unable to get approved documents.", HttpStatusCode.BadRequest);
            }
        }
    }
}

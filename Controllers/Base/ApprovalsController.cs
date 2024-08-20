// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to approvals data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ApprovalsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly IApprovalService _approvalService;

        /// <summary>
        /// ApprovalsController class constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="approvalService">Interface to the approval service</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ApprovalsController(IAdapterRegistry adapterRegistry, ILogger logger, IApprovalService approvalService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            this._logger = logger;
            _approvalService = approvalService;
        }

        /// <summary>
        /// Get an approval document.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="documentId">ID of approval document</param>
        /// <returns>An ApprovalDocument DTO</returns>
        [HttpGet]
        [HeaderVersionRoute("/approvals/document/{documentId}", 1, true, Name = "GetApprovalDocument")]
        public ActionResult<ApprovalDocument> GetApprovalDocument(string documentId)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                string message = "Approval document ID cannot be null.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return _approvalService.GetApprovalDocument(documentId);
             }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogError(knfe.ToString());
                return CreateNotFoundException("Approval Document", documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Get an approval response.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="responseId">ID of approval response</param>
        /// <returns>An ApprovalResponse DTO</returns>
        [HttpGet]
        [HeaderVersionRoute("/approvals/response/{responseId}", 1, true, Name = "GetApprovalResponse")]
        public ActionResult<ApprovalResponse> GetApprovalResponse(string responseId)
        {
            if (string.IsNullOrEmpty(responseId))
            {
                string message = "Approval response ID cannot be null.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(_approvalService.GetApprovalResponse(responseId));
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogError(knfe.ToString());
                return CreateNotFoundException("Approval Response", responseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }
    }
}

// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Data.Colleague.Exceptions;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// The CorrespondenceRequestsController provides access to retrieve and update a person's correspondence requests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CorrespondenceRequestsController : BaseCompressedApiController
    {
        private readonly ICorrespondenceRequestsService CorrespondenceRequestsService;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Dependency Injection constructor for StudentDocumentsController
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="correspondenceRequestsService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CorrespondenceRequestsController(IAdapterRegistry adapterRegistry, ICorrespondenceRequestsService correspondenceRequestsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            CorrespondenceRequestsService = correspondenceRequestsService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all of a person's correspondence requests.
        /// </summary>
        /// <accessComments>
        /// Users may request their own correspondence requests.
        /// Proxy users who have been granted General Required Documents (CORD) proxy access permission
        /// may view the grantor's correspondence requests.
        /// </accessComments>
        /// <param name="personId">The Id of the person for whom to get correspondence requests</param>
        /// <returns>A list of CorrespondenceRequests objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the personId argument is null, empty,
        /// or the user does not have access to the person's correspondence requests</exception>        
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "personId")]
        [HeaderVersionRoute("/correspondence-requests", 1, true, Name = "GetCorrespondenceRequestsAsync")]
        public async Task<ActionResult<IEnumerable<CorrespondenceRequest>>> GetCorrespondenceRequestsAsync([FromQuery] string personId = "")
        {
            if (string.IsNullOrEmpty(personId))
            {
                return CreateHttpResponseException("personId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await CorrespondenceRequestsService.GetCorrespondenceRequestsAsync(personId));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(pe.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting CorrespondenceRequests resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Used to notify back office users when a self-service user has uploaded a new attachment associated with one of their correspondence requests.
        /// If a status code has been specified, the status of the correspondence request will also be changed.
        /// </summary>
        /// <accessComments>
        /// Users may submit attachment notifications for their own correspondence requests.
        /// </accessComments>
        /// <param name="attachmentNotification">Object that contains the person Id, Communication code and optionally the assign date of the correspondence request.</param>
        /// <returns>The CorrespondenceRequest notified of an attachment</returns>
        /// <exception cref="HttpResponseException">Thrown if the personId or communication code properties of the input object are null, empty,
        /// if the correspondence request cannot be updated due to a record lock, 
        /// or if the user does not have access to the person's correspondence requests</exception> 
        [HttpPut]
        [HeaderVersionRoute("/correspondence-requests/attachment-notification", 1, true, Name = "AttachmentNotificationAsync")]
        public async Task<ActionResult<CorrespondenceRequest>> PutAttachmentNotificationAsync(CorrespondenceAttachmentNotification attachmentNotification)
        {
            CorrespondenceRequest returnDto = null;
            if (attachmentNotification == null || string.IsNullOrEmpty(attachmentNotification.PersonId) || string.IsNullOrEmpty(attachmentNotification.CommunicationCode))
            {
                return CreateHttpResponseException("Must provide person Id and communication code.", System.Net.HttpStatusCode.BadRequest);
            }
            try
            {
                returnDto = await CorrespondenceRequestsService.AttachmentNotificationAsync(attachmentNotification);
            }
            catch (RecordLockException ioex)
            {
                // Record lock - status could not be updated
                logger.LogError(ioex, "PutAttachmentNotificationAsync failed due to a record lock.");
                return CreateHttpResponseException(ioex.Message, System.Net.HttpStatusCode.Conflict);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, "PutAttachmentNotificationAsync failed due to a permission problem.");
                return CreateHttpResponseException(peex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException nfex)
            {
                // Record lock - status could not be updated
                logger.LogError(nfex, "PutAttachmentNotificationAsync failed due to the record not found.");
                return CreateHttpResponseException(nfex.Message, System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PutAttachmentNotificationAsync failed due to a repository error.");
                return CreateHttpResponseException(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
            return returnDto;
        }
    }
}

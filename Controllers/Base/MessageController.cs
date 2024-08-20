// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Client;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Updates worktask status.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class MessageController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IMessageService messageService;

        /// <summary>
        /// Creates a message controller object.
        /// </summary>
        /// <param name="messageService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public MessageController(IMessageService messageService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.messageService = messageService;
            this.logger = logger;
        }

        /// <summary>
        /// Update the message for the indicated person
        /// </summary>
        /// <param name="personId">Required. Concatenation of personId and newState </param>
        /// <param name="newState">Required. Concatenation of personId and newState </param>
        /// <param name="msg">Required. Message that is to be updated.</param>
        /// <accessComments>
        /// Only the current user may update their own messages.
        /// </accessComments>
        /// <returns>Updated message</returns>
        [HttpPut]
        public async Task<ActionResult<WorkTask>> UpdateMessageWorklistAsync([FromQuery] string personId, [FromQuery] ExecutionState newState, [FromBody] WorkTask msg)
        {
            var messageId = msg.Id;
            
            try
            {
                if (string.IsNullOrEmpty(messageId))
                {
                    logger.LogError("A query parameter is required to retrieve messages");
                    throw new ArgumentNullException(messageId);
                }
                return await messageService.UpdateMessageWorklistAsync(messageId, personId, newState);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                throw new ColleagueWebApiException();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw new ColleagueWebApiException();
            }

        }

        /// <summary>
        /// Create the message for the indicated person
        /// </summary>
        /// <param name="personId">The person for whom the message is created in database.  </param>
        /// <param name="workflowDefId">ID of the workflow.</param>
        /// <param name="processCode">Process code of workflow.  </param>
        /// <param name="subjectLine">Subject line of worktask</param>
        /// <param name="advisorId">Subject line of worktask</param>
        /// <accessComments>
        /// Only the current user may create their own messages.
        /// </accessComments>
        /// <returns>The newly created message</returns>
        [HttpPost]
        public async Task<ActionResult<WorkTask>> CreateMessageWorklistAsync([FromQuery] string workflowDefId, [FromQuery] string processCode, [FromQuery] string subjectLine, [FromQuery] string advisorId, [FromBody] string personId)
        {

            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    logger.LogError("A person ID is required to retrieve messages.");
                    throw new ArgumentNullException(personId);
                }
                if (string.IsNullOrEmpty(workflowDefId))
                {
                    logger.LogError("A workflow definition ID is required to retrieve messages.");
                    throw new ArgumentNullException(workflowDefId);
                }
                if (string.IsNullOrEmpty(processCode))
                {
                    logger.LogError("A process code is required to retrieve messages.");
                    throw new ArgumentNullException(processCode);
                }
                if (string.IsNullOrEmpty(subjectLine))
                {
                    logger.LogError("A subject line is required to retrieve messages.");
                    throw new ArgumentNullException(subjectLine);
                }

                return await messageService.CreateMessageWorklistAsync(personId, workflowDefId, processCode, subjectLine);

            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                throw new ColleagueWebApiException();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw new ColleagueWebApiException();
            }

        }

    }
}

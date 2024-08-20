// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.TimeManagement.Services;
using Ellucian.Colleague.Dtos.TimeManagement;
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

using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.TimeManagement
{
    /// <summary>
    /// Comments controller
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.TimeManagement)]
    public class TimeHistoryCommentsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly ITimeHistoryCommentsService commentsService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        ///  Constructs the comments controller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="commentsService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TimeHistoryCommentsController(ILogger logger, ITimeHistoryCommentsService commentsService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.commentsService = commentsService;
        }

        #region V1 obsolete
        /// <summary>
        /// Gets all time History comments records the logged in user has permission to get. 
        /// These time History comments records may be associated to the time History of the current user, or to the time History of a subordinate if the current user has a supervisor role.
        /// See the TimeHistoryComments for specific properties of the created and returned objects.
        /// Error conditions: 
        /// 1.  400 - Bad request.
        /// </summary>
        /// 
        /// ///<param name="timeCardHistoryIds">
        /// /// mandatory parameter for passing all timeCardHistoryIds for which we need to fetch comments
        /// ///</param>
        /// <returns>A list of time History comments records</returns>
        [HttpPost]
        [Obsolete("Obsolete as of API verson 1.24; use version GetTimeHistoryComments2Async of this endpoint")]
        [HeaderVersionRoute("/qapi/time-history-comments", 1, true, Name = "GetTimeHistoryCommentsAsync")]
        public async Task<ActionResult<IEnumerable<TimeHistoryComments>>> GetTimeHistoryCommentsAsync([FromBody] IEnumerable<string> timeCardHistoryIds)    
        {
            if (timeCardHistoryIds == null || !timeCardHistoryIds.Any())
            {
                return CreateHttpResponseException("timeCardHistoryIds ids are required in request body", HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await commentsService.GetHistoryCommentsAsync(timeCardHistoryIds));
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to GetTimeHistoryAsync";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
        #endregion

        /// <summary>
        /// Gets all time History comments records the logged in user has permission to get. 
        /// These time History comments records may be associated to the time History of the current user, or to the time History of a subordinate if the current user has a supervisor role.
        /// See the TimeHistoryComments for specific properties of the created and returned objects.
        /// Error conditions: 
        /// 1.  400 - Bad request.
        /// </summary>
        /// 
        /// ///<param name="effectivePersonId">
        /// /// Optional parameter for passing effective person Id
        /// ///</param>
        /// <returns>A list of time History comments records</returns>
        [HttpGet]
        [HeaderVersionRoute("/time-history-comments", 2, true, Name = "GetTimeHistoryComments2Async")]
        public async Task<ActionResult<IEnumerable<TimeHistoryComments>>> GetTimeHistoryComments2Async([FromQuery] string effectivePersonId = null)
        {
            try
            {
                return Ok(await commentsService.GetHistoryComments2Async(effectivePersonId));
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to GetTimeHistoryComments2Async";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedErrorMessage, HttpStatusCode.BadRequest);
            }
        }
    }
}

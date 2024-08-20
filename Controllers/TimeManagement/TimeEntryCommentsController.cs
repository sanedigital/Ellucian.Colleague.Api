// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.TimeManagement.Services;
using Ellucian.Colleague.Dtos.TimeManagement;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;


namespace Ellucian.Colleague.Api.Controllers.TimeManagement
{
    /// <summary>
    /// Comments controller
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.TimeManagement)]
    public class TimeEntryCommentsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly ITimeEntryCommentsService commentsService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        ///  Constructs the comments controller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="commentsService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TimeEntryCommentsController(ILogger logger, ITimeEntryCommentsService commentsService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.commentsService = commentsService;
        }

        /// <summary>
        /// Gets all time entry comments records the logged in user has permission to get. 
        /// These time entry comments records may be associated to the time entry of the current user, or to the time entry of a subordinate if the current user has a supervisor role.
        /// See the TimeEntryComments for specific properties of the created and returned objects.
        /// </summary>
        /// 
        /// <exception>Forbidden. The user is attempting to access time entry comments for an entity other than self or subordinates. </exception>
        /// <exception>BadRequest. The server cannot or will not process the request due to something that is perceived to be a client error.</exception>
        /// 
        /// <param name="effectivePersonId">
        /// Optional parameter for passing effective person Id
        /// </param>
        /// <returns>A list of time entry comments records</returns>
        [HttpGet]
        [HeaderVersionRoute("/time-entry-comments", 1, true, Name = "GetTimeEntryCommentsAsync")]
        public async Task<ActionResult<IEnumerable<TimeEntryComments>>> GetTimeEntryCommentsAsync([FromQuery] string effectivePersonId = null)
        {
            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var comments = await commentsService.GetCommentsAsync(effectivePersonId);

                stopWatch.Stop();
                logger.LogInformation(string.Format("GetTimecards2Async time elapsed: {0} for {1} timecards", stopWatch.ElapsedMilliseconds, comments.Count()));

                return Ok(comments);
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to GetTimeCardsAsync";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Creates a time entry comments record. 
        /// This time entry comments record may be associated to the time entry of the current user, or to the time entry of a subordinate if the current user has a supervisor role.
        /// See the TimeEntryComments for specific properties of the created and returned object.
        /// </summary>
        /// 
        /// <exception>Forbidden.  The user is attempting to create time entry comments for an entity other than self or subordinates. </exception>
        /// <exception>BadRequest. The server cannot or will not process the request due to something that is perceived to be a client error.</exception>
        /// 
        /// <accessComments>
        /// In order to create a time entry comments for an employee, the current user must be:
        /// 1. The employee. An employee cannot create comments for other employees or,
        /// 2. The employee's supervisor. A supervior can only create comments for his/her supervisees or
        /// 3. A proxy for the employee's supervisor. A proxy for a supervisor can only create comments for the supervisor's supervisees.
        /// 
        /// If any of these conditions fail, this route will return a 403 Forbidden status and not create any of the requested timecards.
        /// </accessComments>
        /// 
        /// <param name="comments"></param>
        /// <param name="effectivePersonId"></param>
        /// <returns>TimeEntryComments</returns>
        [HttpPost]
        [HeaderVersionRoute("/time-entry-comments", 1, true, Name = "CreateTimeEntryCommentsAsync")]
        public async Task<ActionResult<TimeEntryComments>> CreateTimeEntryCommentsAsync([FromBody] TimeEntryComments comments, [FromQuery] string effectivePersonId = null)
        {
            try
            {
                return await commentsService.CreateCommentsAsync(comments, effectivePersonId);
            }
            catch(PermissionsException pe)
            {
                var message = string.Format("Current user does not have permission to CreateCommentsAsync for employee {0}", comments.EmployeeId);
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
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }

        }
    }
}

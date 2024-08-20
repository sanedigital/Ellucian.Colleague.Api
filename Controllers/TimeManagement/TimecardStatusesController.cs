// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.TimeManagement.Services;
using Ellucian.Colleague.Dtos.TimeManagement;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;


namespace Ellucian.Colleague.Api.Controllers.TimeManagement
{
    /// <summary>
    /// TimecardStatuses Controller
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.TimeManagement)]
    public class TimecardStatusesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly ITimecardStatusesService timecardStatusService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string invalidPermissionsErrorMessage = "The current user does not have the permissions to perform the requested operation.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="timecardStatusService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TimecardStatusesController(ILogger logger, ITimecardStatusesService timecardStatusService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.timecardStatusService = timecardStatusService;
        }

        /// <summary>
        /// Creates a timecard status for the given timecard. A TimecardStatus represents an action performed on a Timecard at a point in time.
        /// Along with the TimecardStatus, a TimecardHistory record is also created. The TimecardHistory is a copy of the Timecard
        /// during the state in which the TimecardStatus was actioned.
        /// Once a status record is created, it can be read, but never modified.
        /// Changes in status should create a new timecard status record for the associated timecard.
        /// 
        /// Example: If an employee submits a Timecard for approval, a TimecardStatus is created indicating the action performed by the employee. A TimecardHistory record is also created which is a copy of the Timecard when it was submitted.
        /// 
        /// The endpoint will reject the creation of a TimecardStatus if:
        ///     1. 400 - TimecardStatus object is not included in request body
        ///     2. 400 - TimecardStatus timecardId is not same as uri timecardId
        /// </summary>
        /// 
        /// <accessComments>
        /// In order to create a timecard status for an employee, the current user must be:
        /// 1. The employee. An employee cannot create statuses for other employees or,
        /// 2. The employee's supervisor. A supervior can only create statuses for his/her supervisees or
        /// 3. A proxy for the employee's supervisor. A proxy for a supervisor can only create statuses for the supervisor's supervisees.
        /// 
        /// If any of these conditions fail, this route will return a 403 Forbidden status and not create any of the requested timecards.
        /// </accessComments>
        /// 
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="effectivePersonId">Specify the effectivePersonId as a parameter in the URI of the request. Specify the personId of someone else when the current user is proxying for someone else. </param>
        /// <returns>The newly created timecard status</returns>
        [HttpPost]
        [HeaderVersionRoute("/timecards/{id}/timecard-statuses", 1, true, Name = "CreateTimecardStatusAsync")]
        public async Task<ActionResult<TimecardStatus>> CreateTimecardStatusAsync([FromRoute] string id, [FromBody] TimecardStatus status, [FromQuery] string effectivePersonId = null)
        {
            logger.LogDebug("********* Start - Process to create time card status - Start *********");
            if (string.IsNullOrEmpty(id))
            {
                logger.LogDebug("Argument id cannot be null or empty");
                return CreateHttpResponseException("id is required");
            }
            if (status == null)
            {
                logger.LogDebug("Argument status cannot be null or empty");
                return CreateHttpResponseException("TimecardStatus object is missing from request body", HttpStatusCode.BadRequest);
            }
            if (id != status.TimecardId)
            {
                logger.LogDebug("timecardId from uri is not same as status timecardId");
                return CreateHttpResponseException("timecardId from uri is not same as status timecardId", HttpStatusCode.BadRequest);
            }
            try
            {
                logger.LogDebug("fetching CreateTimecardStatusesAsync service method");
                var response = await timecardStatusService.CreateTimecardStatusesAsync(new List<TimecardStatus>() { status }, effectivePersonId);
                logger.LogDebug("fetching CreateTimecardStatusesAsync service method completed successfully");

                logger.LogDebug("********* End - Process to create time card status - End *********");
                return response.FirstOrDefault();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = string.Format("You are not allowed to create a timecard status - {0}", pe.Message);
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a set of timecard statuses. Replaces the individual TimecardStatus post method.
        /// Each TimecardStatus represents an action performed on a Timecard at a point in time.
        /// Along with the TimecardStatus, a TimecardHistory record is also created. The TimecardHistory is a copy of the Timecard
        /// during the state in which the TimecardStatus was actioned.
        /// Once  a status record is created, it can be read, but never modified.
        /// Changes in status should create a new timecard status record for the associated timecard.
        /// 
        /// Example: If an employee submits a Timecard for approval, a TimecardStatus is created indicating the action performed by the employee. A TimecardHistory record is also created which is a copy of the Timecard when it was submitted.
        /// 
        /// The endpoint will reject the creation of TimecardStatus if:
        ///     1. Bad Request: 400 - No TimecardStatus objects are included, or any TimecardStatus object does not provide valid data
        ///     2. Forbidden: 403 - User does not have permissions to create a TimecardStatus object for any incoming object
        /// </summary>
        /// 
        /// <accessComments>
        /// In order to create timecard statuses for an employee, the current user must be:
        /// 1. The employee; an employee cannot create timecards for other employees or,
        /// 2. The employee's supervisor; a supervior can only create timecards for his/her supervisees or
        /// 3. A proxy for the employee's supervisor; a proxy for a supervisor can only create timecards for the supervisor's supervisees.
        /// 
        /// If any of these conditions fail, this route will return a 403 Forbidden status and not create any of the requested timecards.
        /// </accessComments>
        /// 
        /// <param name="statuses"></param>
        /// <param name="effectivePersonId">Specify the effectivePersonId as a parameter in the URI of the request. Specify the personId of someone else when the current user is proxying for someone else. </param>
        /// <returns>The list of all successfully created timecard statuses. Any errors are logged but do not fail the request unless all items fail to post.</returns>
        [HttpPost]
        [HeaderVersionRoute("/timecard-statuses", 1, true, Name = "CreateTimecardStatusesAsync")]
        public async Task<ActionResult<IEnumerable<TimecardStatus>>> CreateTimecardStatusesAsync([FromBody] List<TimecardStatus> statuses, [FromQuery] string effectivePersonId = null)
        {
            logger.LogDebug("********* Start - Process to create a set of time card statuses - Start *********");
            if (statuses == null || statuses.Count() == 0)
            {
                logger.LogDebug("TimecardStatus list is missing from request body");
                return CreateHttpResponseException("TimecardStatus list is missing from request body", HttpStatusCode.BadRequest);
            }
            foreach (var status in statuses)
            {
                if (status == null || string.IsNullOrEmpty(status.TimecardId))
                {
                    logger.LogDebug("Invalid TimecardStatus object found in the request body");
                    return CreateHttpResponseException("Invalid TimecardStatus object found in the request body", HttpStatusCode.BadRequest);
                }
            }
            try
            {
                var timecardstatuses = await timecardStatusService.CreateTimecardStatusesAsync(statuses, effectivePersonId);
                logger.LogDebug("********* End - Process to create a set of time card statuses - End *********");
                return Ok(timecardstatuses);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
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

        /// <summary>
        /// Gets all timecard statuses for a given Timecard id.
        /// The status with the most recent timestamp add datetime should be considered the active status for the timecard
        /// 
        /// The endpoint will reject the update of a Timecard if:
        ///     1. Bad Request: 400 - Timecard id is not provided in URI
        ///     2. Forbidden: 403 - User does not have permissions to get the requested TimecardStatus object
        /// </summary>
        /// <accessComments>
        /// In order to get a timecard status for an employee, the current user must be:
        /// 1. The employee. An employee cannot get a timecard status for another employees or,
        /// 2. The employee's supervisor. A supervior can only get timecard statuses for his/her supervisees or,
        /// 3. A proxy of the employee's supervisor. A proxy-supervisor can only get the timecard statuses of their grantor's supervisees
        /// 
        /// If any of these conditions fail, this route will return a 403 Forbidden status and not create any of the requested timecards.
        /// </accessComments>
        /// <param name="id"></param>
        /// <param name="effectivePersonId"></param>
        /// <returns>All statuses for the timecard</returns>
        [HttpGet]
        [HeaderVersionRoute("/timecards/{id}/timecard-statuses", 1, true, Name = "GetTimecardStatusesByTimecardIdAsync")]
        public async Task<ActionResult<IEnumerable<TimecardStatus>>> GetTimecardStatusesByTimecardIdAsync([FromRoute] string id, [FromQuery] string effectivePersonId = null)
        {
            logger.LogDebug("********* Start - Process to get time card statuses by timecard ID - Start *********");
            if (string.IsNullOrEmpty(id))
            {
                logger.LogDebug("Argument timecard ID cannot be null or empty");
                return CreateHttpResponseException("Timecard id required in request URI", HttpStatusCode.BadRequest);
            }
            try
            {
                var timecardstatuses = await timecardStatusService.GetTimecardStatusesByTimecardIdAsync(id, effectivePersonId);
                logger.LogDebug("********* End - Process to get time card statuses by timecard ID - End *********");
                return Ok(timecardstatuses);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("Timecard", id);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You are not allowed to get timecard statuses for the specified timecard - {0}", pe.Message);
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets all timecard statuses for a given set of Timecard ids.
        /// The status with the most recent timestamp add datetime should be considered the active status for the timecard
        /// 
        /// The endpoint will reject the update of a Timecard if:
        ///     1. Bad request: 400 - Timecard id is not provided in URI 
        ///     2. Forbidden: 403 - User does not have permissions to get one or more of the TimecardStatus objects
        /// </summary>
        /// <accessComments>
        /// In order to get timecard statuses for an employee, the current user must be:
        /// 1. The employee. An employee cannot get a timecard status for another employees or,
        /// 2. The employee's supervisor. A supervior can only get timecard statuses for his/her supervisees or
        /// 3. A proxy of the supervisor. A proxy-supervisor can only get timecard statuses their grantor's supervisees
        /// 
        /// If any of these conditions fail, this route will return a 403 Forbidden status and no TimecardStatuses will be returned
        /// </accessComments>
        /// <param name="effectivePersonId">Person Id of the effective user</param>
        /// <param name="timecardIds">Collection of timecardids to retrieve statuses of</param>
        /// <returns>All statuses for the timecard</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/timecard-statuses", 1, true, Name = "GetTimecardStatusesByTimecardIdsAsync")]
        public async Task<ActionResult<IEnumerable<TimecardStatus>>> QueryTimecardStatusesAsync([FromBody] IEnumerable<string> timecardIds, [FromQuery] string effectivePersonId = null)
        {
            logger.LogDebug("********* Start - Process to query time card statuses - Start *********");
            if (timecardIds == null || !timecardIds.Any())
            {
                logger.LogDebug("Timecard ids are required in request body");
                return CreateHttpResponseException("Timecard ids are required in request body", HttpStatusCode.BadRequest);
            }
            try
            {
                var timecardstatuses = await timecardStatusService.GetTimecardStatusesByTimecardIdsAsync(timecardIds, effectivePersonId);
                logger.LogDebug("********* End - Process to query time card statuses - End *********");
                return Ok(timecardstatuses);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You are not allowed to get timecard statuses for the specified timecards - {0}", pe.Message);
                logger.LogError(pe, pe.Message);
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

        /// <summary>
        /// Get the latest timecard statuses for which you have permission to view. A single timecard can have multiple
        /// statuses, and this endpoint returns the most recent of the statuses.
        /// </summary>
        /// 
        /// The endpoint will reject the update of a Timecard if:
        ///     1. Forbidden: 403 - User does not have permissions to get the timecard statuses for the effective user
        /// 
        ///<accessComments>
        /// In order to get the latest timecard statuses, the current user must be:
        /// 1. The employee. An employee cannot get timecard statuses for another employees or,
        /// 2. The employee's supervisor. A supervior can only get timecard statuses for his/her supervisees or
        /// 3. A proxy of the supervisor. A proxy-supervisor can only get timecard statuses their grantor's supervisees
        /// 
        /// If any of these conditions fail, this route will return a 403 Forbidden status and no TimecardStatuses will be returned
        /// </accessComments>

        /// ///<param name="effectivePersonId">
        /// /// Optional parameter for passing effective person Id - will use current user Id if parameter is null or empty
        /// ///</param>
        /// <returns>Latest statuses for all timecards for which you have permission to view</returns>
        [HttpGet]
        [HeaderVersionRoute("/timecard-statuses", 1, true, Name = "GetLatestTimecardStatusesAsync")]
        public async Task<ActionResult<IEnumerable<TimecardStatus>>> GetLatestTimecardStatusesAsync([FromQuery] string effectivePersonId = null)
        {
            try
            {
                logger.LogDebug("********* Start - Process to get latest time card statuses - Start *********");
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var timecardStatuses = await timecardStatusService.GetTimecardStatusesAsync(false, effectivePersonId);

                stopWatch.Stop();
                logger.LogInformation(string.Format("GetTimecards2Async time elapsed: {0} for {1} timecard statuses", stopWatch.ElapsedMilliseconds, timecardStatuses.Count()));

                logger.LogDebug("********* End - Process to get latest time card statuses - End *********");
                return Ok(timecardStatuses);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You are not allowed to get timecard statuses for the specified timecards - {0}", pe.Message);
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.TimeManagement.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.TimeManagement;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Constraints;
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

using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.TimeManagement
{
    /// <summary>
    /// Exposes access to employee time cards for time entry
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.TimeManagement)]
    public class TimecardHistoriesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly ITimecardHistoriesService timecardHistoriesService; 
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Timecard Histories controller constructor
        /// </summary>
        /// 
        /// <param name="logger"></param>
        /// <param name="timecardHistoriesService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TimecardHistoriesController(ILogger logger, ITimecardHistoriesService timecardHistoriesService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.timecardHistoriesService = timecardHistoriesService;
        }

        /// <summary>
        /// Gets all timecard histories for the currently authenticated API user (as an employee) that exist between a start date and an end date.
        /// 
        /// Example:  if the current user is an employee, all of the employees timecard histories between the start date and end date will be returned.
        /// Example:  if the current user is a manager, all of his/her supervised employees' timecard histories between a start date and an end date will be returned.
        /// 
        /// The endpoint will not return the requested timecard histories if:
        ///     1.  400 - the startDate or endDate is not included in request URI
        ///     2.  403 - User does not have permission to get requested timecard histories
        /// </summary>
        /// 
        /// <returns>
        /// A list of timecard histories.  
        /// See the documentation for TimecardHistory for specific property information.
        /// </returns>
        [HttpGet]
        [Obsolete("Obsolete as of API verson 1.15; use version 2 of this endpoint")]
        [QueryStringConstraint(allowOtherKeys: false, "startDate", "endDate")]
        [HeaderVersionRoute("/timecard-histories", 1, false, Name = "GetTimecardHistoriesAsync")]
        public async Task<ActionResult<IEnumerable<TimecardHistory>>> GetTimecardHistoriesAsync(
            [FromQuery(Name = "startDate")]DateTime startDate,
            [FromQuery(Name = "endDate")]DateTime endDate)
        {
            try
            {
                logger.LogDebug(String.Format("*******Start - Process to get timecard histories for the currently authenticated API user between {0} and {1} - Start***********", startDate, endDate));
                var getTimeCard = await timecardHistoriesService.GetTimecardHistoriesAsync(startDate, endDate);
                logger.LogDebug(String.Format("*******End - Process to get timecard histories for the currently authenticated API user between {0} and {1} - End ***********", startDate, endDate));
                return Ok(getTimeCard);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to GetTimecardHistoriesAsync - {0}", pe.Message);
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
        /// Gets all timecard histories for the currently authenticated API user (as an employee) that exist between a start date and an end date.
        /// 
        /// Example: If the current user is an employee, all of the employees timecard histories between the start date and end date will be returned.
        /// Example: If the current user is a manager, all of his/her supervised employees' timecard histories between a start date and an end date will be returned.
        /// Example: If the current user is an admin, this endpoint returns the timecard histories between the start date and end date for the effectivePersonId
        /// The endpoint will not return the requested timecard histories if:
        ///     1.  400 - the startDate or endDate is not included in request URI
        ///     2.  403 - User does not have permission to get requested timecard histories
        /// </summary>
        /// /// <param name="startDate">
        /// Start Date
        /// </param>
        /// /// <param name="endDate">
        /// End Date
        /// </param>
        /// <param name="effectivePersonId">
        /// Optional parameter for passing effective person Id
        /// </param>
        /// <param name="statusAction">
        /// Optional parameter for passing Time Card Status 
        /// </param>
        /// <returns>
        /// A list of timecard histories.  
        /// See the documentation for TimecardHistory for specific property information.
        /// </returns>
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "startDate", "endDate")]
        [HeaderVersionRoute("/timecard-histories", 2, true, Name = "GetTimecardHistories2Async")]
        public async Task<ActionResult<IEnumerable<TimecardHistory2>>> GetTimecardHistories2Async(
            [FromQuery(Name = "startDate")]DateTime startDate,
            [FromQuery(Name = "endDate")]DateTime endDate,
            [FromQuery(Name = "effectivePersonId")]string effectivePersonId = null, [FromQuery(Name = "statusAction")]string statusAction = null)
        {
            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                logger.LogDebug(String.Format("*******Start - Process to get timecard histories for the currently authenticated API user between {0} and {1} - Start***********", startDate, endDate));
                var timecardHistories = await timecardHistoriesService.GetTimecardHistories2Async(startDate, endDate, effectivePersonId, statusAction);

                stopWatch.Stop();
                logger.LogInformation(string.Format("GetTimecards2Async time elapsed: {0} for {1} timecards", stopWatch.ElapsedMilliseconds, timecardHistories.Count()));
                logger.LogDebug(String.Format("*******End - Process to get timecard histories for the currently authenticated API user between {0} and {1} - End ***********", startDate, endDate));
                return Ok(timecardHistories);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to GetTimecardHistoriesAsync");
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }

        }
    }
}

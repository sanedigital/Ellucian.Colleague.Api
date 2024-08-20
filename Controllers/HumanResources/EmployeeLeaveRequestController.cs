// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using System.Collections;
using Ellucian.Colleague.Dtos.HumanResources;
using System.Net.Http;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// API end-points related to employee leave rquest.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmployeeLeaveRequestController : BaseCompressedApiController
    {
        private readonly IEmployeeLeaveRequestService _employeeLeaveRequestService;
        private readonly ILogger _logger;

        private const string existingResourceErrorMessage = "Cannot create resource that already exists.";
        private const string recordLockErrorMessage = "The record you tried to access was locked.";
        private const string invalidPermissionsErrorMessage = "The current user does not have the permissions to perform the requested operation.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private static readonly string noLeaveRequestIdErrorMessage = "leaveRequestId is required in the request";
        private static readonly string noPositionIdErrorMessage = "positionId is required in the request";
        private static readonly string unexpectedErrorMessage = "Unexpected error occurred while getting leave request details";
        private static readonly string positionSupervisorsUnexpectedErrorMessage = "Unexpected error occurred while getting the position supervisors information";
        private static readonly string noLeaveRequestObjectErrorMessage = "Leave Request DTO is required in body of request";
        private static readonly string noLeaveRequestCommentObjectErrorMessage = "Leave Request Comment DTO is required in body of the request";
        private const string getLeaveRequestRouteId = "GetLeaveRequestInfoByLeaveRequestId";
        private static readonly string supervisorsUnexpectedErrorMessage = "Unexpected error occurred while getting the supervisee information by their primary position";
        private static readonly string unexpectedErrorMessageLeaveRequestsForTimeEntry = "Unexpected error occurred while getting leave request details for the specified date range";
        /// <summary>
        /// Initializes a new instance of the EmployeeLeaveRequestController class.
        /// </summary>
        /// <param name="employeeLeaveRequestService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmployeeLeaveRequestController(IEmployeeLeaveRequestService employeeLeaveRequestService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _employeeLeaveRequestService = employeeLeaveRequestService;
            this._logger = logger;
        }

        /// <summary>
        /// Gets all leave requests for the currently authenticated API user .
        /// All leave requests will be returned regardless of status.
        /// The endpoint will not return the leave requests if:
        ///     1.  403 - User does not have permission to get requested leave request
        ///</summary>
        /// <accessComments>
        /// If the current user is an employee, all of the employee's leave requests will be returned.
        /// If the current user is a leave approver or a proxy of the leave approver, leave requests of all the supervisees will be returned. 
        /// </accessComments>
        /// <param name="effectivePersonId">
        ///  Optional parameter for passing effective person Id
        /// </param>
        /// <returns>A list of Leave Requests</returns>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/leave-requests", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetLeaveRequestsAsyncV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/leave-requests", 1, false, Name = "GetLeaveRequestsAsync")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets a Leave request for currently authenticated API user.",
         HttpMethodDescription = "Gets a Leave request for currently authenticated API user.")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetLeaveRequestsAsync([FromQuery] string effectivePersonId = null)
        {
            try
            {
                _logger.LogDebug("************Start- Process to get Leave Request for a speciifc/logged in user - " + effectivePersonId + " - Start************");
                var leaveRequests = await _employeeLeaveRequestService.GetLeaveRequestsAsync(effectivePersonId);
                _logger.LogDebug("************End- Process to get Leave Requests for a speciifc/logged in user- " + effectivePersonId + " End************");

                return Ok(leaveRequests);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Gets the LeaveRequest information corresponding to the input id.
        /// </summary>
        /// <accessComments>
        /// 1) Any authenticated user can view their own leave request information.
        /// 2) Leave approvers(users with the permission APPROVE.REJECT.LEAVE.REQUEST) or their proxies can view the leave request information of their supervisees. 
        /// </accessComments>
        /// <param name="id">Leave Request Id</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>LeaveRequest DTO</returns>
        [HttpGet]
        [HeaderVersionRoute("/leave-requests/{id}", 1, true, Name = "GetLeaveRequestInfoByLeaveRequestId")]
        public async Task<ActionResult<LeaveRequest>> GetLeaveRequestInfoByLeaveRequestIdAsync([FromRoute] string id, [FromQuery] string effectivePersonId = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogError(noLeaveRequestIdErrorMessage);
                return CreateHttpResponseException(noLeaveRequestIdErrorMessage, HttpStatusCode.BadRequest);
            }
            try
            {
                _logger.LogDebug("************Start- Process to get Leave Requests for a specific leave request Id - Start************");
                var employeeleaverequests = await _employeeLeaveRequestService.GetLeaveRequestInfoByLeaveRequestIdAsync(id, effectivePersonId);
                _logger.LogDebug("************End- Process to get Leave Requests for a specific leave request Id - End************");

                return employeeleaverequests;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = string.Format(pe.Message);
                _logger.LogError(pe, message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(unexpectedErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Approved Leave Requests for a timecard week based on the date range.
        /// </summary>
        /// <accessComments>
        /// 1) Any authenticated user can view their own leave request information within the date range.
        /// 2) Timecard approvers(users with the permission APPROVE.REJECT.TIME.ENTRY) or their proxies can view the leave request information of their supervisees, within the date range. 
        /// </accessComments>
        /// <param name="startDate">Start date of timecard week</param>
        /// <param name="endDate">End date of timecard week</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>List of LeaveRequest DTO</returns>
        [HttpGet]
        [HeaderVersionRoute("/leave-requests-timeentry", 1, true, Name = "GetLeaveRequestsForTimeEntry")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetLeaveRequestsForTimeEntryAsync(DateTime startDate, DateTime endDate, string effectivePersonId = null)
        {
            try
            {
                _logger.LogDebug("************Start- Process to get Leave Requests for time entry - Start************");
                var leaverequests = await _employeeLeaveRequestService.GetLeaveRequestsForTimeEntryAsync(startDate, endDate, effectivePersonId);
                _logger.LogDebug("************End- Process to get Leave Requests for time entry - End************");

                return Ok(leaverequests);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ane)
            {
                var message = "Unexpected null value found in argument(s)";
                _logger.LogError(ane, ane.Message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                var message = "User doesn't have permissions to view approved leave requests for the specified date range ";
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedErrorMessageLeaveRequestsForTimeEntry, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Creates a single Leave Request. This POST endpoint will create a Leave Request along with its associated leave request details 
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can create their own leave request     
        /// Supervisor can create leave requests on behalf of their supervisees
        /// The endpoint will reject the creation of a Leave Request if Employee does not have the correct permission.
        /// </accessComments>
        /// <param name="leaveRequest">Leave Request DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>Newly created Leave Request Object</returns>
        [HttpPost]
        [HeaderVersionRoute("/leave-requests", 1, true, Name = "CreateLeaveRequest")]
        public async Task<ActionResult<LeaveRequest>> CreateLeaveRequestAsync([FromBody] LeaveRequest leaveRequest, [FromQuery] string effectivePersonId = null)
        {
            if (leaveRequest == null)
            {
                return CreateHttpResponseException("Leave Request DTO is required in body of request");
            }
            try
            {
                _logger.LogDebug("************Start - Process to create Leave Request -- Start ************");
                var newLeaveRequest = await _employeeLeaveRequestService.CreateLeaveRequestAsync(leaveRequest, effectivePersonId);
                _logger.LogDebug("************End - Process to create Leave Request -- End ************");

                return newLeaveRequest;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = string.Format(pe.Message);
                _logger.LogError(pe, message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException ere)
            {
                _logger.LogError(ere, ere.Message);
                SetResourceLocationHeader(getLeaveRequestRouteId, new { id = ere.ExistingResourceId });
                var exception = new WebApiException();
                exception.Message = ere.Message;
                exception.AddConflict(ere.ExistingResourceId);
                return CreateHttpResponseException(exception, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create a Leave Request Status record.
        /// </summary>
        /// <accessComments>
        /// 1) Any authenticated user can create a leave request status record for their own leave request.
        /// 2) Leave approvers(users with the permission APPROVE.REJECT.LEAVE.REQUEST) or their proxies can create a leave request status record for the leave requests of their supervisees. 
        /// </accessComments>
        /// <param name="status">Leave Request Status DTO</param>
        /// <param name="effectivePersonId">Optional parameter - Current user or proxy user person id.</param>
        /// <returns>Newly created Leave Request Status</returns>
        [HttpPost]
        [HeaderVersionRoute("/leave-request-statuses", 1, true, Name = "CreateLeaveRequestStatus")]
        public async Task<ActionResult<LeaveRequestStatus>> CreateLeaveRequestStatusAsync([FromBody] LeaveRequestStatus status, [FromQuery] string effectivePersonId = null)
        {
            if (status == null)
            {
                return CreateHttpResponseException(noLeaveRequestObjectErrorMessage, HttpStatusCode.BadRequest);
            }

            try
            {
                _logger.LogDebug("************Start - Process to create Leave Request Status -- Start ************");
                var response = await _employeeLeaveRequestService.CreateLeaveRequestStatusAsync(status, effectivePersonId);
                _logger.LogDebug("************End - Process to create Leave Request Status -- End ************");
                return response;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Gets the HumanResourceDemographics information of supervisors for the given position of a supervisee.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can access the HumanResourceDemographics information of their own supervisors.
        /// </accessComments>
        /// <param name="id">Position Id</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id.</param>
        /// <returns>List of HumanResourceDemographics DTOs</returns>
        [HttpPost]
        [HeaderVersionRoute("/position-supervisors", 1, true, Name = "GetSupervisorsByPositionId")]
        public async Task<ActionResult<IEnumerable<HumanResourceDemographics>>> GetSupervisorsByPositionIdAsync([FromBody] string id, [FromQuery] string effectivePersonId = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogError(noPositionIdErrorMessage);
                return CreateHttpResponseException(noPositionIdErrorMessage, HttpStatusCode.BadRequest);
            }
            try
            {
                _logger.LogDebug("************Start - Process to fetch Supervisors by position Id-- Start ************");
                var supervisors = await _employeeLeaveRequestService.GetSupervisorsByPositionIdAsync(id, effectivePersonId);
                _logger.LogDebug("************End - Process to fetch Supervisors by position Id-- End ************");
                return Ok(supervisors);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(positionSupervisorsUnexpectedErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This end point returns all the supervisees for the currently authenticated leave approver.       
        /// The endpoint will not return the supervisees if:
        ///     1.  403 - User does not have permission to get supervisee information
        /// </summary>
        /// <accessComments>
        ///  Current user must be Leave Approver(users with the permission APPROVE.REJECT.LEAVE.REQUEST) or their proxy to fetch all of their supervisees
        /// </accessComments>
        /// <param name="effectivePersonId">
        ///  Optional parameter for passing effective person Id
        /// </param>
        /// <returns><see cref="HumanResourceDemographics">List of HumanResourceDemographics DTOs</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to fetch supervisees.</exception>
        [HttpGet]
        [HeaderVersionRoute("/leave-approval-supervisees", 1, true, Name = "GetSuperviseesByPrimaryPositionForSupervisor")]
        public async Task<ActionResult<IEnumerable<HumanResourceDemographics>>> GetSuperviseesByPrimaryPositionForSupervisorAsync([FromQuery] string effectivePersonId = null)
        {
            try
            {
                _logger.LogDebug("************Start - Process to fetch supervisee primary position -- Start ************");
                var supervisees = await _employeeLeaveRequestService.GetSuperviseesByPrimaryPositionForSupervisorAsync(effectivePersonId);
                _logger.LogDebug("************End - Process to fetch supervisee primary position -- End ************");
                return Ok(supervisees);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(supervisorsUnexpectedErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This endpoint will update an existing Leave Request along with its Leave Request Details. 
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can update their own leave request.    
        /// Supervisor can update leave requests created by their supervisees 
        /// The endpoint will reject the update of a Leave Request if the employee does not have a valid permission.
        /// </accessComments>       
        /// <param name="leaveRequest"><see cref="LeaveRequest">Leave Request DTO</see></param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns><see cref="LeaveRequest">Newly updated Leave Request object</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the leaveRequest DTO is not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to update the leave request.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if the leave request record to be edited doesn't exist in the DB.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if the leave request record to be edited is locked or if a duplicate leave request record already exists in the DB.</exception>
        [HttpPut]
        [HeaderVersionRoute("/leave-requests", 1, true, Name = "UpdateLeaveRequest")]
        public async Task<ActionResult<LeaveRequest>> UpdateLeaveRequestAsync([FromBody] LeaveRequest leaveRequest, [FromQuery] string effectivePersonId = null)
        {
            try
            {
                if (leaveRequest == null)
                {
                    return CreateHttpResponseException(noLeaveRequestObjectErrorMessage);
                }
                _logger.LogDebug("************Start - Process to update leave request -- Start ************");
                var updatedLeaveRequest = await _employeeLeaveRequestService.UpdateLeaveRequestAsync(leaveRequest, effectivePersonId);
                _logger.LogDebug("************End - Process to update leave request -- End ************");
                return updatedLeaveRequest;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = string.Format(pe.Message);
                _logger.LogError(pe, message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException cnfe)
            {
                _logger.LogError(cnfe, cnfe.Message);
                return CreateNotFoundException("LeaveRequest", leaveRequest.Id);
            }
            catch (RecordLockException rle)
            {
                _logger.LogError(rle, rle.Message);
                return CreateHttpResponseException(recordLockErrorMessage, HttpStatusCode.Conflict);
            }
            catch (ExistingResourceException ere)
            {
                _logger.LogError(ere, ere.Message);
                SetResourceLocationHeader(getLeaveRequestRouteId, new { id = ere.ExistingResourceId });
                var exception = new WebApiException();
                exception.Message = existingResourceErrorMessage;
                exception.AddConflict("ExistingResource");
                return CreateHttpResponseException(exception, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This endpoint will create a new leave request comment associated with a leave request. 
        /// </summary>
        /// <accessComments>
        /// 1) Any authenticated user can create a comment associated with their own leave request.     
        /// 2) Leave approvers(users with the permission APPROVE.REJECT.LEAVE.REQUEST) or their proxies can create a comment for the leave requests of their supervisees.  
        /// </accessComments>     
        /// <param name="leaveRequestComment">Leave Request Comment DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns><see cref="LeaveRequestComment">Leave Request Comment DTO</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to create the leave request comment.</exception>
        [HttpPost]
        [HeaderVersionRoute("/leave-request-comments", 1, true, Name = "CreateLeaveRequestComment")]
        public async Task<ActionResult<LeaveRequestComment>> CreateLeaveRequestCommentsAsync([FromBody] LeaveRequestComment leaveRequestComment, [FromQuery] string effectivePersonId = null)
        {
            if (leaveRequestComment == null)
            {
                return CreateHttpResponseException(noLeaveRequestCommentObjectErrorMessage);
            }
            try
            {
                _logger.LogDebug("************Start - Process to create leave request comment -- Start ************");
                var leaverequestcomment = await _employeeLeaveRequestService.CreateLeaveRequestCommentsAsync(leaveRequestComment, effectivePersonId);
                _logger.LogDebug("************End - Process to create leave request comment -- End ************");
                return leaverequestcomment;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = pe.Message;
                _logger.LogError(pe, message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        #region Experience end-points
        /// <summary>
        /// This ethos pro-code api end-point will return the following necessary information needed to perform leave request from the experience.
        /// 1. Leave Types for Leave requests.
        /// This API also performs a few of the data validations.
        /// The requested information must be owned by the employee.
        /// </summary>
        /// <param name="employeeId">Optional parameter: employeeId for whom the leave request being shown.</param>      
        /// <returns>PositionPayPeriodsTimecards DTO</returns>

        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/leave-request-leave-types", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetLeaveTypesForLeaveRequestAsyncV1.0.0", IsEthosEnabled = true)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets the leave types for leave request. Also performs data validations.",
           HttpMethodDescription = "Gets the leave types for leave request. Also performs data validations.")]
        //ProCode API for Experience
        public async Task<ActionResult<IEnumerable<LeaveRequestLeaveTypes>>> GetLeaveTypesForLeaveRequestAsync(string employeeId = null)
        {
            _logger.LogDebug("********* Start - Process to get leave types for leave request information - Start *********");
            try
            {
                var leaveTypes = await _employeeLeaveRequestService.GetLeaveTypesForLeaveRequestAsync(employeeId);
                _logger.LogDebug("********* End - Process to get leave types for leave request information - End *********");
                return Ok(leaveTypes);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
        #endregion
    }
}

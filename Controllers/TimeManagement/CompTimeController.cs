// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.TimeManagement.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.TimeManagement;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
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
using System.Web;

/// <summary>
/// Exposes access to employee comp time for time entry
/// </summary>
[Authorize]
[LicenseProvider(typeof(EllucianLicenseProvider))]
[EllucianLicenseModule(ModuleConstants.TimeManagement)]
public class CompTimeController : BaseCompressedApiController
{
    private readonly ILogger logger;
    private readonly ICompTimeService compTimeService;

    private const string getCompTimeRouteId = "GetCompTimeAsync";
    private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

    /// <summary>
    /// Comp Time controller constructor
    /// </summary>
    /// 
    /// <param name="logger"></param>
    /// <param name="compTimeService"></param>
    /// <param name="actionContextAccessor"></param>
    /// <param name="apiSettings"></param>
    public CompTimeController(ILogger logger, ICompTimeService compTimeService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
    {

        this.logger = logger;
        this.compTimeService = compTimeService;

    }

    /// <summary>
    /// Gets the comp time record associated with the pay period id for the currently authenticated API user (as an employee).
    /// 
    /// Example:  if the current user is an employee, the associated comp time accrual record to the pay period id will be returned.
    /// Example:  if the current user is a manager, the associated comp time accrual record to the pay period id will be returned.
    /// 
    /// The endpoint will not return the requested CompTimeAccrual if:
    ///     1.  400 - Ids were not included in request URI
    ///     2.  403 - User does not have permission to get requested CompTimeAccrual
    ///     3.  404 - The Comp Time Accrual resources requested does not exist
    /// </summary>
    /// 
    /// /// <param name="criteria"></param>
    /// /// <param name="effectivePersonId">
    /// /// Optional parameter for passing effective person Id
    /// ///</param>
    /// <returns>A comp time accrual record</returns>
    [HttpPost]
    [HeaderVersionRoute("/qapi/comp-time-accrual", 1, true, Name = "QueryCompTimeAccrualAsync")]
    public async Task<ActionResult<List<CompTimeAccrual>>> QueryCompTimeAccrualAsync(CompTimeAccrualQueryCriteria criteria, [FromQuery] string effectivePersonId = null)
    {
        try
        {
            logger.LogDebug("************Start - Process to get the comp time record associated with the pay period id for the current user - Start************");
            if (criteria == null || criteria.Ids == null)
            {
                throw new ArgumentNullException("criteria", "The query criteria must be specified.");
            }

            if (!criteria.Ids.Any())
            {
                throw new ArgumentException("At least one Id for a CompTimeAccrual must be provided");
            }
            var compTimeAccrual = await compTimeService.QueryCompTimeAccrualAsync(criteria, effectivePersonId);
            logger.LogDebug("************End - Process to get the comp time record associated with the pay period id for the current user is successful - End************");
            return compTimeAccrual;
        }
        catch (PermissionsException pe)
        {
            var message = string.Format("You do not have permission to GetCompTimeAccrual - {0}", pe.Message);
            logger.LogError(pe, message);
            return CreateHttpResponseException("You do not have permission to GetCompTimeAccrual", HttpStatusCode.Forbidden);
        }
        catch (KeyNotFoundException knfe)
        {
            logger.LogError(knfe, knfe.Message);
            return CreateHttpResponseException("One of the ids provided was not found.", HttpStatusCode.BadRequest);
        }
        catch (ColleagueSessionExpiredException csse)
        {
            logger.LogError(csse, csse.Message);
            return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return CreateHttpResponseException("An argument was missing, null, or empty.", HttpStatusCode.BadRequest);
        }

    }

    /// <summary>
    /// Gets a collection of all associated OvertimeCompTimeThresholdAllocations with employee, pay period id and end date, and week start and end date.
    /// 
    /// Example:  if the current user is an employee, the associated OvertimeCompTimeThresholdAllocations to the pay period id will be returned.
    /// Example:  if the current user is a manager, the associated OvertimeCompTimeThresholdAllocations to the pay period id will be returned for the employee being viewed.
    /// 
    /// The endpoint will not return the requested collection of OvertimeCompTimeThresholdAllocation if:
    ///     1.  400 - One of the required criteria properties is missing
    ///     2.  403 - User does not have permission to get requested OvertimeCompTimeThresholdAllocation
    ///     3.  404 - The OvertimeCompTimeThresholdAllocation resources requested do not exist
    /// </summary>
    /// 
    /// /// <param name="criteria">The employee id, pay period id, pay period end date, week start date, and week end date</param>
    /// /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
    /// <returns>A list of OvertimeCompTimeThresholdAllocation</returns>
    /// <accessComments>
    /// An authenticated user (supervisor) with the following permission code may retrieve OvertimeCompTimeThresholdAllocations for any of their supervisees.
    /// For proxy-supervisors to retrieve supervisee data, the grantor must have the permission code below.
    /// APPROVE.REJECT.TIME.ENTRY
    /// </accessComments>
    [HttpPost]
    [HeaderVersionRoute("/qapi/overtime-comp-time-threshold-allocation", 1, true, Name = "QueryOvertimeCompTimeThresholdAllocationAsync")]
    public async Task<ActionResult<List<OvertimeCompTimeThresholdAllocation>>> QueryOvertimeCompTimeThresholdAllocationAsync(OvertimeCompTimeThresholdAllocationCriteria criteria, [FromQuery] string effectivePersonId = null)
    {
        try
        {
            logger.LogDebug("************Start - Process to get a collection of all associated OvertimeCompTimeThresholdAllocations with employee, pay period id and end date, and week start and end date - Start************");
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "The query criteria must be specified.");
            }

            if (string.IsNullOrWhiteSpace(criteria.EmployeeId) ||
                string.IsNullOrWhiteSpace(criteria.PayCycleId))
            {
                throw new ArgumentException("An Employee ID and Paycycle ID must be provided");
            }

            if (criteria.PayPeriodEndDate == null)
            {
                throw new ArgumentException("A PayPeriodEndDate must be provided");
            }
            var overtimeCompTime = await compTimeService.QueryOvertimeCompTimeThresholdAllocationAsync(criteria, effectivePersonId);
            logger.LogDebug("************End - Process to get a collection of all associated OvertimeCompTimeThresholdAllocations with employee, pay period id and end date, and week start and end date is successful - End************");
            return overtimeCompTime;
        }
        catch (PermissionsException pe)
        {
            var message = string.Format("You do not have permission to Query OvertimeCompTimeThresholdAllocation - {0}", pe.Message);
            logger.LogError(pe, message);
 
            return CreateHttpResponseException("You do not have permission to Query OvertimeCompTimeThresholdAllocation", HttpStatusCode.Forbidden);
        }
        catch (ColleagueSessionExpiredException csse)
        {
            logger.LogError(csse, csse.Message);
            return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return CreateHttpResponseException("Unable to query OvertimeCompTimeThresholdAllocation", HttpStatusCode.BadRequest);
        }

    }

    /// <summary>
    /// Updates the requested comp time accrual record and associated accrual detail records. The effectivePersonId argument is optional.
    /// <br />
    /// Performs the following actions on associated time entries:
    ///     1.  A new accrual detail record will be created.
    ///     2.  An existing accrual detail record will be modified.
    ///     3.  The absence of a prior accrual detail record will prompt a deletion.
    /// 
    /// The endpoint will reject the update of a Comp Time Accrual if:
    ///     1. 403 - Person does not have the correct permissions to update Comp Time
    ///     2. 404 - The Comp Time Accrual resource requested for update does not exist
    ///     2. 409 - The Comp Time Accrual resource has changed on server
    ///     3. 409 - The Comp Time Accrual resource is locked by another resource
    /// </summary>
    /// <accessComments>
    /// 1. Employees can update their own Comp Time Accrual records.
    /// 2. Supervisors who have a role associated to the permission code - APPROVE.REJECT.TIME.ENTRY -
    /// can update comp time for employees for whom they supervise.
    /// 3. Supervisors can authorize other users (Proxies) via Employee Proxy Self Service. A user who is proxying for a supervisor with the
    /// proper permission code can update comp time accrual records for employees for whom the supervisor supervises.
    /// </accessComments>
    /// <param name="compTimeAccrual">The comp time accrual record</param>
    /// <param name="effectivePersonId">Optional: If the current user is proxying for a supervisor, submit the supervisor's personId as the effectivePersonId</param>
    /// <returns>A CompTimeAccrual record</returns>
    [HttpPut]
    [HeaderVersionRoute("/comp-time-accrual", 1, true, Name = "UpdateCompTimeAccrualAsync")]
    public async Task<ActionResult<CompTimeAccrual>> UpdateCompTimeAccrualAsync([FromBody]CompTimeAccrual compTimeAccrual, [FromQuery] string effectivePersonId = null)
    {
        if (compTimeAccrual == null)
        {
            return CreateHttpResponseException("compTimeAccrual is required in body of request", HttpStatusCode.BadRequest);
        }
        try
        {
            logger.LogDebug("************Start - Process to update the requested comp time accrual record and associated accrual detail records - Start************");
            var updatedCompTimeAccrual = await compTimeService.UpdateCompTimeAccrualAsync(compTimeAccrual, effectivePersonId);
            logger.LogDebug("************End - Process to update the requested comp time accrual record and associated accrual detail records is successful - End************");
            return updatedCompTimeAccrual;
        }
        catch (PermissionsException pe)
        {
            var message = string.Format("You do not have permission to UpdateCompTimeAccrualAsync - {0}", pe.Message);
            logger.LogError(pe, message);
            return CreateHttpResponseException("You do not have permission to UpdateCompTimeAccrualAsync.", HttpStatusCode.Forbidden);
        }
        catch (KeyNotFoundException knfe)
        {
            logger.LogError(knfe, knfe.Message);
            return CreateNotFoundException("CompTimeAccrual", compTimeAccrual.Id);
        }
        catch (RecordLockException rle)
        {
            logger.LogError(rle, rle.Message);
            return CreateHttpResponseException("The record you tried to access was locked. Please wait and try again.", HttpStatusCode.Conflict);
        }
        catch (ColleagueSessionExpiredException csse)
        {
            logger.LogError(csse, csse.Message);
            return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return CreateHttpResponseException("An argument was missing, null, or empty.", HttpStatusCode.BadRequest);
        }
    }

    /// <summary>
    /// Delete a CompTimeAccrual record
    /// The endpoint will thrown an exception if:
    ///     1.  400 - Id is not included in request URI
    ///     2.  403 - User does not have permission to delete requested CompTimeAccrual
    ///     3.  404 - The Comp Time Accrual resource requested to be deleted does not exist
    /// </summary>
    /// <param name="id">CompTimeAccrual Id to delete</param>
    /// <param name="effectivePersonId">Optional: If the current user is proxying for a supervisor, submit the supervisor's personId as the effectivePersonId</param>
    /// <accessComments>
    /// Users with the following permission codes can delete update CompTimeAccrual:
    /// APPROVE.REJECT.TIME.ENTRY
    /// </accessComments>
    [HttpDelete]
    [HeaderVersionRoute("/comp-time-accrual/{id}", 1, true, Name = "DeleteCompTimeAccrual")]
    public async Task<ActionResult<DeleteCompTimeAccrualResponse>> DeleteCompTimeAccrualAsync([FromRoute] string id, [FromQuery] string effectivePersonId = null)
    {
        try
        {
            logger.LogDebug("********Start - Process to delete a comp time accrual record - Start********");
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id", "An id must be specified.");
            }
            var deleteCompTimeAccrual = await compTimeService.DeleteCompTimeAccrualAsync(id, effectivePersonId);
            logger.LogDebug("********End - Process to delete a comp time accrual record is successful- End********");
            return deleteCompTimeAccrual;            
        }
        catch (PermissionsException pe)
        {
            var message = string.Format("You do not have permission to DeleteCompTimeAccrualAsync - {0}", pe.Message);
            logger.LogError(pe, message);
            return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
        }
        catch (KeyNotFoundException knfe)
        {
            logger.LogError(knfe, knfe.Message);
            return CreateNotFoundException("CompTimeAccrual", id);
        }
        catch (ColleagueSessionExpiredException csse)
        {
            logger.LogError(csse, csse.Message);
            return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return CreateHttpResponseException("An argument was null, empty, or missing.", HttpStatusCode.BadRequest);
        }
    }

    /// <summary>
    /// Validates the the comp time submitted by an authenticated API user (as an employee).
    /// 
    /// Example:  if the current user is an employee, the associated comp time accrual record to the pay period id will be returned.
    /// Example:  if the current user is a manager, the associated comp time accrual record to the pay period id will be returned.
    /// 
    /// The endpoint will not return a ValidateCompTimeResponse if:
    ///     1.  400 - Criteria is not included in the body
    ///     2.  403 - User does not have permission to get validate this CompTimeAccrual record
    /// </summary>
    /// 
    /// /// <param name="criteria"></param>
    /// /// <param name="effectivePersonId">
    /// /// Optional parameter for passing effective person Id
    /// ///</param>
    /// <returns>A ValidateCompTimeResponse</returns>
    [HttpPost]
    [HeaderVersionRoute("/qapi/validate-comp-time", 1, true, Name = "ValidateCompTimeAsync")]
    public async Task<ActionResult<ValidateCompTimeResponse>> ValidateCompTimeAsync([FromBody] ValidateCompTimeCriteria criteria, [FromQuery] string effectivePersonId = null)
    {
        try
        {
            logger.LogDebug("***********Start - Process to validate the comp time is submitted by an authenticated API user (as an employee) - Start**********");
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "A criteria must be specified.");
            }
            var validateCompTimeAccrual = await compTimeService.ValidateCompTimeAsync(criteria, effectivePersonId);
            logger.LogDebug("***********End - Process to validate the comp time is submitted by an authenticated API user (as an employee) is successful - End**********");
            return validateCompTimeAccrual;
        }
        catch (PermissionsException pe)
        {
            var message = string.Format("You do not have permission to GetCompTimeAccrual - {0}", pe.Message);
            logger.LogError(pe, message);
            return CreateHttpResponseException("You do not have permission to GetCompTimeAccrual", HttpStatusCode.Forbidden);
        }
        catch (ColleagueSessionExpiredException csse)
        {
            logger.LogError(csse, csse.Message);
            return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return CreateHttpResponseException("An argument was missing, null, or empty.", HttpStatusCode.BadRequest);
        }
    }
}

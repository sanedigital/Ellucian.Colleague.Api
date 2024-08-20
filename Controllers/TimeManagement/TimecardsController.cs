// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.TimeManagement.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.TimeManagement;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Api.Utility;

namespace Ellucian.Colleague.Api.Controllers.TimeManagement
{
    /// <summary>
    /// Exposes access to employee time cards for time entry
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.TimeManagement)]
    [Metadata(ApiDescription = "Provides access to timecard related data", ApiDomain = "Human Resources")]
    [Route("/[controller]/[action]")]
    public class TimecardsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly ITimecardsService timecardsService;

        private const string getTimecardRouteId = "GetTimecardAsync";
        private const string getTimecard2RouteId = "GetTimecard2Async";
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";
        private const string existingResourceErrorMessage = "Cannot create resource that already exists.";
        private const string invalidPermissionsErrorMessage = "The current user does not have the permissions to perform the requested operation.";
        private const string recordLockErrorMessage = "The record you tried to access was locked. Please wait and try again.";

        /// <summary>
        /// TimeCards controller constructor
        /// </summary>
        /// 
        /// <param name="logger"></param>
        /// <param name="timecardsService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TimecardsController(ILogger logger, ITimecardsService timecardsService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {

            this.logger = logger;
            this.timecardsService = timecardsService;

        }

        #region V1 obsolete
        /// <summary>
        /// Gets all timecards for the currently authenticated API user (as an employee).
        /// All timecards will be returned regardless of status.
        /// 
        /// Example:  if the current user is an employee, all of the employees timecards will be returned.
        /// Example:  if the current user is a manager, all of his/her supervised employees' timecards will be returned.
        /// 
        /// The endpoint will not return the requested timecard if:
        ///     1.  400 - Id is not included in request URI
        ///     2.  403 - User does not have permisttion to get requested timecard
        /// </summary>
        /// 
        /// <returns>A list of timecards</returns>
        [HttpGet]
        [Obsolete("Obsolete as of API verson 1.15; use version 2 of this endpoint")]
        [HeaderVersionRoute("/timecards", 1, false, Name = "GetTimecardsAsync")]
        public async Task<ActionResult<IEnumerable<Timecard>>> GetTimecardsAsync()
        {
            logger.LogDebug("********* Start - Process to get Time cards v 1.15 - Start *********");
            try
            {
                var timecards = await timecardsService.GetTimecardsAsync();
                logger.LogDebug("********* End - Process to get Time cards v 1.15 - End *********");
                return Ok(timecards);
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
        /// Gets a single timecard for a given timecard Id
        /// 
        /// The endpoint will not return the requested timecard if:
        ///     1.  400 - Id is not included in request URI
        ///     2.  400 - Unhandled application exception             
        ///     3.  403 - User does not have permisttion to get requested timecard
        ///     4.  404 - Given timecard Id is not found
        /// </summary>
        /// 
        /// <param name="id"></param>
        /// <returns>The requested timecard object</returns>
        [HttpGet]
        [Obsolete("Obsolete as of API verson 1.15; use version 2 of this endpoint")]
        [HeaderVersionRoute("/timecards/{id}", 1, false, Name = "GetTimecardAsync")]
        public async Task<ActionResult<Timecard>> GetTimecardAsync([FromRoute] string id)
        {
            logger.LogDebug("********* Start - Process to get Time card v 1.15 - Start *********");
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.LogDebug("Argument Id cannot be null or empty");
                return CreateHttpResponseException("Id is a required argument", HttpStatusCode.BadRequest);
            }
            try
            {
                var timecard = await timecardsService.GetTimecardAsync(id);
                logger.LogDebug("********* End - Process to get Time card v 1.15 - End *********");
                return timecard;
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to GetTimeCardAsync";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("Timecard", id);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Create a single Timecard. This POST endpoint will create a Timecard along with it's associated time entries.
        /// 
        /// The endpoint will reject the creation of a Timecard if:
        ///     1. 403 - Employee does not have the correct permissions to create the Timecard
        ///     2. 409 - The Timecard resource has changed on server
        ///     3. 409 - The Timecard resource is locked by another resource
        ///     
        /// See the descriptions of the individual properties for more information about acceptable data values
        /// </summary>
        /// 
        /// <param name="timecard"></param>
        /// <returns>The Timecard created, including the new id returned in the response.</returns>
        [HttpPost]
        [Obsolete("Obsolete as of API verson 1.15; use version 2 of this endpoint")]
        [HeaderVersionRoute("/timecards", 1, false, Name = "CreateTimecardAsync")]
        public async Task<ActionResult<Timecard>> CreateTimecardAsync([FromBody] Timecard timecard)
        {
            logger.LogDebug("********* Start - Process to create Time card - Start *********");
            if (timecard == null)
            {
                return CreateHttpResponseException("timecard DTO is required in body of request");
            }
            try
            {
                logger.LogDebug("Calling service to create timecard");
                var newTimecard = await timecardsService.CreateTimecardAsync(timecard);
                logger.LogDebug("Calling service to create timecard completed successfully");
                logger.LogDebug("********* End - Process to create Time card - End *********");
                return Created(Url.Link(getTimecardRouteId, new { id = newTimecard.Id }), newTimecard);
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to CreateTimecardAsync";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException ere)
            {
                logger.LogError(ere, ere.Message);
                SetResourceLocationHeader(getTimecardRouteId, new { id = ere.ExistingResourceId });
                return CreateHttpResponseException(ere.Message, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates the requested Timecard and associated time entries.
        /// 
        /// Performs the following actions on associated time entries:
        ///     1.  A new time entry record will be created.
        ///     2.  An existing time entry record will be modified.
        ///     3.  The absence of a prior time entry record will prompt a deletion.
        /// 
        /// The endpoint will reject the update of a Timecard if:
        ///     1. 403 - Person does not have the correct permissions to update the Timecard
        ///     2. 404 - The Timecard resource requested for update does not exist
        ///     2. 409 - The Timecard resource has changed on server
        ///     3. 409 - The Timecard resource is locked by another resource
        /// </summary>
        /// 
        /// <param name="timecard"></param>
        /// <returns>A timecard</returns>
        [HttpPut]
        [Obsolete("Obsolete as of API verson 1.15; use version 2 of this endpoint")]
        [HeaderVersionRoute("/timecards/{id}", 1, false, Name = "UpdateTimecardAsync")]
        public async Task<ActionResult<Timecard>> UpdateTimecardAsync([FromBody] Timecard timecard)
        {
            logger.LogDebug("********* Start - Process to update Time card - Start *********");
            if (timecard == null)
            {
                logger.LogDebug("timecard is required in body of request");
                return CreateHttpResponseException("timecard is required in body of request", HttpStatusCode.BadRequest);
            }
            try
            {
                var updatedTimecard = await timecardsService.UpdateTimecardAsync(timecard);
                logger.LogDebug("********* End - Process to update Time card - End *********");
                return updatedTimecard;
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to UpdateTimeCardAsync";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException cnfe)
            {
                logger.LogError(cnfe, cnfe.Message);
                return CreateNotFoundException("Timecard", timecard.Id);
            }
            catch (RecordLockException rle)
            {
                logger.LogError(rle, rle.Message);
                return CreateHttpResponseException(rle.Message, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        /// <summary>
        /// Gets all timecards for the currently authenticated API user (as an employee).
        /// All timecards will be returned regardless of status.
        /// 
        /// Example:  if the current user is an employee, all of the employees timecards will be returned.
        /// Example:  if the current user is a manager, all of his/her supervised employees' timecards will be returned.
        /// 
        /// The endpoint will not return the requested timecard if:
        ///     1.  400 - Id is not included in request URI
        ///     2.  403 - User does not have permisttion to get requested timecard
        /// </summary>
        /// 
        /// ///<param name="effectivePersonId">
        /// /// Optional parameter for passing effective person Id
        /// ///</param>
        /// <returns>A list of timecards</returns>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/timecards", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetTimecards2AsyncV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/timecards", 2, false, Name = "GetTimecards2Async")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets all timecards for the currently authenticated API user (as an employee).All timecards will be returned regardless of status.",
            HttpMethodDescription = "Gets all timecards for the currently authenticated API user(as an employee). All timecards will be returned regardless of status.")]
        public async Task<ActionResult<IEnumerable<Timecard2>>> GetTimecards2Async([FromQuery] string effectivePersonId = null)
        {
            try
            {
                logger.LogDebug("********* Start - Process to get Time cards v2 - Start *********");
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                logger.LogDebug("Calling GetTimecards2Async service method");
                var timecards = await timecardsService.GetTimecards2Async(effectivePersonId);
                logger.LogDebug("Calling GetTimecards2Async service method completed successfully");

                stopWatch.Stop();
                logger.LogInformation(string.Format("GetTimecards2Async time elapsed: {0} for {1} timecards", stopWatch.ElapsedMilliseconds, timecards.Count()));
                logger.LogDebug("********* End - Process to get Time cards v2 - End *********");

                return Ok(timecards);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to GetTimeCards2Async - {0}", pe.Message);
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
        /// Gets a single timecard for a given timecard Id
        /// 
        /// The endpoint will not return the requested timecard if:
        ///     1.  400 - Id is not included in request URI
        ///     2.  400 - Unhandled application exception             
        ///     3.  403 - User does not have permisttion to get requested timecard
        ///     4.  404 - Given timecard Id is not found
        /// </summary>
        /// 
        /// <param name="id"></param>
        /// <param name="effectivePersonId"></param>
        /// <returns>The requested timecard object</returns>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/timecards/{id}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetTimecard2AsyncV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/timecards/{id}", 2, false, Name = "GetTimecard2Async")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets a single timecard for a given timecard Id.",
            HttpMethodDescription = "Gets a single timecard for a given timecard Id.")]
        public async Task<ActionResult<Timecard2>> GetTimecard2Async([FromRoute] string id, [FromQuery] string effectivePersonId = null)
        {
            logger.LogDebug("********* Start - Process to get Time card v2 - Start *********");
            if (string.IsNullOrWhiteSpace(id))
            {
                return CreateHttpResponseException("Id is a required argument", HttpStatusCode.BadRequest);
            }
            try
            {
                var timecards = await timecardsService.GetTimecard2Async(id, effectivePersonId);
                logger.LogDebug("********* End - Process to get Time card v2 - End *********");
                return timecards;
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to GetTimeCard2Async - {0}", pe.Message);
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("Timecard", id);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Create a single Timecard. This POST endpoint will create a Timecard along with it's associated time entries.
        /// 
        /// The endpoint will reject the creation of a Timecard if:
        ///     1. 403 - Employee does not have the correct permissions to create the Timecard
        ///     2. 409 - The Timecard resource has changed on server
        ///     3. 409 - The Timecard resource is locked by another resource
        ///     
        /// See the descriptions of the individual properties for more information about acceptable data values
        /// </summary>
        /// 
        /// <param name="timecard2"></param>
        /// <param name="effectivePersonId"></param>
        /// <returns>The Timecard created, including the new id returned in the response.</returns>
        [HttpPost]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/timecards", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "CreateTimecard2AsyncV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/timecards", 2, false, Name = "CreateTimecard2Async")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Create a timecard along with it's associated time entries.",
            HttpMethodDescription = "Create a timecard along with it's associated time entries.")]
        public async Task<ActionResult<Timecard2>> CreateTimecard2Async([ModelBinder(typeof(EedmModelBinder))] Timecard2 timecard2, [FromQuery] string effectivePersonId = null)
        {
            logger.LogDebug("********* Start - Process to create Time card v2 - Start *********");
            if (timecard2 == null)
            {
                logger.LogDebug("timecard DTO is required in body of request");
                return CreateHttpResponseException("timecard DTO is required in body of request");
            }
            try
            {
                // Put the imported extended data from the EthosEnabledBinder extract into the service
                await timecardsService.ImportExtendedEthosData(ImportExtendedEthosData());

                var newTimecard = await timecardsService.CreateTimecard2Async(timecard2, effectivePersonId);
                logger.LogDebug("********* End - Process to create Time card v2 - End *********");
                return Created(Url.Link(getTimecard2RouteId, new { id = newTimecard.Id }), newTimecard);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to CreateTimecardAsync - {0}", pe.Message);
                logger.LogError(pe, message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException ere)
            {
                logger.LogError(ere, ere.Message);
                SetResourceLocationHeader(getTimecard2RouteId, new { id = ere.ExistingResourceId });
                var exception = new WebApiException();
                exception.Message = ere.Message;
                exception.AddConflict(ere.ExistingResourceId);
                return CreateHttpResponseException(existingResourceErrorMessage, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Use this route to create multiple timecards at once. This route reduces load on the API servers and is much more effective
        /// for the use case than issuing separate requests to create single timecards. The RequestGuid attributes in the CreateTimecardRequest
        /// object and the CreateTimecardResponse object allow the consuming client to "join" any given request to its associated response.
        /// </summary>
        /// <accessComments>
        /// In order to create a timecard for an employee, the current user must:
        /// 1. Be the employee. An employee cannot create timecards for other employees.
        /// 2. Be the employee's supervisor. A supervior can only create timecards for his/her supervisees.
        /// 3. Be a proxy for the employee's supervisor. A proxy for a supervisor can only create timecards for the supervisor's supervisees.
        /// 
        /// If any of these conditions fail, this route will return a 403 Forbidden status and not create any of the requested timecards.
        /// </accessComments>
        /// <param name="createRequests">Specify the array of CreateTimecardRequest objects in the body of the request</param>
        /// <param name="effectivePersonId">Specify the effectivePersonId as a parameter in the URI of the request. Specify the personId of someone else when the current user is proxying for someone else. </param>
        /// <returns>An array of CreateTimecardResponse objects that indicate the success or failure of the associated create request. If the StatusCode in the response is a 409 Conflict, the timecard
        /// already exists. The consuming client can use the ResourceLocation and the TimecardId attributes of the CreateTimecardResponse object to get or update the existing resource.</returns>
        [HttpPost]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/bulk-timecards", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "BulkCreateTimecardsAsyncV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/timecards", 3, false, Name = "BulkCreateTimecard2Async")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Create multiple timecards without it's associated time entries.",
            HttpMethodDescription = "Create multiple timecards without it's associated time entries.")]
        public async Task<ActionResult<IEnumerable<CreateTimecardResponse>>> BulkCreateTimecardsAsync([FromBody] IEnumerable<CreateTimecardRequest> createRequests, [FromQuery] string effectivePersonId = null)
        {
            logger.LogDebug("********* Start - Process to Bulk create Time cards - Start *********");
            if (createRequests == null)
            {
                return CreateHttpResponseException("createTimecardRequests are required in body of request");
            }

            try
            {
                var responses = await timecardsService.BulkCreateTimecards(createRequests, effectivePersonId);
                foreach (var response in responses)
                {
                    if (response.StatusCode == HttpStatusCode.Created ||
                        response.StatusCode == HttpStatusCode.Conflict)
                    {
                        response.ResourceLocation = Url.Link(getTimecard2RouteId, new { id = response.TimecardId });
                    }
                }
                logger.LogDebug("********* End - Process to Bulk create Time cards - End *********");
                return Ok(responses);
            }
            catch (ArgumentNullException ane)
            {

                var message = string.Format("Some argument is null - {0}", ane.Message);
                logger.LogError(ane, message);
                return CreateHttpResponseException(message);
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException(ae.Message);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to create the requested timecards - {0}", pe.Message);
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




        /// <summary>
        /// Updates the requested Timecard and associated time entries. The effectivePersonId argument is optional.
        /// <br />
        /// Performs the following actions on associated time entries:
        ///     1.  A new time entry record will be created.
        ///     2.  An existing time entry record will be modified.
        ///     3.  The absence of a prior time entry record will prompt a deletion.
        /// 
        /// The endpoint will reject the update of a Timecard if:
        ///     1. 403 - Person does not have the correct permissions to update the Timecard
        ///     2. 404 - The Timecard resource requested for update does not exist
        ///     3. 409 - The Timecard resource has changed on server
        ///     4. 409 - The Timecard resource is locked by another resource
        /// </summary>
        /// <accessComments>
        /// 1. Employees can update their own timecards.
        /// 2. Supervisors who have a role associated to the permission code - APPROVE.REJECT.TIME.ENTRY -
        /// can update timecards for employees for whom they supervise.
        /// 3. Supervisors can authorize other users (Proxies) via Employee Proxy Self Service. A user who is proxying for a supervisor with the
        /// proper permission code can update timecards for employees for whom the supervisor supervises.
        /// </accessComments>
        /// <param name="timecard2">The timecard object to update</param>
        /// <param name="effectivePersonId">Optional: If the current user is proxying for a supervisor, submit the supervisor's personId as the effectivePersonId</param>
        /// <returns>A timecard</returns>
        [HttpPut]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/timecards/{id}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "UpdateTimecard2AsyncV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/timecards/{id}", 2, false, Name = "UpdateTimecard2Async")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Update a timecard along with it's associated time entries.",
            HttpMethodDescription = "Update a timecard along with it's associated time entries.")]
        public async Task<ActionResult<Timecard2>> UpdateTimecard2Async([ModelBinder(typeof(EedmModelBinder))] Timecard2 timecard2, [FromQuery] string effectivePersonId = null)
        {
            logger.LogDebug("********* Start - Process to update Time cards - Start *********");
            if (timecard2 == null)
            {
                logger.LogDebug("timecard is required in body of request");
                return CreateHttpResponseException("timecard is required in body of request", HttpStatusCode.BadRequest);
            }
            try
            {
                // Put the imported extended data from the EthosEnabledBinder extract into the service
                await timecardsService.ImportExtendedEthosData(ImportExtendedEthosData());

                var updatedTimecard = await timecardsService.UpdateTimecard2Async(timecard2, effectivePersonId);
                logger.LogDebug("********* End - Process to update Time cards - End *********");
                return updatedTimecard;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to UpdateTimeCardAsync - {0}", pe.Message);
                logger.LogError(pe, message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException cnfe)
            {
                logger.LogError(cnfe, cnfe.Message);
                return CreateNotFoundException("Timecard", timecard2.Id);
            }
            catch (RecordLockException rle)
            {
                logger.LogError(rle, rle.Message);
                return CreateHttpResponseException(recordLockErrorMessage, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves timecards that are associated with the leave requests
        /// </summary>
        /// <accessComments>
        /// 1) Any authenticated user can view the timecards associated to their own leave requests.
        /// 2) Leave approvers(users with the permission APPROVE.REJECT.LEAVE.REQUEST) or their proxies can view the timecards associated to the leave requests of their supervisees. 
        /// </accessComments>
        /// <param name="leaveRequestId">Optional parameter to fetch timecard linked to a specific leave request id</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns><see cref="Timecard2"/>List of Timecard DTO</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned incase of any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to view timecards linked to the leave requests.</exception>
        [HttpGet]
        [HeaderVersionRoute("/timecards-with-leaverequests", 1, true, Name = "GetTimecardsAssociatedWithLeaveRequests")]
        public async Task<ActionResult<IEnumerable<Timecard2>>> GetTimecardsAssociatedWithLeaveRequestsAsync(string leaveRequestId = null, string effectivePersonId = null)
        {

            logger.LogDebug("********* Start - Process to get Time cards associated with leave requests - Start *********");
            try
            {
                var timecards = await timecardsService.GetTimecardsAssociatedWithLeaveRequestsAsync(leaveRequestId, effectivePersonId);
                logger.LogDebug("********* End - Process to get Time cards associated with leave requests - End *********");
                return Ok(timecards);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                var message = "Unexpected null values found in argument(s).See log for more details.";
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, ae.Message);
                var message = "Invalid values found in argument(s).See log for more details.";
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                var message = "User doesn't have permissions to view timecards associated with the specified leave requests";
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = "Unable to retrieve timecards associated with leave requests";
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Retrieves timecards associated with leave employees with timecards
        /// </summary>
        /// <accessComments>
        /// 1) Any authenticated user can view the timecards associated to their own leave requests.
        /// 2) Leave approvers(users with the permission APPROVE.REJECT.LEAVE.REQUEST) or their proxies can view the timecards associated to the leave requests of their supervisees.
        /// </accessComments>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <param name="lookbackStartDate">Optional parameter for enforcing a lookback period</param>
        /// <returns><see cref="Timecard2"/>List of Timecard DTO</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned incase of any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to view timecards linked to the leave requests.</exception>
        [HttpGet]
        [HeaderVersionRoute("/timecards-for-leave", 1, true, Name = "GetTimecardsForLeaveAsync")]
        public async Task<ActionResult<IEnumerable<Timecard2>>> GetTimecardsForLeaveAsync(string effectivePersonId = null, DateTime? lookbackStartDate = null)
        {
            logger.LogDebug("********* Start - Process to get Time cards for leave - Start *********");
            try
            {
                var timecards = await timecardsService.GetTimecardsForLeaveAsync(effectivePersonId, lookbackStartDate);
                logger.LogDebug("********* End - Process to get Time cards for leave - End *********");
                return Ok(timecards);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                var message = "Unexpected null values found in argument(s).See log for more details.";
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, ae.Message);
                var message = "Invalid values found in argument(s).See log for more details.";
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                var message = "User doesn't have permissions to view timecards associated with the specified leave requests";
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = "Unable to retrieve timecards associated with leave requests";
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

        }

        #region Experience end-points
        /// <summary>
        /// This ethos pro-code api end-point will return the following necessary information needed to perform clock-in/out from the experience.
        /// 1. Pay period information for all active clock positions for today.
        /// 2. Timecards associated with the employee.
        /// 3. Latest status of all the associated timecards.
        /// This API also performs a few of the data validations.
        /// The requested information must be owned by the employee.
        /// </summary>
        /// <param name="employeeId">Optional parameter: employeeId for whom the position pay periods and the timecards information is being requested.</param>      
        /// <param name="getAll">Optiomal parameter: Gets only the position pay periods information when set to false</param>
        /// <returns>PositionPayPeriodsTimecards DTO</returns>

        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/position-pay-periods-timecards", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPositionPayPeriodsTimecardsAsyncV1.0.0", IsEthosEnabled = true)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets the position pay periods, timecards, and the latest timecard statuses information. Also performs data validations.",
           HttpMethodDescription = "Gets the position pay periods, timecards, and the latest timecard statuses information. Also performs data validations.")]
        //ProCode API for Experience
        public async Task<ActionResult<PositionPayPeriodsTimecards>> GetPositionPayPeriodsTimecardsAsync(bool getAll = true, string employeeId = null)
        {
            logger.LogDebug("********* Start - Process to get position pay periods and the timecards information - Start *********");
            try
            {
                var positionPayPeriodTimecardsInformation = await timecardsService.GetPositionPayPeriodsTimecardsAsync(getAll, employeeId);
                logger.LogDebug("********* End - Process to get position pay periods and the timecards information - End *********");
                return positionPayPeriodTimecardsInformation;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ApplicationException e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
        #endregion
    }
}

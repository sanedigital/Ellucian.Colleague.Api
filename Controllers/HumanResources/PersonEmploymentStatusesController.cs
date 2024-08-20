// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes personemploymentstatus data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonEmploymentStatusesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPersonEmploymentStatusService personEmploymentStatusService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="personEmploymentStatusService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonEmploymentStatusesController(ILogger logger, IPersonEmploymentStatusService personEmploymentStatusService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.personEmploymentStatusService = personEmploymentStatusService;
        }

        /// <summary>
        /// Get personEmploymentStatus objects. This endpoint returns objects based on the current
        /// user's/user with proxy's permissions.
        /// </summary>
        /// <accessComments>
        /// Example: If the current user is an admin, this endpoint returns the personEmploymentStatuses for the effectivePersonId
        /// Example: If the current user/user with proxy is an employee, this endpoint returns that employee's/proxied employee's personEmploymentStatuses
        /// Example: If the current user/user with proxy is a manager, this endpoint returns all the personEmploymentStatuses of the employees reporting to the manager
        /// Example: If the current user is a leave approver with the APPROVE.REJECT.LEAVE.REQUEST permission, this end point returns the leave approver's PersonEmploymentStatus
        /// and personEmploymentStatuses of all the employees whose leave requests are handled by this leave approver.
        ///</accessComments>
        /// <param name="effectivePersonId">Optional parameter for effective person Id</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns>A list of PersonEmploymentStatus objects</returns>
        [HttpGet]
        [HeaderVersionRoute("/person-employment-statuses", 1, true, Name = "GetPersonEmploymentStatuses")]
        public async Task<ActionResult<IEnumerable<PersonEmploymentStatus>>> GetPersonEmploymentStatusesAsync(string effectivePersonId = null, DateTime? lookupStartDate = null)
        {
            try
            {
                return Ok(await personEmploymentStatusService.GetPersonEmploymentStatusesAsync(effectivePersonId, lookupStartDate));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                var genericErrorMessage = "Unknown error occurred while getting person employment statuses";
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(genericErrorMessage, HttpStatusCode.BadRequest);
            }
        }
    }
}

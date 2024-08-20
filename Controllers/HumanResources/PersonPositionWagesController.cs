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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// PersonPositionWages Controller
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonPositionWagesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPersonPositionWageService personPositionWageService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedErrorMessage = "Unexpected error occurred while getting leave request details";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="personPositionWageService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonPositionWagesController(ILogger logger, IPersonPositionWageService personPositionWageService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.personPositionWageService = personPositionWageService;
        }

        /// <summary>
        /// Gets PersonPositionWage objects based on the authenticated user's permissions. 
        /// </summary> 
        /// <accessComments>
        /// This endpoint returns PersonPositionWage objects based on the current authenticated user's permissions.
        /// Example: If the current user is an admin, this endpoint returns the PersonPositionWages for the effectivePersonId.
        /// Example: If the current user/user who has proxy is an employee, this endpoint returns that employee's/proxy user's PersonPositionWages.
        /// Example: If the current user/user who has proxy is a supervisor with the permission ACCEPT.REJECT.TIME.ENTRY, this endpoint returns the supervisor's PersonPositionWages 
        /// and PersonPositionWages of all the employees reporting to this supervisor.
        /// Example: If the current user is a leave approver with the APPROVE.REJECT.LEAVE.REQUEST permission, this end point returns the leave approver's PersonPositionWages
        /// and PersonPositionWages of all the employees whose leave requests are handled by this leave approver.
        /// </accessComments>
        /// <param name="effectivePersonId">Optional parameter for effective personId, which should be used when proxying on behalf of another user.</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns>A collection of PersonPositionWage objects</returns>
        [HttpGet]
        [HeaderVersionRoute("/person-position-wages", 1, true, Name = "GetPersonPositionWages")]
        public async Task<ActionResult<IEnumerable<PersonPositionWage>>> GetPersonPositionWagesAsync(string effectivePersonId = null, DateTime? lookupStartDate = null)
        {
            try
            {
                return Ok(await personPositionWageService.GetPersonPositionWagesAsync(effectivePersonId, lookupStartDate));
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

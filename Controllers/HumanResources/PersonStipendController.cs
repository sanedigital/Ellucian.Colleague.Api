// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
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
using System.Web;

using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// PersonStipend Controller
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonStipendController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPersonStipendService personStipendService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="personStipendService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonStipendController(ILogger logger, IPersonStipendService personStipendService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.personStipendService = personStipendService;
        }

        /// <summary>
        /// Get PersonStipend objects. 
        /// </summary> 
        /// <accessComments>
        /// This endpoint returns PersonStipend objects based on the current user's permissions.
        /// Example: If the current user is an admin with the permission VIEW.ALL.TIME.HISTORY, this endpoint returns the PersonStipend objects for the effectivePersonId
        /// Example: If the current user/user who has proxy is an employee, this endpoint returns that employee's/proxy user's PersonStipend objects
        /// Example: If the current user/user who has proxy is a supervisor with the permission ACCEPT.REJECT.TIME.ENTRY, 
        /// this endpoint returns the supervisor's PersonStipend objects and all the PersonStipend objects of the supervisees (employees reporting to the supervisor)
        /// </accessComments>
        /// <param name="effectivePersonId">Optional parameter for effective personId, which should be used when proxying on behalf of another user.</param>
        /// <returns>PersonStipend objects</returns>
        [HttpGet]
        [HeaderVersionRoute("/person-stipend", 1, true, Name = "GetPersonStipendAsync")]
        public async Task<ActionResult<IEnumerable<PersonStipend>>> GetPersonStipendAsync(string effectivePersonId = null)
        {
            try
            {
                return Ok(await personStipendService.GetPersonStipendAsync(effectivePersonId));
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                var message = "Unexpected error occurred while getting person stipends";
                logger.LogError(e.ToString());              
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

    }
}

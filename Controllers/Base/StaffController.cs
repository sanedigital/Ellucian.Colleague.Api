// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Staff data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class StaffController : BaseCompressedApiController
    {
        private readonly IStaffService _staffService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StaffController class.
        /// </summary>
        /// <param name="staffService">Service of type <see cref="IStaffService">IStaffService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StaffController(IStaffService staffService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _staffService = staffService;
            this._logger = logger;
        }

        /// <summary>
        /// Get the staff record for the provided ID
        /// </summary>
        /// <param name="staffId">ID for the staff member</param>
        /// <returns>A staff record</returns>
        /// <accessComments>
        /// API endpoint is secured so that only requestor can access data
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/staff/{staffId}", 1, true, Name = "GetStaff")]
        public async Task<ActionResult<Staff>> GetAsync(string staffId)
        {
            if (string.IsNullOrEmpty(staffId))
            {
                _logger.LogError("Invalid staffId " + staffId);
                return CreateHttpResponseException("Invalid staffId " + staffId, HttpStatusCode.BadRequest);
            }
            try
            {
                var staff = await _staffService.GetAsync(staffId);
                return staff;
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving staff record";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while retrieving staff record.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the restrictions for the indicated staff.
        /// </summary>
        /// <param name="staffId">ID of the staff</param>
        /// <returns>The list of <see cref="PersonRestriction"></see> restrictions.</returns>
        /// <accessComments>
        /// API endpoint is secured so that only requestor can access data
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/staff/{staffId}/restrictions", 1, true, Name = "GetStaffRestrictions")]
        public async Task<ActionResult<IEnumerable<PersonRestriction>>> GetStaffRestrictions(string staffId)
        {
            if (string.IsNullOrEmpty(staffId))
            {
                _logger.LogError("Invalid staffId " + staffId);
                return CreateHttpResponseException("Invalid staffId " + staffId, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _staffService.GetStaffRestrictionsAsync(staffId));
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }
    }
}

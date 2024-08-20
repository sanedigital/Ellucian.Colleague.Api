// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base;
using System.Web;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Api;

namespace Ellucian.Colleague.Api.Controllers.Planning
{
    /// <summary>
    /// AdvisementsCompleteController
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Planning)]
    public class AdvisementsCompleteController : BaseCompressedApiController
    {
        private readonly IAdvisorService _advisorService;
        private readonly ILogger _logger;

        /// <summary>
        /// AdvisementsCompleteController constructor
        /// </summary>
        /// <param name="advisorService">Service of type <see cref="IAdvisorService">IAdvisorService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdvisementsCompleteController(IAdvisorService advisorService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _advisorService = advisorService;
            this._logger = logger;
        }

        /// <summary>
        /// Posts a <see cref="Dtos.Student.CompletedAdvisement">completed advisement</see>
        /// </summary>
        /// <param name="studentId">ID of the student whose advisement is being marked complete</param>
        /// <param name="completeAdvisement">A <see cref="Dtos.Student.CompletedAdvisement">completed advisement</see></param>
        /// <returns>An <see cref="Dtos.Planning.Advisee">advisee</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/students/{studentId}/completed-advisements", 1, true, Name = "PostAdvisementComplete")]
        public async Task<ActionResult<Advisee>> PostCompletedAdvisementAsync(string studentId, [FromBody]CompletedAdvisement completeAdvisement)
        {
            try
            {
                var privacyWrapper = await _advisorService.PostCompletedAdvisementAsync(studentId, completeAdvisement);
                var advisee = privacyWrapper.Dto as Advisee;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return advisee;
            }
            catch (PermissionsException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                string message = "Session has expired while posting completed advisement for student " + studentId;
                _logger.LogError(csee, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Controller for StudentDefaultAwardPeriod
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentDefaultAwardPeriodController : BaseCompressedApiController
    {
        private readonly IStudentDefaultAwardPeriodService StudentDefaultAwardPeriodService;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Dependency Injection constructor for StudentDefaultAwardPeriodController
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="studentDefaultAwardPeriodService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentDefaultAwardPeriodController(IAdapterRegistry adapterRegistry, IStudentDefaultAwardPeriodService studentDefaultAwardPeriodService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            StudentDefaultAwardPeriodService = studentDefaultAwardPeriodService;
            this.logger = logger;
        }
        /// <summary>
        /// Call service to get the default award periods
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">student id for whom to retrieve default award periods</param>
        /// <returns></returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/default-award-periods", 1, true, Name = "GetStudentDefaultAwardPeriods")]
        public async Task<ActionResult<IEnumerable<StudentDefaultAwardPeriod>>> GetStudentDefaultAwardPeriodsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await StudentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentDefaultAwardPeriod resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("StudentDefaultAwardPeriods", studentId);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting StudentDefaultAwardPeriod resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}

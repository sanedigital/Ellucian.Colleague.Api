// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// The StudentLoanLimitationsController exposes a student's loan limits, which describe the parameters within which a student can request changes to their loans.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentLoanLimitationsController : BaseCompressedApiController
    {
        private readonly IStudentLoanLimitationService StudentLoanLimitationService;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Dependency Injection Constructor for StudentLoanLimitationsController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="studentLoanLimitationService">StudentLoanLimitationService object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentLoanLimitationsController(IAdapterRegistry adapterRegistry, IStudentLoanLimitationService studentLoanLimitationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            StudentLoanLimitationService = studentLoanLimitationService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns a list of StudentLoanLimitation objects for all the years a student has award data.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">Student id for whom to retrieve the loan limitations</param>
        /// <returns>A list of StudentLoanLimitation objects for all the years a student has award data.</returns>       
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/loan-limits", 1, true, Name = "GetStudentLoanLimitations")]
        public async Task<ActionResult<IEnumerable<StudentLoanLimitation>>> GetStudentLoanLimitationsAsync(string studentId)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                var loanLimits = await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(studentId);
                stopWatch.Stop();
                logger.LogInformation(string.Format("Time elapsed to GetStudentLoanLimitations(controller): {0}", stopWatch.ElapsedMilliseconds));
                return Ok(loanLimits);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to loan limitations resource forbidden. See log for details", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find loan limitations resource. See log for details", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting loan limitations resource. See log for details", System.Net.HttpStatusCode.BadRequest);
            }            
        }
    }
}

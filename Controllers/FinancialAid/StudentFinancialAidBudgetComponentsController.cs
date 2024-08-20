// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
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
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes Financial Aid Budget Components assigned to students
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidBudgetComponentsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _AdapterRegistry;
        private readonly IStudentBudgetComponentService _StudentBudgetComponentService;
        private readonly ILogger _logger;

        /// <summary>
        /// StudentBudgetComponentsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentBudgetComponentService">StudentBudgetComponentService</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentFinancialAidBudgetComponentsController(IAdapterRegistry adapterRegistry, IStudentBudgetComponentService studentBudgetComponentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _AdapterRegistry = adapterRegistry;
            _StudentBudgetComponentService = studentBudgetComponentService;
            this._logger = logger;
        }

        /// <summary>
        /// Get a student's Financial Aid Budget Components for all award years.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get budget components</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of StudentBudgetComponent DTOs</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/financial-aid-budget-components", 1, true, Name = "GetStudentBudgetComponents")]
        public async Task<ActionResult<IEnumerable<StudentBudgetComponent>>> GetStudentFinancialAidBudgetComponentsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId argument is required");
            }

            try
            {
                return Ok(await _StudentBudgetComponentService.GetStudentBudgetComponentsAsync(studentId, getActiveYearsOnly));
            }
            catch (PermissionsException pex)
            {
                var message = string.Format("You do not have permission to get budget component resources for student {0}", studentId);
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                var message = "Unknown error occurred getting student budget components";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Get a student's Financial Aid Budget Components for the specified award year
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get budget components</param>
        /// <param name="awardYear">award year to retrieve budget components for</param>
        /// <returns>A list of StudentBudgetComponent DTOs</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/financial-aid-budget-components/{awardYear}", 1, true, Name = "GetStudentBudgetComponentsForYearAsync")]
        public async Task<ActionResult<IEnumerable<StudentBudgetComponent>>> GetStudentFinancialAidBudgetComponentsForYearAsync(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId argument is required");
            }

            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYear argument is required");
            }

            try
            {
                return Ok(await _StudentBudgetComponentService.GetStudentBudgetComponentsForYearAsync(studentId, awardYear));
            }
            catch (PermissionsException pex)
            {
                var message = string.Format("You do not have permission to get budget component resources for student {0}", studentId);
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                var message = "Unknown error occurred getting student budget components";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message);
            }
        }

    }
}

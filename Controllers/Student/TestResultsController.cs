// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Student Test Results data for Admissions, Placement and Other Tests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class TestResultsController : BaseCompressedApiController
    {
        private readonly ITestResultRepository _testResultRepository;
        private readonly ITestResultService _testResultService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the TestsResultsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="testResultService">Service for TestResult <see cref="ITestResultService">ITestResultService</see>/></param>
        /// <param name="testResultRepository">Repository of type <see cref="ITestResultRepository">ITestResultRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TestResultsController(IAdapterRegistry adapterRegistry,ITestResultService testResultService,
            ITestResultRepository testResultRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _testResultRepository = testResultRepository;
            _testResultService = testResultService;
            this._logger = logger;
        }

        /// <summary>
        /// Gets test results for a specific student.
        /// </summary>
        /// <param name="studentId">Student ID to retrieve test scores for</param>
        /// <param name="type">Type of test to select (admissions, placement, other).  If no type is provided all tests will be returned. </param>
        /// <returns>The <see cref="TestResult">Test Results</see> for the given student, limited to type requested.</returns>
        /// <accessComments>
        /// 1. User must be requesting their own data.
        /// 2. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        [Obsolete("Obsolete as of Api version 1.15, use version 2")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/test-results", 1, false, Name = "GetTestResults")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult>>> GetAsync(string studentId, string type = null)
        {
            try
            {
                return Ok(await _testResultService.GetAsync(studentId, type));
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Gets test results for a specific student.
        /// </summary>
        /// <param name="studentId">Student ID to retrieve test scores for</param>
        /// <param name="type">Type of test to select (admissions, placement, other).  If no type is provided all tests will be returned. </param>
        /// <returns>The <see cref="TestResult2">Test Results</see> for the given student, limited to type requested.</returns>
        /// <accessComments>
        /// 1. User must be requesting their own data.
        /// 2. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/test-results", 2, true, Name = "GetTestResults2")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult2>>> Get2Async(string studentId, string type = null)
        {
            try
            {
                return Ok(await _testResultService.Get2Async(studentId, type));
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Your previous session has expired and is no longer valid.";
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Gets all test results for a list of Student Ids
        /// </summary>
        /// <param name="criteria">DTO Object containing a list of student Ids and Type.</param>
        /// <returns>TestResults DTO Objects</returns>
        /// <accessComments>
        /// API endpoint is secured with VIEW.STUDENT.INFORMATION permission.
        /// </accessComments>
        [Obsolete("Obsolete as of Api version 1.15, use version 2")]
        [HttpPost]   
        [HttpPost]
        [HeaderVersionRoute("/qapi/test-results", 1, false, Name = "GetTestResultsByIdList")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult>>> QueryTestResultsAsync([FromBody] TestResultsQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            string type = criteria.Type;

            try
            {
                return Ok(await _testResultService.GetTestResultsByIdsAsync(studentIds.ToArray(), type));
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Gets all test results for a list of Student Ids
        /// </summary>
        /// <param name="criteria">DTO Object containing a list of student Ids and Type.</param>
        /// <returns>TestResults DTO Objects</returns>
        /// <accessComments>
        /// User with VIEW.STUDENT.INFORMATION permission can retrieve test results for the students. 
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/test-results", 2, true, Name = "GetTestResults2ByIdList")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult2>>> QueryTestResults2Async([FromBody] TestResultsQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            string type = criteria.Type;

            try
            {
                return Ok(await _testResultService.GetTestResults2ByIdsAsync(studentIds.ToArray(), type));
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }
    }
}

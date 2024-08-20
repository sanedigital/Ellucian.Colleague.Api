// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using System.Net;
using System.Net.Http;
using Ellucian.Web.Security;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Retention Alert related data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RetentionAlertController : BaseCompressedApiController
    {
        private readonly IRetentionAlertService _retentionAlertService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RetentionAlertController class.
        /// </summary>
        /// <param name="retentionAlertService">Service of type <see cref="IRetentionAlertService">IRetentionAlertService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor">Interface to action context accessor</param>
        /// <param name="apiSettings"></param>
        public RetentionAlertController(IRetentionAlertService retentionAlertService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _retentionAlertService = retentionAlertService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Case Types.
        /// </summary>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>All <see cref="CaseType">Case Types</see></returns>
        /// <note>AllCaseTypes is cached for 1) 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/case-types", 1, true, Name = "GetCaseTypes")]
        public async Task<ActionResult<IEnumerable<CaseType>>> GetCaseTypesAsync()
        {
            try
            {
                return Ok(await _retentionAlertService.GetCaseTypesAsync());
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving case types";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to retrieve case types", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all Case Categories.
        /// </summary>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>All <see cref="CaseCategory">Case Categories</see></returns>
        /// <note>AllCaseCategories is cached for  1) 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/case-categories", 1, true, Name = "GetCaseCategories")]
        public async Task<ActionResult<IEnumerable<CaseCategory>>> GetCaseCategoriesAsync()
        {
            try
            {
                return Ok(await _retentionAlertService.GetCaseCategoriesAsync());
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving case categories";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to retrieve case categories", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the Org Role settings for Case Categories.
        /// </summary>
        /// <param name="caseCategoryIds">Case Category Ids</param>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <returns>A list of RetentionAlertCaseCategoryOrgRoles</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/retention-alert-case-category-org-roles", 1, true, Name = "QueryRetentionAlertCaseCategoryOrgRoles")]
        public async Task<ActionResult<IEnumerable<RetentionAlertCaseCategoryOrgRoles>>> QueryRetentionAlertCaseCategoryOrgRolesAsync([FromBody]List<string> caseCategoryIds)
        {
            try
            {
                return Ok(await _retentionAlertService.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds));
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while getting case category org roles";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to retrieve Case category org roles");
                return CreateHttpResponseException("Unable to retrieve Case category org roles", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the retention alert permissions asynchronous.
        /// </summary>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns><see cref="RetentionAlertPermissions">Retention alert permissions for the current user</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/retention-alert/permissions", 1, true, Name = "GetRetentionAlertPermissions")]
        public async Task<ActionResult<RetentionAlertPermissions>> GetRetentionAlertPermissionsAsync()
        {
            try
            {
                return await _retentionAlertService.GetRetentionAlertPermissionsAsync();
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving retention alert permissions.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException("An error occurred while retrieving retention alert permissions.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all Case Closure reasons.
        /// </summary>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>All <see cref="CaseClosureReason">Case Categories</see></returns>
        /// <note>AllCaseClosureReasons is cached for  1) 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/case-closure-reasons", 1, true, Name = "GetCaseClosureReasons")]
        public async Task<ActionResult<IEnumerable<CaseClosureReason>>> GetCaseClosureReasonsAsync()
        {
            try
            {
                return Ok(await _retentionAlertService.GetCaseClosureReasonsAsync());
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving case closure reasons";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to retrieve case closure reasons", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves retention alert work cases
        /// </summary>
        /// <param name="retentionAlertQueryCriteria">Criteria to retrieve retention alert cases</param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>A list of <see cref="RetentionAlertWorkCase">RetentionAlertWorkCase</see> object</returns>
        /// <accessComments>
        /// An authenticated user may query retention alert cases if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/retention-alert-cases", 1, false, Name = "QueryRetentionAlertCasesByPost")]
        public async Task<ActionResult<IEnumerable<RetentionAlertWorkCase>>> QueryRetentionAlertWorkCasesByPostAsync([FromBody] RetentionAlertQueryCriteria retentionAlertQueryCriteria)
        {
            if (retentionAlertQueryCriteria == null)
            {
                string errorText = "Must provide the retentionAlertQueryCriteria item to query cases.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _retentionAlertService.GetRetentionAlertCasesAsync(retentionAlertQueryCriteria));
            }
            catch (PermissionsException ex)
            {
                var message = string.Format(ex.Message);
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to retrieve Retention Alert work cases");
                return CreateHttpResponseException("Unable to retrieve Retention Alert work cases", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves retention alert work cases 2 Async
        /// </summary>
        /// <param name="retentionAlertQueryCriteria">Criteria to retrieve retention alert cases</param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>A list of <see cref="RetentionAlertWorkCase2">RetentionAlertWorkCase2</see> object</returns>
        /// <accessComments>
        /// An authenticated user may query retention alert cases if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/retention-alert-cases", 2, true, Name = "QueryRetentionAlertCasesByPost2")]
        public async Task<ActionResult<IEnumerable<RetentionAlertWorkCase2>>> QueryRetentionAlertWorkCasesByPost2Async([FromBody] RetentionAlertQueryCriteria retentionAlertQueryCriteria)
        {
            if (retentionAlertQueryCriteria == null)
            {
                string errorText = "Must provide the retentionAlertQueryCriteria item to query cases.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _retentionAlertService.GetRetentionAlertCases2Async(retentionAlertQueryCriteria));
            }
            catch (PermissionsException ex)
            {
                var message = string.Format(ex.Message);
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving retention alert work cases 2";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to retrieve retention alert work cases 2";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Retrieves retention alert case Detail
        /// </summary>
        /// <param name="id">Retention alert case id to retrieve case detail</param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the case id is not provided to get the case detail.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert case detail.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>A list of <see cref="RetentionAlertCaseDetail">RetentionAlertCaseDetail</see> object</returns>
        /// <accessComments>
        /// An authenticated user may retrieve retention alert case detail if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// CONTRIBUTE.TO.CASES
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/retention-alert-case/{id}", 1, true, Name = "GetRetentionAlertCaseDetail")]
        public async Task<ActionResult<RetentionAlertCaseDetail>> GetRetentionAlertCaseDetailAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                string errorText = "Case id must be specified to get the case detail";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }

            try
            {
                return await _retentionAlertService.GetRetentionAlertCaseDetailAsync(id);
            }
            catch (PermissionsException ex)
            {
                var message = string.Format(ex.Message);
                _logger.LogError(ex, "User is not authorized to retrieve retention alert case detail");
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving alert case detail";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to retrieve retention alert case detail";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query retention alert contributions for the current user
        /// </summary>
        /// <param name="contributionsQueryCriteria">Query criteria</param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert contributions.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>A list of <see cref="RetentionAlertWorkCase">RetentionAlertWorkCase</see> object</returns>
        /// <accessComments>
        /// An authenticated user may retrieve retention alert contributions if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// CONTRIBUTE.TO.CASES
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/retention-alert-contributions", 1, true, Name = "QueryRetentionAlertContributions")]
        public async Task<ActionResult<IEnumerable<RetentionAlertWorkCase>>> QueryRetentionAlertContributionsAsync([FromBody] ContributionsQueryCriteria contributionsQueryCriteria)
        {
            if (contributionsQueryCriteria == null)
            {
                string errorText = "Must provide the contributionsQueryCriteria item to query contributions.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _retentionAlertService.GetRetentionAlertContributionsAsync(contributionsQueryCriteria));
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, "User is not authorized to get contributions");
                return CreateHttpResponseException("User is not authorized to get contributions", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving retention alert contributions";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Unable to retrieve retention alert contributions";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a retention alert case for student
        /// </summary>
        /// <param name="retentionAlertCase">Information for adding a case for student</param>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="RetentionAlertCaseCreateResponse">add retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may retrieve retention alert contributions if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// CONTRIBUTE.TO.CASES
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-case", 1, true, Name = "PostRetentionAlertCase")]
        public async Task<ActionResult<RetentionAlertCaseCreateResponse>> PostRetentionAlertCaseAsync([FromBody] RetentionAlertCase retentionAlertCase)
        {
            if (retentionAlertCase == null)
            {
                string errorText = "Must provide the retentionAlertCase item to add a new case for student.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertCaseCreateResponse newCaseResponse = await _retentionAlertService.AddRetentionAlertCaseAsync(retentionAlertCase);
                return Created(Url.Link("PostRetentionAlertCase", new { id = newCaseResponse.CaseId }), newCaseResponse);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while adding retention alert case for student";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to add retention alert case";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Add a note to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseNote">Information for adding a note to a case for student</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">add a note to a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may add a note to a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// CONTRIBUTE.TO.CASES
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-cases/{caseid}/case-history", 1, false, RouteConstants.EllucianRetentionAlertCaseNoteFormat, Name = "AddRetentionAlertCaseNote")]
        public async Task<ActionResult<RetentionAlertWorkCaseActionResponse>> AddRetentionAlertCaseNoteAsync([FromRoute] string caseId, [FromBody] RetentionAlertWorkCaseNote retentionAlertCaseNote)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to add a note to a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCaseNote == null)
            {
                string errorText = "Must provide the RetentionAlertWorkCaseNote item to add a note to a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.AddRetentionAlertCaseNoteAsync(caseId, retentionAlertCaseNote);
                return newCaseResponse;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while adding note to retention alert case";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to add note to retention alert case";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Add a FollowUp Note to a Retention Alert Case, this will not add the user to the list of Case Owners.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseNote">Information for adding a note to a case for student</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">add a note to a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may add a note to a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// CONTRIBUTE.TO.CASES
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-cases/{caseid}/case-history", 1, false, RouteConstants.EllucianRetentionAlertCaseFollowUpFormat, Name = "AddRetentionAlertCaseFollowUp")]
        public async Task<ActionResult<RetentionAlertWorkCaseActionResponse>> AddRetentionAlertCaseFollowUpAsync([FromRoute] string caseId, [FromBody] RetentionAlertWorkCaseNote retentionAlertCaseNote)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to add followup to a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCaseNote == null)
            {
                string errorText = "Must provide the RetentionAlertWorkCaseNote item to add followup to a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.AddRetentionAlertCaseFollowUpAsync(caseId, retentionAlertCaseNote);
                return newCaseResponse;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while adding followup to retention alert case";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to add followup to retention alert case";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Add a communication code to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseCommCode">Information for adding a communication code to a case for student</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">adding a communication code to a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may add a communication code to a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-cases/{caseid}/case-history", 1, false, RouteConstants.EllucianRetentionAlertCaseCommCodeFormat, Name = "AddRetentionAlertCaseCommCode")]
        public async Task<ActionResult<RetentionAlertWorkCaseActionResponse>> AddRetentionAlertCaseCommCodeAsync([FromRoute] string caseId, [FromBody] RetentionAlertWorkCaseCommCode retentionAlertCaseCommCode)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to add a communication code to a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCaseCommCode == null)
            {
                string errorText = "Must provide the retentionAlertWorkCaseAction item to add a communication code to a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.AddRetentionAlertCaseCommCodeAsync(caseId, retentionAlertCaseCommCode);
                return newCaseResponse;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while adding communication code to a retention alert case";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to add communication code to a retention alert case";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Add a Case Type to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseType">Information for adding a case type to a case for student</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">adding a case type to a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may add a case type to a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-cases/{caseid}/case-history", 1, false, RouteConstants.EllucianRetentionAlertCaseTypeFormat, Name = "AddRetentionAlertCaseType")]
        public async Task<ActionResult<RetentionAlertWorkCaseActionResponse>> AddRetentionAlertCaseTypeAsync([FromRoute] string caseId, [FromBody] RetentionAlertWorkCaseType retentionAlertCaseType)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to add a type to a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCaseType == null)
            {
                string errorText = "Must provide the retentionAlertWorkCaseAction item to add a case type to a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.AddRetentionAlertCaseTypeAsync(caseId, retentionAlertCaseType);
                return newCaseResponse;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while adding a case type to a retention alert case";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to add a case type to a retention alert case";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Change Retention Alert Case Priority
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCasePriority">Information for changing the priority of a retention alert case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">changing the priority of a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may change the priority of a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-cases/{caseid}/case-history", 1, false, RouteConstants.EllucianRetentionAlertCasePriorityFormat, Name = "ChangeRetentionAlertPriority")]
        public async Task<ActionResult<RetentionAlertWorkCaseActionResponse>> ChangeRetentionAlertCasePriorityAsync([FromRoute] string caseId, [FromBody] RetentionAlertWorkCasePriority retentionAlertCasePriority)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to change the priority of a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCasePriority == null)
            {
                string errorText = "Must provide the retentionAlertCasePriority item to change the priority of the retention alert case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.ChangeRetentionAlertCasePriorityAsync(caseId, retentionAlertCasePriority);
                return newCaseResponse;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while changing priority of the retention alert case";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to change priority of the retention alert case";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Close a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseClose">Information for closing a retention alert case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">closing a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may close a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-cases/{caseid}/case-history", 1, false, RouteConstants.EllucianRetentionAlertCaseCloseFormat, Name = "CloseRetentionAlertCase")]
        public async Task<ActionResult<RetentionAlertWorkCaseActionResponse>> CloseRetentionAlertCaseAsync([FromRoute] string caseId, [FromBody] RetentionAlertWorkCaseClose retentionAlertCaseClose)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to close a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCaseClose == null)
            {
                string errorText = "Must provide the retentionAlertWorkCaseAction item to close a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.CloseRetentionAlertCaseAsync(caseId, retentionAlertCaseClose);
                return newCaseResponse;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while closing a retention alert case";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to close a retention alert case";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates a retention alert case for student
        /// </summary>
        /// <param name="id">Id of the Retention alert case to update</param>
        /// <param name="retentionAlertCase">Information for updating a case</param>
        /// <returns><see cref="RetentionAlertCaseCreateResponse">Reponse of the case update</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        [HttpPut]
        [HeaderVersionRoute("/retention-alert-case/{id}", 1, true, Name = "PutRetentionAlertCase")]
        public async Task<ActionResult<RetentionAlertCaseCreateResponse>> PutRetentionAlertCaseAsync([FromRoute] string id, [FromBody] RetentionAlertCase retentionAlertCase)
        {
            if (string.IsNullOrEmpty(id))
            {
                string errorText = "Case id must be specified to update a case for student";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }

            if (retentionAlertCase == null)
            {
                string errorText = "Must provide the retentionAlertCase item to update case for student.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _retentionAlertService.UpdateRetentionAlertCaseAsync(id, retentionAlertCase);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to update retention alert case");
                return CreateHttpResponseException("Unable to update retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a list of cases for each Org Role and Org Entity owning cases for that category
        /// </summary>
        /// <param name="categoryId">Retention Alert Case Category Id</param>
        /// <returns>A list of cases for each Org Role and Org Entity owning cases for that category</returns>        
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may change the priority of a retention alert case if they have one of the following permissions:
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/retention-alert/case-owner-summary/{categoryId}", 1, true, Name = "GetRetentionAlertCaseOwnerSummary")]
        public async Task<ActionResult<RetentionAlertGroupOfCasesSummary>> GetRetentionAlertCaseOwnerSummaryAsync([FromRoute] string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                string errorText = "CategoryId must be specified to get a Category Summary.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _retentionAlertService.GetRetentionAlertCaseOwnerSummaryAsync(categoryId);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while fetching Category Summary for retention alert cases.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get Category Summary for retention alert cases.");
                return CreateHttpResponseException("Unable to get Category Summary for retention alert cases.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the case priorities
        /// </summary>
        /// <returns>All <see cref="CasePriority"/></returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>AllCasePriorities is cached for  1) 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/case-priorities", 1, true, Name = "GetCasePriorities")]
        public async Task<ActionResult<IEnumerable<CasePriority>>> GetCasePrioritiesAsync()
        {
            try
            {
                return Ok(await _retentionAlertService.GetCasePrioritiesAsync());
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving case priorities";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to retrieve case priorities";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Sends the retention alert mail for the work case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertWorkCaseSendMail">Information for sending the mails for the work case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">send mail for the retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may send mails for retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-cases/{caseid}/case-send-mail", 1, false, RouteConstants.EllucianRetentionAlertCaseSendMailFormat, Name = "SendRetentionAlertWorkCaseMail")]
        public async Task<ActionResult<RetentionAlertWorkCaseActionResponse>> SendRetentionAlertWorkCaseMailAsync([FromRoute] string caseId, [FromBody] RetentionAlertWorkCaseSendMail retentionAlertWorkCaseSendMail)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to send a mail.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertWorkCaseSendMail == null)
            {
                string errorText = "Must provide the retentionAlertWorkCaseSendMail item to send a mail.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.SendRetentionAlertWorkCaseMailAsync(caseId, retentionAlertWorkCaseSendMail);
                return newCaseResponse;
            }
            catch (PermissionsException pex)
            {                
                _logger.LogError(pex, "User is not able to send email for case.");
                return CreateHttpResponseException("User is not able to send email for case.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while sending mail for retention alert case";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to send mail for retention alert case";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Set a reminder for a retention alert case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="reminder">Information for setting a reminder for a retention alert case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">setting a reminder for the retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may set a reminder for retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-cases/{caseid}/case-history", 1, false, RouteConstants.EllucianRetentionAlertCaseSetReminderFormat, Name = "AddRetentionAlertCaseReminder")]
        public async Task<ActionResult<RetentionAlertWorkCaseActionResponse>> AddRetentionAlertCaseReminderAsync([FromRoute] string caseId, [FromBody] RetentionAlertWorkCaseSetReminder reminder)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to send a mail.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (reminder == null)
            {
                string errorText = "Must provide the reminder item to set a reminder.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse response = await _retentionAlertService.AddRetentionAlertCaseReminderAsync(caseId, reminder);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, "User is not able to set a reminder for case.");
                return CreateHttpResponseException("User is not able to set a reminder for case.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while setting reminder for retention alert case";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to set a reminder for retention alert case";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Set a reminder for a retention alert case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="reminders">Information for setting a reminder for a retention alert case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">setting a reminder for the retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may set a reminder for retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-cases/{caseid}/case-history", 1, false, RouteConstants.EllucianRetentionAlertCaseManageRemindersFormat, Name = "ManageRetentionAlertCaseReminders")]
        public async Task<ActionResult<RetentionAlertWorkCaseActionResponse>> ManageRetentionAlertCaseRemindersAsync([FromRoute] string caseId, [FromBody] RetentionAlertWorkCaseManageReminders reminders)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to send a mail.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (reminders == null)
            {
                string errorText = "Must provide the reminders to manage retention alert case reminders.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse response = await _retentionAlertService.ManageRetentionAlertCaseRemindersAsync(caseId, reminders);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, "User is not able to set a reminder for case.");
                return CreateHttpResponseException("User is not able to set a reminder for case.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while setting a reminder for retention alert case";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to set a reminder for retention alert case";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves retention alert open cases.
        /// </summary>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert open cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>An <see cref="RetentionAlertOpenCase">RetentionAlertOpenCase</see> object</returns>
        /// <accessComments>
        /// An authenticated user may retrieve retention alert open cases if they have one of the following permissions:
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/retention-alert/open-cases", 1, true, Name = "GetRetentionAlertOpenCases")]
        public async Task<ActionResult<IEnumerable<RetentionAlertOpenCase>>> GetRetentionAlertOpenCasesAsync()
        {
            try
            {
                return Ok(await _retentionAlertService.GetRetentionAlertOpenCasesAsync());
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, "User is not authorized to get open cases.");
                return CreateHttpResponseException("User is not authorized to get open cases.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while fetching open cases";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve retention alert open cases");
                return CreateHttpResponseException("Unable to retrieve retention alert open cases", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a list of closed retention alert cases grouped by closure reason.
        /// </summary>
        /// <param name="categoryId">Retention Alert Category Id</param>
        /// <returns>a list of closed retention alert cases grouped by closure reason.</returns>
        /// <accessComments>
        /// An authenticated user may retrieve retention alert open cases if they have one of the following permissions:
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/retention-alert/closed-cases-by-reason/{categoryId}", 1, true, Name = "GetRetentionAlertClosedCasesByReason")]
        public async Task<ActionResult<IEnumerable<RetentionAlertClosedCasesByReason>>> GetRetentionAlertClosedCasesByReasonAsync([FromRoute] string categoryId)
        {
            try
            {
                return Ok(await _retentionAlertService.GetRetentionAlertClosedCasesByReasonAsync(categoryId));
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, "User is not authorized to get cases closed by reason.");
                return CreateHttpResponseException("User is not authorized to get cases closed by reason.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while fetching retention alert closed cases by reason";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve retention alert closed cases by reason");
                return CreateHttpResponseException("Unable to retrieve retention alert closed cases by reason", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Reassigns the retention alert work case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertWorkCaseReassign">Information for reassigning the work case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating if there were any errors <see cref="RetentionAlertWorkCaseActionResponse">reassign the retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert open cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <accessComments>
        /// An authenticated user can reassign the retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-cases/{caseid}/case-history", 1, false, RouteConstants.EllucianRetentionAlertCaseReassignFormat, Name = "ReassignRetentionAlertWorkCase")]
        public async Task<ActionResult<RetentionAlertWorkCaseActionResponse>> ReassignRetentionAlertWorkCaseAsync([FromRoute] string caseId, [FromBody] RetentionAlertWorkCaseReassign retentionAlertWorkCaseReassign)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to reassign a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertWorkCaseReassign == null)
            {
                string errorText = "Must provide the retentionAlertWorkCaseReassign item to reassign a case.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse caseResponse = await _retentionAlertService.ReassignRetentionAlertWorkCaseAsync(caseId, retentionAlertWorkCaseReassign);
                return caseResponse;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, "User is not authorized to reassign case.");
                return CreateHttpResponseException("User is not authorized to reassign case.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while reassigning the retention alert case";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to reassign the retention alert case";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Set the Send Email Preference 
        /// </summary>
        /// <param name="orgEntityId">Org Entity ID</param>
        /// <param name="sendEmailPreference">Information for setting send email preference</param>
        /// <returns>An RetentionAlertSendEmailPreference <see cref="RetentionAlertSendEmailPreference">the send email preferences.</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert open cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <accessComments>
        /// An authenticated user can reassign the retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/retention-alert-case-worker/{orgEntityId}/email-preference", 1, true, Name = "SetRetentionAlertEmailPreference")]
        public async Task<ActionResult<RetentionAlertSendEmailPreference>> SetRetentionAlertEmailPreferenceAsync([FromRoute] string orgEntityId, [FromBody] RetentionAlertSendEmailPreference sendEmailPreference)
        {
            if (string.IsNullOrEmpty(orgEntityId))
            {
                string errorText = "Must provide the orgEntityId to set the send email preference.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (sendEmailPreference == null)
            {
                string errorText = "Must provide the sendEmailPreference to set the send email preference..";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertSendEmailPreference response = await _retentionAlertService.SetRetentionAlertEmailPreferenceAsync(orgEntityId, sendEmailPreference);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, "User is not authorized to set the Send Email Preference.");
                return CreateHttpResponseException("User is not authorized to set the Send Email Preference.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while setting the send email preference.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to set the send email preference.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the Send Email Preference 
        /// </summary>
        /// <param name="orgEntityId">Org Entity ID</param>
        /// <returns>An RetentionAlertSendEmailPreference <see cref="RetentionAlertSendEmailPreference">the send email preferences.</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert open cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <accessComments>
        /// An authenticated user can reassign the retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/retention-alert-case-worker/{orgEntityId}/email-preference", 1, true, Name = "GetRetentionAlertEmailPreference")]
        public async Task<ActionResult<RetentionAlertSendEmailPreference>> GetRetentionAlertEmailPreferenceAsync([FromRoute] string orgEntityId)
        {
            if (string.IsNullOrEmpty(orgEntityId))
            {
                string errorText = "Must provide the orgEntityId to set the send email preference.";
                _logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertSendEmailPreference response = await _retentionAlertService.GetRetentionAlertEmailPreferenceAsync(orgEntityId);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, "User is not authorized to get the Send Email Preference.");
                return CreateHttpResponseException("User is not authorized to get the Send Email Preference.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving send email preference.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                string message = "Unable to get the send email preference.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}

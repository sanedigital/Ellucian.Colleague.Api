// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Exceptions;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// The StudentAwardsController exposes a student's award data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAwardsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly IStudentAwardService StudentAwardService;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for the StudentAwardsController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="studentAwardService">StudentAwardService</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentAwardsController(IAdapterRegistry adapterRegistry, IStudentAwardService studentAwardService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            StudentAwardService = studentAwardService;
            this.logger = logger;
        }
        /// <summary>
        /// Get all of a student's awards for all years the student has award data in Colleague.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">The studentId for which to get the award data</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of StudentAward DTOs</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/awards", 1, true, Name = "GetAllStudentAwards")]
        public async Task<ActionResult<IEnumerable<StudentAward>>> GetStudentAwardsAsync([FromRoute]string studentId, [FromQuery]bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null");
            }

            try
            {
                return Ok(await StudentAwardService.GetStudentAwardsAsync(studentId, getActiveYearsOnly));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentAwards resource is forbidden. See log for more details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find StudentAwards resource. see log for more details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Operation is invalid based on state of StudentAward object. See log for more details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred while fetching StudentAwards resource. See log for more details", System.Net.HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Get all of a student's awards for the given year.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">The student id for which to get award data</param>
        /// <param name="year">The award year for which to get award data</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of StudentAward Dtos for the given year</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/awards/{year}", 1, true, Name = "GetStudentAwardsForYear")]
        public async Task<ActionResult<IEnumerable<StudentAward>>> GetStudentAwardsAsync([FromRoute]string studentId, [FromRoute] string year, [FromQuery] bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null");
            }
            if (string.IsNullOrEmpty(year))
            {
                return CreateHttpResponseException("year cannot be null");
            }

            try
            {
                return Ok(await StudentAwardService.GetStudentAwardsAsync(studentId, year, getActiveYearsOnly));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentAwards resource is forbidden. See log for more details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find StudentAwards resource. see log for more details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Operation is invalid based on state of StudentAward object. See log for more details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred while fetching StudentAwards resource. See log for more details", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a single StudentAward object based on the given student id, year and award id.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">The student id for which to get award data</param>
        /// <param name="year">The award year for which to get award data</param>
        /// <param name="awardId">The award id for which to get award data</param>
        /// <returns></returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/awards/{year}/{awardId}", 1, true, Name = "GetStudentAward")]
        public async Task<ActionResult<StudentAward>> GetStudentAwardAsync([FromRoute]string studentId, [FromRoute]string year, [FromRoute]string awardId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null");
            }
            if (string.IsNullOrEmpty(year))
            {
                return CreateHttpResponseException("year cannot be null");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                return CreateHttpResponseException("awardId cannot be null");
            }

            try
            {
                return await StudentAwardService.GetStudentAwardsAsync(studentId, year, awardId);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentAwards resource is forbidden. See log for more details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find StudentAwards resource. See log for more details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Operation is invalid based on state of StudentAward object. See log for more details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred while fetching StudentAwards resource. See log for more details", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update the awards contained in the StudentAwardPackage resource. The StudentAwards contained
        /// in the body of the request must match the resource identifiers in the URL. This performs an all or nothing update. 
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student to whom the award belongs</param>
        /// <param name="year">The AwardYear for which the studentAward applies</param>
        /// <param name="studentAwardPackage">The StudentAwardPackage containing StudentAward data to update</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A StudentAwardPackage containing the updated awards </returns>
        /// <exception cref="HttpResponseException">400 - Thrown if the update cannot complete successfully, usually because of bad or mismatched data in the request body</exception>
        /// <exception cref="HttpResponseException">403 - Thrown if the current user does not have permission to update the StudentAwardPackage resource</exception>
        /// <exception cref="HttpResponseException">404 - Thrown if any part of the StudentAwardPackage, award year, award, award period, etc. does not exist</exception>
        /// <exception cref="HttpResponseException">409 - Thrown if the any of the requested changes require review by a Financial Aid counselor or if a pending change request already exists for some or part of the package.
        /// Use the AwardPackageChangeRequest resource to submit change requests</exception>
        [HttpPut]
        [HeaderVersionRoute("/students/{studentId}/awards/{year}", 1, true, Name = "PutStudentAwardPacakge")]
        public async Task<ActionResult<StudentAwardPackage>> PutStudentAwardPackageAsync([FromRoute] string studentId, [FromRoute] string year, [FromBody] StudentAwardPackage studentAwardPackage, 
            [FromQuery]bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId in URI cannot be null or empty");
            }
            if (string.IsNullOrEmpty(year))
            {
                return CreateHttpResponseException("year in URI cannot be null or empty");
            }
            if (studentAwardPackage == null)
            {
                return CreateHttpResponseException("studentAwardPackage in body cannot be null or empty");
            }

            try
            {
                return new StudentAwardPackage()
                {
                    StudentAwards = await StudentAwardService.UpdateStudentAwardsAsync(studentId, year, studentAwardPackage.StudentAwards, getActiveYearsOnly)
                };
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee.Message);
                return CreateHttpResponseException(csee.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = "Access to StudentAwardPackage is forbidden. See log for more details.";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = "Part or all of the StudentAwardPackage resource does not exist. see log for more details.";
                logger.LogError(knfe, message);
                return CreateNotFoundException("StudentAwardPackage", string.Format("studentId: {0}, year: {1}", studentId, year));
            }
            catch (UpdateRequiresReviewException urre)
            {
                var message = "Some or all of the requested updates to StudentAwardPackage requires a review by a Financial Aid Counselor";
                logger.LogError(urre, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                var message = string.Format("Error occurred updating StudentAwardPackage resource: {0}", e.Message);
                logger.LogError(e, message);
                return CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Update a single student award resource. The StudentAward resource in the body of the request
        /// must match the resource identifiers in the URL
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student to whom the award belongs</param>
        /// <param name="year">The AwardYear for which the studentAward applies</param>
        /// <param name="awardId">The Id of the Award to which the studentAward applies</param>
        /// <param name="studentAward">The StudentAward containing data to update</param>
        /// <returns>An updated StudentAward</returns>
        /// <exception cref="HttpResponseException">400 - Thrown if the update cannot complete successfully, usually because of bad or mismatched data in the request body</exception>
        /// <exception cref="HttpResponseException">403 - Thrown if the current user does not have permission to update the StudentAward resource</exception>
        /// <exception cref="HttpResponseException">404 - Thrown if any part of the StudentAward, award year, award, award period, etc. does not exist</exception>
        /// <exception cref="HttpResponseException">409 - Thrown if the requested changes require review by a Financial Aid counselor. Use the AwardPackageChangeRequest endpoints to interact with change requests</exception>
        [HttpPut]
        [HeaderVersionRoute("/students/{studentId}/awards/{year}/{awardId}", 1, true, Name = "PutStudentAward")]
        public async Task<ActionResult<StudentAward>> PutStudentAwardAsync([FromRoute] string studentId, [FromRoute] string year, [FromRoute] string awardId, [FromBody] StudentAward studentAward)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId in URI cannot be null or empty");
            }
            if (string.IsNullOrEmpty(year))
            {
                return CreateHttpResponseException("year in URI cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                return CreateHttpResponseException("awardId in URI cannot be null or empty");
            }
            if (studentAward == null)
            {
                return CreateHttpResponseException("studentAward in body cannot be null or empty");
            }

            try
            {
                return await StudentAwardService.UpdateStudentAwardsAsync(studentId, year, awardId, studentAward);
            }
            catch (PermissionsException pe)
            {
                var message = "Access to StudentAward resource is forbidden. See log for more details.";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = "StudentAward resource does not exist. see log for more details.";
                logger.LogError(knfe, message);
                return CreateNotFoundException("StudentAward", string.Format("studentId: {0}, year: {1}, awardId: {2}", studentId, year, awardId));
            }
            catch (UpdateRequiresReviewException urre)
            {
                var message = "The requested updates to the StudentAward require a review by a Financial Aid Counselor";
                logger.LogError(urre, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                var message = string.Format("Error occurred updating StudentAward resource: {0}", e.Message);
                logger.LogError(e, message);
                return CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// "Obsolete as of version 1.7. Instead, use PUT students/{studentId}/awards/{year}"
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">The student id from the URI - All of the studentAward objects must match this student id</param>
        /// <param name="year">The award year from the URI - All of the studentAward objects must match this award year</param>
        /// <param name="studentAwards">The list studentAward objects for the given year to update</param>
        /// <returns>A list of updated studentAward objects for the given year</returns>
        [HttpPost]
        [Obsolete("Obsolete as of version 1.7. Instead, use PUT students/{studentId}/awards/{year}")]
        [HeaderVersionRoute("/students/{studentId}/awards/{year}", 1, true, Name = "PostStudentAwardsForYear")]
        public ActionResult<IEnumerable<StudentAward>> PostStudentAwards([FromRoute] string studentId, [FromRoute] string year, [FromBody]IEnumerable<StudentAward> studentAwards)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId in URI cannot be null or empty");
            }
            if (string.IsNullOrEmpty(year))
            {
                return CreateHttpResponseException("year in URI cannot be null or empty");
            }
            if (studentAwards == null)
            {
                return CreateHttpResponseException("studentAwards list in request body cannot be null");
            }

            try
            {
                return Ok(Task.Run(async() => await StudentAwardService.UpdateStudentAwardsAsync(studentId, year, studentAwards)).GetAwaiter().GetResult());
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentAwards resource is forbidden. See log for more details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find StudentAwards resource. see log for more details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Unable to perform requsted operation on StudentAwards resource. See log for more details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (OperationCanceledException oce)
            {
                logger.LogError(oce, oce.Message);
                return CreateHttpResponseException("Unable to update StudentAwards resource because of a conflicting edit on the server. See log for more details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred while fetching StudentAwards resource. See log for more details", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// "Obsolete as of version 1.7. Instead, use PUT students/{studentId}/awards/{year}/{awardId}"
        /// </summary>
        /// 
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">The student id - Must match the studentId in the StudentAward parameter object</param>
        /// <param name="year">The award year - Must match the AwardYear in the StudentAward parameter object</param>
        /// <param name="awardId">The award id - Must match the AwardId in the StudentAward parameter object</param>
        /// <param name="studentAward">The StudentAward object that will be used to update Colleague</param>
        /// <returns>A StudentAward object containing updated data from Colleague</returns>
        /// <exception cref="HttpResponseException">Thrown if studentAward is null</exception>
        /// <exception cref="HttpResponseException">Thrown if studentId, year or awardId args do not match the equivalent StudentAward's attributes</exception>
        [HttpPost]
        [Obsolete("Obsolete as of version 1.7. Instead, use PUT students/{studentId}/awards/{year}/{awardId}")]
        [HeaderVersionRoute("/students/{studentId}/awards/{year}/{awardId}", 1, true, Name = "PostStudentAward")]
        public ActionResult<StudentAward> PostStudentAward([FromRoute] string studentId, [FromRoute] string year, [FromRoute] string awardId, [FromBody]StudentAward studentAward)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId in URI cannot be null or empty");
            }
            if (string.IsNullOrEmpty(year))
            {
                return CreateHttpResponseException("year in URI cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                return CreateHttpResponseException("awardId in URI cannot be null or empty");
            }
            if (studentAward == null)
            {
                return CreateHttpResponseException("studentAward in request body cannot be null");
            }

            try
            {
                return Task.Run(async() => await StudentAwardService.UpdateStudentAwardsAsync(studentId, year, awardId, studentAward)).GetAwaiter().GetResult();
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentAwards resource is forbidden. See log for more details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find StudentAwards resource. see log for more details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Unable to perform requsted operation on StudentAwards resource. See log for more details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (OperationCanceledException oce)
            {
                logger.LogError(oce, oce.Message);
                return CreateHttpResponseException("Unable to update StudentAwards resource because of a conflicting edit on the server. See log for more details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred while fetching StudentAwards resource. See log for more details", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query all of a student's awards for the given year.
        /// </summary>
        /// <accessComment>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComment>
        /// <param name="criteria">DTO containing Student Ids, Award Year, and Term. StudentId is required, and either an award year or an award term is required.</param>
        /// <returns>A list of StudentAward Dtos for the given year</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/student-award-summary", 1, true, Name = "QueryStudentAwardSummary")]
        public ActionResult<IEnumerable<StudentAwardSummary>> QueryStudentAwardSummary([FromBody] StudentAwardSummaryQueryCriteria criteria)
        {
            if (criteria.StudentIds == null || criteria.StudentIds.Count() <= 0)
            {
                return CreateHttpResponseException("StudentIds cannot be null");
            }

            try
            {
                return Ok(StudentAwardService.QueryStudentAwardSummary(criteria));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentAwards resource is forbidden. See log for more details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message);
            }
        }
    }
}

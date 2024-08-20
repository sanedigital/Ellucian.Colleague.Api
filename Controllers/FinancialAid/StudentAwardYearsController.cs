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
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// The StudentAwardYearsController exposes a student's financial aid years
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAwardYearsController : BaseCompressedApiController
    {
        private readonly IStudentAwardYearService StudentAwardYearService;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Dependency Injection constructor for StudentAwardYearsController
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="studentAwardYearService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentAwardYearsController(IAdapterRegistry adapterRegistry, IStudentAwardYearService studentAwardYearService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            StudentAwardYearService = studentAwardYearService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all of a student's financial aid years.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have 
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">The Id of the student for whom to get award years</param>
        /// <returns>A list of StudentAwardYear objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        [Obsolete("Obsolete as of Api version 1.8, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-years", 1, false, Name = "GetStudentAwardYears")]
        public ActionResult<IEnumerable<StudentAwardYear>> GetStudentAwardYears(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(StudentAwardYearService.GetStudentAwardYears(studentId));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentAwardYears resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("StudentAwardYears", studentId);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting StudentAwardYears resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all of a student's financial aid years.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have 
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">The Id of the student for whom to get award years</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of StudentAwardYear2 objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-years", 2, true, Name = "GetStudentAwardYears2")]
        public async Task<ActionResult<IEnumerable<StudentAwardYear2>>> GetStudentAwardYears2Async(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await StudentAwardYearService.GetStudentAwardYears2Async(studentId, getActiveYearsOnly));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentAwardYears resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("StudentAwardYears", studentId);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting StudentAwardYears resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the specified financial aid award year for the student
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have 
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">student id for whom to get award year data</param>
        /// <param name="awardYear">award year code for which to retrieve award year data</param>
        /// <returns>StudentAwardYear object</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId or awardYearCode argument is null or empty</exception>
        [Obsolete("Obsolete as of Api version 1.8, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-years/{awardYear}", 1, false, Name = "GetStudentAwardYear")]
        public ActionResult<StudentAwardYear> GetStudentAwardYear(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYearCode cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(StudentAwardYearService.GetStudentAwardYear(studentId, awardYear));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentAwardYears resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("StudentAwardYears", studentId);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting StudentAwardYears resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the specified financial aid award year for the student
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have 
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">student id for whom to get award year data</param>
        /// <param name="awardYear">award year code for which to retrieve award year data</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>StudentAwardYear2 object</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId or awardYearCode argument is null or empty</exception>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-years/{awardYear}", 2, true, Name = "GetStudentAwardYear2")]
        [HeaderVersionRoute("/students-award-years/{studentId}/{awardYear}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetStudentAwardYear2", IsEthosEnabled = true, IsAdministrative = true, Order = -10000)]
        public async Task<ActionResult<StudentAwardYear2>> GetStudentAwardYear2Async(string studentId, string awardYear, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYearCode cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return await StudentAwardYearService.GetStudentAwardYear2Async(studentId, awardYear, getActiveYearsOnly);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to StudentAwardYears resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("StudentAwardYears", awardYear);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting StudentAwardYears resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates the student award year. Currently only the IsPaperCopyOptionSelected property is updated
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">student id</param>
        /// <param name="studentAwardYear">student award year carrying the info</param>
        /// <returns>student award year</returns>
        [Obsolete("Obsolete as of Api version 1.8, use version 2 of this API")]
        [HttpPut]
        [HeaderVersionRoute("/students/{studentId}/award-years", 1, false, Name = "UpdateStudentAwardYear")]
        public ActionResult<StudentAwardYear> UpdateStudentAwardYear([FromRoute] string studentId, [FromBody] StudentAwardYear studentAwardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (studentAwardYear == null)
            {
                return CreateHttpResponseException("studentAwardYear cannot be null");
            }

            try
            {
                return Ok(StudentAwardYearService.UpdateStudentAwardYear(studentAwardYear));
            }
            catch (ArgumentNullException ne)
            {
                logger.LogError(ne, ne.Message);
                return CreateHttpResponseException("studentAwardYear in request body is null. See log for details.");
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("studentAwardYear in request body contains invalid attribute values. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to student award letter resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (OperationCanceledException oce)
            {
                logger.LogError(oce, oce.Message);
                return CreateHttpResponseException("PaperCopyOptionFlag Update request was canceled because of a conflict on the server. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting StudentAwardYear resource. See log for details.");
            }
        }

        /// <summary>
        /// Updates the student award year. Currently only the IsPaperCopyOptionSelected property is updated
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">student id</param>
        /// <param name="studentAwardYear">student award year carrying the info</param>
        /// <returns>StudentAwardYear2 object</returns>
        [HttpPut]
        [HeaderVersionRoute("/students/{studentId}/award-years", 2, true, Name = "UpdateStudentAwardYear2")]
        public async Task<ActionResult<StudentAwardYear2>> UpdateStudentAwardYear2Async([FromRoute] string studentId, [FromBody] StudentAwardYear2 studentAwardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (studentAwardYear == null)
            {
                return CreateHttpResponseException("studentAwardYear cannot be null");
            }

            try
            {
                return await StudentAwardYearService.UpdateStudentAwardYear2Async(studentAwardYear);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ne)
            {
                logger.LogError(ne, ne.Message);
                return CreateHttpResponseException("studentAwardYear in request body is null. See log for details.");
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("studentAwardYear in request body contains invalid attribute values. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to student award letter resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (OperationCanceledException oce)
            {
                logger.LogError(oce, oce.Message);
                return CreateHttpResponseException("PaperCopyOptionFlag Update request was canceled because of a conflict on the server. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting StudentAwardYear resource. See log for details.");
            }
        }

    }
}

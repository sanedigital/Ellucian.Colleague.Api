// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
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


namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Controller exposes methods for interacting with Financial Aid Satisfactory Academic Progress (SAP)
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicProgressEvaluationsController : BaseCompressedApiController
    {
        private readonly IAcademicProgressService academicProgressService;
        private readonly ILogger logger;
        private readonly IAdapterRegistry adapterRegistry;

        /// <summary>
        /// Constructor for the AcademicProgressEvaluationsController
        /// </summary>
        /// <param name="academicProgressService"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AcademicProgressEvaluationsController(IAcademicProgressService academicProgressService, IAdapterRegistry adapterRegistry, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.academicProgressService = academicProgressService;
            this.logger = logger;
            this.adapterRegistry = adapterRegistry;
        }

        /// <summary>
        /// Get AcademicProgressEvaluation entities for the given student. 
        /// </summary>
        /// <param name="studentId">Colleague PERSON id of the student for whom to get AcademicProgressEvaluations</param>
        /// <returns>A list of AcademicProgressEvaluations</returns>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.14. Use GetStudentAcademicProgressEvaluations2Async")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/academic-progress-evaluations", 1, false, Name = "GetStudentAcademicProgressEvaluations")]
        public async Task<ActionResult<IEnumerable<AcademicProgressEvaluation>>> GetStudentAcademicProgressEvaluationsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return Ok(await academicProgressService.GetAcademicProgressEvaluationsAsync(studentId));
            }
            catch (PermissionsException pe)
            {

                logger.LogError(pe, "Current User does not have correct permissions");
                return CreateHttpResponseException("Access to this student's academic progress evaluations is forbidden", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown Exception occurred while getting student academic progress evaluations");
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Get AcademicProgressEvaluation2 DTOs for the given student. 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student for whom to get AcademicProgressEvaluations</param>
        /// <returns>A list of AcademicProgressEvaluations</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/academic-progress-evaluations", 2, true, Name = "GetStudentAcademicProgressEvaluations2")]
        public async Task<ActionResult<IEnumerable<AcademicProgressEvaluation2>>> GetStudentAcademicProgressEvaluations2Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            try
            {
                return Ok(await academicProgressService.GetAcademicProgressEvaluations2Async(studentId));
            }
            catch (PermissionsException pe)
            {

                logger.LogError(pe, "Current User does not have correct permissions");
                return CreateHttpResponseException("Access to this student's academic progress evaluations is forbidden", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown Exception occurred while getting student academic progress evaluations");
                return CreateHttpResponseException(e.Message);
            }
        }
    }
}

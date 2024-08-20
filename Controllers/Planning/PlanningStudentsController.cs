// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Planning
{
    /// <summary>
    /// Supplements the Students controller in the student module namespace
    /// with functionality that is only available from within the planning module namespace.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Planning)]
    public class PlanningStudentsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IProgramEvaluationService _programEvaluationService;
        private readonly IPlanningStudentService _planningStudentService;
        private readonly ILogger _logger;

        /// <summary>
        /// PlanningStudentsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="programEvaluationService">Program Evaluation Service of type <see cref="IProgramEvaluationService">IProgramEvaluationService</see></param>
        /// <param name="planningStudentService">Planning Student Service</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PlanningStudentsController(IAdapterRegistry adapterRegistry, IProgramEvaluationService programEvaluationService,
            IPlanningStudentService planningStudentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _programEvaluationService = programEvaluationService;
            _planningStudentService = planningStudentService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves the program evaluation results for the student's specified program.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="program">The student's program code</param>
        /// <returns>The <see cref="ProgramEvaluation">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [Obsolete("Obsolete as of Colleague API 1.11, use GetEvaluation2Async instead.")]
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "program" })]
        [HttpGet]
        [HeaderVersionRoute("/students/{id}/evaluation/{program}", 1, true, Name = "ProgramEvaluation")]
        [HeaderVersionRoute("/students/{id}/evaluation", 1, false, Name = "ProgramEvaluation2")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Planning.ProgramEvaluation>> GetEvaluationAsync(string id, string program)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntity = (await _programEvaluationService.EvaluateAsync(id, new List<string>() { program }, null)).First();

                // Get the adapter
                var programEvaluationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Ellucian.Colleague.Dtos.Planning.ProgramEvaluation>();

                // use adapter to map data to DTO
                var evaluation = programEvaluationDtoAdapter.MapToType(programEvaluationEntity);
                return evaluation;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results of the given student against a list of programs.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="programCodes">The list of programs to evaluate</param>
        /// <returns>The <see cref="ProgramEvaluation">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a program evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) may retrieve any proram evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/students/{id}/evaluation", 1, false, Name = "QueryEvaluations")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Planning.ProgramEvaluation>>> QueryEvaluationsAsync(string id, [FromBody] List<string> programCodes)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntities = await _programEvaluationService.EvaluateAsync(id, programCodes, null);

                // Get the adapter
                var programEvaluationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Ellucian.Colleague.Dtos.Planning.ProgramEvaluation>();

                // use adapter to map data to DTO
                var evaluations = new List<ProgramEvaluation>();
                foreach (var evaluation in programEvaluationEntities)
                {
                    evaluations.Add(programEvaluationDtoAdapter.MapToType(evaluation));
                }
                return evaluations;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the notice for the specified student and program. Student does not have to be currently enrolled in the program.
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        /// <param name="programCode">The program code</param>
        /// <returns>List of <see cref="EvaluationNotice">Evaluation Notices</see></returns>
        /// <accessComments>
        /// Evaluation Notices can only be retrieved if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve any Evaluation Notices from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Evaluation Notices if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 3. User with VIEW.STUDENT.INFORMATION permission
        /// 4. User is a proxy user
        /// </accessComments>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "programCode" })]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/evaluation-notices/{programCode}", 1, true, Name = "GetEvaluationNotices")]
        public async Task<ActionResult<IEnumerable<Dtos.Student.EvaluationNotice>>> GetEvaluationNoticesAsync(string studentId, string programCode)
        {
            try
            {
                var notices = await _programEvaluationService.GetEvaluationNoticesAsync(studentId, programCode);
                return Ok(notices);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving evaluation notices for the student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a planning student Dtos containing fewer properties than student Dtos for the specified list of student ids
        /// </summary>
        /// <param name="criteria">Planning Student Criteria</param>
        /// <returns>List of <see cref="PlanningStudent">Planning Students</see></returns>
        /// <accessComments>
        /// Student information can be retrieved only if:
        /// 1. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 2. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 3. A user with permissions of VIEW.PERSON.INFORMATION and VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// 
        /// Student privacy is enforced by this response. If any student has an assigned privacy code that the requestor is not authorized to access, 
        /// the response object is returned with an X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of students. 
        /// In this situation, all details except the student name are cleared from the specific student object.        
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/students", 1, false, "application/vnd.ellucian-planning-student.v{0}+json", Name = "QueryPlanningStudents")]
        public async Task<ActionResult<IEnumerable<PlanningStudent>>> QueryPlanningStudentsAsync([FromBody] PlanningStudentCriteria criteria)
        {
            _logger.LogInformation("Entering QueryPlanningStudentsAsync");
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                //this will call planning student service with ienumerable of student ids
                var privacyWrapper = await _planningStudentService.QueryPlanningStudentsAsync(criteria.StudentIds);
                var planningStudents = privacyWrapper.Dto as List<PlanningStudent>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                watch.Stop();
                _logger.LogInformation("QueryPlanningStudentsAsync... completed in " + watch.ElapsedMilliseconds.ToString());

                return Ok((IEnumerable<PlanningStudent>)planningStudents);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                string message = "Session has expired while retrieving planning student";
                _logger.LogError(csee, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results for the student's specified program.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="program">The student's program code</param>
        /// <returns>The <see cref="ProgramEvaluation2">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [Obsolete("Obsolete as of Colleague API 1.13, use GetEvaluation3Async instead.")]
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "program" })]
        [HttpGet]
        [HeaderVersionRoute("/students/{id}/evaluation", 2, false, Name = "ProgramEvaluation3")]
        public async Task<ActionResult<Dtos.Planning.ProgramEvaluation2>> GetEvaluation2Async(string id, string program)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntity = (await _programEvaluationService.EvaluateAsync(id, new List<string>() { program }, null)).First();

                // Get the adapter
                var programEvaluation2DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation2>();

                // use adapter to map data to DTO
                var evaluation = programEvaluation2DtoAdapter.MapToType(programEvaluationEntity);
                return evaluation;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results of the given student against a list of programs.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="programCodes">The list of programs to evaluate</param>
        /// <returns>The <see cref="ProgramEvaluation2">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague API 1.13, use QueryEvaluations3Async instead.")]
        [HttpPost]
        [HeaderVersionRoute("/qapi/students/{id}/evaluation", 2, false, Name = "QueryEvaluations2")]
        public async Task<ActionResult<IEnumerable<Dtos.Planning.ProgramEvaluation2>>> QueryEvaluations2Async(string id, [FromBody] List<string> programCodes)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntities = await _programEvaluationService.EvaluateAsync(id, programCodes, null);

                // Get the adapter
                var programEvaluation2DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation2>();

                // use adapter to map data to DTO
                var evaluations = new List<ProgramEvaluation2>();
                foreach (var evaluation in programEvaluationEntities)
                {
                    evaluations.Add(programEvaluation2DtoAdapter.MapToType(evaluation));
                }
                return evaluations;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Retrieves the program evaluation results for the student's specified program.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="program">The student's program code</param>
        /// <param name="catalogYear">The catalogYear code for the program</param>
        /// <returns>The <see cref="ProgramEvaluation3">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [Obsolete("Obsolete as of Colleague API 1.33, use QueryEvaluations4Async instead.")]
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "program" })]
        [HttpGet]
        [HeaderVersionRoute("/students/{id}/evaluation", 3, false, Name = "ProgramEvaluation4")]
        public async Task<ActionResult<Dtos.Planning.ProgramEvaluation3>> GetEvaluation3Async(string id, string program, string catalogYear = null)
        {
            try
            {
                Domain.Student.Entities.ProgramEvaluation programEvaluationEntity;
                // Call the coordination-layer evaluation service
                if (string.IsNullOrEmpty(catalogYear)) { catalogYear = null; }

                programEvaluationEntity = (await _programEvaluationService.EvaluateAsync(id, new List<string>() { program }, catalogYear)).First();

                // Get the adapter
                var programEvaluation3DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>();

                // use adapter to map data to DTO
                var evaluation = programEvaluation3DtoAdapter.MapToType(programEvaluationEntity);
                return evaluation;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results for the student's specified program.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="program">The student's program code</param>
        /// <param name="catalogYear">The catalogYear code for the program</param>
        /// <returns>The <see cref="ProgramEvaluation3">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "program" })]
        [HttpGet]
        [HeaderVersionRoute("/students/{id}/evaluation", 4, false, Name = "ProgramEvaluation5")]
        [Obsolete("Obsolete as of Colleague API 2.2, use GetEvaluation5Async instead.")]

        public async Task<ActionResult<Dtos.Planning.ProgramEvaluation4>> GetEvaluation4Async(string id, string program, string catalogYear = null)
        {
            try
            {
                Domain.Student.Entities.ProgramEvaluation programEvaluationEntity;
                // Call the coordination-layer evaluation service
                if (string.IsNullOrEmpty(catalogYear)) { catalogYear = null; }

                programEvaluationEntity = (await _programEvaluationService.EvaluateAsync(id, new List<string>() { program }, catalogYear)).First();

                // Get the adapter
                var programEvaluation4DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>();

                // use adapter to map data to DTO
                var evaluation = programEvaluation4DtoAdapter.MapToType(programEvaluationEntity);
                return evaluation;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving program evaluation for the student {0}", id);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results for the student's specified program.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="program">The student's program code</param>
        /// <param name="catalogYear">The catalogYear code for the program</param>
        /// <returns>The <see cref="ProgramEvaluation3">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        /// 
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "program" })]
        [HttpGet]
        [HeaderVersionRoute("/students/{id}/evaluation", 5, true, Name = "ProgramEvaluation5")]

        public async Task<ActionResult<Dtos.Planning.ProgramEvaluation5>> GetEvaluation5Async(string id, string program, string catalogYear = null)
        {
            try
            {
                Domain.Student.Entities.ProgramEvaluation programEvaluationEntity;
                // Call the coordination-layer evaluation service
                if (string.IsNullOrEmpty(catalogYear)) { catalogYear = null; }

                programEvaluationEntity = (await _programEvaluationService.EvaluateAsync(id, new List<string>() { program }, catalogYear)).First();

                // Get the adapter
                var programEvaluation4DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation5>();

                // use adapter to map data to DTO
                var evaluation = programEvaluation4DtoAdapter.MapToType(programEvaluationEntity);
                return evaluation;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving program evaluation for the student {0}", id);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Retrieves the program evaluation results of the given student against a list of programs.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="programCodes">The list of programs to evaluate</param>
        /// <returns>The <see cref="ProgramEvaluation3">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague API 1.33, use QueryEvaluations4Async instead.")]
        [HttpPost]
        [HeaderVersionRoute("/qapi/students/{id}/evaluation", 3, false, Name = "QueryEvaluations3")]
        public async Task<ActionResult<IEnumerable<Dtos.Planning.ProgramEvaluation3>>> QueryEvaluations3Async(string id, [FromBody] List<string> programCodes)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntities = await _programEvaluationService.EvaluateAsync(id, programCodes, null);

                // Get the adapter
                var programEvaluation3DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>();

                // use adapter to map data to DTO
                var evaluations = new List<ProgramEvaluation3>();
                foreach (var evaluation in programEvaluationEntities)
                {
                    evaluations.Add(programEvaluation3DtoAdapter.MapToType(evaluation));
                }
                return evaluations;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results of the given student against a list of programs.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="programCodes">The list of programs to evaluate</param>
        /// <returns>The <see cref="ProgramEvaluation4">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/students/{id}/evaluation", 4, false, Name = "QueryEvaluations4")]
        public async Task<ActionResult<IEnumerable<Dtos.Planning.ProgramEvaluation4>>> QueryEvaluations4Async(string id, [FromBody] List<string> programCodes)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntities = await _programEvaluationService.EvaluateAsync(id, programCodes, null);

                // Get the adapter
                var programEvaluation4DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>();

                // use adapter to map data to DTO
                var evaluations = new List<ProgramEvaluation4>();
                foreach (var evaluation in programEvaluationEntities)
                {
                    evaluations.Add(programEvaluation4DtoAdapter.MapToType(evaluation));
                }
                return evaluations;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results of the given student against a list of programs.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="programCodes">The list of programs to evaluate</param>
        /// <returns>The <see cref="ProgramEvaluation4">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/students/{id}/evaluation", 5, true, Name = "QueryEvaluations5")]
        public async Task<ActionResult<IEnumerable<Dtos.Planning.ProgramEvaluation5>>> QueryEvaluations5Async(string id, [FromBody] List<string> programCodes)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntities = await _programEvaluationService.EvaluateAsync(id, programCodes, null);

                // Get the adapter
                var programEvaluation5DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation5>();

                // use adapter to map data to DTO
                var evaluations = new List<ProgramEvaluation5>();
                foreach (var evaluation in programEvaluationEntities)
                {
                    evaluations.Add(programEvaluation5DtoAdapter.MapToType(evaluation));
                }
                return evaluations;
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.Message);
                return CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}


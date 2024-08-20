// Copyright 2022-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;


namespace Ellucian.Colleague.Api.Controllers.Planning
{
    /// <summary>
    /// This functionality is for Applicants that is only available from within the planning module namespace.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Planning)]
    public class PlanningApplicantsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IProgramEvaluationService _programEvaluationService;
        private readonly IAcademicHistoryService _academicHistoryService;
        private readonly IStudentProgramService _studentProgramService;
        private readonly ILogger _logger;

        /// <summary>
        /// PlanningApplicantsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="programEvaluationService">Program Evaluation Service of type <see cref="IProgramEvaluationService">IProgramEvaluationService</see></param>
        /// <param name="academicHistoryService">Academic History Service</param>
        /// <param name="studentProgramService">Student Program Service</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PlanningApplicantsController(IAdapterRegistry adapterRegistry, IProgramEvaluationService programEvaluationService,
            IAcademicHistoryService academicHistoryService, IStudentProgramService studentProgramService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _programEvaluationService = programEvaluationService;
            _academicHistoryService = academicHistoryService;
            _studentProgramService = studentProgramService;
            this._logger = logger;
        }


        /// <summary>
        /// Retrieves the program evaluation results of the given applicant against a program.
        /// </summary>
        /// <param name="id">The applicant's ID</param>
        /// <param name="program">The program to evaluate</param>
        /// <param name="catalogYear">Catalog year</param>
        /// <returns>The <see cref="ProgramEvaluation4">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. An applicant is accessing its own data
        /// 2. A user is an applicant
        /// 3. A user should have a permission of EVALUATE.WHAT.IF 
        /// </accessComments>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "program" })]
        [HttpGet]
        [HeaderVersionRoute("/applicants/{id}/evaluation", 1, false, Name = "GetApplicantProgramEvaluation")]
        [Obsolete("Obsolete as of Colleague API 2.2, use GetApplicantProgramEvaluation2Async instead.")]
        public async Task<ActionResult<Dtos.Planning.ProgramEvaluation4>> GetApplicantProgramEvaluationAsync(string id, string program, string catalogYear = null)
        {
            Dtos.Planning.ProgramEvaluation4 evaluation = new Dtos.Planning.ProgramEvaluation4();
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "Applicant id must be provided to run evaluation");
                }
                if (string.IsNullOrEmpty(program))
                {
                    throw new ArgumentNullException("programCode", "Program code must be provided to run evaluation.");
                }
                // Call the coordination-layer evaluation service
                var programEvaluationEntity = (await _programEvaluationService.EvaluateApplicantAsync(id, new List<string>() { program }, catalogYear)).FirstOrDefault();

                // Get the adapter
                var programEvaluation4DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>();

                // use adapter to map data to DTO
                if (programEvaluationEntity != null)
                {
                    evaluation = programEvaluation4DtoAdapter.MapToType(programEvaluationEntity);
                }
                return evaluation;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while running evaluation for an applicant";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException ex)
            {
                string message = "User is not an applicant.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pex)
            {
                string message = "Applicant does not have permissions to run the evaluation.";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                string message = "An exception occurred while running evaluation for an applicant.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Retrieves the program evaluation results of the given applicant against a program.
        /// </summary>
        /// <param name="id">The applicant's ID</param>
        /// <param name="program">The program to evaluate</param>
        /// <param name="catalogYear">Catalog year</param>
        /// <returns>The <see cref="ProgramEvaluation5">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. An applicant is accessing its own data
        /// 2. A user is an applicant
        /// 3. A user should have a permission of EVALUATE.WHAT.IF 
        /// </accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "program" })]
        [HeaderVersionRoute("/applicants/{id}/evaluation", 2, true, Name = "GetApplicantProgramEvaluation2")]
        public async Task<ActionResult<Dtos.Planning.ProgramEvaluation5>> GetApplicantProgramEvaluation2Async(string id, string program, string catalogYear = null)
        {
            Dtos.Planning.ProgramEvaluation5 evaluation = new Dtos.Planning.ProgramEvaluation5();
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "Applicant id must be provided to run evaluation");
                }
                if (string.IsNullOrEmpty(program))
                {
                    throw new ArgumentNullException("programCode", "Program code must be provided to run evaluation.");
                }
                // Call the coordination-layer evaluation service
                var programEvaluationEntity = (await _programEvaluationService.EvaluateApplicantAsync(id, new List<string>() { program }, catalogYear)).FirstOrDefault();

                // Get the adapter
                var programEvaluation5DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation5>();

                // use adapter to map data to DTO
                if (programEvaluationEntity != null)
                {
                    evaluation = programEvaluation5DtoAdapter.MapToType(programEvaluationEntity);
                }
                return evaluation;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while running evaluation for an applicant";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException ex)
            {
                string message = "User is not an applicant.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pex)
            {
                string message = "Applicant does not have permissions to run the evaluation.";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                string message = "An exception occurred while running evaluation for an applicant.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Retrieves the program evaluation results of the given applicant against a list of programs.
        /// </summary>
        /// <param name="id">The applicant's ID</param>
        /// <param name="programCodes">The list of programs to evaluate</param>
        /// <returns>The <see cref="ProgramEvaluation4">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. An applicant is accessing its own data
        /// 2. A user is an applicant
        /// 3. A user should have permission of EVALUATE.WHAT.IF 
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/applicants/{id}/evaluation", 1, false, Name = "QueryApplicantEvaluations")]
        [Obsolete("Obsolete as of Colleague API 2.2, use QueryApplicantEvaluations2Async instead.")]

        public async Task<ActionResult<IEnumerable<Dtos.Planning.ProgramEvaluation4>>> QueryApplicantEvaluationsAsync(string id, [FromBody] List<string> programCodes)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "Applicant id must be provided to run evaluation");
                }
                if (programCodes == null || !programCodes.Any())
                {
                    throw new ArgumentNullException("programCodes", "At least one program code must be provided to run evaluation.");
                }
                // Call the coordination-layer evaluation service
                var programEvaluationEntities = await _programEvaluationService.EvaluateApplicantAsync(id, programCodes, null);
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
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while running evaluation for an applicant";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException ex)
            {
                string message = "User is not an applicant.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pex)
            {
                string message = "Applicant does not have permissions to run the evaluation.";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                string message = "An exception occurred while running evaluation for an applicant.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results of the given applicant against a list of programs.
        /// </summary>
        /// <param name="id">The applicant's ID</param>
        /// <param name="programCodes">The list of programs to evaluate</param>
        /// <returns>The <see cref="ProgramEvaluation5">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. An applicant is accessing its own data
        /// 2. A user is an applicant
        /// 3. A user should have permission of EVALUATE.WHAT.IF 
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/applicants/{id}/evaluation", 2, true, Name = "QueryApplicantEvaluations2")]

        public async Task<ActionResult<IEnumerable<Dtos.Planning.ProgramEvaluation5>>> QueryApplicantEvaluations2Async(string id, [FromBody] List<string> programCodes)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "Applicant id must be provided to run evaluation");
                }
                if (programCodes == null || !programCodes.Any())
                {
                    throw new ArgumentNullException("programCodes", "At least one program code must be provided to run evaluation.");
                }
                // Call the coordination-layer evaluation service
                var programEvaluationEntities = await _programEvaluationService.EvaluateApplicantAsync(id, programCodes, null);
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
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while running evaluation for an applicant";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException ex)
            {
                string message = "User is not an applicant.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pex)
            {
                string message = "Applicant does not have permissions to run the evaluation.";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                string message = "An exception occurred while running evaluation for an applicant.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }



        /// <summary>
        /// Retrieves the academic history for the applciant. 
        /// This retrieves all the raw academic credits which includes:
        /// Academic credits that were imported without aplicant being registered as student.
        /// Academic credits that were transfer or non-course credits.
        /// Academic Credits that were for the applicant when it was a student but later again became an applicant.
        /// </summary>
        /// <param name="applicantId">Id of the applicant</param>
        /// <param name="filterCredits">(Optional) used to filter to credits with status of Add, New, Preliminary, Non-Course, Transferm Withdrawn, dropped only.</param>
        /// <param name="includeDrops">(Optional) if filter is true and include dropped is also true then dropped academic credits will also be returned</param>
        /// <returns>The list of <see cref="ApplicantAcademicCredit">Academic Credits</see> for the applicant.</returns>
        /// <accessComments>
        /// Applicant academic history can be retrieved by:
        /// 1. An Applicant is accessing its own data
        /// 2. A user must be an applicant.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/applicants/{applicantId}/academic-credits", 1, true, Name = "GetApplicantAcademicCredits")]
        public async Task<ActionResult<IEnumerable<ApplicantAcademicCredit>>> GetApplicantAcademicCreditsAsync(string applicantId, bool filterCredits = true, bool includeDrops = false)
        {
            if (string.IsNullOrEmpty(applicantId))
            {
                throw new ArgumentNullException("applicantId", "applicantId must be provided in order to retrieve applicant's academic history");
            }
            try
            {
                List<ApplicantAcademicCredit> academicCredits = (await _academicHistoryService.GetApplicantAcademicCreditsAsync(applicantId, filterCredits, includeDrops)).ToList();
                return academicCredits;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving academic history for applicant {0}", applicantId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException ex)
            {
                string message = "User is not an applicant.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pex)
            {
                string message = "User must be self to retrieve applicant's academic credits";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                string message = "An exception occurred while retrieving academic credits for an applicant: " + applicantId;
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets information the programs in which the specified applicant has applied for.
        ///  Inactive programs are student programs with the status of Graduated, Withdrawn or InActive.
        /// includeInactivePrograms flag works in conjunction with currentOnly flag such as:
        /// When includeInactivePrograms is set to true and currentOnly flag is true then only those inactive programs that are not yet ended will be included whereas if currentOnly flag is false then all inactive programs will be included.
        /// When includeInactivePrograms is set to false but currentOnly is true then it means only those inactive programs that were ended in past, will be included.
        /// </summary>
        /// <param name="applicantId">Applicant's ID</param>
        /// <param name="includeInactivePrograms">Should inactive student programs be included (programs with status of Inactive/ withdrawn or STPR.END.DATE is in past)</param>
        /// <param name="currentOnly">Boolean to indicate whether this request is for active student programs, or ended/past programs as well </param>
        /// <returns>All <see cref="ApplicantStudentProgram">Programs</see> in which the specified applicant filled the application for.</returns>
        /// <accessComments>
        /// Applicant programs information can be retrieved only if:
        /// 1. An applicant is accessing its own data.
        /// 2. A user is an applicant.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/applicants/{applicantId}/programs", 1, true, Name = "GetApplicantPrograms")]
        public async Task<ActionResult<IEnumerable<ApplicantStudentProgram>>> GetApplicantProgramsAsync(string applicantId, bool includeInactivePrograms = false, bool currentOnly = true)
        {
            List<ApplicantStudentProgram> studentProgramDtos = new List<ApplicantStudentProgram>();
            if (string.IsNullOrEmpty(applicantId))
            {
                throw new ArgumentNullException("applicantId", "applicantId must be provided in order to retrieve student's programs");
            }
            try
            {

                studentProgramDtos = (await _studentProgramService.GetApplicantProgramsAsync(applicantId, includeInactivePrograms, currentOnly)).ToList();

                return studentProgramDtos;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving programs for applicant {0}", applicantId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException ex)
            {
                string message = "User is not an applicant.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pex)
            {
                string message = "User must be self to retrieve applicant's programs";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                string message = "An exception occurred while retrieving programs for applicant: " + applicantId;
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

        }


    }
}


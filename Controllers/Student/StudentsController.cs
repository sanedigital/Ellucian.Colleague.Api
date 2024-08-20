// Copyright 2012-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Colleague.Dtos.Student.Transcripts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using SectionRegistration = Ellucian.Colleague.Dtos.Student.SectionRegistration;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Accesses Student data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentsController : BaseCompressedApiController
    {
        private readonly IEmergencyInformationService _emergencyInformationService;
        private readonly IAcademicHistoryService _academicHistoryService;
        private readonly IStudentService _studentService;
        private readonly IStudentProgramRepository _studentProgramRepository;
        private readonly IRequirementRepository _requirementRepository;
        private readonly IStudentRestrictionService _studentRestrictionService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment webHostEnvironment;

        /// <summary>
        /// Initializes a new instance of the StudentsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="academicHistoryService">Service of type <see cref="IAcademicHistoryService">IAcademicHistoryService</see></param>
        /// <param name="studentService">Service of type <see cref="IStudentService">IStudentService</see></param>
        /// <param name="studentProgramRepository">Repository of type <see cref="IStudentProgramRepository">IStudentProgramRepository</see></param>
        /// <param name="studentRestrictionService">Service of type <see cref="IStudentRestrictionService">IStudentRestrictionService</see></param>
        /// <param name="requirementRepository">Repository of type <see cref="IRequirementRepository">IRequirementRepository</see></param>
        /// <param name="emergencyInformationService">Service of type <see cref="IEmergencyInformationService">IEmergencyInformationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="apiSettings"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="webHostEnvironment"></param>
        public StudentsController(IAdapterRegistry adapterRegistry, IAcademicHistoryService academicHistoryService,
                                  IStudentService studentService, IStudentProgramRepository studentProgramRepository,
                                  IStudentRestrictionService studentRestrictionService,
                                  IRequirementRepository requirementRepository,
                                  IEmergencyInformationService emergencyInformationService,
                                  ILogger logger,
                                  ApiSettings apiSettings,
                                  IActionContextAccessor actionContextAccessor,
                                  IWebHostEnvironment webHostEnvironment) : base(actionContextAccessor, apiSettings)
        {
            _academicHistoryService = academicHistoryService;
            _studentService = studentService;
            _studentProgramRepository = studentProgramRepository;
            _studentRestrictionService = studentRestrictionService;
            _requirementRepository = requirementRepository;
            _adapterRegistry = adapterRegistry;
            _emergencyInformationService = emergencyInformationService;
            this._logger = logger;
            this.webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Action to get Students from a list of Ids.
        /// Marital status can be null.
        /// Filter out student advisements which ended today or earlier.
        /// </summary>
        /// <param name="criteria">Query criteria for retrieving students.</param>
        /// <returns>StudentBatch3 DTO Objects</returns>
        /// <accessComments>
        /// Authenticated users with the VIEW.PERSON.INFORMATION and VIEW.STUDENT.INFORMATION permission can query students.
        /// 
        /// Student privacy is enforced by this response. If any student has an assigned privacy code that the requestor is not authorized to access, 
        /// the response object is returned with an X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of students. 
        /// In this situation, all details except the student name are cleared from the specific student object.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/students", 4, false, "application/vnd.ellucian-batch.v4+json", Name = "QueryStudentsById4")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentBatch3>>> QueryStudentsById4Async([FromBody] StudentQueryCriteria criteria)
        {
            _logger.LogInformation("Entering QueryStudentsById4Async");
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                // The service to execute this search is in StudentService.
                var privacyWrapper = await _studentService.QueryStudentsById4Async(criteria.StudentIds, false, criteria.GetDegreePlan, criteria.Term);
                var students = privacyWrapper.Dto as List<Dtos.Student.StudentBatch3>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                watch.Stop();
                _logger.LogInformation("QueryStudentsById4Async... completed in " + watch.ElapsedMilliseconds.ToString());

                return Ok((IEnumerable<Dtos.Student.StudentBatch3>)students);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex, pex.Message);
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving students from a list of Ids";
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Gets list of students keys for a given term only.  Other parameters are ignored.
        /// </summary>
        /// <param name="studentQuery">Query parameter object</param>
        /// <returns>List of students for a term.  Only the termId parameter is used at this time.<see cref="Student">Student IDs</see></returns>
        /// <accessComments>
        /// User with permission of VIEW.STUDENT.INFORMATION can retrieve student Ids for given term.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/query-student-ids", 1, true, Name = "PostStudentIds")]
        public async Task<ActionResult<IEnumerable<string>>> PostStudentIdsAsync([FromBody] StudentQuery studentQuery)
        {
            try
            {
                return Ok(await _studentService.SearchIdsAsync(studentQuery.termId));
            }
            catch (PermissionsException pex)  // Not logged in or didn't have right permissions
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)              // Something bad happened
            {
                _logger.LogError(e.Message);
                return CreateHttpResponseException("An error occurred during search: " + e.Message, HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Gets information the programs in which the specified student is enrolled.
        /// </summary>
        /// <param name="studentId">Student's ID</param>
        /// <param name="currentOnly">Boolean to indicate whether this request is for active student programs, or ended/past programs as well</param>
        /// <returns>All <see cref="StudentProgram">Programs</see> in which the specified student is enrolled.</returns>
        /// <accessComments>
        /// Student information can be retrieved only when:
        /// 1. A Student is accessing its own data.
        /// 2. Proxy user is accessing the student's data.
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// </accessComments>
        /// <note>Student Program is cached for 5 minutes.</note>
        [Obsolete("Obsolete as of Api version 1.10, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/programs", 1, false, Name = "GetStudentPrograms")]
        public async Task<ActionResult<IEnumerable<StudentProgram>>> GetStudentProgramsAsync(string studentId, bool currentOnly = true)
        {
            try
            {
                await _studentService.CheckStudentAccessAsync(studentId);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                return CreateNotFoundException("student", studentId);
            }

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentProgram> studentPrograms = await _studentProgramRepository.GetAsync(studentId);

            // Limit set of student programs to current programs if requested
            if (currentOnly == true)
            {
                studentPrograms = studentPrograms.Where(x => x.EndDate == null || x.EndDate >= DateTime.Today);
            }

            List<StudentProgram> studentProgramDtos = new List<StudentProgram>();

            if (studentPrograms.Count() > 0)
            {
                var studentProgramDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, StudentProgram>();
                var requirementDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement, Requirement>();
                foreach (var prog in studentPrograms)
                {
                    var studentProgramDto = studentProgramDtoAdapter.MapToType(prog);
                    foreach (var additionalReq in studentProgramDto.AdditionalRequirements)
                    {
                        if (!String.IsNullOrEmpty(additionalReq.RequirementCode))
                        {
                            additionalReq.Requirement = requirementDtoAdapter.MapToType((await _requirementRepository.GetAsync(additionalReq.RequirementCode)));
                        }
                    }
                    studentProgramDtos.Add(studentProgramDto);
                }
            }
            return studentProgramDtos;
        }

        /// <summary>
        /// Retrieves the academic history for the student. This groups the information on a term by term basis (separating out the non-term classes).
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <returns>The <see cref="AcademicHistory">Academic History</see> for the student.</returns>
        /// <accessComments>
        /// Student academic history can be retrieved by:
        /// 1. A Student is accessing its own data
        /// 2. An Advisor with any of the following permissions is accessing any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 3. An Advisor with any of the following permissions is accessing one of his or her assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 4. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// </accessComments>
        [Obsolete("Obsolete as of Api version 1.5, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/academic-credits", 1, false, Name = "GetAcademicHistory")]
        public async Task<ActionResult<AcademicHistory>> GetAcademicHistoryAsync(string studentId, bool bestFit = false, bool filter = true, string term = null)
        {
            try
            {
                return await _academicHistoryService.GetAcademicHistoryAsync(studentId, bestFit, filter, term);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                return CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves the academic history for the student. This groups the information on a term by term basis (separating out the non-term classes).
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <returns>The <see cref="AcademicHistory2">Academic History</see> for the student.</returns>
        /// <accessComments>
        /// Student academic history can be retrieved by:
        /// 1. A Student is accessing its own data
        /// 2. An Advisor with any of the following permissions is accessing any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 3. An Advisor with any of the following permissions is accessing one of his or her assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 4. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.11, use GetAcademicHistory3Async instead")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/academic-credits", 2, false, Name = "GetAcademicHistory2")]
        public async Task<ActionResult<AcademicHistory2>> GetAcademicHistory2Async(string studentId, bool bestFit = false, bool filter = true, string term = null)
        {
            try
            {
                return await _academicHistoryService.GetAcademicHistory2Async(studentId, bestFit, filter, term);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                return CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves the academic history for the student. This groups the information on a term by term basis (separating out the non-term classes).
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <returns>The <see cref="AcademicHistory3">Academic History</see> for the student.</returns>
        /// <accessComments>
        /// Student academic history can be retrieved by:
        /// 1. A Student is accessing its own data,
        /// 2. A proxy is accessing data for allowed student,
        /// 3. An Advisor with any of the following permissions is accessing any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following permissions is accessing one of his or her assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.18, use GetAcademicHistory4Async instead")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/academic-credits", 3, false, Name = "GetAcademicHistory3")]
        public async Task<ActionResult<AcademicHistory3>> GetAcademicHistory3Async(string studentId, bool bestFit = false, bool filter = true, string term = null)
        {
            try
            {
                return await _academicHistoryService.GetAcademicHistory3Async(studentId, bestFit, filter, term);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                return CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves the academic history for the student. This groups the information on a term by term basis (separating out the non-term classes).
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <param name="includeDrops">(Optional) used to include dropped academic credits</param>
        /// <returns>The <see cref="AcademicHistory4">Academic History</see> for the student.</returns>
        /// <accessComments>
        /// Student academic history can be retrieved by:
        /// 1. A Student is accessing its own data,
        /// 2. A proxy is accessing data for allowed student,
        /// 3. An Advisor with any of the following permissions is accessing any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following permissions is accessing one of his or her assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.30, use GetAcademicHistory5Async instead")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/academic-credits", 4, false, Name = "GetAcademicHistory4")]
        public async Task<ActionResult<AcademicHistory4>> GetAcademicHistory4Async(string studentId, bool bestFit = false, bool filter = true, string term = null, bool includeDrops = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "studentId must be provided in order to retrieve student's academic history");
            }
            try
            {
                return await _academicHistoryService.GetAcademicHistory4Async(studentId, bestFit, filter, term, includeDrops);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving academic history for student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                return CreateNotFoundException("student", studentId);
            }
        }


        /// <summary>
        /// Retrieves the academic history for the student. 
        /// This retrieves all the raw academic credits which includes:
        /// Academic credits that were imported without student being registered to the section.
        /// Academic credits that were transfer, dropped, withdrawn or non-course credits based upon filter and includeDrop parameters.
        /// This groups the information on a term by term basis (separating out the non-term classes).
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <param name="includeDrops">(Optional) used to include dropped academic credits</param>
        /// <param name="retrieveGradeRestrictions">(Optional) Default is true. If false then student's grade view restrictions will not be retrieved and returned with academic credits</param>
        /// <param name="byPassGradeRestrictionsCache">(Optional) This is to retrieve grade restrictions from cache or make a CTX call to get real time restrictions. Default is true which means cache will be bypassed hence CTX will be called</param>
        /// <returns>The <see cref="AcademicHistory4">Academic History</see> for the student.</returns>
        /// <accessComments>
        /// Student academic history can be retrieved by:
        /// 1. A Student is accessing its own data,
        /// 2. A proxy is accessing data for allowed student,
        /// 3. An Advisor with any of the following permissions is accessing any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following permissions is accessing one of his or her assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/academic-credits", 5, true, Name = "GetAcademicHistory5")]
        public async Task<ActionResult<AcademicHistory4>> GetAcademicHistory5Async(string studentId, bool bestFit = false, bool filter = true, string term = null, bool includeDrops = false, bool retrieveGradeRestrictions = true, bool byPassGradeRestrictionsCache = true)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "studentId must be provided in order to retrieve student's academic history");
            }
            try
            {
                return await _academicHistoryService.GetAcademicHistory5Async(studentId, bestFit, filter, term, includeDrops, retrieveGradeRestrictions, byPassGradeRestrictionsCache);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving academic history version 5 for student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string message = "Either User is not self or does not have appropriate permissions to retrieve student's academic history";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                string message = "An exception occurred while retrieving academic history for student: " + studentId;
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Retrieves the Student Restrictions for the provided student. Obsolete as of 1.11 - use GetStudentRestrictionsAsync2
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>All  <see cref="PersonRestriction">Student Restrictions</see> for the provided student.</returns>
        /// <accessComments>
        /// Student restrictions can be retrieved by:
        /// 1. A Student is accessing its own data,
        /// 2. A proxy is accessing data for allowed student,
        /// 3. An Advisor with any of the following permissions is accessing any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following permissions is accessing one of his or her assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.11, use GetStudentRestrictionsAsync2")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/restrictions", 1, false, Name = "GetStudentRestrictions")]
        public async Task<ActionResult<IEnumerable<PersonRestriction>>> GetStudentRestrictionsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Unable to get student restrictions. Invalid studentId " + studentId);
                return CreateHttpResponseException("Unable to get student restrictions. Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _studentRestrictionService.GetStudentRestrictionsAsync(studentId, false));
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the Student Restrictions for the provided student.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>All  <see cref="PersonRestriction">Student Restrictions</see> for the provided student.</returns>
        /// <accessComments>
        /// Student restrictions can be retrieved by:
        /// 1. A Student is accessing its own data,
        /// 2. A proxy is accessing data for allowed student and has 1 or more proxy permissions,
        /// 3. An Advisor with any of the following permissions is accessing any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following permissions is accessing one of his or her assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.16, use GetStudentRestrictions3Async")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/restrictions", 2, false, Name = "GetStudentRestrictions2")]
        public async Task<ActionResult<IEnumerable<PersonRestriction>>> GetStudentRestrictionsAsync2(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Unable to get student restrictions. Invalid studentId " + studentId);
                return CreateHttpResponseException("Unable to get student restrictions. Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                bool useCache = true;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        useCache = false;
                    }
                }

                return Ok(await _studentRestrictionService.GetStudentRestrictionsAsync(studentId, useCache));
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException("Unable to process student restrictions", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the Student Restrictions for the provided student.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>All  <see cref="PersonRestriction">Student Restrictions</see> for the provided student.</returns>
        /// <accessComments>
        /// Student restrictions can be retrieved by:
        /// 1. A Student is accessing its own data,
        /// 2. A person who has been granted Core Notification workflow proxy access for the student,
        /// 3. A user with permission of VIEW.PERSON.RESTRICTIONS is accessing the student's data.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/restrictions", 3, true, Name = "GetStudentRestrictions3")]
        public async Task<ActionResult<IEnumerable<PersonRestriction>>> GetStudentRestrictions3Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Unable to get student restrictions. Invalid studentId " + studentId);
                return CreateHttpResponseException("Unable to get student restrictions. Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                bool useCache = true;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        useCache = false;
                    }
                }

                return Ok(await _studentRestrictionService.GetStudentRestrictions2Async(studentId, useCache));
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException("Unable to process student restrictions", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the Student Restrictions for the provided list of students or list of Restriction keys.
        /// </summary>
        /// <param name="criteria">DTO object which contains Student keys or Restriction keys for selection</param>
        /// <returns>Returns a list of <see cref="PersonRestriction">PersonRestriction</see> DTO objects for the provided list of students or restrictions.</returns>    
        /// <accessComments>
        /// Only users with VIEW.STUDENT.INFORMATION permission can retrieve student restrictions for the provided list of students or list of restriction keys.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/student-restrictions", 1, true, Name = "QueryStudentRestrictions")]
        public async Task<ActionResult<IEnumerable<PersonRestriction>>> PostStudentRestrictionsQuery([FromBody] StudentRestrictionsQueryCriteria criteria)
        {
            try
            {
                if (criteria.Ids != null && criteria.Ids.Count() > 0)
                {
                    return Ok(await _studentRestrictionService.GetStudentRestrictionsByIdsAsync(criteria.Ids));
                }
                else
                {
                    return Ok(await _studentRestrictionService.GetStudentRestrictionsByStudentIdsAsync(criteria.StudentIds));
                }
            }
            catch (PermissionsException pex)  // Not logged in or didn't have right permissions
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)              // Something bad happened
            {
                _logger.LogError(e.Message);
                return CreateHttpResponseException("An error occurred during search: " + e.Message, HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Check to see if the user is eligible to register for the provided term
        /// </summary>
        /// <param name="id">The user's ID</param>
        /// <returns>A list of <see cref="RegistrationMessage"/>Registration Messages.</returns>
        /// <accessComments>
        /// A person may retrieve their own registration eligibility.
        /// A proxy may retrieve data for allowed student.
        /// An authenticated user (advisor) with any of the following permission codes may retrieve registration eligibility for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) with any of the following permission codes may retrieve registration eligibility for any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.3, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/students/{id}/registration-eligibility", 1, false, Name = "GetRegistrationEligibility")]
        public async Task<ActionResult<IEnumerable<RegistrationMessage>>> GetRegistrationEligibilityAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogError("Invalid id");
                return CreateHttpResponseException("Invalid id", HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _studentService.CheckRegistrationEligibilityAsync(id));
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Checks to see if the student is eligible to register.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns><see cref="RegistrationEligibility">Registration Eligibility </see> information containing messages, which, if present, indicate the student
        /// is ineligible, in addition to a boolean HasOverride, set to true if the current user has the ability to override ineligibility.</returns>
        /// <accessComments>
        /// A person may retrieve their own registration eligibility.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve registration eligibility for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve registration eligibility for any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.34, use version 3 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/registration-eligibility", 2, false, Name = "GetRegistrationEligibility2")]
        public async Task<ActionResult<Dtos.Student.RegistrationEligibility>> GetRegistrationEligibility2Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Invalid studentId");
                return CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentService.CheckRegistrationEligibility2Async(studentId);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Timeout have occurred while retrieving registration eligibility for the student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Checks to see if the student is eligible to register.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns><see cref="RegistrationEligibility">Registration Eligibility </see> information containing messages, which, if present, indicate the student
        /// is ineligible, in addition to a boolean HasOverride, set to true if the current user has the ability to override ineligibility.</returns>
        /// <accessComments>
        /// A person may retrieve their own registration eligibility.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve registration eligibility for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve registration eligibility for any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/registration-eligibility", 3, true, Name = "GetRegistrationEligibility3")]
        public async Task<ActionResult<Dtos.Student.RegistrationEligibility>> GetRegistrationEligibility3Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Invalid studentId");
                return CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _studentService.CheckRegistrationEligibility3Async(studentId));
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Timeout has occurred while retrieving registration eligibility for the student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("An error occurred during request processing: " + e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retreive student's registration priority information.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns><see cref="RegistrationPriority">RegistrationPriority</see> registration priority information specific to a student</returns>
        /// <accessComments>
        /// A person may retrieve their own registration priorities.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve registration priorities for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve registration priorities for any student
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/registration-priorities", 1, true, Name = "GetRegistrationPrioritiesAsync")]
        public async Task<ActionResult<IEnumerable<Dtos.Student.RegistrationPriority>>> GetRegistrationPrioritiesAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Invalid studentId parameter while retrieving student's registration priorities");
                return CreateHttpResponseException("The studentId is required to retrieve student's registration priorities.", HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await _studentService.GetRegistrationPrioritiesAsync(studentId));
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Timeout has occurred while retrieving registration priorities for the student: " + studentId;
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                string message = "Either user is not self or does not have appropriate permissions to retrieving registration priorities for the student: " + studentId;
                _logger.LogError(peex, message);
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = "An exception occurred while retrieving registration priorities for the student: " + studentId;
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the transcript restrictions for the provided student.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>Information used to determine if a student should be prevented from seeing or requesting their transcript.</returns>
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
        [HeaderVersionRoute("/students/{studentId}/transcript-restrictions", 2, true, Name = "GetTranscriptRestrictions2")]
        public async Task<ActionResult<Dtos.Student.TranscriptAccess>> GetTranscriptRestrictions2Async(string studentId)
        {
            try
            {
                //var sectionPermission = await _service.GetAsync(sectionId);
                //return sectionPermission;
                var transcriptAccessDto = await _studentService.GetTranscriptRestrictions2Async(studentId);
                return Ok(transcriptAccessDto);
            }
            catch (KeyNotFoundException)
            {
                // Student not found.  Error already logged in repository
                return CreateNotFoundException("student", studentId);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving transcript restrictions for a student";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the transcript restrictions for the provided student.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>All transcript restrictions for the provided student.</returns>
        /// <accessComments>
        /// User must be requesting their own data, or requesting information for advisee with adequate permissions
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/transcript-restrictions", 1, false, Name = "GetTranscriptRestrictions")]
        public async Task<ActionResult<IEnumerable<Dtos.Student.TranscriptRestriction>>> GetTranscriptRestrictionsAsync(string studentId)
        {
            try
            {
                IEnumerable<Domain.Student.Entities.TranscriptRestriction> restrictionsDomain = await _studentService.GetTranscriptRestrictionsAsync(studentId);
                List<Dtos.Student.TranscriptRestriction> restrictionsDto = new List<Dtos.Student.TranscriptRestriction>();
                if (restrictionsDomain.Count() > 0)
                {
                    var restrictionAdapter = new AutoMapperAdapter<Domain.Student.Entities.TranscriptRestriction, Dtos.Student.TranscriptRestriction>(_adapterRegistry, _logger);
                    foreach (var rest in restrictionsDomain)
                    {
                        restrictionsDto.Add(restrictionAdapter.MapToType(rest));
                    }
                }
                return restrictionsDto;
            }
            catch (KeyNotFoundException)
            {
                // Student not found.  Error already logged in repository
                return CreateNotFoundException("student", studentId);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves the ungraded Terms for the provided student.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>All ungraded  <see cref="Term">Terms</see> for the student.</returns>
        ///  <accessComments>
        /// Ungraded terms for a student can be retrieved only if:
        /// 1. A Student is accessing its own data.
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/ungraded-terms", 1, true, Name = "GetStudentUngradedTerms")]
        public async Task<ActionResult<IEnumerable<Term>>> GetUngradedTermsAsync(string studentId)
        {
            try
            {
                return Ok(await _studentService.GetUngradedTermsAsync(studentId));
            }
            catch (KeyNotFoundException)
            {
                // Student not found.  Error already logged in repository
                return CreateNotFoundException("student", studentId);
            }
            catch (PermissionsException)
            {
                return CreateHttpResponseException("User does not have permission to view student", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves Students, including references to the student's Degree Plan, Programs and Restrictions and some demographic information, for those students who match the provided query parameters. At a minimum, Date of Birth and Last Name are required parameters.
        /// </summary>
        /// <param name="studentQuery">Query parameter object</param>
        /// <returns>All <see cref="Student">Students</see> who matched the query.</returns>
        /// <accessComments>
        /// Only users with permission VIEW.ANY.ADVISEE can perform search on students.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/students-query/", 1, true, Name = "PostSearchStudent")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.Student>>> PostSearchStudentAsync([FromBody] StudentQuery studentQuery)
        {
            if (studentQuery.dateOfBirth == null || string.IsNullOrEmpty(studentQuery.lastName))
            {
                return CreateHttpResponseException("This search requires last name and date of birth", HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await _studentService.SearchAsync(studentQuery.lastName, studentQuery.dateOfBirth, studentQuery.firstName, studentQuery.formerName, studentQuery.studentId, studentQuery.governmentId));
            }
            catch (PermissionsException pex)  // Not logged in or didn't have right permissions
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException ae)   // Nothing matched
            {
                _logger.LogError(ae.Message);
                return new List<Ellucian.Colleague.Dtos.Student.Student>();
            }
            catch (Exception e)              // Something bad happened
            {
                _logger.LogError(e.Message);
                return CreateHttpResponseException("An error occurred during search: " + e.Message, HttpStatusCode.NotFound);
            }


        }
        /// <summary>
        /// Accepts a transcript order and enters it into Colleague.  
        /// </summary>
        /// <param name="transcriptRequest">PESC XML Transcript Request</param>
        /// <returns>HTTP 201 if successful; in the body, the status of the request, and the date, if any, of expected future processing.</returns>
        /// <accessComments>
        /// Users must have the VIEW.ANY.ADVISEE permission to create transcript orders
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/transcript-orders/", 1, true, Name = "PostTranscriptOrder")]
        [Consumes("application/xml")] // Consume XML for PESC XML Transcript requests
        public async Task<ActionResult<TranscriptResponse>> PostTranscriptOrderAsync(TranscriptRequest transcriptRequest)
        {

            string dataresponse = null;

            if (transcriptRequest == null || transcriptRequest.TransmissionData == null)
            {
                return CreateHttpResponseException("The XML request was not understood", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(transcriptRequest.TransmissionData.RequestTrackingID))
            {
                return CreateHttpResponseException("The XML request was missing element TranscriptRequest:TransmissionData:RequestTrackingID", HttpStatusCode.BadRequest);
            }
            if (transcriptRequest.TransmissionData.RequestTrackingID.Length > 35)
            {
                return CreateHttpResponseException("TranscriptRequest:TransmissionData:RequestTrackingID cannot be over 35 bytes", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(transcriptRequest.TransmissionData.Source.Organization.DUNS))
            {
                return CreateHttpResponseException("The XML request was missing element TranscriptRequest:TransmissionData:Source.Organization.DUNS", HttpStatusCode.BadRequest);
            }

            try
            {

                dataresponse = await _studentService.OrderTranscriptAsync(transcriptRequest);
            }
            catch (PermissionsException pex)  // Not logged in or didn't have right permissions
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return CreateHttpResponseException("An error occurred during request processing: " + e.Message, HttpStatusCode.BadRequest);
            }

            string orderId = transcriptRequest.TransmissionData.RequestTrackingID;

            TranscriptResponse jsonResponseContainer = new TranscriptResponse() { ResponseData = dataresponse };

            try
            {
                var uri = new UriBuilder(Request.Scheme, Request.Host.Host, Request.Host.Port.HasValue ? (int)Request.Host.Port : -1, $"{Request.PathBase}{Request.Path}/{orderId}").ToString();
                return Created(uri, jsonResponseContainer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetBaseException(), $"Parts: {Request.Scheme ?? "No scheme"}, {Request.Host.Host}, {(Request.Host.Port.HasValue ? Request.Host.Port.Value.ToString() : "no port")}, {Request.PathBase}, {Request.Path}, {orderId ?? "no order id"}");
                throw ex;
            }
        }

        /// <summary>
        /// gets the current status of a transcript order
        /// </summary>
        /// <param name="orderId">Third-party-generated order ID</param>
        /// <param name="currentStatusCode">The cloud's current understanding of the order's status</param>
        /// <returns>Base-64 encoded PESC XML Transcript Response</returns>
        /// <accessComments>
        /// Users must have the VIEW.ANY.ADVISEE permission to check transcript order statuses
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/transcript-orders/{orderId}/{currentStatusCode}", 1, true, Name = "GetTranscriptOrderStatus")]
        public async Task<ActionResult<TranscriptResponse>> GetTranscriptOrderStatusAsync(string orderId, string currentStatusCode)
        {

            string dataresponse = null;

            if (string.IsNullOrEmpty(orderId))
            {
                return CreateHttpResponseException("Request missing orderId", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(currentStatusCode))
            {
                return CreateHttpResponseException("Request missing currentStatusCode", HttpStatusCode.BadRequest);
            }
            try
            {
                dataresponse = await _studentService.CheckTranscriptStatusAsync(orderId, currentStatusCode);
            }
            catch (PermissionsException pex)  // Not logged in or didn't have right permissions
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return CreateHttpResponseException("An error occurred during request processing: " + e.Message, HttpStatusCode.BadRequest);
            }

            TranscriptResponse jsonResponseContainer = new TranscriptResponse() { ResponseData = dataresponse };

            return Ok(jsonResponseContainer);
        }

        /// <summary>
        /// Retrieves a pdf of the student's unofficial transcript. 
        /// </summary>
        /// <param name="studentId">The system id for the student whose transcript is being requested</param>
        /// <param name="transcriptGrouping">The transcript grouping of transcript to return. If empty, transcripts of all grouping types will be returned for the student</param>
        /// <returns>A pdf of the student's unofficial transcript</returns>
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
        [HeaderVersionRoute("/students/{studentId}/unofficial-transcript", 1, true, Name = "GetUnofficialTranscript")]
        public async Task<IActionResult> GetUnofficialTranscriptAsync(string studentId, string transcriptGrouping)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("Student ID cannot be empty/null for unofficial transcript retrieval.", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(transcriptGrouping))
            {
                return CreateHttpResponseException("Transcript Grouping cannot be empty/null for unofficial transcript retrieval.", HttpStatusCode.BadRequest);
            }

            try
            {
                // Only service requests for pdf.  Don't want to return JSON, plain-text, or anything else by design.
                if (Request.GetTypedHeaders().Accept.Any(rqa => rqa.MediaType == "application/pdf"))
                {
                    var path = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports", "Student", "UnofficialTranscript.frx");
                    var deviceInfoPath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports", "Student", "UnofficialTranscriptDeviceInfo.txt");
                    var reportWatermarkPath = !string.IsNullOrEmpty(_apiSettings.UnofficialWatermarkPath) ? _apiSettings.UnofficialWatermarkPath : "";
                    if (string.IsNullOrEmpty(reportWatermarkPath))
                    {
                        reportWatermarkPath = "images/unofficial-watermark.png";
                    }

                    reportWatermarkPath = System.IO.Path.Combine(webHostEnvironment.WebRootPath, reportWatermarkPath);
                    string filenameToUse = string.Empty;
                    var officialTranscriptInfo = await _studentService.GetUnofficialTranscriptAsync(studentId, path, transcriptGrouping, reportWatermarkPath, deviceInfoPath);
                    var renderedBytes = officialTranscriptInfo.Item1;
                    var fileNameToUse = officialTranscriptInfo.Item2;

                    return File(renderedBytes, "application/pdf", fileNameToUse);
                }
                // If the request didn't specify pdf, it's an unsupported request
                else
                {
                    throw new NotSupportedException();
                }
            }
            catch (NotSupportedException)
            {
                return CreateHttpResponseException("Only application/pdf and application/json are served from this endpoint", HttpStatusCode.NotAcceptable);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving student's unofficial transcript";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException)
            {
                return CreateNotFoundException("unofficial transcript", studentId.ToString());
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all the emergency information for a person.
        /// </summary>
        /// <param name="studentId">Pass in a student's ID</param>
        /// <returns>Returns all the emergency information for the specified person</returns>
        [Obsolete("Obsolete as of API version 1.9, use GET /persons/{personId}/emergency-information")]
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/emergency-information", 1, true, Name = "GetStudentEmergencyInformation")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Base.EmergencyInformation>> GetEmergencyInformationAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("Invalid student ID", HttpStatusCode.BadRequest);
            }

            try
            {
                return await _emergencyInformationService.GetEmergencyInformationAsync(studentId);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                return CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Update a person's emergency information.
        /// </summary>
        /// <param name="emergencyInformation">An emergency information object</param>
        /// <returns>The updated emergency information object</returns>
        [Obsolete("Obsolete as of API version 1.9, use PUT /persons/{personId}/emergency-information")]
        [HttpPut]
        [HeaderVersionRoute("/students/{studentId}/emergency-information", 1, true, Name = "PutStudentEmergencyInformation")]
        public ActionResult<EmergencyInformation> PutEmergencyInformation(EmergencyInformation emergencyInformation)
        {
            if (emergencyInformation == null)
            {
                return CreateHttpResponseException("Request missing emergency information", HttpStatusCode.BadRequest);
            }
            try
            {
                var updatedEmergencyInformationTask = _emergencyInformationService.UpdateEmergencyInformation(emergencyInformation);
                var updatedEmergencyInformation = updatedEmergencyInformationTask.GetAwaiter().GetResult();

                return Ok(updatedEmergencyInformation);
            }
            catch (PermissionsException permissionException)
            {
                return CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                return CreateHttpResponseException(csse.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception)
            {
                return CreateHttpResponseException("Unable to update emergency information", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Process course section registration requests for a student. 
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <param name="sectionRegistrations">Registration requests to process</param>
        /// <returns>A registration response which includes any messages from registration</returns>
        /// <accessComments>
        /// A person may perform registration actions (register, drop, waitlist, etc) for themselves.  
        /// An advisor with ALL.ACCESS.ANY.ADVISEE may perform registration actions for any student.
        /// An advisor with ALL.ACCESS.ASSIGNED.ADVISEES may perform registration actions for one of their assigned advisees.
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/students/{studentId}/register", 1, true, Name = "Register")]
        public async Task<ActionResult<RegistrationResponse>> RegisterAsync(string studentId, [FromBody] IEnumerable<SectionRegistration> sectionRegistrations)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("Invalid studentId");
                return CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            if (sectionRegistrations == null || sectionRegistrations.Count() == 0)
            {
                _logger.LogError("Invalid sectionRegistration");
                return CreateHttpResponseException("Invalid sectionRegistration. Must provide at least one.", HttpStatusCode.BadRequest);
            }
            try
            {
                RegistrationResponse response = await _studentService.RegisterAsync(studentId, sectionRegistrations);
                return response;
            }

            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Timeout has occurred while registering for the student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("An error occurred during request processing: " + e.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Execute a set of registration actions to validate success or failure, but do not actually record the registration in the database.
        /// </summary>
        /// <param name="studentGuid">GUID of student to register</param>
        /// <param name="studentRegistrationRequest">A registration request</param>
        /// <returns>Results of the registration</returns>
        /// <accessComments>
        /// The user must have the REGISTER.VALIDATION.ONLY permission
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/students/{studentGuid}/register-validation-only", 1, true, Name = "RegisterValidationOnly")]
        public async Task<ActionResult<StudentRegistrationValidationOnlyResponse>> PostRegisterValidationOnlyAsync(string studentGuid, [FromBody] Dtos.Student.StudentRegistrationValidationOnlyRequest studentRegistrationRequest)
        {
            if (string.IsNullOrEmpty(studentGuid))
            {
                _logger.LogError("Missing studentGuid");
                return CreateHttpResponseException("Missing studentGuid", HttpStatusCode.BadRequest);
            }
            if (studentRegistrationRequest == null || studentRegistrationRequest.SectionActionRequests == null || studentRegistrationRequest.SectionActionRequests.Count() == 0)
            {
                _logger.LogError("Invalid studentRegistrationRequest");
                return CreateHttpResponseException("Invalid studentRegistrationRequest.", HttpStatusCode.BadRequest);
            }
            try
            {
                StudentRegistrationValidationOnlyResponse response = await _studentService.RegisterValidationOnlyAsync(studentGuid, studentRegistrationRequest);
                return Ok(response);
            }

            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Timeout has occurred while registering for the student {0}", studentGuid);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("An error occurred during request processing: " + e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Process course section registration requests for a student, bypassing validations. For use with cross-registration.
        /// </summary>
        /// <param name="studentGuid">GUID of student to register</param>
        /// <param name="studentRegistrationRequest">A registration request</param>
        /// <returns>Results of the registration</returns>
        /// <accessComments>
        /// The user must have the REGISTER.SKIP.VALIDATION permission
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/students/{studentGuid}/register-skip-validations", 1, true, Name = "RegisterSkipValidations")]
        public async Task<ActionResult<StudentRegistrationSkipValidationsResponse>> PostRegisterSkipValidationsAsync(string studentGuid, [FromBody] Dtos.Student.StudentRegistrationSkipValidationsRequest studentRegistrationRequest)
        {
            if (string.IsNullOrEmpty(studentGuid))
            {
                _logger.LogError("Missing studentGuid");
                return CreateHttpResponseException("Missing studentGuid", HttpStatusCode.BadRequest);
            }
            if (studentRegistrationRequest == null || studentRegistrationRequest.SectionActionRequests == null || studentRegistrationRequest.SectionActionRequests.Count() == 0)
            {
                _logger.LogError("Invalid studentRegistrationRequest");
                return CreateHttpResponseException("Invalid studentRegistrationRequest.", HttpStatusCode.BadRequest);
            }
            if ((studentRegistrationRequest.CrossRegHomeStudent ?? false) && (studentRegistrationRequest.CrossRegVisitingStudent ?? false))
            {
                var message = "CrossRegHomeStudent and CrossRegVisitingStudent cannot both be true.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                StudentRegistrationSkipValidationsResponse response = await _studentService.RegisterSkipValidationsAsync(studentGuid, studentRegistrationRequest);
                return Ok(response);
            }

            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Timeout has occurred while registering for the student {0}", studentGuid);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("An error occurred during request processing: " + e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Process drop course section requests by faculty for a student. 
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <param name="sectionDropRegistration">Section Drop Registration request to process</param>
        /// <returns>A registration response which includes any messages from drop registration</returns>
        /// <accessComments>
        /// Faculty with DROP.STUDENT or a departmental oversight person with CREATE.SECTION.DROP.STUDENT may perform drop section action for students.
        /// </accessComments>
        [HttpPut]
        [HeaderVersionRoute("/students/{studentId}/drop-registration", 1, true, Name = "DropRegistration")]
        public async Task<ActionResult<RegistrationResponse>> DropRegistrationAsync(string studentId, [FromBody] SectionDropRegistration sectionDropRegistration)
        {

            if (string.IsNullOrWhiteSpace(studentId))
            {
                _logger.LogError("Invalid studentId");
                return CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }

            if (sectionDropRegistration == null)
            {
                _logger.LogError("Invalid sectionDropRegistrations");
                return CreateHttpResponseException("Invalid sectionDropRegistrations. Must provide at least one.", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(sectionDropRegistration.SectionId))
            {
                _logger.LogError("Invalid sectionId");
                return CreateHttpResponseException("Invalid sectionId. Must provide at least one.", HttpStatusCode.BadRequest);
            }

            try
            {
                RegistrationResponse response = await _studentService.DropRegistrationAsync(studentId, sectionDropRegistration);
                return response;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                var message = "Session has expired while dropping student section registration.";
                _logger.LogError(csee, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex, peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("An error occurred during request processing: " + e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves information for the specified student, including references to the student's DegreePlan, Programs and Restrictions and some demographic information.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>Information about this <see cref="Student">Student</see></returns>
        /// <accessComments>
        /// Student information can be retrieved only if:
        /// 1. A Student is accessing its own data.
        /// 2. Proxy user is accessing the student's data.
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permissions of VIEW.PERSON.INFORMATION and VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// 
        ///  Privacy is enforced by this response. If any student has an assigned privacy code that the advisor or faculty is not authorized to access, the Student response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned. In this situation, 
        /// all details except the student name are cleared from the specific Student object.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}", 1, false, Name = "GetStudent")]
        [HeaderVersionRoute("/students/{studentId}", 2, false, Name = "GetStudent2")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Student.Student>> GetStudentAsync(string studentId)
        {
            try
            {
                var privacyWrapper = await _studentService.GetAsync(studentId);
                var student = privacyWrapper.Dto as Ellucian.Colleague.Dtos.Student.Student;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return student;

            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException)
            {
                return CreateNotFoundException("student", studentId);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving student information for student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception exception)
            {
                return CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves information for the searched student.
        /// </summary>
        /// <param name="criteria">Query criteria for retrieving students.</param>
        /// <param name="pageSize">Number of records to retrieve per page</param>
        /// <param name="pageIndex">Page number</param>
        /// <returns>Information about the queried student(s)</returns>
        /// <accessComments>
        /// Authenticated users with the VIEW.PERSON.INFORMATION and VIEW.STUDENT.INFORMATION permissions can query students.
        /// 
        /// Student privacy is enforced by this response. If any student has an assigned privacy code that the requestor is not authorized to access, 
        /// the response object is returned with an X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of students. 
        /// In this situation, all details except the student name are cleared from the specific student object.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/students", 2, true, Name = "QueryStudentByPost2")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.Student>>> QueryStudentByPost2Async([FromBody] StudentSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            _logger.LogInformation("Entering QueryStudentByPost2Async");
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                // The service to execute this search is in StudentService.
                var privacyWrapper = await _studentService.Search3Async(criteria, pageSize, pageIndex);
                var students = privacyWrapper.Dto as List<Dtos.Student.Student>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                watch.Stop();
                _logger.LogInformation("QueryStudentByPost2Async... completed in " + watch.ElapsedMilliseconds.ToString());

                return Ok((IEnumerable<Dtos.Student.Student>)students);
            }
            catch (PermissionsException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving information for the searched student";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets information the programs in which the specified student is enrolled.
        /// </summary>
        /// <param name="studentId">Student's ID</param>
        /// <param name="currentOnly">Boolean to indicate whether this request is for active student programs, or ended/past programs as well</param>
        /// <returns>All <see cref="StudentProgram2">Programs</see> in which the specified student is enrolled.</returns>
        /// <accessComments>
        /// Student information can be retrieved only if:
        /// 1. A Student is accessing its own data.
        /// 2. Proxy user is accessing the student's data.
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/programs", 2, true, Name = "GetStudentPrograms2")]
        public async Task<ActionResult<IEnumerable<StudentProgram2>>> GetStudentPrograms2Async(string studentId, bool currentOnly = true)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "studentId must be provided in order to retrieve student's programs");
            }
            try
            {
                await _studentService.CheckStudentAccessAsync(studentId);

                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentProgram> studentPrograms = await _studentProgramRepository.GetAsync(studentId);

                // Limit set of student programs to current programs if requested
                if (currentOnly == true)
                {
                    studentPrograms = studentPrograms.Where(x => x.EndDate == null || x.EndDate >= DateTime.Today);
                }

                List<StudentProgram2> studentProgramDtos = new List<StudentProgram2>();

                if (studentPrograms.Count() > 0)
                {
                    var studentProgramDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, StudentProgram2>();
                    var requirementDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement, Requirement>();
                    foreach (var prog in studentPrograms)
                    {
                        var studentProgramDto = studentProgramDtoAdapter.MapToType(prog);
                        foreach (var additionalReq in studentProgramDto.AdditionalRequirements)
                        {
                            if (!String.IsNullOrEmpty(additionalReq.RequirementCode))
                            {
                                additionalReq.Requirement = requirementDtoAdapter.MapToType((await _requirementRepository.GetAsync(additionalReq.RequirementCode)));
                            }
                        }
                        studentProgramDtos.Add(studentProgramDto);
                    }
                }

                return studentProgramDtos;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving programs for student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                string message = "An exception occurred while retrieving programs for student: " + studentId;
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Return a list of Students objects based on page.
        /// </summary>
        ///  <param name="page">page</param>
        /// <param name="person">GUID for a reference to link a student to the common HEDM persons entity.</param>
        /// <param name="type">GUID for the type of the student.</param>
        /// <param name="cohorts">GUID for the groupings of students for reporting/tracking purposes (cohorts) to which the student is associated.</param>
        /// <param name="residency">GUID for the residency type for selecting students.</param>
        /// <returns>List of Students <see cref="Dtos.Students"/> objects representing matching Students</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.STUDENT.INFORMATION can query students.
        /// </accessComments>
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), PermissionsFilter(StudentPermissionCodes.ViewStudentInformation)]
        [ValidateQueryStringFilter(new string[] { "person", "type", "cohorts", "residency" }, false, true)]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/students", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentsV7", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentsAsync(Paging page, [FromQuery] string person = "", [FromQuery] string type = "", [FromQuery] string cohorts = "", [FromQuery] string residency = "")
        {
            string criteria = string.Concat(person, type, cohorts, residency);

            //valid query parameter but empty argument
            if (!string.IsNullOrEmpty(criteria) && (string.IsNullOrEmpty(criteria.Replace("\"", "")) || string.IsNullOrEmpty(criteria.Replace("'", ""))))
            {
                return new PagedActionResult<IEnumerable<Dtos.Students>>(new List<Dtos.Students>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

            if (person == null || person == "null" || type == null || type == "null" || cohorts == null ||
                cohorts == "null" || residency == null || residency == "null")
                // null vs. empty string means they entered a filter with no criteria and we should return an empty set.
                return new PagedActionResult<IEnumerable<Dtos.Students>>(new List<Dtos.Students>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                _studentService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Students>>(new List<Dtos.Students>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _studentService.GetStudentsAsync(page.Offset, page.Limit, bypassCache, person, type, cohorts, residency);

                AddEthosContextProperties(await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Students>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);


            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Return a list of Students objects based on page.
        /// </summary>
        ///  <param name="page">page</param>
        /// <param name="criteria">Person search criteria in JSON format</param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of Students <see cref="Dtos.Students"/> objects representing matching Students</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.STUDENT.INFORMATION permission can query students.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), PermissionsFilter(StudentPermissionCodes.ViewStudentInformation)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Students2))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/students", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudents", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudents2Async(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }

            //if (CheckForEmptyFilterParameters())
            //    return new PagedActionResult<IEnumerable<Dtos.Addresses>>(new List<Dtos.Addresses>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            string personFilterValue = string.Empty;
            var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
            if (personFilterObj != null)
            {
                if (personFilterObj.personFilter != null)
                {
                    personFilterValue = personFilterObj.personFilter.Id;
                }
            }

            var criteriaFilter = GetFilterObject<Dtos.Students2>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.Students2>>(new List<Dtos.Students2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            try
            {
                _studentService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _studentService.GetStudents2Async(page.Offset, page.Limit, criteriaFilter, personFilterValue, bypassCache);

                AddEthosContextProperties(await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Students2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves a Student by Guid.
        /// </summary>
        /// <returns>An <see cref="Dtos.Students">Students</see>object.</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentInformation)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/students/{guid}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentsByGuidV7", IsEedmSupported = true)]
        public async Task<ActionResult<Students>> GetStudentsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                _studentService.ValidatePermissions(GetPermissionsMetaData());
                var student = await _studentService.GetStudentsByGuidAsync(guid);

                if (student != null)
                {

                    AddEthosContextProperties(await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { student.Id }));
                }


                return student;

            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves a Student by Guid.
        /// </summary>
        /// <returns>An <see cref="Dtos.Students">Students</see>object.</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentInformation)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/students/{guid}", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Students2>> GetStudentsByGuid2Async(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                _studentService.ValidatePermissions(GetPermissionsMetaData());
                var student = await _studentService.GetStudentsByGuid2Async(guid);

                if (student != null)
                {

                    AddEthosContextProperties(await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { student.Id }));
                }


                return student;

            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>        
        /// Creates a Student
        /// </summary>
        /// <param name="student"><see cref="Dtos.Students">Student</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.Students">Student</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/students", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentsV7")]
        [HeaderVersionRoute("/students", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentsV16.0.0")]
        public async Task<ActionResult<Dtos.Students>> PostStudentAsync([FromBody] Dtos.Students student)
        {
            //Create is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>        
        /// Updates a Student.
        /// </summary>
        /// <param name="guid">Id of the Student to update</param>
        /// <param name="student"><see cref="Dtos.Student">Student</see> to create</param>
        /// <returns>Updated <see cref="Dtos.Students">Student</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/students/{guid}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentsV7")]
        public async Task<ActionResult<Dtos.Students>> PutStudentAsync([FromRoute] string guid, [FromBody] Dtos.Students student)
        {
            //Update is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>        
        /// Updates a Student.
        /// </summary>
        /// <param name="guid">Id of the Student to update</param>
        /// <param name="student"><see cref="Dtos.Student">Student</see> to create</param>
        /// <returns>Updated <see cref="Dtos.Students">Student</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/students/{guid}", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, IsEedmSupported = true, Name = "PutStudentsByGuidV16.0.0")]
        public async Task<ActionResult<Students2>> PutStudent2Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Students2 student)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (student == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null student argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(student.Id))
            {
                student.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, student.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                var dpList = await _studentService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                var studentReturn = await _studentService.UpdateStudents2Async(
                  await PerformPartialPayloadMerge(student, async () => await _studentService.GetStudentsByGuid2Async(guid, true),
                  dpList, _logger));

                AddEthosContextProperties(dpList,
                    await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return studentReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }


        /// <summary>
        /// Delete (DELETE) an existing Student
        /// </summary>
        /// <param name="guid">Id of the Student to delete</param>
        [HttpDelete]
        [Route("/students/{guid}", Name = "DeleteStudentsByGuid", Order = -10)]
        public async Task<IActionResult> DeleteStudentByGuidAsync([FromRoute] string guid)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Retrieves a student that contains only information needed for degree planning.
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns>A <see cref="PlanningStudent">PlanningStudent</see> object.  </returns>
        /// <accessComments>
        /// Student information can be retrieved only if:
        /// 1. A Student is accessing its own data.
        /// 2. Proxy user is accessing the student's data.
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permissions of VIEW.PERSON.INFORMATION and VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// 
        ///  Privacy is enforced by this response. If any student has an assigned privacy code that the advisor or faculty is not authorized to access, the PlanningStudent response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned. In this situation, 
        /// all details except the student name are cleared from the specific planning student object.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}", 1, false, "application/vnd.ellucian-planning-student.v{0}+json", Name = "GetPlanningStudent")]
        public async Task<ActionResult<PlanningStudent>> GetPlanningStudentAsync(string studentId)
        {
            try
            {

                var privacyWrapper = await _studentService.GetPlanningStudentAsync(studentId);
                var planningStudent = privacyWrapper.Dto as PlanningStudent;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    Response.Headers.Append("X-Content-Restricted", "partial");
                }
                return planningStudent;

            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving planning student for {0}", studentId);
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
        /// This retrieves student's academic levels.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>List of Student's Academic Levels</returns>
        ///<accessComments>
        /// Student Academic Levels can be retrieved only if:
        /// 1. A Student is accessing its own data.
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
        /// 4. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data. 
        ///</accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/academic-levels", 1, true, Name = "GetStudentAcademicLevels")]
        public async Task<ActionResult<IEnumerable<StudentAcademicLevel>>> GetStudentAcademicLevelsAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id passed to retrieve student's academic level cannot be null or empty");
            }

            try
            {
                IEnumerable<StudentAcademicLevel> studentAcademicLevels = await _studentService.GetStudentAcademicLevelsAsync(studentId);
                return Ok(studentAcademicLevels);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving academic levels for student {0}", studentId);
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string message = "A user needs to be self or an advisor in order to view student's academic levels";
                _logger.LogError(pex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = "An exception occured while retrieving student's academic levels for student with id- " + studentId;
                _logger.LogError(e, message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

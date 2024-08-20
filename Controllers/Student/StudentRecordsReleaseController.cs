// Copyright 2022-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
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
using System.Net.Http;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to student release access data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentRecordsReleaseController : BaseCompressedApiController
    {
        private readonly IStudentRecordsReleaseService _studentRecordsReleaseService;
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IStudentConfigurationRepository configurationRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        // GET: StudentReleaseAccessCode
        /// <summary>
        /// Initializes a new instance of the StudentReleaseAccessCodeController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="configurationRepository">Repository of type <see cref="IStudentConfigurationRepository">IStudentConfigurationRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        /// <param name="studentRecordsReleaseService">Service of type <see cref="IStudentRecordsReleaseService">IStudentRecordsReleaseService</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentRecordsReleaseController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, IStudentConfigurationRepository configurationRepository,ILogger logger, IStudentRecordsReleaseService studentRecordsReleaseService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._studentRecordsReleaseService = studentRecordsReleaseService;
            this.referenceDataRepository = referenceDataRepository;
            this.configurationRepository = configurationRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }
        /// <summary>
        /// Any authenticated user can retrieves all student release access codes.
        /// </summary>
        /// <returns>All <see cref="StudentReleaseAccess">student release access codes,descriptions and comments.</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/student-release-access-code", 1, true, Name = "GetStudentReleaseAccessCode")]
        public async Task<ActionResult<IEnumerable<StudentReleaseAccess>>> GetStudentReleaseAccessCodesAsync()
        {
            try
            {
                var studentReleaseAccessCodesCollection = await referenceDataRepository.GetStudentReleaseAccessCodesAsync();

                // Get the right adapter for the type mapping
                var studentReleaseAccessCodesDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentReleaseAccess, StudentReleaseAccess>();

                // Map the StudentReleaseAccess entity to the program DTO
                var studentReleaseAccessCodesDtoCollection = new List<StudentReleaseAccess>();
                if (studentReleaseAccessCodesCollection != null && studentReleaseAccessCodesCollection.Any())
                {
                    foreach (var studentReleaseAccessCodes in studentReleaseAccessCodesCollection)
                    {
                        studentReleaseAccessCodesDtoCollection.Add(studentReleaseAccessCodesDtoAdapter.MapToType(studentReleaseAccessCodes));
                    }
                }
                return studentReleaseAccessCodesDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Your previous session has expired and is no longer valid.";
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Unable to retrieve student release access codes";
                logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the configuration information needed for Colleague Self-Service student records release.
        /// </summary>
        /// <returns>The <see cref="StudentRecordsReleaseConfig"/></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this information.</accessComments>
        [HttpGet]
        [HeaderVersionRoute("/configuration/student-records-release", 1, true, Name = "GetStudentRecordsReleaseConfiguration")]
        public async Task<ActionResult<StudentRecordsReleaseConfig>> GetStudentRecordsReleaseConfigAsync()
        {           
            try
            {                
                var studentRecordsReleaseConfiguration = await configurationRepository.GetStudentRecordsReleaseConfigAsync();

                // Get the right adapter for the type mapping
                var studentRecordsReleaseConfigDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseConfig, StudentRecordsReleaseConfig>();

                // Map the StudentRecordsReleaseConfig entity to the program DTO
                StudentRecordsReleaseConfig studentRecordsReleaseConfigDto = null;
                studentRecordsReleaseConfigDto = studentRecordsReleaseConfigDtoAdapter.MapToType(studentRecordsReleaseConfiguration);
                return studentRecordsReleaseConfigDto;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving student records release configuration information.";
                logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Unable to retrieve student records release configuration information.";
                logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Returns the student records release deny access for the specified student. 
        /// </summary>
        /// <param name="studentId">The student Id to retrieve student records release deny access</param>
        /// <returns>The<see cref="Dtos.Student.StudentRecordsReleaseDenyAccess">StudentRecordsReleaseDenyAccess</see>object</returns>
        /// <accessComments>Only student can retrieve information for self.</accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/student-records-release-deny-access", 1, true, Name = "GetStudentRecordsReleaseDenyAccess")]
        public async Task<ActionResult<StudentRecordsReleaseDenyAccess>> GetStudentRecordsReleaseDenyAccessAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                logger.LogError("Unable to get student records release deny access. Invalid studentId " + studentId);
                return CreateHttpResponseException("Unable to get student records release deny access. Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentRecordsReleaseService.GetStudentRecordsReleaseDenyAccessAsync(studentId);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving student records release deny access for student {0}", studentId);
                logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                string message = string.Format("Access to student records release deny access is forbidden for student {0}", studentId);
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = string.Format("Error occurred while retrieving student records release deny access for student {0}", studentId);
                logger.LogError(e, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the student records release information for the specified student. 
        /// </summary>
        /// <param name="studentId">The student Id to retrieve student records release information</param>
        /// <returns>List of <see cref="Dtos.Student.StudentRecordsReleaseInfo">StudentRecordsReleaseInfo</see>object</returns>
        /// <accessComments>Only student can retrieve information for self.</accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/student-records-release-info", 1, true, Name = "GetStudentRecordsReleaseInformation")]
        public async Task<ActionResult<IEnumerable<StudentRecordsReleaseInfo>>> GetStudentRecordsReleaseInformationAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                logger.LogError("Unable to get student records release information. Invalid studentId " + studentId);
                return CreateHttpResponseException("Unable to get student records release information. Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _studentRecordsReleaseService.GetStudentRecordsReleaseInformationAsync(studentId));
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving student records release information for student {0}", studentId);
                logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                string message = string.Format("Access to student records release information is forbidden for student {0}", studentId);
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = string.Format("Error occurred while retrieving student records release information for student {0}", studentId);
                logger.LogError(e, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Delete a student records release information for a student
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <param name="studentReleaseId">Student Release Id</param>
        /// <returns>The<see cref= "StudentRecordsReleaseInfo"> StudentRecordsReleaseInfo </see> object after deletion </returns >
        /// <accessComments>Only student can delete information for self.</accessComments>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to delete a student records release information</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if any other deletion problem.</exception>
        [HttpPut]
        [HeaderVersionRoute("/student-records-release-info/{studentId}/student-release/{studentReleaseId}", 1, true, Name = "EndStudentRecordsReleaseInformation")]
        public async Task<ActionResult<StudentPetition>> DeleteStudentRecordsReleaseInfoAsync(string studentId, string studentReleaseId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                logger.LogError("Unable to delete student records release information. Invalid studentId " + studentId);
                return CreateHttpResponseException("Unable to delete student records release information. Invalid studentId", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(studentReleaseId))
            {
                logger.LogError("Unable to delete student records release information. Invalid studentReleaseId " + studentReleaseId);
                return CreateHttpResponseException("Unable to delete student records release information. Invalid studentReleaseId", HttpStatusCode.BadRequest);
            }
            else
            {
                StudentRecordsReleaseInfo existingRecord = await _studentRecordsReleaseService.GetStudentRecordsReleaseInfoByIdAsync(studentReleaseId);
                if (existingRecord.EndDate <= DateTime.Today)
                {       
                    logger.LogError("Validity of this record has already ended. Cannot modify");
                    return CreateHttpResponseException("Validity of this record has already ended, Cannot modify.", HttpStatusCode.BadRequest);
                }
            }
            try
            {
                StudentRecordsReleaseInfo studentRecordsReleaseInfo = await _studentRecordsReleaseService.DeleteStudentRecordsReleaseInfoAsync(studentId, studentReleaseId);
                return Created(String.Empty, studentRecordsReleaseInfo);

            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while deleting student records release information for student {0}", studentId);
                logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                string message = string.Format("Access to student records release information is forbidden for student {0}", studentId);
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (RecordLockException rle)
            {
                string message = "Deleting student records release information with id " + studentId + " is locked";
                logger.LogError(rle, message);
                return CreateHttpResponseException(message, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                string message = string.Format("Error occurred while deleting student records release information for student {0}", studentId);
                logger.LogError(e, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create a new student records release information for student.
        /// </summary>
        /// <param name="studentRecordsRelease"><see cref="StudentRecordsReleaseInfo">Student Records Release</see> information for creating a new relationship record.</param>
        /// <returns>Newly created <see cref="StudentRecordsReleaseInfo">student Records Release</see>.</returns>
        /// <accessComments>
        /// This action can only be performed by a student and can add the student records release information to self.
        /// </accessComments>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="StudentRecordsReleaseInfo">Student Records Release</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to add a student records release relationship</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if any other creation problem.</exception>
        [HttpPost]
        [HeaderVersionRoute("/student-records-release-info", 1, true, Name = "AddStudentRecordsReleaseInformation")]
        public async Task<ActionResult<StudentRecordsReleaseInfo>> PostStudentRecordsReleaseInformationAsync([FromBody] StudentRecordsReleaseInfo studentRecordsRelease)
        {
            List<string> errorTexts = new List<string>();
            StudentRecordsReleaseConfig config = (await GetStudentRecordsReleaseConfigAsync())?.Value;
            if (studentRecordsRelease == null)
            {
                errorTexts.Add("Must provide the student Records Release input item to create a new student Records Release information");
            }
            if (string.IsNullOrEmpty(studentRecordsRelease.StudentId))
            {
                errorTexts.Add("Must provide the student ID to create a new student Records Release information");
            }
            if (string.IsNullOrEmpty(studentRecordsRelease.FirstName))
            {
                errorTexts.Add("Must provide the First Name to create a new student Records Release information");
            }
            if (string.IsNullOrEmpty(studentRecordsRelease.LastName))
            {
                errorTexts.Add("Must provide the Last Name to create a new student Records Release information");               
            }
            if (config != null && config.IsPinRequired && string.IsNullOrEmpty(studentRecordsRelease.PIN))
            {
                errorTexts.Add("Must provide the Pin to create a new student Records Release information");             
            }
            if (string.IsNullOrEmpty(studentRecordsRelease.RelationType))
            {
                errorTexts.Add("Must provide the Relation Type to create a new student Records Release information");               
            }
            if (studentRecordsRelease.AccessAreas == null || !studentRecordsRelease.AccessAreas.Any() || studentRecordsRelease.AccessAreas.Any(a => a == null))
            {
                errorTexts.Add("Must provide the Access area codes to create a new student Records Release information");               
            }            
            if (studentRecordsRelease.StartDate != null && studentRecordsRelease.EndDate != null && studentRecordsRelease.StartDate > studentRecordsRelease.EndDate)
            {
                errorTexts.Add("Start Date cannot be later than End date");
            }
            if (studentRecordsRelease.EndDate != null && studentRecordsRelease.EndDate < DateTime.Today)
            {
                errorTexts.Add("End Date cannot be a past date");
            }
            if (!studentRecordsRelease.IsConsentGiven)
            {
                errorTexts.Add("Consent is not given");
            }
            if (errorTexts.Any())
            {
                errorTexts.ForEach(message => logger.LogError(message));
                return CreateHttpResponseException(string.Join("; ", errorTexts), HttpStatusCode.BadRequest);
            }
            try
            {
                StudentRecordsReleaseInfo newstudentRecordsReleaseInfo = await _studentRecordsReleaseService.AddStudentRecordsReleaseInfoAsync(studentRecordsRelease);
                return Created("", newstudentRecordsReleaseInfo);
            }
            catch (PermissionsException pe)
            {
                string message = "Permission exception occured while adding a new student Records Release information";
                logger.LogError(pe,message);
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while adding a new student Records Release information";
                logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (RecordLockException rle)
            {
                string message = "Adding a new student records release information with id " + studentRecordsRelease.StudentId + " is locked";
                logger.LogError(rle, message);
                return CreateHttpResponseException(message, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                string message = "Exception occured while adding a new student records release information";
                logger.LogError(e,message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Deny access to student records release information
        /// </summary>
        /// <param name="studentRecordsRelDenyAccess"><see cref="DenyStudentRecordsReleaseAccessInformation">Student Records Release Deny Access</see> information for denying access to student records release</param>
        /// <returns>The Updated<see cref="StudentRecordsReleaseInfo">StudentRecordsReleaseInfo</see>object</returns>
        /// <accessComments>
        /// This action can only be performed by a student and can deny the student records release access information to self.
        /// </accessComments>
        /// <returns>An HttpResponseMessage which includes the updated<see cref="StudentRecordsReleaseInfo">student records release information</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to deny the student records release access information</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if any other denying problem.</exception>
        [HttpPost]
        [HeaderVersionRoute("/deny-student-records-release-access", 1, true, Name = "DenyStudentRecordsReleaseAccess")]
        public async Task<ActionResult<IEnumerable<StudentRecordsReleaseInfo>>> DenyStudentRecordsReleaseAccessAsync([FromBody] DenyStudentRecordsReleaseAccessInformation studentRecordsRelDenyAccess)
        {
            if (studentRecordsRelDenyAccess == null)
            {
                string errorText = "Must provide the deny student records release access information input item to deny access.";
                logger.LogError(errorText);
                return CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _studentRecordsReleaseService.DenyStudentRecordsReleaseAccessAsync(studentRecordsRelDenyAccess));
            }
            catch (PermissionsException pe)
            {
                string message = "Permission exception occured while denying access to student records release information";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Permission exception occured while denying access to student records release information";
                logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (RecordLockException rle)
            {
                string message = "Denying access to student records release information with id " + studentRecordsRelDenyAccess.StudentId + " is locked";
                logger.LogError(rle, message);
                return CreateHttpResponseException(message, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                string message = "Exception occured while denying access to student records release information";
                logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates a student records release information for student.
        /// </summary>
        /// <param name="studentRecordsRelease"><see cref="StudentRecordsReleaseInfo">Student Records Release</see> information for updating the relationship record.</param>
        ///<returns>The Updated<see cref= "StudentRecordsReleaseInfo"> StudentRecordsReleaseInfo </see> object </returns >
        /// <accessComments>
        /// This action can only be performed by a student and can update the student records release information to self.
        /// </accessComments>
        /// <returns>An HttpResponseMessage which includes true on successful operation</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to add a student records release relationship</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if any other creation problem.</exception>
        [HttpPut]
        [HeaderVersionRoute("/student-records-release-info", 1, true, Name = "UpdateStudentRecordsReleaseInformation")]
        public async Task<ActionResult<StudentRecordsReleaseInfo>> PutStudentRecordsReleaseInformationAsync([FromBody] StudentRecordsReleaseInfo studentRecordsRelease)
        {
            List<string> errorTexts = new List<string>();
            StudentRecordsReleaseConfig config = (await GetStudentRecordsReleaseConfigAsync())?.Value;        
            if (studentRecordsRelease == null)
            {
                errorTexts.Add("Must provide the student records release input item to create a new student Records Release information");
            }
            if (string.IsNullOrEmpty(studentRecordsRelease.Id))
            {
                errorTexts.Add("Must provide the Id to update student records release information");
            }
            else
            {
                StudentRecordsReleaseInfo existingRecord = await _studentRecordsReleaseService.GetStudentRecordsReleaseInfoByIdAsync(studentRecordsRelease.Id);
                if(existingRecord.EndDate <= DateTime.Today)
                {
                    errorTexts.Add("Validity of this record has already ended. Cannot modify");
                }
            }
            if (string.IsNullOrEmpty(studentRecordsRelease.StudentId))
            {
                errorTexts.Add("Must provide the student Id to update the student records release information");
            }
            if (config != null && config.IsPinRequired && string.IsNullOrEmpty(studentRecordsRelease.PIN))
            {
                errorTexts.Add("Must provide the pin to update student records release information");
            }
            if (studentRecordsRelease.AccessAreas == null || !studentRecordsRelease.AccessAreas.Any() || studentRecordsRelease.AccessAreas.Any(a => a == null))
            {
                errorTexts.Add("Must provide the access area codes to update student records release information");
            }
            if (studentRecordsRelease.EndDate != null && studentRecordsRelease.EndDate < DateTime.Today)
            {
                errorTexts.Add("End Date cannot be a past date");
            }
            if (errorTexts.Any())
            {
                errorTexts.ForEach(message => logger.LogError(message));
                return CreateHttpResponseException(string.Join("; ", errorTexts), HttpStatusCode.BadRequest);
            }
            try
            {
                StudentRecordsReleaseInfo updatedStudentRecordsReleaseInfo = await _studentRecordsReleaseService.UpdateStudentRecordsReleaseInfoAsync(studentRecordsRelease);
                return Created(String.Empty, updatedStudentRecordsReleaseInfo);
            }
            catch (PermissionsException pe)
            {
                string message = "Permission exception occured while updating student records release information";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while updating student records release information";
                logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (RecordLockException rle)
            {
                string message = "Updating student records release information with id " + studentRecordsRelease.StudentId + " is locked";
                logger.LogError(rle, message);
                return CreateHttpResponseException(message, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                string message = "Exception occured while updating student records release information";
                logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}

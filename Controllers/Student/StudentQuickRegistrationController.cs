// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student.QuickRegistration;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Controller for student Colleague Self-Service Quick Registration operations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentQuickRegistrationController : BaseCompressedApiController
    {
        private readonly IStudentQuickRegistrationService _studentQuickRegistrationService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the StudentQuickRegistrationController class.
        /// </summary>
        /// <param name="studentQuickRegistrationService">Service of type <see cref="IStudentQuickRegistrationService">IStudentQuickRegistrationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentQuickRegistrationController(IStudentQuickRegistrationService studentQuickRegistrationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {

            _studentQuickRegistrationService = studentQuickRegistrationService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves a given student's Colleague Self-Service Quick Registration information for the provided academic term codes. 
        /// If the Colleague Self-Service Quick Registration workflow is disabled then no quick registration sections will be returned for any academic terms.
        /// If the Colleague Self-Service Quick Registration workflow is enabled then planned course sections will be returned for any academic terms designated for quick registration. 
        /// </summary>
        /// <param name="studentId">ID of the student for whom Colleague Self-Service Quick Registration data will be retrieved</param>
        /// <returns>A <see cref="StudentQuickRegistration"/> object</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this degree plan</exception>
        /// <accessComments>
        /// Authenticated users may retrieve their own Colleague Self-Service Quick Registration information.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/quick-registration-sections", 1, true, Name = "GetStudentQuickRegistrationSectionsAsync")]
        public async Task<ActionResult<StudentQuickRegistration>> GetStudentQuickRegistrationSectionsAsync([FromRoute]string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogError("A student ID is required when retrieving a student's quick registration data.");
                return CreateHttpResponseException("A student ID is required when retrieving a student's quick registration data.", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentQuickRegistrationService.GetStudentQuickRegistrationAsync(studentId);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString(), "Unable to retrieve Colleague Self-Service Quick Registration information.");
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

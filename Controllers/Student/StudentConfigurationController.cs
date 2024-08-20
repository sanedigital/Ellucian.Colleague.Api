// Copyright 2015-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.InstantEnrollment;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to get student parameters and settings.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentConfigurationController : BaseCompressedApiController
    {
        private readonly IStudentConfigurationService _configurationService;
        private readonly IStudentConfigurationRepository _configurationRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// StudentConfigurationController class constructor
        /// </summary>
        /// <param name="configurationRepository">Repository of type <see cref="IStudentConfigurationRepository">IStudentConfigurationRepository</see></param>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="configurationService">Service of type <see cref="IStudentConfigurationService">IStudentConfigurationService</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentConfigurationController(IStudentConfigurationRepository configurationRepository, IAdapterRegistry adapterRegistry, ILogger logger, IStudentConfigurationService configurationService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _configurationService = configurationService;
            _configurationRepository = configurationRepository;
            _adapterRegistry = adapterRegistry;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves the configuration information needed to render a new graduation application asynchronously.
        /// </summary>
        /// <returns>The <see cref="GraduationConfiguration">Graduation Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. NotFound if the required setup is not complete or available.</exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>GraduationConfiguration data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/student-graduation", 1, false, Name = "GetGraduationConfiguration")]
        public async Task<ActionResult<GraduationConfiguration>> GetGraduationConfigurationAsync()
        {
            GraduationConfiguration configurationDto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration configuration = await _configurationRepository.GetGraduationConfigurationAsync();
                var graduationConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration, Ellucian.Colleague.Dtos.Student.GraduationConfiguration>();
                configurationDto = graduationConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed to render a new graduation application asynchronously.
        /// </summary>
        /// <returns>The <see cref="GraduationConfiguration2">Graduation Configuration2</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. NotFound if the required setup is not complete or available.</exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>GraduationConfiguration is cached for  24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/student-graduation", 2, true, Name = "GetGraduationConfiguration2")]
        public async Task<ActionResult<GraduationConfiguration2>> GetGraduationConfiguration2Async()
        {
            GraduationConfiguration2 configuration2Dto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration configuration = await _configurationRepository.GetGraduationConfigurationAsync();
                var graduationConfiguration2DtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration, Ellucian.Colleague.Dtos.Student.GraduationConfiguration2>();
                configuration2Dto = graduationConfiguration2DtoAdapter.MapToType(configuration);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired  while retrieving graduation configuration";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configuration2Dto;
        }

        /// <summary>
        /// Retrieves the configuration information needed to render a new transcript request or enrollment verification in self-service asynchronously.
        /// </summary>
        /// <returns>The <see cref="StudentRequestConfiguration">StudentRequestConfiguration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Student request configuration data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/student-request", 1, true, Name = "GetStudentRequestConfiguration")]
        public async Task<ActionResult<StudentRequestConfiguration>> GetStudentRequestConfigurationAsync()
        {
            StudentRequestConfiguration configurationDto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.StudentRequestConfiguration configuration = await _configurationRepository.GetStudentRequestConfigurationAsync();
                var studentRequestConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRequestConfiguration, Ellucian.Colleague.Dtos.Student.StudentRequestConfiguration>();
                configurationDto = studentRequestConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving student request configuration information.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for faculty grading asynchronously.
        /// </summary>
        /// <returns>The <see cref="FacultyGradingConfiguration">Faculty Grading Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. NotFound if the required setup is not complete or available.</exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Faculty grading configuration data is cached for 24 hours.</note>
        [Obsolete("Obsolete as of API version 1.36, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/configuration/faculty-grading", 1, false, Name = "GetFacultyGradingConfiguration")]
        public async Task<ActionResult<FacultyGradingConfiguration>> GetFacultyGradingConfigurationAsync()
        {
            FacultyGradingConfiguration configurationDto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration configuration = await _configurationRepository.GetFacultyGradingConfigurationAsync();
                var configurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration, Ellucian.Colleague.Dtos.Student.FacultyGradingConfiguration>();
                configurationDto = configurationDtoAdapter.MapToType(configuration);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for faculty grading asynchronously.
        /// </summary>
        /// <returns>The <see cref="FacultyGradingConfiguration2">Faculty Grading Configuration2</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. NotFound if the required setup is not complete or available.</exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Faculty grading configuration data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/faculty-grading", 2, true, Name = "GetFacultyGradingConfiguration2")]
        public async Task<ActionResult<FacultyGradingConfiguration2>> GetFacultyGradingConfiguration2Async()
        {
            FacultyGradingConfiguration2 configuration2Dto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration2 configuration2 = await _configurationRepository.GetFacultyGradingConfiguration2Async();
                var configuration2DtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration2, Ellucian.Colleague.Dtos.Student.FacultyGradingConfiguration2>();
                configuration2Dto = configuration2DtoAdapter.MapToType(configuration2);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving faculty grading configuration information.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configuration2Dto;
        }

        /// <summary>
        /// Retrieves the student profile configuration information needed for student profile asynchronously.
        /// </summary>
        /// <returns>The <see cref="StudentProfileConfiguration">Student Profile Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. NotFound if the required setup is not complete or available.</exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>StudentProfileConfiguration is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/student-profile", 1, true, Name = "GetStudentProfileConfigurationAsync")]
        public async Task<ActionResult<StudentProfileConfiguration>> GetStudentProfileConfigurationAsync()
        {
            try
            {
                return await _configurationService.GetStudentProfileConfigurationAsync();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
                _logger.LogError(csse, invalidSessionErrorMessage);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException("Error retrieving Student Profile Configuration for faculty", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the configuration information needed for course catalog searches asynchronously.
        /// </summary>
        /// <returns>The <see cref="CourseCatalogConfiguration">Course Catalog Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Course Catalog Configuration data is cached for 24 hours.</note>
        [Obsolete("Obsolete as of API version 1.26, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/configuration/course-catalog", 1, false, Name = "GetCourseCatalogConfiguration")]
        public async Task<ActionResult<CourseCatalogConfiguration>> GetCourseCatalogConfigurationAsync()
        {
            CourseCatalogConfiguration configurationDto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration configuration = await _configurationRepository.GetCourseCatalogConfigurationAsync();
                var catalogConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, Ellucian.Colleague.Dtos.Student.CourseCatalogConfiguration>();
                configurationDto = catalogConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for course catalog searches asynchronously.
        /// </summary>
        /// <returns>The <see cref="CourseCatalogConfiguration2">Course Catalog Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Course Catalog Configuration data is cached for 24 hours.</note>
        [Obsolete("Obsolete as of API version 1.29, use version 3 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/configuration/course-catalog", 2, false, Name = "GetCourseCatalogConfiguration2")]
        public async Task<ActionResult<CourseCatalogConfiguration2>> GetCourseCatalogConfiguration2Async()
        {
            CourseCatalogConfiguration2 configurationDto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration configuration = await _configurationRepository.GetCourseCatalogConfiguration2Async();
                var catalogConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, Ellucian.Colleague.Dtos.Student.CourseCatalogConfiguration2>();
                configurationDto = catalogConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for registration processing asynchronously.
        /// </summary>
        /// <returns>The <see cref="RegistrationConfiguration">Registration Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Registration configuration data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/registration", 1, true, Name = "GetRegistrationConfigurationAsync")]
        public async Task<ActionResult<RegistrationConfiguration>> GetRegistrationConfigurationAsync()
        {
            RegistrationConfiguration configurationDto = null;
            try
            {
                Domain.Student.Entities.RegistrationConfiguration configuration = await _configurationRepository.GetRegistrationConfigurationAsync();
                var catalogConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration, RegistrationConfiguration>();
                configurationDto = catalogConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving registration configuration";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException("Could not retrieve registration configuration data.", HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for Colleague Self-Service instant enrollment
        /// </summary>
        /// <returns>The <see cref="InstantEnrollmentConfiguration"/></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Instant enrollment configuration information is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/instant-enrollment", 1, true, Name = "GetInstantEnrollmentConfigurationAsync")]
        public async Task<ActionResult<InstantEnrollmentConfiguration>> GetInstantEnrollmentConfigurationAsync()
        {
            InstantEnrollmentConfiguration configurationDto = null;
            try
            {
                Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration configuration = await _configurationRepository.GetInstantEnrollmentConfigurationAsync();
                var instantEnrollmentConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration, InstantEnrollmentConfiguration>();
                configurationDto = instantEnrollmentConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving Colleague Self-Service instant enrollment configuration data";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException("Could not retrieve Colleague Self-Service instant enrollment configuration data.", HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for course catalog searches asynchronously.
        /// </summary>
        /// <returns>The <see cref="CourseCatalogConfiguration3">Course Catalog Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Course Catalog Configuration data is cached for 24 hours.</note>
        [Obsolete("Obsolete as of API version 1.32, use version 4 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/configuration/course-catalog", 3, false, Name = "GetCourseCatalogConfiguration3")]
        public async Task<ActionResult<CourseCatalogConfiguration3>> GetCourseCatalogConfiguration3Async()
        {
            try
            {
                return await _configurationService.GetCourseCatalogConfiguration3Async();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving course catalog configuration data";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get the course catalog configuration information.");
                return CreateHttpResponseException("Unable to get the course catalog configuration information.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the configuration information needed for course catalog searches asynchronously.
        /// </summary>
        /// <returns>The <see cref="CourseCatalogConfiguration4">Course Catalog Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Course Catalog Configuration data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/course-catalog", 4, true, Name = "GetCourseCatalogConfiguration4")]
        public async Task<ActionResult<CourseCatalogConfiguration4>> GetCourseCatalogConfiguration4Async()
        {
            try
            {
                return await _configurationService.GetCourseCatalogConfiguration4Async();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving course catalog configuration data";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get the course catalog configuration information.");
                return CreateHttpResponseException("Unable to get the course catalog configuration information.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the configuration information needed for My Progress evaluation asynchronously.
        /// </summary>
        /// <returns>The <see cref="MyProgressConfiguration">MyProgress Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. NotFound if the required setup is not complete or available.</exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>My Progress Configuration data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/my-progress", 1, true, Name = "GetMyProgressConfiguration")]
        public async Task<ActionResult<MyProgressConfiguration>> GetMyProgressConfigurationAsync()
        {
            MyProgressConfiguration configurationDto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.MyProgressConfiguration configuration = await _configurationRepository.GetMyProgressConfigurationAsync();
                var myProgressConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MyProgressConfiguration, Ellucian.Colleague.Dtos.Student.MyProgressConfiguration>();
                configurationDto = myProgressConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired  while retrieving MyProgress configuration";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException ex)
            {
                string message = "Key not found while retrieving MyProgress configuration";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                string message = "Exception occurred while retrieving MyProgress configuration";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the section census configuration information needed for Colleague Self-Service
        /// </summary>
        /// <returns>The <see cref="SectionCensusConfiguration"/></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Section census configuration data is cached for 24 hours.</note>
        [Obsolete("Obsolete as of API version 1.36, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/configuration/section-census", 1, false, Name = "GetSectionCensusConfiguration")]
        public async Task<ActionResult<SectionCensusConfiguration>> GetSectionCensusConfigurationAsync()
        {
            SectionCensusConfiguration configurationDto = null;
            try
            {
                Domain.Student.Entities.SectionCensusConfiguration configuration = await _configurationRepository.GetSectionCensusConfigurationAsync();
                var sectionCensusConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionCensusConfiguration, SectionCensusConfiguration>();
                configurationDto = sectionCensusConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException("Could not retrieve Colleague Self-Service section census configuration data.", HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the section census configuration2 information needed for Colleague Self-Service
        /// </summary>
        /// <returns>The <see cref="SectionCensusConfiguration2"/></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Section census configuration data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/section-census", 2, true, Name = "GetSectionCensusConfiguration2")]
        public async Task<ActionResult<SectionCensusConfiguration2>> GetSectionCensusConfiguration2Async()
        {
            SectionCensusConfiguration2 configuration2Dto = null;
            try
            {
                Domain.Student.Entities.SectionCensusConfiguration2 configuration = await _configurationRepository.GetSectionCensusConfiguration2Async();
                var sectionCensusConfiguration2EntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionCensusConfiguration2, SectionCensusConfiguration2>();
                configuration2Dto = sectionCensusConfiguration2EntityToDtoAdapter.MapToType(configuration);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving section census configuration2 information";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                var message = "Could not retrieve Colleague Self-Service section census configuration2 information";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            return configuration2Dto;
        }

        /// <summary>
        /// Returns course delimiter defined on CDEF
        /// </summary>
        /// <returns>The Course Delimiter string</returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <returns></returns>
        [HttpGet]
        [HeaderVersionRoute("/configuration/course-delimiter", 1, true, Name = "GetCourseDelimiterConfiguration")]
        public async Task<ActionResult<string>> GetCourseDelimiterConfigurationAsync()
        {
            string defaultCourseDelimiter = "-";//default course delimiter
            string courseDelimiter = string.Empty;
            try
            {
                courseDelimiter = await _configurationRepository.GetCourseDelimiterAsync();
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving course delimiter";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not retrieve CDEF course delimiter configuration data, uses a default hyphen.");
                courseDelimiter = defaultCourseDelimiter;
            }
            return courseDelimiter;
        }

        /// <summary>
        /// Retrieves the academic record configuration information needed for Colleague Self-Service
        /// </summary>
        /// <returns>The <see cref="AcademicRecordConfiguration"/></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>AcademicRecordConfiguration is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/academic-record", 1, true, Name = "GetAcademicRecordConfiguration")]
        public async Task<ActionResult<AcademicRecordConfiguration>> GetAcademicRecordConfigurationAsync()
        {
            try
            {
                var configuration = await _configurationService.GetAcademicRecordConfigurationAsync();
                return Ok(configuration);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving academic record configuration";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get the academic record configuration information.");
                return CreateHttpResponseException("Unable to get the academic record configuration information.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves course section availability information configuration
        /// </summary>
        /// <returns>The <see cref="SectionAvailabilityInformationConfiguration">Section availability information configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can retrieve section availability information configuration data.</accessComments>
        /// <note>Section availability information configuration data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/section-availability-information", 1, true, Name = "GetSectionAvailabilityInformationConfigurationAsync")]
        public async Task<ActionResult<SectionAvailabilityInformationConfiguration>> GetSectionAvailabilityInformationConfigurationAsync()
        {
            SectionAvailabilityInformationConfiguration configurationDto = null;
            try
            {
                Domain.Student.Entities.SectionAvailabilityInformationConfiguration configuration = await _configurationRepository.GetSectionAvailabilityInformationConfigurationAsync();
                var catalogConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionAvailabilityInformationConfiguration, SectionAvailabilityInformationConfiguration>();
                configurationDto = catalogConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving section availability information configuration information";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Could not retrieve section availability information configuration data.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for Colleague Self-Service faculty attendance
        /// </summary>
        /// <returns>The <see cref="FacultyAttendanceConfiguration"/></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.</exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Faculty attendance configuration data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/faculty-attendance", 1, true, Name = "GetFacultyAttendanceConfigurationAsync")]
        public async Task<ActionResult<FacultyAttendanceConfiguration>> GetFacultyAttendanceConfigurationAsync()
        {
            FacultyAttendanceConfiguration configurationDto = null;
            try
            {
                var configuration = await _configurationRepository.GetFacultyAttendanceConfigurationAsync();
                var configurationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.FacultyAttendanceConfiguration, FacultyAttendanceConfiguration>();
                configurationDto = configurationDtoAdapter.MapToType(configuration);
            }
            catch (ColleagueSessionExpiredException lex)
            {
                string message = "Session has expired while retrieving faculty attendance configuration information.";
                _logger.LogError(lex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Unable to retrieve faculty attendance configuration information.";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            return configurationDto;
        }

        /// <summary>
        /// Retrieves the section deadline dates configuration information
        /// </summary>
        /// <returns>The <see cref="SectionDeadlineDatesConfiguration"/></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Section Deadline Dates Configuration data is cached for 24 hours.</note>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [HeaderVersionRoute("/configuration/section-deadline-dates", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetSectionDeadlineDatesConfigurationV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/configuration/section-deadline-dates", 1, false, Name = "GetSectionDeadlineDatesConfiguration")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets the section deadline dates configuration information.",
            HttpMethodDescription = "Gets the section deadline dates configuration information.")]
        public async Task<ActionResult<SectionDeadlineDatesConfiguration>> GetSectionDeadlineDatesConfigurationAsync()
        {
            try
            {
                return await _configurationService.GetSectionDeadlineDatesConfigurationAsync();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving section deadline dates configuration information";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                var message = "Could not retrieve Colleague Self-Service section deadline dates configuration information";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}

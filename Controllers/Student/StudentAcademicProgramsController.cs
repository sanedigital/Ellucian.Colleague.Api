// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to StudentAcademicPrograms data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAcademicProgramsController : BaseCompressedApiController
    {
        private readonly IStudentAcademicProgramService _studentAcademicProgramService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAcademicProgramsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="StudentAcademicProgramService">Repository of type <see cref="IStudentAcademicProgramService">IStudentAcademicProgramService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public StudentAcademicProgramsController(IAdapterRegistry adapterRegistry, IStudentAcademicProgramService StudentAcademicProgramService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentAcademicProgramService = StudentAcademicProgramService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves an Student Academic Program by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.StudentAcademicPrograms">StudentAcademicPrograms</see>object.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent })]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-programs/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAcademicProgramsByGuidV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAcademicPrograms>> GetStudentAcademicProgramsByGuidAsync(string id)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                var studentAcademicProgram = await _studentAcademicProgramService.GetStudentAcademicProgramByGuidAsync(id);

                if (studentAcademicProgram != null)
                {

                    AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentAcademicProgram.Id }));
                }

                return studentAcademicProgram;

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
        /// Retrieves an Student Academic Program by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.StudentAcademicPrograms2">StudentAcademicPrograms</see>object.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent })]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-programs/{id}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAcademicProgramsByGuidV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAcademicPrograms2>> GetStudentAcademicProgramsByGuid2Async(string id)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                var studentAcademicProgram = await _studentAcademicProgramService.GetStudentAcademicProgramByGuid2Async(id);

                if (studentAcademicProgram != null)
                {

                    AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentAcademicProgram.Id }));
                }

                return studentAcademicProgram;

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
        /// Retrieves an Student Academic Program by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.StudentAcademicPrograms3">StudentAcademicPrograms</see>object.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent })]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-programs/{id}", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAcademicProgramsByGuidV16_0_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAcademicPrograms3>> GetStudentAcademicProgramsByGuid3Async(string id)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                var studentAcademicProgram = await _studentAcademicProgramService.GetStudentAcademicProgramByGuid3Async(id);

                if (studentAcademicProgram != null)
                {

                    AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentAcademicProgram.Id }));
                }
                return studentAcademicProgram;
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
        /// Retrieves an Student Academic Program by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.StudentAcademicPrograms4">StudentAcademicPrograms</see>object.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent })]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-programs/{id}", "17.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAcademicProgramsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAcademicPrograms4>> GetStudentAcademicProgramsByGuid4Async(string id)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                var studentAcademicProgram = await _studentAcademicProgramService.GetStudentAcademicProgramByGuid4Async(id);

                if (studentAcademicProgram != null)
                {

                    AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentAcademicProgram.Id }));
                }
                return studentAcademicProgram;
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
        /// Return a list of StudentAcademicPrograms objects based on selection criteria.
        /// </summary>
        ///  <param name="page">page</param>
        /// <param name="student">Id (GUID) of the student enrolled on the academic program</param>
        /// <param name="startOn">Student Academic Programs that starts on or after this date</param>
        /// <param name="endOn">Student Academic Programs that ends on or before this date</param>
        /// <param name="program">academic program Name Contains ...program...</param>
        /// <param name="catalog">Student Academic Program catalog  equal to</param>
        /// <param name="enrollmentStatus">Student Academic Program status equals to </param>
        /// <param name="programOwner">The owner of the academic program. This property represents the global identifier for the Program Owner.</param>
        /// <param name="site">	The site (campus) the student enrolls for the program at</param>
        /// <param name="academicLevel">The academic level associated with the enrollment of the student in the academic program</param>
        /// <param name="graduatedOn">The date the student graduate from the program.</param>
        /// <param name="credentials">The academic credentials that can be awarded for completing an academic program</param>
        /// <param name="graduatedAcademicPeriod">Filter to provide the academic period the student graduated in.</param>
        /// <returns>List of StudentAcademicPrograms <see cref="Dtos.StudentAcademicPrograms"/> objects representing matching Student Academic Programs</returns>
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(new string[] { "student", "startOn", "endOn", "program", "catalog", "enrollmentStatus", "programOwner", "site", "academicLevel", "graduatedOn",
            "credentials", "graduatedAcademicPeriod" }, false, true)]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-programs", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAcademicProgramsV6", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentAcademicProgramsAsync(Paging page, [FromQuery] string student = "", [FromQuery] string startOn = "", [FromQuery] string endOn = "",
            [FromQuery] string program = "", [FromQuery] string catalog = "", [FromQuery] string enrollmentStatus = "", [FromQuery] string programOwner = "", [FromQuery] string site = "",
            [FromQuery] string academicLevel = "", [FromQuery] string graduatedOn = "", [FromQuery] string credentials = "", [FromQuery] string graduatedAcademicPeriod = "")
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            // Force dates to be YYYY-MM-DD
            try
            {
                if (!string.IsNullOrEmpty(startOn))
                {
                    var test  = DateTime.ParseExact(startOn, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).ToString();
                }
            }
            catch (Exception e)
            {
                var errmsg = "Invalid Date format: startOn: " + startOn + ", filter requires YYYY-MM-DD.";
                _logger.LogError(errmsg);
                return CreateHttpResponseException(new IntegrationApiException(errmsg, IntegrationApiUtility.GetDefaultApiError(errmsg)));
            }
            try
            {
                if (!string.IsNullOrEmpty(endOn))
                {
                    var test = DateTime.ParseExact(endOn, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).ToString();
                }
            }
            catch (Exception e)
            {
                var errmsg = "Invalid Date format: endOn: " + endOn + ", filter requires YYYY-MM-DD.";
                _logger.LogError(errmsg);
                return CreateHttpResponseException(new IntegrationApiException(errmsg, IntegrationApiUtility.GetDefaultApiError(errmsg)));
            }

            try
            {
                if (!string.IsNullOrEmpty(graduatedOn))
                {
                    var test = DateTime.ParseExact(graduatedOn, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).ToString();
                }
            }
            catch (Exception e)
            {
                var errmsg = "Invalid Date format: graduatedOn: " + graduatedOn + ", filter requires YYYY-MM-DD.";
                _logger.LogError(errmsg);
                return CreateHttpResponseException(new IntegrationApiException(errmsg, IntegrationApiUtility.GetDefaultApiError(errmsg)));
            }


            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                if ((!string.IsNullOrEmpty(enrollmentStatus)) && (!ValidEnumerationValue(typeof(EnrollmentStatusType), enrollmentStatus)))
                {
                    throw new ColleagueWebApiException(string.Concat("'", enrollmentStatus, "' is an invalid enumeration value. "));
                }

                var pageOfItems = await _studentAcademicProgramService.GetStudentAcademicProgramsAsync(page.Offset, page.Limit, bypassCache, student, startOn, endOn, program,
                   catalog, enrollmentStatus, programOwner, site, academicLevel, graduatedOn, credentials, graduatedAcademicPeriod);

                AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentAcademicPrograms>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch(InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Return a list of StudentAcademicPrograms objects based on selection criteria.
        /// </summary>
        ///  <param name="page">page</param>
        /// <param name="criteria">filter criteria</param>
        /// <returns>List of StudentAcademicPrograms <see cref="Dtos.StudentAcademicPrograms2"/> objects representing matching Student Academic Programs</returns>
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.StudentAcademicProgramsFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-programs", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAcademicProgramsV11", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentAcademicPrograms2Async(Paging page, QueryStringFilter criteria = null)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            //do the filters
            string program = string.Empty, student = string.Empty, site = string.Empty, academicLevel = string.Empty,  startOn = string.Empty,  endOn = string.Empty,
                enrollmentStatus = string.Empty, graduatedAcademicPeriod = string.Empty, graduatedOn = string.Empty;
            List<string> credentials = new List<string>();

            var criteriaObj = GetFilterObject<Dtos.Filters.StudentAcademicProgramsFilter>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.StudentAcademicPrograms2>>(new List<Dtos.StudentAcademicPrograms2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            if (criteriaObj != null)
            {
                student = (criteriaObj.Student != null && !string.IsNullOrEmpty(criteriaObj.Student.Id)) ? criteriaObj.Student.Id : string.Empty;
                program = (criteriaObj.AcademicProgram != null && !string.IsNullOrEmpty(criteriaObj.AcademicProgram.Id)) ? criteriaObj.AcademicProgram.Id : string.Empty;
                site = (criteriaObj.Site != null && !string.IsNullOrEmpty(criteriaObj.Site.Id)) ? criteriaObj.Site.Id : string.Empty;
                academicLevel = (criteriaObj.AcademicLevel != null && !string.IsNullOrEmpty(criteriaObj.AcademicLevel.Id)) ? criteriaObj.AcademicLevel.Id : string.Empty;

                if (criteriaObj.Credentials != null && criteriaObj.Credentials.Any())
                {
                    criteriaObj.Credentials.ToList().ForEach(i =>
                    {
                        credentials.Add(i.Id);
                    });
                }

                // Force dates to be YYYY-MM-DD
                try
                {
                    if (!string.IsNullOrEmpty(criteriaObj.StartOn))
                    {
                        startOn = DateTime.ParseExact(criteriaObj.StartOn, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).ToString();
                    }
                }
                catch (Exception e)
                {
                    var errmsg = "Invalid Date format: startOn: " + criteriaObj.StartOn + ", filter requires YYYY-MM-DD.";
                    _logger.LogError(errmsg);
                    return CreateHttpResponseException(new IntegrationApiException(errmsg, IntegrationApiUtility.GetDefaultApiError(errmsg)));
                }
                try
                {
                    if (!string.IsNullOrEmpty(criteriaObj.EndOn))
                    {
                        endOn = DateTime.ParseExact(criteriaObj.EndOn, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).ToString();
                    }
                }
                catch (Exception e)
                {
                    var errmsg = "Invalid Date format: endOn: " + criteriaObj.EndOn + ", filter requires YYYY-MM-DD.";
                    _logger.LogError(errmsg);
                    return CreateHttpResponseException(new IntegrationApiException(errmsg, IntegrationApiUtility.GetDefaultApiError(errmsg)));
                }

                try
                {
                    if (!string.IsNullOrEmpty(criteriaObj.GraduatedOn))
                    {
                        graduatedOn = DateTime.ParseExact(criteriaObj.GraduatedOn, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).ToString();
                    }
                }
                catch (Exception e)
                {
                    var errmsg = "Invalid Date format: graduatedOn: " + criteriaObj.GraduatedOn + ", filter requires YYYY-MM-DD.";
                    _logger.LogError(errmsg);
                    return CreateHttpResponseException(new IntegrationApiException(errmsg, IntegrationApiUtility.GetDefaultApiError(errmsg)));
                }

                enrollmentStatus = criteriaObj.EnrollmentStatus != null ? criteriaObj.EnrollmentStatus.EnrollStatus.ToString() : string.Empty;

                if(criteriaObj.AcademicPeriods != null && criteriaObj.AcademicPeriods.ActualGraduation != null && !string.IsNullOrEmpty(criteriaObj.AcademicPeriods.ActualGraduation.Id))
                {
                    graduatedAcademicPeriod = criteriaObj.AcademicPeriods.ActualGraduation.Id;
                }
                else if (criteriaObj.ActualGraduation != null && !string.IsNullOrEmpty(criteriaObj.ActualGraduation.Id))
                {
                    graduatedAcademicPeriod = criteriaObj.ActualGraduation.Id;
                }
            }

            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _studentAcademicProgramService.GetStudentAcademicPrograms2Async(page.Offset, page.Limit, bypassCache, student, startOn, endOn, program,
                    enrollmentStatus, site, academicLevel, graduatedOn, credentials, graduatedAcademicPeriod);

                AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentAcademicPrograms2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Return a list of StudentAcademicPrograms objects based on selection criteria.
        /// </summary>
        ///  <param name="page">page</param>
        /// <param name="criteria">filter criteria</param>
        /// <returns>List of StudentAcademicPrograms <see cref="Dtos.StudentAcademicPrograms3"/> objects representing matching Student Academic Programs</returns>
        [HttpGet, ]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAcademicPrograms3))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-programs", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAcademicProgramsV16_0_0", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentAcademicPrograms3Async(Paging page, QueryStringFilter criteria = null)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            //do the filters
            string program = string.Empty, student = string.Empty, site = string.Empty, academicLevel = string.Empty, startOn = string.Empty, endOn = string.Empty,
                enrollmentStatus = string.Empty, graduatedAcademicPeriod = string.Empty, graduatedOn = string.Empty;
            List<string> credentials = new List<string>();

            var criteriaObj = GetFilterObject<Dtos.StudentAcademicPrograms3>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.StudentAcademicPrograms3>>(new List<Dtos.StudentAcademicPrograms3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _studentAcademicProgramService.GetStudentAcademicPrograms3Async(page.Offset, page.Limit, criteriaObj, bypassCache);

                AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentAcademicPrograms3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Return a list of StudentAcademicPrograms objects based on selection criteria.
        /// </summary>
        ///  <param name="page">page</param>
        /// <param name="criteria">filter criteria</param>
        /// <param name="personFilter">person filter criteria</param>
        /// <returns>List of StudentAcademicPrograms <see cref="Dtos.StudentAcademicPrograms4"/> objects representing matching Student Academic Programs</returns>
        [HttpGet,]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAcademicPrograms4))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-programs", "17.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAcademicPrograms", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentAcademicPrograms4Async(Paging page, QueryStringFilter criteria = null, QueryStringFilter personFilter= null)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            //do the filters

            string personFilterValue = string.Empty;
            var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
            if ((personFilterObj != null) && (personFilterObj.personFilter != null))
            {
                personFilterValue = personFilterObj.personFilter.Id;
            }

            string program = string.Empty, student = string.Empty, site = string.Empty, academicLevel = string.Empty, startOn = string.Empty, endOn = string.Empty,
                enrollmentStatus = string.Empty, graduatedAcademicPeriod = string.Empty, graduatedOn = string.Empty;
            List<string> credentials = new List<string>();

            var criteriaObj = GetFilterObject<Dtos.StudentAcademicPrograms4>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.StudentAcademicPrograms4>>(new List<Dtos.StudentAcademicPrograms4>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _studentAcademicProgramService.GetStudentAcademicPrograms4Async(page.Offset, page.Limit, criteriaObj, personFilterValue, bypassCache);

                AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentAcademicPrograms4>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Creates an Student Academic Program.
        /// </summary>
        /// <param name="StudentAcademicPrograms"><see cref="Dtos.StudentAcademicPrograms">StudentAcademicPrograms</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.StudentAcademicPrograms">StudentAcademicPrograms</see></returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent)]
        [HeaderVersionRoute("/student-academic-programs", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAcademicProgramsV6", IsEedmSupported = true)]
        public async Task<IActionResult> CreateStudentAcademicProgramsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAcademicPrograms StudentAcademicPrograms)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (StudentAcademicPrograms == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null StudentAcademicPrograms argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(StudentAcademicPrograms.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null GUID ",
                    IntegrationApiUtility.GetDefaultApiError("Null GUID is required in request body.")));
            }
            if (StudentAcademicPrograms.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException(new IntegrationApiException(" Not a Null GUID ",
                    IntegrationApiUtility.GetDefaultApiError("On a post, you can not define a GUID")));
            }
            var validationResult = await ValidateStudentAcademicPrograms(StudentAcademicPrograms);
            if (validationResult != null)
            {
                return validationResult;
            }
            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _studentAcademicProgramService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAcademicProgramService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the student academic programs
                var studentAcademicProgram = await _studentAcademicProgramService.CreateStudentAcademicProgramAsync(StudentAcademicPrograms, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentAcademicProgram.Id }));

                return Ok(studentAcademicProgram);
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
        /// Creates an Student Academic Program.
        /// </summary>
        /// <param name="StudentAcademicPrograms"><see cref="Dtos.StudentAcademicPrograms2">StudentAcademicPrograms</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.StudentAcademicPrograms2">StudentAcademicPrograms</see></returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent)]
        [HeaderVersionRoute("/student-academic-programs", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAcademicProgramsV11", IsEedmSupported = true)]
        public async Task<IActionResult> CreateStudentAcademicPrograms2Async([ModelBinder(typeof(EedmModelBinder))]  Dtos.StudentAcademicPrograms2 StudentAcademicPrograms)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (StudentAcademicPrograms == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null StudentAcademicPrograms argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(StudentAcademicPrograms.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null GUID ",
                    IntegrationApiUtility.GetDefaultApiError("Null GUID is required in request body.")));
            }
            if (StudentAcademicPrograms.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException(new IntegrationApiException(" Not a Null GUID ",
                    IntegrationApiUtility.GetDefaultApiError("On a post, you can not define a GUID")));
            }
            if (!StudentAcademicPrograms.CurriculumObjective.HasValue)
            {
                return CreateHttpResponseException(new IntegrationApiException("Missing Curriculum Objective",
                    IntegrationApiUtility.GetDefaultApiError("Curriculum objective is required")));
            }
            var validationResult = await ValidateStudentAcademicPrograms2(StudentAcademicPrograms);
            if (validationResult != null)
            {
                return validationResult;
            }
            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _studentAcademicProgramService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAcademicProgramService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the student academic programs
                var studentAcademicProgram = await _studentAcademicProgramService.CreateStudentAcademicProgram2Async(StudentAcademicPrograms, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentAcademicProgram.Id }));

                return Ok(studentAcademicProgram);
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
        /// Creates an Student Academic Program.
        /// </summary>
        /// <param name="StudentAcademicPrograms"><see cref="Dtos.StudentAcademicPrograms2">StudentAcademicPrograms</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.StudentAcademicPrograms3">StudentAcademicPrograms</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/student-academic-programs", "17.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAcademicProgramsV17_0_0")]
        [HeaderVersionRoute("/student-academic-programs", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAcademicProgramsV16_0_0")]
        public async Task<ActionResult<Dtos.StudentAcademicPrograms3>> CreateStudentAcademicPrograms3Async(Dtos.StudentAcademicPrograms3 StudentAcademicPrograms)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }


        /// <summary>
        /// Updates an Student Academic Program.
        /// </summary>
        /// <param name="id">Id of the Student Academic Program to update</param>
        /// <param name="studentAcademicPrograms"><see cref="Dtos.StudentAcademicPrograms">StudentAcademicPrograms</see> to create</param>
        /// <returns>Updated <see cref="Dtos.StudentAcademicPrograms">StudentAcademicPrograms</see></returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent)]
        [HeaderVersionRoute("/student-academic-programs/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAcademicPrograms", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAcademicPrograms>> UpdateStudentAcademicProgramsAsync([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAcademicPrograms studentAcademicPrograms)
        {

            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentAcademicPrograms == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null StudentAcademicPrograms argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(studentAcademicPrograms.Id))
            {
                studentAcademicPrograms.Id = id.ToLowerInvariant();
            }
            if (!id.Equals(studentAcademicPrograms.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
            if (string.Equals(studentAcademicPrograms.Id, Guid.Empty.ToString()))
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid GUID ",
              IntegrationApiUtility.GetDefaultApiError("The null GUID is not valid")));
            }

            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _studentAcademicProgramService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _studentAcademicProgramService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAcademicProgramService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic

                var mergedStudentAcadProgram = await PerformPartialPayloadMerge(studentAcademicPrograms, async () => await _studentAcademicProgramService.GetStudentAcademicProgramByGuidAsync(id),
                    dpList, _logger);
                await ValidateStudentAcademicPrograms(mergedStudentAcadProgram);
                var studentAcademicProgramReturn = await _studentAcademicProgramService.UpdateStudentAcademicProgramAsync(mergedStudentAcadProgram, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return studentAcademicProgramReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Updates an Student Academic Program.
        /// </summary>
        /// <param name="id">Id of the Student Academic Program to update</param>
        /// <param name="studentAcademicPrograms"><see cref="Dtos.StudentAcademicPrograms">StudentAcademicPrograms</see> to create</param>
        /// <returns>Updated <see cref="Dtos.StudentAcademicPrograms2">StudentAcademicPrograms</see></returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent)]
        [HeaderVersionRoute("/student-academic-programs/{id}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAcademicProgramsV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAcademicPrograms2>> UpdateStudentAcademicPrograms2Async([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAcademicPrograms2 studentAcademicPrograms)
        {

            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentAcademicPrograms == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null StudentAcademicPrograms argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(studentAcademicPrograms.Id))
            {
                studentAcademicPrograms.Id = id.ToLowerInvariant();
            }
            if (!id.Equals(studentAcademicPrograms.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
            if (string.Equals(studentAcademicPrograms.Id, Guid.Empty.ToString()))
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid GUID ",
              IntegrationApiUtility.GetDefaultApiError("The null GUID is not valid")));
            }

            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _studentAcademicProgramService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _studentAcademicProgramService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAcademicProgramService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var mergedStudentAcadProgram = await PerformPartialPayloadMerge(studentAcademicPrograms, async () => await _studentAcademicProgramService.GetStudentAcademicProgramByGuid2Async(id),
                        dpList, _logger);
                var validationResult = await ValidateStudentAcademicPrograms2(mergedStudentAcadProgram);
                if (validationResult != null)
                {
                    return Ok(validationResult);
                }
                var studentAcademicProgramReturn = await _studentAcademicProgramService.UpdateStudentAcademicProgram2Async(mergedStudentAcadProgram, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return studentAcademicProgramReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Updates an Student Academic Program.
        /// </summary>
        /// <param name="id">Id of the Student Academic Program to update</param>
        /// <param name="studentAcademicPrograms"><see cref="Dtos.StudentAcademicPrograms">StudentAcademicPrograms</see> to create</param>
        /// <returns>Updated <see cref="Dtos.StudentAcademicPrograms3">StudentAcademicPrograms</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/student-academic-programs/{id}", "17.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAcademicProgramsV17_0_0")]
        [HeaderVersionRoute("/student-academic-programs/{id}", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAcademicProgramsV16_0_0")]
        public async Task<ActionResult<Dtos.StudentAcademicPrograms3>> UpdateStudentAcademicPrograms3Async([FromRoute] string id, [FromBody] Dtos.StudentAcademicPrograms3 studentAcademicPrograms)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }


        /// <summary>
        /// Delete an existing student academic programs
        /// </summary>
        /// <param name="id">Employee GUID for update.</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        [HttpDelete]
        [Route("/student-academic-programs/{id}", Name = "DefaultDeleteStudentAcademicPrograms", Order = -10)]
        public async Task<IActionResult> DeleteStudentAcademicProgramsAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #region submissions
                 
        /// <summary>
        /// Updates an Student Academic Program.
        /// </summary>
        /// <param name="id">Id of the Student Academic Program to update</param>
        /// <param name="studentAcademicPrograms"><see cref="Dtos.StudentAcademicPrograms">StudentAcademicPrograms</see> to create</param>
        /// <returns>Updated <see cref="Dtos.StudentAcademicPrograms4">StudentAcademicPrograms</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent)]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationStudentAcademicProgramSubmissionsFormat },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/student-academic-programs/{id}", "17.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultPutStudentAcademicProgramsSubmissionsV1.0.0", Order = -20)]
        public async Task<ActionResult<Dtos.StudentAcademicPrograms4>> UpdateStudentAcademicProgramsSubmissionsAsync([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAcademicProgramsSubmissions studentAcademicPrograms)
        {

            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Null StudentAcademicProgramsSubmissions guid", "guid is a required property.");
                }
                if (studentAcademicPrograms == null)
                {
                    throw new ArgumentNullException("Null StudentAcademicProgramsSubmissions argument", "The request body is required.");
                }
                if (string.IsNullOrEmpty(studentAcademicPrograms.Id))
                {
                    studentAcademicPrograms.Id = id.ToLowerInvariant();
                }
                if (!id.Equals(studentAcademicPrograms.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("GUID not the same as in request body.");
                }
                if (string.Equals(studentAcademicPrograms.Id, Guid.Empty.ToString()))
                {
                    throw new InvalidOperationException("Nil GUID cannot be used in PUT operation.");
                }


                //get Data Privacy List
                var dpList = await _studentAcademicProgramService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _studentAcademicProgramService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAcademicProgramService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var mergedStudentAcadProgram = await PerformPartialPayloadMerge(studentAcademicPrograms, async () => await _studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(id),
                        dpList, _logger);
                var studentAcademicProgramReturn = await _studentAcademicProgramService.UpdateStudentAcademicProgramSubmissionAsync(mergedStudentAcadProgram, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return studentAcademicProgramReturn;
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
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
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
        /// Creates an Student Academic Program.
        /// </summary>
        /// <param name="StudentAcademicPrograms"><see cref="Dtos.StudentAcademicPrograms2">StudentAcademicPrograms</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.StudentAcademicPrograms2">StudentAcademicPrograms</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent)]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationStudentAcademicProgramSubmissionsFormat },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/student-academic-programs", "17.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultPostStudentAcademicProgramsSubmissionsV1.0.0", Order = -20)]
        public async Task<ActionResult<Dtos.StudentAcademicPrograms4>> CreateStudentAcademicProgramsSubmissionsAsync([ModelBinder(typeof(EedmModelBinder))]  Dtos.StudentAcademicProgramsSubmissions StudentAcademicPrograms)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                if (StudentAcademicPrograms == null)
                {
                    throw new ArgumentNullException("Null StudentAcademicProgramsSubmissions argument", "The request body is required.");
                }
                if (string.IsNullOrEmpty(StudentAcademicPrograms.Id))
                {
                    throw new ArgumentNullException("Null StudentAcademicProgramsSubmissions guid", "guid is a required property.");
                }
                if (StudentAcademicPrograms.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("Not null StudentAcademicProgramsSubmissions guid", "On a post, you can not define a GUID");
                }


                //call import extend method that needs the extracted extension data and the config
                await _studentAcademicProgramService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAcademicProgramService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the student academic programs
                var studentAcademicProgram = await _studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(StudentAcademicPrograms, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentAcademicProgram.Id }));

                return studentAcademicProgram;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
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



        #endregion

        #region Replacement

        /// <summary>
        /// Creates an Student Academic Program.
        /// </summary>
        /// <param name="StudentAcademicPrograms"><see cref="Dtos.StudentAcademicPrograms4">StudentAcademicPrograms</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.StudentAcademicPrograms4">StudentAcademicPrograms</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ReplaceStudentAcademicProgram)]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationStudentAcademicProgramReplacements },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/student-academic-programs", "17.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultStudentAcademicProgramsReplacementsV1.0.0", Order = -20)]
        public async Task<ActionResult<Dtos.StudentAcademicPrograms4>> CreateStudentAcademicProgramsReplacementsAsync([ModelBinder(typeof(EedmModelBinder))]  Dtos.StudentAcademicProgramReplacements StudentAcademicPrograms)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                _studentAcademicProgramService.ValidatePermissions(GetPermissionsMetaData());
                if (StudentAcademicPrograms == null)
                {
                    throw new ArgumentNullException("Null StudentAcademicProgramsSubmissions argument", "The request body is required.");
                }
                if (string.IsNullOrEmpty(StudentAcademicPrograms.Id))
                {
                    throw new ArgumentNullException("Null StudentAcademicProgramsSubmissions guid", "guid is a required property.");
                }
                if (StudentAcademicPrograms.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("Not null StudentAcademicProgramsSubmissions guid", "On a post, you can not define a GUID");
                }


                //call import extend method that needs the extracted extension data and the config
                await _studentAcademicProgramService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAcademicProgramService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the student academic programs
                var studentAcademicProgram = await _studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(StudentAcademicPrograms, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAcademicProgramService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAcademicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentAcademicProgram.Id }));

                return studentAcademicProgram;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
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
        /// Update academic program replacement.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [HttpGet]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationStudentAcademicProgramReplacements },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/student-academic-programs/{guid}", "17.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAcademicProgramsReplacementsByGuidV1.0.0", Order = -20)]
        public async Task<ActionResult<Dtos.StudentTranscriptGrades>> GetAcademicProgramsReplacementsByGuidAsync([FromRoute] string guid)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update academic program replacement.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <param name="personFilter"></param>
        /// <returns></returns>
        [HttpGet]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationStudentAcademicProgramReplacements },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/student-academic-programs", "17.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAcademicProgramsReplacementsV1.0.0", Order = -20)]
        public async Task<ActionResult<Dtos.StudentTranscriptGrades>> GetAcademicProgramsReplacementsAsync(Paging page, QueryStringFilter criteria = null, QueryStringFilter personFilter = null)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update academic program replacement.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="studentAcademicProgramReplacements"></param>
        /// <returns></returns>
        [HttpPut]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationStudentAcademicProgramReplacements },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/student-academic-programs/{guid}", "17.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultPutStudentAcademicProgramsReplacementsV1.0.0", Order = -20)]
        public async Task<ActionResult<Dtos.StudentTranscriptGrades>> PutAcademicProgramsReplacementsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAcademicProgramReplacements studentAcademicProgramReplacements)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion


        /// <summary>
        /// Validates the data in the StudentAcademicPrograms object
        /// </summary>
        /// <param name="stuAcadProg">StudentAcademicPrograms from the request</param>
        private async Task<IActionResult> ValidateStudentAcademicPrograms(StudentAcademicPrograms stuAcadProg)
        {


            if (stuAcadProg.Program == null || string.IsNullOrEmpty(stuAcadProg.Program.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null program argument",
                    IntegrationApiUtility.GetDefaultApiError("The program ID is required.")));
            }
            if (stuAcadProg.Student == null || string.IsNullOrEmpty(stuAcadProg.Student.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null student argument",
                IntegrationApiUtility.GetDefaultApiError("The Student ID is required.")));
            }
            //if (stuAcadProg.StartDate == default(DateTime))
            if (stuAcadProg.StartDate == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null start date argument",
                    IntegrationApiUtility.GetDefaultApiError("The Start Date is required.")));
            }
            if (stuAcadProg.EnrollmentStatus != null && string.IsNullOrEmpty(stuAcadProg.EnrollmentStatus.EnrollStatus.ToString()))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null enrollment status",
                    IntegrationApiUtility.GetDefaultApiError("The enrollment status is required.")));
            }
            //check end date is not before start date
            if (stuAcadProg.EndDate != null && stuAcadProg.EndDate < stuAcadProg.StartDate)
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid End Date",
                    IntegrationApiUtility.GetDefaultApiError("Student Academic Program end date cannot be before start date.")));
            }
            //check graduation date is not before start date
            if (stuAcadProg.GraduatedOn != null && stuAcadProg.GraduatedOn < stuAcadProg.StartDate)
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid Graduation Date",
                    IntegrationApiUtility.GetDefaultApiError("Student Academic Program graduation date cannot be before start date.")));
            }
            //check credentials is not before start date
            if (stuAcadProg.CredentialsDate != null && stuAcadProg.CredentialsDate < stuAcadProg.StartDate)
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid Credentials Date",
                    IntegrationApiUtility.GetDefaultApiError("Student Academic Program credential date cannot be before start date.")));
            }
            //if the enrollment status is inactive, then the end date is required.
            if (stuAcadProg.EndDate == null && stuAcadProg.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Inactive)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null End Date",
                    IntegrationApiUtility.GetDefaultApiError("End date is required for the enrollment status of inactive.")));
            }
            // the status of complete is not valid for PUT/POST
            if (stuAcadProg.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Complete)
            {
                return CreateHttpResponseException(new IntegrationApiException("Incorrect EnrollmentStatus",
                    IntegrationApiUtility.GetDefaultApiError("The enrollment status of complete is not supported.")));
            }

            //the status of active cannot have end date
            if (stuAcadProg.EndDate != null && stuAcadProg.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Active)
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid End Date",
                    IntegrationApiUtility.GetDefaultApiError("End date is not valid for the enrollment status of active.")));
            }
            //check the credentials body is good.
            if (stuAcadProg.Credentials != null && stuAcadProg.Credentials.Count > 0)
            {
                foreach (var cred in stuAcadProg.Credentials)
                {
                    if (cred == null || string.IsNullOrEmpty(cred.Id))
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid credentials",
                            IntegrationApiUtility.GetDefaultApiError("Credential id is a required field when credentials are in the message body.")));
                    }
                }
            }
            //check the recognitions body is good.
            if (stuAcadProg.Recognitions != null && stuAcadProg.Recognitions.Count > 0)
            {
                foreach (var honor in stuAcadProg.Recognitions)
                {
                    if (honor == null || string.IsNullOrEmpty(honor.Id))
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid recognitions",
                            IntegrationApiUtility.GetDefaultApiError("Recognition id is a required field when recognitions are in the message body.")));
                    }
                }
            }

            //check displines body is good.
            if (stuAcadProg.Disciplines != null && stuAcadProg.Disciplines.Count > 0)
            {
                foreach (var dis in stuAcadProg.Disciplines)
                {
                    if (dis.Discipline == null)
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid disciplines",
                           IntegrationApiUtility.GetDefaultApiError("Discipline is a required field when disciplines are in the message body.")));
                    }
                    else if (string.IsNullOrEmpty(dis.Discipline.Id))
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid discipline",
                           IntegrationApiUtility.GetDefaultApiError("Discipline id is a required field when discipline is in the message body.")));
                    }
                    else if (string.IsNullOrEmpty(dis.AdministeringInstitutionUnit.Id))
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid discipline",
                           IntegrationApiUtility.GetDefaultApiError("Administering Institution Unit id is a required field when Administering Institution Unit is in the message body.")));
                    }
                    else if (dis.SubDisciplines != null)
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid Subdisciplines",
                           IntegrationApiUtility.GetDefaultApiError("Subdisciplines are not supported.")));
                    }
                }
            }

            return null;

        }

        /// <summary>
        /// Validates the data in the StudentAcademicPrograms object
        /// </summary>
        /// <param name="stuAcadProg">StudentAcademicPrograms from the request</param>
        private async Task<IActionResult> ValidateStudentAcademicPrograms2(StudentAcademicPrograms2 stuAcadProg)
        {
            if (stuAcadProg.AcademicProgram == null || string.IsNullOrEmpty(stuAcadProg.AcademicProgram.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null program argument",
                    IntegrationApiUtility.GetDefaultApiError("The program ID is required.")));
            }
            if (stuAcadProg.Student == null || string.IsNullOrEmpty(stuAcadProg.Student.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null student argument",
                IntegrationApiUtility.GetDefaultApiError("The Student ID is required.")));
            }
            //validate curriculumObjective
            if (stuAcadProg.CurriculumObjective == Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective.NotSet)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null curriculumObjective argument",
                    IntegrationApiUtility.GetDefaultApiError("The curriculum objective is required.")));
            }
            else
            {
                //if it is outcome then there needs to be graduation date. 
                if (stuAcadProg.CurriculumObjective == Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective.Outcome && stuAcadProg.CredentialsDate == null)
                {
                    return CreateHttpResponseException(new IntegrationApiException("Invalid curriculumObjective argument",
                        IntegrationApiUtility.GetDefaultApiError("Crendentials Date is required for the curriculum objective of type of 'outcome'.")));
                }
            }
            //validate preference
            if (stuAcadProg.StartDate == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null start date argument",
                    IntegrationApiUtility.GetDefaultApiError("The Start Date is required.")));
            }
            if (stuAcadProg.EnrollmentStatus != null && string.IsNullOrEmpty(stuAcadProg.EnrollmentStatus.EnrollStatus.ToString()))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null enrollment status",
                    IntegrationApiUtility.GetDefaultApiError("The enrollment status is required.")));
            }
            //check end date is not before start date
            if (stuAcadProg.EndDate != null && stuAcadProg.EndDate < stuAcadProg.StartDate)
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid End Date",
                    IntegrationApiUtility.GetDefaultApiError("Student Academic Program end date cannot be before start date.")));
            }
            //check exptected graduation date is not before start date
            if (stuAcadProg.ExpectedGraduationDate != null && stuAcadProg.ExpectedGraduationDate < stuAcadProg.StartDate)
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid Expected Graduation Date",
                    IntegrationApiUtility.GetDefaultApiError("Student Academic Program exptected graduation date cannot be before start date.")));
            }
            //check graduation date is not before start date
            if (stuAcadProg.GraduatedOn != null && stuAcadProg.GraduatedOn < stuAcadProg.StartDate)
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid Graduation Date",
                    IntegrationApiUtility.GetDefaultApiError("Student Academic Program graduation date cannot be before start date.")));
            }
            //check credentials is not before start date
            if (stuAcadProg.CredentialsDate != null && stuAcadProg.CredentialsDate < stuAcadProg.StartDate)
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid Credentials Date",
                    IntegrationApiUtility.GetDefaultApiError("Student Academic Program credential date cannot be before start date.")));
            }
            //if the enrollment status is inactive, then the end date is required.
            if (stuAcadProg.EndDate == null && stuAcadProg.EnrollmentStatus != null && stuAcadProg.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Inactive)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null End Date",
                    IntegrationApiUtility.GetDefaultApiError("End date is required for the enrollment status of inactive.")));
            }
            // the status of complete is not valid for PUT/POST
            if (stuAcadProg.EnrollmentStatus != null && stuAcadProg.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Complete)
            {
                return CreateHttpResponseException(new IntegrationApiException("Incorrect EnrollmentStatus",
                    IntegrationApiUtility.GetDefaultApiError("The enrollment status of complete is not supported. Graduation processing can only be invoked directly in Colleague.")));
            }

            // the preference of primary is not supported for PUT/POST
            if (stuAcadProg.Preference == Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary)
            {
                return CreateHttpResponseException(new IntegrationApiException("Incorrect EnrollmentStatus",
                    IntegrationApiUtility.GetDefaultApiError("The preference attribute is not supported.")));
            }

            //the status of active cannot have end date
            if (stuAcadProg.EndDate != null && stuAcadProg.EnrollmentStatus != null && stuAcadProg.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Active)
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid End Date",
                    IntegrationApiUtility.GetDefaultApiError("End date is not valid for the enrollment status of active.")));
            }
            //check the credentials body is good.
            if (stuAcadProg.Credentials != null && stuAcadProg.Credentials.Count > 0)
            {
                foreach (var cred in stuAcadProg.Credentials)
                {
                    if (cred == null || string.IsNullOrEmpty(cred.Id))
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid credentials",
                            IntegrationApiUtility.GetDefaultApiError("Credential id is a required field when credentials are in the message body.")));
                    }
                }
            }
            //check the recognitions body is good.
            if (stuAcadProg.Recognitions != null && stuAcadProg.Recognitions.Count > 0)
            {
                foreach (var honor in stuAcadProg.Recognitions)
                {
                    if (honor == null || string.IsNullOrEmpty(honor.Id))
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid recognitions",
                            IntegrationApiUtility.GetDefaultApiError("Recognition id is a required field when recognitions are in the message body.")));
                    }
                }
            }

            //check displines body is good.
            if (stuAcadProg.Disciplines != null && stuAcadProg.Disciplines.Count > 0)
            {
                foreach (var dis in stuAcadProg.Disciplines)
                {
                    if (dis.Discipline == null)
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid disciplines",
                           IntegrationApiUtility.GetDefaultApiError("Discipline is a required field when disciplines are in the message body.")));
                    }
                    else if (string.IsNullOrEmpty(dis.Discipline.Id))
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid discipline",
                           IntegrationApiUtility.GetDefaultApiError("Discipline id is a required field when discipline is in the message body.")));
                    }
                    else if (dis.AdministeringInstitutionUnit != null && string.IsNullOrEmpty(dis.AdministeringInstitutionUnit.Id))
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid discipline",
                           IntegrationApiUtility.GetDefaultApiError("Administering Institution Unit id is a required field when Administering Institution Unit is in the message body.")));
                    }
                    else if (dis.SubDisciplines != null)
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Invalid Subdisciplines",
                           IntegrationApiUtility.GetDefaultApiError("Subdisciplines are not supported.")));
                    }
                }
            }

            return null;
        }
    }
}

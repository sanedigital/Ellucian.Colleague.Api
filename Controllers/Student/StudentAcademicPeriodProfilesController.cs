// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using System.Threading.Tasks;
using System;
using System.Linq;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using System.Net;

using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAcademicPeriodProfiles data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAcademicPeriodProfilesController : BaseCompressedApiController
    {
        private readonly IStudentAcademicPeriodProfilesService _studentAcademicPeriodProfilesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAcademicPeriodProfilesController class.
        /// </summary>
        /// <param name="studentAcademicPeriodProfilesService">Repository of type <see cref="IStudentAcademicPeriodProfilesService">IStudentAcademicPeriodProfilesService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public StudentAcademicPeriodProfilesController(IStudentAcademicPeriodProfilesService studentAcademicPeriodProfilesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentAcademicPeriodProfilesService = studentAcademicPeriodProfilesService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves an Student Academic Period Profiles by ID.
        /// </summary>
        /// <returns>An <see cref="StudentAcademicPeriodProfiles">StudentAcademicPeriodProfiles</see>object.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicPeriodProfile)]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-period-profiles/{id}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAcademicPeriodProfileByGuid", IsEedmSupported = true)]
        [HeaderVersionRoute("/student-academic-period-profiles/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAcademicPeriodProfileByGuidV7", IsEedmSupported = true)]
        public async Task<ActionResult<StudentAcademicPeriodProfiles>> GetStudentAcademicPeriodProfileByGuidAsync(string id)
        {
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
                _studentAcademicPeriodProfilesService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties((await _studentAcademicPeriodProfilesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)),
                    await _studentAcademicPeriodProfilesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return await _studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfileByGuidAsync(id);
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
        /// Return a list of StudentAcademicPeriodProfiles objects based on selection criteria.
        /// </summary>
        ///  <param name="page">page</param>
        /// <param name="person">Id (GUID) A reference to link a student to the common HEDM persons entity</param>     
        /// <param name="academicPeriod">Id (GUID) A term within an academic year (for example, Semester).</param>
        /// <returns>List of StudentAcademicPeriodProfiles <see cref="StudentAcademicPeriodProfiles"/> objects representing matching Student Academic Period Profiles</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicPeriodProfile)]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 50 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(new string[] { "person", "academicPeriod" }, false, true)]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-period-profiles", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAcademicPeriodProfilesV7", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentAcademicPeriodProfilesAsync(Paging page, [FromQuery] string person = "", [FromQuery] string academicPeriod = "")
        {
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
                _studentAcademicPeriodProfilesService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(50, 0);
                }                

                var pageOfItems = await _studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(page.Offset, page.Limit, bypassCache, person, academicPeriod);

                AddEthosContextProperties((await _studentAcademicPeriodProfilesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)),
                    await _studentAcademicPeriodProfilesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<StudentAcademicPeriodProfiles>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Return a list of StudentAcademicPeriodProfiles objects based on selection criteria.
        /// </summary>
        ///  <param name="page">page</param>
        /// <param name="criteria"> - JSON formatted selection criteria.  Can contain:</param>
        /// <returns>List of StudentAcademicPeriodProfiles <see cref="StudentAcademicPeriodProfiles"/> objects representing matching Student Academic Period Profiles</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicPeriodProfile)]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 50 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAcademicPeriodProfiles))]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-period-profiles", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAcademicPeriodProfiles", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentAcademicPeriodProfiles2Async(Paging page, QueryStringFilter criteria = null)
        {
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
                _studentAcademicPeriodProfilesService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(50, 0);
                }
                string person = string.Empty, academicPeriod = string.Empty;

                var criteriaObj = GetFilterObject<Dtos.StudentAcademicPeriodProfiles>(_logger, "criteria");

                if (criteriaObj != null)
                {
                    person = criteriaObj.Person != null && !string.IsNullOrEmpty(criteriaObj.Person.Id) ? criteriaObj.Person.Id : string.Empty;

                    academicPeriod = criteriaObj.AcademicPeriod != null && !string.IsNullOrEmpty(criteriaObj.AcademicPeriod.Id) ?
                        criteriaObj.AcademicPeriod.Id : string.Empty;
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentAcademicPeriodProfiles>>(new List<Dtos.StudentAcademicPeriodProfiles>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(page.Offset, page.Limit, bypassCache, person, academicPeriod);

                AddEthosContextProperties((await _studentAcademicPeriodProfilesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)),
                    await _studentAcademicPeriodProfilesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<StudentAcademicPeriodProfiles>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Creates a Student Academic Period Profile.
        /// </summary>
        /// <param name="studentAcademicPeriodProfiles"><see cref="StudentAcademicPeriodProfiles">StudentAcademicPeriodProfiles</see> to create</param>
        /// <returns>Newly created <see cref="StudentAcademicPeriodProfiles">StudentAcademicPeriodProfiles</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/student-academic-period-profiles", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAcademicPeriodProfilesV11")]
        [HeaderVersionRoute("/student-academic-period-profiles", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAcademicPeriodProfilesV7")]
        public async Task<ActionResult<StudentAcademicPeriodProfiles>> CreateStudentAcademicPeriodProfilesAsync([ModelBinder(typeof(EedmModelBinder))] StudentAcademicPeriodProfiles studentAcademicPeriodProfiles)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Student Academic Period Profile.
        /// </summary>
        /// <param name="id">Id of the Student Academic Period Profile to update</param>
        /// <param name="studentAcademicPeriodProfiles"><see cref="StudentAcademicPeriodProfiles">StudentAcademicPeriodProfiles</see> to create</param>
        /// <returns>Updated <see cref="StudentAcademicPeriodProfiles">StudentAcademicPeriodProfiles</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/student-academic-period-profiles/{id}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAcademicPeriodProfilesV11")]
        [HeaderVersionRoute("/student-academic-period-profiles/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAcademicPeriodProfilesV7")]
        public async Task<ActionResult<StudentAcademicPeriodProfiles>> UpdateStudentAcademicPeriodProfilesAsync([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] StudentAcademicPeriodProfiles studentAcademicPeriodProfiles)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Student Academic Period Profiles.
        /// </summary>
        /// <param name="id">ID of the Student Academic Period Profile to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/student-academic-period-profiles/{id}", Name = "DefaultDeleteStudentAcademicPeriodProfiles", Order = -10)]
        public async Task<IActionResult> DeleteStudentAcademicPeriodProfilesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentUnverifiedGrades
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentUnverifiedGradesController : BaseCompressedApiController
    {
        private readonly IStudentUnverifiedGradesService _studentUnverifiedGradesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentUnverifiedGradesController class.
        /// </summary>
        /// <param name="studentUnverifiedGradesService">Service of type <see cref="IStudentUnverifiedGradesService">IStudentUnverifiedGradesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentUnverifiedGradesController(IStudentUnverifiedGradesService studentUnverifiedGradesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentUnverifiedGradesService = studentUnverifiedGradesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentUnverifiedGrades
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">filter criteria</param>
        /// <param name="section">filter section</param>
        /// <returns>List of StudentUnverifiedGrades <see cref="Dtos.StudentUnverifiedGrades"/> objects representing matching studentUnverifiedGrades</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentUnverifiedGrades, StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions })]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentUnverifiedGrades))]
        [QueryStringFilterFilter("section", typeof(Dtos.Filters.StudentUnverifiedGradesFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/student-unverified-grades", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentUnverifiedGrades", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentUnverifiedGradesAsync(Paging page, QueryStringFilter criteria, QueryStringFilter section)
        {
            string student = string.Empty, sectionRegistration = string.Empty, sectionId = string.Empty;

            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var criteriaObj = GetFilterObject<Dtos.StudentUnverifiedGrades>(_logger, "criteria");
            if (criteriaObj != null)
            {
                student = criteriaObj.Student != null ? criteriaObj.Student.Id : string.Empty;
                sectionRegistration = criteriaObj.SectionRegistration != null ? criteriaObj.SectionRegistration.Id : string.Empty;
            }

            var sectionObj = GetFilterObject<Dtos.Filters.StudentUnverifiedGradesFilter>(_logger, "section");
            if (sectionObj != null)
            {
                sectionId = sectionObj.Section != null ? sectionObj.Section.Id : string.Empty;
            }
            

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.StudentUnverifiedGrades>>(new List<Dtos.StudentUnverifiedGrades>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            try
            {
                _studentUnverifiedGradesService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesAsync(page.Offset, page.Limit, student, sectionRegistration, sectionId, bypassCache);

                AddEthosContextProperties(
                  await _studentUnverifiedGradesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentUnverifiedGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentUnverifiedGrades>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Read (GET) a studentUnverifiedGrades using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentUnverifiedGrades</param>
        /// <returns>A studentUnverifiedGrades object <see cref="Dtos.StudentUnverifiedGrades"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentUnverifiedGrades, StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions })]    
        [HeaderVersionRoute("/student-unverified-grades/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentUnverifiedGradesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentUnverifiedGrades>> GetStudentUnverifiedGradesByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _studentUnverifiedGradesService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _studentUnverifiedGradesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentUnverifiedGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentUnverifiedGradesService.GetStudentUnverifiedGradesByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Create (POST) a new studentUnverifiedGrades
        /// </summary>
        /// <param name="studentUnverifiedGrades">DTO of the new studentUnverifiedGrades</param>
        /// <returns>A studentUnverifiedGrades object <see cref="Dtos.StudentUnverifiedGrades"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-unverified-grades", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentUnverifiedGradesV1.0.0")]
        public async Task<ActionResult<Dtos.StudentUnverifiedGrades>> PostStudentUnverifiedGradesAsync([FromBody] Dtos.StudentUnverifiedGrades studentUnverifiedGrades)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentUnverifiedGrades
        /// </summary>
        /// <param name="guid">GUID of the studentUnverifiedGrades to update</param>
        /// <param name="studentUnverifiedGrades">DTO of the updated studentUnverifiedGrades</param>
        /// <returns>A studentUnverifiedGrades object <see cref="Dtos.StudentUnverifiedGrades"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-unverified-grades/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentUnverifiedGradesV1.0.0")]
        public async Task<ActionResult<Dtos.StudentUnverifiedGrades>> PutStudentUnverifiedGradesAsync([FromRoute] string guid, [FromBody] Dtos.StudentUnverifiedGrades studentUnverifiedGrades)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentUnverifiedGrades
        /// </summary>
        /// <param name="guid">GUID to desired studentUnverifiedGrades</param>
        [HttpDelete]
        [Route("/student-unverified-grades/{guid}", Name = "DefaultDeleteStudentUnverifiedGrades", Order = -10)]
        public async Task<IActionResult> DeleteStudentUnverifiedGradesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

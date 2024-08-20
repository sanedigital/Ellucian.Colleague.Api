// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
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


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentGradePointAverages
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
    public class StudentGradePointAveragesController : BaseCompressedApiController
    {
        private readonly IStudentGradePointAveragesService _studentGradePointAveragesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentGradePointAveragesController class.
        /// </summary>
        /// <param name="studentGradePointAveragesService">Service of type <see cref="IStudentGradePointAveragesService">IStudentGradePointAveragesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public StudentGradePointAveragesController(IStudentGradePointAveragesService studentGradePointAveragesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentGradePointAveragesService = studentGradePointAveragesService;
            this._logger = logger;
        }


        /// <summary>
        /// Return all studentGradePointAverages
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <param name="gradeDate"></param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentGradePointAverages)]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentGradePointAverages))]
        [QueryStringFilterFilter("gradeDate", typeof(Dtos.Filters.GradeDateFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [MetadataAttributes.BulkSupported]
        [HeaderVersionRoute("/student-grade-point-averages", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentGradePointAverages", IsEedmSupported = true, IsBulkSupported = true)]
        public async Task<IActionResult> GetStudentGradePointAveragesAsync(Paging page, QueryStringFilter criteria, //QueryStringFilter academicPeriod, 
            QueryStringFilter gradeDate)
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
                _studentGradePointAveragesService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.StudentGradePointAverages>(_logger, "criteria");

                //gradeDate
                string gradeDateFilterValue = string.Empty;
                var gradeDateFilterObj = GetFilterObject<Dtos.Filters.GradeDateFilter>(_logger, "gradeDate");
                if (gradeDateFilterObj != null && gradeDateFilterObj.GradeDate.HasValue )
                {
                    gradeDateFilterValue = gradeDateFilterObj.GradeDate.Value.ToString();
                }


                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentGradePointAverages>>(new List<Dtos.StudentGradePointAverages>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);


                var pageOfItems = await _studentGradePointAveragesService.GetStudentGradePointAveragesAsync(page.Offset, page.Limit, criteriaObj, gradeDateFilterValue, bypassCache);

                if (pageOfItems != null && pageOfItems.Item1.Any())
                {
                    AddEthosContextProperties(
                      await _studentGradePointAveragesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _studentGradePointAveragesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                          pageOfItems.Item1.Select(i => i.Id).ToList()));
                }

                return new PagedActionResult<IEnumerable<Dtos.StudentGradePointAverages>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a studentGradePointAverages using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentGradePointAverages</param>
        /// <returns>A studentGradePointAverages object <see cref="Dtos.StudentGradePointAverages"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentGradePointAverages)]
        [HttpGet]
        [HeaderVersionRoute("/student-grade-point-averages/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentGradePointAveragesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentGradePointAverages>> GetStudentGradePointAveragesByGuidAsync(string guid)
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
                _studentGradePointAveragesService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _studentGradePointAveragesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentGradePointAveragesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentGradePointAveragesService.GetStudentGradePointAveragesByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new studentGradePointAverages
        /// </summary>
        /// <param name="studentGradePointAverages">DTO of the new studentGradePointAverages</param>
        /// <returns>A studentGradePointAverages object <see cref="Dtos.StudentGradePointAverages"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/student-grade-point-averages", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentGradePointAveragesV100")]
        public async Task<ActionResult<Dtos.StudentGradePointAverages>> PostStudentGradePointAveragesAsync([FromBody] Dtos.StudentGradePointAverages studentGradePointAverages)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing studentGradePointAverages
        /// </summary>
        /// <param name="guid">GUID of the studentGradePointAverages to update</param>
        /// <param name="studentGradePointAverages">DTO of the updated studentGradePointAverages</param>
        /// <returns>A studentGradePointAverages object <see cref="Dtos.StudentGradePointAverages"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/student-grade-point-averages/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentGradePointAveragesV100")]
        public async Task<ActionResult<Dtos.StudentGradePointAverages>> PutStudentGradePointAveragesAsync([FromRoute] string guid, [FromBody] Dtos.StudentGradePointAverages studentGradePointAverages)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a studentGradePointAverages
        /// </summary>
        /// <param name="guid">GUID to desired studentGradePointAverages</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/student-grade-point-averages/{guid}", Name = "DefaultDeleteStudentGradePointAverages", Order = -10)]
        public async Task<IActionResult> DeleteStudentGradePointAveragesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

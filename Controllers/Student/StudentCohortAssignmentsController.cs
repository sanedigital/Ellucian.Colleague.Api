// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to StudentCohortAssignments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentCohortAssignmentsController : BaseCompressedApiController
    {
        private readonly IStudentCohortAssignmentsService _studentCohortAssignmentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentCohortAssignmentsController class.
        /// </summary>
        /// <param name="studentCohortAssignmentsService">Service of type <see cref="IStudentCohortAssignmentsService">IStudentCohortAssignmentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public StudentCohortAssignmentsController(IStudentCohortAssignmentsService studentCohortAssignmentsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentCohortAssignmentsService = studentCohortAssignmentsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentCohortAssignments.
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">JSON formatted selection criteria.</param>
        /// <returns>List of StudentCohortAssignments <see cref="Dtos.StudentCohortAssignments"/> objects representing matching studentCohortAssignments</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)) , PermissionsFilter(StudentPermissionCodes.ViewStudentCohortAssignments)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentCohortAssignments))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HttpGet]
        [HeaderVersionRoute("/student-cohort-assignments", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentCohortAssignments", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentCohortAssignmentsAsync(Paging page, QueryStringFilter criteria)
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
                _studentCohortAssignmentsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var criteriaObj = GetFilterObject<Dtos.StudentCohortAssignments>(_logger, "criteria");
                
                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentCohortAssignments>>(new List<Dtos.StudentCohortAssignments>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                Dictionary<string, string> filterQualifiers = GetFilterQualifiers(_logger);

                var pageOfItems = await _studentCohortAssignmentsService.GetStudentCohortAssignmentsAsync(page.Offset, page.Limit, criteriaObj, 
                                  filterQualifiers, bypassCache);

                AddEthosContextProperties(
                  await _studentCohortAssignmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentCohortAssignmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentCohortAssignments>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a studentCohortAssignments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentCohortAssignments</param>
        /// <returns>A studentCohortAssignments object <see cref="Dtos.StudentCohortAssignments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentCohortAssignments)]
        [HeaderVersionRoute("/student-cohort-assignments/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentCohortAssignmentsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentCohortAssignments>> GetStudentCohortAssignmentsByGuidAsync(string guid)
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
                _studentCohortAssignmentsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _studentCohortAssignmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentCohortAssignmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentCohortAssignmentsService.GetStudentCohortAssignmentsByGuidAsync(guid);
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
        /// Create (POST) a new studentCohortAssignments
        /// </summary>
        /// <param name="studentCohortAssignments">DTO of the new studentCohortAssignments</param>
        /// <returns>A studentCohortAssignments object <see cref="Dtos.StudentCohortAssignments"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-cohort-assignments", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentCohortAssignmentsV100")]
        public async Task<ActionResult<Dtos.StudentCohortAssignments>> PostStudentCohortAssignmentsAsync([FromBody] Dtos.StudentCohortAssignments studentCohortAssignments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentCohortAssignments
        /// </summary>
        /// <param name="guid">GUID of the studentCohortAssignments to update</param>
        /// <param name="studentCohortAssignments">DTO of the updated studentCohortAssignments</param>
        /// <returns>A studentCohortAssignments object <see cref="Dtos.StudentCohortAssignments"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-cohort-assignments/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentCohortAssignmentsV100")]
        public async Task<ActionResult<Dtos.StudentCohortAssignments>> PutStudentCohortAssignmentsAsync([FromRoute] string guid, [FromBody] Dtos.StudentCohortAssignments studentCohortAssignments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentCohortAssignments
        /// </summary>
        /// <param name="guid">GUID to desired studentCohortAssignments</param>
        [HttpDelete]
        [Route("/student-cohort-assignments/{guid}", Name = "DefaultDeleteStudentCohortAssignments", Order = -10)]
        public async Task<IActionResult> DeleteStudentCohortAssignmentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

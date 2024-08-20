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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentTranscriptGradesOptions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTranscriptGradesOptionsController : BaseCompressedApiController
    {
        private readonly IStudentTranscriptGradesOptionsService _StudentTranscriptGradesOptionsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentTranscriptGradesOptionsController class.
        /// </summary>
        /// <param name="StudentTranscriptGradesOptionsService">Service of type <see cref="IStudentTranscriptGradesOptionsService">IStudentTranscriptGradesOptionsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentTranscriptGradesOptionsController(IStudentTranscriptGradesOptionsService StudentTranscriptGradesOptionsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _StudentTranscriptGradesOptionsService = StudentTranscriptGradesOptionsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all StudentTranscriptGradesOptions
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="student">filter student</param>
        /// <returns>List of StudentTranscriptGradesOptions <see cref="Dtos.StudentTranscriptGradesOptions"/> objects representing matching StudentTranscriptGradesOptions</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentTranscriptGrades, StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments })]  
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("student", typeof(Dtos.Filters.StudentFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/student-transcript-grades", "1.0.0", false, RouteConstants.HedtechIntegrationStudentTranscriptGradesOptionsFormat, Name = "GetStudentTranscriptGradesOptionsV1.0.0", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentTranscriptGradesOptionsAsync(Paging page, QueryStringFilter student)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var studentFilter = GetFilterObject<Dtos.Filters.StudentFilter>(_logger, "student");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.StudentTranscriptGradesOptions>>(new List<Dtos.StudentTranscriptGradesOptions>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            try
            {
                _StudentTranscriptGradesOptionsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _StudentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsAsync(page.Offset, page.Limit, studentFilter, bypassCache);

                AddEthosContextProperties(
                  await _StudentTranscriptGradesOptionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _StudentTranscriptGradesOptionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentTranscriptGradesOptions>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a StudentTranscriptGradesOptions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired StudentTranscriptGradesOptions</param>
        /// <returns>A StudentTranscriptGradesOptions object <see cref="Dtos.StudentTranscriptGradesOptions"/> in EEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)] 
        [PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentTranscriptGrades, StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/student-transcript-grades/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationStudentTranscriptGradesOptionsFormat, Name = "GetStudentTranscriptGradesOptionsByGuidV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentTranscriptGradesOptions>> GetStudentTranscriptGradesOptionsByGuidAsync(string guid)
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
                _StudentTranscriptGradesOptionsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _StudentTranscriptGradesOptionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _StudentTranscriptGradesOptionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _StudentTranscriptGradesOptionsService.GetStudentTranscriptGradesOptionsByGuidAsync(guid);
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
        /// Create (POST) a new StudentTranscriptGradesOptions
        /// </summary>
        /// <param name="StudentTranscriptGradesOptions">DTO of the new StudentTranscriptGradesOptions</param>
        /// <returns>A StudentTranscriptGradesOptions object <see cref="Dtos.StudentTranscriptGradesOptions"/> in EEDM format</returns>
        [HttpPost]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/student-transcript-grades", "1.0.0", false, RouteConstants.HedtechIntegrationStudentTranscriptGradesOptionsFormat, Name = "PostStudentTranscriptGradesOptionsV1.0.0")]
        public async Task<ActionResult<Dtos.StudentTranscriptGradesOptions>> PostStudentTranscriptGradesOptionsAsync([FromBody] Dtos.StudentTranscriptGradesOptions StudentTranscriptGradesOptions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing StudentTranscriptGradesOptions
        /// </summary>
        /// <param name="guid">GUID of the StudentTranscriptGradesOptions to update</param>
        /// <param name="StudentTranscriptGradesOptions">DTO of the updated studentTranscriptGrades</param>
        /// <returns>A StudentTranscriptGradesOptions object <see cref="Dtos.StudentTranscriptGradesOptions"/> in EEDM format</returns>
        [HttpPut]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/student-transcript-grades/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationStudentTranscriptGradesOptionsFormat, Name = "PutStudentTranscriptGradesOptionsV1.0.0")]
        public async Task<ActionResult<Dtos.StudentTranscriptGradesOptions>> PutStudentTranscriptGradesOptionsAsync([FromRoute] string guid, [FromBody] Dtos.StudentTranscriptGradesOptions StudentTranscriptGradesOptions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Http.Controllers;
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



namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentTranscriptGrades
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTranscriptGradesController : BaseCompressedApiController
    {
        private readonly IStudentTranscriptGradesService _studentTranscriptGradesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentTranscriptGradesController class.
        /// </summary>
        /// <param name="studentTranscriptGradesService">Service of type <see cref="IStudentTranscriptGradesService">IStudentTranscriptGradesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentTranscriptGradesController(IStudentTranscriptGradesService studentTranscriptGradesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentTranscriptGradesService = studentTranscriptGradesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentTranscriptGrades
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">filter criteria</param>
        /// <returns>List of StudentTranscriptGrades <see cref="Dtos.StudentTranscriptGrades"/> objects representing matching studentTranscriptGrades</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentTranscriptGrades, StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentTranscriptGrades))]
        [HeaderVersionRoute("/student-transcript-grades", "1.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentTranscriptGrades", IsEedmSupported = true, IsBulkSupported = true)]
        public async Task<IActionResult> GetStudentTranscriptGradesAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            
            var criteriaFilter = GetFilterObject<Dtos.StudentTranscriptGrades>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.StudentTranscriptGradesOptions>>(new List<Dtos.StudentTranscriptGradesOptions>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            try
            {
                _studentTranscriptGradesService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(
                  await _studentTranscriptGradesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentTranscriptGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentTranscriptGrades>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
            catch (InvalidOperationException e)
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
        /// Read (GET) a studentTranscriptGrades using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentTranscriptGrades</param>
        /// <returns>A studentTranscriptGrades object <see cref="Dtos.StudentTranscriptGrades"/> in EEDM format</returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentTranscriptGrades, StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments })]
        [HeaderVersionRoute("/student-transcript-grades/{guid}", "1.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentTranscriptGradesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentTranscriptGrades>> GetStudentTranscriptGradesByGuidAsync(string guid)
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
                _studentTranscriptGradesService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _studentTranscriptGradesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentTranscriptGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentTranscriptGradesService.GetStudentTranscriptGradesByGuidAsync(guid);
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
        /// Create (POST) a new studentTranscriptGrades
        /// </summary>
        /// <param name="studentTranscriptGrades">DTO of the new studentTranscriptGrades</param>
        /// <returns>A studentTranscriptGrades object <see cref="Dtos.StudentTranscriptGrades"/> in EEDM format</returns>
        [HttpPost]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/student-transcript-grades", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentTranscriptGradesV1.0.0")]
        public async Task<ActionResult<Dtos.StudentTranscriptGrades>> PostStudentTranscriptGradesAsync([FromBody] Dtos.StudentTranscriptGrades studentTranscriptGrades)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing studentTranscriptGrades
        /// </summary>
        /// <param name="guid">GUID of the studentTranscriptGrades to update</param>
        /// <param name="studentTranscriptGrades">DTO of the updated studentTranscriptGrades</param>
        /// <returns>A studentTranscriptGrades object <see cref="Dtos.StudentTranscriptGrades"/> in EEDM format</returns>
        [HttpPut]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/student-transcript-grades/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentTranscriptGradesV1.0.0")]
        public async Task<ActionResult<Dtos.StudentTranscriptGrades>> PutStudentTranscriptGradesAsync([FromRoute] string guid, [FromBody] Dtos.StudentTranscriptGrades studentTranscriptGrades)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentTranscriptGrades
        /// </summary>
        /// <param name="guid">GUID to desired studentTranscriptGrades</param>
        [HttpDelete]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [Route("/student-transcript-grades/{guid}", Name = "DefaultDeleteStudentTranscriptGrades", Order = -10)]
        public async Task<IActionResult> DeleteStudentTranscriptGradesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        #region Adjustments

        /// <summary>
        /// Return all studentTranscriptGradesAdjustments
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of StudentTranscriptGradesAdjustments <see cref="Dtos.StudentTranscriptGradesAdjustments"/> objects representing matching studentTranscriptGrades</returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<IActionResult> GetStudentTranscriptGradesAdjustmentsAsync(Paging page)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Get a studentTranscriptGradesAdjustments
        /// </summary>
        /// <param name="guid">GUID to desired studentTranscriptGradesAdjustments</param>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<IActionResult> GetStudentTranscriptGradesAdjustmentsByGuidAsync(string guid)
        {
            //Get is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing StudentTranscriptGradesAdjustments
        /// </summary>
        /// <param name="guid">GUID of the studentTranscriptGradesAdjustments to update</param>
        /// <param name="studentTranscriptGradesAdjustments">DTO of the updated studentTranscriptGradesAdjustments</param>
        /// <returns>A StudentTranscriptGradesAdjustments object <see cref="Dtos.StudentTranscriptGradesAdjustments"/> in EEDM format</returns>
        [HttpPut] [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter)),PermissionsFilter(StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments)]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationStudentTranscriptGradesAdjustmentsFormat },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/student-transcript-grades/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultPutStudentTranscriptGradesAdjustmentsV1.0.0", Order = -15)]
        public async Task<ActionResult<Dtos.StudentTranscriptGrades>> PutStudentTranscriptGradesAdjustmentsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentTranscriptGradesAdjustments studentTranscriptGradesAdjustments)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentTranscriptGradesAdjustments == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null studentTranscriptGradesAdjustments argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(studentTranscriptGradesAdjustments.Id))
            {
                studentTranscriptGradesAdjustments.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, studentTranscriptGradesAdjustments.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _studentTranscriptGradesService.ValidatePermissions(GetPermissionsMetaData());
                var dpList = await _studentTranscriptGradesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                await _studentTranscriptGradesService.ImportExtendedEthosData(await ExtractExtendedData(await _studentTranscriptGradesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var studentTranscriptGradesReturn = await _studentTranscriptGradesService.UpdateStudentTranscriptGradesAdjustmentsAsync(
                  await PerformPartialPayloadMerge(studentTranscriptGradesAdjustments, async () => await _studentTranscriptGradesService.GetStudentTranscriptGradesAdjustmentsByGuidAsync(guid, true),
                  dpList, _logger));

                AddEthosContextProperties(dpList,
                    await _studentTranscriptGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return studentTranscriptGradesReturn;
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
        /// Create (POST) a new studentTranscriptGrades
        /// </summary>
        /// <param name="studentTranscriptGradesAdjustments">DTO of the new studentTranscriptGrades</param>
        /// <returns>A studentTranscriptGrades object <see cref="Dtos.StudentTranscriptGradesAdjustments"/> in EEDM format</returns>
        [HttpPost]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationStudentTranscriptGradesAdjustmentsFormat },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/student-transcript-grades", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultPostStudentTranscriptGradesAdjustmentsV1.0.0", Order = -15)]
        public async Task<ActionResult<Dtos.StudentTranscriptGrades>> PostStudentTranscriptGradesAdjustmentsAsync([FromBody] Dtos.StudentTranscriptGradesAdjustments studentTranscriptGradesAdjustments)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion
    }
}

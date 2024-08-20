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

using Ellucian.Web.Http.ModelBinding;
using System.Net.Http;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http.Constraints;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentUnverifiedGradesSubmissions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentUnverifiedGradesSubmissionsController : BaseCompressedApiController
    {
        private readonly IStudentUnverifiedGradesService _studentUnverifiedGradesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentUnverifiedGradesSubmissionsController class.
        /// </summary>
        /// <param name="studentUnverifiedGradesService">Service of type <see cref="IStudentUnverifiedGradesService">IStudentUnverifiedGradesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentUnverifiedGradesSubmissionsController(IStudentUnverifiedGradesService studentUnverifiedGradesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentUnverifiedGradesService = studentUnverifiedGradesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentUnverifiedGradesSubmissions
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of StudentUnverifiedGradesSubmissions <see cref="Dtos.StudentUnverifiedGradesSubmissions"/> objects representing matching studentUnverifiedGradesSubmissions</returns>
        [HttpGet]
        public async Task<IActionResult> GetStudentUnverifiedGradesSubmissionsAsync(Paging page)
        {
            //Get is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Read (GET) a studentUnverifiedGradesSubmissions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentUnverifiedGradesSubmissions</param>
        /// <returns>A studentUnverifiedGradesSubmissions object <see cref="Dtos.StudentUnverifiedGradesSubmissions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        public async Task<ActionResult<Dtos.StudentUnverifiedGradesSubmissions>> GetStudentUnverifiedGradesSubmissionsByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _studentUnverifiedGradesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentUnverifiedGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(guid);
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
        /// Update (PUT) an existing StudentUnverifiedGradesSubmissions
        /// </summary>
        /// <param name="guid">GUID of the studentUnverifiedGradesSubmissions to update</param>
        /// <param name="studentUnverifiedGradesSubmissions">DTO of the updated studentUnverifiedGradesSubmissions</param>
        /// <returns>A StudentUnverifiedGrades object <see cref="Dtos.StudentUnverifiedGrades"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions)]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationStudentUnverifiedGradesSubmissionsFormat },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/student-unverified-grades/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultPutStudentUnverifiedGradesSubmissionsV1.0.0", Order = -15)]
        public async Task<ActionResult<Dtos.StudentUnverifiedGrades>> PutStudentUnverifiedGradesSubmissionsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentUnverifiedGradesSubmissions studentUnverifiedGradesSubmissions)
        {
           
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentUnverifiedGradesSubmissions == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null studentUnverifiedGradesSubmissions argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(studentUnverifiedGradesSubmissions.Id))
            {
                studentUnverifiedGradesSubmissions.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, studentUnverifiedGradesSubmissions.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _studentUnverifiedGradesService.ValidatePermissions(GetPermissionsMetaData());
                var dpList = await _studentUnverifiedGradesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                // Save incoming Last Attendance info for later comparison after partial put logic
                DateTime? submittedLastAttendanceDate = null;
                Dtos.EnumProperties.StudentUnverifiedGradesStatus submittedLastAttendanceStatus = Dtos.EnumProperties.StudentUnverifiedGradesStatus.NotSet;
                if (studentUnverifiedGradesSubmissions.LastAttendance != null)
                {
                    if (studentUnverifiedGradesSubmissions.LastAttendance.Date != null)
                    {
                        submittedLastAttendanceDate = studentUnverifiedGradesSubmissions.LastAttendance.Date;
                    }
                    if (studentUnverifiedGradesSubmissions.LastAttendance.Status == Dtos.EnumProperties.StudentUnverifiedGradesStatus.Neverattended)
                    {
                        submittedLastAttendanceStatus = Dtos.EnumProperties.StudentUnverifiedGradesStatus.Neverattended;
                    }
                }
                
                var studentUnverifiedGradesSubmissionsDto = _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(guid, true);
                var studentUnverifiedGradesDto = (await PerformPartialPayloadMerge(studentUnverifiedGradesSubmissions, async () => await studentUnverifiedGradesSubmissionsDto, dpList, _logger));

                // If we received a last attend date and no last attend status, clear out the status if it already exists.
                if ((submittedLastAttendanceDate != null) && (submittedLastAttendanceStatus == Dtos.EnumProperties.StudentUnverifiedGradesStatus.NotSet))
                {
                    if (studentUnverifiedGradesDto.LastAttendance != null)
                    {
                        if (studentUnverifiedGradesDto.LastAttendance.Status == Dtos.EnumProperties.StudentUnverifiedGradesStatus.Neverattended)
                        {
                            studentUnverifiedGradesDto.LastAttendance.Status = Dtos.EnumProperties.StudentUnverifiedGradesStatus.NotSet;
                        }
                    }
                }
                // If we received a last attend status and no last attend date, clear the date if it already exists.
                if ((submittedLastAttendanceStatus == Dtos.EnumProperties.StudentUnverifiedGradesStatus.Neverattended) && submittedLastAttendanceDate == null)
                {
                    if (studentUnverifiedGradesDto.LastAttendance != null)
                    {
                        if (studentUnverifiedGradesDto.LastAttendance.Date != null)
                        {
                            studentUnverifiedGradesDto.LastAttendance.Date = null;
                        }
                    }
                }

                await _studentUnverifiedGradesService.ImportExtendedEthosData(await ExtractExtendedData(await _studentUnverifiedGradesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var studentUnverifiedGradesReturn = await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGradesDto);

                AddEthosContextProperties(dpList,
                    await _studentUnverifiedGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return studentUnverifiedGradesReturn;
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
        /// Create (POST) a new studentUnverifiedGradesSubmissions
        /// </summary>
        /// <param name="studentUnverifiedGradesSubmissions">DTO of the new studentUnverifiedGradesSubmissions</param>
        /// <returns>A studentUnverifiedGrades object <see cref="Dtos.StudentUnverifiedGrades"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, PermissionsFilter(StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions)]
        [ContentTypeConstraint(new[] { RouteConstants.HedtechIntegrationStudentUnverifiedGradesSubmissionsFormat },
                               new[] { "1.0.0" })]
        [HeaderVersionRoute("/student-unverified-grades", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultPostStudentUnverifiedGradesSubmissionsV1.0.0", Order = -15)]
        public async Task<ActionResult<Dtos.StudentUnverifiedGrades>> PostStudentUnverifiedGradesSubmissionsAsync(Dtos.StudentUnverifiedGradesSubmissions studentUnverifiedGradesSubmissions)
        {
            
            if (studentUnverifiedGradesSubmissions == null)
            {
                return CreateHttpResponseException("Request body must contain a valid studentUnverifiedGradesSubmissions.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(studentUnverifiedGradesSubmissions.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null studentUnverifiedGradesSubmissions id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (studentUnverifiedGradesSubmissions.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }
            try
            {
                _studentUnverifiedGradesService.ValidatePermissions(GetPermissionsMetaData());
                await _studentUnverifiedGradesService.ImportExtendedEthosData(await ExtractExtendedData(await _studentUnverifiedGradesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                var response = await _studentUnverifiedGradesService.CreateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGradesSubmissions);
                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _studentUnverifiedGradesService.GetDataPrivacyListByApi(GetRouteResourceName(), true),

                await _studentUnverifiedGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { response.Id }));

                return response;
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
            catch (ConfigurationException e)
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
        /// Delete (DELETE) a studentUnverifiedGradesSubmissions
        /// </summary>
        /// <param name="guid">GUID to desired studentUnverifiedGradesSubmissions</param>
        /// <returns>HttpResponseMessage</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteStudentUnverifiedGradesSubmissionsAsync([FromQuery] string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

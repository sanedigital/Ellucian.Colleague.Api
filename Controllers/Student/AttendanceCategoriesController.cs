// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AttendanceCategories
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AttendanceCategoriesController : BaseCompressedApiController
    {
        private readonly IAttendanceCategoriesService _attendanceCategoriesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AttendanceCategoriesController class.
        /// </summary>
        /// <param name="attendanceCategoriesService">Service of type <see cref="IAttendanceCategoriesService">IAttendanceCategoriesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AttendanceCategoriesController(IAttendanceCategoriesService attendanceCategoriesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _attendanceCategoriesService = attendanceCategoriesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all attendanceCategories
        /// </summary>
        /// <returns>List of AttendanceCategories <see cref="Dtos.AttendanceCategories"/> objects representing matching attendanceCategories</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/attendance-categories", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAttendanceCategories", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AttendanceCategories>>> GetAttendanceCategoriesAsync()
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
                return Ok(await _attendanceCategoriesService.GetAttendanceCategoriesAsync(bypassCache));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, "Session has expired while retrieving attendance categories");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(csse), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Read (GET) a attendanceCategories using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired attendanceCategories</param>
        /// <returns>A attendanceCategories object <see cref="Dtos.AttendanceCategories"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/attendance-categories/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAttendanceCategoriesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AttendanceCategories>> GetAttendanceCategoriesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _attendanceCategoriesService.GetAttendanceCategoriesByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Create (POST) a new attendanceCategories
        /// </summary>
        /// <param name="attendanceCategories">DTO of the new attendanceCategories</param>
        /// <returns>A attendanceCategories object <see cref="Dtos.AttendanceCategories"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/attendance-categories", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAttendanceCategoriesV10")]
        public async Task<ActionResult<Dtos.AttendanceCategories>> PostAttendanceCategoriesAsync([FromBody] Dtos.AttendanceCategories attendanceCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing attendanceCategories
        /// </summary>
        /// <param name="guid">GUID of the attendanceCategories to update</param>
        /// <param name="attendanceCategories">DTO of the updated attendanceCategories</param>
        /// <returns>A attendanceCategories object <see cref="Dtos.AttendanceCategories"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/attendance-categories/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAttendanceCategoriesV10")]
        public async Task<ActionResult<Dtos.AttendanceCategories>> PutAttendanceCategoriesAsync([FromRoute] string guid, [FromBody] Dtos.AttendanceCategories attendanceCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a attendanceCategories
        /// </summary>
        /// <param name="guid">GUID to desired attendanceCategories</param>
        [HttpDelete]
        [Route("/attendance-categories/{guid}", Name = "DefaultDeleteAttendanceCategories")]
        public async Task<IActionResult> DeleteAttendanceCategoriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentRegistrationEligibilities
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentRegistrationEligibilitiesController : BaseCompressedApiController
    {
        private readonly IStudentRegistrationEligibilitiesService _studentRegistrationEligibilitiesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentRegistrationEligibilitiesController class.
        /// </summary>
        /// <param name="studentRegistrationEligibilitiesService">Service of type <see cref="IStudentRegistrationEligibilitiesService">IStudentRegistrationEligibilitiesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentRegistrationEligibilitiesController(IStudentRegistrationEligibilitiesService studentRegistrationEligibilitiesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentRegistrationEligibilitiesService = studentRegistrationEligibilitiesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return a single studentRegistrationEligibilities matching required filters of
        /// Student and Academic Period.
        /// </summary>
        /// <returns>StudentRegistrationEligibilities <see cref="Dtos.StudentRegistrationEligibilities"/> object representing matching studentRegistrationEligibilities</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(new string[] { StudentPermissionCodes.ViewStuRegistrationEligibility })]
        [ValidateQueryStringFilter(), QueryStringFilterFilter("criteria", typeof(Dtos.StudentRegistrationEligibilities))]
        [HttpGet]
        [HeaderVersionRoute("/student-registration-eligibilities", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentRegistrationEligibilities", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.StudentRegistrationEligibilities>> GetStudentRegistrationEligibilitiesAsync(QueryStringFilter criteria)
        {
            string studentId = string.Empty, academicPeriodId = string.Empty;
            try
            {
                _studentRegistrationEligibilitiesService.ValidatePermissions(GetPermissionsMetaData());

                var bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var criteriaObj = GetFilterObject<Dtos.StudentRegistrationEligibilities>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    studentId = criteriaObj.Student != null ? criteriaObj.Student.Id : string.Empty;
                    academicPeriodId = criteriaObj.AcademicPeriod != null ? criteriaObj.AcademicPeriod.Id : string.Empty;
                }

                if (CheckForEmptyFilterParameters())
                    return new Dtos.StudentRegistrationEligibilities();

                var items = await _studentRegistrationEligibilitiesService.GetStudentRegistrationEligibilitiesAsync(studentId, academicPeriodId, bypassCache);
                if (items == null)
                {
                    return new Dtos.StudentRegistrationEligibilities();
                }

                AddDataPrivacyContextProperty((await _studentRegistrationEligibilitiesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());

                return items;
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
        /// Read (GET) a studentRegistrationEligibilities using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentRegistrationEligibilities</param>
        /// <returns>A studentRegistrationEligibilities object <see cref="Dtos.StudentRegistrationEligibilities"/> in EEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/student-registration-eligibilities/{guid}", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentRegistrationEligibilitiesByGuid")]
        public async Task<ActionResult<Dtos.StudentRegistrationEligibilities>> GetStudentRegistrationEligibilitiesByGuidAsync(string guid)
        {
            //GET by guid is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Create (POST) a new studentRegistrationEligibilities
        /// </summary>
        /// <param name="studentRegistrationEligibilities">DTO of the new studentRegistrationEligibilities</param>
        /// <returns>A studentRegistrationEligibilities object <see cref="Dtos.StudentRegistrationEligibilities"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/student-registration-eligibilities", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentRegistrationEligibilitiesV9")]
        public async Task<ActionResult<Dtos.StudentRegistrationEligibilities>> PostStudentRegistrationEligibilitiesAsync([FromBody] Dtos.StudentRegistrationEligibilities studentRegistrationEligibilities)
        {
            // Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing studentRegistrationEligibilities
        /// </summary>
        /// <param name="guid">GUID of the studentRegistrationEligibilities to update</param>
        /// <param name="studentRegistrationEligibilities">DTO of the updated studentRegistrationEligibilities</param>
        /// <returns>A studentRegistrationEligibilities object <see cref="Dtos.StudentRegistrationEligibilities"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/student-registration-eligibilities/{guid}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentRegistrationEligibilitiesV9")]
        public async Task<ActionResult<Dtos.StudentRegistrationEligibilities>> PutStudentRegistrationEligibilitiesAsync([FromRoute] string guid, [FromBody] Dtos.StudentRegistrationEligibilities studentRegistrationEligibilities)
        {
            // Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a studentRegistrationEligibilities
        /// </summary>
        /// <param name="guid">GUID to desired studentRegistrationEligibilities</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/student-registration-eligibilities/{guid}", Name = "DefaultDeleteStudentRegistrationEligibilities", Order = -10)]
        public async Task<IActionResult> DeleteStudentRegistrationEligibilitiesAsync(string guid)
        {
            // Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

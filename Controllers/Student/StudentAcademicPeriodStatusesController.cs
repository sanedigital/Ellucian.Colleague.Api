// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Filters;
using System.Linq;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAcademicPeriodStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAcademicPeriodStatusesController : BaseCompressedApiController
    {
        private readonly IStudentAcademicPeriodStatusesService _studentAcademicPeriodStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAcademicPeriodStatusesController class.
        /// </summary>
        /// <param name="studentAcademicPeriodStatusesService">Service of type <see cref="IStudentAcademicPeriodStatusesService">IStudentAcademicPeriodStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentAcademicPeriodStatusesController(IStudentAcademicPeriodStatusesService studentAcademicPeriodStatusesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentAcademicPeriodStatusesService = studentAcademicPeriodStatusesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentAcademicPeriodStatuses
        /// </summary>
        /// <returns>List of StudentAcademicPeriodStatuses <see cref="Dtos.StudentAcademicPeriodStatuses"/> objects representing matching studentAcademicPeriodStatuses</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAcademicPeriodStatuses)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-period-statuses", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAcademicPeriodStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriodStatuses>>> GetStudentAcademicPeriodStatusesAsync(QueryStringFilter criteria)
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
                var filter = GetFilterObject<Dtos.StudentAcademicPeriodStatuses>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                {
                    return new List<Dtos.StudentAcademicPeriodStatuses>(new List<Dtos.StudentAcademicPeriodStatuses>());
                }
                if (filter.Usages != null && filter.Usages.Count > 1)
                {
                    return new List<Dtos.StudentAcademicPeriodStatuses>(new List<Dtos.StudentAcademicPeriodStatuses>());
                }

                var studentAcademicPeriodStatuses = await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesAsync(bypassCache);

                if (studentAcademicPeriodStatuses != null && studentAcademicPeriodStatuses.Any())
                {
                    AddEthosContextProperties(await _studentAcademicPeriodStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _studentAcademicPeriodStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              studentAcademicPeriodStatuses.Select(a => a.Id).ToList()));
                }
                if (studentAcademicPeriodStatuses != null && studentAcademicPeriodStatuses.Any() && !string.IsNullOrEmpty(filter.Code))
                {
                    studentAcademicPeriodStatuses = studentAcademicPeriodStatuses.Where(saps => saps.Code.Equals(filter.Code, StringComparison.OrdinalIgnoreCase));
                }

                if (studentAcademicPeriodStatuses != null && studentAcademicPeriodStatuses.Any() 
                    && (filter.Usages != null) && (filter.Usages.Any()))
                {
                    studentAcademicPeriodStatuses =  studentAcademicPeriodStatuses
                         .Where(p => filter.Usages.Any(a => p.Usages != null && p.Usages.Contains(a)));
                }

                return Ok(studentAcademicPeriodStatuses);
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
        /// Read (GET) a studentAcademicPeriodStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicPeriodStatuses</param>
        /// <returns>A studentAcademicPeriodStatuses object <see cref="Dtos.StudentAcademicPeriodStatuses"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-period-statuses/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAcademicPeriodStatusesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAcademicPeriodStatuses>> GetStudentAcademicPeriodStatusesByGuidAsync(string guid)
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
                   await _studentAcademicPeriodStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentAcademicPeriodStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesByGuidAsync(guid);
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
        /// Create (POST) a new studentAcademicPeriodStatuses
        /// </summary>
        /// <param name="studentAcademicPeriodStatuses">DTO of the new studentAcademicPeriodStatuses</param>
        /// <returns>A studentAcademicPeriodStatuses object <see cref="Dtos.StudentAcademicPeriodStatuses"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-academic-period-statuses", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAcademicPeriodStatusesV1.0.0")]
        public async Task<ActionResult<Dtos.StudentAcademicPeriodStatuses>> PostStudentAcademicPeriodStatusesAsync([FromBody] Dtos.StudentAcademicPeriodStatuses studentAcademicPeriodStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentAcademicPeriodStatuses
        /// </summary>
        /// <param name="guid">GUID of the studentAcademicPeriodStatuses to update</param>
        /// <param name="studentAcademicPeriodStatuses">DTO of the updated studentAcademicPeriodStatuses</param>
        /// <returns>A studentAcademicPeriodStatuses object <see cref="Dtos.StudentAcademicPeriodStatuses"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-academic-period-statuses/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAcademicPeriodStatusesV1.0.0")]
        public async Task<ActionResult<Dtos.StudentAcademicPeriodStatuses>> PutStudentAcademicPeriodStatusesAsync([FromRoute] string guid, [FromBody] Dtos.StudentAcademicPeriodStatuses studentAcademicPeriodStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentAcademicPeriodStatuses
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicPeriodStatuses</param>
        [HttpDelete]
        [Route("/student-academic-period-statuses/{guid}", Name = "DefaultDeleteStudentAcademicPeriodStatuses", Order = -10)]
        public async Task<IActionResult> DeleteStudentAcademicPeriodStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

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
using Ellucian.Web.Http;
using Newtonsoft.Json;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAdvisorRelationships
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAdvisorRelationshipsController : BaseCompressedApiController
    {
        private readonly IStudentAdvisorRelationshipsService _studentAdvisorRelationshipsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAdvisorRelationshipsController class.
        /// </summary>
        /// <param name="studentAdvisorRelationshipsService">Service of type <see cref="IStudentAdvisorRelationshipsService">IStudentAdvisorRelationshipsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentAdvisorRelationshipsController(IStudentAdvisorRelationshipsService studentAdvisorRelationshipsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentAdvisorRelationshipsService = studentAdvisorRelationshipsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentAdvisorRelationships
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">Filters to be used within this API. They must be in JSON and contain the following fields: student,advisor, advisorType and startAcademicPeriod</param>
        /// <returns>List of StudentAdvisorRelationships <see cref="Dtos.StudentAdvisorRelationships"/> objects representing matching studentAdvisorRelationships</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAdivsorRelationships })]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.StudentAdvisorRelationshipFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HttpGet]
        [HeaderVersionRoute("/student-advisor-relationships", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAdvisorRelationshipsV8", IsEedmSupported = true)]
        [HeaderVersionRoute("/student-advisor-relationships", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAdvisorRelationships", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentAdvisorRelationshipsAsync(Paging page, QueryStringFilter criteria)
        {
            string student = string.Empty, advisor = string.Empty, advisorType = string.Empty, startAcademicPeriod = string.Empty;

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
                _studentAdvisorRelationshipsService.ValidatePermissions(GetPermissionsMetaData());

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var criteriaObj = GetFilterObject<Dtos.Filters.StudentAdvisorRelationshipFilter>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    student = criteriaObj.Student != null ? criteriaObj.Student.Id : string.Empty;
                    advisor = criteriaObj.Advisor != null ? criteriaObj.Advisor.Id : string.Empty;
                    advisorType = criteriaObj.AdvisorType != null ? criteriaObj.AdvisorType.Id : string.Empty;
                    startAcademicPeriod = criteriaObj.StartAcademicPeriod != null ? criteriaObj.StartAcademicPeriod.Id : string.Empty;
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentAdvisorRelationships>>(new List<Dtos.StudentAdvisorRelationships>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsAsync(page.Offset, page.Limit, bypassCache,
                    student, advisor, advisorType, startAcademicPeriod);

                AddEthosContextProperties(
                  await _studentAdvisorRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentAdvisorRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));
                return new PagedActionResult<IEnumerable<Dtos.StudentAdvisorRelationships>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a studentAdvisorRelationships using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAdvisorRelationships</param>
        /// <returns>A studentAdvisorRelationships object <see cref="Dtos.StudentAdvisorRelationships"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAdivsorRelationships })]
        [HttpGet]
        [HeaderVersionRoute("/student-advisor-relationships/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAdvisorRelationshipsByGuidV8", IsEedmSupported = true)]
        [HeaderVersionRoute("/student-advisor-relationships/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAdvisorRelationshipsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAdvisorRelationships>> GetStudentAdvisorRelationshipsByGuidAsync(string guid)
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
                _studentAdvisorRelationshipsService.ValidatePermissions(GetPermissionsMetaData());

                AddEthosContextProperties(
                   await _studentAdvisorRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentAdvisorRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync(guid);
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
        /// Create (POST) a new studentAdvisorRelationships
        /// </summary>
        /// <param name="studentAdvisorRelationships">DTO of the new studentAdvisorRelationships</param>
        /// <returns>A studentAdvisorRelationships object <see cref="Dtos.StudentAdvisorRelationships"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/student-advisor-relationships", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAdvisorRelationshipsV8")]
        [HeaderVersionRoute("/student-advisor-relationships", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAdvisorRelationshipsV10")]
        public async Task<ActionResult<Dtos.StudentAdvisorRelationships>> PostStudentAdvisorRelationshipsAsync([FromBody] Dtos.StudentAdvisorRelationships studentAdvisorRelationships)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentAdvisorRelationships
        /// </summary>
        /// <param name="guid">GUID of the studentAdvisorRelationships to update</param>
        /// <param name="studentAdvisorRelationships">DTO of the updated studentAdvisorRelationships</param>
        /// <returns>A studentAdvisorRelationships object <see cref="Dtos.StudentAdvisorRelationships"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/student-advisor-relationships/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAdvisorRelationshipsV8")]
        [HeaderVersionRoute("/student-advisor-relationships/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAdvisorRelationshipsV10")]
        public async Task<ActionResult<Dtos.StudentAdvisorRelationships>> PutStudentAdvisorRelationshipsAsync([FromRoute] string guid, [FromBody] Dtos.StudentAdvisorRelationships studentAdvisorRelationships)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentAdvisorRelationships
        /// </summary>
        /// <param name="guid">GUID to desired studentAdvisorRelationships</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/student-advisor-relationships/{guid}", Name = "DefaultDeleteStudentAdvisorRelationships", Order = -10)]
        public async Task<IActionResult> DeleteStudentAdvisorRelationshipsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

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
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAcademicPeriods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAcademicPeriodsController : BaseCompressedApiController
    {
        private readonly IStudentAcademicPeriodsService _studentAcademicPeriodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAcademicPeriodsController class.
        /// </summary>
        /// <param name="studentAcademicPeriodsService">Service of type <see cref="IStudentAcademicPeriodsService">IStudentAcademicPeriodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentAcademicPeriodsController(IStudentAcademicPeriodsService studentAcademicPeriodsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentAcademicPeriodsService = studentAcademicPeriodsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentAcademicPeriods
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria"></param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of StudentAcademicPeriods <see cref="Dtos.StudentAcademicPeriods"/> objects representing matching studentAcademicPeriods</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicPeriods)]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAcademicPeriods))]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-periods", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAcademicPeriods", IsEedmSupported = true, IsBulkSupported = true)]
        public async Task<IActionResult> GetStudentAcademicPeriodsAsync(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
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
                _studentAcademicPeriodsService.ValidatePermissions(GetPermissionsMetaData());
                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if ((personFilterObj != null) && (personFilterObj.personFilter != null))
                {
                    personFilterValue = personFilterObj.personFilter.Id;
                }
                
                var filters = GetFilterObject<Dtos.StudentAcademicPeriods>(_logger, "criteria");
               
                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentAcademicPeriods>>(new List<Dtos.StudentAcademicPeriods>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _studentAcademicPeriodsService.GetStudentAcademicPeriodsAsync(page.Offset, page.Limit, 
                    personFilterValue, filters, bypassCache);

                AddEthosContextProperties(
                  await _studentAcademicPeriodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentAcademicPeriodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentAcademicPeriods>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a studentAcademicPeriods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicPeriods</param>
        /// <returns>A studentAcademicPeriods object <see cref="Dtos.StudentAcademicPeriods"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicPeriods)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-periods/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAcademicPeriodsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAcademicPeriods>> GetStudentAcademicPeriodsByGuidAsync(string guid)
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
                _studentAcademicPeriodsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _studentAcademicPeriodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentAcademicPeriodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentAcademicPeriodsService.GetStudentAcademicPeriodsByGuidAsync(guid);
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
        /// Create (POST) a new studentAcademicPeriods
        /// </summary>
        /// <param name="studentAcademicPeriods">DTO of the new studentAcademicPeriods</param>
        /// <returns>A studentAcademicPeriods object <see cref="Dtos.StudentAcademicPeriods"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-academic-periods", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAcademicPeriodsV1.0.0")]
        public async Task<ActionResult<Dtos.StudentAcademicPeriods>> PostStudentAcademicPeriodsAsync([FromBody] Dtos.StudentAcademicPeriods studentAcademicPeriods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentAcademicPeriods
        /// </summary>
        /// <param name="guid">GUID of the studentAcademicPeriods to update</param>
        /// <param name="studentAcademicPeriods">DTO of the updated studentAcademicPeriods</param>
        /// <returns>A studentAcademicPeriods object <see cref="Dtos.StudentAcademicPeriods"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-academic-periods/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAcademicPeriodsV1.0.0")]
        public async Task<ActionResult<Dtos.StudentAcademicPeriods>> PutStudentAcademicPeriodsAsync([FromRoute] string guid, [FromBody] Dtos.StudentAcademicPeriods studentAcademicPeriods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentAcademicPeriods
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicPeriods</param>
        [HttpDelete]
        [Route("/student-academic-periods/{guid}", Name = "DefaultDeleteStudentAcademicPeriods", Order = -10)]
        public async Task<IActionResult> DeleteStudentAcademicPeriodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

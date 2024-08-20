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
    /// Provides access to StudentFinancialAidAcademicProgressStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidAcademicProgressStatusesController : BaseCompressedApiController
    {
        private readonly IStudentFinancialAidAcademicProgressStatusesService _studentFinancialAidAcademicProgressStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentFinancialAidAcademicProgressStatusesController class.
        /// </summary>
        /// <param name="studentFinancialAidAcademicProgressStatusesService">Service of type <see cref="IStudentFinancialAidAcademicProgressStatusesService">IStudentFinancialAidAcademicProgressStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentFinancialAidAcademicProgressStatusesController(IStudentFinancialAidAcademicProgressStatusesService studentFinancialAidAcademicProgressStatusesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentFinancialAidAcademicProgressStatusesService = studentFinancialAidAcademicProgressStatusesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentFinancialAidAcademicProgressStatuses
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>        
        /// <param name="criteria">StudentFinancialAidAcademicProgressStatuses search criteria in JSON format.</param>  
        /// <returns>List of StudentFinancialAidAcademicProgressStatuses <see cref="Dtos.StudentFinancialAidAcademicProgressStatuses"/> objects representing matching studentFinancialAidAcademicProgressStatuses</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAcadProgress)]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentFinancialAidAcademicProgressStatuses))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HeaderVersionRoute("/student-financial-aid-academic-progress-statuses", "15", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentFinancialAidAcademicProgressStatuses", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentFinancialAidAcademicProgressStatusesAsync(Paging page, QueryStringFilter criteria)
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
                _studentFinancialAidAcademicProgressStatusesService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var criteriaObject = GetFilterObject<Dtos.StudentFinancialAidAcademicProgressStatuses>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAcademicProgressStatuses>>(new List<Dtos.StudentFinancialAidAcademicProgressStatuses>(),
                        page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                
                var pageOfItems = await _studentFinancialAidAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(page.Offset, page.Limit, criteriaObject, bypassCache);

                AddEthosContextProperties(
                  await _studentFinancialAidAcademicProgressStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentFinancialAidAcademicProgressStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAcademicProgressStatuses>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Read (GET) a studentFinancialAidAcademicProgressStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentFinancialAidAcademicProgressStatuses</param>
        /// <returns>A studentFinancialAidAcademicProgressStatuses object <see cref="Dtos.StudentFinancialAidAcademicProgressStatuses"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAcadProgress)]
        [HeaderVersionRoute("/student-financial-aid-academic-progress-statuses/{guid}", "15", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentFinancialAidAcademicProgressStatusesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentFinancialAidAcademicProgressStatuses>> GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(string guid)
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
                _studentFinancialAidAcademicProgressStatusesService.ValidatePermissions(GetPermissionsMetaData());
                //AddDataPrivacyContextProperty((await _studentFinancialAidAcademicProgressStatusesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                   await _studentFinancialAidAcademicProgressStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentFinancialAidAcademicProgressStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentFinancialAidAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(guid);
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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Create (POST) a new studentFinancialAidAcademicProgressStatuses
        /// </summary>
        /// <param name="studentFinancialAidAcademicProgressStatuses">DTO of the new studentFinancialAidAcademicProgressStatuses</param>
        /// <returns>A studentFinancialAidAcademicProgressStatuses object <see cref="Dtos.StudentFinancialAidAcademicProgressStatuses"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-financial-aid-academic-progress-statuses", "15", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentFinancialAidAcademicProgressStatusesV15")]
        public IActionResult PostStudentFinancialAidAcademicProgressStatusesAsync([FromBody] Dtos.StudentFinancialAidAcademicProgressStatuses studentFinancialAidAcademicProgressStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing studentFinancialAidAcademicProgressStatuses
        /// </summary>
        /// <param name="guid">GUID of the studentFinancialAidAcademicProgressStatuses to update</param>
        /// <param name="studentFinancialAidAcademicProgressStatuses">DTO of the updated studentFinancialAidAcademicProgressStatuses</param>
        /// <returns>A studentFinancialAidAcademicProgressStatuses object <see cref="Dtos.StudentFinancialAidAcademicProgressStatuses"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-financial-aid-academic-progress-statuses/{guid}", "15", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentFinancialAidAcademicProgressStatusesV15")]
        public IActionResult PutStudentFinancialAidAcademicProgressStatusesAsync([FromRoute] string guid, [FromBody] Dtos.StudentFinancialAidAcademicProgressStatuses studentFinancialAidAcademicProgressStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentFinancialAidAcademicProgressStatuses
        /// </summary>
        /// <param name="guid">GUID to desired studentFinancialAidAcademicProgressStatuses</param>
        [HttpDelete]
        [Route("/student-financial-aid-academic-progress-statuses/{guid}", Name = "DefaultDeleteStudentFinancialAidAcademicProgressStatuses", Order = -10)]
        public IActionResult DeleteStudentFinancialAidAcademicProgressStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

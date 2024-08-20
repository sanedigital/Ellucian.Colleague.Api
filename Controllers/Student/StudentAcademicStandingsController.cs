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
using Ellucian.Colleague.Coordination.Student;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAcademicStandings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAcademicStandingsController : BaseCompressedApiController
    {
        private readonly IStudentAcademicStandingsService _studentAcademicStandingsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAcademicStandingsController class.
        /// </summary>
        /// <param name="studentAcademicStandingsService">Service of type <see cref="IStudentAcademicStandingsService">IStudentAcademicStandingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentAcademicStandingsController(IStudentAcademicStandingsService studentAcademicStandingsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentAcademicStandingsService = studentAcademicStandingsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentAcademicStandings
        /// </summary>
        /// <returns>List of StudentAcademicStandings <see cref="Dtos.StudentAcademicStandings"/> objects representing matching studentAcademicStandings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcadStandings } )]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/student-academic-standings", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAcademicStandings", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentAcademicStandingsAsync(Paging page)
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
                _studentAcademicStandingsService.ValidatePermissions(GetPermissionsMetaData());

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _studentAcademicStandingsService.GetStudentAcademicStandingsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _studentAcademicStandingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAcademicStandingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentAcademicStandings>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a studentAcademicStandings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicStandings</param>
        /// <returns>A studentAcademicStandings object <see cref="Dtos.StudentAcademicStandings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcadStandings })]
        [HeaderVersionRoute("/student-academic-standings/{guid}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAcademicStandingsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAcademicStandings>> GetStudentAcademicStandingsByGuidAsync(string guid)
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
                _studentAcademicStandingsService.ValidatePermissions(GetPermissionsMetaData());

                AddEthosContextProperties(
                    await _studentAcademicStandingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAcademicStandingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _studentAcademicStandingsService.GetStudentAcademicStandingsByGuidAsync(guid);
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
        /// Create (POST) a new studentAcademicStandings
        /// </summary>
        /// <param name="studentAcademicStandings">DTO of the new studentAcademicStandings</param>
        /// <returns>A studentAcademicStandings object <see cref="Dtos.StudentAcademicStandings"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-academic-standings", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAcademicStandingsV8")]
        public async Task<ActionResult<Dtos.StudentAcademicStandings>> PostStudentAcademicStandingsAsync([FromBody] Dtos.StudentAcademicStandings studentAcademicStandings)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentAcademicStandings
        /// </summary>
        /// <param name="guid">GUID of the studentAcademicStandings to update</param>
        /// <param name="studentAcademicStandings">DTO of the updated studentAcademicStandings</param>
        /// <returns>A studentAcademicStandings object <see cref="Dtos.StudentAcademicStandings"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-academic-standings/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAcademicStandingsV8")]
        public async Task<ActionResult<Dtos.StudentAcademicStandings>> PutStudentAcademicStandingsAsync([FromRoute] string guid, [FromBody] Dtos.StudentAcademicStandings studentAcademicStandings)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentAcademicStandings
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicStandings</param>
        [HttpDelete]
        [Route("/student-academic-standings/{guid}", Name = "DefaultDeleteStudentAcademicStandings", Order = -10)]
        public async Task<IActionResult> DeleteStudentAcademicStandingsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

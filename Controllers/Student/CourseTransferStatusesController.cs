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
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to CourseTransferStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CourseTransferStatusesController : BaseCompressedApiController
    {
        private readonly ICourseTransferStatusesService _courseTransferStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CourseTransferStatusesController class.
        /// </summary>
        /// <param name="courseTransferStatusesService">Service of type <see cref="ICourseTransferStatusesService">ICourseTransferStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CourseTransferStatusesController(ICourseTransferStatusesService courseTransferStatusesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _courseTransferStatusesService = courseTransferStatusesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all courseTransferStatuses
        /// </summary>
        /// <returns>List of CourseTransferStatuses <see cref="Dtos.CourseTransferStatuses"/> objects representing matching courseTransferStatuses</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/course-transfer-statuses", "13", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetCourseTransferStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CourseTransferStatuses>>> GetCourseTransferStatusesAsync()
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
                var coursetranferstatusesDtos = await _courseTransferStatusesService.GetCourseTransferStatusesAsync(bypassCache);


                AddEthosContextProperties(await _courseTransferStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _courseTransferStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          coursetranferstatusesDtos.Select(dd => dd.Id).ToList()));
                return Ok(coursetranferstatusesDtos);
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
        /// Read (GET) a courseTransferStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired courseTransferStatuses</param>
        /// <returns>A courseTransferStatuses object <see cref="Dtos.CourseTransferStatuses"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/course-transfer-statuses/{guid}", "13", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCourseTransferStatusesByGuid")]
        public async Task<ActionResult<Dtos.CourseTransferStatuses>> GetCourseTransferStatusesByGuidAsync(string guid)
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
                var courseTranferStatusDto = await _courseTransferStatusesService.GetCourseTransferStatusesByGuidAsync(guid);
                if (courseTranferStatusDto == null)
                {
                    throw new KeyNotFoundException("Course transfer status not found for GUID " + guid);
                }

                AddEthosContextProperties(await _courseTransferStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _courseTransferStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          new List<string>() { courseTranferStatusDto.Id }));
                return courseTranferStatusDto;
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
        /// Create (POST) a new courseTransferStatuses
        /// </summary>
        /// <param name="courseTransferStatuses">DTO of the new courseTransferStatuses</param>
        /// <returns>A courseTransferStatuses object <see cref="Dtos.CourseTransferStatuses"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/course-transfer-statuses", "13", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCourseTransferStatusesV13")]
        public async Task<ActionResult<Dtos.CourseTransferStatuses>> PostCourseTransferStatusesAsync([FromBody] Dtos.CourseTransferStatuses courseTransferStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing courseTransferStatuses
        /// </summary>
        /// <param name="guid">GUID of the courseTransferStatuses to update</param>
        /// <param name="courseTransferStatuses">DTO of the updated courseTransferStatuses</param>
        /// <returns>A courseTransferStatuses object <see cref="Dtos.CourseTransferStatuses"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/course-transfer-statuses/{guid}", "13", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCourseTransferStatusesV13")]
        public async Task<ActionResult<Dtos.CourseTransferStatuses>> PutCourseTransferStatusesAsync([FromRoute] string guid, [FromBody] Dtos.CourseTransferStatuses courseTransferStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a courseTransferStatuses
        /// </summary>
        /// <param name="guid">GUID to desired courseTransferStatuses</param>
        [HttpDelete]
        [Route("/course-transfer-statuses/{guid}", Name = "DefaultDeleteCourseTransferStatuses", Order = -10)]
        public async Task<IActionResult> DeleteCourseTransferStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

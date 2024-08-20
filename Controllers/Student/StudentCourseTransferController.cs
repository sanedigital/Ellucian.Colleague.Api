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
using System.Linq;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentCourseTransfer
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentCourseTransferController : BaseCompressedApiController
    {
        private readonly IStudentCourseTransferService _StudentCourseTransferService;
        private readonly ILogger _logger;
        private int offset, limit;

        /// <summary>
        /// Initializes a new instance of the StudentCourseTransferController class.
        /// </summary>
        /// <param name="StudentCourseTransferService">Service of type <see cref="IStudentCourseTransferService">IStudentCourseTransferService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentCourseTransferController(IStudentCourseTransferService StudentCourseTransferService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _StudentCourseTransferService = StudentCourseTransferService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all StudentCourseTransfer
        /// </summary>
        /// <returns>List of StudentCourseTransfer <see cref="Dtos.StudentCourseTransfer"/> objects representing matching StudentCourseTransfer</returns>
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentCourseTransfers })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/student-course-transfers", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentCourseTransferV11", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentCourseTransfersAsync(Paging page, bool ignoreCache = false)
        {
            var bypassCache = false;
            

            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            
            if (page == null)
            {
                offset = 0;
                limit = 100;
            }
            else
            {
                offset = page.Offset;
                limit = page.Limit;
            }
            try
            {
                _StudentCourseTransferService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _StudentCourseTransferService.GetStudentCourseTransfersAsync(offset, limit, bypassCache);

                AddEthosContextProperties(
                    await _StudentCourseTransferService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _StudentCourseTransferService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentCourseTransfer>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);


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
        /// Read (GET) a StudentCourseTransfer using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired StudentCourseTransfer</param>
        /// <returns>A StudentCourseTransfer object <see cref="Dtos.StudentCourseTransfer"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentCourseTransfers })]
        [HttpGet]
        [HeaderVersionRoute("/student-course-transfers/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentCourseTransferByGuidV11")]
        public async Task<ActionResult<Dtos.StudentCourseTransfer>> GetStudentCourseTransferByGuidAsync(string guid)
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
                _StudentCourseTransferService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _StudentCourseTransferService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _StudentCourseTransferService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _StudentCourseTransferService.GetStudentCourseTransferByGuidAsync(guid, bypassCache);
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
        /// Return all StudentCourseTransfer
        /// </summary>
        /// <returns>List of StudentCourseTransfer <see cref="Dtos.StudentCourseTransfer"/> objects representing matching StudentCourseTransfer</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentCourseTransfers })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/student-course-transfers", "13", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentCourseTransfer", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentCourseTransfers2Async(Paging page, bool ignoreCache = false)
        {
            var bypassCache = false;


            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                offset = 0;
                limit = 100;
            }
            else
            {
                offset = page.Offset;
                limit = page.Limit;
            }
            try
            {
                _StudentCourseTransferService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _StudentCourseTransferService.GetStudentCourseTransfers2Async(offset, limit, bypassCache);

                AddEthosContextProperties(
                    await _StudentCourseTransferService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _StudentCourseTransferService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentCourseTransfer>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);


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
        /// Read (GET) a StudentCourseTransfer using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired StudentCourseTransfer</param>
        /// <returns>A StudentCourseTransfer object <see cref="Dtos.StudentCourseTransfer"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentCourseTransfers })]
        [HttpGet]
        [HeaderVersionRoute("/student-course-transfers/{guid}", "13", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentCourseTransferByGuid")]
        public async Task<ActionResult<Dtos.StudentCourseTransfer>> GetStudentCourseTransfer2ByGuidAsync(string guid)
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
                _StudentCourseTransferService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _StudentCourseTransferService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _StudentCourseTransferService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _StudentCourseTransferService.GetStudentCourseTransfer2ByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new StudentCourseTransfer
        /// </summary>
        /// <param name="StudentCourseTransfer">DTO of the new StudentCourseTransfer</param>
        /// <returns>A StudentCourseTransfer object <see cref="Dtos.StudentCourseTransfer"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-course-transfers", "13", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentCourseTransferV13")]
        [HeaderVersionRoute("/student-course-transfers", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentCourseTransferV11")]
        public async Task<ActionResult<Dtos.StudentCourseTransfer>> PostStudentCourseTransferAsync([FromBody] Dtos.StudentCourseTransfer StudentCourseTransfer)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing StudentCourseTransfer
        /// </summary>
        /// <param name="guid">GUID of the StudentCourseTransfer to update</param>
        /// <param name="StudentCourseTransfer">DTO of the updated StudentCourseTransfer</param>
        /// <returns>A StudentCourseTransfer object <see cref="Dtos.StudentCourseTransfer"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-course-transfers/{guid}", "13", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentCourseTransferV13")]
        [HeaderVersionRoute("/student-course-transfers/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentCourseTransferV11")]
        public async Task<ActionResult<Dtos.StudentCourseTransfer>> PutStudentCourseTransferAsync([FromRoute] string guid, [FromBody] Dtos.StudentCourseTransfer StudentCourseTransfer)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a StudentCourseTransfer
        /// </summary>
        /// <param name="guid">GUID to desired StudentCourseTransfer</param>
        [HttpDelete]
        [Route("/student-course-transfers/{guid}", Name = "DefaultDeleteStudentCourseTransfer", Order = -10)]
        public async Task<IActionResult> DeleteStudentCourseTransferAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

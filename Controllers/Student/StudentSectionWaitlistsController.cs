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
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http.Configuration;
using Microsoft.Extensions.Options;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentSectionWaitlists
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentSectionWaitlistsController : BaseCompressedApiController
    {
        private readonly IStudentSectionWaitlistsService _studentSectionWaitlistsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentSectionWaitlistsController class.
        /// </summary>
        /// <param name="studentSectionWaitlistsService">Service of type <see cref="IStudentSectionWaitlistsService">IStudentSectionWaitlistsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentSectionWaitlistsController(IStudentSectionWaitlistsService studentSectionWaitlistsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentSectionWaitlistsService = studentSectionWaitlistsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentSectionWaitlists
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of StudentSectionWaitlists <see cref="Dtos.StudentSectionWaitlist"/> objects representing matching studentSectionWaitlists</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentSectionWaitlist })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/student-section-waitlists", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentSectionWaitlists", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentSectionWaitlistsAsync(Paging page)
        {
            try
            {
                _studentSectionWaitlistsService.ValidatePermissions(GetPermissionsMetaData());

                var bypassCache = true;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _studentSectionWaitlistsService.GetStudentSectionWaitlistsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _studentSectionWaitlistsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentSectionWaitlistsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentSectionWaitlist>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a studentSectionWaitlists using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentSectionWaitlists</param>
        /// <returns>A studentSectionWaitlists object <see cref="Dtos.StudentSectionWaitlist"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentSectionWaitlist }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-section-waitlists/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentSectionWaitlistsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentSectionWaitlist>> GetStudentSectionWaitlistsByGuidAsync(string guid)
        {
            try
            {
                _studentSectionWaitlistsService.ValidatePermissions(GetPermissionsMetaData());

                var bypassCache = true;
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
                var item = await _studentSectionWaitlistsService.GetStudentSectionWaitlistsByGuidAsync(guid);

                if (item != null)
                {

                    AddEthosContextProperties(await _studentSectionWaitlistsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentSectionWaitlistsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { item.Id }));
                }

                return item;
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
        /// Create (POST) a new studentSectionWaitlists
        /// </summary>
        /// <param name="studentSectionWaitlists">DTO of the new studentSectionWaitlists</param>
        /// <returns>A studentSectionWaitlists object <see cref="Dtos.StudentSectionWaitlists"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-section-waitlists", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentSectionWaitlistsV10")]
        public async Task<ActionResult<Dtos.StudentSectionWaitlist>> PostStudentSectionWaitlistsAsync([FromBody] Dtos.StudentSectionWaitlist studentSectionWaitlists)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentSectionWaitlists
        /// </summary>
        /// <param name="guid">GUID of the studentSectionWaitlists to update</param>
        /// <param name="studentSectionWaitlists">DTO of the updated studentSectionWaitlists</param>
        /// <returns>A studentSectionWaitlists object <see cref="Dtos.StudentSectionWaitlists"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-section-waitlists/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentSectionWaitlistsV10")]
        public async Task<ActionResult<Dtos.StudentSectionWaitlist>> PutStudentSectionWaitlistsAsync([FromRoute] string guid, [FromBody] Dtos.StudentSectionWaitlist studentSectionWaitlists)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentSectionWaitlists
        /// </summary>
        /// <param name="guid">GUID to desired studentSectionWaitlists</param>
        [HttpDelete]
        [Route("/student-section-waitlists/{guid}", Name = "DefaultDeleteStudentSectionWaitlists", Order = -10)]
        public async Task<IActionResult> DeleteStudentSectionWaitlistsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

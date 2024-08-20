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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to JobApplications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class JobApplicationsController : BaseCompressedApiController
    {
        private readonly IJobApplicationsService _jobApplicationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the JobApplicationsController class.
        /// </summary>
        /// <param name="jobApplicationsService">Service of type <see cref="IJobApplicationsService">IJobApplicationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public JobApplicationsController(IJobApplicationsService jobApplicationsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _jobApplicationsService = jobApplicationsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all jobApplications
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of JobApplications <see cref="Dtos.JobApplications"/> objects representing matching jobApplications</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(HumanResourcesPermissionCodes.ViewJobApplications)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/job-applications", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetJobApplications", IsEedmSupported = true)]
        public async Task<IActionResult> GetJobApplicationsAsync(Paging page)
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
                _jobApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _jobApplicationsService.GetJobApplicationsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _jobApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _jobApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.JobApplications>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a jobApplications using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired jobApplications</param>
        /// <returns>A jobApplications object <see cref="Dtos.JobApplications"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(HumanResourcesPermissionCodes.ViewJobApplications)]
        [HttpGet]
        [HeaderVersionRoute("/job-applications/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetJobApplicationsByGuid")]
        public async Task<ActionResult<Dtos.JobApplications>> GetJobApplicationsByGuidAsync(string guid)
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
                _jobApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _jobApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _jobApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _jobApplicationsService.GetJobApplicationsByGuidAsync(guid);
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
        /// Create (POST) a new jobApplications
        /// </summary>
        /// <param name="jobApplications">DTO of the new jobApplications</param>
        /// <returns>A jobApplications object <see cref="Dtos.JobApplications"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/job-applications", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostJobApplicationsV10")]
        public async Task<ActionResult<Dtos.JobApplications>> PostJobApplicationsAsync([FromBody] Dtos.JobApplications jobApplications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing jobApplications
        /// </summary>
        /// <param name="guid">GUID of the jobApplications to update</param>
        /// <param name="jobApplications">DTO of the updated jobApplications</param>
        /// <returns>A jobApplications object <see cref="Dtos.JobApplications"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/job-applications/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutJobApplicationsV10")]
        public async Task<ActionResult<Dtos.JobApplications>> PutJobApplicationsAsync([FromRoute] string guid, [FromBody] Dtos.JobApplications jobApplications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a jobApplications
        /// </summary>
        /// <param name="guid">GUID to desired jobApplications</param>
        [HttpDelete]
        [Route("/job-applications/{guid}", Name = "DefaultDeleteJobApplications")]
        public async Task<IActionResult> DeleteJobApplicationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

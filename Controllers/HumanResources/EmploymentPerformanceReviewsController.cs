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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Net.Http;
using Ellucian.Colleague.Domain.Base.Exceptions;

using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmploymentPerformanceReviews
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentPerformanceReviewsController : BaseCompressedApiController
    {
        private readonly IEmploymentPerformanceReviewsService _employmentPerformanceReviewsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentPerformanceReviewsController class.
        /// </summary>
        /// <param name="employmentPerformanceReviewsService">Service of type <see cref="IEmploymentPerformanceReviewsService">IEmploymentPerformanceReviewsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmploymentPerformanceReviewsController(IEmploymentPerformanceReviewsService employmentPerformanceReviewsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _employmentPerformanceReviewsService = employmentPerformanceReviewsService;
            this._logger = logger;
        }

        #region GET Methods
        /// <summary>
        /// Return all employmentPerformanceReviews
        /// </summary>
        /// <param name="page">Page of items for Paging</param>
        /// <returns>List of EmploymentPerformanceReviews <see cref="Dtos.EmploymentPerformanceReviews"/> objects representing matching employmentPerformanceReviews</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewEmploymentPerformanceReview,
            HumanResourcesPermissionCodes.CreateUpdateEmploymentPerformanceReview })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/employment-performance-reviews", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEmploymentPerformanceReviews", IsEedmSupported = true)]
        public async Task<IActionResult> GetEmploymentPerformanceReviewsAsync(Paging page)
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
                page = new Paging(100, 0);
            }

            try
            {
                _employmentPerformanceReviewsService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _employmentPerformanceReviewsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _employmentPerformanceReviewsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.EmploymentPerformanceReviews>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a employmentPerformanceReviews using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employmentPerformanceReviews</param>
        /// <returns>A employmentPerformanceReviews object <see cref="Dtos.EmploymentPerformanceReviews"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewEmploymentPerformanceReview,
            HumanResourcesPermissionCodes.CreateUpdateEmploymentPerformanceReview })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/employment-performance-reviews/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmploymentPerformanceReviewsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmploymentPerformanceReviews>> GetEmploymentPerformanceReviewsByGuidAsync(string guid)
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
                _employmentPerformanceReviewsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _employmentPerformanceReviewsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _employmentPerformanceReviewsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsByGuidAsync(guid);
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
        #endregion

        #region POST Method
        /// <summary>
        /// Create (POST) a new employmentPerformanceReviews
        /// </summary>
        /// <param name="employmentPerformanceReviews">DTO of the new employmentPerformanceReviews</param>
        /// <returns>An EmploymentPerformanceReviews DTO object <see cref="Dtos.EmploymentPerformanceReviews"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)),PermissionsFilter(HumanResourcesPermissionCodes.CreateUpdateEmploymentPerformanceReview) ]
        [HttpPost]
        [HeaderVersionRoute("/employment-performance-reviews", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEmploymentPerformanceReviewsV10", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmploymentPerformanceReviews>> PostEmploymentPerformanceReviewsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.EmploymentPerformanceReviews employmentPerformanceReviews)
        {

            if (employmentPerformanceReviews == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null employmentPerformanceReviews argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            try
            {
                _employmentPerformanceReviewsService.ValidatePermissions(GetPermissionsMetaData());
                ValidateEmploymentPerformanceReviews(employmentPerformanceReviews);

                //call import extend method that needs the extracted extension data and the config
                await _employmentPerformanceReviewsService.ImportExtendedEthosData(await ExtractExtendedData(await _employmentPerformanceReviewsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                // Create the performance review
                var performanceReview = await _employmentPerformanceReviewsService.PostEmploymentPerformanceReviewsAsync(employmentPerformanceReviews);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _employmentPerformanceReviewsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _employmentPerformanceReviewsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { performanceReview.Id }));

                return performanceReview;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
            catch (ConfigurationException e)
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
        #endregion

        #region PUT Method
        /// <summary>
        /// Update (PUT) an existing employmentPerformanceReviews
        /// </summary>
        /// <param name="guid">GUID of the employmentPerformanceReviews to update</param>
        /// <param name="employmentPerformanceReviews">DTO of the updated employmentPerformanceReviews</param>
        /// <returns>An EmploymentPerformanceReviews DTO object <see cref="Dtos.EmploymentPerformanceReviews"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(HumanResourcesPermissionCodes.CreateUpdateEmploymentPerformanceReview)]
        [HttpPut]
        [HeaderVersionRoute("/employment-performance-reviews/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmploymentPerformanceReviewsV10", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmploymentPerformanceReviews>> PutEmploymentPerformanceReviewsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.EmploymentPerformanceReviews employmentPerformanceReviews)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (employmentPerformanceReviews == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null employmentPerformanceReviews argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(employmentPerformanceReviews.Id))
            {
                employmentPerformanceReviews.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(employmentPerformanceReviews.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != employmentPerformanceReviews.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _employmentPerformanceReviewsService.ValidatePermissions(GetPermissionsMetaData());
                await DoesUpdateViolateDataPrivacySettings(employmentPerformanceReviews, await _employmentPerformanceReviewsService.GetDataPrivacyListByApi(GetRouteResourceName(), true), _logger);

                ValidateEmploymentPerformanceReviews(employmentPerformanceReviews);
                
                //get Data Privacy List
                var dpList = await _employmentPerformanceReviewsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _employmentPerformanceReviewsService.ImportExtendedEthosData(await ExtractExtendedData(await _employmentPerformanceReviewsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                // Update the performance review
                var performanceReview = await _employmentPerformanceReviewsService.PutEmploymentPerformanceReviewsAsync(guid,
                    await PerformPartialPayloadMerge(employmentPerformanceReviews,
                    async() => await _employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsByGuidAsync(guid), dpList, _logger));

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(dpList,
                   await _employmentPerformanceReviewsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { performanceReview.Id }));

                return performanceReview;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
            catch (ConfigurationException e)
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
        #endregion

        #region DELETE method
        /// <summary>
        /// Deletes employment performance review based on id
        /// </summary>
        /// <param name="guid">id</param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete, PermissionsFilter(HumanResourcesPermissionCodes.DeleteEmploymentPerformanceReview)]
        [HttpDelete]
        [Route("/employment-performance-reviews/{guid}", Name = "DefaultDeleteEmploymentPerformanceReviews")]
        public async Task<IActionResult> DeleteEmploymentPerformanceReviewsAsync(string guid)
        {
            try
            {
                _employmentPerformanceReviewsService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("Employment performance review guid cannot be null or empty");
                }
                await _employmentPerformanceReviewsService.DeleteEmploymentPerformanceReviewAsync(guid);
                return NoContent();
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (RepositoryException e)
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
        #endregion

        #region Helper Methods
        /// <summary>
        /// Helper method to validate employment-performance-reviews PUT/POST.
        /// </summary>
        /// <param name="employmentPerformanceReviews"><see cref="Dtos.EmploymentPerformanceReviews"/>EmploymentPerformanceReviews DTO object of type</param>
        private void ValidateEmploymentPerformanceReviews(Dtos.EmploymentPerformanceReviews employmentPerformanceReviews)
        {

            if (employmentPerformanceReviews == null)
            {
                throw new ArgumentNullException("employmentPerformanceReviews", "The body is required when submitting an employmentPerformanceReviews. ");
            }

        }
        #endregion
    }
}

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
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmploymentPerformanceReviewRatings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentPerformanceReviewRatingsController : BaseCompressedApiController
    {
        private readonly IEmploymentPerformanceReviewRatingsService _employmentPerformanceReviewRatingsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentPerformanceReviewRatingsController class.
        /// </summary>
        /// <param name="employmentPerformanceReviewRatingsService">Service of type <see cref="IEmploymentPerformanceReviewRatingsService">IEmploymentPerformanceReviewRatingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmploymentPerformanceReviewRatingsController(IEmploymentPerformanceReviewRatingsService employmentPerformanceReviewRatingsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _employmentPerformanceReviewRatingsService = employmentPerformanceReviewRatingsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all employmentPerformanceReviewRatings
        /// </summary>
        /// <returns>List of EmploymentPerformanceReviewRatings <see cref="Dtos.EmploymentPerformanceReviewRatings"/> objects representing matching employmentPerformanceReviewRatings</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/employment-performance-review-ratings", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEmploymentPerformanceReviewRatings", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings>>> GetEmploymentPerformanceReviewRatingsAsync()
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
                var employmentPerformanceReviewRatings = await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsAsync(bypassCache);

                if (employmentPerformanceReviewRatings != null && employmentPerformanceReviewRatings.Any())
                {
                    AddEthosContextProperties(await _employmentPerformanceReviewRatingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _employmentPerformanceReviewRatingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              employmentPerformanceReviewRatings.Select(a => a.Id).ToList()));
                }
                return Ok(employmentPerformanceReviewRatings);
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
        /// Read (GET) a employmentPerformanceReviewRatings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employmentPerformanceReviewRatings</param>
        /// <returns>A employmentPerformanceReviewRatings object <see cref="Dtos.EmploymentPerformanceReviewRatings"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/employment-performance-review-ratings/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmploymentPerformanceReviewRatingsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmploymentPerformanceReviewRatings>> GetEmploymentPerformanceReviewRatingsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                   await _employmentPerformanceReviewRatingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _employmentPerformanceReviewRatingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsByGuidAsync(guid);
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
        /// Create (POST) a new employmentPerformanceReviewRatings
        /// </summary>
        /// <param name="employmentPerformanceReviewRatings">DTO of the new employmentPerformanceReviewRatings</param>
        /// <returns>A employmentPerformanceReviewRatings object <see cref="Dtos.EmploymentPerformanceReviewRatings"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/employment-performance-review-ratings", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEmploymentPerformanceReviewRatingsV10")]
        public async Task<ActionResult<Dtos.EmploymentPerformanceReviewRatings>> PostEmploymentPerformanceReviewRatingsAsync([FromBody] Dtos.EmploymentPerformanceReviewRatings employmentPerformanceReviewRatings)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employmentPerformanceReviewRatings
        /// </summary>
        /// <param name="guid">GUID of the employmentPerformanceReviewRatings to update</param>
        /// <param name="employmentPerformanceReviewRatings">DTO of the updated employmentPerformanceReviewRatings</param>
        /// <returns>A employmentPerformanceReviewRatings object <see cref="Dtos.EmploymentPerformanceReviewRatings"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/employment-performance-review-ratings/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmploymentPerformanceReviewRatingsV10")]
        public async Task<ActionResult<Dtos.EmploymentPerformanceReviewRatings>> PutEmploymentPerformanceReviewRatingsAsync([FromRoute] string guid, [FromBody] Dtos.EmploymentPerformanceReviewRatings employmentPerformanceReviewRatings)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employmentPerformanceReviewRatings
        /// </summary>
        /// <param name="guid">GUID to desired employmentPerformanceReviewRatings</param>
        [HttpDelete]
        [Route("/employment-performance-review-ratings/{guid}", Name = "DefaultDeleteEmploymentPerformanceReviewRatings")]
        public async Task<IActionResult> DeleteEmploymentPerformanceReviewRatingsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

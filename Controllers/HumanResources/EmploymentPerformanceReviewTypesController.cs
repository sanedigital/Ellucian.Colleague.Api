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

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmploymentPerformanceReviewTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentPerformanceReviewTypesController : BaseCompressedApiController
    {
        private readonly IEmploymentPerformanceReviewTypesService _employmentPerformanceReviewTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentPerformanceReviewTypesController class.
        /// </summary>
        /// <param name="employmentPerformanceReviewTypesService">Service of type <see cref="IEmploymentPerformanceReviewTypesService">IEmploymentPerformanceReviewTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmploymentPerformanceReviewTypesController(IEmploymentPerformanceReviewTypesService employmentPerformanceReviewTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _employmentPerformanceReviewTypesService = employmentPerformanceReviewTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all employmentPerformanceReviewTypes
        /// </summary>
        /// <returns>List of EmploymentPerformanceReviewTypes <see cref="Dtos.EmploymentPerformanceReviewTypes"/> objects representing matching employmentPerformanceReviewTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/employment-performance-review-types", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmEmploymentPerformanceReviewTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewTypes>>> GetEmploymentPerformanceReviewTypesAsync()
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
                var employmentPerformanceReviewTypes = await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesAsync(bypassCache);

                if (employmentPerformanceReviewTypes != null && employmentPerformanceReviewTypes.Any())
                {
                    AddEthosContextProperties(await _employmentPerformanceReviewTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _employmentPerformanceReviewTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              employmentPerformanceReviewTypes.Select(a => a.Id).ToList()));
                }

                return Ok(employmentPerformanceReviewTypes);                
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
        /// Read (GET) a employmentPerformanceReviewTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employmentPerformanceReviewTypes</param>
        /// <returns>A employmentPerformanceReviewTypes object <see cref="Dtos.EmploymentPerformanceReviewTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/employment-performance-review-types/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmEmploymentPerformanceReviewTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmploymentPerformanceReviewTypes>> GetEmploymentPerformanceReviewTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _employmentPerformanceReviewTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _employmentPerformanceReviewTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesByGuidAsync(guid);
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
        /// Create (POST) a new employmentPerformanceReviewTypes
        /// </summary>
        /// <param name="employmentPerformanceReviewTypes">DTO of the new employmentPerformanceReviewTypes</param>
        /// <returns>A employmentPerformanceReviewTypes object <see cref="Dtos.EmploymentPerformanceReviewTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/employment-performance-review-types", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmEmploymentPerformanceReviewTypes")]
        public async Task<ActionResult<Dtos.EmploymentPerformanceReviewTypes>> PostEmploymentPerformanceReviewTypesAsync([FromBody] Dtos.EmploymentPerformanceReviewTypes employmentPerformanceReviewTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employmentPerformanceReviewTypes
        /// </summary>
        /// <param name="guid">GUID of the employmentPerformanceReviewTypes to update</param>
        /// <param name="employmentPerformanceReviewTypes">DTO of the updated employmentPerformanceReviewTypes</param>
        /// <returns>A employmentPerformanceReviewTypes object <see cref="Dtos.EmploymentPerformanceReviewTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/employment-performance-review-types/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmEmploymentPerformanceReviewTypes")]
        public async Task<ActionResult<Dtos.EmploymentPerformanceReviewTypes>> PutEmploymentPerformanceReviewTypesAsync([FromRoute] string guid, [FromBody] Dtos.EmploymentPerformanceReviewTypes employmentPerformanceReviewTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employmentPerformanceReviewTypes
        /// </summary>
        /// <param name="guid">GUID to desired employmentPerformanceReviewTypes</param>
        [HttpDelete]
        [Route("/employment-performance-review-types/{guid}", Name = "DeleteHedmEmploymentPerformanceReviewTypes")]
        public async Task<IActionResult> DeleteEmploymentPerformanceReviewTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

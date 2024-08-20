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
    /// Provides access to DeductionCategories
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class DeductionCategoriesController : BaseCompressedApiController
    {
        private readonly IDeductionCategoriesService _deductionCategoriesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the DeductionCategoriesController class.
        /// </summary>
        /// <param name="deductionCategoriesService">Service of type <see cref="IDeductionCategoriesService">IDeductionCategoriesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DeductionCategoriesController(IDeductionCategoriesService deductionCategoriesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _deductionCategoriesService = deductionCategoriesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all deductionCategories
        /// </summary>
                /// <returns>List of DeductionCategories <see cref="Dtos.DeductionCategories"/> objects representing matching deductionCategories</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/deduction-categories", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetDeductionCategories", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.DeductionCategories>>> GetDeductionCategoriesAsync()
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
                var deductionCategories = await _deductionCategoriesService.GetDeductionCategoriesAsync(bypassCache);

                if (deductionCategories != null && deductionCategories.Any())
                {
                    AddEthosContextProperties(await _deductionCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _deductionCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              deductionCategories.Select(a => a.Id).ToList()));
                }
                return Ok(deductionCategories);
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
        /// Read (GET) a deductionCategories using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired deductionCategories</param>
        /// <returns>A deductionCategories object <see cref="Dtos.DeductionCategories"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/deduction-categories/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetDeductionCategoriesByGuid")]
        public async Task<ActionResult<Dtos.DeductionCategories>> GetDeductionCategoriesByGuidAsync(string guid)
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
                AddEthosContextProperties(
                    await _deductionCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(),bypassCache),
                    await _deductionCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _deductionCategoriesService.GetDeductionCategoriesByGuidAsync(guid);
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
        /// Create (POST) a new deductionCategories
        /// </summary>
        /// <param name="deductionCategories">DTO of the new deductionCategories</param>
        /// <returns>A deductionCategories object <see cref="Dtos.DeductionCategories"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/deduction-categories", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostDeductionCategoriesV11")]
        public async Task<ActionResult<Dtos.DeductionCategories>> PostDeductionCategoriesAsync([FromBody] Dtos.DeductionCategories deductionCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing deductionCategories
        /// </summary>
        /// <param name="guid">GUID of the deductionCategories to update</param>
        /// <param name="deductionCategories">DTO of the updated deductionCategories</param>
        /// <returns>A deductionCategories object <see cref="Dtos.DeductionCategories"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/deduction-categories/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutDeductionCategoriesV11")]
        public async Task<ActionResult<Dtos.DeductionCategories>> PutDeductionCategoriesAsync([FromRoute] string guid, [FromBody] Dtos.DeductionCategories deductionCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a deductionCategories
        /// </summary>
        /// <param name="guid">GUID to desired deductionCategories</param>
        [HttpDelete]
        [Route("/deduction-categories/{guid}", Name = "DefaultDeleteDeductionCategories")]
        public async Task<IActionResult> DeleteDeductionCategoriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

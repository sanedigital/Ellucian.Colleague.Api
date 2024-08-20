// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
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
    /// Provides access to FinancialAidFundCategories data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidFundCategoriesController : BaseCompressedApiController
    {
        private readonly IFinancialAidFundCategoryService _financialAidFundCategoryService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidFundCategoriesController class.
        /// </summary>
        /// <param name="financialAidFundCategoryService">Repository of type <see cref="IFinancialAidFundCategoryService">IFinancialAidFundCategoryService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidFundCategoriesController(IFinancialAidFundCategoryService financialAidFundCategoryService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _financialAidFundCategoryService = financialAidFundCategoryService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all financial aid fund categories.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All FinancialAidFundCategory objects.</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/financial-aid-fund-categories", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidFundCategories", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidFundCategory>>> GetFinancialAidFundCategoriesAsync()
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var categories = await _financialAidFundCategoryService.GetFinancialAidFundCategoriesAsync(bypassCache);

                if (categories != null && categories.Any()) {
                    AddEthosContextProperties(
                      await _financialAidFundCategoryService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _financialAidFundCategoryService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                          categories.Select(i => i.Id).ToList()));
                }
                return Ok(categories);

            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Retrieves an Financial Aid FundCategories by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.FinancialAidFundCategory">FinancialAidFundCategory</see>object.</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/financial-aid-fund-categories/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidFundCategoryById", IsEedmSupported = true)]
        public async Task<ActionResult<FinancialAidFundCategory>> GetFinancialAidFundCategoryByIdAsync(string id)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                AddEthosContextProperties(
                     await _financialAidFundCategoryService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _financialAidFundCategoryService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         new List<string>() { id }));

                return await _financialAidFundCategoryService.GetFinancialAidFundCategoryByGuidAsync(id);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Creates a Financial Aid Fund Category.
        /// </summary>
        /// <param name="financialAidFundCategory"><see cref="Dtos.FinancialAidFundCategory">FinancialAidFundCategory</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.FinancialAidFundCategory">FinancialAidFundCategory</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/financial-aid-fund-categories", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFinancialAidFundCategoriesV7")]
        public async Task<ActionResult<Dtos.FinancialAidFundCategory>> PostFinancialAidFundCategoryAsync([FromBody] Dtos.FinancialAidFundCategory financialAidFundCategory)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Financial Aid Fund Category.
        /// </summary>
        /// <param name="id">Id of the Financial Aid Fund Category to update</param>
        /// <param name="financialAidFundCategory"><see cref="Dtos.FinancialAidFundCategory">FinancialAidFundCategory</see> to create</param>
        /// <returns>Updated <see cref="Dtos.FinancialAidFundCategory">FinancialAidFundCategory</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/financial-aid-fund-categories/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFinancialAidFundCategoriesV7")]
        public async Task<ActionResult<Dtos.FinancialAidFundCategory>> PutFinancialAidFundCategoryAsync([FromRoute] string id, [FromBody] Dtos.FinancialAidFundCategory financialAidFundCategory)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Financial Aid Fund Category.
        /// </summary>
        /// <param name="id">ID of the Financial Aid Fund Category to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/financial-aid-fund-categories/{id}", Name = "DeleteFinancialAidFundCategories", Order = -10)]
        public async Task<IActionResult> DeleteFinancialAidFundCategoryAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

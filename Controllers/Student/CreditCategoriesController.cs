// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using System.Net;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to CreditTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Route("/[controller]/[action]")]
    public class CreditCategoriesController : BaseCompressedApiController
    {
        private readonly ICurriculumService _curriculumService;
        private readonly ILogger _logger;

        /// <summary>
        /// CreditTypesController constructor
        /// </summary>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CreditCategoriesController(ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _curriculumService = curriculumService;
            this._logger = logger;
        }

        #region Get Methods

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all credit categories.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All CreditCategory objects</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/credit-categories", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmCreditCategories3", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<CreditCategory3>>> GetCreditCategories3Async()
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
                var dtos = await _curriculumService.GetCreditCategories3Async(bypassCache);

                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        dtos.Select(i => i.Id).ToList()));

                return Ok(dtos);

                //return await _curriculumService.GetCreditCategories3Async(bypassCache);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves a credit category by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CreditCategory3">CreditCategory.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/credit-categories/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCreditCategoryByGuid3", IsEedmSupported = true)]
        public async Task<ActionResult<CreditCategory3>> GetCreditCategoryByGuid3Async(string id)
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
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Credit category id is required.");
                }

                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _curriculumService.GetCreditCategoryByGuid3Async(id);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        #endregion

        #region Post Methods

        /// <summary>
        /// Creates a Credit Category.
        /// </summary>
        /// <metadata>
        /// <apiType>ethos</apiType>
        /// </metadata>
        /// <param name="creditCategory"><see cref="CreditCategory2">CreditCategory</see> to create</param>
        /// <returns>Newly created <see cref="CreditCategory2">CreditCategory</see></returns>
        [HttpPost]
        public async Task<ActionResult<CreditCategory2>> PostCreditCategoryAsync([FromBody] CreditCategory2 creditCategory)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a Credit Category.
        /// </summary>
        /// <metadata>
        /// <apiType>ethos</apiType>
        /// </metadata>
        /// <param name="creditCategory"><see cref="CreditCategory3">CreditCategory</see> to create</param>
        /// <returns>Newly created <see cref="CreditCategory3">CreditCategory</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/credit-categories", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCreditCategoriesV6")]
        public async Task<ActionResult<CreditCategory3>> PostCreditCategoryV6Async([FromBody] CreditCategory3 creditCategory)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods

        /// <summary>
        /// Updates a Credit Category.
        /// </summary>
        /// <metadata>
        /// <apiType>ethos</apiType>
        /// </metadata>
        /// <param name="id">Id of the Credit Category to update</param>
        /// <param name="creditCategory"><see cref="CreditCategory2">CreditCategory</see> to create</param>
        /// <returns>Updated <see cref="CreditCategory2">CreditCategory</see></returns>
        [HttpPut]
        public async Task<ActionResult<CreditCategory2>> PutCreditCategoryAsync([FromRoute] string id, [FromBody] CreditCategory2 creditCategory)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a Credit Category.
        /// </summary>
        /// <metadata>
        /// <apiType>ethos</apiType>
        /// </metadata>
        /// <param name="id">Id of the Credit Category to update</param>
        /// <param name="creditCategory"><see cref="CreditCategory3">CreditCategory</see> to create</param>
        /// <returns>Updated <see cref="CreditCategory3">CreditCategory</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/credit-categories/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCreditCategoriesV6")]
        public async Task<ActionResult<CreditCategory3>> PutCreditCategoryV6Async([FromRoute] string id, [FromBody] CreditCategory3 creditCategory)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Delete Methods

        /// <summary>
        /// Delete (DELETE) an existing Credit Category
        /// </summary>
        /// <metadata>
        /// <apiType>ethos</apiType>
        /// </metadata>
        /// <param name="id">Id of the Credit Category to delete</param>
        [HttpDelete]
        public async Task<ActionResult<CreditCategory2>> DeleteCreditCategoryAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Credit Category
        /// </summary>
        /// <metadata>
        /// <apiType>ethos</apiType>
        /// </metadata>
        /// <param name="id">Id of the Credit Category to delete</param>
        [HttpDelete]
        [Route("/credit-categories/{id}", Name = "DeleteCreditCategoriesV6", Order = -10)]
        public async Task<ActionResult<CreditCategory3>> DeleteCreditCategoryV6Async([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion
    }
}

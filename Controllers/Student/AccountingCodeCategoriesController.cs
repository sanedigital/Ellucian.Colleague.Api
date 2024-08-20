// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
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
    /// Provides access to AccountingCodeCategories
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AccountingCodeCategoriesController : BaseCompressedApiController
    {
        private readonly IAccountingCodeCategoriesService _accountingCodeCategoriesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AccountingCodeCategoriesController class.
        /// </summary>
        /// <param name="accountingCodeCategoriesService">Service of type <see cref="IAccountingCodeCategoriesService">IAccountingCodeCategoriesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AccountingCodeCategoriesController(IAccountingCodeCategoriesService accountingCodeCategoriesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _accountingCodeCategoriesService = accountingCodeCategoriesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all accountingCodeCategories
        /// </summary>
        /// <returns>List of AccountingCodeCategories <see cref="Dtos.AccountingCodeCategory"/> objects representing matching accountingCodeCategories</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/accounting-code-categories", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAccountingCodeCategories", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AccountingCodeCategory>>> GetAccountingCodeCategoriesAsync()
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
                var items = await _accountingCodeCategoriesService.GetAccountingCodeCategoriesAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _accountingCodeCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _accountingCodeCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(a => a.Id).ToList()));
                }

                return Ok(items);
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
        /// Read (GET) a accountingCodeCategories using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingCodeCategories</param>
        /// <returns>A accountingCodeCategories object <see cref="Dtos.AccountingCodeCategory"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/accounting-code-categories/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAccountingCodeCategoriesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AccountingCodeCategory>> GetAccountingCodeCategoryByGuidAsync(string guid)
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
                var item = await _accountingCodeCategoriesService.GetAccountingCodeCategoryByGuidAsync(guid);

                if(item != null)
                {
                    AddEthosContextProperties(await _accountingCodeCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _accountingCodeCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
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
        /// Create (POST) a new accountingCodeCategories
        /// </summary>
        /// <param name="accountingCodeCategories">DTO of the new accountingCodeCategories</param>
        /// <returns>A accountingCodeCategories object <see cref="Dtos.AccountingCodeCategory"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/accounting-code-categories", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountingCodeCategoriesV11")]
        public async Task<ActionResult<Dtos.AccountingCodeCategory>> PostAccountingCodeCategoryAsync([FromBody] Dtos.AccountingCodeCategory accountingCodeCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountingCodeCategories
        /// </summary>
        /// <param name="guid">GUID of the accountingCodeCategories to update</param>
        /// <param name="accountingCodeCategories">DTO of the updated accountingCodeCategories</param>
        /// <returns>A accountingCodeCategories object <see cref="Dtos.AccountingCodeCategory"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/accounting-code-categories/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountingCodeCategoriesV11")]
        public async Task<ActionResult<Dtos.AccountingCodeCategory>> PutAccountingCodeCategoryAsync([FromRoute] string guid, [FromBody] Dtos.AccountingCodeCategory accountingCodeCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a accountingCodeCategories
        /// </summary>
        /// <param name="guid">GUID to desired accountingCodeCategories</param>
        [HttpDelete]
        [Route("/accounting-code-categories/{guid}", Name = "DefaultDeleteAccountingCodeCategories")]
        public async Task<IActionResult> DeleteAccountingCodeCategoryAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

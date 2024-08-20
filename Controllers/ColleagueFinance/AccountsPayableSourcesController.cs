// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Colleague.Dtos;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to AccountsPayableSources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class AccountsPayableSourcesController : BaseCompressedApiController
    {
        private readonly IAccountsPayableSourcesService _accountsPayableSourcesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AccountsPayableSourcesController class.
        /// </summary>
        /// <param name="accountsPayableSourcesService">Service of type <see cref="IAccountsPayableSourcesService">IAccountsPayableSourcesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AccountsPayableSourcesController(IAccountsPayableSourcesService accountsPayableSourcesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _accountsPayableSourcesService = accountsPayableSourcesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all accountsPayableSources
        /// </summary>
        /// <returns>List of AccountsPayableSources <see cref="AccountsPayableSources"/> objects representing matching accountsPayableSources</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/accounts-payable-sources", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAccountsPayableSources", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<AccountsPayableSources>>> GetAccountsPayableSourcesAsync()
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
                var items = await _accountsPayableSourcesService.GetAccountsPayableSourcesAsync(bypassCache);

                AddEthosContextProperties(await _accountsPayableSourcesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _accountsPayableSourcesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    items.Select(a => a.Id).ToList()));

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
        /// Read (GET) a accountsPayableSources using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountsPayableSources</param>
        /// <returns>A accountsPayableSources object <see cref="AccountsPayableSources"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/accounts-payable-sources/{guid}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAccountsPayableSourcesByGuid")]
        public async Task<ActionResult<AccountsPayableSources>> GetAccountsPayableSourcesByIdAsync(string guid)
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
                var item = await _accountsPayableSourcesService.GetAccountsPayableSourcesByGuidAsync(guid);

                if (item != null)
                {
                    AddEthosContextProperties(await _accountsPayableSourcesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _accountsPayableSourcesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
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
        /// Create (POST) a new accountsPayableSources
        /// </summary>
        /// <param name="accountsPayableSources">DTO of the new accountsPayableSources</param>
        /// <returns>A accountsPayableSources object <see cref="AccountsPayableSources"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/accounts-payable-sources", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountsPayableSourcesV8Async")]
        public async Task<ActionResult<AccountsPayableSources>> PostAccountsPayableSourcesAsync([FromBody] AccountsPayableSources accountsPayableSources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountsPayableSources
        /// </summary>
        /// <param name="guid">GUID of the accountsPayableSources to update</param>
        /// <param name="accountsPayableSources">DTO of the updated accountsPayableSources</param>
        /// <returns>A accountsPayableSources object <see cref="Dtos.AccountsPayableSources"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/accounts-payable-sources/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountsPayableSourcesV8Async")]
        public async Task<ActionResult<AccountsPayableSources>> PutAccountsPayableSourcesAsync([FromRoute] string guid, [FromBody] AccountsPayableSources accountsPayableSources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a accountsPayableSources
        /// </summary>
        /// <param name="guid">GUID to desired accountsPayableSources</param>
        [HttpDelete]
        [Route("/accounts-payable-sources/{guid}", Name = "DefaultDeleteAccountsPayableSourcesAsync", Order = -10)]
        public async Task<IActionResult> DeleteAccountsPayableSourcesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

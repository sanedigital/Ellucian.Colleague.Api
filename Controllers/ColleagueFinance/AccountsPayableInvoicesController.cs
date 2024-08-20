// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.ModelBinding;

using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.ColleagueFinance;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to AccountsPayableInvoices
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class AccountsPayableInvoicesController : BaseCompressedApiController
    {
        private readonly IAccountsPayableInvoicesService _accountsPayableInvoicesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AccountsPayableInvoicesController class.
        /// </summary>
        /// <param name="accountsPayableInvoicesService">Service of type <see cref="IAccountsPayableInvoicesService">IAccountsPayableInvoicesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public AccountsPayableInvoicesController(IAccountsPayableInvoicesService accountsPayableInvoicesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _accountsPayableInvoicesService = accountsPayableInvoicesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all accountsPayableInvoices version 11
        /// </summary>
        /// <returns>List of AccountsPayableInvoices <see cref="Dtos.AccountsPayableInvoices2"/> objects representing matching accountsPayableInvoices</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewApInvoices, ColleagueFinancePermissionCodes.UpdateApInvoices })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(AccountsPayableInvoices2))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/accounts-payable-invoices", "11.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountsPayableInvoices", IsEedmSupported = true)]
        public async Task<IActionResult> GetAccountsPayableInvoices2Async(Paging page, QueryStringFilter criteria)
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
            var criteriaFilter = GetFilterObject<Dtos.AccountsPayableInvoices2>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.AccountsPayableInvoices2>>(new List<Dtos.AccountsPayableInvoices2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            
            try
            {
                _accountsPayableInvoicesService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _accountsPayableInvoicesService.GetAccountsPayableInvoices2Async(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(
                await _accountsPayableInvoicesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                await _accountsPayableInvoicesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));
                                
                return new PagedActionResult<IEnumerable<Dtos.AccountsPayableInvoices2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a accountsPayableInvoices version 11 using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountsPayableInvoices</param>
        /// <returns>A accountsPayableInvoices object <see cref="Dtos.AccountsPayableInvoices2"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewApInvoices, ColleagueFinancePermissionCodes.UpdateApInvoices }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/accounts-payable-invoices/{guid}", "11.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAccountsPayableInvoicesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AccountsPayableInvoices2>> GetAccountsPayableInvoices2ByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

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
                _accountsPayableInvoicesService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _accountsPayableInvoicesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _accountsPayableInvoicesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _accountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid);
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
        /// Create (POST) a new accountsPayableInvoices
        /// </summary>
        /// <param name="accountsPayableInvoices">DTO of the new accountsPayableInvoices</param>
        /// <returns>A accountsPayableInvoices object <see cref="Dtos.AccountsPayableInvoices2"/> in EEDM format</returns>
        [HttpPost, PermissionsFilter(new string[] {  ColleagueFinancePermissionCodes.UpdateApInvoices }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/accounts-payable-invoices", "11.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountsPayableInvoicesV11.2.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AccountsPayableInvoices2>> PostAccountsPayableInvoices2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.AccountsPayableInvoices2 accountsPayableInvoices)
        {
            if (accountsPayableInvoices == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null accountsPayableInvoices argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                _accountsPayableInvoicesService.ValidatePermissions(GetPermissionsMetaData());
                if (accountsPayableInvoices.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("accountsPayableInvoicesDto", "Nil GUID must be used in POST operation.");
                }                

                //call import extend method that needs the extracted extension dataa and the config
                await _accountsPayableInvoicesService.ImportExtendedEthosData(await ExtractExtendedData(await _accountsPayableInvoicesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var accountsPayableInvoiceReturn = await _accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoices);
                
                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _accountsPayableInvoicesService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _accountsPayableInvoicesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { accountsPayableInvoiceReturn.Id }));

                return accountsPayableInvoiceReturn;
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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                if (e.Errors == null || e.Errors.Count() <= 0)
                {
                    return CreateHttpResponseException(e.Message);
                }
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
        
        /// <summary>
        /// Update (PUT) an existing accountsPayableInvoices
        /// </summary>
        /// <param name="guid">GUID of the accountsPayableInvoices to update</param>
        /// <param name="accountsPayableInvoices">DTO of the updated accountsPayableInvoices</param>
        /// <returns>A accountsPayableInvoices object <see cref="Dtos.AccountsPayableInvoices2"/> in EEDM format</returns>
        [HttpPut, PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.UpdateApInvoices }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/accounts-payable-invoices/{guid}", "11.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountsPayableInvoicesV11.2.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AccountsPayableInvoices2>> PutAccountsPayableInvoices2Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.AccountsPayableInvoices2 accountsPayableInvoices)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (accountsPayableInvoices == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null accountsPayableInvoices argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(accountsPayableInvoices.Id))
            {
                accountsPayableInvoices.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(accountsPayableInvoices.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != accountsPayableInvoices.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _accountsPayableInvoicesService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _accountsPayableInvoicesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _accountsPayableInvoicesService.ImportExtendedEthosData(await ExtractExtendedData(await _accountsPayableInvoicesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var accountsPayableInvoiceReturn = await _accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid,
                    await PerformPartialPayloadMerge(accountsPayableInvoices,
                        async () => await _accountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(dpList,
                    await _accountsPayableInvoicesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return accountsPayableInvoiceReturn;
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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }          
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                if (e.Errors == null || e.Errors.Count() <= 0)
                {
                    return CreateHttpResponseException(e.Message);
                }
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

        /// <summary>
        /// Delete (DELETE) a accountsPayableInvoices
        /// </summary>
        /// <param name="guid">GUID to desired accountsPayableInvoices</param>
        [HttpDelete]
        [Route("/accounts-payable-invoices/{guid}", Name = "DefaultDeleteAccountsPayableInvoices", Order = -10)]
        public async Task<IActionResult> DeleteAccountsPayableInvoicesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
        
    }
}

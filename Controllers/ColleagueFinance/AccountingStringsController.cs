// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Constraints;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to Accounting Strings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class AccountingStringsController : BaseCompressedApiController
    {
        private readonly IAccountingStringService _accountingStringService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AccountingStringsController class.
        /// </summary>
        /// <param name="accountingStringService">Service of type <see cref="IAccountingStringService">IAccountingStringService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AccountingStringsController(IAccountingStringService accountingStringService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _accountingStringService = accountingStringService;
            this._logger = logger;
        }

        #region Accounting String
        /// <summary>
        /// Read (GET) an AccountingString using an accounting string as a filter, return confirms it exists and is valid
        /// </summary>
        /// <param name="accountingString">Accounting String to search for, may contain project number</param>
        /// <param name="validOn">date to check for to see if accounting string is valid on the date</param>
        /// <returns>An AccountingString object <see cref="Dtos.AccountingString"/> in DataModel format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), PermissionsFilter(ColleagueFinancePermissionCodes.ViewAccountingStrings)]
        [ValidateQueryStringFilter(new string[] { "accountingString", "validOn" }, false, true)]
        [HeaderVersionRoute("/accounting-strings", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountingStringsByFilterDefault", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AccountingString>> GetAccountingStringByFilterAsync([FromQuery] string accountingString = "", [FromQuery] string validOn = "")
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(accountingString))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null accountingString argument",
                    IntegrationApiUtility.GetDefaultApiError("The accountingString must be specified in the request URL.")));
            }

            try
            {
                _accountingStringService.ValidatePermissions(GetPermissionsMetaData());

                DateTime? validOnDate = null;

                if (!string.IsNullOrEmpty(validOn))
                {
                    DateTime outDate;
                    if (DateTime.TryParse(validOn, out outDate))
                    {
                        validOnDate = outDate;
                    }
                    else
                    {
                        throw new ArgumentException(
                            "The value provided for validOn filter could not be converted into a date");
                    }
                }

                AddDataPrivacyContextProperty((await _accountingStringService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                
                return await _accountingStringService.GetAccoutingStringByFilterCriteriaAsync(accountingString, validOnDate);
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
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                var exception = new Exception(e.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
            }           
            catch (ArgumentException e)
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
        /// AccountingString Get all
        /// </summary>
        /// <returns>MethodNotAllowed error</returns>
        [HttpGet]
        // TODO: Verify route commented out by domain
        //[HeaderVersionRoute("/accounting-strings", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountingStringsDefault")]
        public async Task<ActionResult<IEnumerable<Dtos.AccountingString>>> GetAccountingStringsAsync()
        {
            //Get all is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException("Null accountingString argument",
                    IntegrationApiUtility.GetDefaultApiError("The accountingString must be specified in the request URL when a GET operation is requested.")));
            //throw new ArgumentNullException("Null accountingString argument", "The accountingString must be specified in the request URL when a GET operation is requested.");
        }

        /// <summary>
        /// AccountingString Post
        /// </summary>
        /// <returns>MethodNotAllowed error</returns>
        [HttpPost]
        [HeaderVersionRoute("/accounting-strings", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountingStringV7")]
        public async Task<ActionResult<Dtos.AccountingString>> PostAccountingStringsAsync([FromBody] Dtos.AccountingString accountingString)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// AccountingString Get by Id
        /// </summary>
        /// <returns>MethodNotAllowed error</returns>
        [HttpGet]
        [HeaderVersionRoute("/accounting-strings/{guid}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultAccountingStringByGuid")]
        public async Task<ActionResult<Dtos.AccountingString>> GetAccountingStringsByGuidAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// AccountingString Put
        /// </summary>
        /// <returns>MethodNotAllowed error</returns>
        [HttpPut]
        [HeaderVersionRoute("/accounting-strings/{guid}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountingStringV7")]
        public async Task<ActionResult<Dtos.AccountingString>> PutAccountingStringsAsync([FromRoute] string guid, [FromBody] Dtos.AccountingString accountingString)
        {
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// AccountingString Delete
        /// </summary>
        /// <returns>MethodNotAllowed error</returns>
        [HttpDelete]
        [Route("/accounting-strings/{guid}", Name = "DefaulAccountingStrings", Order = -10)]
        public async Task<IActionResult> DeleteAccountingStringsAsync(string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }



        #endregion

        #region Accounting String Components

        /// <summary>
        /// Return all accountingStringComponents
        /// </summary>
        /// <returns>List of AccountingStringComponents <see cref="Dtos.AccountingStringComponent"/> objects representing matching accountingStringComponents</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/accounting-string-components", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountingStringComponents", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponent>>> GetAccountingStringComponentsAsync()
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
                return Ok(await _accountingStringService.GetAccountingStringComponentsAsync(bypassCache));
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
        /// Read (GET) a accountingStringComponents using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponents</param>
        /// <returns>A accountingStringComponents object <see cref="Dtos.AccountingStringComponent"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/accounting-string-components/{guid}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAccountingStringComponentsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AccountingStringComponent>> GetAccountingStringComponentsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _accountingStringService.GetAccountingStringComponentsByGuidAsync(guid);
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
        /// Create (POST) a new accountingStringComponents
        /// </summary>
        /// <param name="accountingStringComponents">DTO of the new accountingStringComponents</param>
        /// <returns>A accountingStringComponents object <see cref="Dtos.AccountingStringComponent"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/accounting-string-components", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountingStringComponentsV8")]
        public async Task<ActionResult<Dtos.AccountingStringComponent>> PostAccountingStringComponentsAsync([FromBody] Dtos.AccountingStringComponent accountingStringComponents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountingStringComponents
        /// </summary>
        /// <param name="guid">GUID of the accountingStringComponents to update</param>
        /// <param name="accountingStringComponents">DTO of the updated accountingStringComponent</param>
        /// <returns>A accountingStringComponents object <see cref="Dtos.AccountingStringComponent"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/accounting-string-components/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountingStringComponentsV8")]
        public async Task<ActionResult<Dtos.AccountingStringComponent>> PutAccountingStringComponentsAsync([FromRoute] string guid, [FromBody] Dtos.AccountingStringComponent accountingStringComponents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a accountingStringComponents
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponents</param>
        [HttpDelete]
        [Route("/accounting-string-components/{guid}", Name = "DefaultDeleteAccountingStringComponents", Order = -10)]
        public async Task<IActionResult> DeleteAccountingStringComponentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
        #endregion

        #region Accounting String Formats


        /// <summary>
        /// Return all accountingStringFormats
        /// </summary>
        /// <returns>List of AccountingStringFormats <see cref="Dtos.AccountingStringFormats"/> objects representing matching accountingStringFormats</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/accounting-string-formats", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountingStringFormats", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringFormats>>> GetAccountingStringFormatsAsync()
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
                return Ok(await _accountingStringService.GetAccountingStringFormatsAsync(bypassCache));
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
        /// Read (GET) a accountingStringFormats using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringFormats</param>
        /// <returns>A accountingStringFormats object <see cref="Dtos.AccountingStringFormats"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/accounting-string-formats/{guid}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAccountingStringFormatsByGuid")]
        public async Task<ActionResult<Dtos.AccountingStringFormats>> GetAccountingStringFormatsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _accountingStringService.GetAccountingStringFormatsByGuidAsync(guid);
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
        /// Create (POST) a new accountingStringFormats
        /// </summary>
        /// <param name="accountingStringFormats">DTO of the new accountingStringFormats</param>
        /// <returns>A accountingStringFormats object <see cref="Dtos.AccountingStringFormats"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/accounting-string-formats", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountingStringFormatsV8")]
        public async Task<ActionResult<Dtos.AccountingStringFormats>> PostAccountingStringFormatsAsync([FromBody] Dtos.AccountingStringFormats accountingStringFormats)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountingStringFormats
        /// </summary>
        /// <param name="guid">GUID of the accountingStringFormats to update</param>
        /// <param name="accountingStringFormats">DTO of the updated accountingStringFormats</param>
        /// <returns>A accountingStringFormats object <see cref="Dtos.AccountingStringFormats"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/accounting-string-formats/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountingStringFormatsV8")]
        public async Task<ActionResult<Dtos.AccountingStringFormats>> PutAccountingStringFormatsAsync([FromRoute] string guid, [FromBody] Dtos.AccountingStringFormats accountingStringFormats)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a accountingStringFormats
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringFormats</param>
        [HttpDelete]
        [Route("/accounting-string-formats/{guid}", Name = "DefaultDeleteAccountingStringFormats", Order = -10)]
        public async Task<IActionResult> DeleteAccountingStringFormatsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
        #endregion

        #region Accounting String Components Values

        /// <summary>
        /// Return all accountingStringComponentValues
        /// </summary>
        /// <returns>List of AccountingStringComponentValues <see cref="Dtos.AccountingStringComponentValues"/> objects representing matching accountingStringComponentValues</returns>
        [HttpGet, ValidateQueryStringFilter(), PermissionsFilter(ColleagueFinancePermissionCodes.ViewAccountingStrings)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AccountingStringComponentValuesFilter)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/accounting-string-component-values", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountingStringComponentValuesV8", IsEedmSupported = true)]
        public async Task<IActionResult> GetAccountingStringComponentValuesAsync(Paging page, QueryStringFilter criteria)
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
                _accountingStringService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                string component = string.Empty, transactionStatus = string.Empty, typeAccount = string.Empty, typeFund = string.Empty;
                var criteriaValues = GetFilterObject<Dtos.AccountingStringComponentValuesFilter>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.AccountingStringComponentValues>>(new List<Dtos.AccountingStringComponentValues>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                if (criteriaValues != null)
                {
                    component = criteriaValues.Component != null && !string.IsNullOrEmpty(criteriaValues.Component.Id)
                        ? criteriaValues.Component.Id : string.Empty;
                    transactionStatus = criteriaValues.TransactionStatus != null ?
                        criteriaValues.TransactionStatus.ToString() : string.Empty;
                    typeAccount = criteriaValues.Type != null && criteriaValues.Type.Account != null ?
                       criteriaValues.Type.Account.ToString() : string.Empty;
                    typeFund = criteriaValues.Type != null && criteriaValues.Type.Fund != null ?
                       criteriaValues.Type.Fund.ToString() : string.Empty;

                    if (typeAccount == string.Empty && criteriaValues.TypeAccount != null)
                        typeAccount = criteriaValues.TypeAccount.ToString();
                    if (typeFund == string.Empty && !string.IsNullOrEmpty(criteriaValues.TypeFund))
                        typeFund = criteriaValues.TypeFund;
                }
                var pageOfItems = await _accountingStringService.GetAccountingStringComponentValuesAsync(page.Offset, page.Limit, component, transactionStatus, typeAccount, typeFund, bypassCache);

                AddEthosContextProperties(
                await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.AccountingStringComponentValues>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a accountingStringComponentValues using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.ViewAccountingStrings)]
        [HttpGet]
        [HeaderVersionRoute("/accounting-string-component-values/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountingStringComponentValuesByGuidV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AccountingStringComponentValues>> GetAccountingStringComponentValuesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _accountingStringService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                //AddDataPrivacyContextProperty((await _accountingStringService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                 await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { guid }));

                return await _accountingStringService.GetAccountingStringComponentValuesByGuidAsync(guid);
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
        /// Create (POST) a new accountingStringComponentValues
        /// </summary>
        /// <param name="accountingStringComponentValues">DTO of the new accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/accounting-string-component-values", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountingStringComponentValuesV8")]
        [HeaderVersionRoute("/accounting-string-component-values", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountingStringComponentValuesV11")]
        [HeaderVersionRoute("/accounting-string-component-values", "15", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAccountingStringComponentValuesV15")]
        public async Task<ActionResult<Dtos.AccountingStringComponentValues>> PostAccountingStringComponentValuesAsync([FromBody] Dtos.AccountingStringComponentValues accountingStringComponentValues)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountingStringComponentValues
        /// </summary>
        /// <param name="guid">GUID of the accountingStringComponentValues to update</param>
        /// <param name="accountingStringComponentValues">DTO of the updated accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/accounting-string-component-values/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountingStringComponentValuesV8")]
        [HeaderVersionRoute("/accounting-string-component-values/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountingStringComponentValuesV11")]
        [HeaderVersionRoute("/accounting-string-component-values/{guid}", "15", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAccountingStringComponentValuesV15")]
        public async Task<ActionResult<Dtos.AccountingStringComponentValues>> PutAccountingStringComponentValuesAsync([FromRoute] string guid, [FromBody] Dtos.AccountingStringComponentValues accountingStringComponentValues)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a accountingStringComponentValues
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponentValues</param>
        [HttpDelete]
        [Route("/accounting-string-component-values/{guid}", Name = "DefaultDeleteAccountingStringComponentValues", Order = -10)]
        public async Task<IActionResult> DeleteAccountingStringComponentValuesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Return all accountingStringComponentValues
        /// </summary>
        /// <returns>List of AccountingStringComponentValues <see cref="Dtos.AccountingStringComponentValues"/> objects representing matching accountingStringComponentValues</returns>
        [HttpGet, ValidateQueryStringFilter(), PermissionsFilter(ColleagueFinancePermissionCodes.ViewAccountingStrings)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AccountingStringComponentValues2)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/accounting-string-component-values", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountingStringComponentValuesV11", IsEedmSupported = true)]
        public async Task<IActionResult> GetAccountingStringComponentValues2Async(Paging page, QueryStringFilter criteria)
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
                _accountingStringService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                string component = string.Empty, transactionStatus = string.Empty, typeAccount = string.Empty, typeFund = string.Empty;
                var criteriaValues = GetFilterObject<Dtos.AccountingStringComponentValues2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.AccountingStringComponentValues2>>(new List<Dtos.AccountingStringComponentValues2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                if (criteriaValues != null)
                {
                    component = criteriaValues.Component != null && !string.IsNullOrEmpty(criteriaValues.Component.Id)
                        ? criteriaValues.Component.Id : string.Empty;
                    transactionStatus = criteriaValues.TransactionStatus != null ?
                        criteriaValues.TransactionStatus.ToString() : string.Empty;
                    typeAccount = criteriaValues.Type != null && criteriaValues.Type.Account != null ?
                       criteriaValues.Type.Account.ToString() : string.Empty;
                    typeFund = criteriaValues.Type != null && criteriaValues.Type.Fund != null ?
                       criteriaValues.Type.Fund.ToString() : string.Empty;

                }
                var pageOfItems = await _accountingStringService.GetAccountingStringComponentValues2Async(page.Offset, page.Limit, component, transactionStatus, typeAccount, typeFund, bypassCache);
                AddEthosContextProperties(
                 await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.AccountingStringComponentValues2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Return all accountingStringComponentValues
        /// </summary>
        /// <returns>List of AccountingStringComponentValues <see cref="Dtos.AccountingStringComponentValues"/> objects representing matching accountingStringComponentValues</returns>
        [HttpGet, ValidateQueryStringFilter(), PermissionsFilter(ColleagueFinancePermissionCodes.ViewAccountingStrings)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AccountingStringComponentValues3)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("effectiveOn", typeof(Dtos.Filters.AccountingStringsFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/accounting-string-component-values", "15", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountingStringComponentValues", IsEedmSupported = true)]
        public async Task<IActionResult> GetAccountingStringComponentValues3Async(Paging page, QueryStringFilter effectiveOn, QueryStringFilter criteria)
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
                _accountingStringService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                string transactionStatus = string.Empty, typeFund = string.Empty;                
                var criteriaValues = GetFilterObject<Dtos.AccountingStringComponentValues3>(_logger, "criteria");

                DateTime? effectiveOnValue = null;
                var effectiveOnFilterObj = GetFilterObject<Dtos.Filters.AccountingStringsFilter>(_logger, "effectiveOn");
                if(effectiveOnFilterObj != null && effectiveOnFilterObj.EffectiveOn.HasValue)
                {
                    effectiveOnValue = effectiveOnFilterObj.EffectiveOn.Value;
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.AccountingStringComponentValues3>>(new List<Dtos.AccountingStringComponentValues3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                if (criteriaValues != null)
                {
                    transactionStatus = criteriaValues.TransactionStatus != null ?
                        criteriaValues.TransactionStatus.ToString() : string.Empty;
                    if (!string.IsNullOrEmpty(transactionStatus) && !transactionStatus.ToLowerInvariant().Equals("available", StringComparison.OrdinalIgnoreCase))
                    {
                        return new PagedActionResult<IEnumerable<Dtos.AccountingStringComponentValues3>>(new List<Dtos.AccountingStringComponentValues3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }

                    typeFund = criteriaValues.Type != null && criteriaValues.Type.Fund != null ?
                       criteriaValues.Type.Fund.ToString() : string.Empty;

                    if(!string.IsNullOrEmpty(typeFund))
                    {
                        return new PagedActionResult<IEnumerable<Dtos.AccountingStringComponentValues3>>(new List<Dtos.AccountingStringComponentValues3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }

                }
                var pageOfItems = await _accountingStringService.GetAccountingStringComponentValues3Async(page.Offset, page.Limit, criteriaValues, effectiveOnValue, bypassCache);

                AddEthosContextProperties(
                 await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.AccountingStringComponentValues3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a accountingStringComponentValues using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.ViewAccountingStrings)]
        [HttpGet]
        [HeaderVersionRoute("/accounting-string-component-values/{guid}", "15", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAccountingStringComponentValuesByGuid")]
        public async Task<ActionResult<Dtos.AccountingStringComponentValues3>> GetAccountingStringComponentValues3ByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _accountingStringService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                AddEthosContextProperties(
                 await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { guid }));

                return await _accountingStringService.GetAccountingStringComponentValues3ByGuidAsync(guid, bypassCache);
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
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a accountingStringComponentValues using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.ViewAccountingStrings)]
        [HttpGet]
        [HeaderVersionRoute("/accounting-string-component-values/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAccountingStringComponentValuesByGuidV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AccountingStringComponentValues2>> GetAccountingStringComponentValues2ByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _accountingStringService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                //AddDataPrivacyContextProperty((await _accountingStringService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                 await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { guid }));

                return await _accountingStringService.GetAccountingStringComponentValues2ByGuidAsync(guid, bypassCache);
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
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }        

        /// <summary>
        /// Create (POST) a new accountingStringComponentValues
        /// </summary>
        /// <param name="accountingStringComponentValues">DTO of the new accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpPost]
        public async Task<ActionResult<Dtos.AccountingStringComponentValues>> PostAccountingStringComponentValues2Async([FromBody] Dtos.AccountingStringComponentValues2 accountingStringComponentValues)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountingStringComponentValues
        /// </summary>
        /// <param name="guid">GUID of the accountingStringComponentValues to update</param>
        /// <param name="accountingStringComponentValues">DTO of the updated accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpPut]
        public async Task<ActionResult<Dtos.AccountingStringComponentValues>> PutAccountingStringComponentValues2Async([FromQuery] string guid, [FromBody] Dtos.AccountingStringComponentValues2 accountingStringComponentValues)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        #endregion Accounting String Components Values
    }
}

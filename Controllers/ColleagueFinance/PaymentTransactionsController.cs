// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to PaymentTransactions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class PaymentTransactionsController : BaseCompressedApiController
    {
        private readonly IPaymentTransactionsService _paymentTransactionsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PaymentTransactionsController class.
        /// </summary>
        /// <param name="paymentTransactionsService">Service of type <see cref="IPaymentTransactionsService">IPaymentTransactionsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PaymentTransactionsController(IPaymentTransactionsService paymentTransactionsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _paymentTransactionsService = paymentTransactionsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all paymentTransactions
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="document">Named query</param>
        ///  <param name="criteria">criteria filter</param>
        /// <returns>List of PaymentTransactions <see cref="Dtos.PaymentTransactions"/> objects representing matching paymentTransactions</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.ViewPaymentTransactionsIntg)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("document", typeof(DocumentFilter)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(PaymentTransactions))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HeaderVersionRoute("/payment-transactions", "12.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPaymentTransactions", IsEedmSupported = true)]
        public async Task<IActionResult> GetPaymentTransactionsAsync(Paging page, QueryStringFilter document, QueryStringFilter criteria)
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
                _paymentTransactionsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var criteriaFilter = GetFilterObject<Dtos.PaymentTransactions>(_logger, "criteria");
                string documentGuid = string.Empty;
                var documentTypeValue = InvoiceTypes.NotSet;

                var documentFilter = GetFilterObject<DocumentFilter>(_logger, "document");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.PaymentTransactions>>(new List<Dtos.PaymentTransactions>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                if (documentFilter.Document != null)
                {
                    documentGuid = documentFilter.Document.Id;
                    documentTypeValue = documentFilter.Document.Type;
                    if (documentTypeValue != InvoiceTypes.NotSet && string.IsNullOrEmpty(documentGuid))
                    {
                        throw new ArgumentException("documentGuid", "Id is required when requesting a document");
                    }
                    if (documentTypeValue == InvoiceTypes.NotSet && !string.IsNullOrEmpty(documentGuid))
                    {
                        throw new ArgumentException("documentType", "Type is required when requesting a document");
                    }
                }

                var pageOfItems = await _paymentTransactionsService.GetPaymentTransactionsAsync(page.Offset, page.Limit, documentGuid, documentTypeValue, criteriaFilter, bypassCache);

                AddEthosContextProperties(
                    await _paymentTransactionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _paymentTransactionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PaymentTransactions>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a paymentTransactions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired paymentTransactions</param>
        /// <returns>A paymentTransactions object <see cref="Dtos.PaymentTransactions"/> in EEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.ViewPaymentTransactionsIntg)]
        [HeaderVersionRoute("/payment-transactions/{guid}", "12.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPaymentTransactionsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PaymentTransactions>> GetPaymentTransactionsByGuidAsync(string guid)
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
                _paymentTransactionsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _paymentTransactionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _paymentTransactionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _paymentTransactionsService.GetPaymentTransactionsByGuidAsync(guid);
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
        /// Create (POST) a new paymentTransactions
        /// </summary>
        /// <param name="paymentTransactions">DTO of the new paymentTransactions</param>
        /// <returns>A paymentTransactions object <see cref="Dtos.PaymentTransactions"/> in EEDM format</returns>
        [HttpPost]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/payment-transactions", "12.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPaymentTransactionsV12_1_0")]
        public async Task<ActionResult<Dtos.PaymentTransactions>> PostPaymentTransactionsAsync([FromBody] Dtos.PaymentTransactions paymentTransactions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing paymentTransactions
        /// </summary>
        /// <param name="guid">GUID of the paymentTransactions to update</param>
        /// <param name="paymentTransactions">DTO of the updated paymentTransactions</param>
        /// <returns>A paymentTransactions object <see cref="Dtos.PaymentTransactions"/> in EEDM format</returns>
        [HttpPut]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/payment-transactions/{guid}", "12.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPaymentTransactionsV12_1_0")]
        public async Task<ActionResult<Dtos.PaymentTransactions>> PutPaymentTransactionsAsync([FromRoute] string guid, [FromBody] Dtos.PaymentTransactions paymentTransactions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a paymentTransactions
        /// </summary>
        /// <param name="guid">GUID to desired paymentTransactions</param>
        [HttpDelete]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [Route("/payment-transactions/{guid}", Name = "DefaultDeletePaymentTransactions", Order = -10)]
        public async Task<IActionResult> DeletePaymentTransactionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

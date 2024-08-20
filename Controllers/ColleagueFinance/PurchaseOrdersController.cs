// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Http.ModelBinding;

using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for purchase orders
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class PurchaseOrdersController : BaseCompressedApiController
    {
        private readonly IPurchaseOrderService purchaseOrderService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the PurchaseOrdersController object
        /// </summary>
        /// <param name="purchaseOrderService">Purchase Order service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.purchaseOrderService = purchaseOrderService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a specified purchase order
        /// </summary>
        /// <param name="purchaseOrderId">The requested purchase order ID</param>
        /// <returns>Purchase Order DTO</returns>
        /// <accessComments>
        /// Requires permission VIEW.PURCHASE.ORDER, and requires access to at least one of the
        /// general ledger numbers on the purchase order line items.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/purchase-orders/{purchaseOrderId}", 1, false, Name = "GetPurchaseOrder")]
        public async Task<ActionResult<PurchaseOrder>> GetPurchaseOrderAsync(string purchaseOrderId)
        {
            if (string.IsNullOrEmpty(purchaseOrderId))
            {
                string message = "A Purchase Order ID must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var purchaseOrder = await purchaseOrderService.GetPurchaseOrderAsync(purchaseOrderId);
                return purchaseOrder;
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the purchase order.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, knfex.Message);
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to get the purchase order.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the purchase order.", HttpStatusCode.BadRequest);
            }
        }

        #region EEDM V11
        /// <summary>
        /// Return all purchaseOrders
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">Criteria Filter (orderNumber)</param>
        /// <returns>List of PurchaseOrders <see cref="Dtos.PurchaseOrders2"/> objects representing matching purchaseOrders</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewPurchaseOrders, ColleagueFinancePermissionCodes.UpdatePurchaseOrders })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PurchaseOrders2))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/purchase-orders", "11.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPurchaseOrders", IsEedmSupported = true)]
        public async Task<IActionResult> GetPurchaseOrdersAsync(Paging page, QueryStringFilter criteria)
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
                purchaseOrderService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var criteriaObject = GetFilterObject<Dtos.PurchaseOrders2>(logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.PurchaseOrders2>>(new List<Dtos.PurchaseOrders2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await purchaseOrderService.GetPurchaseOrdersAsync(page.Offset, page.Limit, criteriaObject, bypassCache);
                AddEthosContextProperties(
                    await purchaseOrderService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await purchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PurchaseOrders2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a purchaseOrders using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired purchaseOrders</param>
        /// <returns>A purchaseOrders object <see cref="Dtos.PurchaseOrders"/> in EEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewPurchaseOrders, ColleagueFinancePermissionCodes.UpdatePurchaseOrders })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/purchase-orders/{guid}", "11.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPurchaseOrdersByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PurchaseOrders2>> GetPurchaseOrdersByGuidAsync(string guid)
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
                purchaseOrderService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await purchaseOrderService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await purchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await purchaseOrderService.GetPurchaseOrdersByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new purchaseOrders
        /// </summary>
        /// <param name="purchaseOrders">DTO of the new purchaseOrders</param>
        /// <returns>A purchaseOrders object <see cref="Dtos.PurchaseOrders"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2),  PermissionsFilter(ColleagueFinancePermissionCodes.UpdatePurchaseOrders)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/purchase-orders", "11.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPurchaseOrdersV11_2_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PurchaseOrders2>> PostPurchaseOrdersAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.PurchaseOrders2 purchaseOrders)
        {
            if (purchaseOrders == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null purchaseOrders argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            try
            {
                purchaseOrderService.ValidatePermissions(GetPermissionsMetaData());
                if (purchaseOrders.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("purchaseOrdersDto", "Nil GUID must be used in POST operation.");
                }

                //call import extend method that needs the extracted extension data and the config
                await purchaseOrderService.ImportExtendedEthosData(await ExtractExtendedData(await purchaseOrderService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                // Create Purchase Order
                var purchaseOrderReturn = await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrders);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await purchaseOrderService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await purchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { purchaseOrderReturn.Id }));

                return purchaseOrderReturn;
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing purchaseOrders
        /// </summary>
        /// <param name="guid">GUID of the purchaseOrders to update</param>
        /// <param name="purchaseOrders">DTO of the updated purchaseOrders</param>
        /// <returns>A purchaseOrders object <see cref="Dtos.PurchaseOrders"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), PermissionsFilter(ColleagueFinancePermissionCodes.UpdatePurchaseOrders)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/purchase-orders/{guid}", "11.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPurchaseOrdersV11_2_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PurchaseOrders2>> PutPurchaseOrdersAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.PurchaseOrders2 purchaseOrders)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (purchaseOrders == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null purchaseOrders argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(purchaseOrders.Id))
            {
                purchaseOrders.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(purchaseOrders.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != purchaseOrders.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                purchaseOrderService.ValidatePermissions(GetPermissionsMetaData());
                // get Data Privacy List
                var dpList = await purchaseOrderService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await purchaseOrderService.ImportExtendedEthosData(await ExtractExtendedData(await purchaseOrderService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var mergedPurchaseOrder = await PerformPartialPayloadMerge(purchaseOrders,
                            async () => await purchaseOrderService.GetPurchaseOrdersByGuidAsync(guid),
                            dpList, logger);

                var purchaseOrderReturn = await purchaseOrderService.PutPurchaseOrdersAsync(guid, mergedPurchaseOrder);

                //store dataprivacy list and get the extended data to store

                AddEthosContextProperties(dpList,
                    await purchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return purchaseOrderReturn;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                if (e.Errors == null || e.Errors.Count <= 0)
                {
                    return CreateHttpResponseException(e.Message);
                }
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }



        #endregion

        /// <summary>
        /// Create / Update a purchase order.
        /// </summary>
        /// <param name="purchaseOrderCreateUpdateRequest">The purchase order create update request DTO.</param>        
        /// <returns>The purchase order create response DTO.</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission CREATE.UPDATE.PURCHASE.ORDER.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/purchaseOrders", 1, false, Name = "PostPurchaseOrder", Order = -10)]
        public async Task<ActionResult<Dtos.ColleagueFinance.PurchaseOrderCreateUpdateResponse>> PostPurchaseOrderAsync([FromBody] Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest purchaseOrderCreateUpdateRequest)
        {
            try
            {
                return await purchaseOrderService.CreateUpdatePurchaseOrderAsync(purchaseOrderCreateUpdateRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to create/update the purchase order.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to create/update the purchase order.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to create/update the purchase order.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Delete (DELETE) a purchaseOrders
        /// </summary>
        /// <param name="guid">GUID to desired purchaseOrders</param>
        [HttpDelete]
        [Route("/purchase-orders/{guid}", Name = "DefaultDeletePurchaseOrders", Order = -10)]
        public async Task<IActionResult> DeletePurchaseOrdersAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


        /// <summary>
        /// Retrieves list of Purchase Order summary
        /// </summary>
        /// <param name="personId">ID logged in user</param>
        /// <returns>list of Purchase Order Summary DTO</returns>
        /// <accessComments>
        /// Requires permission VIEW.PURCHASE.ORDER, and requires access to at least one of the
        /// general ledger numbers on the purchase order line items.
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague Web API 1.30. Use QueryPurchaseOrderSummariesAsync.")]
        [HttpGet]
        [HeaderVersionRoute("/purchase-orders-summary/{personId}", 1, false, Name = "GetPurchaseOrderSummaryByPersonIdAsync")]
        public async Task<ActionResult<IEnumerable<PurchaseOrderSummary>>> GetPurchaseOrderSummaryByPersonIdAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                string message = "person Id must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var purchaseOrder = await purchaseOrderService.GetPurchaseOrderSummaryByPersonIdAsync(personId);
                return Ok(purchaseOrder);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the purchase order.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, knfex.Message);
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the purchase order.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Void a purchase order.
        /// </summary>
        /// <param name="purchaseOrderVoidRequest">The purchase order void request DTO.</param>        
        /// <returns>The purchase order void response DTO.</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission CREATE.UPDATE.PURCHASE.ORDER.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/purchase-orders-void", 1, false, Name = "VoidPurchaseOrder")]
        public async Task<ActionResult<Dtos.ColleagueFinance.PurchaseOrderVoidResponse>> VoidPurchaseOrderAsync([FromBody] Dtos.ColleagueFinance.PurchaseOrderVoidRequest purchaseOrderVoidRequest)
        {
            try
            {
                return await purchaseOrderService.VoidPurchaseOrderAsync(purchaseOrderVoidRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to void the purchase order.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to void the purchase order.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to void the purchase order.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves list of purchase order summary
        /// </summary>
        /// <param name="filterCriteria">procurement filter criteria</param>
        /// <returns>list of purchase order Summary DTO</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission VIEW.PURCHASE.ORDER or CREATE.UPDATE.PURCHASE.ORDER.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/purchase-order-summaries", 1, true, Name = "QueryPurchaseOrderSummariesAsync")]
        public async Task<ActionResult<IEnumerable<PurchaseOrderSummary>>> QueryPurchaseOrderSummariesAsync([FromBody] Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria filterCriteria)
        {
            if (filterCriteria == null)
            {
                return CreateHttpResponseException("Request body must contain a valid search criteria.", HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await purchaseOrderService.QueryPurchaseOrderSummariesAsync(filterCriteria));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to search purchase order.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument to search purchase order.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, knfex.Message);
                return CreateHttpResponseException("Record not found to search purchase order.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to search purchase order.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to search purchase order.", HttpStatusCode.BadRequest);
            }
        }



    }
}

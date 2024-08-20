// Copyright 2015-2024 Ellucian Company L.P. and its affiliates.

using System;
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
using Ellucian.Web.Security;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for blanket purchase orders.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class BlanketPurchaseOrdersController : BaseCompressedApiController
    {
        private readonly IBlanketPurchaseOrderService blanketPurchaseOrderService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the BlanketPurchaseOrdersController object.
        /// </summary>
        /// <param name="blanketPurchaseOrderService">Blanket purchase order service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BlanketPurchaseOrdersController(IBlanketPurchaseOrderService blanketPurchaseOrderService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.blanketPurchaseOrderService = blanketPurchaseOrderService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a specified blanket purchase order.
        /// </summary>
        /// <param name="blanketPurchaseOrderId">ID of the requested blanket purchase order.</param>
        /// <returns>Blanket purchase order DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.BLANKET.PURCHASE.ORDER, and requires access to at least one of the
        /// general ledger numbers on the blanket purchase order.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/blanket-purchase-orders/{blanketPurchaseOrderId}", 1, true, Name = "GetBlanketPurchaseOrder", Order = -5)]
        public async Task<ActionResult<BlanketPurchaseOrder>> GetBlanketPurchaseOrderAsync(string blanketPurchaseOrderId)
        {
            if (string.IsNullOrEmpty(blanketPurchaseOrderId))
            {
                string message = "A Blanket Purchase Order ID must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var bpo = await blanketPurchaseOrderService.GetBlanketPurchaseOrderAsync(blanketPurchaseOrderId);
                return bpo;
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, "Insufficient permissions to get the blanket purchase order.");
                return CreateHttpResponseException("Insufficient permissions to get the blanket purchase order.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, "Invalid argument.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, "Record not found.");
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ApplicationException aex)
            {
                logger.LogError(aex, "Invalid data in record.");
                return CreateHttpResponseException("Invalid data in record.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, "Session expired: unable to get the blanket purchase order.");
                return CreateHttpResponseException("Session expired: unable to get the blanket purchase order.", HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to get the blanket purchase order.");
                return CreateHttpResponseException("Unable to get the blanket purchase order.", HttpStatusCode.BadRequest);
            }
        }

        #region EEDM V15.1.0
        /// <summary>
        /// Return all blanket purchase orders
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">Filtering Criteria object</param>
        /// <returns>List of BlanketPurchaseOrders <see cref="Dtos.BlanketPurchaseOrders"/> objects representing matching BlanketPurchaseOrders</returns>
        [HttpGet, PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrders, ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.BlanketPurchaseOrders))]
        [HeaderVersionRoute("/blanket-purchase-orders", "16.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetBlanketPurchaseOrders", IsEedmSupported = true)]
        public async Task<IActionResult> GetBlanketPurchaseOrdersAsync(Paging page, QueryStringFilter criteria)
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
                blanketPurchaseOrderService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var criteriaObj = GetFilterObject<Dtos.BlanketPurchaseOrders>(logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.BlanketPurchaseOrders>>(new List<Dtos.BlanketPurchaseOrders>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await blanketPurchaseOrderService.GetBlanketPurchaseOrdersAsync(page.Offset, page.Limit, criteriaObj, bypassCache);
                AddEthosContextProperties(
                    await blanketPurchaseOrderService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await blanketPurchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.BlanketPurchaseOrders>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a blanket purchase order using a GUID
        /// </summary>
        /// <param name="id">GUID to desired purchaseOrders</param>
        /// <returns>A BlanketPurchaseOrders object <see cref="Dtos.BlanketPurchaseOrders"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrders, ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders })]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/blanket-purchase-orders/{id}", "16.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetBlanketPurchaseOrdersByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.BlanketPurchaseOrders>> GetBlanketPurchaseOrdersByGuidAsync(string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                blanketPurchaseOrderService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await blanketPurchaseOrderService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await blanketPurchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return await blanketPurchaseOrderService.GetBlanketPurchaseOrdersByGuidAsync(id);
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
        /// Create (POST) a new blanket purchase order
        /// </summary>
        /// <param name="blanketPurchaseOrders">DTO of the new BlanketPurchaseOrders</param>
        /// <returns>A BlanketPurchaseOrders object <see cref="Dtos.BlanketPurchaseOrders"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders)]
        [HeaderVersionRoute("/blanket-purchase-orders", "16.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostBlanketPurchaseOrdersV16_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.BlanketPurchaseOrders>> PostBlanketPurchaseOrdersAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.BlanketPurchaseOrders blanketPurchaseOrders)
        {
            if (blanketPurchaseOrders == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null blanketPurchaseOrders argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            try
            {
                blanketPurchaseOrderService.ValidatePermissions(GetPermissionsMetaData());
                if (blanketPurchaseOrders.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("blanketPurchaseOrdersDto", "Nil GUID must be used in POST operation.");
                }
                ValidateBPO(blanketPurchaseOrders);

                //call import extend method that needs the extracted extension data and the config
                await blanketPurchaseOrderService.ImportExtendedEthosData(await ExtractExtendedData(await blanketPurchaseOrderService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                // Create Purchase Order
                var purchaseOrderReturn = await blanketPurchaseOrderService.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await blanketPurchaseOrderService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await blanketPurchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { purchaseOrderReturn.Id }));

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
        /// Update (PUT) an existing blanketPurchaseOrders
        /// </summary>
        /// <param name="id">GUID of the blanketPurchaseOrders to update</param>
        /// <param name="blanketPurchaseOrders">DTO of the updated blanketPurchaseOrders</param>
        /// <returns>A blanketPurchaseOrders object <see cref="Dtos.BlanketPurchaseOrders"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders)]
        [HeaderVersionRoute("/blanket-purchase-orders/{id}", "16.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutBlanketPurchaseOrdersV16_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.BlanketPurchaseOrders>> PutBlanketPurchaseOrdersAsync([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.BlanketPurchaseOrders blanketPurchaseOrders)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (blanketPurchaseOrders == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null blanketPurchaseOrders argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(blanketPurchaseOrders.Id))
            {
                blanketPurchaseOrders.Id = id.ToLowerInvariant();
            }
            else if ((string.Equals(id, Guid.Empty.ToString())) || (string.Equals(blanketPurchaseOrders.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (id.ToLowerInvariant() != blanketPurchaseOrders.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                blanketPurchaseOrderService.ValidatePermissions(GetPermissionsMetaData());
                // get Data Privacy List
                var dpList = await blanketPurchaseOrderService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await blanketPurchaseOrderService.ImportExtendedEthosData(await ExtractExtendedData(await blanketPurchaseOrderService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var mergedPurchaseOrder = await PerformPartialPayloadMerge(blanketPurchaseOrders,
                            async () => await blanketPurchaseOrderService.GetBlanketPurchaseOrdersByGuidAsync(id),
                            dpList, logger);

                ValidateBPO(mergedPurchaseOrder);

                var purchaseOrderReturn = await blanketPurchaseOrderService.PutBlanketPurchaseOrdersAsync(id, mergedPurchaseOrder);

                //store dataprivacy list and get the extended data to store

                AddEthosContextProperties(dpList,
                    await blanketPurchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

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

        /// <summary>
        /// Helper method to validate Blanket Purchase Order PUT/POST.
        /// </summary>
        /// <param name="bpo">Blanket Purchase Order DTO object of type <see cref="Dtos.BlanketPurchaseOrders"/></param>
        private void ValidateBPO(Dtos.BlanketPurchaseOrders bpo)
        {
            var integrationApiException = new IntegrationApiException();
            var sourceGuid = !string.Equals(bpo.Id, Guid.Empty.ToString()) ? bpo.Id : string.Empty;
            var sourceId = (bpo.OrderDetails != null && bpo.OrderDetails.Any() && bpo.OrderDetails[0] != null && !string.IsNullOrEmpty(bpo.OrderDetails[0].OrderDetailNumber)) ? bpo.OrderDetails[0].OrderDetailNumber : string.Empty;
            var defaultCurrency = new CurrencyIsoCode?();

            if (bpo.Vendor == null || bpo.Vendor.ExistingVendor == null
                || bpo.Vendor.ExistingVendor.Vendor == null || string.IsNullOrEmpty(bpo.Vendor.ExistingVendor.Vendor.Id))
            {
                // throw new ArgumentNullException("blanketPurchaseOrders.Vendor", "The vendor id is required when submitting a blanket purchase order. ");
                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.vendor.existingVendor",
                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                    message: "The vendor id is a required property when submitting a blanket purchase order."));
            }
            if (bpo.Vendor != null && bpo.Vendor.ExistingVendor != null && bpo.Vendor.ExistingVendor.AlternativeVendorAddress != null && !string.IsNullOrEmpty(bpo.Vendor.ExistingVendor.AlternativeVendorAddress.Id))
            {
                if (bpo.Vendor.ManualVendorDetails != null && bpo.Vendor.ManualVendorDetails.AddressLines != null && bpo.Vendor.ManualVendorDetails.AddressLines.Any())
                {
                    integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.vendor.existingVendor",
                        "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                        message: "The manualVendorDetails address and existing vendor alternativeVendorAddress cannot both be defined when submitting a blanket purchase order."));
                }
            }
            if (bpo.OrderedOn == default(DateTime))
            {
                // throw new ArgumentNullException("blanketPurchaseOrders.OrderedOn.", "OrderedOn is a required field");
                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderedOn",
                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                    message: "The orderedOn date is a required property when submitting a blanket purchase order."));
            }

            if (bpo.TransactionDate == default(DateTime))
            {
                // throw new ArgumentNullException("blanketPurchaseOrders.TransactionDate.", "TransactionDate is a required field");
                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.transactionDate",
                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                    message: "The transactionDate is a required property when submitting a blanket purchase order."));
            }

            if (bpo.OrderedOn > bpo.TransactionDate)
            {
                // throw new ArgumentNullException("blanketPurchaseOrders.TransactionDate.", "TransactionDate cannot before OrderedOn date.");
                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.transactionDate",
                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                    message: "The transactionDate cannot be before orderedOn date when submitting a blanket purchase order."));
            }

            if (bpo.OverrideShippingDestination != null && bpo.OverrideShippingDestination.Place != null)
            {
                if (bpo.OverrideShippingDestination.Place.Country != null && bpo.OverrideShippingDestination.Place.Country.Code.Value != IsoCode.CAN && bpo.OverrideShippingDestination.Place.Country.Code.Value != IsoCode.USA)
                {
                    // throw new ArgumentNullException("blanketPurchaseOrders.OverrideShippingDestination.Country.", "Country code can only be CAN or USA");
                    integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.overrideShippingDestination.place.country",
                        "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                        message: "The override shipping destination country can only be 'CAN' or 'USA' when submitting a blanket purchase order."));
                }
                if (bpo.OverrideShippingDestination.Contact != null && !string.IsNullOrEmpty(bpo.OverrideShippingDestination.Contact.Extension) && bpo.OverrideShippingDestination.Contact.Extension.Length > 4)
                {
                    // throw new ArgumentNullException("blanketPurchaseOrders.OverrideShippingDestination.Contact.Extension", "The Extension cannot be greater then 4 in length.");
                    integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.overrideShippingDestination.contact.extension",
                        "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                        message: "The Extension cannot be greater then 4 in length when submitting a blanket purchase order."));
                }
            }
            if (bpo.Vendor != null && bpo.Vendor.ManualVendorDetails != null && bpo.Vendor.ManualVendorDetails.Place != null)
            {
                if (bpo.Vendor.ManualVendorDetails.Place.Country != null &&
                    bpo.Vendor.ManualVendorDetails.Place.Country.Code.Value != IsoCode.CAN && bpo.Vendor.ManualVendorDetails.Place.Country.Code.Value != IsoCode.USA)
                {
                    // throw new ArgumentNullException("blanketPurchaseOrders.Vendor.Country.", "Country code can only be CAN or USA");
                    integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.vendor.manualVendorDetails.place.country",
                        "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                        message: "The vendor country can only be 'CAN' or 'USA' when submitting a blanket purchase order."));
                }
            }

            if (bpo.Comments != null)
            {
                foreach (var comments in bpo.Comments)
                {
                    if (comments.Type == CommentTypes.NotSet)
                    {
                        // throw new ArgumentNullException("blanketPurchaseOrders.Comments", "Type is require for a comment");
                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.comment.type",
                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                            message: "The commentType is a required property when submitting comments on a blanket purchase order."));
                    }
                    if (comments.Type == null)
                    {
                        // throw new ArgumentNullException("blanketPurchaseOrders.Comments", "Type is require for a comment");
                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.comment.type",
                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                            message: "The commentType is a required property when submitting comments on a blanket purchase order."));
                    }
                }
            }
            if (bpo.Buyer != null && bpo.Buyer.Id == null)
            {
                // throw new ArgumentNullException("blanketPurchaseOrders.Buyer", "Buyer Id is require for a Buyer object.");
                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.buyer.id",
                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                    message: "The buyer id is a required property when submitting buyer information on a blanket purchase order."));
            }
            if (bpo.Initiator != null && bpo.Initiator.Detail != null && string.IsNullOrWhiteSpace(bpo.Initiator.Detail.Id))
            {
                // throw new ArgumentNullException("blanketPurchaseOrders.Initiator.Detail", "The Initiator detail Id is require for an Initiator detail object.");
                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.initiator.id",
                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                    message: "The initiator id is a required property when submitting initiator information on a blanket purchase order."));
            }
            if (bpo.PaymentTerms != null && string.IsNullOrWhiteSpace(bpo.PaymentTerms.Id))
            {
                // throw new ArgumentNullException("blanketPurchaseOrders.PaymentTerms", "The PaymentTerms Id is require for a PaymentTerms object.");
                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.paymentTerms.id",
                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                    message: "The paymentTerms id is a required property when submitting payment terms information on a blanket purchase order."));
            }
            if (bpo.Classification != null && string.IsNullOrWhiteSpace(bpo.Classification.Id))
            {
                // throw new ArgumentNullException("blanketPurchaseOrders.Classification", "The Classification Id is require for a Classification object.");
                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.classification.id",
                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                    message: "The classification id is a required property when submitting classification information on a blanket purchase order."));
            }
            if (bpo.SubmittedBy != null && string.IsNullOrWhiteSpace(bpo.SubmittedBy.Id))
            {
                // throw new ArgumentNullException("blanketPurchaseOrders.SubmittedBy", "The SubmittedBy Id is require for a SubmittedBy object.");
                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.submittedBy.id",
                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                    message: "The submittedBy id is a required property when submitting submitted by information on a blanket purchase order."));
            }


            if (bpo.OrderDetails == null || !bpo.OrderDetails.Any())
            {
                // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.", "At least one line item must be provided when submitting an accounts-payable-invoice. ");
                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails",
                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                    message: "The orderDetails is a required property when submitting a blanket purchase order."));
            }
            if (bpo.OrderDetails != null && bpo.OrderDetails.Any() && bpo.OrderDetails.Count > 1)
            {
                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails",
                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                    message: "No more than one orderDetails property is supported."));
            }
            if (bpo.OrderDetails != null && bpo.OrderDetails.Any())
            {
                foreach (var detail in bpo.OrderDetails)
                {
                    if (detail.CommodityCode != null && string.IsNullOrWhiteSpace(detail.CommodityCode.Id))
                    {
                        // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.CommodityCode", "The commodity code id is required when submitting a commodity code. ");
                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.commodityCode.id",
                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                            message: "The commodity code id is a required property when submitting commodity information on a blanket purchase order."));
                    }
                    if (detail.AdditionalAmount != null && (!detail.AdditionalAmount.Value.HasValue || detail.AdditionalAmount.Currency == null))
                    {
                        // throw new ArgumentNullException("blanketPurchaseOrders.AdditionalAmount.UnitPrice", "The additional amount value and currency are required when submitting a line item additional price. ");
                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.additionalAmount",
                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                            message: "The additional amount value and currency are required properties when submitting additional amount information on a blanket purchase order."));
                    }
                    if (detail.DiscountAmount != null && (!detail.DiscountAmount.Value.HasValue || detail.DiscountAmount.Currency == null))
                    {                        
                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.DiscountAmount",
                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                            message: "The discount amount value and currency are required properties when submitting discount amount information on a blanket purchase order."));
                    }
                    if (detail.AdditionalAmount != null)
                    {
                        try
                        {
                            defaultCurrency = checkCurrency(defaultCurrency, detail.AdditionalAmount.Currency);
                        }
                        catch (ArgumentException ex)
                        {
                            integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.invalid.currency",
                                "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId, message: ex.Message));
                        }
                    }
                    if (detail.DiscountAmount != null && detail.DiscountAmount.Currency != null)
                    {
                        try
                        {
                            defaultCurrency = checkCurrency(defaultCurrency, detail.DiscountAmount.Currency);
                        }
                        catch (ArgumentException ex)
                        {
                            integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.invalid.currency",
                                "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId, message: ex.Message));
                        }
                    }
                    if (detail.TaxCodes != null)
                    {
                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.taxCodes",
                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                            message: "The taxCodes property is not supported on a blanket purchase order in Colleague."));
                    }
                    if (detail.ReferenceRequisitions != null)
                    {
                        var referenceDoc = detail.ReferenceRequisitions;
                        if (referenceDoc == null)
                        {
                            // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.Reference.Document", "The requisition ID is a required field");
                            integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.referenceRequisitions",
                                "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                                message: "The reference requisitions is a required property when submitting requisition information on a blanket purchase order."));
                        }
                        else
                        {
                            foreach (var requsition in referenceDoc)
                            {
                                if (requsition.Requisition == null || string.IsNullOrEmpty(requsition.Requisition.Id))
                                {
                                    integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.referenceRequisitions.requisition.id",
                                        "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                                        message: "The reference requisitions id is a required property when submitting requisition information on a blanket purchase order."));
                                }
                            }
                        }
                    }

                    if (detail.AccountDetails != null && detail.AccountDetails.Any())
                    {
                        foreach (var accountDetail in detail.AccountDetails)
                        {
                            if (accountDetail.AccountingString == null || string.IsNullOrEmpty(accountDetail.AccountingString))
                            {
                                // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.AccountDetails.AccountingString", "The AccountingString is required when submitting a line item account detail.");
                                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.accountDetails.accountingString",
                                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                                    message: "The accounting string is a required property when submitting account details on a blanket purchase order."));

                            }
                            if (accountDetail.Allocation != null)
                            {
                                var allocation = accountDetail.Allocation;
                                if (allocation.Allocated != null)
                                {
                                    var allocated = allocation.Allocated;
                                    if (allocated.Amount != null && (!allocated.Amount.Value.HasValue || allocated.Amount.Currency == null))
                                    {
                                        // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.AccountDetail.Allocation.Allocated",
                                        //     "The Allocation.Allocated value and currency are required when submitting a line item AccountDetail.Allocation.Allocated. ");
                                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.accountDetails.allocation.allocated",
                                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                                            message: "The allocated value and currency are required properties when submitting account details on a blanket purchase order."));
                                    }
                                    if (allocated.Amount != null)
                                    {
                                        try
                                        {
                                            defaultCurrency = checkCurrency(defaultCurrency, allocated.Amount.Currency);
                                        }
                                        catch (ArgumentException ex)
                                        {
                                            integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.invalid.currency",
                                                "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId, message: ex.Message));
                                        }
                                    }
                                }
                                if (allocation.TaxAmount != null && (allocation.TaxAmount.Value.HasValue || allocation.TaxAmount.Currency != null))
                                {
                                    integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.accountDetails.allocation.taxAmount",
                                        "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                                        message: "The taxAmount property is not supported on a blanket purchase order in Colleague."));
                                }
                                if (allocation.TaxAmount != null)
                                {
                                    try
                                    {
                                        defaultCurrency = checkCurrency(defaultCurrency, allocation.TaxAmount.Currency);
                                    }
                                    catch (ArgumentException ex)
                                    {
                                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.invalid.currency",
                                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId, message: ex.Message));
                                    }
                                }
                                if (allocation.AdditionalAmount != null && (!allocation.AdditionalAmount.Value.HasValue || allocation.AdditionalAmount.Currency == null))
                                {
                                    // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.AccountDetail.Allocation.AdditionalAmount",
                                    //     "The additional amount value and currency are required when submitting a line item account detail allocation additional amount. ");
                                    integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.accountDetails.allocation.additionalAmount",
                                        "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                                        message: "The additional amount value and currency are required properties when submitting account details additional amounts on a blanket purchase order."));
                                }
                                if (allocation.AdditionalAmount != null)
                                {
                                    try
                                    {
                                        defaultCurrency = checkCurrency(defaultCurrency, allocation.AdditionalAmount.Currency);
                                    }
                                    catch (ArgumentException ex)
                                    {
                                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.invalid.currency",
                                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId, message: ex.Message));
                                    }
                                }
                                if (allocation.DiscountAmount != null && (!allocation.DiscountAmount.Value.HasValue || allocation.AdditionalAmount.Currency == null))
                                {
                                    // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.AccountDetails.DiscountAmount.Allocation.AdditionalAmount",
                                    //     "The discount amount value and currency are required when submitting a line item account detail allocation discount amount. ");
                                    integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.accountDetails.allocation.discountAmount",
                                        "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                                        message: "The discount amount value and currency are required properties when submitting account details discount amounts on a blanket purchase order."));
                                }
                                if (allocation.DiscountAmount != null)
                                {
                                    try
                                    {
                                        defaultCurrency = checkCurrency(defaultCurrency, allocation.DiscountAmount.Currency);
                                    }
                                    catch (ArgumentException ex)
                                    {
                                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.invalid.currency",
                                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId, message: ex.Message));
                                    }
                                }

                            }
                            if (accountDetail.Allocation == null)
                            {
                                // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.AccountDetails.Allocation", "The Allocation is required when submitting a line item account detail. ");
                                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.allocation",
                                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                                    message: "The allocation is a required property when submitting account details on a blanket purchase order."));
                            }
                        }

                    }
                    if (string.IsNullOrEmpty(detail.Description))
                    {
                        // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.Description", "The Description is required when submitting a line item. ");
                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.description",
                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                            message: "The description is a required property when submitting account details on a blanket purchase order."));
                    }
                    if (detail.Amount == null)
                    {
                        // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.UnitPrice", "The UnitPrice is required when submitting a line item. ");
                        integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.amount",
                            "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                            message: "The amount is a required property when submitting account details on a blanket purchase order."));
                    }
                    else
                    {
                        if (detail.Amount.Currency != null)
                        {
                            try
                            {
                                defaultCurrency = checkCurrency(defaultCurrency, detail.Amount.Currency);
                            }
                            catch (ArgumentException ex)
                            {
                                integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.invalid.currency",
                                    "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId, message: ex.Message));
                            }
                        }
                        else
                        {
                            // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.UnitPrice", "The UnitPrice currency is a required when submitting a line item. ");
                            integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.amount.currency",
                                "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                                message: "The amount currency is a required property when submitting account details on a blanket purchase order."));
                        }
                        if (!detail.Amount.Value.HasValue)
                        {
                            // throw new ArgumentNullException("blanketPurchaseOrders.LineItems.UnitPrice", "The UnitPrice value is required when submitting a line item. ");
                            integrationApiException.AddError(new IntegrationApiError("blanketPurchaseOrders.orderDetails.amount.value",
                                "blanket-purchase-orders request validation failed", guid: sourceGuid, id: sourceId,
                                message: "The amount value is a required property when submitting account details on a blanket purchase order."));
                        }
                    }
                }
            }
            if (integrationApiException != null && integrationApiException.Errors != null && integrationApiException.Errors.Any())
            {
                throw integrationApiException;
            }
        }

        private CurrencyIsoCode? checkCurrency(CurrencyIsoCode? defaultValue, CurrencyIsoCode? newValue)
        {
            if (defaultValue != null && defaultValue != newValue && newValue != null)
            {
                throw new ArgumentException("blanketPurchaseOrders.Currency", "All currency codes in the request must be the same and cannot differ.");
            }
            CurrencyIsoCode? cc = newValue == null ? defaultValue : newValue;
            return cc;
        }

        #endregion

        /// <summary>
        /// Delete (DELETE) a purchaseOrders
        /// </summary>
        /// <param name="id">GUID to desired purchaseOrders</param>
        [HttpDelete]
        [Route("/blanket-purchase-orders/{id}", Name = "DefaultDeleteBlanketPurchaseOrders", Order = -10)]
        public async Task<IActionResult> DeleteBlanketPurchaseOrdersAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

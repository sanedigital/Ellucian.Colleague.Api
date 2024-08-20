// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Constraints;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for vouchers.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class VouchersController : BaseCompressedApiController
    {
        private readonly IVoucherService voucherService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the VouchersController object.
        /// </summary>
        /// <param name="voucherService">Voucher service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public VouchersController(IVoucherService voucherService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.voucherService = voucherService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a specified voucher.
        /// </summary>
        /// <param name="voucherId">ID of the requested voucher.</param>
        /// <returns>Voucher DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.VOUCHER, and requires access to at least one of the
        /// general ledger numbers on the voucher line items.
        /// </accessComments>
        [Obsolete("Obsolete as of API verson 1.15; use version 2 of this endpoint")]
        [HttpGet]
        [HeaderVersionRoute("/vouchers/{voucherId}", 1, false, Name = "GetVoucher")]
        public async Task<ActionResult<Voucher>> GetVoucherAsync(string voucherId)
        {
            if (string.IsNullOrEmpty(voucherId))
            {
                string message = "A Voucher ID must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var voucher = await voucherService.GetVoucherAsync(voucherId);
                return voucher;
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the voucher.", HttpStatusCode.Forbidden);
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
            catch (ApplicationException aex)
            {
                logger.LogError(aex, aex.Message);
                return CreateHttpResponseException("Invalid data in record.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return CreateHttpResponseException("Unable to get the voucher.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a specified voucher.
        /// </summary>
        /// <param name="voucherId">ID of the requested voucher.</param>
        /// <returns>Voucher DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.VOUCHER, and requires access to at least one of the
        /// general ledger numbers on the voucher line items.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/vouchers/{voucherId}", 2, true, Name = "GetVoucher2")]
        public async Task<ActionResult<Voucher2>> GetVoucher2Async(string voucherId)
        {
            if (string.IsNullOrEmpty(voucherId))
            {
                string message = "A Voucher ID must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var voucher = await voucherService.GetVoucher2Async(voucherId);
                return voucher;
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the voucher.", HttpStatusCode.Forbidden);
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
                logger.LogDebug(csee, "Session expired - unable to get the voucher.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the voucher.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves list of voucher summary
        /// </summary>
        /// <param name="personId">ID logged in user</param>
        /// <returns>list of Voucher Summary DTO</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission VIEW.VOUCHER.
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague Web API 1.30. Use QueryVoucherSummariesAsync.")]
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "personId")]
        [HeaderVersionRoute("/voucher-summaries", 1, true, Name = "GetVoucherSummariesAsync")]
        public async Task<ActionResult<IEnumerable<VoucherSummary>>> GetVoucherSummariesAsync([FromQuery] string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                string message = "person Id must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var Voucher = await voucherService.GetVoucherSummariesAsync(personId);
                return Ok(Voucher);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the Voucher summary.", HttpStatusCode.Forbidden);
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
                return CreateHttpResponseException("Unable to get the Voucher summary.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create / Update a voucher.
        /// </summary>
        /// <param name="voucherCreateUpdateRequest">The voucher create update request DTO.</param>        
        /// <returns>The voucher create response DTO.</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission CREATE.UPDATE.VOUCHER.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/vouchers", 1, true, Name = "PostVouchers")]
        public async Task<ActionResult<Dtos.ColleagueFinance.VoucherCreateUpdateResponse>> PostVoucherAsync([FromBody] Dtos.ColleagueFinance.VoucherCreateUpdateRequest voucherCreateUpdateRequest)
        {
            if (voucherCreateUpdateRequest == null)
            {
                return CreateHttpResponseException("Request body must contain a valid voucher.", HttpStatusCode.BadRequest);
            }
            try
            {
                return await voucherService.CreateUpdateVoucherAsync(voucherCreateUpdateRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to create/update the voucher.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to create/update the voucher.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to create/update the voucher.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets a payment address of person for voucher
        /// </summary>
        /// <returns> Payment address DTO</returns>
        /// <accessComments>
        /// Requires  permission CREATE.UPDATE.VOUCHER.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/reimburse-person-address", 1, true, Name = "GetReimbursePersonAddressForVoucher")]
        public async Task<ActionResult<VendorsVoucherSearchResult>> GetReimbursePersonAddressForVoucherAsync()
        {
            try
            {
                return await voucherService.GetReimbursePersonAddressForVoucherAsync();
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the person address.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to get the person address.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to get the person address.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Void a Voucher.
        /// </summary>
        /// <param name="voucherVoidRequest">The voucher void request DTO.</param>        
        /// <returns>The voucher void response DTO.</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission CREATE.UPDATE.VOUCHER.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/vouchers-void", 1, false, Name = "VoidVoucher")]
        public async Task<ActionResult<Dtos.ColleagueFinance.VoucherVoidResponse>> VoidVoucherAsync([FromBody] Dtos.ColleagueFinance.VoucherVoidRequest voucherVoidRequest)
        {
            if (voucherVoidRequest == null)
            {
                return CreateHttpResponseException("Request body must contain a valid voucher detail.", HttpStatusCode.BadRequest);
            }
            try
            {
                return await voucherService.VoidVoucherAsync(voucherVoidRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to void the voucher.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to void the voucher.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to void the voucher.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves list of vouchers
        /// </summary>
        /// <param name="vendorId">Vendor id</param>
        /// <param name="invoiceNo">Invoice number</param>
        /// <returns>List of <see cref="Voucher2">Vouchers</see></returns>
        /// <accessComments>
        /// Requires permission VIEW.VOUCHER.
        /// </accessComments>
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "vendorId", "invoiceNo")]
        [HeaderVersionRoute("/vouchers", 1, true, Name = "GetVouchersByVendorAndInvoiceNoAsync")]
        public async Task<ActionResult<IEnumerable<Voucher2>>> GetVouchersByVendorAndInvoiceNoAsync(string vendorId, string invoiceNo)
        {
            if (string.IsNullOrEmpty(vendorId))
            {
                string message = "vendor Id must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(invoiceNo))
            {
                string message = "invoice number must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var voucherIds = await voucherService.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNo);
                return Ok(voucherIds);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument to query the voucher.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - could not get vouchers.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Retrieves list of voucher summary
        /// </summary>
        /// <param name="filterCriteria">procurement filter criteria</param>
        /// <returns>list of voucher summary DTO</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission VIEW.VOUCHER or CREATE.UPDATE.VOUCHER.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/voucher-summaries", 1, true, Name = "QueryVoucherSummariesAsync")]
        public async Task<ActionResult<IEnumerable<VoucherSummary>>> QueryVoucherSummariesAsync([FromBody] Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria filterCriteria)
        {
            if (filterCriteria == null)
            {
                return CreateHttpResponseException("Request body must contain a valid search criteria.", HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await voucherService.QueryVoucherSummariesAsync(filterCriteria));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to search vouchers.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument to search vouchers.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, knfex.Message);
                return CreateHttpResponseException("Record not found to search vouchers.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Session expired - unable to search the vouchers.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to search the vouchers.", HttpStatusCode.BadRequest);
            }
        }
    }
}

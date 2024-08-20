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

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for recurring vouchers
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class RecurringVouchersController : BaseCompressedApiController
    {
        private readonly IRecurringVoucherService recurringVoucherService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the RecurringVouchersController object
        /// </summary>
        /// <param name="recurringVoucherService">Recurring Voucher service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RecurringVouchersController(IRecurringVoucherService recurringVoucherService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.recurringVoucherService = recurringVoucherService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a specified recurring voucher
        /// </summary>
        /// <param name="recurringVoucherId">The requested recurring voucher ID</param>
        /// <returns>Recurring Voucher DTO</returns>
        /// <accessComments>
        /// Requires permission VIEW.RECURRING.VOUCHER, and requires access to at least one of the 
        /// general ledger numbers on the recurring voucher line items.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/recurring-vouchers/{recurringVoucherId}", 1, true, Name = "GetRecurringVoucher")]
        public async Task<ActionResult<RecurringVoucher>> GetRecurringVoucherAsync(string recurringVoucherId)
        {
            if (string.IsNullOrEmpty(recurringVoucherId))
            {
                string message = "A Recurring Voucher ID must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var recurringVoucher = await recurringVoucherService.GetRecurringVoucherAsync(recurringVoucherId);
                return recurringVoucher;
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the recurring voucher.", HttpStatusCode.Forbidden);
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
                return CreateHttpResponseException("Unable to get the recurring voucher.", HttpStatusCode.BadRequest);
            }
        }
    }
}

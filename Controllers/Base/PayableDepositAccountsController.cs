// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers.Base
{

    /// <summary>
    /// These routes are obsolete. Use PayableDepositDirective routes instead
    /// </summary>
    [Obsolete("Obsolete as of API 1.116")]
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PayableDepositAccountsController : BaseCompressedApiController
    {
        private readonly ILogger logger;

        /// <summary>
        /// Instantiate a new PayableDepositAccountsController
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PayableDepositAccountsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
        }

        /// <summary>
        /// This route is obsolete as of API 1.16 for security reasons. Use GetPayableDepositDirectives instead
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PayableDepositAccount>>> GetPayableDepositsAsync()
        {
            await Task.FromResult<IEnumerable<PayableDepositAccount>>(null);
            SetResourceLocationHeader("GetPayableDepositDirectives");
            return CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);
        }

        /// <summary>
        /// This route is obsolete as of API 1.16 for security reasons. Use GetPayableDepositDirective instead
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpGet]
        public async Task<ActionResult<PayableDepositAccount>> GetPayableDepositAsync([FromRoute] string id)
        {

            await Task.FromResult<PayableDepositAccount>(null);
            SetResourceLocationHeader("GetPayableDepositDirective", new { id = id });
            return CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);

        }

        /// <summary>
        /// This route is obsolete as of API 1.16 for security reasons. Use CreatePayableDepositDirective instead
        /// </summary>
        /// <param name="payableDepositAccount"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpPost]
        public async Task<IActionResult> CreatePayableDepositAsync([FromBody] PayableDepositAccount payableDepositAccount)
        {
            await Task.FromResult<PayableDepositAccount>(null);
            SetResourceLocationHeader("CreatePayableDepositDirective");
            return CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);
        }

        /// <summary>
        /// This route is obsolete as of API 1.16 for security reasons. Use UpdatePayableDepositDirective instead
        /// </summary>
        /// <param name="updatedPayableDepositAccount"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpPost]
        public async Task<ActionResult<PayableDepositAccount>> UpdatePayableDepositAsync([FromBody] PayableDepositAccount updatedPayableDepositAccount)
        {
            await Task.FromResult<PayableDepositAccount>(null);
            SetResourceLocationHeader("UpdatePayableDepositDirective", new { id = updatedPayableDepositAccount.PayableDeposits[0].Id });
            return CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);
        }

        /// <summary>
        /// This route is obsolete as of API 1.16 for security reasons. Use DeletePayableDepositDirective instead.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpDelete]
        public async Task<IActionResult> DeletePayableDepositAsync([FromRoute] string id)
        {
            await Task.FromResult<PayableDepositAccount>(null);
            SetResourceLocationHeader("DeletePayableDepositDirective", new { id = id });
            return CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);
        }

    }
}


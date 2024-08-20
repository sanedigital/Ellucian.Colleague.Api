// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to Accounts Payable Tax code information.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class AccountsPayableTaxesController : BaseCompressedApiController
    {
        private readonly IAccountsPayableTaxService accountsPayableTaxService;

        /// <summary>
        /// Constructor to initialize AccountsPayableTaxesController object.
        /// </summary>
        /// <param name="accountsPayableTaxService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AccountsPayableTaxesController(IAccountsPayableTaxService accountsPayableTaxService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.accountsPayableTaxService = accountsPayableTaxService;
        }

        /// <summary>
        /// Get all of the Accounts Payable Tax codes.
        /// </summary>
        /// <returns></returns>
        /// <note>AccountsPayableTax is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/accounts-payable-taxes", 1, true, Name = "GetAccountsPayableTaxes")]
        public async Task<ActionResult<IEnumerable<AccountsPayableTax>>> GetAccountsPayableTaxesAsync()
        {
            var accountsPayableTaxCodes = await accountsPayableTaxService.GetAccountsPayableTaxesAsync();
            return Ok(accountsPayableTaxCodes);
        }
    }
}

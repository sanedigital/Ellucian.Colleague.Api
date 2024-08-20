// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// These routes are obsolete as of API 1.16 for security reasons. Use PayrollDepositDirective routes instead
    /// </summary>
    [Obsolete("Obsolete as of API 1.16")]
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class DirectDepositsController : BaseCompressedApiController
    {

        private readonly ILogger logger;

        /// <summary>
        /// DirectDepositsController constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DirectDepositsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
        }

        /// <summary>
        /// This route is obsolete and moved permanantly to GetPayrollDepositDirectivesAsync for security reasons
        /// </summary>
        /// <param name="employeeId">Employee Id</param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpGet]
        public async Task<ActionResult<DirectDeposits>> GetDirectDepositsAsync(string employeeId)
        {
            await Task.FromResult<DirectDeposits>(null);
            SetResourceLocationHeader("GetPayrollDepositDirectives");            
            return CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);
        }

        /// <summary>
        /// This route is obsolete and moved permanantly to UpdatePayrollDepositDirectivesAsync for security reasons
        /// </summary>
        /// <param name="employeeId">Employee Id</param>
        /// <param name="updatedDirectDeposits">DirectDeposits object</param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpPost]
        public async Task<ActionResult<DirectDeposits>> UpdateDirectDepositsAsync([FromRoute] string employeeId, [FromBody] DirectDeposits updatedDirectDeposits)
        {
            await Task.FromResult<DirectDeposits>(null);
            SetResourceLocationHeader("UpdatePayrollDepositDirectives");
            return CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);
        }
    }
}

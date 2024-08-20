// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// This is the controller for the type of Tax Form Statements.
    /// </summary>
    [Obsolete("Obsolete as of API version 1.14, use HumanResourcesTaxFormStatementsController instead")]
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class TaxFormStatementsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly ITaxFormStatementService taxFormStatementService;

        /// <summary>
        /// Initialize the Tax Form Statement controller.
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        /// <param name="taxFormStatementService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TaxFormStatementsController(IAdapterRegistry adapterRegistry, ILogger logger, ITaxFormStatementService taxFormStatementService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormStatementService = taxFormStatementService;
        }

        /// <summary>
        /// Returns a set of tax form statements for the specified person and tax form ID (W-2, 1095-C, etc.).
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxFormId">Tax Form ID</param>
        /// <returns>Set of tax form statements</returns>
        [HttpGet]
        [HeaderVersionRoute("/tax-form-statements/{personId}/{taxFormId}", 1, false, Name = "GetTaxFormStatements")]
        public async Task<ActionResult<IEnumerable<TaxFormStatement>>> GetAsync(string personId, TaxForms taxFormId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            return Ok(await taxFormStatementService.GetAsync(personId, taxFormId));
        }
    }
}

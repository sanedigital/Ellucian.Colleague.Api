// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

using Ellucian.Web.Adapters;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// This is the controller for Tax Form Box Codes.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class TaxFormBoxCodesController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly ITaxFormBoxCodesService boxCodesService;

        /// <summary>
        /// This constructor initializes the Tax Form Box Codes controller.
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        /// <param name="boxCodesService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TaxFormBoxCodesController(IAdapterRegistry adapterRegistry, ILogger logger, ITaxFormBoxCodesService boxCodesService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.boxCodesService = boxCodesService;
        }

        /// <summary>
        /// Returns all BoxCodes
        /// </summary>
        /// <returns>List of BoxCodes DTO objects </returns>
        /// <accessComments>
        /// No permission is needed.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/tax-form-boxcodes", 1, false, Name = "GetAllTaxFormBoxCodes")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.TaxFormBoxCodes>>> GetAllTaxFormBoxCodesAsync()
        {
            try
            {
                var dtos = await boxCodesService.GetAllTaxFormBoxCodesAsync();
                return Ok(dtos);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get Box codes.", HttpStatusCode.BadRequest);
            }

        }

        
    }
}

// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
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
    /// Provides access to TaxFormCodesController
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class TaxFormCodesController : BaseCompressedApiController
    {
        private readonly ITaxFormsService _taxFormsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the TaxFormCodesController class.
        /// </summary>
        /// <param name="taxFormsService">Service of type <see cref="ITaxFormsService">ITaxFormsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TaxFormCodesController(ITaxFormsService taxFormsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _taxFormsService = taxFormsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all Tax forms
        /// </summary>
        /// <returns>List of Tax forms <see cref="Dtos.ColleagueFinance.TaxForm"/> objects representing matching TaxForm</returns>
        /// <accessComments>
        /// Any authenticated user can get the TaxForms
        /// </accessComments>
        /// <note>TaxForm is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/tax-form-codes", 1, true, Name = "GetTaxForms")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.TaxForm>>> GetTaxFormsAsync()
        {
            try
            {
                var taxForms = await _taxFormsService.GetTaxFormsAsync();
                return Ok(taxForms);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get taxforms.", HttpStatusCode.BadRequest);
            }
        }
    }
}

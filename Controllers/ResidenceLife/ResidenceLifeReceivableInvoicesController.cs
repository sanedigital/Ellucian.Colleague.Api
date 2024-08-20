// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.Net;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ResidenceLife.Services;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers.ResidenceLife
{
    /// <summary>
    /// APIs related to receivable invoices in the context of residence life
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class ResidenceLifeReceivableInvoicesController : BaseCompressedApiController
    {
        private readonly IResidenceLifeAccountsReceivableService _residenceLifeAccountsReceivableService;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for the ResidenceLifeReceivableInvoicesController
        /// </summary>
        /// <param name="residenceLifeAccountsReceivableService">Service of type <see cref="IResidenceLifeAccountsReceivableService">IResidenceLifeAccountsReceivableService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ResidenceLifeReceivableInvoicesController(IResidenceLifeAccountsReceivableService residenceLifeAccountsReceivableService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _residenceLifeAccountsReceivableService = residenceLifeAccountsReceivableService;
            this._logger = logger;
        }

        /// <summary>
        /// Create a new receivable invoice in the context of residence life
        /// </summary>
        /// <param name="receivableInvoice">The receivable invoice to create</param>
        /// <returns>The resulting receivable invoice</returns>
        /// <accessComments>
        /// API endpoint is secured with CREATE.RL.INVOICES permission code
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/residence-life-receivable-invoices", 1, true, Name = "Create")]
        public ActionResult<Dtos.Finance.ReceivableInvoice> PostReceivableInvoice(Dtos.ResidenceLife.ReceivableInvoice receivableInvoice)
        {
            try
            {
                return Ok(_residenceLifeAccountsReceivableService.CreateReceivableInvoice(receivableInvoice));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.ToString());
                return CreateHttpResponseException(string.Empty, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

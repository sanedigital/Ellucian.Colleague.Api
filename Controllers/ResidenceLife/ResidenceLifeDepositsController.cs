// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
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
    /// APIs related to deposits in the context of residence life
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]

    public class ResidenceLifeDepositsController : BaseCompressedApiController
    {
        private readonly IResidenceLifeAccountsReceivableService _residenceLifeAccountsReceivableService;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for the ResidenceLifeDepositsController
        /// </summary>
        /// <param name="residenceLifeAccountsReceivableService">Service of type <see cref="IResidenceLifeAccountsReceivableService">IResidenceLifeAccountsReceivableService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ResidenceLifeDepositsController(IResidenceLifeAccountsReceivableService residenceLifeAccountsReceivableService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _residenceLifeAccountsReceivableService = residenceLifeAccountsReceivableService;
            this._logger = logger;
        }

        /// <summary>
        /// Create a new deposit in the context of residence life
        /// </summary>
        /// <param name="deposit">The deposit to create</param>
        /// <returns>The resulting deposit</returns>
        /// <accessComments>
        /// API endpoint is secured with CREATE.RL.DEPOSITS permission code
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/residence-life-deposits", 1, true, Name = "CreateResidenceLifeDeposit")]
        public ActionResult<Dtos.Finance.Deposit> PostDeposit(Dtos.ResidenceLife.Deposit deposit)
        {
            try
            {
                return Ok(_residenceLifeAccountsReceivableService.CreateDeposit(deposit));
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.ToString());
                return CreateHttpResponseException(string.Empty, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

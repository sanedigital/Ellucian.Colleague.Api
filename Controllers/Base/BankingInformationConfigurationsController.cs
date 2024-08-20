// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Exposes methods to interact with Configuration objects for Colleague Self Service Banking Information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class BankingInformationConfigurationsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IBankingInformationConfigurationService bankingInformationConfigurationService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="bankingInformationConfigurationService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BankingInformationConfigurationsController(ILogger logger, IAdapterRegistry adapterRegistry, IBankingInformationConfigurationService bankingInformationConfigurationService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.adapterRegistry = adapterRegistry;
            this.bankingInformationConfigurationService = bankingInformationConfigurationService;
        }

        /// <summary>
        /// Get the Configuration object for Colleague Self Service Banking Information
        /// </summary>
        /// <returns>Returns a single banking information configuration object</returns>
        /// <note>Banking Information Configuration is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/banking-information-configuration", 1, true, Name = "BankingInformationConfiguration")]
        public async Task<ActionResult<BankingInformationConfiguration>> GetAsync()
        {
            try
            {
                var bankingInformationConfiguration = await bankingInformationConfigurationService.GetBankingInformationConfigurationAsync();
                return bankingInformationConfiguration;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = "Banking information configuration record does not exist";
                logger.LogError(knfe, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred getting banking information configuration");
                return CreateHttpResponseException(e.Message);
            }
        }
    }
}

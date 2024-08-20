// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
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


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Pay Statement Configuration Controller routes requests for configurations for pay statements
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayStatementConfigurationController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IPayStatementConfigurationService payStatementConfigurationService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        /// <param name="payStatementConfigurationService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PayStatementConfigurationController(IAdapterRegistry adapterRegistry, ILogger logger, IPayStatementConfigurationService payStatementConfigurationService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.payStatementConfigurationService = payStatementConfigurationService;
        }

        /// <summary>
        /// Get the pay statement configuration for the environment.
        /// 
        /// A successful request will return a status code of 200 and a pay statement configuration object
        /// An unsuccessful request will return a status code of 400
        /// </summary>
        /// <returns>A PayStatementConfiguration object that can be used to govern Employee Self Service Earnings Statements</returns>
        
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/pay-statement-configuration", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPayStatementConfigurationAsyncV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/pay-statement-configuration", 1, false, Name = "GetPayStatementConfigurationAsync")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets access to Paystatement Configuration object for the currently authenticated API user.",
          HttpMethodDescription = "Gets access to Paystatement Configuration object for the currently authenticated API user.")]
        public async Task<ActionResult<PayStatementConfiguration>> GetPayStatementConfigurationAsync()
        {
            try
            {
                return await payStatementConfigurationService.GetPayStatementConfigurationAsync();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred", HttpStatusCode.BadRequest);
            }
        }
    }
}

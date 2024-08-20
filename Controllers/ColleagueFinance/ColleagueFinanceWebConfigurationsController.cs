// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for ColleagueFinanceWebConfiguration
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class ColleagueFinanceWebConfigurationsController : BaseCompressedApiController
    {
        private readonly IColleagueFinanceWebConfigurationsService colleagueFinanceWebConfigurationsService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the ColleagueFinanceWebConfigurationsController object
        /// </summary>
        /// <param name="colleagueFinanceWebConfigurationsService">ColleagueFinanceWebConfigurationsService object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ColleagueFinanceWebConfigurationsController(IColleagueFinanceWebConfigurationsService colleagueFinanceWebConfigurationsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.colleagueFinanceWebConfigurationsService = colleagueFinanceWebConfigurationsService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves Colleague finance web configurations
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get the Colleague finance web configurations
        /// </accessComments>
        /// <returns>Colleague finance web configurations object</returns>
        [HttpGet]
        [HeaderVersionRoute("/cf-web-configurations", 1, false, Name = "GetColleagueFinanceWebConfigurationsAsync")]
        public async Task<ActionResult<ColleagueFinanceWebConfiguration>> GetColleagueFinanceWebConfigurationsAsync()
        {
            try
            {
                var colleagueFinanceWebConfigurations = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
                return colleagueFinanceWebConfigurations;
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the CF Web configurations.", HttpStatusCode.BadRequest);
            }
        }

    }
}

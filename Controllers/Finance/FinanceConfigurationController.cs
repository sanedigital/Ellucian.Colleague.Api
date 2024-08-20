// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Configuration;
using Ellucian.Web.Http.Controllers;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get student finance parameters and settings.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    public class FinanceConfigurationController : BaseCompressedApiController
    {
        private readonly IFinanceConfigurationService _service;
        private readonly ILogger _logger;

        /// <summary>
        /// FinanceConfigurationController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IFinanceConfigurationService">IFinanceConfigurationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinanceConfigurationController(IFinanceConfigurationService service, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _service = service;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves the configuration information for Student Finance.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>The <see cref="FinanceConfiguration">Finance Configuration</see></returns>
        /// <note>FinanceConfiguration is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration", 1, true, Name = "GetConfiguration")]
        [HeaderVersionRoute("/configuration", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetFinanceConfiguration", IsEthosEnabled = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<FinanceConfiguration> Get()
        {
            FinanceConfiguration configurationDto = null;
            try
            {
                configurationDto = _service.GetFinanceConfiguration();
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

            return Ok(configurationDto);
        }

        /// <summary>
        /// Retrieves the configuration information for Immediate Payment Control.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>The <see cref="ImmediatePaymentControl">Immediate Payment Control</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the required setup is not complete</exception>
        /// <note>ImmediatePaymentControl is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/configuration/ipc", 1, true, Name = "GetImmediatePaymentControl")]
        public ActionResult<ImmediatePaymentControl> GetImmediatePaymentControl()
        {
            try
            {
                return Ok(_service.GetImmediatePaymentControl());
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

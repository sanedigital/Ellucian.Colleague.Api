// Copyright 2022-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using System.ComponentModel;
using System.Net;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.Planning
{
    /// <summary>
    /// Provides access to retrieve Student Planning configuration data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Planning)]
    public class PlanningConfigurationController : BaseCompressedApiController
    {
        private readonly IPlanningConfigurationRepository _planningConfigurationRepository;
        private readonly ILogger _logger;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// AdvisorsController constructor
        /// </summary>
        /// <param name="planningConfigurationRepository">Interface to the planning configuration repository</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="adapterRegistry">Adapter registry</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PlanningConfigurationController(IPlanningConfigurationRepository planningConfigurationRepository, ILogger logger, IAdapterRegistry adapterRegistry, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _planningConfigurationRepository = planningConfigurationRepository;
            this._logger = logger;
            _adapterRegistry = adapterRegistry;
        }

        /// <summary>
        /// Retrieves the configuration information for Student Planning.
        /// </summary>
        /// <returns>The <see cref="PlanningConfiguration">Student Planning configuration</see> data</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the required setup is not complete</exception>
        [HttpGet]
        [HeaderVersionRoute("/configuration/planning", 1, true, Name = "GetPlanningConfiguration")]
        public async Task<ActionResult<PlanningConfiguration>> GetPlanningConfigurationAsync()
        {
            PlanningConfiguration configurationDto = null;
            try
            {
                var configurationEntity = await _planningConfigurationRepository.GetPlanningConfigurationAsync();
                var adapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.PlanningConfiguration, PlanningConfiguration>();
                configurationDto = adapter.MapToType(configurationEntity);
                return Ok(configurationDto);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving planning configuration";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}

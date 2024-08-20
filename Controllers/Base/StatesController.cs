// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
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


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides an API controller for fetching state and province codes.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class StatesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;


        /// <summary>
        /// Initializes a new instance of the StatesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StatesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.referenceDataRepository = referenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Gets information for all State and Province Codes 
        /// </summary>
        /// <returns>List of State Dtos</returns>
        /// <note>State is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/states", 1, true, Name = "GetStates")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.State>>> GetAsync()
        {
            try
            {
                var stateDtoCollection = new List<Ellucian.Colleague.Dtos.Base.State>();
                var stateCollection = await referenceDataRepository.GetStateCodesAsync();
                // Get the right adapter for the type mapping
                var stateDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.State, Ellucian.Colleague.Dtos.Base.State>();
                // Map the code and description to the Dto
                if (stateCollection != null && stateCollection.Count() > 0)
                {
                    foreach (var state in stateCollection)
                    {
                        stateDtoCollection.Add(stateDtoAdapter.MapToType(state));
                    }
                }

                return stateDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving States.";
                logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to retrieve States.");
            }


        }
    }
}

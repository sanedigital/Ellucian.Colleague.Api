// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Disability data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class DisabilityTypesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the DisabilityTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DisabilityTypesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Disability Types.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Domain.Base.Entities.DisabilityType">Disability Type codes and descriptions.</see></returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        /// <note>DisabilityType is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/disability-types", 1, true, Name = "GetDisabilityTypes")]
        public IEnumerable<Ellucian.Colleague.Dtos.Base.DisabilityType> Get()
        {
            var disabilityTypeDtoCollection = new List<Ellucian.Colleague.Dtos.Base.DisabilityType>();
            var disabilityTypeCollection = _referenceDataRepository.DisabilityTypes;
            // Get the right adapter for the type mapping
            var disabilityTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.DisabilityType, Ellucian.Colleague.Dtos.Base.DisabilityType>();
            // Map the grade entity to the grade DTO
            foreach (var disabilityType in disabilityTypeCollection)
            {
                disabilityTypeDtoCollection.Add(disabilityTypeDtoAdapter.MapToType(disabilityType));
            }

            return disabilityTypeDtoCollection;

        }
    }
}

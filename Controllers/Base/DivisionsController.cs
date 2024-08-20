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
    /// Provides access to Divisions data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class DivisionsController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the DivisionsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DivisionsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Gets information for all Divisions codes
        /// </summary>
        /// <returns>List of <see cref="Division"/>Divisions</returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        /// <note>Division is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/divisions", 1, true, Name = "GetDivisions")]
        public IEnumerable<Ellucian.Colleague.Dtos.Base.Division> Get()
        {
            var divisionDtoCollection = new List<Ellucian.Colleague.Dtos.Base.Division>();
            var divisionCollection = _referenceDataRepository.Divisions;
            // Get the right adapter for the type mapping
            var divisionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Division, Ellucian.Colleague.Dtos.Base.Division>();
            // Map the grade entity to the grade DTO
            foreach (var division in divisionCollection)
            {
                divisionDtoCollection.Add(divisionDtoAdapter.MapToType(division));
            }

            return divisionDtoCollection;

        }
    }
}

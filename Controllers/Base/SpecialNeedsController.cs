// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Special Needs data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class SpecialNeedsController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the SpecialNeedsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">ReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SpecialNeedsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Gets all of the Special Needs.
        /// </summary>
        /// <returns>All <see cref="SpecialNeed">SpecialNeeds</see></returns>
        /// <note>SpecialNeed is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/special-needs", 1, true, Name = "GetSpecialNeeds")]
        public IEnumerable<SpecialNeed> Get()
        {
            var specialNeedCollection = _referenceDataRepository.SpecialNeeds;

            // Get the right adapter for the type mapping
            var specialNeedDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.SpecialNeed, SpecialNeed>();

            // Map the courselevel entity to the program DTO
            var specialNeedDtoCollection = new List<SpecialNeed>();
            foreach (var specialNeed in specialNeedCollection)
            {
                specialNeedDtoCollection.Add(specialNeedDtoAdapter.MapToType(specialNeed));
            }

            return specialNeedDtoCollection;
        }
    }
}


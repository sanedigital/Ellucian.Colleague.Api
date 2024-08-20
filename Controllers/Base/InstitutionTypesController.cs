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
    /// Provides access to Institution Type data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class InstitutionTypesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the InstitutionTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public InstitutionTypesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Institution Types.
        /// </summary>
        /// <returns>All <see cref="InstitutionType">Institution Type codes and descriptions.</see></returns>
        /// <note>InstitutionType is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/institution-types", 1, true, Name = "GetInstitutionTypes")]
        public IEnumerable<InstitutionType> Get()
        {
            var institutionTypeCollection = _referenceDataRepository.InstitutionTypes;

            // Get the right adapter for the type mapping
            var institutionTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.InstitutionType, InstitutionType>();

            // Map the institutionType entity to the program DTO
            var institutionTypeDtoCollection = new List<InstitutionType>();
            foreach (var institutionType in institutionTypeCollection)
            {
                institutionTypeDtoCollection.Add(institutionTypeDtoAdapter.MapToType(institutionType));
            }

            return institutionTypeDtoCollection;
        }
    }
}

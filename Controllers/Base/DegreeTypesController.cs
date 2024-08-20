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
using Ellucian.Web.Adapters;
using Ellucian.Web.License;
namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Degree Type data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class DegreeTypesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the DegreeTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DegreeTypesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Degree Types.
        /// </summary>
        /// <returns>All <see cref="DegreeType">Degree Type codes and descriptions.</see></returns>
        /// <note>DegreeType is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/degree-types", 1, true, Name = "GetDegreeTypes")]
        public IEnumerable<DegreeType> Get()
        {
            var degreeTypeCollection = _referenceDataRepository.DegreeTypes;

            // Get the right adapter for the type mapping
            var degreeTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.DegreeType, DegreeType>();

            // Map the degreeType entity to the program DTO
            var degreeTypeDtoCollection = new List<DegreeType>();
            foreach (var degreeType in degreeTypeCollection)
            {
                degreeTypeDtoCollection.Add(degreeTypeDtoAdapter.MapToType(degreeType));
            }

            return degreeTypeDtoCollection;
        }
    }
}

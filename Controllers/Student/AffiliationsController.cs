// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using System.Net;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Affiliations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AffiliationsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// AdmittedStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AffiliationsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Affiliations with PilotFlag set to Yes or True.
        /// </summary>
        /// <returns>All <see cref="Affiliation">Affiliation</see> codes and descriptions.</returns>
        /// <note>Affiliation is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/affiliations", 1, true, Name = "GetAffiliations")]
        public async Task<ActionResult<IEnumerable<Affiliation>>> GetAsync()
        {
            var affiliationCollection =await _referenceDataRepository.GetAffiliationsAsync();

            // Get the right adapter for the type mapping
            var affiliationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.Affiliation, Affiliation>();

            // Map the admitted status entity to the program DTO
            var affiliationDtoCollection = new List<Affiliation>();
            foreach (var affiliation in affiliationCollection)
            {
                affiliationDtoCollection.Add(affiliationDtoAdapter.MapToType(affiliation));
            }

            return affiliationDtoCollection;
        }
    }
}

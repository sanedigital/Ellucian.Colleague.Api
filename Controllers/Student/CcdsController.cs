// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to CCD data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CcdsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the CcdsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CcdsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        // GET /api/ccds
        /// <summary>
        /// Retrieves all Certificates, Credentials, Diplomas (Ccds).
        /// </summary>
        /// <returns>All <see cref="Ccd">CCD</see> codes and descriptions.</returns>
        /// <note>Ccd is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/ccds", 1, true, Name = "GetCCDs")]
        public async Task<ActionResult<IEnumerable<Ccd>>> GetAsync()
        {
            var ccdCollection =await _referenceDataRepository.GetCcdsAsync();

            // Get the right adapter for the type mapping
            var ccdsDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Ccd, Ccd>();

            // Map the degree plan entity to the degree plan DTO
            var ccdDtoCollection = new List<Ccd>();
            foreach (var ccd in ccdCollection)
            {
                ccdDtoCollection.Add(ccdsDtoAdapter.MapToType(ccd));
            }

            return ccdDtoCollection;
        }
    }
}


// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Base.Repositories;
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
    /// Degree Controller
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class DegreesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the DegreesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DegreesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Degrees.
        /// </summary>
        /// <returns>All Degree codes and descriptions.</returns>
        /// <note>Degree is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/degrees", 1, true, Name = "GetDegrees")]
        public  async Task<IEnumerable<Degree>> GetAsync()
        {
            var degreeCollection = await _referenceDataRepository.GetDegreesAsync();

            // Get the right adapter for the type mapping
            var degreeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Degree, Degree>();

            // Map the degree plan entity to the degree plan DTO
            var degreeDtoCollection = new List<Degree>();
            foreach (var degree in degreeCollection)
            {
                degreeDtoCollection.Add(degreeDtoAdapter.MapToType(degree));
            }

            return degreeDtoCollection;
        }
    }
}

// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
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
    /// Provides access to AdmittedStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmittedStatusesController : BaseCompressedApiController
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
        public AdmittedStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Admitted Statuses.
        /// </summary>
        /// <returns>All <see cref="AdmittedStatus">Admitted Status</see> codes and descriptions.</returns>
        /// <note>AdmittedStatus is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/admitted-statuses", 1, true, Name = "GetAdmittedStatuses")]
        public async Task<ActionResult<IEnumerable<AdmittedStatus>>> GetAsync()
        {
            var admittedStatusCollection = await _referenceDataRepository.GetAdmittedStatusesAsync();

            // Get the right adapter for the type mapping
            var admittedStatusDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AdmittedStatus, AdmittedStatus>();

            // Map the admitted status entity to the program DTO
            var admittedStatusDtoCollection = new List<AdmittedStatus>();
            foreach (var admittedStatus in admittedStatusCollection)
            {
                admittedStatusDtoCollection.Add(admittedStatusDtoAdapter.MapToType(admittedStatus));
            }

            return admittedStatusDtoCollection;
        }
    }
}

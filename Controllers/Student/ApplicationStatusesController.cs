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
using Ellucian.Web.Adapters;
using Ellucian.Web.License;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to ApplicationStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ApplicationStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// ApplicationStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ApplicationStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Application Statuses.
        /// </summary>
        /// <returns>All <see cref="ApplicationStatus">Application Status</see> codes and descriptions.</returns>
        /// <note>ApplicationStatus is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/application-statuses", 1, true, Name = "GetApplicationStatuses")]
        public async Task<ActionResult<IEnumerable<ApplicationStatus>>> GetAsync()
        {
            var applicationStatusCollection = await _referenceDataRepository.GetApplicationStatusesAsync();

            // Get the right adapter for the type mapping
            var applicationStatusDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ApplicationStatus, ApplicationStatus>();

            // Map the application status entity to the program DTO
            var applicationStatusDtoCollection = new List<ApplicationStatus>();
            foreach (var applicationStatus in applicationStatusCollection)
            {
                applicationStatusDtoCollection.Add(applicationStatusDtoAdapter.MapToType(applicationStatus));
            }

            return applicationStatusDtoCollection;
        }
    }
}

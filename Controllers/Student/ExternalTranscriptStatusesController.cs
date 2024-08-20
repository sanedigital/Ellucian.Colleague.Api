// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
    /// Provides access to external transcript status data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ExternalTranscriptStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the ExternalTranscriptStatusesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ExternalTranscriptStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all External Transcript Statuses.
        /// </summary>
        /// <returns>All <see cref="ExternalTranscriptStatus">External Transcript Status</see> codes and descriptions.</returns>
        /// <note>ExternalTranscriptStatus is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/external-transcript-statuses", 1, true, Name = "GetExternalTranscriptStatuses")]
        public async Task<ActionResult<IEnumerable<ExternalTranscriptStatus>>> GetAsync()
        {
            var externalTranscriptStatusCollection =await _referenceDataRepository.GetExternalTranscriptStatusesAsync();

            // Get the right adapter for the type mapping
            var externalTranscriptStatusDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ExternalTranscriptStatus, ExternalTranscriptStatus>();

            // Map the ExternalTranscriptStatus entity to the program DTO
            var externalTranscriptStatusDtoCollection = new List<ExternalTranscriptStatus>();
            foreach (var externalTranscriptStatus in externalTranscriptStatusCollection)
            {
                externalTranscriptStatusDtoCollection.Add(externalTranscriptStatusDtoAdapter.MapToType(externalTranscriptStatus));
            }

            return externalTranscriptStatusDtoCollection;
        }
    }
}

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
    /// Provides access to transcript category data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class TranscriptCategoriesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the TranscriptCategoriesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TranscriptCategoriesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Transcript Categories.
        /// </summary>
        /// <returns>All <see cref="TranscriptCategory">Transcript Category</see> codes and descriptions.</returns>
        /// <note>TranscriptCategory is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/transcript-categories", 1, true, Name = "GetTranscriptCategories")]
        public async Task<ActionResult<IEnumerable<TranscriptCategory>>> GetAsync()
        {
            var transcriptCategoryCollection = await _referenceDataRepository.GetTranscriptCategoriesAsync();

            // Get the right adapter for the type mapping
            var transcriptCategoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.TranscriptCategory, TranscriptCategory>();

            // Map the TranscriptCategory entity to the program DTO
            var transcriptCategoryDtoCollection = new List<TranscriptCategory>();
            foreach (var transcriptCategory in transcriptCategoryCollection)
            {
                transcriptCategoryDtoCollection.Add(transcriptCategoryDtoAdapter.MapToType(transcriptCategory));
            }

            return transcriptCategoryDtoCollection;
        }
    }
}

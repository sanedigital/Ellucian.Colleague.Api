// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using System.Linq;
using System.Net;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Gown Sizes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class GownSizesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// AdmittedStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Repository of type <see cref="ILogger">IStudentReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GownSizesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.referenceDataRepository = referenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all Gown Sizes with PilotFlag set to Yes or True.
        /// </summary>
        /// <returns>All <see cref="GownSize">GownSize</see> codes and descriptions.</returns>
        /// <note>GownSizes is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/gown-sizes", 1, true, Name = "GetGownSizes")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.GownSize>>> GetAsync()
        {
            try
            {
                var gownSizeCollection = await referenceDataRepository.GetGownSizesAsync();
                // Get the right adapter for the type mapping
                var gownSizeDtoAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.GownSize, GownSize>();

                // Map the gown size entity to the program DTO
                var gownSizeDtoCollection = new List<Ellucian.Colleague.Dtos.Student.GownSize>();
                if (gownSizeCollection != null && gownSizeCollection.Any())
                {
                    foreach (var gownSize in gownSizeCollection)
                    {
                        gownSizeDtoCollection.Add(gownSizeDtoAdapter.MapToType(gownSize));
                    }
                }
                return gownSizeDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Unable to retrieve GownSize data");
                return CreateHttpResponseException("Unable to retrieve GownSize data", HttpStatusCode.BadRequest);
            }
        }
    }
}

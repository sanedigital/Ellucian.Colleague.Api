// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to the cap size data for graduation.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CapSizesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the CapSizesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CapSizesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all Cap Sizes.
        /// </summary>
        /// <returns>All <see cref="CapSize">Cap Size</see> codes and descriptions.</returns>
        /// <accessComments>Any authenticated user can retrieve cap sizes.</accessComments>
        /// <note>CapSizes are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/cap-sizes", 1, true, Name = "GetCapSizes")]
        public async Task<ActionResult<IEnumerable<CapSize>>> GetAsync()
        {
            try
            {
                var capSizeCollection = await referenceDataRepository.GetCapSizesAsync();

                // Get the right adapter for the type mapping
                var capSizeDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CapSize, CapSize>();

                // Map the CapSize entity to the program DTO
                var capSizeDtoCollection = new List<CapSize>();
                if (capSizeCollection != null && capSizeCollection.Any())
                {
                    foreach (var applicationStatusCategory in capSizeCollection)
                    {
                        capSizeDtoCollection.Add(capSizeDtoAdapter.MapToType(applicationStatusCategory));
                    }
                }
                return capSizeDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (System.Exception e)
            {
                this.logger.LogError(e, "Unable to retrieve the Cap Size information");
                return CreateHttpResponseException("Unable to retrieve the Cap Size information.", System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}

// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;

using Ellucian.Data.Colleague.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Base
{

    /// <summary>
    /// Provides access to Commencement Site data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CommencementSitesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the CommencementSitesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CommencementSitesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.referenceDataRepository = referenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves Commencement Site objects containing code and descriptions
        /// </summary>
        /// <returns>A list of CommencementSite Dto objects</returns>
        /// <accessComments>Any authenticated user can retrieve commencement sites.</accessComments>
        /// <note>CommencementSites are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/commencement-sites", 1, true, Name = "GetCommencementSites")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.CommencementSite>>> GetAsync()
        {
            try
            {
                var CommencementSiteDtoCollection = new List<Ellucian.Colleague.Dtos.Base.CommencementSite>();
                var CommencementSiteCollection = await referenceDataRepository.GetCommencementSitesAsync();
                // Get the right adapter for the type mapping
                var CommencementSiteDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CommencementSite, Ellucian.Colleague.Dtos.Base.CommencementSite>();
                // Map the CommencementSite entity to the CommencementSite DTO
                if (CommencementSiteCollection != null && CommencementSiteCollection.Any())
                {
                    foreach (var CommencementSite in CommencementSiteCollection)
                    {
                        CommencementSiteDtoCollection.Add(CommencementSiteDtoAdapter.MapToType(CommencementSite));
                    }
                }

                return CommencementSiteDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Unable to retrieve commencement site information");
                return CreateHttpResponseException("Unable to retrieve commencement site information", HttpStatusCode.BadRequest);
            }
        }

    }
}

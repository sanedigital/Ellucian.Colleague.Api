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
    /// Provides access to the yearly cycle data for courses.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class YearlyCyclesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the YearlyCyclesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public YearlyCyclesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all Yearly Cycles.
        /// </summary>
        /// <returns>All <see cref="YearlyCycle">Yearly Cycle</see> codes and descriptions.</returns>
        /// <accessComments>Any authenticated user can retrieve yearly cycles.</accessComments>
        /// <note>Yearly cycles are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/yearly-cycles", 1, true, Name = "GetYearlyCycles")]
        public async Task<ActionResult<IEnumerable<YearlyCycle>>> GetAsync()
        {
            try
            {
                var yearlyCycleCollection = await referenceDataRepository.GetYearlyCyclesAsync();

                // Get the right adapter for the type mapping
                var yearlyCycleDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.YearlyCycle, YearlyCycle>();

                // Map the YearlyCycle entity to the program DTO
                var yearlyCycleDtoCollection = new List<YearlyCycle>();
                if (yearlyCycleCollection != null && yearlyCycleCollection.Any())
                {
                    foreach (var yc in yearlyCycleCollection)
                    {
                        yearlyCycleDtoCollection.Add(yearlyCycleDtoAdapter.MapToType(yc));
                    }
                }
                return yearlyCycleDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving Yearly Cycle information";
                logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (System.Exception e)
            {
                this.logger.LogError(e, "Unable to retrieve the Yearly Cycle information");
                return CreateHttpResponseException("Unable to retrieve data");
            }
        }
    }
}

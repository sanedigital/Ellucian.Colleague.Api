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
    /// Provides access to the session cycle data for courses.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SessionCyclesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the SessionCyclesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SessionCyclesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all Session Cycles.
        /// </summary>
        /// <returns>All <see cref="SessionCycle">Session Cycle</see> codes and descriptions.</returns>
        /// <accessComments>Any authenticated user can retrieve session cycles.</accessComments>
        /// <note>SessionCycles are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/session-cycles", 1, true, Name = "GetSessionCycles")]
        public async Task<ActionResult<IEnumerable<SessionCycle>>> GetAsync()
        {
            try
            {
                var sessionCycleCollection = await referenceDataRepository.GetSessionCyclesAsync();

                // Get the right adapter for the type mapping
                var sessionCycleDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SessionCycle, SessionCycle>();

                // Map the SessionCycle entity to the program DTO
                var sessionCycleDtoCollection = new List<SessionCycle>();
                if (sessionCycleCollection != null && sessionCycleCollection.Any())
                {
                    foreach (var sc in sessionCycleCollection)
                    {
                        sessionCycleDtoCollection.Add(sessionCycleDtoAdapter.MapToType(sc));
                    }
                }
                return sessionCycleDtoCollection;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (System.Exception e)
            {
                this.logger.LogError(e, "Unable to retrieve the Session Cycle information");
                return CreateHttpResponseException("Unable to retrieve data");
            }
        }
    }
}

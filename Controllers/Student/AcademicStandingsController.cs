// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Academic Standing Code data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicStandingsController : BaseCompressedApiController
    {
        private readonly IAcademicStandingsService _academicStandingsService;
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AcademicStandingsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="academicStandingsService">Service of type <see cref="IAcademicStandingsService">IAcademicStandingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AcademicStandingsController(IAdapterRegistry adapterRegistry, IAcademicStandingsService academicStandingsService, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _academicStandingsService = academicStandingsService;
            _referenceDataRepository = referenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Gets all of the Academic Standings Codes
        /// </summary>
        /// <returns>All <see cref="AcademicStanding">AcademicStandings</see></returns>
        /// <note>AcademicStanding is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/academic-standings", 1, false, Name = "GetAcademicStandings")]
        public async Task<ActionResult<IEnumerable<AcademicStanding>>> GetAsync()
        {
            var academicStandingCollection =await  _referenceDataRepository.GetAcademicStandingsAsync();

            // Get the right adapter for the type mapping
            var academicStandingDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicStanding, AcademicStanding>();

            // Map the AdvisorType entity to the program DTO
            var academicStandingDtoCollection = new List<AcademicStanding>();
            foreach (var academicStanding in academicStandingCollection)
            {
                academicStandingDtoCollection.Add(academicStandingDtoAdapter.MapToType(academicStanding));
            }

            return academicStandingDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all accounting codes.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All accounting codes objects.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/academic-standings", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAcademicStandings", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AcademicStanding>>> GetAcademicStandingsAsync()
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var academicStandings = await _academicStandingsService.GetAcademicStandingsAsync(bypassCache);

                if (academicStandings != null && academicStandings.Any())
                {
                    AddEthosContextProperties(await _academicStandingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _academicStandingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              academicStandings.Select(a => a.Id).ToList()));
                }
                return Ok(academicStandings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a accounting code by ID.
        /// </summary>
        /// <param name="id">Id of accounting code to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.AcademicStanding">accounting code.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/academic-standings/{id}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAcademicStandingsById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.AcademicStanding>> GetAcademicStandingByIdAsync(string id)
        {
            try
            {

                AddEthosContextProperties(
                    await _academicStandingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _academicStandingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _academicStandingsService.GetAcademicStandingByIdAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Creates a AcademicStanding.
        /// </summary>
        /// <param name="academicStanding"><see cref="Dtos.AcademicStanding">AcademicStanding</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicStanding">AcademicStanding</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/academic-standings", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAcademicStandingsV8")]
        public async Task<ActionResult<Dtos.AcademicStanding>> PostAcademicStandingAsync([FromBody] Dtos.AcademicStanding academicStanding)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Updates a accounting code.
        /// </summary>
        /// <param name="id">Id of the AcademicStanding to update</param>
        /// <param name="academicStanding"><see cref="Dtos.AcademicStanding">AcademicStanding</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicStanding">AcademicStanding</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/academic-standings/{id}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAcademicStandingsV8")]
        public async Task<ActionResult<Dtos.AcademicStanding>> PutAcademicStandingAsync([FromRoute] string id, [FromBody] Dtos.AcademicStanding academicStanding)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing academicStanding
        /// </summary>
        /// <param name="id">Id of the academicStanding to delete</param>
        [HttpDelete]
        [Route("/academic-standings/{id}", Name = "DeleteAcademicStandings")]
        public async Task<IActionResult> DeleteAcademicStandingAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

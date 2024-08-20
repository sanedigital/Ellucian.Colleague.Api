// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Adapters;
using Ellucian.Web.License;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Coordination.Student.Services;
using System.Net;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to career goal data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CareerGoalsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ICareerGoalsService _careerGoalsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CareerGoalsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="careerGoalsService">Service of type <see cref="ICareerGoalsService">ICareerGoalsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CareerGoalsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository,
            ICareerGoalsService careerGoalsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;

            _careerGoalsService = careerGoalsService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Career Goals.
        /// </summary>
        /// <returns>All <see cref="CareerGoal">Career Goal</see> codes and descriptions.</returns>
        /// <note>CareerGoal is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/career-goals", 1, false, Name = "GetCareerGoals")]
        public async Task<ActionResult<IEnumerable<CareerGoal>>> GetAsync()
        {
            var careerGoalCollection = await _referenceDataRepository.GetCareerGoalsAsync(false);

            // Get the right adapter for the type mapping
            var careerGoalDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CareerGoal, CareerGoal>();

            // Map the CareerGoal entity to the program DTO
            var careerGoalDtoCollection = new List<CareerGoal>();
            foreach (var careerGoal in careerGoalCollection)
            {
                careerGoalDtoCollection.Add(careerGoalDtoAdapter.MapToType(careerGoal));
            }

            return careerGoalDtoCollection;
        }

        /// <summary>
        /// Return all careerGoals
        /// </summary>
        /// <returns>List of CareerGoals <see cref="Dtos.CareerGoals"/> objects representing matching careerGoals</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/career-goals", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCareerGoals", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CareerGoals>>> GetCareerGoalsAsync()
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var careerGoals = await _careerGoalsService.GetCareerGoalsAsync(bypassCache);

                if (careerGoals != null && careerGoals.Any())
                {
                    AddEthosContextProperties(await _careerGoalsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _careerGoalsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              careerGoals.Select(a => a.Id).ToList()));
                }
                return Ok(careerGoals);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a careerGoals using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired careerGoals</param>
        /// <returns>A careerGoals object <see cref="Dtos.CareerGoals"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/career-goals/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCareerGoalsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.CareerGoals>> GetCareerGoalsByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                   await _careerGoalsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _careerGoalsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _careerGoalsService.GetCareerGoalsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new careerGoals
        /// </summary>
        /// <param name="careerGoals">DTO of the new careerGoals</param>
        /// <returns>A careerGoals object <see cref="Dtos.CareerGoals"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/career-goals", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCareerGoalsV1.0.0")]
        public async Task<ActionResult<Dtos.CareerGoals>> PostCareerGoalsAsync([FromBody] Dtos.CareerGoals careerGoals)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing careerGoals
        /// </summary>
        /// <param name="guid">GUID of the careerGoals to update</param>
        /// <param name="careerGoals">DTO of the updated careerGoals</param>
        /// <returns>A careerGoals object <see cref="Dtos.CareerGoals"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/career-goals/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCareerGoalsV1.0.0")]
        public async Task<ActionResult<Dtos.CareerGoals>> PutCareerGoalsAsync([FromRoute] string guid, [FromBody] Dtos.CareerGoals careerGoals)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a careerGoals
        /// </summary>
        /// <param name="guid">GUID to desired careerGoals</param>
        [HttpDelete]
        [Route("/career-goals/{guid}", Name = "DefaultDeleteCareerGoals", Order = -10)]
        public async Task<IActionResult> DeleteCareerGoalsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

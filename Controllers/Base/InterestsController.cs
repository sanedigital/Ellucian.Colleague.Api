// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Web.Http.Filters;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Interest data.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    [Authorize]
    public class InterestsController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly IInterestsService _interestsService;

        /// <summary>
        /// InterestsController constructor
        /// </summary>
        /// <param name="interestsService">Service of type <see cref="IInterestsService">IInterestsService</see></param>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public InterestsController(IInterestsService interestsService, IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            this._logger = logger;
            _interestsService = interestsService;
        }

        #region Get Methods
        /// <summary>
        /// Retrieves all Interests.
        /// </summary>
        /// <returns>All <see cref="Interest">Interest codes and descriptions.</see></returns>
        /// <note>Interest is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/interests", 1, false, Name = "GetInterests")]
        public IEnumerable<Interest> GetInterests()
        {
            var InterestCollection = _referenceDataRepository.Interests;

            // Get the right adapter for the type mapping
            var InterestDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Interest, Interest>();

            // Map the Interest entity to the program DTO
            var InterestDtoCollection = new List<Interest>();
            foreach (var Interest in InterestCollection)
            {
                InterestDtoCollection.Add(InterestDtoAdapter.MapToType(Interest));
            }

            return InterestDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all interests.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Dtos.Interests">Interests</see> objects.</returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/interests", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmInterests", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Interest>>> GetHedmInterestsAsync()
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
                var interests = await _interestsService.GetHedmInterestsAsync(bypassCache); 

                if (interests != null && interests.Any())
                {

                    AddEthosContextProperties(await _interestsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _interestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              interests.Select(a => a.Id).ToList()));
                }
                return Ok(interests);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all interest areas.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Dtos.InterestArea">InterestArea</see> objects.</returns>
        /// 
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/interest-areas", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInterestAreas", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.InterestArea>>> GetInterestAreasAsync()
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
                var interestAreas = await _interestsService.GetInterestAreasAsync(bypassCache);

                if (interestAreas != null && interestAreas.Any())
                {
                    AddEthosContextProperties(await _interestsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _interestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              interestAreas.Select(a => a.Id).ToList()));
                }
                return Ok(interestAreas);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an interest by ID.
        /// </summary>
        /// <param name="id">Unique ID representing the interest to get</param>
        /// <returns>An <see cref="Dtos.Interest">Interest</see> object.</returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/interests/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmInterestById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Interest>> GetHedmInterestByIdAsync(string id)
        {
            try
            {
                var interest = await _interestsService.GetHedmInterestByIdAsync(id);
                if (interest != null)
                {

                    AddEthosContextProperties(
                        await _interestsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                        await _interestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { interest.Id }));
                }
                return interest;
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an interest area by ID.
        /// </summary>
        /// <param name="id">Unique ID representing the interest area to get</param>
        /// <returns>An <see cref="Dtos.InterestArea">InterestArea</see> object.</returns>
        [HttpGet]
        [HeaderVersionRoute("/interest-areas/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInterestAreasById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.InterestArea>> GetInterestAreasByIdAsync(string id)
        {
            try
            {
                return await _interestsService.GetInterestAreasByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }
        #endregion

        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete an existing interest in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Unique ID representing the interest to delete</param>
        [HttpDelete]
        [Route("/interests/{id}", Name = "DeleteHedmInterests", Order = -10)]
        public async Task<IActionResult> DeleteHedmInterestAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete an existing interest area in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Unique ID representing the interest area to delete</param>
        [HttpDelete]
        [Route("/interest-areas/{id}", Name = "DeleteInterestAreas", Order = -10)]
        public async Task<IActionResult> DeleteInterestAreasAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Update an Interest Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Id of the Interest to update</param>
        /// <param name="interest"><see cref="Dtos.Interest">Interest</see> to update</param>
        [HttpPut]
        [HeaderVersionRoute("/interests/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmInterestsV6")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.InterestArea>> PutHedmInterestAsync([FromRoute] string id, [FromBody] Ellucian.Colleague.Dtos.Interest interest)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Update a Interest Area Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Id of the Interest Area to update</param>
        /// <param name="interestArea"><see cref="Dtos.InterestArea">InterestArea</see> to update</param>
        [HttpPut]
        [HeaderVersionRoute("/interest-areas/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInterestAreasV6")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.InterestArea>> PutInterestAreasAsync([FromRoute] string id, [FromBody] Ellucian.Colleague.Dtos.InterestArea interestArea)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Post Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Create an Interest Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="interest"><see cref="Dtos.Interest">Interest</see> to update</param>
        [HttpPost]
        [HeaderVersionRoute("/interests", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmInterestsV6")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Interest>> PostHedmInterestAsync([FromBody] Ellucian.Colleague.Dtos.Interest interest)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Create a Interest Area Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="interestArea"><see cref="Dtos.InterestArea">InterestArea</see> to update</param>
        [HttpPost]
        [HeaderVersionRoute("/interest-areas", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInterestAreasV6")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.InterestArea>> PostInterestAreasAsync([FromBody] Ellucian.Colleague.Dtos.InterestArea interestArea)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

    }
}

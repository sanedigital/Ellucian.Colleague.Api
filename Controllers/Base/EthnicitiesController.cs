// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Ethnicity data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EthnicitiesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the EthnicitiesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EthnicitiesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IDemographicService demographicService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _demographicService = demographicService;
            this._logger = logger;
        }

        /// <summary>
        /// Gets all of the ethnicities.
        /// </summary>
        /// <returns>All <see cref="Ethnicity">Ethnicities</see></returns>
        /// <note>Ethnicity data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/ethnicities", 1, false, Name = "GetEthnicities")]
        public async Task<ActionResult<IEnumerable<Ethnicity>>> GetAsync()
        {
            try
            {
                var ethnicityCollection = await _referenceDataRepository.EthnicitiesAsync();

                // Get the right adapter for the type mapping
                var ethnicityDtoAdapter = new EthnicityEntityAdapter(_adapterRegistry, _logger);

                // Map the ethnicity entity to the program DTO
                var ethnicityDtoCollection = new List<Ethnicity>();
                foreach (var ethnicity in ethnicityCollection)
                {
                    ethnicityDtoCollection.Add(ethnicityDtoAdapter.MapToType(ethnicity));
                }

                return ethnicityDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving ethnicity types data";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <remarks>For use with Ellucian EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves all ethnicities. If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All Ethnicity objects.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/ethnicities", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEedmEthnicities2", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Ethnicity2>>> GetEthnicities2Async()
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
                var ethnicities = await _demographicService.GetEthnicities2Async(bypassCache);

                if (ethnicities != null && ethnicities.Any())
                {
                    AddEthosContextProperties(await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              ethnicities.Select(a => a.Id).ToList()));
                }
                return Ok(ethnicities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>For use with Ellucian EEDM Version 4</remarks>
        /// <summary>
        /// Retrieves an ethnicity by ID.
        /// </summary>
        /// <returns>An Ethnicity</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/ethnicities/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEthnicityById2", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Ethnicity2>> GetEthnicityById2Async(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _demographicService.GetEthnicityById2Async(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a Ethnicity.
        /// </summary>
        /// <param name="ethnicity"><see cref="Ethnicity2">Ethnicity</see> to update</param>
        /// <returns>Newly updated <see cref="Ethnicity2">Ethnicity</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/ethnicities/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEthnicities")]
        public async Task<ActionResult<Dtos.Ethnicity2>> PutEthnicitiesAsync([FromBody] Dtos.Ethnicity2 ethnicity)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a Ethnicity.
        /// </summary>
        /// <param name="ethnicity">Ethnicity to create</param>
        /// <returns>Newly created Ethnicity</returns>
        [HttpPost]
        [HeaderVersionRoute("/ethnicities", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEthnicities")]
        public async Task<ActionResult<Dtos.Ethnicity2>> PostEthnicitiesAsync([FromBody] Dtos.Ethnicity2 ethnicity)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Ethnicity
        /// </summary>
        /// <param name="id">Id of the Ethnicity to delete</param>
        [HttpDelete]
        [Route("/ethnicities/{id}", Name = "DeleteEthnicities", Order = -10)]
        public async Task<IActionResult> DeleteEthnicitiesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Controllers;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Security;
using System.Net;
using Ellucian.Colleague.Coordination.Base.Services;

using Ellucian.Web.Http.ModelBinding;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides a API controller for fetching country codes.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CountriesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly ICountriesService _countriesService;

        /// <summary>
        /// Initializes a new instance of the CountriesController class.
        /// </summary>
        /// <param name="countriesService">Service of type <see cref="ICountriesService">ICountriesService</see></param>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CountriesController(ICountriesService countriesService, IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._adapterRegistry = adapterRegistry;
            this._referenceDataRepository = referenceDataRepository;
            this._logger = logger;
            this._countriesService = countriesService;
        }

        #region countries

        /// <summary>
        /// Gets information for all Country codes
        /// </summary>
        /// <returns>List of Country Dtos</returns>
        /// <note>Country codes are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/countries", 1, false, Name = "GetCountries")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.Country>>> GetAsync()
        {
            try
            {
                var countryDtoCollection = new List<Ellucian.Colleague.Dtos.Base.Country>();
                var countryCollection = await _referenceDataRepository.GetCountryCodesAsync(false);
                // Get the right adapter for the type mapping
                var countryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Country, Ellucian.Colleague.Dtos.Base.Country>();
                // Map the grade entity to the grade DTO
                if (countryCollection != null && countryCollection.Count() > 0)
                {
                    foreach (var country in countryCollection)
                    {
                        countryDtoCollection.Add(countryDtoAdapter.MapToType(country));
                    }
                }

                return countryDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving countries.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to retrieve Countries.");
            }
        }

        /// <summary>
        /// Return all countries
        /// </summary>
        /// <returns>List of Countries <see cref="Dtos.Countries"/> objects representing matching countries</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/countries", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCountries", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Countries>>> GetCountriesAsync()
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
                var countries = await _countriesService.GetCountriesAsync(bypassCache);

                if (countries != null && countries.Any())
                {
                    AddEthosContextProperties(await _countriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _countriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              countries.Select(a => a.Id).ToList()));
                }
                return Ok(countries);
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
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving countries";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
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
        /// Read (GET) a countries using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired countries</param>
        /// <returns>A countries object <see cref="Dtos.Countries"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/countries/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCountriesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Countries>> GetCountriesByGuidAsync(string guid)
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
                   await _countriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _countriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _countriesService.GetCountriesByGuidAsync(guid);
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
        /// Create (POST) a new countries
        /// </summary>
        /// <param name="countries">DTO of the new countries</param>
        /// <returns>A countries object <see cref="Dtos.Countries"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/countries", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCountriesV1.0.0")]
        public async Task<ActionResult<Dtos.Countries>> PostCountriesAsync([FromBody] Dtos.Countries countries)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing countries
        /// </summary>
        /// <param name="guid">GUID of the countries to update</param>
        /// <param name="countries">DTO of the updated countries</param>
        /// <returns>A countries object <see cref="Dtos.Countries"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPut]
        [HeaderVersionRoute("/countries/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCountriesV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Countries>> PutCountriesAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Countries countries)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (countries == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null comment argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(countries.Id))
            {
                countries.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(countries.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != countries.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _countriesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _countriesService.ImportExtendedEthosData(await ExtractExtendedData(await _countriesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var countriesReturn = await _countriesService.PutCountriesAsync(guid,
                    await PerformPartialPayloadMerge(countries, async () => await _countriesService.GetCountriesByGuidAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _countriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return countriesReturn;
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
        /// Delete (DELETE) a countries
        /// </summary>
        /// <param name="guid">GUID to desired countries</param>
        [HttpDelete]
        [Route("/countries/{guid}", Name = "DefaultDeleteCountries", Order = -10)]
        public async Task<IActionResult> DeleteCountriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        #endregion

        #region country-iso-codes

        /// <summary>
        /// Return all countryIsoCodes
        /// </summary>
        /// <returns>List of CountryIsoCodes <see cref="Dtos.CountryIsoCodes"/> objects representing matching countryIsoCodes</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/country-iso-codes", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCountryIsoCodes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CountryIsoCodes>>> GetCountryIsoCodesAsync()
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
                var countryIsoCodes = await _countriesService.GetCountryIsoCodesAsync(bypassCache);

                if (countryIsoCodes != null && countryIsoCodes.Any())
                {
                    AddEthosContextProperties(await _countriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _countriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              countryIsoCodes.Select(a => a.Id).ToList()));
                }
                return Ok(countryIsoCodes);
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
        /// Read (GET) a countryIsoCodes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired countryIsoCodes</param>
        /// <returns>A countryIsoCodes object <see cref="Dtos.CountryIsoCodes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/country-iso-codes/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCountryIsoCodesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.CountryIsoCodes>> GetCountryIsoCodesByGuidAsync(string guid)
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
                   await _countriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _countriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _countriesService.GetCountryIsoCodesByGuidAsync(guid);
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
        /// Create (POST) a new countryIsoCodes
        /// </summary>
        /// <param name="countryIsoCodes">DTO of the new countryIsoCodes</param>
        /// <returns>A countryIsoCodes object <see cref="Dtos.CountryIsoCodes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/country-iso-codes", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCountryIsoCodesV1.0.0")]
        public async Task<ActionResult<Dtos.CountryIsoCodes>> PostCountryIsoCodesAsync([FromBody] Dtos.CountryIsoCodes countryIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing countryIsoCodes
        /// </summary>
        /// <param name="guid">GUID of the countryIsoCodes to update</param>
        /// <param name="countryIsoCodes">DTO of the updated countryIsoCodes</param>
        /// <returns>A countryIsoCodes object <see cref="Dtos.CountryIsoCodes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/country-iso-codes/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCountryIsoCodesV1.0.0")]
        public async Task<ActionResult<Dtos.CountryIsoCodes>> PutCountryIsoCodesAsync([FromRoute] string guid, [FromBody] Dtos.CountryIsoCodes countryIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a countryIsoCodes
        /// </summary>
        /// <param name="guid">GUID to desired countryIsoCodes</param>
        [HttpDelete]
        [Route("/country-iso-codes/{guid}", Name = "DefaultDeleteCountryIsoCodes", Order = -10)]
        public async Task<IActionResult> DeleteCountryIsoCodesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
        #endregion
    }
}

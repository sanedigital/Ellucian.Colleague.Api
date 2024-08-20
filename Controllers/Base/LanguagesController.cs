// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Http.ModelBinding;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Language data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class LanguagesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILanguagesService _languagesService;
        private readonly ILogger _logger;
        /// <summary>
        /// Initializes a new instance of the LanguagesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="languagesService">Language service<see cref="ILanguagesService">ILanguagesService</see></param>
        /// <param name="logger">Logger<see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public LanguagesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILanguagesService languagesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _languagesService = languagesService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Languages.
        /// </summary>
        /// <returns>All <see cref="Language">Language codes and descriptions.</see></returns>
        /// <note>Language is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/languages", 1, false, Name = "GetLanguages")]
        public IEnumerable<Language> Get()
        {
            var languageCollection = _referenceDataRepository.Languages;

            // Get the right adapter for the type mapping
            var languageDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Language, Language>();

            // Map the language entity to the program DTO
            var languageDtoCollection = new List<Language>();
            foreach (var language in languageCollection)
            {
                languageDtoCollection.Add(languageDtoAdapter.MapToType(language));
            }

            return languageDtoCollection;
        }        

        /// <summary>
        /// Return all languages
        /// </summary>
        /// <returns>List of Languages <see cref="Dtos.Languages"/> objects representing matching languages</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/languages", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetLanguages", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Languages>>> GetLanguagesAsync()
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
                var languages = await _languagesService.GetLanguagesAsync(bypassCache);

                if (languages != null && languages.Any())
                {
                    AddEthosContextProperties(await _languagesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _languagesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              languages.Select(a => a.Id).ToList()));
                }
                return Ok(languages);
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
        /// Read (GET) a languages using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired languages</param>
        /// <returns>A languages object <see cref="Dtos.Languages"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/languages/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetLanguagesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Languages>> GetLanguagesByGuidAsync(string guid)
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
                   await _languagesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _languagesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _languagesService.GetLanguagesByGuidAsync(guid);
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
        /// Create (POST) a new languages
        /// </summary>
        /// <param name="languages">DTO of the new languages</param>
        /// <returns>A languages object <see cref="Dtos.Languages"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/languages", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostLanguagesV1.0.0")]
        public async Task<ActionResult<Dtos.Languages>> PostLanguagesAsync([FromBody] Dtos.Languages languages)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing languages
        /// </summary>
        /// <param name="guid">GUID of the languages to update</param>
        /// <param name="language">DTO of the updated languages</param>
        /// <returns>A languages object <see cref="Dtos.Languages"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/languages/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutLanguagesV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Languages>> PutLanguagesAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Languages language)
        {            
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (language == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null language argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(language.Id))
            {
                language.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, language.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            var languageDto = await _languagesService.GetLanguagesByGuidAsync(language.Id);
            if (languageDto == null)
            {
                throw new ArgumentException(string.Format("Unable to retrieve langauge for guid."));
            }
            if (language.Code != null)
            {
                if (string.IsNullOrEmpty(language.Code))
                {
                    throw new ArgumentException(string.Format("The code cannot be removed for an existing language."));
                }
                else if (language.Code != languageDto.Code)
                {
                    throw new ArgumentException(string.Format("The code cannot be changed for an existing language."));
                }
            }
            else
            {
                // Drop through if Code not provided in Body.  Partial Put merge logic will keep it intact.
            }
            if (!string.IsNullOrEmpty(language.Title))
            {
                if (!string.IsNullOrEmpty(languageDto.Title))
                {
                    if (language.Title != languageDto.Title)
                    {
                        throw new ArgumentException(string.Format("The title cannot be changed for an existing language."));
                    }
                }
            }

            try
            {
                var dpList = await _languagesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                var languageReturn = await _languagesService.UpdateLanguageAsync(
                  await PerformPartialPayloadMerge(language, async () => await _languagesService.GetLanguagesByGuidAsync(guid, true),
                  dpList, _logger));

                AddEthosContextProperties(dpList,
                    await _languagesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return languageReturn;
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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }

        }

        /// <summary>
        /// Delete (DELETE) a languages
        /// </summary>
        /// <param name="guid">GUID to desired languages</param>
        [HttpDelete]
        [Route("/languages/{guid}", Name = "DefaultDeleteLanguages", Order = -10)]
        public async Task<IActionResult> DeleteLanguagesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }


        /// <summary>
        /// Return all languageIsoCodes
        /// </summary>
        /// <returns>List of LanguageIsoCodes <see cref="Dtos.LanguageIsoCodes"/> objects representing matching languageIsoCodes</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/language-iso-codes", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetLanguageIsoCodes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.LanguageIsoCodes>>> GetLanguageIsoCodesAsync()
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
                var languageIsoCodes = await _languagesService.GetLanguageIsoCodesAsync(bypassCache);

                if (languageIsoCodes != null && languageIsoCodes.Any())
                {
                    AddEthosContextProperties(await _languagesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _languagesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              languageIsoCodes.Select(a => a.Id).ToList()));
                }
                return Ok(languageIsoCodes);
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
        /// Read (GET) a languageIsoCodes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired languageIsoCodes</param>
        /// <returns>A languageIsoCodes object <see cref="Dtos.LanguageIsoCodes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/language-iso-codes/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetLanguageIsoCodesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.LanguageIsoCodes>> GetLanguageIsoCodesByGuidAsync(string guid)
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
                   await _languagesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _languagesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _languagesService.GetLanguageIsoCodeByGuidAsync(guid);
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
        /// Create (POST) a new languageIsoCodes
        /// </summary>
        /// <param name="languageIsoCodes">DTO of the new languageIsoCodes</param>
        /// <returns>A languageIsoCodes object <see cref="Dtos.LanguageIsoCodes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/language-iso-codes", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostLanguageIsoCodesV1.0.0")]
        public async Task<ActionResult<Dtos.LanguageIsoCodes>> PostLanguageIsoCodesAsync([FromBody] Dtos.LanguageIsoCodes languageIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing languageIsoCodes
        /// </summary>
        /// <param name="guid">GUID of the languageIsoCodes to update</param>
        /// <param name="languageIsoCodes">DTO of the updated languageIsoCodes</param>
        /// <returns>A languageIsoCodes object <see cref="Dtos.LanguageIsoCodes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/language-iso-codes/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutLanguageIsoCodesV1.0.0")]
        public async Task<ActionResult<Dtos.LanguageIsoCodes>> PutLanguageIsoCodesAsync([FromRoute] string guid, [FromBody] Dtos.LanguageIsoCodes languageIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a languageIsoCodes
        /// </summary>
        /// <param name="guid">GUID to desired languageIsoCodes</param>
        [HttpDelete]
        [Route("/language-iso-codes/{guid}", Name = "DefaultDeleteLanguageIsoCodes", Order = -10)]
        public async Task<IActionResult> DeleteLanguageIsoCodesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

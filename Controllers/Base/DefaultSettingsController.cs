// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to DefaultSettings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class DefaultSettingsController : BaseCompressedApiController
    {
        private readonly IDefaultSettingsService _defaultSettingsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the DefaultSettingsController class.
        /// </summary>
        /// <param name="defaultSettingsService">Service of type <see cref="IDefaultSettingsService">IDefaultSettingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DefaultSettingsController(IDefaultSettingsService defaultSettingsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _defaultSettingsService = defaultSettingsService;
            this._logger = logger;
        }

        #region GET default-settings
        /// <summary>
        /// Return all defaultSettings
        /// </summary>
        /// <returns>List of DefaultSettings <see cref="Dtos.DefaultSettings"/> objects representing matching defaultSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.DefaultSettings))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/default-settings", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetDefaultSettings", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettings>>> GetDefaultSettingsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            List<Dtos.DefaultSettingsEthos> resourcesFilter = new List<Dtos.DefaultSettingsEthos>();
            var criteriaObject = GetFilterObject<Dtos.DefaultSettings>(_logger, "criteria");
            if (CheckForEmptyFilterParameters())
                return new List<Dtos.DefaultSettings>();
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos);
            }
            try
            {
                var defaultSettings = await _defaultSettingsService.GetDefaultSettingsAsync(resourcesFilter, bypassCache);

                if (defaultSettings != null && defaultSettings.Any())
                {
                    AddEthosContextProperties(await _defaultSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _defaultSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              defaultSettings.Select(a => a.Id).ToList()));
                }
                return Ok(defaultSettings);
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
        /// Read (GET) a defaultSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/default-settings/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetDefaultSettingsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.DefaultSettings>> GetDefaultSettingsByGuidAsync(string guid)
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
                   await _defaultSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _defaultSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _defaultSettingsService.GetDefaultSettingsByGuidAsync(guid, bypassCache);
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
        #endregion

        #region GET default-settings-options
        /// <summary>
        /// Return all defaultSettings options
        /// </summary>
        /// <returns>List of DefaultSettings <see cref="Dtos.DefaultSettingsOptions"/> objects representing matching defaultSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.DefaultSettingsOptions))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/default-settings", "1.0.0", false, RouteConstants.HedtechIntegrationDefaultSettingsOptionsFormat, Name = "GetDefaultSettingsOptionsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettingsOptions>>> GetDefaultSettingsOptionsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            List<Dtos.DefaultSettingsEthos> resourcesFilter = new List<Dtos.DefaultSettingsEthos>();
            var criteriaObject = GetFilterObject<Dtos.DefaultSettingsOptions>(_logger, "criteria");
            if (CheckForEmptyFilterParameters())
                return new List<Dtos.DefaultSettingsOptions>();
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos);
            }
            try
            {
                var defaultSettings = await _defaultSettingsService.GetDefaultSettingsOptionsAsync(resourcesFilter, bypassCache);

                if (defaultSettings != null && defaultSettings.Any())
                {
                    AddEthosContextProperties(await _defaultSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _defaultSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              defaultSettings.Select(a => a.Id).ToList()));
                }
                return Ok(defaultSettings);
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
        /// Read (GET) a defaultSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettingsOptions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/default-settings/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationDefaultSettingsOptionsFormat, Name = "GetDefaultSettingsOptionsByGuidV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.DefaultSettingsOptions>> GetDefaultSettingsOptionsByGuidAsync(string guid)
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
                   await _defaultSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _defaultSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _defaultSettingsService.GetDefaultSettingsOptionsByGuidAsync(guid, bypassCache);
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

        #endregion

        #region GET default-settings-advanced-search-options
        /// <summary>
        /// Return all defaultSettings advanced search options
        /// </summary>
        /// <returns>List of DefaultSettingsAdvancedSearchOptions <see cref="Dtos.DefaultSettingsOptions"/> objects representing matching defaultSettingsAdvancedSearchOptions</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]        
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("advancedSearch", typeof(Dtos.Filters.DefaultSettingsFilter))]
        [HttpGet]
        [HeaderVersionRoute("/default-settings", "1.0.0", false, RouteConstants.HedtechIntegrationDefaultSettingsAdvancedSearchOptionsFormat, Name = "GetDefaultSettingsAdvancedSearchOptionsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettingsAdvancedSearchOptions>>> GetDefaultSettingsAdvancedSearchOptionsAsync(QueryStringFilter advancedSearch)
        {
            if (advancedSearch == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null argument",
                    IntegrationApiUtility.GetDefaultApiError("advancedSearch must be specified in the request URL when a GET operation is requested.")));
            }           

            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var advancedSearchFilter = GetFilterObject<Dtos.Filters.DefaultSettingsFilter>(_logger, "advancedSearch");

            if (string.IsNullOrEmpty(advancedSearchFilter.Keyword))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null argument",
                    IntegrationApiUtility.GetDefaultApiError("keyword must be specified in the request URL when a GET operation is requested.")));
            }
            if (advancedSearchFilter.DefaultSettings == null || string.IsNullOrEmpty(advancedSearchFilter.DefaultSettings.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null argument",
                    IntegrationApiUtility.GetDefaultApiError("defaultSettings must be specified in the request URL when a GET operation is requested.")));
            }

            try
            {
                var defaultSettings = await _defaultSettingsService.GetDefaultSettingsAdvancedSearchOptionsAsync(advancedSearchFilter, bypassCache);

                return Ok(defaultSettings);
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
        /// Read (GET) a defaultSettingsAdvancedSearchOptions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired defaultSettingsAdvancedSearchOptions</param>
        /// <returns>A defaultSettingsAdvancedSearchOptions object <see cref="Dtos.DefaultSettingsAdvancedSearchOptions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/default-settings/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationDefaultSettingsAdvancedSearchOptionsFormat, Name = "GetDefaultSettingsAdvancedSearchOptionsByGuidV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.DefaultSettingsAdvancedSearchOptions>> GetDefaultSettingsAdvancedSearchOptionsByGuidAsync(string guid)
        {
            //Get by Guid is not supported for this alternate view.
            var exception = new IntegrationApiException();
            exception.AddError(new IntegrationApiError("Invalid.Operation",
                "Invalid operation for alternate view of resource.",
                "The Get by Guid operation is only available when requesting the default-settings representation."));
            return CreateHttpResponseException(exception, HttpStatusCode.NotAcceptable);
        }

        #endregion

        /// <summary>
        /// Update (PUT) an existing DefaultSettings
        /// </summary>
        /// <param name="guid">GUID of the defaultSettings to update</param>
        /// <param name="defaultSettings">DTO of the updated defaultSettings</param>
        /// <returns>A DefaultSettings object <see cref="Dtos.DefaultSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPut]
        [HeaderVersionRoute("/default-settings/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutDefaultSettingsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.DefaultSettings>> PutDefaultSettingsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.DefaultSettings defaultSettings)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (defaultSettings == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null defaultSettings argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(defaultSettings.Id))
            {
                defaultSettings.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, defaultSettings.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {                
                var origDefaultSettings = await _defaultSettingsService.GetDefaultSettingsByGuidAsync(guid, true);
                var mergedSettings = await PerformPartialPayloadMerge(defaultSettings, origDefaultSettings,
                  await _defaultSettingsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                  _logger);

                // Changed to look at the mergedSettings data instead of simply comparing original to the new request.
                if (origDefaultSettings != null && mergedSettings != null)
                {
                    IntegrationApiException exception = null;
                    
                    if (origDefaultSettings.Source != null && mergedSettings.Source != null
                        && origDefaultSettings.Source.Value.Equals(mergedSettings.Source.Value, StringComparison.OrdinalIgnoreCase)
                        && !origDefaultSettings.Source.Title.Equals(mergedSettings.Source.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The Source Title cannot be changed for a default setting."));

                    }
                    if (!string.IsNullOrEmpty(origDefaultSettings.Title) && !origDefaultSettings.Title.Equals(mergedSettings.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The title cannot be changed for a default setting."));
                    }
                    if (!string.IsNullOrEmpty(origDefaultSettings.Description) && !origDefaultSettings.Description.Equals(mergedSettings.Description, StringComparison.OrdinalIgnoreCase))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The Description cannot be changed for a default setting."));
                    }
                    //HED-32899
                    /*
                     *Advance search is not null but either search type is null or min search length is null.
                    */
                    if (defaultSettings.AdvancedSearch != null)
                    {
                        if (!defaultSettings.AdvancedSearch.AdvanceSearchType.HasValue)
                        {
                            if (exception == null) exception = new IntegrationApiException();
                            exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The type of advanced search may not be changed for a default setting."));
                        }
                        if (!defaultSettings.AdvancedSearch.MinSearchLength.HasValue)
                        {
                            if (exception == null) exception = new IntegrationApiException();
                            exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The minimum search length needed for an advanced search may not be changed for a default setting."));
                        }
                    }
                    /*
                     *If the requested type does not match the existing IDS.SEARCH.TYPE (or the existing IDS.SEARCH.TYPE is blank), issue err13.
                    */
                    if ((origDefaultSettings.AdvancedSearch != null && mergedSettings.AdvancedSearch != null && origDefaultSettings.AdvancedSearch.AdvanceSearchType != mergedSettings.AdvancedSearch.AdvanceSearchType)||
                        (origDefaultSettings.AdvancedSearch == null && mergedSettings.AdvancedSearch != null && mergedSettings.AdvancedSearch.AdvanceSearchType.HasValue))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The type of advanced search may not be changed for a default setting."));
                    }
                    /*
                     * If the requested minSearchLength does not match the existing IDS.SEARCH.MIN.LENGTH (or IDS.SEARCH.MIN.LENGTH is blank), issue err14.
                    */
                    if (origDefaultSettings.AdvancedSearch != null && mergedSettings.AdvancedSearch != null)
                    {
                        if (origDefaultSettings.AdvancedSearch.MinSearchLength.HasValue && mergedSettings.AdvancedSearch.MinSearchLength.HasValue &&
                       !origDefaultSettings.AdvancedSearch.MinSearchLength.Value.Equals(mergedSettings.AdvancedSearch.MinSearchLength.Value) ||
                        (origDefaultSettings.AdvancedSearch == null && mergedSettings.AdvancedSearch.MinSearchLength.HasValue))
                        {
                            if (exception == null) exception = new IntegrationApiException();
                            exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The minimum search length needed for an advanced search may not be changed for a default setting."));
                        }
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                }

                return await _defaultSettingsService.UpdateDefaultSettingsAsync(mergedSettings);
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
        /// Create (POST) a new defaultSettings
        /// </summary>
        /// <param name="defaultSettings">DTO of the new defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettings"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/default-settings", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostDefaultSettingsV1.0.0")]
        public async Task<ActionResult<Dtos.DefaultSettings>> PostDefaultSettingsAsync(Dtos.DefaultSettings defaultSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing defaultSettingsOptions
        /// </summary>
        /// <param name="defaultSettings">DTO of the new defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/default-settings/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationDefaultSettingsOptionsFormat, Name = "PutDefaultSettingsOptionsV1.0.0")]
        public async Task<ActionResult<Dtos.DefaultSettingsOptions>> PutDefaultSettingsOptionsAsync(Dtos.DefaultSettingsOptions defaultSettings)
        {
            //Put is not supported for Colleague but EEDM requires full crud support.
            var exception = new IntegrationApiException();
            exception.AddError(new IntegrationApiError("Invalid.Operation",
                "Invalid operation for alternate view of resource.", 
                "The Update operation is only available when requesting the default-settings representation."));
            return CreateHttpResponseException(exception, HttpStatusCode.NotAcceptable);
        }

        /// <summary>
        /// Create (POST) a new defaultSettingsOptions
        /// </summary>
        /// <param name="defaultSettings">DTO of the new defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/default-settings", "1.0.0", true, RouteConstants.HedtechIntegrationDefaultSettingsOptionsFormat, Name = "PostDefaultSettingsOptionsV1.0.0")]
        public async Task<ActionResult<Dtos.DefaultSettingsOptions>> PostDefaultSettingsOptionsAsync(Dtos.DefaultSettingsOptions defaultSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing defaultSettingsAdvancedSearchOptions
        /// </summary>
        /// <param name="defaultSettings">DTO of the new defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/default-settings/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationDefaultSettingsAdvancedSearchOptionsFormat, Name = "PutDefaultSettingsAdvancedSearchOptionsV1.0.0")]
        public async Task<ActionResult<Dtos.DefaultSettingsOptions>> PutDefaultSettingsAdvancedSearchOptionsAsync(Dtos.DefaultSettingsOptions defaultSettings)
        {
            //Put is not supported for Colleague but EEDM requires full crud support.
            var exception = new IntegrationApiException();
            exception.AddError(new IntegrationApiError("Invalid.Operation",
                "Invalid operation for alternate view of resource.",
                "The Update operation is only available when requesting the default-settings representation."));
            return CreateHttpResponseException(exception, HttpStatusCode.NotAcceptable);
        }

        /// <summary>
        /// Create (POST) a new defaultSettingsAdvancedSearchOptions
        /// </summary>
        /// <param name="defaultSettings">DTO of the new defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/default-settings", "1.0.0", true, RouteConstants.HedtechIntegrationDefaultSettingsAdvancedSearchOptionsFormat, Name = "PostDefaultSettingsAdvancedSearchOptionsV1.0.0")]
        public async Task<ActionResult<Dtos.DefaultSettingsOptions>> PostDefaultSettingsAdvancedSearchOptionsAsync(Dtos.DefaultSettingsOptions defaultSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a defaultSettings
        /// </summary>
        /// <param name="guid">GUID to desired defaultSettings</param>
        /// <returns>IActionResult</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/default-settings/{guid}", Name = "DefaultDeleteDefaultSettings", Order = -10)]
        public async Task<IActionResult> DeleteDefaultSettingsAsync([FromRoute] string guid)
        {
            //Delete is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

    }
}

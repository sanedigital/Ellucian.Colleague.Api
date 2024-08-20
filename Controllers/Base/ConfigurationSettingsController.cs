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
    /// Provides access to ConfigurationSettings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ConfigurationSettingsController : BaseCompressedApiController
    {
        private readonly IConfigurationSettingsService _configurationSettingsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ConfigurationSettingsController class.
        /// </summary>
        /// <param name="configurationSettingsService">Service of type <see cref="IConfigurationSettingsService">IConfigurationSettingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ConfigurationSettingsController(IConfigurationSettingsService configurationSettingsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _configurationSettingsService = configurationSettingsService;
            this._logger = logger;
        }

        #region GET configuration-settings
        /// <summary>
        /// Return all configurationSettings
        /// </summary>
        /// <returns>List of ConfigurationSettings <see cref="Dtos.ConfigurationSettings"/> objects representing matching configurationSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.ConfigurationSettings))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/configuration-settings", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetConfigurationSettings", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ConfigurationSettings>>> GetConfigurationSettingsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            List<string> resourcesFilter = new List<string>();
            var criteriaObject = GetFilterObject<Dtos.ConfigurationSettings>(_logger, "criteria");
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Resources != null && criteriaObject.Ethos.Resources.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos.Resources);
            }
            try
            {
                var configurationSettings = await _configurationSettingsService.GetConfigurationSettingsAsync(resourcesFilter, bypassCache);

                if (configurationSettings != null && configurationSettings.Any())
                {
                    AddEthosContextProperties(await _configurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _configurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              configurationSettings.Select(a => a.Id).ToList()));
                }
                return Ok(configurationSettings);
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
        /// Read (GET) a configurationSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired configurationSettings</param>
        /// <returns>A configurationSettings object <see cref="Dtos.ConfigurationSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/configuration-settings/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetConfigurationSettingsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ConfigurationSettings>> GetConfigurationSettingsByGuidAsync(string guid)
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
                   await _configurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _configurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _configurationSettingsService.GetConfigurationSettingsByGuidAsync(guid, bypassCache);
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

        #region GET configuration-settings-options
        /// <summary>
        /// Return all configurationSettings options
        /// </summary>
        /// <returns>List of ConfigurationSettings <see cref="Dtos.ConfigurationSettingsOptions"/> objects representing matching configurationSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.ConfigurationSettingsOptions))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/configuration-settings", "1.0.0", false, RouteConstants.HedtechIntegrationConfigurationSettingsOptionsFormat, Name = "GetConfigurationSettingsOptionsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ConfigurationSettingsOptions>>> GetConfigurationSettingsOptionsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            List<string> resourcesFilter = new List<string>();
            var criteriaObject = GetFilterObject<Dtos.ConfigurationSettingsOptions>(_logger, "criteria");
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Resources != null && criteriaObject.Ethos.Resources.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos.Resources);
            }
            try
            {
                var configurationSettings = await _configurationSettingsService.GetConfigurationSettingsOptionsAsync(resourcesFilter, bypassCache);

                if (configurationSettings != null && configurationSettings.Any())
                {
                    AddEthosContextProperties(await _configurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _configurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              configurationSettings.Select(a => a.Id).ToList()));
                }
                return Ok(configurationSettings);
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
        /// Read (GET) a configurationSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired configurationSettings</param>
        /// <returns>A configurationSettings object <see cref="Dtos.ConfigurationSettingsOptions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/configuration-settings/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationConfigurationSettingsOptionsFormat, Name = "GetConfigurationSettingsOptionsByGuidV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ConfigurationSettingsOptions>> GetConfigurationSettingsOptionsByGuidAsync(string guid)
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
                   await _configurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _configurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _configurationSettingsService.GetConfigurationSettingsOptionsByGuidAsync(guid, bypassCache);
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

        /// <summary>
        /// Update (PUT) an existing ConfigurationSettings
        /// </summary>
        /// <param name="guid">GUID of the configurationSettings to update</param>
        /// <param name="configurationSettings">DTO of the updated configurationSettings</param>
        /// <returns>A ConfigurationSettings object <see cref="Dtos.ConfigurationSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPut]
        [HeaderVersionRoute("/configuration-settings/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutConfigurationSettingsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ConfigurationSettings>> PutConfigurationSettingsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.ConfigurationSettings configurationSettings)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (configurationSettings == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null configurationSettings argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(configurationSettings.Id))
            {
                configurationSettings.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, configurationSettings.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                var origConfigurationSettings = await _configurationSettingsService.GetConfigurationSettingsByGuidAsync(guid, true);
                var mergedSettings = await PerformPartialPayloadMerge(configurationSettings, origConfigurationSettings,
                  await _configurationSettingsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                  _logger);

                if (origConfigurationSettings != null && mergedSettings != null)
                {
                    IntegrationApiException exception = null;
                    if (origConfigurationSettings.Source != null && mergedSettings.Source != null
                        && origConfigurationSettings.Source.Value.Equals(mergedSettings.Source.Value, StringComparison.OrdinalIgnoreCase)
                        && !origConfigurationSettings.Source.Title.Equals(mergedSettings.Source.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The Source Title cannot be changed for a configuration setting."));

                    }
                    if (!string.IsNullOrEmpty(origConfigurationSettings.Title) && !origConfigurationSettings.Title.Equals(mergedSettings.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The title cannot be changed for a configuration setting."));
                    }
                    if (!string.IsNullOrEmpty(origConfigurationSettings.Description) && !origConfigurationSettings.Description.Equals(mergedSettings.Description, StringComparison.OrdinalIgnoreCase))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The Description cannot be changed for a configuration setting."));
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                }
                return await _configurationSettingsService.UpdateConfigurationSettingsAsync(mergedSettings);
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
        /// Create (POST) a new configurationSettings
        /// </summary>
        /// <param name="configurationSettings">DTO of the new configurationSettings</param>
        /// <returns>A configurationSettings object <see cref="Dtos.ConfigurationSettings"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/configuration-settings", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostConfigurationSettingsV1.0.0")]
        public async Task<ActionResult<Dtos.ConfigurationSettings>> PostConfigurationSettingsAsync(Dtos.ConfigurationSettings configurationSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing configurationSettingsOptions
        /// </summary>
        /// <param name="configurationSettings">DTO of the new configurationSettings</param>
        /// <returns>A configurationSettings object <see cref="Dtos.ConfigurationSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/configuration-settings/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationConfigurationSettingsOptionsFormat, Name = "PutConfigurationSettingsOptionsV1.0.0")]
        public async Task<ActionResult<Dtos.ConfigurationSettingsOptions>> PutConfigurationSettingsOptionsAsync(Dtos.ConfigurationSettingsOptions configurationSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Create (POST) a new configurationSettingsOptions
        /// </summary>
        /// <param name="configurationSettings">DTO of the new configurationSettings</param>
        /// <returns>A configurationSettings object <see cref="Dtos.ConfigurationSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/configuration-settings", "1.0.0", true, RouteConstants.HedtechIntegrationConfigurationSettingsOptionsFormat, Name = "PostConfigurationSettingsOptionsV1.0.0")]
        public async Task<ActionResult<Dtos.ConfigurationSettingsOptions>> PostConfigurationSettingsOptionsAsync(Dtos.ConfigurationSettingsOptions configurationSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a configurationSettings
        /// </summary>
        /// <param name="guid">GUID to desired configurationSettings</param>
        /// <returns>IActionResult</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/configuration-settings/{guid}", Name = "DefaultDeleteConfigurationSettings", Order = -10)]
        public async Task<IActionResult> DeleteConfigurationSettingsAsync([FromRoute] string guid)
        {
            //Delete is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

    }
}

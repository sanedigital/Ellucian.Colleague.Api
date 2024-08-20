// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to CollectionConfigurationSettings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CollectionConfigurationSettingsController : BaseCompressedApiController
    {
        private readonly ICollectionConfigurationSettingsService _collectionConfigurationSettingsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CollectionConfigurationSettingsController class.
        /// </summary>
        /// <param name="collectionConfigurationSettingsService">Service of type <see cref="ICollectionConfigurationSettingsService">ICollectionConfigurationSettingsService</see></param>
        /// <param name="actionAccessor"></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="apiSettings"></param>
        public CollectionConfigurationSettingsController(ICollectionConfigurationSettingsService collectionConfigurationSettingsService, IActionContextAccessor actionAccessor, ILogger logger, ApiSettings apiSettings) : base(actionAccessor, apiSettings)
        {
            _collectionConfigurationSettingsService = collectionConfigurationSettingsService;
            this._logger = logger;
        }

        #region GET collection-configuration-settings
        /// <summary>
        /// Return all collectionConfigurationSettings
        /// </summary>
        /// <returns>List of CollectionConfigurationSettings <see cref="Dtos.CollectionConfigurationSettings"/> objects representing matching collectionConfigurationSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.CollectionConfigurationSettings))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/collection-configuration-settings", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCollectionConfigurationSettings", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.CollectionConfigurationSettings>>> GetCollectionConfigurationSettingsAsync(QueryStringFilter criteria)
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
            var criteriaObject = GetFilterObject<Dtos.CollectionConfigurationSettingsOptions>(_logger, "criteria");
            if (CheckForEmptyFilterParameters())
                return new List<Dtos.CollectionConfigurationSettings>();
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos);
            }
            try
            {
                var collectionConfigurationSettings = await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsAsync(resourcesFilter, bypassCache);

                if (collectionConfigurationSettings != null && collectionConfigurationSettings.Any())
                {
                    AddEthosContextProperties(await _collectionConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _collectionConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              collectionConfigurationSettings.Select(a => a.Id).ToList()));
                }
                return Ok(collectionConfigurationSettings);
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
        /// Read (GET) a collectionConfigurationSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired collectionConfigurationSettings</param>
        /// <returns>A collectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/collection-configuration-settings/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCollectionConfigurationSettingsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.CollectionConfigurationSettings>> GetCollectionConfigurationSettingsByGuidAsync(string guid)
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
                   await _collectionConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _collectionConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsByGuidAsync(guid, bypassCache);
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

        #region GET collection-configuration-settings-options
        /// <summary>
        /// Return all collectionConfigurationSettings options
        /// </summary>
        /// <returns>List of CollectionConfigurationSettings <see cref="Dtos.CollectionConfigurationSettingsOptions"/> objects representing matching collectionConfigurationSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.CollectionConfigurationSettingsOptions))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/collection-configuration-settings", "1.0.0", false, RouteConstants.HedtechIntegrationCollectionConfigurationSettingsOptionsFormat, Name = "GetCollectionConfigurationSettingsOptionsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CollectionConfigurationSettingsOptions>>> GetCollectionConfigurationSettingsOptionsAsync(QueryStringFilter criteria)
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
            var criteriaObject = GetFilterObject<Dtos.CollectionConfigurationSettingsOptions>(_logger, "criteria");
            if (CheckForEmptyFilterParameters())
                return new List<Dtos.CollectionConfigurationSettingsOptions>();
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos);
            }
            try
            {
                var collectionConfigurationSettings = await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsOptionsAsync(resourcesFilter, bypassCache);

                if (collectionConfigurationSettings != null && collectionConfigurationSettings.Any())
                {
                    AddEthosContextProperties(await _collectionConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _collectionConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              collectionConfigurationSettings.Select(a => a.Id).ToList()));
                }
                return Ok(collectionConfigurationSettings);
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
        /// Read (GET) a collectionConfigurationSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired collectionConfigurationSettings</param>
        /// <returns>A collectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettingsOptions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/collection-configuration-settings/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationCollectionConfigurationSettingsOptionsFormat, Name = "GetCollectionConfigurationSettingsOptionsByGuidV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.CollectionConfigurationSettingsOptions>> GetCollectionConfigurationSettingsOptionsByGuidAsync(string guid)
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
                   await _collectionConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _collectionConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsOptionsByGuidAsync(guid, bypassCache);
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
        /// Update (PUT) an existing CollectionConfigurationSettings
        /// </summary>
        /// <param name="guid">GUID of the collectionConfigurationSettings to update</param>
        /// <param name="collectionConfigurationSettings">DTO of the updated collectionConfigurationSettings</param>
        /// <returns>A CollectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPut]
        [HeaderVersionRoute("/collection-configuration-settings/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCollectionConfigurationSettingsV1.0.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.CollectionConfigurationSettings>> PutCollectionConfigurationSettingsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.CollectionConfigurationSettings collectionConfigurationSettings)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (collectionConfigurationSettings == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null collectionConfigurationSettings argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(collectionConfigurationSettings.Id))
            {
                collectionConfigurationSettings.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, collectionConfigurationSettings.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                var configSettings = await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsByGuidAsync(guid, true);
                var mergedSettings = await PerformPartialPayloadMerge(collectionConfigurationSettings, configSettings,
                  await _collectionConfigurationSettingsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                  _logger);

                if (configSettings != null && mergedSettings != null)
                {
                    IntegrationApiException exception = null;

                    if (configSettings.Source != null && mergedSettings.Source != null)
                    {
                        // Check for missing source.value properties
                        var invalidValues = mergedSettings.Source.Where(cs => string.IsNullOrEmpty(cs.Value));
                        if (invalidValues != null && invalidValues.Any())
                        {
                            if (exception == null) exception = new IntegrationApiException();
                            exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The value property is required when defining a sourceSettings object."));
                        }
                        else
                        {
                            // Check for duplicate source settings values
                            var uniqueValues = mergedSettings.Source.Select(cs => cs.Value).Distinct();
                            if (uniqueValues.Count() != mergedSettings.Source.Count())
                            {
                                if (exception == null) exception = new IntegrationApiException();
                                exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "Duplicate values are not allowed when defining a source settings value."));
                            }
                            foreach (var source in configSettings.Source)
                            {
                                var matchingSource = mergedSettings.Source.Where(ms => ms.Value.Equals(source.Value, StringComparison.OrdinalIgnoreCase));
                                foreach (var matchSource in matchingSource)
                                {
                                    if (matchSource.Title != source.Title)
                                    {
                                        if (exception == null) exception = new IntegrationApiException();
                                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The source settings title cannot be changed for a collection configuration setting."));
                                    }
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(mergedSettings.Title) && !mergedSettings.Title.Equals(configSettings.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The title cannot be changed for a collection configuration setting."));
                    }
                    if (!string.IsNullOrEmpty(mergedSettings.Description) && !mergedSettings.Description.Equals(configSettings.Description, StringComparison.OrdinalIgnoreCase))
                    {
                        if ( exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The description cannot be changed for a collection configuration setting."));         
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                }

                return await _collectionConfigurationSettingsService.UpdateCollectionConfigurationSettingsAsync(mergedSettings);
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
        /// Create (POST) a new collectionConfigurationSettings
        /// </summary>
        /// <param name="collectionConfigurationSettings">DTO of the new collectionConfigurationSettings</param>
        /// <returns>A collectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettings"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/collection-configuration-settings", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCollectionConfigurationSettingsV1.0.0")]
        public async Task<ActionResult<Dtos.CollectionConfigurationSettings>> PostCollectionConfigurationSettingsAsync(Dtos.CollectionConfigurationSettings collectionConfigurationSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing collectionConfigurationSettingsOptions
        /// </summary>
        /// <param name="collectionConfigurationSettings">DTO of the new collectionConfigurationSettings</param>
        /// <returns>A collectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/collection-configuration-settings/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationCollectionConfigurationSettingsOptionsFormat, Name = "PutCollectionConfigurationSettingsOptionsV1.0.0")]
        public async Task<ActionResult<Dtos.CollectionConfigurationSettingsOptions>> PutCollectionConfigurationSettingsOptionsAsync(Dtos.CollectionConfigurationSettingsOptions collectionConfigurationSettings)
        {
            //Put is not supported for Colleague but EEDM requires full crud support.
            var exception = new IntegrationApiException();
            exception.AddError(new IntegrationApiError("Invalid.Operation",
                "Invalid operation for alternate view of resource.",
                "The Update operation is only available when requesting the collection-configuration-settings representation."));
            return CreateHttpResponseException(exception, HttpStatusCode.NotAcceptable);
        }

        /// <summary>
        /// Create (POST) a new collectionConfigurationSettingsOptions
        /// </summary>
        /// <param name="collectionConfigurationSettings">DTO of the new collectionConfigurationSettings</param>
        /// <returns>A collectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/collection-configuration-settings", "1.0.0", true, RouteConstants.HedtechIntegrationCollectionConfigurationSettingsOptionsFormat, Name = "PostCollectionConfigurationSettingsOptionsV1.0.0")]
        public async Task<ActionResult<Dtos.CollectionConfigurationSettingsOptions>> PostCollectionConfigurationSettingsOptionsAsync(Dtos.CollectionConfigurationSettingsOptions collectionConfigurationSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a collectionConfigurationSettings
        /// </summary>
        /// <param name="guid">GUID to desired collectionConfigurationSettings</param>
        /// <returns>IActionResult</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/collection-configuration-settings/{guid}", Name = "DefaultDeleteCollectionConfigurationSettings", Order = -10)]
        public async Task<IActionResult> DeleteCollectionConfigurationSettingsAsync([FromRoute] string guid)
        {
            //Delete is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

    }
}

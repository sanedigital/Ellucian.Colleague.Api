// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.EthosExtend;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to EthosApiBuilder data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EthosApiBuilderController : BaseCompressedApiController
    {
        private readonly IEthosApiBuilderService _ethosApiBuilderService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EthosApiBuilderController class.
        /// </summary>
        /// <param name="ethosApiBuilderService">Service of type <see cref="IEthosApiBuilderService">IEthosApiBuilderService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EthosApiBuilderController(IEthosApiBuilderService ethosApiBuilderService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _ethosApiBuilderService = ethosApiBuilderService;
            this._logger = logger;
        }

        #region GET

        /// <summary>
        /// Respond to route and execute the appropriate method
        /// </summary>
        /// <param name="routeInfo">Data about the route</param>
        /// <param name="configuration"></param>
        /// <param name="extendedEthosVersion"></param>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="ethosApiBuilder"></param>
        /// <returns></returns>
        [HttpGet, HttpPost, HttpPut, HttpDelete]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        private async Task<IActionResult> GetEthosApiBuilderAsync(EthosResourceRouteInfo routeInfo, EthosApiConfiguration configuration, EthosExtensibleData extendedEthosVersion, string id, Paging page, Dtos.EthosApiBuilder ethosApiBuilder)
        {
            var method = routeInfo.RequestMethod.Method;

            var versionWithStatus = routeInfo.ResourceVersionNumber;
            if (routeInfo.ResourceVersionStatus.Equals("beta", StringComparison.OrdinalIgnoreCase))
            {
                versionWithStatus = string.Concat(versionWithStatus, "-beta");
            }
            if (!string.IsNullOrEmpty(routeInfo.ParentName))
            {
                if (method == "DELETE")
                {
                    return NotAcceptableStatusException();
                }
                else
                {
                    CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.{0}.v{1}+json", routeInfo.ExtendedSchemaResourceId.ToLower(), versionWithStatus));
                }
            }
            else
            {
                CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.v{0}+json", versionWithStatus));
            }

            try
            {
                if (string.IsNullOrEmpty(configuration.PrimaryGuidSource) && string.IsNullOrEmpty(id))
                {
                    var extendedKeyDictionaries = await ValidateIdsFromQueryAsync(routeInfo, extendedEthosVersion);
                    var extendedFileDefinitions = extendedKeyDictionaries.Item1;
                    var extendedKeyDefinitions = extendedKeyDictionaries.Item2;

                    // Check for complete key identifiers included in request
                    bool completeKeyFound = true;
                    foreach (var columnName in configuration.ColleagueKeyNames)
                    {
                        if (!extendedKeyDefinitions.ContainsKey(columnName))
                        {
                            completeKeyFound = false;
                        }
                    }

                    // Check for additional filtering properties
                    bool additonalPropertiesFound = !(completeKeyFound && routeInfo.ExtendedFilterDefinitions.Count() == configuration.ColleagueKeyNames.Count());
                    if (completeKeyFound && !additonalPropertiesFound)
                    {
                        id = BuildRecordKeyFromFilterData(extendedKeyDictionaries, extendedEthosVersion, configuration.ApiType);
                        if (!string.IsNullOrEmpty(id))
                        {
                            if (method == "GET_ALL") method = "GET_ID";
                            if (method == "POST") method = "PUT";
                        }
                    }
                }
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

            if (string.IsNullOrEmpty(id))
            {
                switch (method)
                {
                    case ("GET"):
                        return await GetEthosApiBuilderAsync(routeInfo, configuration, extendedEthosVersion, page);
                    case ("GET_ALL"):
                        return await GetEthosApiBuilderAsync(routeInfo, configuration, extendedEthosVersion, page);
                    case ("POST_QAPI"):
                        return await GetEthosApiBuilderAsync(routeInfo, configuration, extendedEthosVersion, page);
                    case ("POST"):
                        return await PostEthosApiBuilderAsync(routeInfo, configuration, extendedEthosVersion, ethosApiBuilder);
                    default:
                        {
                            return MethodNotAllowedException();
                        }
                }
            }
            else
            {
                switch (method)
                {
                    case ("GET"):
                        return await GetEthosApiBuilderByIdAsync(routeInfo, configuration, extendedEthosVersion, id);
                    case ("GET_ID"):
                        return await GetEthosApiBuilderByIdAsync(routeInfo, configuration, extendedEthosVersion, id);
                    case ("POST_QAPI"):
                        page = ConfigurePage(page, configuration);
                        var ethosApiBuilderDto = await GetEthosApiBuilderByIdAsync(routeInfo, configuration, extendedEthosVersion, id);
                        if (ethosApiBuilderDto is OkObjectResult)
                        {
                            var ethosApiBuilderDtos = new List<Dtos.EthosApiBuilder>() { ((OkObjectResult)ethosApiBuilderDto).Value as Dtos.EthosApiBuilder };
                            return new PagedActionResult<IEnumerable<Dtos.EthosApiBuilder>>(ethosApiBuilderDtos, page, 1, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                        }
                        else
                        {
                            return ethosApiBuilderDto;
                        }
                        break;
                    case ("PUT"):
                        return await PutEthosApiBuilderAsync(routeInfo, configuration, extendedEthosVersion, id, ethosApiBuilder);
                    case ("DELETE"):
                        return await DeleteEthosApiBuilderAsync(routeInfo, configuration, extendedEthosVersion, id);
                    default:
                        {
                            return MethodNotAllowedException();
                        }
                }
            }

            // Should be unreachable
            return MethodNotAllowedException();
        }

        /// <summary>
        /// Gets all data records for a single API extension
        /// </summary>
        /// <param name="routeInfo">Data about the route</param>
        /// <param name="configuration"></param>
        /// <param name="extendedEthosVersion"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        private async Task<IActionResult> GetEthosApiBuilderAsync(EthosResourceRouteInfo routeInfo, EthosApiConfiguration configuration, EthosExtensibleData extendedEthosVersion, Paging page)
        {
            bool bypassCache = routeInfo.BypassCache ? true : false;

            try
            {
                page = ConfigurePage(page, configuration);

                var filterDictionary = await ValidateFilterParametersAsync(routeInfo.ExtendedFilterDefinitions, extendedEthosVersion);

                var pageOfItems = await _ethosApiBuilderService.GetEthosApiBuilderAsync(page.Offset, page.Limit, configuration.ResourceName, filterDictionary, bypassCache);

                AddEthosContextProperties(await _ethosApiBuilderService.GetDataPrivacyListByApi(routeInfo, bypassCache),
                              await _ethosApiBuilderService.GetExtendedEthosDataByResource(routeInfo,
                              pageOfItems.Item1.Select(a => a._Id).ToList()));

                // Set Content Restricted Header if required.
                var secureDataDefinition = _ethosApiBuilderService.GetSecureDataDefinition();
                if (secureDataDefinition != null && secureDataDefinition.Item1)
                {
                    SetContentRestrictedHeader("partial");
                }

                return new PagedActionResult<IEnumerable<Dtos.EthosApiBuilder>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                var exception = new IntegrationApiException();
                exception.AddError(new IntegrationApiError("GUID.Not.Found", "An error occurred translating the GUID to an ID.", e.Message, HttpStatusCode.NotFound));
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
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
        /// Retrieves a single record by ID for extended data.
        /// </summary>
        /// <param name="routeInfo">Data about the route</param>
        /// <param name="configuration"></param>
        /// <param name="extendedEthosVersion"></param>
        /// <param name="id">Id of record to retrieve.</param>
        /// <returns>An extended data object.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        private async Task<IActionResult> GetEthosApiBuilderByIdAsync(EthosResourceRouteInfo routeInfo, EthosApiConfiguration configuration, EthosExtensibleData extendedEthosVersion, string id)
        {
            bool bypassCache = routeInfo.BypassCache;

            try
            {
                var recordKey = id;

                if (string.IsNullOrEmpty(recordKey))
                {
                    throw new ArgumentNullException(string.Format("Must provide an id for '{0}'.", routeInfo.ResourceName));
                }

                Dtos.EthosApiBuilder ethosApiBuilder = null;
                ethosApiBuilder = await _ethosApiBuilderService.GetEthosApiBuilderByIdAsync(id, configuration.ResourceName);

                if (ethosApiBuilder != null)
                {
                    var extendData = await _ethosApiBuilderService.GetExtendedEthosDataByResource(routeInfo,
                              new List<string>() { ethosApiBuilder._Id });

                    AddEthosContextProperties(await _ethosApiBuilderService.GetDataPrivacyListByApi(routeInfo, bypassCache),
                              extendData);

                    // Set Content Restricted Header if required.
                    var secureDataDefinition = _ethosApiBuilderService.GetSecureDataDefinition();
                    if (secureDataDefinition != null && secureDataDefinition.Item1)
                    {
                        SetContentRestrictedHeader("partial");
                    }

                    if (!string.IsNullOrEmpty(configuration.CurrentUserIdPath))
                    {
                        if (extendData != null && extendData.Any())
                        {
                            var resourceId = extendData.Select(pi => pi.ResourceId).FirstOrDefault();
                            if (string.IsNullOrEmpty(resourceId) || resourceId != recordKey.Split(DmiString._XM)[0])
                            {
                                throw new PermissionsException("User is not authorized to view this content.");
                            }
                        }
                        else
                        {
                            throw new PermissionsException("User is not authorized to view this content.");
                        }
                    }
                    ethosApiBuilder._Id = (extendData != null && extendData.Any()) ? extendData.FirstOrDefault().ResourceId : recordKey;
                }

                return Ok(ethosApiBuilder);
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
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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

        #region POST
        /// <summary>
        /// Creates a EthosApiBuilder.
        /// </summary>
        /// <param name="routeInfo">Data about the route</param>
        /// <param name="configuration"></param>
        /// <param name="extendedEthosVersion"></param>
        /// <param name="ethosApiBuilder"><see cref="Dtos.EthosApiBuilder">ethosApiBuilder</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.EthosApiBuilder">EthosApiBuilder</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [ServiceFilter(typeof(EedmResponseFilter))]
        private async Task<IActionResult> PostEthosApiBuilderAsync(EthosResourceRouteInfo routeInfo, EthosApiConfiguration configuration, EthosExtensibleData extendedEthosVersion, [ModelBinder(typeof(EedmModelBinder))] Dtos.EthosApiBuilder ethosApiBuilder)
        {
            bool bypassCache = routeInfo.BypassCache;

            try
            {
                if (ethosApiBuilder == null)
                {
                    return CreateHttpResponseException(new IntegrationApiException(string.Format("Must provide a request body for '{0}'.", routeInfo.ResourceName)));
                }
                if (string.IsNullOrEmpty(ethosApiBuilder._Id) && !string.IsNullOrEmpty(configuration.PrimaryGuidSource))
                {
                    return CreateHttpResponseException(new IntegrationApiException(string.Format("Must provide a valid GUID for '{0}' POST in request body.", routeInfo.ResourceName)));
                }
                if (ethosApiBuilder._Id != Guid.Empty.ToString() && !string.IsNullOrEmpty(configuration.PrimaryGuidSource))
                {
                    return CreateHttpResponseException(new IntegrationApiException(string.Format("The requested GUID cannot be consumed on a '{0}' POST request.", routeInfo.ResourceName)));
                }

                if (!string.IsNullOrEmpty(configuration.PrimaryGuidSource) || !string.IsNullOrEmpty(configuration.PrimaryKeyName))
                {
                    if (!string.IsNullOrEmpty(configuration.PrimaryKeyName) && string.IsNullOrEmpty(ethosApiBuilder._Id))
                    {
                        ethosApiBuilder._Id = "$NEW";
                    }
                    Validate(ethosApiBuilder);
                }
                else
                {
                    if (!string.IsNullOrEmpty(configuration.ProcessId))
                    {
                        ethosApiBuilder._Id = configuration.ProcessId;
                    }
                    else
                    {
                        ethosApiBuilder._Id = configuration.PrimaryEntity;
                    }
                }

                //get Data Privacy List
                var dpList = await _ethosApiBuilderService.GetDataPrivacyListByApi(routeInfo, true);

                //call import extend method that needs the extracted extension data and the config
                await _ethosApiBuilderService.ImportExtendedEthosData(await ExtractExtendedData(await _ethosApiBuilderService.GetExtendedEthosConfigurationByResource(routeInfo), _logger));

                var filterDictionary = await ValidateFilterParametersAsync(routeInfo.ExtendedFilterDefinitions, extendedEthosVersion, true);

                var ethosApiBuilderReturn = await _ethosApiBuilderService.PostEthosApiBuilderAsync(ethosApiBuilder, routeInfo, filterDictionary);

                AddEthosContextProperties(dpList,
                    await _ethosApiBuilderService.GetExtendedEthosDataByResource(routeInfo, new List<string>() { ethosApiBuilderReturn._Id }));

                // For Ethos Subroutine APIs, null out the ID property, which contains the Subroutine Name.
                if (!string.IsNullOrEmpty(configuration.ApiType) && configuration.ApiType.Equals("S", StringComparison.OrdinalIgnoreCase) ||
                    (string.IsNullOrEmpty(configuration.PrimaryGuidSource) && configuration.PrimaryEntity == ethosApiBuilderReturn._Id))
                {
                    ethosApiBuilderReturn._Id = null;
                }

                // Set Content Restricted Header if required.
                var secureDataDefinition = _ethosApiBuilderService.GetSecureDataDefinition();
                if (secureDataDefinition != null && secureDataDefinition.Item1)
                {
                    SetContentRestrictedHeader("partial");
                }

                return Ok(ethosApiBuilderReturn);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                var exception = new IntegrationApiException();
                exception.AddError(new IntegrationApiError("GUID.Not.Found", "An error occurred translating the GUID to an ID.", e.Message, HttpStatusCode.NotFound));
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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

        #region PUT
        /// <summary>
        /// Updates a person visa.
        /// </summary>
        /// <param name="routeInfo">Data about the route</param>
        /// <param name="configuration"></param>
        /// <param name="extendedEthosVersion"></param>
        /// <param name="id">id of the ethosApiBuilder to update</param>
        /// <param name="ethosApiBuilder"><see cref="Dtos.EthosApiBuilder">ethosApiBuilder</see> to create</param>
        /// <returns>Updated <see cref="Dtos.EthosApiBuilder">Dtos.EthosApiBuilder</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        private async Task<IActionResult> PutEthosApiBuilderAsync(EthosResourceRouteInfo routeInfo, EthosApiConfiguration configuration, EthosExtensibleData extendedEthosVersion, string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.EthosApiBuilder ethosApiBuilder)
        {
            bool bypassCache = routeInfo.BypassCache;

            try
            {
                var recordKey = id;

                if (string.IsNullOrEmpty(recordKey))
                {
                    return CreateHttpResponseException(new IntegrationApiException(string.Format("Must provide an id for '{0}'.", routeInfo.ResourceName)));
                }

                if (!string.IsNullOrWhiteSpace(recordKey) && recordKey.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return CreateHttpResponseException(new IntegrationApiException(string.Format("Nil GUID must not be used in {0} PUT operation.", routeInfo.ResourceName)));
                }

                if (ethosApiBuilder == null)
                {
                    return CreateHttpResponseException(new IntegrationApiException(string.Format("Must provide a request body for '{0}'.", routeInfo.ResourceName)));
                }

                if (string.IsNullOrEmpty(ethosApiBuilder._Id))
                {
                    return CreateHttpResponseException(new IntegrationApiException(string.Format("Must provide an id for '{0}' in request body.", routeInfo.ResourceName)));
                }

                if (!string.IsNullOrWhiteSpace(ethosApiBuilder._Id) && ethosApiBuilder._Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return CreateHttpResponseException(new IntegrationApiException(string.Format("Nil GUID must not be used in {0} PUT operation.", routeInfo.ResourceName)));
                }

                if (configuration.ColleagueKeyNames != null && configuration.ColleagueKeyNames.Count() > 1)
                {
                    CompareKeyValues(id, ethosApiBuilder._Id);
                }
                else
                {
                    if (!string.IsNullOrEmpty(id) && !id.Equals(ethosApiBuilder._Id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new InvalidOperationException("id in URL is not the same as in request body.")));
                    }
                }

                Validate(ethosApiBuilder);

                //get Data Privacy List
                var dpList = await _ethosApiBuilderService.GetDataPrivacyListByApi(routeInfo, true);

                //call import extend method that needs the extracted extension data and the config
                await _ethosApiBuilderService.ImportExtendedEthosData(await ExtractExtendedData(await _ethosApiBuilderService.GetExtendedEthosConfigurationByResource(routeInfo), _logger));

                Dtos.EthosApiBuilder ethosApiBuilderReturn = null;
                bool performPostInstead = false;

                var filterDictionary = await ValidateFilterParametersAsync(routeInfo.ExtendedFilterDefinitions, extendedEthosVersion, true);

                try
                {
                    //ethosApiBuilderReturn = await _ethosApiBuilderService.PutEthosApiBuilderAsync(recordKey,
                    //    await PerformPartialPayloadMerge(ethosApiBuilder, await GetEthosApiBuilderByIdAsync(routeInfo, configuration, extendedEthosVersion, recordKey),
                    //    dpList, _logger), configuration.ResourceName);

                    ethosApiBuilderReturn = await _ethosApiBuilderService.PutEthosApiBuilderAsync(recordKey, ethosApiBuilder, routeInfo, filterDictionary);

                    if (string.IsNullOrEmpty(ethosApiBuilder._Id))
                    {
                        ethosApiBuilderReturn._Id = recordKey;
                    }
                }
                catch (IntegrationApiException ex)
                {
                    if (ex.Errors != null && ex.Errors.FirstOrDefault() != null && ex.Errors.FirstOrDefault().Code == "GUID.Not.Found")
                    {
                        performPostInstead = true;
                    }
                    else
                    {
                        return CreateHttpResponseException(ex);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (performPostInstead)
                {
                    ethosApiBuilderReturn = await _ethosApiBuilderService.PostEthosApiBuilderAsync(ethosApiBuilder, routeInfo, filterDictionary);
                }
                AddEthosContextProperties(dpList,
                    await _ethosApiBuilderService.GetExtendedEthosDataByResource(routeInfo, new List<string>() { ethosApiBuilderReturn._Id }));

                // Set Content Restricted Header if required.
                var secureDataDefinition = _ethosApiBuilderService.GetSecureDataDefinition();
                if (secureDataDefinition != null && secureDataDefinition.Item1)
                {
                    SetContentRestrictedHeader("partial");
                }

                return Ok(ethosApiBuilderReturn);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                var exception = new IntegrationApiException();
                if (!string.IsNullOrEmpty(configuration.PrimaryKeyName))
                {
                    exception.AddError(new IntegrationApiError("Key.Not.Found", "An error occurred translating the key to an ID.", e.Message, HttpStatusCode.NotFound, id));
                }
                else
                {
                    exception.AddError(new IntegrationApiError("GUID.Not.Found", "An error occurred translating the GUID to an ID.", e.Message, HttpStatusCode.NotFound, id));
                }
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }

            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                var apiException = IntegrationApiUtility.ConvertToIntegrationApiException(e);
                return CreateHttpResponseException(apiException);
            }
        }
        #endregion

        #region DELETE
        /// <summary>
        /// Delete (DELETE) a record from an extended data set
        /// </summary>
        /// <param name="routeInfo">Data about the route</param>
        /// <param name="configuration"></param>
        /// <param name="extendedEthosVersion"></param>
        /// <param name="id">id of the record to delete</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        private async Task<ActionResult> DeleteEthosApiBuilderAsync(EthosResourceRouteInfo routeInfo, EthosApiConfiguration configuration, EthosExtensibleData extendedEthosVersion, string id)
        {
            bool bypassCache = routeInfo.BypassCache;

            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException(string.Format("{0} id cannot be null or empty", routeInfo.ResourceName));
                }
                await _ethosApiBuilderService.DeleteEthosApiBuilderAsync(id, configuration.ResourceName);
                return NoContent();
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                var exception = new IntegrationApiException();
                exception.AddError(new IntegrationApiError("GUID.Not.Found", "An error occurred translating the GUID to an ID.", e.Message, HttpStatusCode.NotFound, id));
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
            }
            catch (RepositoryException e)
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

        #region routing

        /// <summary>
        /// Respond to route and execute the appropriate method.  This will only be hit if there are routes defined on the resource, therefore always return a 406 if an error occurs.
        /// </summary>       
        /// <param name="id"></param>
        /// <param name="resource"></param>
        /// <param name="page"></param>
        /// <param name="ethosApiBuilder"></param>
        /// <returns></returns>
        [HttpGet, HttpPost, HttpPut, HttpDelete]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ExtendedRoute("/{resource}/{id?}", Order = -1)]
        [ExtendedRoute("/qapi/{resource}", Order = -1)]
        [Route("/{resource}/{id?}", Order = 10)]
        
    public async Task<IActionResult> GetAlternativeRouteOrNotAcceptable([ModelBinder(typeof(EedmModelBinder))] Dtos.EthosApiBuilder ethosApiBuilder, [FromRoute] string resource = null, [FromRoute] string id = null, Paging page = null)
        {
            bool bypassCache = false;
            bool methodSupported = false;
            EthosResourceRouteInfo routeInfo = new EthosResourceRouteInfo();
            EthosApiConfiguration configuration = new EthosApiConfiguration();
            EthosExtensibleData extendedEthosVersion = new EthosExtensibleData();
            Dictionary<string, string> keyDictionary = new Dictionary<string, string>();

            try
            {
                var actionRequestContext = _actionContextAccessor.ActionContext.HttpContext.Request;

                if (actionRequestContext == null)
                {
                    return NotAcceptableStatusException();
                }

                var routeData = _actionContextAccessor.ActionContext.HttpContext.GetEndpoint() as RouteEndpoint;

                if (routeData == null)
                {
                    return NotAcceptableStatusException();
                }

                if (string.IsNullOrEmpty(resource))
                {
                    resource = routeData.RoutePattern.RawText ?? "";
                    if (resource.LastIndexOf("/") > 0)
                    {
                        resource = resource.Remove(resource.LastIndexOf("/"));
                    }

                    if (resource.StartsWith("{"))
                    {
                        resource = resource.Remove(1, 1);
                    }
                    if (resource.EndsWith("}"))
                    {
                        resource = resource.Remove(resource.Length - 1, 1);
                    }
                }
                bool getByIdFilter = false;
                var queryString = Request.Query;
                foreach (var queryName in queryString)
                {
                    if (queryName.Key.Equals("id") || queryName.Equals("key"))
                    {
                        // We will get the ID from the Query instead of directly from the ID on URL.
                        // The route returns id with a value of the query object.  We need to parse
                        // out the query object, which is done elsewhere.
                        id = string.Empty;
                        getByIdFilter = true;
                    }
                }

                routeInfo = GetEthosResourceRouteInfo(resource, true);
                if (routeInfo == null)
                {
                    return NotAcceptableStatusException();
                }

                bool defaultVersionUsed = false;
                if (string.IsNullOrEmpty(routeInfo.ResourceVersionNumber) || routeInfo.ResourceVersionNumber.Length > routeInfo.RequestedVersionNumber.Length)
                {
                    if (string.IsNullOrEmpty(routeInfo.ResourceVersionNumber))
                    {
                        if (routeInfo.RequestMethod.ToString() == "PUT" || routeInfo.RequestMethod.ToString() == "POST")
                        {
                            return MethodNotAllowedException();
                        }
                    }
                    var resourceName = routeInfo.ResourceName;
                    if (!string.IsNullOrEmpty(routeInfo.ParentName)) resourceName = routeInfo.ExtendedSchemaResourceId;
                    routeInfo.ResourceVersionNumber = await _ethosApiBuilderService.GetEthosExtensibilityResourceDefaultVersion(resourceName, routeInfo.BypassCache, routeInfo.RequestedVersionNumber);
                    defaultVersionUsed = true;
                }

                var routeTemplate = routeData.RoutePattern.RawText;
                if (routeInfo.isQueryByPost)
                {
                    if (string.IsNullOrEmpty(id) && !routeInfo.ExtendedFilterDefinitions.Any())
                    {
                        var bodyString = await (new StreamReader(Request.Body).ReadToEndAsync());
                        if (!string.IsNullOrEmpty(bodyString))
                        {
                            routeInfo.ExtendedFilterDefinitions = ParseJsonStringValues(bodyString, "", routeInfo.ExtendedFilterDefinitions);
                            routeInfo.RequestMethod = new HttpMethod("POST_QAPI");
                        }
                    }
                    else
                    {
                        return NotAcceptableStatusException();
                    }
                }

                configuration = await GetApiConfiguration(routeInfo, bypassCache);
                if (configuration == null || string.IsNullOrEmpty(configuration.ResourceName))
                {
                    return NotAcceptableStatusException();
                }
                if (!string.IsNullOrEmpty(configuration.ParentResourceName) && !configuration.ParentResourceName.Equals(routeInfo.ResourceName, StringComparison.OrdinalIgnoreCase))
                {
                    return NotAcceptableStatusException();
                }

                extendedEthosVersion = await _ethosApiBuilderService.GetExtendedEthosConfigurationByResource(routeInfo, bypassCache);
                if (extendedEthosVersion == null)
                {
                    return NotAcceptableStatusException();
                }

                //  If this defined as an Ethos Extension on an Ellucian delivered API, throw 406
                if (configuration.HttpMethods == null || !configuration.HttpMethods.Any())
                {
                    return NotAcceptableStatusException();
                }

                // If the configuration status is "beta" then disallow access unless they include "beta" in the version request.
                var requestedStatus = string.IsNullOrEmpty(routeInfo.ResourceVersionStatus) ? string.Empty : routeInfo.ResourceVersionStatus;
                var versionStatus = string.IsNullOrEmpty(extendedEthosVersion.VersionReleaseStatus) ? string.Empty : extendedEthosVersion.VersionReleaseStatus;

                // Allow for return of a "beta" status when no Accept Header specified a version (default version selected)
                if ((defaultVersionUsed || routeInfo.RequestedVersionNumber != routeInfo.ResourceVersionNumber) && versionStatus.Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    requestedStatus = "beta";
                }

                // Validate that the requested status and API statuses are correct
                if (versionStatus.Equals("b", StringComparison.OrdinalIgnoreCase) && !requestedStatus.Equals("beta", StringComparison.OrdinalIgnoreCase))
                {
                    return NotAcceptableStatusException();
                }
                else if (requestedStatus.Equals("beta", StringComparison.OrdinalIgnoreCase) && !versionStatus.Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    return NotAcceptableStatusException();
                }
                // If we've made a request for a specific release status then make sure they match
                else if (!requestedStatus.Equals("beta", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(requestedStatus) && !requestedStatus.StartsWith(versionStatus, StringComparison.OrdinalIgnoreCase))
                {
                    return NotAcceptableStatusException();
                }
                else if (!string.IsNullOrEmpty(requestedStatus) && string.IsNullOrEmpty(versionStatus))
                {
                    return NotAcceptableStatusException();
                }
                routeInfo.ResourceVersionStatus = requestedStatus;

                // Only support "beta" on X-Media-Type header which is set later using routeInfo.ResourceVersionStatus
                if (!requestedStatus.Equals("beta", StringComparison.OrdinalIgnoreCase) || !versionStatus.Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    routeInfo.ResourceVersionStatus = string.Empty;
                }

                // If we are dealing with a Business Process API from a UI form, then we can do
                // either PUT with, or without a key on the URL.  The key will instead be
                // found in the body and validated in the Put or Post method.
                if (!string.IsNullOrEmpty(configuration.ApiType) && configuration.ApiType.Equals("T", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(id) && !routeInfo.ExtendedFilterDefinitions.Any() && !routeInfo.isQueryByPost && routeInfo.RequestMethod.Method == "PUT")
                    {
                        var bodyString = await (new StreamReader(Request.Body).ReadToEndAsync());
                        if (!string.IsNullOrEmpty(bodyString))
                        {
                            routeInfo.ExtendedFilterDefinitions = ParseJsonStringValues(bodyString, "", routeInfo.ExtendedFilterDefinitions);
                        }
                    }
                }
                else
                {
                    if (routeInfo.RequestMethod.Method == "PUT" && string.IsNullOrEmpty(id) && !routeInfo.ExtendedFilterDefinitions.Any())
                    {
                        return NotAcceptableStatusException();
                    }
                    else
                    {
                        if (routeInfo.RequestMethod.Method == "POST" && (!string.IsNullOrEmpty(id) || routeInfo.ExtendedFilterDefinitions.Any()))
                        {
                            return NotAcceptableStatusException();
                        }
                    }
                }
                if (routeInfo.RequestMethod.Method == "PUT" || routeInfo.RequestMethod.Method == "POST")
                {
                    if (ethosApiBuilder != null && string.IsNullOrEmpty(ethosApiBuilder._Id) && Request.Body != null)
                    {
                        var bodyString = await (new StreamReader(Request.Body).ReadToEndAsync());
                        var extendedFilterDefinitions = routeInfo.ExtendedFilterDefinitions;
                        routeInfo.ExtendedFilterDefinitions = new Dictionary<string, Tuple<List<string>, string>>();
                        routeInfo.ExtendedFilterDefinitions = ParseJsonStringValues(bodyString, "", routeInfo.ExtendedFilterDefinitions);
                        var extendedKeyDictionaries = await ValidateIdsFromQueryAsync(routeInfo, extendedEthosVersion, true);
                        routeInfo.ExtendedFilterDefinitions = extendedFilterDefinitions;
                        ethosApiBuilder._Id = await GetIdPropertyFromBody(extendedKeyDictionaries, extendedEthosVersion, configuration.ApiType, configuration.PrimaryEntity, configuration.PrimaryGuidSource);

                    }
                }

                // If we don't have IDs and are requesting a GET then this is a GET all records.
                if (string.IsNullOrEmpty(id) && routeInfo.RequestMethod.Method == "GET" && !getByIdFilter)
                {
                    routeInfo.RequestMethod = new HttpMethod("GET_ALL");
                }
                else
                {
                    if (!string.IsNullOrEmpty(id) && routeInfo.RequestMethod.Method == "GET" && getByIdFilter)
                    {
                        routeInfo.RequestMethod = new HttpMethod("GET_ID");
                    }
                }

                if (!string.IsNullOrEmpty(configuration.PrimaryKeyName) && !string.IsNullOrEmpty(id) && !(_ethosApiBuilderService.UnEncodePrimaryKey(id).Contains("+")))
                {
                    id = _ethosApiBuilderService.EncodePrimaryKey(string.Concat(configuration.PrimaryEntity, "+", id));
                }

                foreach (var meth in configuration.HttpMethods)
                {
                    var supportedMethod = meth.Method;
                    switch (routeInfo.RequestMethod.Method)
                    {
                        case "GET":
                            {
                                if (supportedMethod == "GET" || supportedMethod == "GET_ID" || supportedMethod == "GET_ALL" || supportedMethod == "POST_QAPI")
                                {
                                    methodSupported = true;
                                }
                                break;
                            }
                        case "GET_ID":
                            {
                                if (supportedMethod == "GET" || supportedMethod == "GET_ID")
                                {
                                    methodSupported = true;
                                }
                                break;
                            }
                        case "GET_ALL":
                            {
                                if (supportedMethod == "GET" || supportedMethod == "GET_ALL")
                                {
                                    methodSupported = true;
                                }
                                break;
                            }
                        case "POST_QAPI":
                            {
                                if (supportedMethod == "GET" || supportedMethod == "POST_QAPI")
                                {
                                    methodSupported = true;
                                }
                                break;
                            }
                        default:
                            {
                                if (meth.Method == routeInfo.RequestMethod.Method)
                                {
                                    methodSupported = true;
                                }
                                break;
                            }
                    }
                }
            }
            catch (JsonException ex)
            {
                var message = ex.Message;
                var exception = new IntegrationApiException();
                exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
            }
            catch (IntegrationApiException ex)
            {
                return CreateHttpResponseException(ex);
            }
            catch (Exception)
            {
                throw;
            }

            if (!methodSupported)
            {
                return MethodNotAllowedException();
            }

            // Reset BypassCache flag since we have already re-read any configuration settings using the existing
            // bypassCache flag and no longer need to get the data from disk.
            routeInfo.BypassCache = false;

            return await GetEthosApiBuilderAsync(routeInfo, configuration, extendedEthosVersion, id, page, ethosApiBuilder);
        }
        #endregion

        #region Validate
        /// <summary>
        /// Check for all required fields
        /// </summary>
        /// <param name="ethosApiBuilder">ethosApiBuilder</param>
        private static void Validate(Dtos.EthosApiBuilder ethosApiBuilder)
        {
            if (ethosApiBuilder != null && string.IsNullOrEmpty(ethosApiBuilder._Id))
            {
                throw new ArgumentNullException("Must provide an id for extended data resource.");
            }
        }

        /// <summary>
        /// Gets Ethos Resource information from route
        /// </summary>
        /// <param name="resource">Name of the API or resource name</param>
        /// <param name="alternativeRepresenation">Boolean to use alternative representation from route.</param>
        /// <returns>EthosResourceRouteInfo</returns>
        private EthosResourceRouteInfo GetEthosResourceRouteInfo(string resource, bool alternativeRepresenation = false)
        {
            var bypassCache = false;
            if (Request != null && Request.Headers != null && Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var ethosRouteInfo = new EthosResourceRouteInfo();
            ethosRouteInfo.ReportEthosExtendedErrors = true;
            ethosRouteInfo.BypassCache = bypassCache;

            var routeTemplate = _actionContextAccessor.ActionContext.HttpContext.GetRouteTemplate() ?? "";
            if (routeTemplate.StartsWith("qapi")) ethosRouteInfo.isQueryByPost = true;

            if (Request == null || Request.Headers == null || Request.Headers.Accept == StringValues.Empty)
            {
                return ethosRouteInfo;
            }

            var contentType = Request.Headers.Accept.ToString();
            string versionStatus = string.Empty;

            if (contentType == null)
            {
                return ethosRouteInfo;
            }

            if (contentType.Contains("+"))
            {
                contentType = contentType.Split('+')[0].ToString();
            }
            ethosRouteInfo.EthosResourceIdentifier = contentType;

            ethosRouteInfo.RequestMethod = new HttpMethod(Request.Method);

            ethosRouteInfo.ResourceName = resource;
            if (!string.IsNullOrEmpty(contentType))
            {
                string hedtechIntegrationMediaTypePrefix = "application/vnd.hedtech.integration.";
                var alternativeRepresenationName = contentType.Replace(hedtechIntegrationMediaTypePrefix, "");

                if (alternativeRepresenationName.LastIndexOf(".v") > 0)
                {
                    alternativeRepresenationName = alternativeRepresenationName.Remove(alternativeRepresenationName.LastIndexOf(".v"));
                    if (!string.IsNullOrEmpty(alternativeRepresenationName))
                    {
                        ethosRouteInfo.ExtendedSchemaResourceId = alternativeRepresenationName;
                        ethosRouteInfo.ParentName = resource;
                    }
                }
                else if (contentType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "*/*";
                }

                if (contentType.LastIndexOf(".v") > 0)
                {
                    // remove the period and the leading 'v'
                    var splitContent = contentType.Remove(0, contentType.LastIndexOf(".v") + 2).Split('-');
                    if (splitContent.Count() == 2)
                    {
                        contentType = splitContent[0];
                        versionStatus = splitContent[1];
                    }
                    // if (string.IsNullOrEmpty(versionStatus)) return null;
                }
                ethosRouteInfo.ResourceVersionNumber = ExtractVersionNumberOnly(contentType);
                ethosRouteInfo.RequestedVersionNumber = ethosRouteInfo.ResourceVersionNumber;
                ethosRouteInfo.ResourceVersionStatus = versionStatus;
            }

            if (!string.IsNullOrEmpty(ethosRouteInfo.ResourceVersionNumber))
            {
                var versionNumber = ethosRouteInfo.ResourceVersionNumber.Split('.');
                int versionCount = versionNumber.Count();
                string version = versionNumber[0];
                if (versionCount == 1) version += ".0.0";
                if (versionCount == 2) version += "." + versionNumber[1] + ".0";
                if (versionCount == 3) version += "." + versionNumber[1] + "." + versionNumber[2];
                ethosRouteInfo.ResourceVersionNumber = version;
            }

            if (ethosRouteInfo.isQueryByPost && ethosRouteInfo.RequestMethod.Method != "POST")
            {
                throw new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError);
            }

            if (ethosRouteInfo == null || string.IsNullOrEmpty(ethosRouteInfo.ResourceName))
            {
                throw new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError);
            }


            // Update Extended Filter Definitions
            ethosRouteInfo.ExtendedFilterDefinitions = GetEthosExtendedFilters();

            // Update global bypassCache flag for configuration cache
            ethosRouteInfo.BypassCache = bypassCache;

            // Get the Restricted Fields accept header
            if (Request.Headers.TryGetValue("Accept-Restricted-Fields", out StringValues returnRestrictedContent))
            {
                if (returnRestrictedContent.Contains("*"))
                {
                    ethosRouteInfo.ReturnRestrictedFields = true;
                }
            }

            return ethosRouteInfo;
        }

        /// <summary>
        /// Extract a version number from the routing request Accept header.
        /// </summary>
        /// <param name="original">Original Accept Header submitted with request.</param>
        /// <returns>Version number from the request Accept Header.</returns>
        private string ExtractVersionNumberOnly(string original)
        {
            if (original.LastIndexOf(".v") > 0)
            {
                // remove the period and the leading 'v'
                original = original.Remove(0, original.LastIndexOf(".v") + 2);
            }


            if (Regex.Matches(original, @"[a-zA-Z]", RegexOptions.Compiled).Count > 0)
            {
                NotAcceptableStatusException(shouldThrowException: true);
            }

            var regex = new Regex(@"(?:(\d+)\.)?(?:(\d+)\.)?(?:(\d+)\.\d+)|(?:(\d+))", RegexOptions.Compiled);
            Match semanticVersion = regex.Match(original);
            if (semanticVersion.Success)
            {
                return semanticVersion.Value;
            }
            else return string.Empty;
        }

        /// <summary>
        /// Get the API configuration for an API
        /// </summary>
        /// <param name="routeInfo">Object containing information about a route.</param>
        /// <param name="bypassCache">Boolean set to true to retrieve data fresh from Colleague, set to false to use cache.</param>
        /// <returns></returns>
        private async Task<EthosApiConfiguration> GetApiConfiguration(EthosResourceRouteInfo routeInfo, bool bypassCache)
        {
            if (routeInfo == null)
            {
                throw new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError);
            }

            var configuration = await _ethosApiBuilderService.GetEthosApiConfigurationByResource(routeInfo, bypassCache);
            if (configuration == null || string.IsNullOrEmpty(configuration.ResourceName))
            {
                throw new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError);
            }

            // for BPAs, make sure the resource name matches and there are http methods
            if (configuration.ApiType != null && configuration.ApiType.Equals("T", StringComparison.OrdinalIgnoreCase))
            {
                if ((routeInfo.ResourceName == null)
                    || (!configuration.ResourceName.Equals(routeInfo.ResourceName, StringComparison.OrdinalIgnoreCase))
                    || (configuration.HttpMethods == null
                    || !configuration.HttpMethods.Any()))
                {
                    NotAcceptableStatusException(shouldThrowException: true);
                }
            }
            else
            {
                //  If an spec is expected to have a parent, but no accept header is provided,
                //  OR if a spec doesn't have a parent, but one is provided in the accept header, 
                //  OR if a spec has a parent, but it's different from what is provided, then throw a 406
                //  OR is an Ethos Extension on an ellucian delivered API
                if ((!string.IsNullOrEmpty(configuration.ParentResourceName) && string.IsNullOrEmpty(routeInfo.ParentName))
                || (string.IsNullOrEmpty(configuration.ParentResourceName) && !string.IsNullOrEmpty(routeInfo.ParentName))
                || (!string.IsNullOrEmpty(configuration.ParentResourceName) && !configuration.ParentResourceName.Equals(routeInfo.ParentName, StringComparison.OrdinalIgnoreCase))
                || (configuration.HttpMethods == null || !configuration.HttpMethods.Any()))
                {
                    NotAcceptableStatusException(shouldThrowException: true);
                }
            }

            return configuration;
        }

        /// <summary>
        /// Configuration the Paging parameters for an API
        /// </summary>
        /// <param name="page"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private Paging ConfigurePage(Paging page, EthosApiConfiguration configuration)
        {
            if (configuration.PageLimit == null || !configuration.PageLimit.HasValue || configuration.PageLimit.Value == 0)
            {
                if (page == null || (page.Offset < 0 || page.Limit <= 0))
                {
                    page = new Paging(0, 0)
                    {
                        DefaultLimit = null
                    };
                }
            }
            else
            {
                if (page == null)
                {
                    if (configuration.PageLimit != null && configuration.PageLimit.HasValue)
                    {
                        page = new Paging(configuration.PageLimit.Value, 0);
                    }
                    else
                    {
                        page = new Paging(100, 0);
                    }
                }
                else
                {
                    if (page.Limit >= configuration.PageLimit)
                    {
                        page = new Paging(configuration.PageLimit.Value, page.Offset);
                    }
                    page.DefaultLimit = configuration.PageLimit;
                }
            }
            return page;
        }

        /// <summary>
        /// Extract the query string from the URL as a filter object
        /// </summary>
        /// <returns>>Tuple containing path, oper, and values.</returns>
        private Dictionary<string, Tuple<List<string>, string>> GetEthosExtendedFilters()
        {
            Dictionary<string, Tuple<List<string>, string>> extendedFilterDefinitions = new Dictionary<string, Tuple<List<string>, string>>();


            var queryString = Request.Query;
            foreach (var query in queryString.Keys)
            {
                if (query == null)
                {
                    var message = string.Concat("'", queryString, "' is an invalid query string parameter for filtering. ");
                    throw new JsonException(message);
                }
                var queryName = query.ToString();
                if (queryName.Equals("offset", StringComparison.OrdinalIgnoreCase) || queryName.Equals("limit", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var value = queryString[queryName];

                try
                {
                    extendedFilterDefinitions = ParseJsonStringValues(value, queryName, extendedFilterDefinitions);
                    if (extendedFilterDefinitions.Count() == 0)
                    {
                        var message = string.Concat("'", queryName, "=", value, "' is an invalid query parameter for filtering. ");
                        throw new JsonException(message);
                    }
                }
                catch (JsonException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    var message = string.Concat("'", queryName, "=", value, "' is an invalid query parameter for filtering. ", ex.Message);
                    throw new JsonException(message);
                }
            }
            return extendedFilterDefinitions;
        }

        /// <summary>
        /// Validate query string on filters to include filters in GET all by filters.
        /// Also include key query filters if requested for PUT or POST transactions so
        /// that they can assemble a valid key for the subsequent GET by ID operation.
        /// </summary>
        /// <param name="filterDictionary">Dictionary of valid filters from configuration</param>
        /// <param name="extendedEthosVersion">Version Configuration data</param>
        /// <param name="includeKeys">Include Key Query filters for PUT and POST only.</param>
        private async Task<Dictionary<string, EthosExtensibleDataFilter>> ValidateFilterParametersAsync(Dictionary<string, Tuple<List<string>, string>> filterDictionary, EthosExtensibleData extendedEthosVersion, bool includeKeys = false)
        {
            // Validate the query names
            var queryString = Request.Query;
            List<string> queryNames = new List<string>();
            foreach (var query in queryString.Keys)
            {
                if (!query.ToString().Equals("offset", StringComparison.OrdinalIgnoreCase) && !query.ToString().Equals("limit", StringComparison.OrdinalIgnoreCase) && !query.ToString().Equals("key", StringComparison.OrdinalIgnoreCase) && !query.ToString().Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    var matchingQueryName = extendedEthosVersion.ExtendedDataFilterList.FirstOrDefault(ed => ed.QueryName == query.ToString());
                    if (matchingQueryName == null)
                    {
                        var message = string.Concat("'", query, "' is an invalid query parameter for filtering.");
                        var exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                        throw IntegrationApiUtility.ConvertToIntegrationApiException(exception);
                    }
                    queryNames.Add(query.ToString());
                }
            }

            // Validate the Query
            var returnDictionary = new Dictionary<string, EthosExtensibleDataFilter>();
            foreach (var filter in filterDictionary)
            {
                var filterName = filter.Key;
                var filterValues = filter.Value.Item1;
                var filterOper = filter.Value.Item2;

                var matchingProperty = extendedEthosVersion.ExtendedDataFilterList.FirstOrDefault(ed => ed.FullJsonPath == filterName.TrimEnd(']').TrimEnd('['));
                if (matchingProperty == null)
                {
                    var message = string.Concat("'", filterName, "' is an invalid query parameter for filtering.");
                    var exception = new IntegrationApiException();
                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                    throw exception;
                }
                else
                {
                    if (!queryNames.Contains(matchingProperty.QueryName) && queryNames.Any() && !matchingProperty.KeyQuery)
                    {
                        var message = string.Concat("'", filterName, "' is an invalid query parameter for filtering. Expecting '", matchingProperty.QueryName, "='");
                        var exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                        throw exception;
                    }

                    // Validate Filter Operators
                    bool validFilterOper = true;
                    if (matchingProperty.ValidFilterOpers != null && matchingProperty.ValidFilterOpers.Any())
                    {
                        if (!string.IsNullOrEmpty(filterOper) && !matchingProperty.ValidFilterOpers.Contains(filterOper))
                        {
                            validFilterOper = false;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filterOper))
                        {
                            switch (filterOper)
                            {
                                case "$lte":
                                    break;
                                case "$gte":
                                    break;
                                case "$lt":
                                    break;
                                case "$gt":
                                    break;
                                case "$ne":
                                    break;
                                case "$eq":
                                    break;
                                default:
                                    validFilterOper = false;
                                    break;
                            }
                        }
                    }
                    if (!validFilterOper)
                    {
                        var message = string.Concat("'", filterOper, "' is an invalid query parameter for filtering. ");
                        var exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                        throw exception;
                    }

                    List<string> newValues = new List<string>();
                    foreach (var value in filterValues)
                    {
                        var newValue = value;
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (matchingProperty.JsonPropertyType == JsonPropertyTypeExtensions.Date)
                            {
                                try
                                {
                                    DateTime temp = Convert.ToDateTime(newValue);
                                    newValue = string.Concat(temp.Month.ToString(), "/", temp.Day.ToString(), "/", temp.Year.ToString());
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid Date. ", ex.Message);
                                    var exception = new IntegrationApiException();
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                    throw exception;
                                }
                            }
                            if (matchingProperty.JsonPropertyType == JsonPropertyTypeExtensions.DateTime)
                            {
                                try
                                {
                                    DateTime temp = Convert.ToDateTime(newValue);
                                    newValue = string.Concat(temp.Month.ToString(), "/", temp.Day.ToString(), "/", temp.Year.ToString());
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid DateTime. ", ex.Message);
                                    var exception = new IntegrationApiException();
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                    throw exception;
                                }
                            }
                            if (matchingProperty.JsonPropertyType == JsonPropertyTypeExtensions.Time)
                            {
                                try
                                {
                                    newValue = DateTime.Today.ToString() + "T" + newValue;
                                    DateTime temp = Convert.ToDateTime(newValue);
                                    newValue = string.Concat(temp.Hour.ToString(), ":", temp.Minute.ToString(), ":", temp.Second.ToString());
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid DateTime. ", ex.Message);
                                    var exception = new IntegrationApiException();
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                    throw exception;
                                }
                            }
                            if (matchingProperty.JsonPropertyType == JsonPropertyTypeExtensions.Number)
                            {
                                try
                                {
                                    decimal temp = Convert.ToDecimal(newValue);
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid Decimal Number. ", ex.Message);
                                    var exception = new IntegrationApiException();
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                    throw exception;
                                }
                            }
                            if (!string.IsNullOrEmpty(matchingProperty.GuidColumnName) && !string.IsNullOrEmpty(matchingProperty.GuidFileName))
                            {
                                newValue = await _ethosApiBuilderService.GetRecordIdFromGuidAsync(newValue, matchingProperty.GuidFileName, matchingProperty.GuidColumnName);
                                if (string.IsNullOrEmpty(newValue)) newValue = value;
                            }
                            // Business Process API may have a translation for documentation purposes only but should not translate
                            if (!string.IsNullOrEmpty(matchingProperty.TransColumnName) && extendedEthosVersion.ApiType != "T")
                            {
                                newValue = await _ethosApiBuilderService.GetRecordIdFromTranslationAsync(newValue, matchingProperty.TransFileName, matchingProperty.TransColumnName, matchingProperty.TransTableName);
                                if (string.IsNullOrEmpty(newValue)) newValue = value;
                            }
                            if (matchingProperty.Enumerations != null && matchingProperty.Enumerations.Any() && extendedEthosVersion.ApiType != "T")
                            {
                                var matchingEnumeration = matchingProperty.Enumerations.FirstOrDefault(en => en.EnumerationValue.Equals(newValue, StringComparison.OrdinalIgnoreCase));
                                if (matchingEnumeration == null)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid enumeration value. ");
                                    var exception = new IntegrationApiException();
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                    throw exception;
                                }
                                else
                                {
                                    newValue = matchingEnumeration.ColleagueValue;
                                }
                            }
                            newValues.Add(newValue);
                        }
                    }
                    if (!returnDictionary.ContainsKey(filterName))
                    {
                        matchingProperty.FilterValue = newValues;
                        matchingProperty.FilterOper = filterOper;
                        returnDictionary.Add(filterName, matchingProperty);
                    }
                }
            }
            // Return all Key filters Items for key construction in Repository
            if (includeKeys && extendedEthosVersion.ExtendedDataFilterList != null && extendedEthosVersion.ExtendedDataFilterList.Any())
            {
                var keyFilters = extendedEthosVersion.ExtendedDataFilterList.Where(kf => kf.KeyQuery);
                foreach (var filter in keyFilters)
                {
                    var filterName = filter.FullJsonPath;
                    if (!returnDictionary.ContainsKey(filterName))
                    {
                        returnDictionary.Add(filterName, filter);
                    }
                }
            }
            return returnDictionary;
        }

        /// <summary>
        /// Compare two key values whereby each key component may be in a different order than the other
        /// but the two keys are still considered equal as long as each component in one exists in the other.
        /// </summary>
        /// <param name="id1">First key to compare</param>
        /// <param name="id2">Second key to compare against first key</param>
        private void CompareKeyValues(string id1, string id2)
        {
            if (string.IsNullOrEmpty(id1) || string.IsNullOrEmpty(id2))
            {
                throw new InvalidOperationException("id in URL is not the same as in request body.");
            }
            var unencodedId1 = _ethosApiBuilderService.UnEncodePrimaryKey(id1);
            var unencodedId2 = _ethosApiBuilderService.UnEncodePrimaryKey(id2);
            var key1Values = unencodedId1.Split(DmiString._XM);
            var key2Values = unencodedId2.Split(DmiString._XM);

            foreach (var key1 in key1Values)
            {
                if (!key2Values.Contains(key1))
                {
                    throw new InvalidOperationException("id in URL is not the same as in request body.");
                }
            }
        }

        /// <summary>
        /// Using the filters defined for key component queries, build a record key string.
        /// </summary>
        /// <param name="extendedKeyDictionaries">Key Component Dictionary</param>
        /// <param name="extendedEthosVersion">Version specific configuration</param>
        /// <param name="apiType">Api Type of A (Spec based API), E or "" (Extensions to API), or T (Business Process API)</param>
        /// <returns></returns>
        private string BuildRecordKeyFromFilterData(Tuple<Dictionary<string, string>, Dictionary<string, string>> extendedKeyDictionaries, EthosExtensibleData extendedEthosVersion, string apiType)
        {
            string id = string.Empty;
            var extendedFileDefinitions = extendedKeyDictionaries.Item1;
            var extendedKeyDefinitions = extendedKeyDictionaries.Item2;
            if (extendedKeyDefinitions != null && extendedKeyDefinitions.Any())
            {
                var recordKey = string.Empty;
                // Add key parts that are base on a specific table.
                foreach (var keyDict in extendedFileDefinitions)
                {
                    if (!string.IsNullOrEmpty(recordKey))
                    {
                        recordKey = string.Concat(recordKey, DmiString._XM, keyDict.Key, "+", keyDict.Value);
                    }
                    else
                    {
                        recordKey = string.Concat(keyDict.Key, "+", keyDict.Value);
                    }
                }
                // Add key parts with no file associated to them
                foreach (var keyDict in extendedKeyDefinitions)
                {
                    var matchingColumn = extendedEthosVersion.ExtendedDataFilterList.FirstOrDefault(edf => edf.ColleagueColumnName == keyDict.Key);
                    if (matchingColumn != null && string.IsNullOrEmpty(matchingColumn.ColleagueFileName))
                    {
                        if (!string.IsNullOrEmpty(recordKey))
                        {
                            recordKey = string.Concat(recordKey, DmiString._XM, keyDict.Key, "+", keyDict.Value);
                        }
                        else
                        {
                            recordKey = string.Concat(keyDict.Key, "+", keyDict.Value);
                        }
                    }
                }
                if (!apiType.Equals("T", StringComparison.OrdinalIgnoreCase))
                {
                    recordKey = recordKey.Split(DmiString._XM)[0];
                }
                id = _ethosApiBuilderService.EncodePrimaryKey(recordKey);
            }
            return id;
        }

        /// <summary>
        /// Validate the ID properties defined in a query string.
        /// </summary>
        /// <param name="routeInfo">Object containing route information</param>
        /// <param name="extendedEthosVersion">Version Specific Configuration</param>
        /// <param name="ignoreErrors">Flag to ignore any errors or exceptions</param>
        /// <returns></returns>
        private async Task<Tuple<Dictionary<string, string>, Dictionary<string, string>>> ValidateIdsFromQueryAsync(EthosResourceRouteInfo routeInfo, EthosExtensibleData extendedEthosVersion, bool ignoreErrors = false)
        {
            var fileNameDictionary = new Dictionary<string, string>();
            var columnNameDictionary = new Dictionary<string, string>();
            var exception = new IntegrationApiException();
            bool requireAllKeys = false;

            // Validate the query names
            var queryString = Request.Query;
            foreach (var query in queryString.Keys)
            {
                if (query.ToString().Equals("key", StringComparison.OrdinalIgnoreCase) || query.ToString().Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    requireAllKeys = true;
                }
            }

            // If we are processing a Business Process API (BAPI) from a UI form then we may not have any primary keys
            // or tables, therefore we will support QAPI as if it were a GET by ID.
            if (extendedEthosVersion.ColleagueFileNames != null && extendedEthosVersion.ColleagueFileNames.Any())
            {
                // Only continue processing if we are using a query name of "key" or "id" or we are not doing QAPI Post 
                if (routeInfo.isQueryByPost)
                {
                    return new Tuple<Dictionary<string, string>, Dictionary<string, string>>(fileNameDictionary, columnNameDictionary);
                }
                else if (!requireAllKeys)
                {
                    return new Tuple<Dictionary<string, string>, Dictionary<string, string>>(fileNameDictionary, columnNameDictionary);
                }
            }
            else
            {
                // If we don't have Colleague File Names defined then require all keys in the QAPI Post.
                if (routeInfo.isQueryByPost)
                {
                    requireAllKeys = true;
                }
            }

            // Validate query string
            var keyDictionary = routeInfo.ExtendedFilterDefinitions;
            foreach (var filter in keyDictionary)
            {
                var filterName = filter.Key;
                var value = (filter.Value.Item1 != null && filter.Value.Item1.Any()) ? filter.Value.Item1.FirstOrDefault() : string.Empty;

                var matchingProperty = extendedEthosVersion.ExtendedDataFilterList.FirstOrDefault(ed => ed.FullJsonPath == filterName.TrimEnd(']').TrimEnd('['));
                if (matchingProperty == null)
                {
                    if (!requireAllKeys || ignoreErrors)
                    {
                        continue;
                    }
                    var message = string.Concat("'", filterName, "' is an invalid key property within the schema.");
                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                }
                else
                {
                    if (matchingProperty.KeyQuery && !string.IsNullOrEmpty(value))
                    {
                        var primaryFileName = matchingProperty.ColleagueFileName;
                        var fileName = !string.IsNullOrEmpty(matchingProperty.ColleagueFileName) ? matchingProperty.ColleagueFileName : matchingProperty.ColleagueColumnName;
                        var columnName = matchingProperty.ColleagueColumnName;
                        var databaseUsageType = matchingProperty.DatabaseUsageType;
                        var colleaguePropertyPosition = matchingProperty.ColleaguePropertyPosition;
                        var jsonPropertyType = matchingProperty.JsonPropertyType;
                        var guidColumnName = matchingProperty.GuidColumnName;
                        var guidFileName = matchingProperty.GuidFileName;
                        var transColumnName = matchingProperty.TransColumnName;
                        var transFileName = matchingProperty.TransFileName;
                        var transTableName = matchingProperty.TransTableName;
                        var enumerations = matchingProperty.Enumerations;
                        var newValue = value;
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (value.StartsWith("@"))
                            {
                                // Don't allow an "@" in a key value
                                var message = string.Concat("'", value, "' is an invalid key value.  The '@' sign is not allowed.");
                                exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                            }
                            if (fileName == primaryFileName && !databaseUsageType.Equals("k", StringComparison.OrdinalIgnoreCase))
                            {
                                if (routeInfo.isQueryByPost || ignoreErrors)
                                {
                                    continue;
                                }
                                var message = string.Concat("'", filterName, "' is an invalid key property within the schema.");
                                exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                            }
                            if (jsonPropertyType == JsonPropertyTypeExtensions.Date)
                            {
                                try
                                {
                                    DateTime temp = Convert.ToDateTime(newValue);
                                    newValue = string.Concat(temp.Month.ToString(), "/", temp.Day.ToString(), "/", temp.Year.ToString());
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid Date. ", ex.Message);
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                }
                            }
                            if (jsonPropertyType == JsonPropertyTypeExtensions.DateTime)
                            {
                                try
                                {
                                    DateTime temp = Convert.ToDateTime(newValue);
                                    newValue = string.Concat(temp.Month.ToString(), "/", temp.Day.ToString(), "/", temp.Year.ToString());
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid DateTime. ", ex.Message);
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                }
                            }
                            if (jsonPropertyType == JsonPropertyTypeExtensions.Time)
                            {
                                try
                                {
                                    newValue = DateTime.Today.ToString() + "T" + newValue;
                                    DateTime temp = Convert.ToDateTime(newValue);
                                    newValue = string.Concat(temp.Hour.ToString(), ":", temp.Minute.ToString(), ":", temp.Second.ToString());
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid DateTime. ", ex.Message);
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                }
                            }
                            if (jsonPropertyType == JsonPropertyTypeExtensions.Number)
                            {
                                try
                                {
                                    decimal temp = Convert.ToDecimal(newValue);
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid Decimal Number. ", ex.Message);
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                }
                            }
                            if (!string.IsNullOrEmpty(guidColumnName) && !string.IsNullOrEmpty(guidFileName))
                            {
                                newValue = await _ethosApiBuilderService.GetRecordIdFromGuidAsync(newValue, guidFileName, guidColumnName);
                                if (string.IsNullOrEmpty(newValue)) newValue = value;
                            }
                            if (!string.IsNullOrEmpty(transColumnName))
                            {
                                newValue = await _ethosApiBuilderService.GetRecordIdFromTranslationAsync(newValue, transFileName, transColumnName, transTableName);
                                if (string.IsNullOrEmpty(newValue)) newValue = value;
                            }
                            if (enumerations != null && enumerations.Any())
                            {
                                var matchingEnumeration = enumerations.FirstOrDefault(en => en.EnumerationValue.Equals(newValue, StringComparison.OrdinalIgnoreCase));
                                if (matchingEnumeration == null)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid enumeration value. ");
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                }
                                else
                                {
                                    newValue = matchingEnumeration.ColleagueValue;
                                }
                            }

                            if (fileName != columnName)
                            {

                                // Update Filter Name entry for full key assembly.
                                if (colleaguePropertyPosition.HasValue && colleaguePropertyPosition.Value > 0)
                                {
                                    var newKeyValue = string.Empty;
                                    int keyCount = 0;
                                    string[] splitValues = splitValues = newValue.Split('*');
                                    int pos = Convert.ToInt32(colleaguePropertyPosition);
                                    string origValue = string.Empty;
                                    if (fileNameDictionary.TryGetValue(fileName, out origValue))
                                    {
                                        splitValues = origValue.Split('*');
                                        keyCount = splitValues.Count();
                                    }
                                    int max = pos > keyCount ? pos - 1 : keyCount - 1;
                                    for (int i = 0; i <= max; i++)
                                    {
                                        if (i == pos - 1)
                                        {
                                            newKeyValue = string.Concat(newKeyValue, newValue);
                                        }
                                        else
                                        {
                                            if (i < keyCount)
                                            {
                                                newKeyValue = string.Concat(newKeyValue, splitValues[i]);
                                            }
                                        }
                                        newKeyValue = string.Concat(newKeyValue, "*");
                                    }
                                    newValue = newKeyValue.TrimEnd('*');
                                }
                                if (!fileNameDictionary.ContainsKey(fileName))
                                {
                                    fileNameDictionary.Add(fileName, newValue);
                                }
                                else
                                {
                                    fileNameDictionary[fileName] = newValue;
                                }
                            }

                            // Update Column Name value in the key dictionary
                            if (!columnNameDictionary.ContainsKey(columnName))
                            {
                                columnNameDictionary.Add(columnName, newValue);
                            }
                            else
                            {
                                columnNameDictionary[columnName] = newValue;
                            }
                        }
                    }
                }
            }

            if (requireAllKeys && columnNameDictionary.Count() < extendedEthosVersion.ColleagueKeyNames.Count())
            {
                foreach (var keyName in extendedEthosVersion.ColleagueKeyNames)
                {
                    if (!columnNameDictionary.ContainsKey(keyName) && !ignoreErrors)
                    {
                        var message = string.Concat("'", keyName, "' is a required key property within the schema.");
                        var matchingProperty = extendedEthosVersion.ExtendedDataFilterList.FirstOrDefault(ed => ed.ColleagueColumnName.Equals(keyName, StringComparison.OrdinalIgnoreCase));
                        if (matchingProperty != null)
                        {
                            message = string.Concat("'", matchingProperty.FullJsonPath, "' is a required key property within the schema.");
                        }
                        exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                    }
                }
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return new Tuple<Dictionary<string, string>, Dictionary<string, string>>(fileNameDictionary, columnNameDictionary);
        }

        /// <summary>
        /// Extract the ID properties from the Json BODY of a QAPI or PUT or POST request.
        /// </summary>
        /// <param name="extendedKeyDictionaries">Tuple containing Key Dictionaries with information about a key property</param>
        /// <param name="extendedEthosVersion">Version specific configuration</param>
        /// <param name="apiType">Api Type of A (Spec based API), E or "" (Extensions to API), or T (Business Process API)</param>
        /// <param name="primaryEntity">Primary entity as defined by the configuration</param>
        /// <param name="primaryGuidSource">indicator to see if the API has GUID support</param>
        /// <returns>String containing the ID for the record being processed.</returns>
        private async Task<string> GetIdPropertyFromBody(Tuple<Dictionary<string, string>, Dictionary<string, string>> extendedKeyDictionaries, EthosExtensibleData extendedEthosVersion, string apiType, string primaryEntity, string primaryGuidSource)
        {
            string id = string.Empty;
            try
            {
                JContainer jsonString;
                var bodyString = string.Empty;
                Request.Body.Position = 0;
                bodyString = await new StreamReader(Request.Body).ReadToEndAsync();
                jsonString = JObject.Parse(bodyString);
                if (jsonString != null)
                {
                    var selectedToken = jsonString.SelectToken("$.id", false);
                    if (selectedToken != null)
                    {
                        if (selectedToken.Children().Any())
                        {
                            id = BuildRecordKeyFromFilterData(extendedKeyDictionaries, extendedEthosVersion, apiType);
                        }
                        else
                        {
                            id = selectedToken.ToString();
                            //don't do this if id is a guid
                            if (string.IsNullOrEmpty(primaryGuidSource))
                            {
                                if (!(_ethosApiBuilderService.UnEncodePrimaryKey(id).Contains("+")))
                                {
                                    id = BuildRecordKeyFromFilterData(extendedKeyDictionaries, extendedEthosVersion, apiType);
                                    if (string.IsNullOrEmpty(id))
                                    {
                                        id = _ethosApiBuilderService.EncodePrimaryKey(string.Concat(primaryEntity, "+", selectedToken.ToString()));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        id = BuildRecordKeyFromFilterData(extendedKeyDictionaries, extendedEthosVersion, apiType);
                    }
                }
            }
            catch
            {
                return id;
            }
            return id;
        }

        /// <summary>
        /// Parse JSON object values from a JSON string
        /// </summary>
        /// <param name="value">JSON string to deserialize into a JSON object</param>
        /// <param name="queryName">Name of query being parsed for purposes of reporting an error</param>
        /// <param name="extendedFilterDefinitions">>Tuple containing path, oper, and values.</param>
        /// <returns>>Tuple containing path, oper, and values.</returns>
        private Dictionary<string, Tuple<List<string>, string>> ParseJsonStringValues(string value, string queryName, Dictionary<string, Tuple<List<string>, string>> extendedFilterDefinitions)
        {
            try
            {
                object filterObj = JsonConvert.DeserializeObject(value);
                JObject jObject = (JObject)filterObj;

                //get all the properties associated with the query string
                IEnumerable<JProperty> jProperties = jObject.Properties();

                // Loop through all the properties of that JObject
                foreach (JProperty jProperty in jProperties)
                {

                    if ((jProperty != null) && (jProperty.Value != null))
                    {
                        var jToken = (jProperty.Value);
                        if (jToken != null)
                        {
                            extendedFilterDefinitions = ProcessTokens(jToken, queryName, jProperty.Name, extendedFilterDefinitions);
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                string message;
                if (string.IsNullOrEmpty(queryName))
                {
                    message = string.Concat("'", value, "' is an invalid Json string. ");
                }
                else
                {
                    message = string.Concat("'", queryName, "=", value, "' is an invalid query parameter for filtering. ", ex.Message);
                }
                throw new JsonException(message);
            }

            return extendedFilterDefinitions;
        }

        /// <summary>
        /// Process Children of a token to extract path, oper and values for a filter
        /// </summary>
        /// <param name="jToken">JSON object token</param>
        /// <param name="extendedFilterDefinitions">>Tuple containing path, oper, and values.</param>
        /// <param name="queryName">Query Name that filters may belong to for reporting issues with JSON parsing.</param>
        /// <param name="propertyName">A property name that may also apply for reporting issues in JSON parsing.</param>
        /// <returns>Tuple containing path, oper, and values.</returns>
        private Dictionary<string, Tuple<List<string>, string>> ProcessTokens(JToken jToken, string queryName, string propertyName, Dictionary<string, Tuple<List<string>, string>> extendedFilterDefinitions)
        {
            string path;
            string oper = string.Empty;
            List<string> values = new List<string>();

            var jTokenChildren = jToken.Children();
            if (jTokenChildren.Any())
            {
                foreach (var childToken in jTokenChildren)
                {
                    if (childToken.HasValues)
                    {
                        extendedFilterDefinitions = ProcessTokens(childToken, queryName, propertyName, extendedFilterDefinitions);
                    }
                }
            }

            if (jToken.Type == JTokenType.Array)
            {
                extendedFilterDefinitions = ProcessArrayTokens(jToken, extendedFilterDefinitions);
            }
            else
            {
                path = jToken.HasValues && jToken.Last != null ? jToken.Last.Path : jToken.Path;
                if (jToken.HasValues && jToken.Last != null && jToken.Last.HasValues && jToken.Last.Last != null && jToken.Last.Last.HasValues)
                {
                    path = jToken.Last.Last.Path;
                    if (jToken.Last.Last.Last != null && jToken.Last.Last.Last.HasValues)
                    {
                        path = jToken.Last.Last.Last.Path;
                        if (jToken.Last.Last.Last.Last != null && jToken.Last.Last.Last.Last.HasValues)
                        {
                            path = jToken.Last.Last.Last.Last.Path;
                        }
                    }
                }
                var pathSplit = path.Split('.');
                path = string.Empty;
                foreach (var part in pathSplit)
                {
                    if (part.StartsWith("$"))
                    {
                        oper = part;
                    }
                    else
                    {
                        path = string.Concat(path, ".", part).TrimStart('.');
                    }
                }
                var temp = jToken.HasValues && jToken.Last != null && jToken.Last.HasValues ? jToken.Last.Values() : jToken.Values();
                if (jToken.HasValues && jToken.Last != null && jToken.Last.HasValues && jToken.Last.Last != null && jToken.Last.Last.HasValues)
                {
                    temp = jToken.Last.Last.Values();
                    if (jToken.Last.Last.Last != null && jToken.Last.Last.Last.HasValues)
                    {
                        temp = jToken.Last.Last.Last.Values();
                        if (jToken.Last.Last.Last.Last != null && jToken.Last.Last.Last.Last.HasValues)
                        {
                            temp = jToken.Last.Last.Last.Last.Values();
                        }
                    }
                }
                if (temp != null && jToken.HasValues)
                {
                    foreach (var tokenValue in temp)
                    {
                        values.Add(tokenValue.ToString());
                    }
                }
                else
                {
                    values.Add(jToken.Value<string>());
                }

                path = path.Replace("[0]", "[]");
                if (path.IndexOf("[") > 0)
                {
                    string restOfPath = string.Empty;
                    if (path.IndexOf("]") < path.Length)
                    {
                        restOfPath = path.Substring(path.IndexOf("]") + 1);
                    }
                    path = path.Substring(0, path.IndexOf("[")) + "[]" + restOfPath;
                }
                if (values != null && values.Any() && !string.IsNullOrEmpty(path))
                {
                    if (!extendedFilterDefinitions.ContainsKey(path))
                    {
                        extendedFilterDefinitions.Add(path, new Tuple<List<string>, string>(values, oper));
                    }
                }
                else
                {
                    string message;
                    if (string.IsNullOrEmpty(queryName))
                    {
                        message = string.Concat("'", propertyName, ":", jToken.ToString(), "' is an invalid query parameter for filtering. ");
                    }
                    else
                    {
                        message = string.Concat("'", queryName, "=", jToken.ToString(), "' is an invalid query parameter for filtering. ");
                    }
                    throw new JsonException(message);
                }
            }

            return extendedFilterDefinitions;
        }

        /// <summary>
        /// Process Children of a token to extract path, oper and values for a filter
        /// </summary>
        /// <param name="cToken"></param>
        /// <param name="extendedFilterDefinitions"></param>
        /// <returns>Tuple containing path, oper, and values.</returns>
        private Dictionary<string, Tuple<List<string>, string>> ProcessArrayTokens(JToken cToken, Dictionary<string, Tuple<List<string>, string>> extendedFilterDefinitions)
        {
            string path = string.Empty;
            string oper = string.Empty;
            List<string> values = new List<string>();

            var children = cToken.Children();
            if (children.Any())
            {
                foreach (var gToken in children)
                {
                    extendedFilterDefinitions = ProcessArrayTokens(gToken, extendedFilterDefinitions);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = cToken.HasValues && cToken.Last != null ? cToken.Last.Path : cToken.Path;
                    var pathSplit = path.Split('.');
                    path = string.Empty;
                    foreach (var part in pathSplit)
                    {
                        if (part.StartsWith("$"))
                        {
                            oper = part;
                        }
                        else
                        {
                            path = string.Concat(path, ".", part).TrimStart('.');
                        }
                    }
                }
                var temp = cToken.HasValues && cToken.Last != null ? cToken.Last.Values() : cToken.Values();
                if (temp != null && temp.Any())
                {
                    foreach (var val in temp)
                    {
                        values.Add(val.ToString());
                    }
                }
                else
                {
                    if (cToken.HasValues)
                    {
                        values.Add(cToken.Value<string>());
                    }
                }

                path = path.Replace("[0]", "[]");
                if (path.IndexOf("[") > 0)
                {
                    string restOfPath = string.Empty;
                    if (path.IndexOf("]") < path.Length)
                    {
                        restOfPath = path.Substring(path.IndexOf("]") + 1);
                    }
                    path = path.Substring(0, path.IndexOf("[")) + "[]" + restOfPath;
                }
                if (values != null && values.Any() && !string.IsNullOrEmpty(path))
                {
                    if (!extendedFilterDefinitions.ContainsKey(path))
                    {
                        extendedFilterDefinitions.Add(path, new Tuple<List<string>, string>(values, oper));
                    }
                }
            }

            return extendedFilterDefinitions;
        }

        #endregion
    }
}

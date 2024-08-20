// Copyright 2020-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.EthosExtend;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Utilities;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using NUglify.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Api.Controllers
{
	/// <summary>
	/// Provides specific version information
	/// </summary>
	[Authorize]
	[LicenseProvider(typeof(EllucianLicenseProvider))]
	[EllucianLicenseModule(ModuleConstants.Base)]
	public class MetadataController : BaseCompressedApiController
	{
		private const string EEDM_WEBAPI_METADATA_CACHE_KEY = "EEDM_WEBAPI_METADATA_CACHE_KEY";
		private const string mediaFormat = "vnd.hedtech.integration";
		private const string isEEdmSupported = "isEedmSupported";
		private const string sourceSystem = "colleague";
		private const int PageLimit = 100;

		private const string GUID_PATTERN = "^[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}$";

		private readonly ICacheProvider _cacheProvider;
		private readonly ILogger _logger;
		private readonly IServer server;
		private readonly IEthosApiBuilderService _ethosApiBuilderService;
		private readonly IActionDescriptorCollectionProvider _actionProvider;
		private readonly IEnumerable<EndpointDataSource> _endpoints;

		private bool useV2errors = false;
		private bool ignoreDefaultFiltering = false;
		private string[] permissionsCollection = null;
		private List<string> requestedContentTypes = new List<string>();
		private Dictionary<string, Type> queryNamesDtoTypes = new Dictionary<string, Type>();
		private Dictionary<string, Domain.Base.Entities.EthosExtensibleData> queryNamesVersionConfigs = new Dictionary<string, Domain.Base.Entities.EthosExtensibleData>();

		/// <summary>
		///MetadataController
		/// </summary>
		public MetadataController(
			IEthosApiBuilderService ethosApiBuilderService, ICacheProvider cacheProvider, ILogger logger, IServer server, IActionContextAccessor actionContextAccessor, IActionDescriptorCollectionProvider actionProvider, ApiSettings apiSettings, IEnumerable<EndpointDataSource> endpoints) : base(actionContextAccessor, apiSettings)
		{
			_cacheProvider = cacheProvider;
			this._logger = logger;
			this.server = server;
			_ethosApiBuilderService = ethosApiBuilderService;
			_actionProvider = actionProvider;
			_endpoints = endpoints;
		}

		/// <summary>
		/// Retrieves all the openAPI Specifications for a resource
		/// </summary>
		/// <returns>OpenAPI Specifications version 3.0.</returns>
		[CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
		[HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
		[ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
		[HeaderVersionRoute("/metadata/{resourceName}", 3, false, RouteConstants.HedtechIntegrationOpenApiMetdataTypeFormat, Name = "GetOpenApiMetadataForDefaultOpenApiMetdataType", IsEedmSupported = true)]
		[HeaderVersionRoute("/metadata/{resourceName}", 3, false, RouteConstants.HedtechIntegrationOpenApiMetdataPublishTypeFormat, Name = "GetOpenApiMetadataForDefaultOpenApiMetdataPublishType", IsEedmSupported = true)]
		public async Task<ActionResult<IEnumerable<object>>> GetOpenApiMetadata([FromRoute] string resourceName)
		{
			bool bypassCache = false;
			if ((Request != null) && (Request.GetTypedHeaders().CacheControl != null))
			{
				if (Request.GetTypedHeaders().CacheControl.NoCache)
				{
					bypassCache = true;
				}
			}
			bool publishDocumentation = false;
			if (Request != null && Request.GetTypedHeaders().Accept != null)
			{
				if (Request.GetTypedHeaders().Accept.Any(hv => hv.MediaType.Equals(string.Format(RouteConstants.HedtechIntegrationOpenApiMetdataPublishTypeFormat, 3))))
				{
					publishDocumentation = true;
				}
			}

			try
			{
				if (string.IsNullOrEmpty(resourceName))
				{

					return CreateHttpResponseException(new IntegrationApiException("",
						new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.",
						"API name is needed to return OpenAPI specifications.")));
				}

				var endpoints = from epc in _endpoints
								from ep in epc.Endpoints
								where ep is RouteEndpoint
								select ep as RouteEndpoint;

				if (!string.IsNullOrEmpty(resourceName))
				{
					endpoints = endpoints.Where(x =>
					{
						var apiName = GetApiName(x.RoutePattern.RawText);
						return apiName == resourceName;
					}).ToList();
				}
				var openApidocs = await GetOpenApiAsync(endpoints, resourceName, bypassCache);
				var openApiDocObjects = new List<object>();
				if (openApidocs != null && openApidocs.Any())
				{
					foreach (var openApidoc in openApidocs)
					{
						try
						{
							var openApiJson = openApidoc.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
							if (publishDocumentation)
								CreateYamlFile(openApidoc);
							if (!string.IsNullOrEmpty(openApiJson))
							{
								var openApiJsonObject = JsonConvert.DeserializeObject(openApiJson);
								if (openApiJsonObject != null)
								{
									openApiDocObjects.Add(openApiJsonObject);
								}
							}
						}
						catch (Exception ex)
						{
							_logger.LogError(ex.ToString() + openApidoc);
						}
					}
				}
				return openApiDocObjects;
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
		/// Retrieves openAPI Specifications for a resource for a particular version
		/// </summary>
		/// <returns>OpenAPI Specifications version 3.0.</returns>
		[CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
		[HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
		[ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
		[HeaderVersionRoute("/metadata/{resourceName}/{version}", 3, false, RouteConstants.HedtechIntegrationOpenApiMetdataTypeFormat, Name = "GetOpenApiMetadataByVersionForDefaultOpenApiMetdataType", IsEedmSupported = true)]
		[HeaderVersionRoute("/metadata/{resourceName}/{version}", 3, false, RouteConstants.HedtechIntegrationOpenApiMetdataPublishTypeFormat, Name = "GetOpenApiMetadataByVersionForDefaultOpenApiMetdataPublishType", IsEedmSupported = true)]
		public async Task<ActionResult<object>> GetOpenApiMetadataByVersion([FromRoute] string resourceName, [FromRoute] string version)
		{
			bool bypassCache = false;
			if ((Request != null) && (Request.GetTypedHeaders().CacheControl != null))
			{
				if (Request.GetTypedHeaders().CacheControl.NoCache)
				{
					bypassCache = true;
				}
			}
			bool publishDocumentation = false;
			if (Request != null && Request.GetTypedHeaders().Accept != null)
			{
				if (Request.GetTypedHeaders().Accept.Any(hv => hv.MediaType.Equals(string.Format(RouteConstants.HedtechIntegrationOpenApiMetdataPublishTypeFormat, 3))))
				{
					publishDocumentation = true;
				}
			}

			try
			{
				if (string.IsNullOrEmpty(resourceName))
				{

					throw new IntegrationApiException("",
						new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.",
						"API name is needed to return OpenAPI specifications."));
				}
				if (string.IsNullOrEmpty(version))
				{

					throw new IntegrationApiException("",
						new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.",
						"Version is needed to return OpenAPI specifications for a specific version."));
				}

				var endpoints = from epc in _endpoints
								from ep in epc.Endpoints
								where ep is RouteEndpoint
								select ep as RouteEndpoint;

				if (!string.IsNullOrEmpty(resourceName))
				{
					endpoints = endpoints.Where(x =>
					{
						var apiName = GetApiName(x.RoutePattern.RawText);
						return apiName == resourceName;
					}).ToList();
				}

				var openApidocs = await GetOpenApiAsync(endpoints, resourceName, bypassCache);
				var versionNumberComparer = new MetadataVersionNumberComparer();
				if (openApidocs != null && openApidocs.Any())
				{
					IEnumerable<OpenApiDocument> openApiDocs = null;
					string latestVersion = "0.0.0";
					if (version.ToLower() != "latest")
					{
						latestVersion = version;
					}
					else
					{
						foreach (var openApiDoc in openApidocs)
						{
							if (openApiDoc.Info != null && !string.IsNullOrEmpty(openApiDoc.Info.Version))
							{
								if (versionNumberComparer.Compare(openApiDoc.Info.Version, latestVersion) == 1)
								{
									latestVersion = openApiDoc.Info.Version;
								}
							}
						}
					}

					openApiDocs = openApidocs.Where(ver => ver.Info != null && !string.IsNullOrEmpty(ver.Info.Version) && ver.Info.Version.Replace("-beta", "").Equals(latestVersion));
					if (openApiDocs == null || !openApiDocs.Any())
					{
						openApiDocs = openApidocs.Where(ver => ver.Info != null && !string.IsNullOrEmpty(ver.Info.Version) && ver.Info.Version.StartsWith(latestVersion));
					}

					if (openApiDocs != null && openApiDocs.Any())
					{
						List<object> returnObject = new List<object>();

						foreach (var openApiDoc in openApiDocs)
						{
							try
							{
								var openApiJson = openApiDoc.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
								if (publishDocumentation)
									CreateYamlFile(openApiDoc);
								if (!string.IsNullOrEmpty(openApiJson))
								{
									returnObject.Add(JsonConvert.DeserializeObject(openApiJson));
								}
							}
							catch (Exception ex)
							{
								_logger.LogError(ex.ToString() + openApiDoc);
							}
						}
						if (returnObject.Count() > 1)
							return returnObject;
						else
							return returnObject.FirstOrDefault();
					}
					else
					{
						throw new IntegrationApiException("",
							new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.",
							"OpenAPI specifications does not exist for this version."));
					}
				}
				else
				{
					throw new KeyNotFoundException(string.Format("No openAPI specifications found for version '{0}' of the resource {1}", version, resourceName));
				}
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
		/// Create the YAML file from the OpenApi Document and save it to the Documentation folder.
		/// </summary>
		/// <param name="openApiDocument"></param>
		private void CreateYamlFile(OpenApiDocument openApiDocument)
		{
			if (openApiDocument != null && openApiDocument.Info != null)
			{
				string apiName = openApiDocument.Info.Title;
				string apiVersion = openApiDocument.Info.Version;
				// Manifest (and API) must have symantic versioning.
				if (apiVersion.Split(".").Count() <= 2)
				{
					if (apiVersion.Split(".").Count() == 2) apiVersion = string.Concat(apiVersion, ".0");
					else apiVersion = string.Concat(apiVersion, ".0.0");
				}
				OpenApiString openApiDomain = (openApiDocument.Info.Extensions.FirstOrDefault(dict => dict.Key == "x-source-domain").Value as OpenApiString);
				OpenApiString openApiType = (openApiDocument.Info.Extensions.FirstOrDefault(dict => dict.Key == "x-api-type").Value as OpenApiString);
				var key = string.Concat(apiName.ToLower(), ".yaml");
				var openApiYaml = openApiDocument.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_0);

				var fullPath = AppDomain.CurrentDomain.BaseDirectory;
				var basePath = fullPath.Substring(0, fullPath.IndexOf("\\Source"));
				var codeBase = Path.Combine(basePath, "Source\\Documentation\\OpenAPI");

				// Make sure that "OpenAPI" exists in path"
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
				}

				string dirName = "ColleagueWebNonEthosAPIs";
				if (!string.IsNullOrEmpty(openApiType.Value))
				{
					switch (openApiType.Value)
					{
						case "ethos":
							{
								dirName = "ColleagEedmAPIs";
								break;
							}
						case "bus-proc":
							{
								dirName = "ColleagueBusAPIs";
								break;
							}
						case "specification":
							{
								dirName = "ColleagueSpecAPIs";
								break;
							}
						case "legacy":
							{
								dirName = "ColleagueWebNonEthosAPIs";
								break;
							}
						case "web-ethos":
							{
								dirName = "ColleagueWebEthosAPIs";
								break;
							}
						default:
							{
								dirName = "ColleagueWebNonEthosAPIs";
								break;
							}
					}
				}
				// Make sure Type exists in path"
				codeBase = Path.Combine(codeBase, dirName);
				uri = new UriBuilder(codeBase);
				path = Uri.UnescapeDataString(uri.Path);
				if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
				}

				// Add API name and version to path for creation of directory
				codeBase = Path.Combine(codeBase, string.Concat(apiName.ToLower(), "-", apiVersion));
				uri = new UriBuilder(codeBase);
				path = Uri.UnescapeDataString(uri.Path);
				if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
				}

				// Add Key to path for creation of Yaml file
				codeBase = Path.Combine(codeBase, key);
				uri = new UriBuilder(codeBase);
				path = Uri.UnescapeDataString(uri.Path);

				// Save yaml file in the documentation directory
				try
				{
					System.IO.File.WriteAllText(path, openApiYaml);
				}
				catch (Exception ex)
				{
					throw new ColleagueWebApiDtoException("No access to update '" + path + "'. ", ex);
				}
			}
		}

		/// <summary>
		/// Retrieves all the openAPI Manifest Data for publishing in the API Catalog.
		/// </summary>
		/// <returns>Manifest Data for the sepcified API and version.</returns>
		[CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
		[HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
		[ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
		[HttpGet]
		[HeaderVersionRoute("/metadata/manifest/{apiDomain}/{apiType}", 3, false, RouteConstants.HedtechIntegrationOpenApiMetdataTypeFormat, Name = "GetOpenApiManifestForOpenApi")]
		[HeaderVersionRoute("/metadata/manifest/api/{resourceName}/{version}", 3, false, RouteConstants.HedtechIntegrationOpenApiMetdataTypeFormat, Name = "GetOpenApiManifestByApiNameAndVersion")]
		[HeaderVersionRoute("/metadata/manifest/{apiDomain}/{apiType}", 3, false, RouteConstants.HedtechIntegrationOpenApiMetdataPublishTypeFormat, Name = "GetOpenApiManifestForOpenApiPublish")]
		[HeaderVersionRoute("/metadata/manifest/api/{resourceName}/{version}", 3, false, RouteConstants.HedtechIntegrationOpenApiMetdataPublishTypeFormat, Name = "GetOpenApiManifestByApiNameAndVersionPublish")]
		public async Task<ActionResult<IEnumerable<Dtos.OpenApiManifest>>> GetOpenApiManifestMetadata([FromRoute] string apiDomain, [FromRoute] string apiType, [FromRoute] string resourceName, [FromRoute] string version)
		{
			bool bypassCache = false;
			string selectedResource = resourceName;
			string selectedVersion = version;
			if (apiDomain == "api")
			{
				selectedResource = apiType;
				apiDomain = "";
				apiType = "";
			}
			if (version == "any" || version == "all") selectedVersion = string.Empty;

			if ((Request != null) && (Request.GetTypedHeaders().CacheControl != null))
			{
				if (Request.GetTypedHeaders().CacheControl.NoCache)
				{
					bypassCache = true;
				}
			}
			bool publishDocumentation = false;
			if (Request != null && Request.GetTypedHeaders().Accept != null)
			{
				if (Request.GetTypedHeaders().Accept.Any(hv => hv.MediaType.Equals(string.Format(RouteConstants.HedtechIntegrationOpenApiMetdataPublishTypeFormat, 3))))
				{
					publishDocumentation = true;
				}
			}

			try
			{
				var endpoints = from epc in _endpoints
								from ep in epc.Endpoints
								where ep is RouteEndpoint
								select ep as RouteEndpoint;

				if (!string.IsNullOrEmpty(selectedResource))
				{
					endpoints = endpoints.Where(x =>
					{
						var apiName = GetApiName(x.RoutePattern.RawText);
						return apiName == selectedResource;
					}).ToList();
				}

				var manifests = await GetOpenApiManifestsAsync(endpoints, apiDomain, apiType, selectedResource, selectedVersion, bypassCache);
				if (publishDocumentation)
				{
					Dictionary<string, IEnumerable<OpenApiDocument>> manifestApiGenerated = new Dictionary<string, IEnumerable<OpenApiDocument>>();
					List<OpenApiManifest> newManifestList = new List<OpenApiManifest>();
					foreach (var manifest in manifests)
					{
						try
						{
							var routeSource = manifest.RouteSource;
							var resourceEndPoints = endpoints.Where(x =>
							{
								var apiName = GetApiName(x.RoutePattern.RawText);
								return apiName.StartsWith(routeSource);
							}).ToList();
							if (resourceEndPoints != null && resourceEndPoints.Any())
							{
								IEnumerable<OpenApiDocument> openApiDocs = new List<OpenApiDocument>();
								var success = manifestApiGenerated.TryGetValue(routeSource, out openApiDocs);
								if (!success)
								{
									openApiDocs = await GetOpenApiAsync(resourceEndPoints, routeSource, bypassCache);
									foreach (var openApiDoc in openApiDocs)
									{
										var openApiJson = openApiDoc.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
										CreateYamlFile(openApiDoc);
									}
									manifestApiGenerated.Add(routeSource, openApiDocs);
								}

								if (!string.IsNullOrEmpty(apiDomain) && !string.IsNullOrEmpty(apiType))
								{
									var openApiDocList = openApiDocs.Where(api =>
									{
										bool matchingDomain = false;
										bool matchingType = false;
										if (api.Info != null && api.Info.Extensions != null)
										{
											OpenApiString openApiDomain = (api.Info.Extensions.FirstOrDefault(dict => dict.Key == "x-source-domain").Value as OpenApiString);
											OpenApiString openApiType = (api.Info.Extensions.FirstOrDefault(dict => dict.Key == "x-api-type").Value as OpenApiString);
											if (openApiDomain != null)
												matchingDomain = (openApiDomain.Value.Replace(" ", "").ToLower() == ConvertDomain2Enum(apiDomain).ToString().ToLower());
											if (openApiType != null)
												matchingType = (openApiType.Value.ToLower() == apiType.ToLower());
										}
										return (matchingDomain && matchingType);

									});
									openApiDocs = openApiDocList;
								}

								if (openApiDocs != null && openApiDocs.Any())
								{
									newManifestList.Add(manifest);
								}
							}
						}
						catch (Exception ex)
						{
							_logger.LogError(ex.ToString());
						}
					}
					manifests = newManifestList;
					CreateManifestFile(manifests, selectedResource, apiDomain, apiType, selectedVersion);
				}

				return manifests;
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
		/// Create the YAML file from the OpenApi Document and save it to the Documentation folder.
		/// </summary>
		/// <param name="openApiManifestList"></param>
		/// <param name="selectedResource"></param>
		/// <param name="apiDomain"></param>
		/// <param name="apiType"></param>
		/// <param name="selectedVersion"></param>
		private void CreateManifestFile(List<OpenApiManifest> openApiManifestList, string selectedResource, string apiDomain, string apiType, string selectedVersion)
		{
			if (openApiManifestList != null && openApiManifestList.Any())
			{
				if (!string.IsNullOrEmpty(apiDomain) && !string.IsNullOrEmpty(apiType))
				{
					openApiManifestList = openApiManifestList.Where(oap => oap.Domain.Equals(ConvertDomain2Enum(apiDomain)) && oap.ApiType.Equals(ConvertApiType2Enum(apiType))).ToList();
				}
				openApiManifestList = openApiManifestList.OrderBy(oap => oap.ApiName).ThenBy(oap => oap.Version).ToList();
				string[] columnNames = new string[] { "api_name", "version", "release", "status", "domain", "release_environment", "api_type", "api_owner" };
				var header = string.Join(",", columnNames);
				var lines = new List<string>();
				lines.Add(header);
				var lineValues = openApiManifestList.Select(row =>
				{
					var apiName = row.ApiName.ToLower();
					var release = ConvertReleaseStatusFromEnum(row.Release);
					var domain = ConvertDomainFromEnum(row.Domain);
					var type = ConvertApiTypeFromEnum(row.ApiType);
					var status = row.Status.ToString().ToLower();
					var version = row.Version;
					// Manifest (and API) must have symantic versioning.
					if (version.Split(".").Count() <= 2)
					{
						if (version.Split(".").Count() == 2) version = string.Concat(version, ".0");
						else version = string.Concat(version, ".0.0");
					}
					return string.Concat(apiName, ",", version, ",", release, ",", status, ",", domain, ",", row.ReleaseEnvironent, ",", type, ",", row.ApiOwner);
				});
				lines.AddRange(lineValues);
				var key = string.Concat("colleague", "_", GetCurrentMonth(), "_", DateTime.Today.Year.ToString());
				if (!string.IsNullOrEmpty(selectedResource)) key = string.Concat(key, "_", selectedResource);
				if (!string.IsNullOrEmpty(apiDomain)) key = string.Concat(key, "_", apiDomain);
				if (!string.IsNullOrEmpty(apiType)) key = string.Concat(key, "_", apiType);
				if (!string.IsNullOrEmpty(selectedVersion)) key = string.Concat(key, "_", selectedVersion);
				key = string.Concat(key, ".csv");

				var fullPath = AppDomain.CurrentDomain.BaseDirectory;
				var basePath = fullPath.Substring(0, fullPath.IndexOf("\\Source"));
				var codeBase = Path.Combine(basePath, "Source\\Documentation\\OpenAPI");

				// Make sure that "OpenAPI" exists in path
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
				}

				// Make sure "ManifestFiles" exists in path
				codeBase = Path.Combine(codeBase, "ManifestFiles");
				uri = new UriBuilder(codeBase);
				path = Uri.UnescapeDataString(uri.Path);
				if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
				}

				codeBase = Path.Combine(codeBase, key);
				uri = new UriBuilder(codeBase);
				path = Uri.UnescapeDataString(uri.Path);

				// Save manifest file in the documentation directory
				try
				{
					System.IO.File.WriteAllLines(path, lines);
				}
				catch (Exception ex)
				{
					throw new ColleagueWebApiDtoException("No access to update '" + path + "'. ", ex);
				}
			}
		}

		private string GetCurrentMonth()
		{
			int month = DateTime.Now.Month;
			switch (month)
			{
				case 1:
					return "January";
				case 2:
					return "February";
				case 3:
					return "March";
				case 4:
					return "April";
				case 5:
					return "May";
				case 6:
					return "June";
				case 7:
					return "July";
				case 8:
					return "August";
				case 9:
					return "September";
				case 10:
					return "October";
				case 11:
					return "November";
				case 12:
					return "December";
				default:
					return "Unknown";
			}
		}

		/// <summary>
		/// Update not supported
		/// </summary>
		/// <param name="schema"></param>
		/// <returns></returns>
		[HttpPut]
		[HeaderVersionRoute("/metadata/{resourceName}/{version}", 3, false, RouteConstants.HedtechIntegrationOpenApiMetdataTypeFormat, Name = "PutOpenApiMetadata")]
		[HeaderVersionRoute("/metadata/{resourceName}", 3, false, RouteConstants.HedtechIntegrationDefaultOpenApiMetdataTypeFormat, Name = "PutOpenApiMetadataForDefaultOpenApiMetdataType")]
		public ActionResult<IEnumerable<object>> PutOpenApiMetadata([FromBody] object schema)
		{
			//Create is not supported for Colleague but HeDM requires full crud support.
			return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

		}

		/// <summary>
		/// Create not supported
		/// </summary>
		/// <param name="schema"></param>
		/// <returns></returns>
		[HttpPost]
		[HeaderVersionRoute("/metadata/{resourceName}/{version}", 3, false, RouteConstants.HedtechIntegrationOpenApiMetdataTypeFormat, Name = "PostOpenApiMetadata")]
		[HeaderVersionRoute("/metadata", 3, false, RouteConstants.HedtechIntegrationDefaultOpenApiMetdataTypeFormat, Name = "PostOpenApiMetadataForDefaultOpenApiMetdataType")]
		public ActionResult<IEnumerable<object>> PostOpenApiMetadata([FromBody] object schema)
		{
			//Update is not supported for Colleague but HeDM requires full crud support.
			return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
		}

		/// <summary>
		/// Delete not supported
		/// </summary>
		/// <param name="resourceName"></param>
		[HttpDelete]
		[Route("/metadata/{resourceName}/{version}", Name = "DeleteOpenApiMetadata", Order = -10)]
		public IActionResult DeleteOpenApiMetadata(string resourceName)
		{
			//Delete is not supported for Colleague but HeDM requires full crud support.
			return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
		}


		/// <summary>
		/// Gets all the openAPI specifications for a spec-based or Business Process API
		/// </summary>
		/// <param name="httpRoutes"></param>
		/// <param name="selectedResource"></param>
		/// <param name="bypassCache"></param>
		private async Task<IEnumerable<OpenApiDocument>> GetOpenApiAsync(IEnumerable<RouteEndpoint> httpRoutes,
				string selectedResource, bool bypassCache = false)
		{
			var metadataCacheKey = EEDM_WEBAPI_METADATA_CACHE_KEY + selectedResource;
			var openApiDocs = new List<OpenApiDocument>();
			try
			{
				if (bypassCache == false)
				{
					if (_cacheProvider != null && _cacheProvider.Contains(metadataCacheKey))
					{
						openApiDocs = _cacheProvider[metadataCacheKey] as List<OpenApiDocument>;
						return openApiDocs;
					}
				}
				EthosApiConfiguration apiConfiguration = new EthosApiConfiguration();
				List<Domain.Base.Entities.EthosExtensibleData> apiVersionConfigs = new List<Domain.Base.Entities.EthosExtensibleData>();
				useV2errors = true;

				if (httpRoutes != null && httpRoutes.Any())
				{
					openApiDocs = await BuildApiConfigurationFromRoutesAsync(httpRoutes, selectedResource, bypassCache);
				}
				else
				{
					// get api configuration Info
					EthosResourceRouteInfo routeInfo = new EthosResourceRouteInfo()
					{
						ResourceName = selectedResource
					};
					//get EDM.EXTENSIONS
					apiConfiguration = await _ethosApiBuilderService.GetEthosApiConfigurationByResource(routeInfo, bypassCache);
					if (apiConfiguration == null)
					{
						return openApiDocs;
					}
					//get all EDM.EXT.VERSIONS
					apiVersionConfigs = await _ethosApiBuilderService.GetExtendedEthosVersionsConfigurationsByResource(routeInfo, bypassCache, false);
					if (apiVersionConfigs == null || !apiVersionConfigs.Any())
					{
						return openApiDocs;
					}

					//we have all the specifications data that we need
					foreach (var apiVersionConfig in apiVersionConfigs)
					{
						var openApiDoc = new OpenApiDocument();
						//build the info oject
						openApiDoc.Info = BuildOpenApiInfoProperty(apiConfiguration, apiVersionConfig);
						//build servers object
						openApiDoc.Servers = BuildOpenApiServersProperty(apiConfiguration);
						//build paths object
						openApiDoc.Paths = BuildOpenApiPathsProperty(apiConfiguration, apiVersionConfig);
						//add components to the document
						openApiDoc.Components = BuildOpenApiComponentsProperty(apiConfiguration, apiVersionConfig);
						openApiDocs.Add(openApiDoc);
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			_cacheProvider.Add(EEDM_WEBAPI_METADATA_CACHE_KEY, openApiDocs, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions()
			{
				AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
			});
			return openApiDocs;
		}

		/// <summary>
		/// Returns OpenAPI documentation in Extended data format for Pro-Code APIs
		/// </summary>
		/// <param name="httpRoutes"></param>
		/// <param name="selectedDomain">Create manifests only for APIs within the specified domain.</param>
		/// <param name="selectedType">Create manifests only for APIs with this specified type.</param>
		/// <param name="selectedResource">Specific API name requested for results.</param>
		/// <param name="selectedVersion">Specific version for a specified API to be returned.</param>
		/// <param name="bypassCache"></param>
		/// <returns></returns>
		private async Task<List<Dtos.OpenApiManifest>> GetOpenApiManifestsAsync(IEnumerable<RouteEndpoint> httpRoutes, string selectedDomain, string selectedType, string selectedResource, string selectedVersion, bool bypassCache = false)
		{
			List<Dtos.OpenApiManifest> openApiManifestDocs = new List<Dtos.OpenApiManifest>();

			string[] headerVersionConstraintValue = null;

			Assembly asm = Assembly.GetExecutingAssembly();

			var controlleractionlist = asm.GetTypes()
						.Where(tt => typeof(BaseCompressedApiController).IsAssignableFrom(tt))
						.SelectMany(tt => tt.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
						.Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
						.Select(x => new
						{
							Controller = x.DeclaringType.Name,
							Action = x.Name,
							x.DeclaringType,
						})
						.OrderBy(x => x.Controller).ThenBy(x => x.Action).ToList();

			Dictionary<string, List<string>> mediaTypeMethodsSupported = new Dictionary<string, List<string>>();

			foreach (RouteEndpoint httpRoute in httpRoutes)
			{
				var ResourceName = selectedResource;
				var ReleaseStatus = "R";
				var ApiDomain = "";
				var ApiType = OpenApiManifestType.Web;
				var tempType = "Legacy";
				try
				{
					//Get the route template
					var routeTemplate = !string.IsNullOrEmpty(httpRoute.RoutePattern.RawText) ? httpRoute.RoutePattern.RawText : selectedResource;

					//gets api name
					var apiName = GetApiName(routeTemplate);
					var routeSource = apiName;
					var routeSplit = routeTemplate.Split('/');
					if (routeSplit.Count() >= 2)
					{
						if (routeSplit[0].Equals("qapi", StringComparison.OrdinalIgnoreCase))
						{
							routeSource = routeSplit[1];
						}
						else
						{
							routeSource = routeSplit[0];
						}
					}
					else
					{
						routeSource = routeSplit[0];
					}

					if (routeTemplate.StartsWith("{")) continue;
					if (!string.IsNullOrEmpty(selectedResource) && !apiName.Contains(selectedResource)) continue;

					var headerVersionInfo = httpRoute.Metadata.FirstOrDefault(m => m is HeaderVersionRouteAttribute) as HeaderVersionRouteAttribute;

					var tempXMediaType = string.Empty;
					var versionOnly = string.Empty;
					if (headerVersionInfo != null)
					{
						headerVersionConstraintValue = headerVersionInfo.MediaTypes?.ToArray() ?? Array.Empty<string>();
						if (headerVersionInfo.IsEthosEnabled || headerVersionInfo.IsEedmSupported)
						{
							if (headerVersionInfo.IsEedmSupported)
							{
								ApiType = OpenApiManifestType.Ethos;
								tempType = "Ethos";
							}
							if (headerVersionInfo.IsEthosEnabled)
								ApiType = OpenApiManifestType.EthosEnabled;
							tempType = "Web-Ethos";
							tempXMediaType = string.Format("application/vnd.hedtech.integration.v{0}+json", headerVersionInfo.RouteVersion);
						}
						versionOnly = headerVersionInfo.RouteVersion;
					}

					if (headerVersionConstraintValue != null && headerVersionConstraintValue.Any())
					{
						tempXMediaType = headerVersionConstraintValue[0];
						if (tempXMediaType.Contains("/"))
							tempXMediaType = tempXMediaType.Split('/')[1];
					}
					else
					{
						if (headerVersionInfo != null && !string.IsNullOrEmpty(headerVersionInfo.RouteVersion))
						{
							tempXMediaType = string.Format("application/vnd.ellucian.v{0}+json", headerVersionInfo.RouteVersion);
						}
					}

					if (!string.IsNullOrEmpty(tempXMediaType))
					{
						versionOnly = ExtractVersionNumberOnly(tempXMediaType);
					}
					if (!string.IsNullOrEmpty(selectedVersion) && !versionOnly.Split(".")[0].Equals(selectedVersion.Split(".")[0]))
					{
						continue;
					}

					// If we don't have a version, then skip it.
					if (string.IsNullOrEmpty(versionOnly))
					{
						continue;
					}

					// Skip any obsolete bulk-requests for which we don't want to document.
					if (tempXMediaType.Contains("bulk-requests"))
					{
						continue;
					}

					// If this has an Ethos header and is identified as Legacy, but it's not Ethos Enabled
					// then it may be an end-point that is not supported and should not be included.
					if (tempXMediaType.Contains("vnd.hedtech.integration") && (tempType == "Legacy" || tempType == "Web-NonEthos"))
					{
						continue;
					}

					object controller = string.Empty;
					object action = string.Empty;
					object requestedContentType = string.Empty;

					httpRoute.RoutePattern.Defaults.TryGetValue("action", out action);
					httpRoute.RoutePattern.Defaults.TryGetValue("controller", out controller);

					try
					{
						var controlleraction = controlleractionlist
							.FirstOrDefault(x => x.Controller == string.Concat(controller.ToString(), "Controller"));

						if (controlleraction == null) continue;

						var controllerType = controlleraction.DeclaringType;

						var routeAction = action.ToString();
						if (routeAction == "ImportExtendedEthosData") continue;

						if (controllerType != null)
						{
							// Get information about the schema from the controller attribute SchemasAttribute
							// and update the manifest with this data.
							var controllerCustomAttributes = controllerType.GetCustomAttributes();
							var schemaMetaData = controllerType.GetDocumentation();
							if (!string.IsNullOrEmpty(schemaMetaData.ApiDomain)) ApiDomain = schemaMetaData.ApiDomain;

							// Derive domain from module code if it hasn't been defined by the SchemaAttribute
							if (string.IsNullOrEmpty(ApiDomain))
							{
								var controllerLicenseModule = controllerCustomAttributes.Where(mi => mi.GetType().Name == "EllucianLicenseModuleAttribute");
								if (controllerLicenseModule != null && controllerLicenseModule.Any())
								{
									foreach (EllucianLicenseModuleAttribute schemaData in controllerLicenseModule)
									{
										if (string.IsNullOrEmpty(ApiDomain))
										{
											if (!string.IsNullOrEmpty(schemaData.ModuleCode))
											{
												ApiDomain = ConvertModuleToDomain(schemaData.ModuleCode);
											}
										}
									}
								}
							}

							MethodInfo method = controllerType.GetMethods().FirstOrDefault(m => m.Name == routeAction);
							if (method == null)
							{
								// Try to find the method with "Async" appended to the end since 'routeAction' may not have it.
								if (!routeAction.EndsWith("Async"))
								{
									routeAction += "Async";
									method = controllerType.GetMethods().FirstOrDefault(m => m.Name == routeAction);
								}
							}

							if (method == null)
							{
								// If we can't find the method in the controller then we can't build the version record at all.
								// we won't have any properties so continue to next method defined.
								continue;
							}

							var obsoleteMessage = string.Empty;
							if (method.GetCustomAttribute<ObsoleteAttribute>() != null)
							{
								obsoleteMessage = method.GetCustomAttribute<ObsoleteAttribute>().Message;
							}

							var schemaMetadata = method.GetDocumentation();
							if (schemaMetadata != null)
							{
								if (!string.IsNullOrEmpty(schemaMetadata.ApiDomain)) ApiDomain = schemaMetadata.ApiDomain;
								if (!string.IsNullOrEmpty(schemaMetadata.ApiType)) ApiType = ConvertApiType2Enum(schemaMetadata.ApiType);
							}

							var customRouteAttributes = method.GetCustomAttributes();

							var apiDomain = ConvertDomain2Enum(ApiDomain);
							var domainSelected = string.IsNullOrEmpty(selectedDomain) ? OpenApiManifestDomain.NotSet : ConvertDomain2Enum(selectedDomain);
							var typeSelected = string.IsNullOrEmpty(selectedType) ? OpenApiManifestType.NotSet : ConvertApiType2Enum(selectedType);
							if (apiDomain != OpenApiManifestDomain.NotSet && !string.IsNullOrEmpty(apiName) && !string.IsNullOrEmpty(versionOnly) && !string.IsNullOrEmpty(ReleaseStatus))
							{
								//if ((string.IsNullOrEmpty(selectedDomain) || apiDomain == domainSelected) && (string.IsNullOrEmpty(selectedType) || typeSelected == ApiType))
								if (string.IsNullOrEmpty(selectedType) || typeSelected == ApiType)
								{
									//var openApiManifest = openApiManifestDocs.Where(api => api.ApiName == apiName && api.Version == versionOnly && api.ApiType == ApiType && api.Domain == apiDomain).FirstOrDefault();
									var openApiManifest = openApiManifestDocs.Where(api => api.ApiName == apiName && api.Version == versionOnly && api.ApiType == ApiType).FirstOrDefault();
									if (openApiManifest == null)
									{
										openApiManifest = new OpenApiManifest();
									}
									else
									{
										openApiManifestDocs.Remove(openApiManifest);
									}
									// Update the Manifest Document
									openApiManifest.ApiName = apiName;
									openApiManifest.Version = versionOnly;
									openApiManifest.Release = ConvertReleaseStatus2Enum(ReleaseStatus);
									openApiManifest.Status = OpenApiManifestStatus.Publish;
									openApiManifest.Domain = apiDomain;
									// openApiManifest.ReleaseEnvironent = string.Concat(GetWebAppRoot(), "/metadata/");
									openApiManifest.ReleaseEnvironent = "manual";
									openApiManifest.ApiType = ApiType;
									openApiManifest.ApiOwner = "colleague";
									openApiManifest.RouteSource = routeSource;

									openApiManifestDocs.Add(openApiManifest);
								}
							}
						}
					}
					catch (Exception)
					{
						continue;  // Skip to next Controller
					}
				}
				catch (Exception ex)
				{
					var e = ex.Message;
					//no need to throw since not all routes will have _customMediaTypes field
				}
			}

			return openApiManifestDocs;
		}

		private string GetWebAppRoot()
		{
			var addresses = server.Features.Get<IServerAddressesFeature>().Addresses;
			var url = new Uri(addresses.First());
			string host = (url.IsDefaultPort) ?
				url.Host :
				url.Authority;
			host = String.Format("{0}://{1}", url.Scheme, host);
			if (url.AbsolutePath == "/")
				return host;
			else
				return host + Request.PathBase;
		}

		/// <summary>
		/// Returns OpenAPI documentation in Extended data format for Pro-Code APIs
		/// </summary>
		/// <param name="httpRoutes"></param>
		/// <param name="selectedResource"></param>
		/// <param name="bypassCache"></param>
		/// <returns></returns>
		private async Task<List<OpenApiDocument>> BuildApiConfigurationFromRoutesAsync(IEnumerable<RouteEndpoint> httpRoutes, string selectedResource, bool bypassCache = false)
		{
			EthosApiConfiguration apiConfiguration = new EthosApiConfiguration();
			List<OpenApiDocument> openApiDocs = new List<OpenApiDocument>();

			string[] headerVersionConstraintValue = null;

			Assembly asm = Assembly.GetExecutingAssembly();

			var controlleractionlist = asm.GetTypes()
						.Where(tt => typeof(BaseCompressedApiController).IsAssignableFrom(tt))
						.SelectMany(tt => tt.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
						.Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
						.Select(x => new
						{
							Controller = x.DeclaringType.Name,
							Action = x.Name,
							x.DeclaringType,
						})
						.OrderBy(x => x.Controller).ThenBy(x => x.Action).ToList();

			apiConfiguration.HttpMethods = new List<EthosApiSupportedMethods>();
			Dictionary<string, List<string>> mediaTypeMethodsSupported = new Dictionary<string, List<string>>();

			foreach (RouteEndpoint httpRoute in httpRoutes)
			{
				apiConfiguration.ResourceName = selectedResource;
				apiConfiguration.ReleaseStatus = "R";
				apiConfiguration.ApiDomain = "";
				apiConfiguration.ApiType = "Web-NonEthos";
				apiConfiguration.PageLimit = PageLimit;
				OpenApiDocument openApiDocument = new OpenApiDocument();
				queryNamesDtoTypes = new Dictionary<string, Type>();
				queryNamesVersionConfigs = new Dictionary<string, Domain.Base.Entities.EthosExtensibleData>();
				try
				{
					//Get the route template
					var routeTemplate = !string.IsNullOrEmpty(httpRoute.RoutePattern.RawText) ? httpRoute.RoutePattern.RawText : selectedResource;
					var apiName = string.Empty;

					//gets api name
					apiName = GetApiName(routeTemplate);

					//Allowed http method
					var allowedMethod = string.Empty;

					//get all constraints
					var constraints = from m in httpRoute.Metadata
									  where m is HttpMethodAttribute
									  select (HttpMethodAttribute)m;
					allowedMethod = (from c in constraints
									 select c.HttpMethods.FirstOrDefault()).FirstOrDefault() ?? string.Empty;

					if (!string.IsNullOrEmpty(allowedMethod))
					{
						var routeSplit = routeTemplate.Split('/');
						if (routeSplit.Count() == 1)
						{
							if (allowedMethod != "POST" && allowedMethod != "PUT" && allowedMethod != "DELETE")
								allowedMethod = "GET_ALL";
						}
						else if (routeSplit.Count() >= 2)
						{
							if (routeSplit[0].Equals("qapi", StringComparison.OrdinalIgnoreCase))
							{
								allowedMethod = "QAPI_POST";
							}
							else
							{
								if (allowedMethod == "GET" && routeTemplate.Contains('{'))
									allowedMethod = "GET_ID";
								else if (allowedMethod == "GET")
									allowedMethod = "GET_ALL";
							}
						}
					}
					else
					{
						continue;
					}

					var headerVersionInfo = httpRoute.Metadata.FirstOrDefault(m => m is HeaderVersionRouteAttribute) as HeaderVersionRouteAttribute;

					var tempXMediaType = string.Empty;
					var versionOnly = string.Empty;
					if (headerVersionInfo != null)
					{
						headerVersionConstraintValue = headerVersionInfo.MediaTypes?.ToArray() ?? Array.Empty<string>();
						if (headerVersionInfo.IsEthosEnabled || headerVersionInfo.IsEedmSupported)
						{
							if (headerVersionInfo.IsEedmSupported)
								apiConfiguration.ApiType = "Ethos";
							else if (headerVersionInfo.IsEthosEnabled)
								apiConfiguration.ApiType = "Web-Ethos";
							tempXMediaType = string.Format("application/vnd.hedtech.integration.v{0}+json", headerVersionInfo.RouteVersion);
						}
						versionOnly = headerVersionInfo.RouteVersion;
					}

					if (headerVersionConstraintValue != null && headerVersionConstraintValue.Any())
					{
						tempXMediaType = headerVersionConstraintValue[0];
						if (tempXMediaType.Contains("/"))
							tempXMediaType = tempXMediaType.Split('/')[1];
					}
					else
					{
						if (string.IsNullOrEmpty(tempXMediaType) && headerVersionInfo != null && !string.IsNullOrEmpty(headerVersionInfo.RouteVersion))
						{
							tempXMediaType = string.Format("application/vnd.ellucian.v{0}+json", headerVersionInfo.RouteVersion);
						}
					}

					if (!string.IsNullOrEmpty(tempXMediaType))
					{
						versionOnly = ExtractVersionNumberOnly(tempXMediaType);
					}

					// If we don't have a version, then skip it.
					if (string.IsNullOrEmpty(versionOnly))
					{
						continue;
					}

					// Skip any obsolete bulk-requests for which we don't want to document.
					if (tempXMediaType.Contains("bulk-requests"))
					{
						continue;
					}

					// If this has an Ethos header and is identified as Legacy, but it's not Ethos Enabled
					// then it may be an end-point that is not supported and should not be included.
					if (tempXMediaType.Contains("vnd.hedtech.integration") && (apiConfiguration.ApiType == "Legacy" || apiConfiguration.ApiType == "Web-NonEthos"))
					{
						continue;
					}

					var alternateView = string.Empty;
					if (!tempXMediaType.Contains("integration.v") && !tempXMediaType.Contains("ellucian.v"))
					{
						var mediaTypeSplit = tempXMediaType.Split(".").ToList();
						var mediaTypeSplitCount = mediaTypeSplit.Count();
						var index = mediaTypeSplit.IndexOf("integration");
						if (index > 0 && index < (mediaTypeSplitCount - 1)) alternateView = mediaTypeSplit.ElementAt(index + 1);
						else
						{
							alternateView = mediaTypeSplit.ElementAt(1);
							if (alternateView.StartsWith("ellucian-")) alternateView = alternateView.Substring(9);
						}
					}
					apiConfiguration.ResourceName = apiName;
					apiConfiguration.ParentResourceName = alternateView;

					object controller = string.Empty;
					object action = string.Empty;
					object requestedContentType = string.Empty;

					httpRoute.RoutePattern.Defaults.TryGetValue("action", out action);
					httpRoute.RoutePattern.Defaults.TryGetValue("controller", out controller);

					var controlleraction = controlleractionlist
						.FirstOrDefault(x => x.Controller == string.Concat(controller.ToString(), "Controller"));
					var controllerType = controlleraction.DeclaringType;

					var routeAction = action.ToString();

					if (controllerType != null)
					{
						//apiConfiguration.Description = controllerType.GetDocumentation();
						// Get information about the schema from the controller attribute SchemasAttribute
						// and update the configuration with this data.
						var controllerCustomAttributes = controllerType.GetCustomAttributes();
						//var controllerSchemaAttributes = controllerCustomAttributes.Where(mi => mi.GetType().Name == "MetadataAttribute");
						var schemaMetaData = controllerType.GetDocumentation();
						if (!string.IsNullOrEmpty(schemaMetaData.ApiDescription)) apiConfiguration.Description = schemaMetaData.ApiDescription;
						if (!string.IsNullOrEmpty(schemaMetaData.ApiDomain)) apiConfiguration.ApiDomain = schemaMetaData.ApiDomain;

						// Derive domain from module code if it hasn't been defined by the SchemaAttribute
						if (string.IsNullOrEmpty(apiConfiguration.ApiDomain))
						{
							var controllerLicenseModule = controllerCustomAttributes.Where(mi => mi.GetType().Name == "EllucianLicenseModuleAttribute");
							if (controllerLicenseModule != null && controllerLicenseModule.Any())
							{
								foreach (EllucianLicenseModuleAttribute schemaData in controllerLicenseModule)
								{
									if (string.IsNullOrEmpty(apiConfiguration.ApiDomain))
									{
										if (!string.IsNullOrEmpty(schemaData.ModuleCode))
										{
											apiConfiguration.ApiDomain = ConvertModuleToDomain(schemaData.ModuleCode);
										}
									}
								}
							}
						}

						MethodInfo method = controllerType.GetMethods().FirstOrDefault(m => m.Name == routeAction);
						if (method == null)
						{
							// Try to find the method with "Async" appended to the end since 'routeAction' may not have it.
							if (!routeAction.EndsWith("Async"))
							{
								routeAction += "Async";
								method = controllerType.GetMethods().FirstOrDefault(m => m.Name == routeAction);
							}
						}

						if (method == null)
						{
							// If we can't find the method in the controller then we can't build the version record at all.
							// we won't have any properties so continue to next method defined.
							continue;
						}

						var obsoleteMessage = string.Empty;
						if (method.GetCustomAttribute<ObsoleteAttribute>() != null)
						{
							obsoleteMessage = method.GetCustomAttribute<ObsoleteAttribute>().Message;
							if (string.IsNullOrEmpty(obsoleteMessage)) obsoleteMessage = "Obsolete version, use a more recent version instead.";
							obsoleteMessage = string.Concat("<b>Warning:</b> ", obsoleteMessage);
						}

						requestedContentTypes = new List<string>();
						var customRouteAttributes = method.GetCustomAttributes();
						foreach (var customRouteAttribute in customRouteAttributes)
						{
							if (customRouteAttribute.GetType().Name == "TypeFilterAttribute")
							{
								var attr = (TypeFilterAttribute)customRouteAttribute;
								if (attr != null)
								{
									var implAttr = attr.ImplementationType;
									if (implAttr.Name == "PagingFilter")
									{
										allowedMethod = "GET_ALL";
										var ignorePaging = (bool)attr.Arguments[0];
										if (!apiConfiguration.PageLimit.HasValue)
											apiConfiguration.PageLimit = (int)attr.Arguments[1];
									}
									else if (implAttr.Name == "FilteringFilter")
									{
										allowedMethod = "GET_ALL";
										ignoreDefaultFiltering = (bool)attr.Arguments[0];
									}
									else if (implAttr.Name == "PermissionsFilter")
									{
										permissionsCollection = (string[])attr.Arguments[0];
									}
								}
							}
							else if (customRouteAttribute.GetType().Name == "QueryStringFilterFilter")
							{
								allowedMethod = "GET_ALL";
								var attr = (QueryStringFilterFilter)customRouteAttribute;
								if (attr != null)
								{
									var dictKey = attr.FilterGroupName;
									var dictValue = attr.FilterType;
									if (!queryNamesDtoTypes.ContainsKey(dictKey))
									{
										queryNamesDtoTypes.Add(dictKey, dictValue);
									}
								}
							}
							else if (customRouteAttribute.GetType().Name == "ContentTypeConstraint")
							{
								var attr = (ContentTypeConstraint)customRouteAttribute;
								if (attr != null)
								{
									requestedContentTypes.AddRange(attr.SupportedContentTypes);
								}
							}
						}

						// Get Page Liimit from Filter attached to the controller method
						if (allowedMethod.Equals("GET_ALL"))
						{
							if (customRouteAttributes != null && customRouteAttributes.Any(mi => mi.GetType().Name == "PagingFilter"))
							{
								if (!apiConfiguration.PageLimit.HasValue)
									apiConfiguration.PageLimit = ((PagingFilter)customRouteAttributes.First(mi => mi.GetType().Name == "PagingFilter")).DefaultLimit;
							}
						}

						// Get Permissions Codes requirements
						if (customRouteAttributes != null && customRouteAttributes.Any(mi => mi.GetType().Name == "PermissionsFilter"))
						{
							permissionsCollection = ((PermissionsFilter)customRouteAttributes.First(mi => mi.GetType().Name == "PermissionsFilter")).PermissionsCollection;
						}

						// Get Default FilteringFilter value
						if (customRouteAttributes != null && customRouteAttributes.Any(mi => mi.GetType().Name == "FilteringFilter"))
						{
							ignoreDefaultFiltering = ((FilteringFilter)customRouteAttributes.First(mi => mi.GetType().Name == "FilteringFilter")).IgnoreFiltering;
						}

						// Get Default FilteringFilter value
						useV2errors = false;
						if (customRouteAttributes != null && customRouteAttributes.Any(mi => mi.GetType().Name == "CustomMediaTypeAttributeFilter"))
						{
							var errorContentType = ((CustomMediaTypeAttributeFilter)customRouteAttributes.First(mi => mi.GetType().Name == "CustomMediaTypeAttributeFilter")).ErrorContentType;
							if (errorContentType == IntegrationErrors2)
							{
								useV2errors = true;
							}
						}

						// If the specific route has been defined as Ethos Enabled, then always use v2 error messages.
						if (headerVersionInfo != null && headerVersionInfo.IsEthosEnabled)
						{
							useV2errors = true;
						}

						// Now get information about the schema from the specific controller method attribute SchemasAttribute
						// and update the configuration data and version data.
						string versionDescription = string.Empty;
						string versionReleaseStatus = string.Empty;
						string permission = string.Empty, description = string.Empty, summary = string.Empty, methodReturns = string.Empty, licenseName = string.Empty, note = string.Empty;
						List<string> exceptions = new List<string>();
						Dictionary<string, string> arguments = new Dictionary<string, string>();

						var methodMetadata = method.GetDocumentation();
						if (methodMetadata != null)
						{
							if (!string.IsNullOrEmpty(methodMetadata.ApiDomain)) apiConfiguration.ApiDomain = methodMetadata.ApiDomain;
							if (!string.IsNullOrEmpty(methodMetadata.ApiType)) apiConfiguration.ApiType = methodMetadata.ApiType;
							apiConfiguration.Audience = methodMetadata.Audience;
							apiConfiguration.DeprecatedOn = methodMetadata.DeprecatedOn;
							apiConfiguration.SunsetOn = methodMetadata.SunsetOn;
							if (!string.IsNullOrEmpty(methodMetadata.ApiDescription)) versionDescription = methodMetadata.ApiDescription;
							if (!string.IsNullOrEmpty(methodMetadata.ApiVersionStatus)) versionReleaseStatus = methodMetadata.ApiVersionStatus;
							if (!string.IsNullOrEmpty(methodMetadata.HttpMethodPermission)) permission = methodMetadata.HttpMethodPermission;
							if (!string.IsNullOrEmpty(methodMetadata.HttpMethodSummary)) summary = methodMetadata.HttpMethodSummary;
							if (!string.IsNullOrEmpty(methodMetadata.HttpMethodReturns)) methodReturns = methodMetadata.HttpMethodReturns;
							if (!string.IsNullOrEmpty(methodMetadata.HttpMethodDescription)) description = methodMetadata.HttpMethodDescription;
							if (!string.IsNullOrEmpty(methodMetadata.LicenseName)) licenseName = methodMetadata.LicenseName;
							if (!string.IsNullOrEmpty(methodMetadata.Note)) note = methodMetadata.Note;
							if (methodMetadata.HttpMethodArguments != null && methodMetadata.HttpMethodArguments.Any()) arguments = methodMetadata.HttpMethodArguments;
							if (methodMetadata.HttpMethodExceptions != null && methodMetadata.HttpMethodExceptions.Any()) exceptions = methodMetadata.HttpMethodExceptions;
						}

						var parameters = httpRoute.RoutePattern.Parameters;
						foreach (var param in parameters)
						{
							if (!arguments.ContainsKey(param.Name))
								arguments.Add(param.Name, "An Identifier for the resource or other required parameter used for resource designation.");
						}

						if (!string.IsNullOrEmpty(obsoleteMessage))
						{
							description = string.Concat(obsoleteMessage, Environment.NewLine, Environment.NewLine, description);
						}
						if (!string.IsNullOrEmpty(licenseName))
						{
							description = string.Concat(description, Environment.NewLine, Environment.NewLine, "<b>License Name</b>", Environment.NewLine, Environment.NewLine, licenseName);
						}
						if (!string.IsNullOrEmpty(note))
						{
							description = string.Concat(description, Environment.NewLine, Environment.NewLine, "<b>Note:</b>", Environment.NewLine, Environment.NewLine, note);
						}

						apiConfiguration.HttpMethods = new List<EthosApiSupportedMethods>()
						{
							new EthosApiSupportedMethods(allowedMethod, permission, description, summary, routeTemplate, arguments, exceptions, methodReturns)
						};

						// Keep a dictionary of media types supported by the different routes selected.
						var xMediaTypeKey = string.Concat(routeTemplate, "+", tempXMediaType);
						if (!string.IsNullOrEmpty(xMediaTypeKey))
						{
							if (mediaTypeMethodsSupported.ContainsKey(xMediaTypeKey))
							{
								mediaTypeMethodsSupported[xMediaTypeKey].Add(allowedMethod);
							}
							else
							{
								mediaTypeMethodsSupported.Add(xMediaTypeKey, new List<string>() { allowedMethod });
							}
						}

						var apiVersionConfiguration = new Domain.Base.Entities.EthosExtensibleData(apiName, versionOnly, tempXMediaType, routeTemplate, "")
						{
							Description = versionDescription,
							VersionReleaseStatus = versionReleaseStatus,
							InquiryFields = new List<string>()
						};

						var requestApiVersionConfig = new Domain.Base.Entities.EthosExtensibleData(apiName, versionOnly, tempXMediaType, routeTemplate, "")
						{
							Description = versionDescription,
							VersionReleaseStatus = versionReleaseStatus,
							InquiryFields = new List<string>()
						};

						var apiVersionConfig = GetVersionPropertiesForApiResponse(method, apiVersionConfiguration);
						apiVersionConfig = await CopyDataRowToFilterRow(apiVersionConfig, apiName, versionOnly);
						if (allowedMethod.StartsWith("QAPI") || allowedMethod.StartsWith("PUT") || allowedMethod.StartsWith("POST"))
						{
							requestApiVersionConfig = GetVersionPropertiesForApiRequest(method, requestApiVersionConfig);
							requestApiVersionConfig = await CopyDataRowToFilterRow(requestApiVersionConfig, apiName, versionOnly);
						}
						if (queryNamesDtoTypes != null && queryNamesDtoTypes.Any())
						{
							foreach (var dictItem in queryNamesDtoTypes)
							{
								var dictKey = dictItem.Key;
								var dictValue = dictItem.Value;
								var dtoTypeNameInput = dictValue.AssemblyQualifiedName;
								if (!string.IsNullOrEmpty(dtoTypeNameInput))
								{
									try
									{
										var tempApiVersionConfig = new Domain.Base.Entities.EthosExtensibleData(apiName, versionOnly, tempXMediaType, routeTemplate, "")
										{
											Description = versionDescription,
											VersionReleaseStatus = versionReleaseStatus,
											InquiryFields = new List<string>()
										};
										var filterNames = new List<string>();
										tempApiVersionConfig = GetVersionPropertiesFromDto(dtoTypeNameInput, tempApiVersionConfig, true, filterNames);
										tempApiVersionConfig = await CopyDataRowToFilterRow(tempApiVersionConfig, selectedResource, versionOnly);

										if (tempApiVersionConfig != null && tempApiVersionConfig.ExtendedDataFilterList != null && tempApiVersionConfig.ExtendedDataFilterList.Any())
										{
											queryNamesVersionConfigs.Add(dictKey, tempApiVersionConfig);
										}
									}
									catch (Exception ex)
									{
										var e = ex.Message;
										//no need to throw since not all routes will have _customMediaTypes field
									}
								}
							}
						}

						if (apiVersionConfig != null && apiVersionConfig.ExtendedDataList != null && apiVersionConfig.ExtendedDataList.Any())
						{
							var openApiDoc = openApiDocs.Where(api =>
							{
								//bool matchingDomain = false;
								bool matchingType = false;
								bool matchingVersion = false;
								if (api.Info != null && api.Info.Extensions != null)
								{
									OpenApiString openApiDomain = (api.Info.Extensions.FirstOrDefault(dict => dict.Key == "x-source-domain").Value as OpenApiString);
									OpenApiString openApiType = (api.Info.Extensions.FirstOrDefault(dict => dict.Key == "x-api-type").Value as OpenApiString);
									//if (openApiDomain != null)
									//    matchingDomain = (openApiDomain.Value.Replace(" ", "").ToLower() == ConvertDomain2Enum(apiConfiguration.ApiDomain).ToString().ToLower());
									if (openApiType != null)
										matchingType = (openApiType.Value.ToLower() == apiConfiguration.ApiType.ToLower());
									if (!string.IsNullOrEmpty(api.Info.Version))
										matchingVersion = (api.Info.Version == apiVersionConfig.ApiVersionNumber);
								}
								//return (matchingDomain && matchingType && matchingVersion && (api.Info != null && api.Info.Title.Equals(apiName, StringComparison.OrdinalIgnoreCase)));
								return (matchingType && matchingVersion && (api.Info != null && api.Info.Title.Equals(apiName, StringComparison.OrdinalIgnoreCase)));

							}).FirstOrDefault();

							if (openApiDoc == null)
							{
								openApiDoc = new OpenApiDocument();
								//build servers object
								openApiDoc.Servers = BuildOpenApiServersProperty(apiConfiguration);
							}
							else
							{
								openApiDocs.Remove(openApiDoc);
							}

							//build or update the info oject
							openApiDoc.Info = UpdateOpenApiInfoProperty(apiConfiguration, apiVersionConfig, openApiDoc.Info);
							if (!string.IsNullOrEmpty(licenseName))
								openApiDoc.Info.License = new OpenApiLicense() { Name = licenseName, Url = new Uri("https://www.ellucian.com/privacy") };
							else
							{
								var year = "2023-" + DateTime.Today.Year.ToString();
								openApiDoc.Info.License = new OpenApiLicense() { Name = $"© {year} Ellucian Company L.P. and its affiliates. All rights reserved.", Url = new Uri("https://www.ellucian.com/privacy") };
							}

							//build paths object
							openApiDoc = BuildOpenApiPathPropertyFromRoute(apiConfiguration, apiVersionConfig, requestApiVersionConfig, allowedMethod, openApiDoc);

							//add components to the document
							var components = BuildOpenApiComponentsProperty(apiConfiguration, apiVersionConfig, requestApiVersionConfig);
							var existingComponents = openApiDoc.Components;
							if (existingComponents == null)
								existingComponents = new OpenApiComponents();
							if (components != null && components.Schemas != null)
							{
								foreach (var component in components.Schemas)
								{
									var schemasKey = component.Key;
									if (!existingComponents.Schemas.ContainsKey(schemasKey))
										existingComponents.Schemas.Add(schemasKey, component.Value);
								}
							}
							if (components != null && components.SecuritySchemes != null)
							{
								foreach (var component in components.SecuritySchemes)
								{
									var schemasKey = component.Key;
									if (!existingComponents.SecuritySchemes.ContainsKey(schemasKey))
										existingComponents.SecuritySchemes.Add(schemasKey, component.Value);
								}
							}
							openApiDoc.Components = existingComponents;

							openApiDocs.Add(openApiDoc);
						}
					}
				}
				catch (Exception ex)
				{
					var e = ex.Message;
					//no need to throw since not all routes will have _customMediaTypes field
				}
			}

			return openApiDocs;
		}

		/// <summary>
		/// Copy data from the Data List to the Filter List for pro-code APIs.
		/// </summary>
		/// <param name="apiVersionConfig"></param>
		/// <param name="selectedResource"></param>
		/// <param name="versionOnly"></param>
		/// <returns></returns>
		private async Task<Domain.Base.Entities.EthosExtensibleData> CopyDataRowToFilterRow(Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string selectedResource, string versionOnly)
		{
			if (apiVersionConfig != null && apiVersionConfig.ExtendedDataList != null && apiVersionConfig.ExtendedDataList.Any())
			{
				// Only get Extended data if we have either id or code in the schema
				var idColumn = apiVersionConfig.ExtendedDataList.FirstOrDefault(ed => ed.JsonTitle.Equals("id", StringComparison.OrdinalIgnoreCase) ||
					ed.JsonTitle.Equals("code", StringComparison.OrdinalIgnoreCase));
				if (idColumn != null)
				{
					// Get description, max length, table name, reference file, and reference field from run-time CDD if missing
					apiVersionConfig.ExtendedDataList = (await _ethosApiBuilderService.GetExtendedEthosDataRowDefault(apiVersionConfig.ExtendedDataList.ToList())).ToList();

					// Merge any extension data into the apiVersionConfigs
					EthosResourceRouteInfo routeInfo = new EthosResourceRouteInfo()
					{
						ResourceName = selectedResource
					};
					var mergeApiVersionConfigs = await _ethosApiBuilderService.GetExtendedEthosVersionsConfigurationsByResource(routeInfo, false, false);
					if (mergeApiVersionConfigs != null)
					{
						foreach (var mergeConfig in mergeApiVersionConfigs)
						{
							if (string.IsNullOrEmpty(mergeConfig.ApiVersionNumber) || mergeConfig.ApiVersionNumber == versionOnly || mergeConfig.ApiVersionNumber == versionOnly.Split('.')[0])
							{
								foreach (var dataRow in mergeConfig.ExtendedDataList)
								{
									apiVersionConfig.AddItemToExtendedData(dataRow);
								}
								foreach (var inquiryColumn in mergeConfig.InquiryFields)
								{
									apiVersionConfig.InquiryFields.Add(inquiryColumn);
								}
							}
						}
					}
				}

				// Update inquiry only fields for invalid database usage for request
				foreach (var dataRow in apiVersionConfig.ExtendedDataList)
				{
					if (dataRow.DatabaseUsageType == "I" || string.IsNullOrEmpty(dataRow.DatabaseUsageType))
					{
						if (!string.IsNullOrEmpty(dataRow.ColleagueColumnName))
						{
							apiVersionConfig.InquiryFields.Add(dataRow.ColleagueColumnName);
						}
					}

					// Add Filter List for QAPI and GET all
					var filterRow = new Domain.Base.Entities.EthosExtensibleDataFilter(dataRow.ColleagueColumnName, dataRow.ColleagueFileName, dataRow.JsonTitle,
						dataRow.JsonPath, dataRow.JsonPropertyType, new List<string>(), dataRow.ColleaguePropertyLength);

					filterRow.DatabaseUsageType = dataRow.DatabaseUsageType;
					filterRow.Required = dataRow.Required;
					filterRow.SelectFileName = dataRow.ColleagueFileName;
					filterRow.TransColumnName = dataRow.TransColumnName;
					filterRow.TransFileName = dataRow.TransFileName;
					filterRow.TransTableName = dataRow.TransTableName;
					filterRow.Enumerations = dataRow.Enumerations;

					apiVersionConfig.AddItemToExtendedDataFilter(filterRow);
				}
			}

			return apiVersionConfig;
		}

		/// <summary>
		/// Extract the version number from a customMediaType.  Extracts integers or semantic versions.
		/// </summary>
		/// <param name="original"></param>
		/// <returns>Version number.  May contain none, or unknown number of decimals</returns>
		private string ExtractVersionNumberOnly(string original)
		{
			var regex = new Regex(@"(?:(\d+)\.)?(?:(\d+)\.)?(?:(\d+)\.\d+)|(?:(\d+))", RegexOptions.Compiled);
			Match semanticVersion = regex.Match(original);
			if (semanticVersion.Success)
			{
				return semanticVersion.Value;
			}
			else return string.Empty;
		}

		/// <summary>
		/// Convert the module code designation to an API Domain code.
		/// </summary>
		/// <param name="moduleCode"></param>
		/// <returns></returns>
		private string ConvertModuleToDomain(string moduleCode)
		{
			string domain = "CORE";
			switch (moduleCode)
			{
				case ModuleConstants.Base:
					{
						domain = "CORE";
						break;
					}
				case ModuleConstants.Student:
					{
						domain = "ST";
						break;
					}
				case ModuleConstants.Planning:
					{
						domain = "ST";
						break;
					}
				case ModuleConstants.Finance:
					{
						domain = "CF";
						break;
					}
				case ModuleConstants.FinancialAid:
					{
						domain = "FA";
						break;
					}
				case ModuleConstants.ColleagueFinance:
					{
						domain = "CF";
						break;
					}
				case ModuleConstants.ResidenceLife:
					{
						domain = "CORE";
						break;
					}
				case ModuleConstants.HumanResources:
					{
						domain = "HR";
						break;
					}
				case ModuleConstants.ProjectsAccounting:
					{
						domain = "CF";
						break;
					}
				case ModuleConstants.CampusOrgs:
					{
						domain = "CORE";
						break;
					}
				case ModuleConstants.TimeManagement:
					{
						domain = "HR";
						break;
					}
				case ModuleConstants.FALink:
					{
						domain = "FA";
						break;
					}
				case ModuleConstants.BudgetManagement:
					{
						domain = "CF";
						break;
					}
			}
			return domain;
		}

		/// <summary>
		/// Convert the api type string to an API manifest type enumeration.
		/// </summary>
		/// <param name="releaseType"></param>
		/// <returns></returns>
		private OpenApiManifestType ConvertApiType2Enum(string releaseType)
		{
			OpenApiManifestType openApiManifestType;
			switch (releaseType.ToLower())
			{
				case "legacy":
					openApiManifestType = OpenApiManifestType.Web;
					break;
				case "ethos":
					openApiManifestType = OpenApiManifestType.Ethos;
					break;
				case "bpa":
					openApiManifestType = OpenApiManifestType.BusinessProcess;
					break;
				case "specification":
					openApiManifestType = OpenApiManifestType.Specification;
					break;
				case "web-ethos":
					openApiManifestType = OpenApiManifestType.EthosEnabled;
					break;
				default:
					openApiManifestType = OpenApiManifestType.Web;
					break;
			}
			return openApiManifestType;
		}

		/// <summary>
		/// Convert the api type enumeration to an API manifest type string.
		/// </summary>
		/// <param name="releaseType"></param>
		/// <returns></returns>
		private string ConvertApiTypeFromEnum(OpenApiManifestType releaseType)
		{
			string openApiManifestType = "web";
			switch (releaseType)
			{
				case OpenApiManifestType.Web:
					openApiManifestType = "web-nonethos";
					break;
				case OpenApiManifestType.Ethos:
					openApiManifestType = "ethos";
					break;
				case OpenApiManifestType.BusinessProcess:
					openApiManifestType = "bus-proc";
					break;
				case OpenApiManifestType.Specification:
					openApiManifestType = "specification";
					break;
				case OpenApiManifestType.EthosEnabled:
					openApiManifestType = "web-ethos";
					break;
				default:
					openApiManifestType = "web-nonethos";
					break;
			}
			return openApiManifestType;
		}

		/// <summary>
		/// Convert the api status string to an API Release Status enumeration.
		/// </summary>
		/// <param name="releaseStatus"></param>
		/// <returns></returns>
		private OpenApiReleaseStatus ConvertReleaseStatus2Enum(string releaseStatus)
		{
			OpenApiReleaseStatus openApiReleaseStatus;
			switch (releaseStatus.ToLower())
			{
				case "beta":
					openApiReleaseStatus = OpenApiReleaseStatus.Beta;
					break;
				case "select":
					openApiReleaseStatus = OpenApiReleaseStatus.Select;
					break;
				default:
					openApiReleaseStatus = OpenApiReleaseStatus.GeneralAvailability;
					break;
			}
			return openApiReleaseStatus;
		}

		/// <summary>
		/// Convert the api status enumeration to an API Release Status string.
		/// </summary>
		/// <param name="releaseStatusEnumeration"></param>
		/// <returns></returns>
		private string ConvertReleaseStatusFromEnum(OpenApiReleaseStatus releaseStatusEnumeration)
		{
			string releaseStatus = "ga";
			switch (releaseStatusEnumeration)
			{
				case OpenApiReleaseStatus.Beta:
					releaseStatus = "beta";
					break;
				case OpenApiReleaseStatus.Select:
					releaseStatus = "select";
					break;
				default:
					releaseStatus = "ga";
					break;
			}
			return releaseStatus;
		}

		/// <summary>
		/// Convert the domain string designation to an API Domain enumeration.
		/// </summary>
		/// <param name="domainString"></param>
		/// <returns></returns>
		private OpenApiManifestDomain ConvertDomain2Enum(string domainString)
		{
			OpenApiManifestDomain domain = OpenApiManifestDomain.Foundation;
			switch (domainString.ToLower())
			{
				case "core":
					domain = OpenApiManifestDomain.Foundation;
					break;
				case "st":
					domain = OpenApiManifestDomain.Student;
					break;
				case "cf":
					domain = OpenApiManifestDomain.Finance;
					break;
				case "fa":
					domain = OpenApiManifestDomain.FinancialAid;
					break;
				case "hr":
					domain = OpenApiManifestDomain.HumanResources;
					break;
				case "foundation":
					domain = OpenApiManifestDomain.Foundation;
					break;
				case "student":
					domain = OpenApiManifestDomain.Student;
					break;
				case "finance":
					domain = OpenApiManifestDomain.Finance;
					break;
				case "financial aid":
					domain = OpenApiManifestDomain.FinancialAid;
					break;
				case "financialaid":
					domain = OpenApiManifestDomain.FinancialAid;
					break;
				case "human resources":
					domain = OpenApiManifestDomain.HumanResources;
					break;
				case "humanresources":
					domain = OpenApiManifestDomain.HumanResources;
					break;
			}
			return domain;
		}

		/// <summary>
		/// Convert the domain enumeration designation to an API Domain string.
		/// </summary>
		/// <param name="domainEnumeration"></param>
		/// <returns></returns>
		private string ConvertDomainFromEnum(OpenApiManifestDomain domainEnumeration)
		{
			string domain = "Foundation";
			switch (domainEnumeration)
			{
				case OpenApiManifestDomain.Foundation:
					domain = "Foundation";
					break;
				case OpenApiManifestDomain.Student:
					domain = "Student";
					break;
				case OpenApiManifestDomain.Finance:
					domain = "Finance";
					break;
				case OpenApiManifestDomain.FinancialAid:
					domain = "Financial Aid";
					break;
				case OpenApiManifestDomain.HumanResources:
					domain = "Human Resources";
					break;
				default:
					domain = domainEnumeration.ToString();
					break;
			}
			return domain;
		}

		/// <summary>
		/// Get type type name of the HTTP Request
		/// </summary>
		/// <param name="mi"></param>
		/// <param name="apiVersionConfig"></param>
		/// <returns>The Type Name for the request</returns>
		private Domain.Base.Entities.EthosExtensibleData GetVersionPropertiesForApiRequest(MethodInfo mi, Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
		{
			var dtoTypeNameInput = string.Empty;
			apiVersionConfig.ExtendedDataList = new List<Domain.Base.Entities.EthosExtensibleDataRow>();

			// get type name
			Type type = null;
			var customParameters = mi.GetParameters();
			if (customParameters != null)
			{
				foreach (var parameter in customParameters)
				{
					Type parameterType = parameter.ParameterType;
					if (parameterType != null)
					{
						if (parameterType.IsArray || parameterType.Name == "IEnumerable`1" || parameterType.Name == "IList`1" ||
							parameterType.Name == "ICollection`1" || parameterType.Name == "JContainer" ||
							parameterType.Name == "JObject" || parameterType.Name == "JToken")
						{
							type = parameterType.GetGenericArguments().FirstOrDefault();
							if (type.Name.Equals("string", StringComparison.OrdinalIgnoreCase))
							{
								dtoTypeNameInput = parameterType.Name;
								type = null;
							}
						}
						else if (!parameterType.Name.Equals("string", StringComparison.OrdinalIgnoreCase) && !parameterType.IsValueType)
						{
							type = parameterType;
						}
					}
				}
			}

			bool useCamelCase = false;
			if (type != null)
			{
				var customAttributes = mi.GetCustomAttributes();
				var ethosEnabledFilter = customAttributes.Where(ca => ca.GetType().Name == "EthosEnabledFilter");
				if (ethosEnabledFilter != null)
				{
					foreach (var attr in ethosEnabledFilter)
					{
						var filterType = (EthosEnabledFilter)attr;
						useCamelCase = filterType._useCamelCase;
					}
				}

				dtoTypeNameInput = type.AssemblyQualifiedName;
			}

			if (!string.IsNullOrEmpty(dtoTypeNameInput))
			{
				try
				{
					var filterNames = new List<string>();
					apiVersionConfig = GetVersionPropertiesFromDto(dtoTypeNameInput, apiVersionConfig, useCamelCase, filterNames);
				}
				catch (Exception ex)
				{
					throw;
				}

			}

			return apiVersionConfig;
		}

		/// <summary>
		/// Get the Json Schema properties
		/// </summary>
		/// <param name="mi"></param>
		/// <param name="apiVersionConfig"></param>
		/// <returns></returns>
		private Domain.Base.Entities.EthosExtensibleData GetVersionPropertiesForApiResponse(MethodInfo mi, Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
		{
			var dtoTypeAsArrayInput = false;
			var dtoTypeNameInput = string.Empty;
			apiVersionConfig.ExtendedDataList = new List<Domain.Base.Entities.EthosExtensibleDataRow>();

			// get type name and if array...
			Type type = mi.ReturnType;
			var customAttributes = mi.GetCustomAttributes();

			if (type != null)
			{
				if (type.IsArray)
				{
					type = type.GetElementType();
					dtoTypeAsArrayInput = true;
				}
				else if (type.IsGenericType && !type.IsValueType)
				{
					type = type.GetGenericArguments().SingleOrDefault();
					dtoTypeAsArrayInput = true;
				}
			}
			if (type != null && type.IsGenericType && !type.IsValueType)
			{
				type = type.GetGenericArguments().SingleOrDefault();
				if (type.IsGenericType && !type.IsValueType)
				{
					type = type.GetGenericArguments().SingleOrDefault();
				}
			}

			if (type.Name == "IHttpActionResult" || type.Name == "IActionResult")
			{
				var queryStringFilter = customAttributes.Where(ca => ca.GetType().Name == "QueryStringFilterFilter");
				if (queryStringFilter != null)
				{

					foreach (var attr in queryStringFilter)
					{
						var filterType = (QueryStringFilterFilter)attr;
						var filterGroup = filterType.FilterGroupName;
						if (filterGroup == "criteria")
						{
							type = filterType.FilterType;
						}
					}
				}
			}
			if (type != null)
			{
				bool useCamelCase = false;
				var ethosEnabledFilter = customAttributes.Where(ca => ca.GetType().Name == "EthosEnabledFilter");
				if (ethosEnabledFilter != null)
				{
					foreach (var attr in ethosEnabledFilter)
					{
						var filterType = (EthosEnabledFilter)attr;
						useCamelCase = filterType._useCamelCase;
					}
				}

				dtoTypeNameInput = type.AssemblyQualifiedName;
				if (!string.IsNullOrEmpty(dtoTypeNameInput))
				{
					try
					{
						var filterNames = new List<string>();
						apiVersionConfig = GetVersionPropertiesFromDto(dtoTypeNameInput, apiVersionConfig, useCamelCase, filterNames);
					}
					catch (Exception ex)
					{
						throw;
					}
				}
			}

			return apiVersionConfig;
		}

		/// <summary>
		/// Get the Schema from the DTO
		/// </summary>
		/// <param name="dtoTypeNameInput"></param>
		/// <param name="apiVersionConfig"></param>
		/// <param name="useCamelCase"></param>
		/// <param name="filterNames"></param>
		/// <param name="rootJsonPath"></param>
		/// <returns></returns>
		private Domain.Base.Entities.EthosExtensibleData GetVersionPropertiesFromDto(string dtoTypeNameInput, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, bool useCamelCase, List<string> filterNames, string rootJsonPath = "/")
		{
			Type dtoType = Type.GetType(dtoTypeNameInput);

			if (dtoTypeNameInput == "IEnumerable`1" || dtoTypeNameInput == "IList`1" || dtoTypeNameInput == "List`1" ||
				dtoTypeNameInput == "ICollection`1" || dtoTypeNameInput == "JContainer" ||
				dtoTypeNameInput == "JObject" || dtoTypeNameInput == "JToken" || dtoType.Name == "IActionResult")
			{
				var columnName = "";
				var fileName = "";
				var jsonPropertyName = "IEnumerable[]";
				var jsonPath = "/";
				var jsonPropertyType = "string";
				var dataDescription = "List of type string for input.";
				var dataRequired = true;

				var dataRow = new Domain.Base.Entities.EthosExtensibleDataRow(columnName, fileName, jsonPropertyName, jsonPath, jsonPropertyType, "")
				{
					Description = dataDescription,
					Required = dataRequired
				};

				if (dataRow != null)
				{
					apiVersionConfig.AddItemToExtendedData(dataRow);
				}
			}
			else
			{
				// Only allow recursive calls up to 10 nodes deep.  Just in case we run into something similar to JContainer used if falink
				if (rootJsonPath.Split("/").Count() <= 10)
				{
					var properties = dtoType.GetProperties();
					if (properties != null && properties.Any())
					{
						if (properties != null)
						{
							foreach (var orgProperty in properties)
							{
								var saveFilterNames = new List<string>();
								saveFilterNames.AddRange(filterNames);
								apiVersionConfig = GetExtensibleDataRow(dtoType, apiVersionConfig, orgProperty, useCamelCase, filterNames, rootJsonPath);
								filterNames = saveFilterNames;
							}
						}
					}
				}
			}

			return apiVersionConfig;
		}

		/// <summary>
		/// Build an EthosExtensibleDataRow for the Open API specs to work with.
		/// </summary>
		/// <param name="dtoType"></param>
		/// <param name="apiVersionConfig"></param>
		/// <param name="orgProperty"></param>
		/// <param name="useCamelCase"></param>
		/// <param name="filterNames"></param>
		/// <param name="rootJsonPath"></param>
		/// <returns></returns>
		private Domain.Base.Entities.EthosExtensibleData GetExtensibleDataRow(Type dtoType, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, PropertyInfo orgProperty, bool useCamelCase, List<string> filterNames, string rootJsonPath = "/")
		{
			PropertyInfo pi = null;
			FieldInfo fi = null;
			MemberInfo[] mi = null;
			if (orgProperty != null && !string.IsNullOrEmpty(orgProperty.Name))
			{
				pi = orgProperty;
				fi = dtoType.GetField(orgProperty.Name);
				mi = dtoType.GetMember(orgProperty.Name);
			}

			string jsonPropertyName = orgProperty.Name;
			string jsonPath = rootJsonPath;
			string jsonPropertyType = "string";
			string[] enumNames = null;
			if (pi != null)
			{
				var displayName = GetDisplayName(pi);
				if (!string.IsNullOrEmpty(displayName))
					jsonPropertyName = displayName;
			}
			else
			{
				if (mi != null && mi.Any())
				{
					var customAttributes = mi.FirstOrDefault().CustomAttributes;
					if (customAttributes != null & customAttributes.Any())
					{
						foreach (var custAttr in customAttributes)
						{
							if (custAttr != null && custAttr.NamedArguments != null)
							{
								foreach (var arg in custAttr.NamedArguments)
								{
									if (arg.MemberName == "Name")
									{
										jsonPropertyName = arg.TypedValue.Value.ToString();
									}
								}
							}
							if (custAttr != null && custAttr.ConstructorArguments != null)
							{
								foreach (var arg in custAttr.ConstructorArguments)
								{
									if (arg.ArgumentType.Name == "String")
									{
										jsonPropertyName = arg.Value.ToString();
									}
								}
							}
						}
					}
				}
			}

			Type fieldType = null;

			if (pi != null)
			{
				fieldType = pi.PropertyType;
			}
			else if (fi != null)
			{
				fieldType = fi.FieldType;
			}
			else if (mi != null && mi.Any())
			{
				var memberTypes = mi.FirstOrDefault().MemberType;

				fieldType = memberTypes.GetType();
			}
			if (fieldType != null)
			{

				if (fieldType.IsGenericType
					&& fieldType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
				{
					Type underlyingType = Nullable.GetUnderlyingType(fieldType);
					if (underlyingType == typeof(DateTime))
					{
						jsonPropertyType = GetJsonPropertyTypeForDateTime(pi);
					}
					else if (underlyingType == typeof(DateTimeOffset))
					{
						jsonPropertyType = "datetime";
					}
					else if (underlyingType == typeof(Enum) || underlyingType.BaseType == typeof(Enum))
					{
						enumNames = GetEnumNames(underlyingType);
					}
					else if (underlyingType == typeof(bool) || underlyingType.BaseType == typeof(bool))
					{
						jsonPropertyType = "bool";
					}
					else if (underlyingType == typeof(int) || underlyingType.BaseType == typeof(int))
					{
						jsonPropertyType = "integer";
					}
					else if (underlyingType == typeof(long) || underlyingType.BaseType == typeof(long))
					{
						jsonPropertyType = "long";
					}
					else if (underlyingType == typeof(float) || underlyingType.BaseType == typeof(float))
					{
						jsonPropertyType = "float";
					}
					else if (underlyingType == typeof(decimal) || underlyingType.BaseType == typeof(decimal))
					{
						jsonPropertyType = "decimal";
					}
				}
				else if (fieldType == typeof(DateTime))
				{
					jsonPropertyType = GetJsonPropertyTypeForDateTime(pi);
				}
				else if (fieldType == typeof(DateTimeOffset))
				{
					jsonPropertyType = "datetime";
				}
				else if (fieldType.IsEnum)
				{
					enumNames = GetEnumNames(fieldType);
				}
				else if (fieldType == typeof(bool))
				{
					jsonPropertyType = "bool";
				}
				else if (fieldType == typeof(long))
				{
					jsonPropertyName = "long";
				}
				else if (fieldType == typeof(int))
				{
					jsonPropertyType = "integer";
				}
				else if (fieldType == typeof(float))
				{
					jsonPropertyType = "float";
				}
				else if (fieldType == typeof(decimal))
				{
					jsonPropertyType = "decimal";
				}
				else if (fieldType.Name == "IEnumerable`1" || fieldType.Name == "IList`1" || fieldType.Name == "ICollection`1" ||
					fieldType.Name == "List`1" || fieldType.Name == "JContainer" ||
					fieldType.Name == "JObject" || fieldType.Name == "JToken")
				{
					Type underlyingType = fieldType.GetGenericArguments().FirstOrDefault();
					if (underlyingType != null)
					{
						if (underlyingType.IsEnum)
						{
							enumNames = Enum.GetNames(underlyingType);
						}

						if (underlyingType.IsClass && underlyingType != typeof(string) && underlyingType != typeof(DateTime) && underlyingType != typeof(DateTimeOffset))
						{
							if (useCamelCase)
							{
								var firstCharacter = jsonPropertyName.Substring(0, 1).ToLower();
								var otherCharacters = jsonPropertyName.Substring(1);
								jsonPropertyName = firstCharacter + otherCharacters;
							}
							jsonPath = string.Concat(jsonPath, jsonPropertyName, "[]/");
							var dtoTypeNameInput = underlyingType.AssemblyQualifiedName;
							if (pi != null)
							{
								var propertyFilters = GetFilterName(pi);
								if (propertyFilters != null && propertyFilters.Any())
								{
									filterNames.AddRange(propertyFilters);
								}
							}
							return GetVersionPropertiesFromDto(dtoTypeNameInput, apiVersionConfig, useCamelCase, filterNames, jsonPath);
						}
						else
						{
							jsonPropertyName = string.Concat(jsonPropertyName, "[]");
						}
					}
				}
				else if (fieldType.IsEnum)
				{
					enumNames = Enum.GetNames(fieldType);
				}
				else if (fieldType.IsClass && fieldType != typeof(string) && fieldType != typeof(DateTime) && fieldType != typeof(DateTimeOffset))
				{
					if (useCamelCase)
					{
						var firstCharacter = jsonPropertyName.Substring(0, 1).ToLower();
						var otherCharacters = jsonPropertyName.Substring(1);
						jsonPropertyName = firstCharacter + otherCharacters;
					}
					jsonPath = string.Concat(jsonPath, jsonPropertyName, "/");
					var dtoTypeNameInput = fieldType.AssemblyQualifiedName;
					if (pi != null)
					{
						var propertyFilters = GetFilterName(pi);
						if (propertyFilters != null && propertyFilters.Any())
						{
							filterNames.AddRange(propertyFilters);
						}
					}
					return GetVersionPropertiesFromDto(dtoTypeNameInput, apiVersionConfig, useCamelCase, filterNames, jsonPath);
				}
			}
			if (!string.IsNullOrEmpty(jsonPropertyName))
			{
				if (useCamelCase)
				{
					var firstCharacter = jsonPropertyName.Substring(0, 1).ToLower();
					var otherCharacters = jsonPropertyName.Substring(1);
					jsonPropertyName = firstCharacter + otherCharacters;
				}

				// Get additional Data from MetadataAttributes for column documentation
				string columnName = string.Empty;
				string fileName = string.Empty;
				int? length = null;
				string dataDescription = string.Empty;
				bool dataRequired = false;
				bool dataIsInquiryOnly = false;
				string referenceFileName = string.Empty;
				string referenceTableName = string.Empty;
				string referenceColumnName = string.Empty;

				MetadataAttribute schema = null;
				if (pi != null)
				{
					schema = pi.GetDocumentation();
				}
				else if (mi != null)
				{
					schema = mi.FirstOrDefault().GetDocumentation();
				}
				if (schema != null)
				{
					dataDescription = schema.DataDescription;
					columnName = schema.DataElementName;
					//if there is no column name like in shared DTO, we cannot put the column name, we can get other information from the metadata tags instead. 

					if (!string.IsNullOrEmpty(schema.DataFileName)) fileName = schema.DataFileName;
					// overide the maxlength from the CDD for boolean as the CDD size will be 1 but the schema value will be true or false. 
					if (jsonPropertyType == "bool")
						length = 5;
					else if (schema.DataMaxLength > 0)
						length = schema.DataMaxLength;
					if (!string.IsNullOrEmpty(schema.DataReferenceFileName)) referenceFileName = schema.DataReferenceFileName;
					if (!string.IsNullOrEmpty(schema.DataReferenceColumnName)) referenceColumnName = schema.DataReferenceColumnName;
					if (!string.IsNullOrEmpty(schema.DataReferenceTableName)) referenceTableName = schema.DataReferenceTableName;
					if (schema.DataIsInquiryOnly) dataIsInquiryOnly = schema.DataIsInquiryOnly;
					if (schema.DataRequired) dataRequired = schema.DataRequired;
				}
				var dataRow = new Domain.Base.Entities.EthosExtensibleDataRow(columnName, fileName, jsonPropertyName, jsonPath, jsonPropertyType, "", length)
				{
					Description = dataDescription,
					Required = dataRequired,
					TransFileName = referenceFileName,
					TransColumnName = referenceColumnName,
					TransTableName = referenceTableName,
					TransType = referenceFileName.Contains("VALCODES") ? "T" : (!string.IsNullOrEmpty(referenceFileName) ? "F" : string.Empty)
				};

				if (enumNames != null && enumNames.Any())
				{
					dataRow.TransType = "E";
					dataRow.Enumerations = new List<Domain.Base.Entities.EthosApiEnumerations>();
					foreach (var enumValue in enumNames)
					{
						dataRow.Enumerations.Add(new Domain.Base.Entities.EthosApiEnumerations(enumValue.Substring(0), enumValue));
					}
				}
				if (dataRow != null)
				{
					if (pi != null)
					{
						var propertyFilters = GetFilterName(pi);
						if (propertyFilters != null && propertyFilters.Any())
						{
							filterNames.AddRange(propertyFilters);
						}
					}
					dataRow.filterNames = filterNames;
					apiVersionConfig.AddItemToExtendedData(dataRow);
					if (!string.IsNullOrEmpty(columnName) && dataIsInquiryOnly)
					{
						apiVersionConfig.InquiryFields.Add(columnName);
					}
				}
			}
			return apiVersionConfig;
		}

		/// <summary>
		/// Returns OpenApiComponents object using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		/// <param name="requestApiVersionConfig">version configuration infor for request (may be different than response).</param>
		private OpenApiComponents BuildOpenApiComponentsProperty(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, Domain.Base.Entities.EthosExtensibleData requestApiVersionConfig = null)
		{
			if (requestApiVersionConfig == null)
			{
				requestApiVersionConfig = apiVersionConfig;
			}
			var components = new OpenApiComponents();
			var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_{0}_{1}");
			bool isFilterSupported = false;
			//set up paths by looping through supported methods
			if (apiConfiguration.HttpMethods != null && apiConfiguration.HttpMethods.Any())
			{
				foreach (var httpMethod in apiConfiguration.HttpMethods)
				{
					var method = string.Empty;
					if (!string.IsNullOrEmpty(httpMethod.RouteTemplate))
					{
						componentSchemaPrefix = String.Concat(httpMethod.RouteTemplate.Replace("/", "_").Replace("{", "").Replace("}", ""), "_{0}_{1}");
						if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
						{
							componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiConfiguration.ParentResourceName, "_{0}_{1}");
						}
					}
					else
					{
						componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_{0}_{1}");
					}
					if (httpMethod.Method != null && !string.IsNullOrEmpty(httpMethod.Method))
					{
						if (!httpMethod.Method.Contains("QAPI"))
						{
							method = httpMethod.Method.Split('_')[0].ToLower();
							switch (method)
							{
								//schema for get
								case "get":
									{
										if (httpMethod.Method.ToLower() == "get")
										{
											if (!components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, method, "response")))
											{
												var getResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
												components.Schemas.Add(string.Format(componentSchemaPrefix, method, "response"), getResponseSchema);
											}
											isFilterSupported = true;
										}
										else if (httpMethod.Method.ToLower() == "get_id")
										{
											if (!components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, method, "response")))
											{
												var getResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
												components.Schemas.Add(string.Format(componentSchemaPrefix, method, "response"), getResponseSchema);
											}
										}
										if (httpMethod.Method.ToLower() == "get_all")
										{
											if (!components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, method, "response")))
											{
												var getResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
												components.Schemas.Add(string.Format(componentSchemaPrefix, method, "response"), getResponseSchema);
											}
											isFilterSupported = true;
										}

										break;
									}
								//section for put
								case "put":
									{
										if (!components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, method, "request")))
										{
											var putRequestSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, requestApiVersionConfig, "put");
											components.Schemas.Add(string.Format(componentSchemaPrefix, method, "request"), putRequestSchema);
										}
										if (!components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, method, "response")))
										{
											var putResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
											components.Schemas.Add(string.Format(componentSchemaPrefix, method, "response"), putResponseSchema);
										}
										break;
									}
								//section for put
								case "post":
									{
										if (!components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, method, "request")))
										{
											var postRequestSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, requestApiVersionConfig, "post");
											components.Schemas.Add(string.Format(componentSchemaPrefix, method, "request"), postRequestSchema);
										}
										if (!components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, method, "response")))
										{
											var postResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
											components.Schemas.Add(string.Format(componentSchemaPrefix, method, "response"), postResponseSchema);
										}
										break;
									}
							}
						}
						else
						{
							if (!components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, "query", "request")))
							{
								if (queryNamesVersionConfigs != null && queryNamesVersionConfigs.ContainsKey("criteria"))
								{
									var getResponseSchema = GetFilterOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, queryNamesVersionConfigs["criteria"], "criteria");
									components.Schemas.Add(string.Format(componentSchemaPrefix, "query", "request"), getResponseSchema);
									isFilterSupported = true;
								}
								else if (IsEthos(apiConfiguration))
								{
									var getResponseSchema = GetFilterOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, requestApiVersionConfig, "criteria");
									components.Schemas.Add(string.Format(componentSchemaPrefix, "query", "request"), getResponseSchema);
									isFilterSupported = true;
								}
								else
								{
									var getResponseSchema = GetFilterOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, requestApiVersionConfig);
									components.Schemas.Add(string.Format(componentSchemaPrefix, "query", "request"), getResponseSchema);
									isFilterSupported = true;
								}
							}
							if (!components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, "query", "response")))
							{
								var getResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
								components.Schemas.Add(string.Format(componentSchemaPrefix, "query", "response"), getResponseSchema);
							}
						}
					}
				}
			}
			//add component for id

			if (IsCompositeKey(apiConfiguration) && !SupportGetAllOnly(apiConfiguration) && !SupportPostOnly(apiConfiguration))
			{
				components.Schemas.Add(string.Format(componentSchemaPrefix, "id", "parameter"), GetIdOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig));
			}

			// component for filter schema to be used by criteria and qapi_post and named queries
			if (queryNamesVersionConfigs != null && queryNamesVersionConfigs.Any())
			{
				foreach (var queryConfig in queryNamesVersionConfigs)
				{
					if (queryConfig.Value.ExtendedDataFilterList != null && queryConfig.Value.ExtendedDataFilterList.Any())
					{
						if (queryConfig.Key == "criteria")
						{
							if (!components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, "query", "request")))
							{
								var getResponseSchema = GetFilterOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, queryConfig.Value, queryConfig.Key);
								components.Schemas.Add(string.Format(componentSchemaPrefix, "query", "request"), getResponseSchema);
							}
						}
						else
						{
							if (!components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, queryConfig.Key, "parameter")))
							{
								var getResponseSchema = GetFilterOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, queryConfig.Value, queryConfig.Key);
								components.Schemas.Add(string.Format(componentSchemaPrefix, queryConfig.Key, "parameter"), getResponseSchema);
							}
						}
					}
				}
			}
			else if (isFilterSupported && !components.Schemas.ContainsKey(string.Format(componentSchemaPrefix, "query", "request")))
			{
				var getRequestSchema = GetFilterOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, requestApiVersionConfig, "criteria");
				if (getRequestSchema.Properties.Any()) components.Schemas.Add(string.Format(componentSchemaPrefix, "query", "request"), getRequestSchema);
			}

			// component for name query schema to be used by criteria and qapi_post
			if (apiVersionConfig != null && apiVersionConfig.ExtendedDataFilterList != null)
			{
				var nameQueries = apiVersionConfig.ExtendedDataFilterList.Where(query => query.NamedQuery);
				if (nameQueries != null && nameQueries.Any())
				{
					components.Schemas.Add(string.Format(componentSchemaPrefix, "namedQuery", "parameter"), GetNameQueryOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig));
				}
			}

			if (useV2errors)
			{
				// add v2 error component
				// add error schema 
				var errorsSchema = new OpenApiSchema();
				errorsSchema.Required.Add("errors");
				errorsSchema.Type = "object";
				var errorsSchemaProperty = new OpenApiSchema() { Type = "array" };
				var errorsSchemaPropertyItems = new OpenApiSchema() { Type = "object" };
				errorsSchemaPropertyItems.Properties.Add("id", new OpenApiSchema() { Type = "string", Description = "The global identifier of the resource in error." });
				errorsSchemaPropertyItems.Properties.Add("sourceId", new OpenApiSchema() { Type = "string", Description = "The source applications data reference identifier for the primary data entity used to create the resource. This is useful for referencing the source item through the applications administrative user interface." });
				errorsSchemaPropertyItems.Properties.Add("code", new OpenApiSchema() { Type = "string", Description = "The error message code used to describe the error details." });
				errorsSchemaPropertyItems.Properties.Add("description", new OpenApiSchema() { Type = "string", Description = "The error description used to describe the error details." });
				errorsSchemaPropertyItems.Properties.Add("message", new OpenApiSchema() { Type = "string", Description = "The detailed actionable error message." });
				errorsSchemaProperty.Items = errorsSchemaPropertyItems;
				errorsSchema.Properties.Add("errors", errorsSchemaProperty);
				components.Schemas.Add("errors_2_0_0", errorsSchema);
			}
			else
			{
				// add v1 error component
				// add error schema 
				var errorsSchema = new OpenApiSchema();
				errorsSchema.Required.Add("message");
				errorsSchema.Type = "object";
				errorsSchema.Properties.Add("message", new OpenApiSchema() { Type = "string" });
				errorsSchema.Properties.Add("conflict", new OpenApiSchema() { Type = "string" });
				errorsSchema.Properties.Add("isEmpty", new OpenApiSchema() { Type = "boolean" });
				components.Schemas.Add("errors", errorsSchema);
			}
			// add security schemes
			components.SecuritySchemes.Add("EthosIntegrationBearer", new OpenApiSecurityScheme() { Type = SecuritySchemeType.Http, Name = "EthosIntegrationBearer", In = ParameterLocation.Header, Scheme = "bearer" });
			components.SecuritySchemes.Add("BasicAuth", new OpenApiSecurityScheme() { Type = SecuritySchemeType.Http, Name = "BasicAuth", In = ParameterLocation.Header, Scheme = "basic" });
			return components;
		}

		/// <summary>
		/// Returns OpenApiPaths object using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		private OpenApiPaths BuildOpenApiPathsProperty(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
		{
			var paths = new OpenApiPaths();
			//check if this API has composite Key. if that is the case then we do not display {id} path for those APIs. 
			var isCompositeKey = IsCompositeKey(apiConfiguration);
			//set up paths by looping through supported methods
			if (apiConfiguration.HttpMethods != null && apiConfiguration.HttpMethods.Any())
			{
				OpenApiPathItem apiPathItem = null;
				OpenApiPathItem apiByIdPathItem = null;
				OpenApiPathItem qapiPathItem = null;
				bool isGetPathItemAdded = false;
				bool isGetbyIdPathItemAdded = false;
				bool isQapiPathItemAdded = false;
				foreach (var httpMethod in apiConfiguration.HttpMethods)
				{
					var method = string.Empty;
					if (httpMethod.Method != null && !string.IsNullOrEmpty(httpMethod.Method))
					{
						var routeTemplate = httpMethod.RouteTemplate;
						if (!httpMethod.Method.Contains("QAPI"))
						{
							method = httpMethod.Method.Split('_')[0].ToLower();
							if (apiPathItem == null)
								apiPathItem = new OpenApiPathItem();
							if (apiByIdPathItem == null)
								apiByIdPathItem = new OpenApiPathItem();
							switch (method)
							{
								//section for get
								case "get":
									{
										if (httpMethod.Method.ToLower() == "get")
										{
											if (!isGetPathItemAdded)
											{
												apiPathItem.AddOperation(OperationType.Get, BuildGetOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
												isGetPathItemAdded = true;
											}
											if (!isGetbyIdPathItemAdded && !isCompositeKey)
											{
												apiByIdPathItem.AddOperation(OperationType.Get, BuildGetbyIdOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
												isGetbyIdPathItemAdded = true;
											}
											if (!isQapiPathItemAdded)
											{
												qapiPathItem = new OpenApiPathItem();
												qapiPathItem.AddOperation(OperationType.Post, BuildQapiPostOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
												isQapiPathItemAdded = true;
											}

										}
										else if (httpMethod.Method.ToLower() == "get_id")
										{
											if (!isGetbyIdPathItemAdded && !isCompositeKey)
											{
												apiByIdPathItem.AddOperation(OperationType.Get, BuildGetbyIdOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
												isGetbyIdPathItemAdded = true;
											}
											else if (!isGetPathItemAdded)
											{
												apiPathItem.AddOperation(OperationType.Get, BuildGetOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
												isGetPathItemAdded = true;
											}
										}
										else if (httpMethod.Method.ToLower() == "get_all")
										{
											if (!isGetPathItemAdded)
											{
												apiPathItem.AddOperation(OperationType.Get, BuildGetOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
												isGetPathItemAdded = true;
											}
										}
										break;
									}
								//section for put
								case "put":
									{
										if (!isCompositeKey)
											apiByIdPathItem.AddOperation(OperationType.Put, BuildPutOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
										else
											apiPathItem.AddOperation(OperationType.Put, BuildPutOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
										break;
									}
								//section for get
								case "post":
									{
										apiPathItem.AddOperation(OperationType.Post, BuildPostOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
										break;
									}
								//section for get
								case "delete":
									{
										if (!isCompositeKey)
											apiByIdPathItem.AddOperation(OperationType.Delete, BuildDeleteOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
										else
											apiPathItem.AddOperation(OperationType.Delete, BuildDeleteOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
										break;
									}
							}

						}
						else
						{
							if (!isQapiPathItemAdded)
							{
								qapiPathItem = new OpenApiPathItem();
								qapiPathItem.AddOperation(OperationType.Post, BuildQapiPostOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
								isQapiPathItemAdded = true;
							}
						}
					}
				}
				if (apiPathItem != null && apiPathItem.Operations != null && apiPathItem.Operations.Any())
				{
					if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
						paths.Add(string.Concat("/api/", apiConfiguration.ParentResourceName), apiPathItem);
					else
						paths.Add(string.Concat("/api/", apiConfiguration.ResourceName), apiPathItem);
				}
				if (apiByIdPathItem != null && !isCompositeKey && apiByIdPathItem.Operations != null && apiByIdPathItem.Operations.Any())
				{
					if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
						paths.Add(string.Concat("/api/", apiConfiguration.ParentResourceName, "/{id}"), apiByIdPathItem);
					else
						paths.Add(string.Concat("/api/", apiConfiguration.ResourceName, "/{id}"), apiByIdPathItem);
				}
				if (qapiPathItem != null && qapiPathItem.Operations != null && qapiPathItem.Operations.Any())
				{
					if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
						paths.Add(string.Concat("/api/qapi/", apiConfiguration.ParentResourceName), qapiPathItem);
					else
						paths.Add(string.Concat("/api/qapi/", apiConfiguration.ResourceName), qapiPathItem);
				}
			}
			return paths;
		}

		/// <summary>
		/// Returns OpenApiPaths object using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS for response</param>
		/// <param name="requestApiVersionConfig">version configuration info from EDM.EXT.VERSIONS for request (which may be different than response)</param>
		/// <param name="selectedMethod">Method to update in the OpenApiDocument</param>
		/// <param name="openApiDocument">Working OpenAPI document</param>
		private OpenApiDocument BuildOpenApiPathPropertyFromRoute(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, Domain.Base.Entities.EthosExtensibleData requestApiVersionConfig, string selectedMethod, OpenApiDocument openApiDocument)
		{
			var paths = openApiDocument.Paths;
			if (paths == null)
				paths = new OpenApiPaths();

			OpenApiPathItem apiPathItem = null;
			var method = string.Empty;
			var httpMethod = apiConfiguration.HttpMethods.FirstOrDefault(http => http.Method.Equals(selectedMethod, StringComparison.OrdinalIgnoreCase));
			if (httpMethod == null)
			{
				return openApiDocument;
			}
			var routeTemplate = httpMethod.RouteTemplate;
			var arguments = httpMethod.Parameters;
			var exceptions = httpMethod.Exceptions;
			var methodReturns = httpMethod.MethodReturns;

			var pathKey = String.Concat("/api/", routeTemplate);
			if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
				pathKey = String.Concat(pathKey, " (", apiConfiguration.ParentResourceName, ")");
			if (paths.ContainsKey(pathKey))
				apiPathItem = paths[pathKey];
			else
				apiPathItem = new OpenApiPathItem();

			if (selectedMethod != null && !string.IsNullOrEmpty(selectedMethod))
			{
				method = httpMethod.Method.Split('_')[0].ToLower();
				switch (selectedMethod.ToLower())
				{
					//section for get
					case "get_id":
						{
							var openApiOperation = BuildGetbyIdOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary, routeTemplate);
							openApiOperation = OverrideParametersObject(apiConfiguration, openApiOperation, arguments, routeTemplate, selectedMethod);
							openApiOperation = OverrideResponsesObject(openApiOperation, httpMethod.Exceptions);
							apiPathItem.AddOperation(OperationType.Get, openApiOperation);
							break;
						}
					case "get_all":
						{
							var openApiOperation = BuildGetOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary, routeTemplate);
							openApiOperation = OverrideParametersObject(apiConfiguration, openApiOperation, arguments, routeTemplate, selectedMethod);
							openApiOperation = OverrideResponsesObject(openApiOperation, httpMethod.Exceptions);
							apiPathItem.AddOperation(OperationType.Get, openApiOperation);
							break;
						}
					case "qapi_post":
						{
							var openApiOperation = BuildQapiPostOperationObject(apiConfiguration, requestApiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary, routeTemplate);
							openApiOperation = OverrideParametersObject(apiConfiguration, openApiOperation, arguments, routeTemplate);
							openApiOperation = OverrideResponsesObject(openApiOperation, httpMethod.Exceptions);
							apiPathItem.AddOperation(OperationType.Post, openApiOperation);
							break;
						}
					//section for put
					case "put":
						{
							var openApiOperation = BuildPutOperationObject(apiConfiguration, requestApiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary, routeTemplate);
							openApiOperation = OverrideParametersObject(apiConfiguration, openApiOperation, arguments, routeTemplate);
							openApiOperation = OverrideResponsesObject(openApiOperation, httpMethod.Exceptions);
							apiPathItem.AddOperation(OperationType.Put, openApiOperation);
							break;
						}
					//section for post
					case "post":
						{
							var openApiOperation = BuildPostOperationObject(apiConfiguration, requestApiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary, routeTemplate);
							openApiOperation = OverrideParametersObject(apiConfiguration, openApiOperation, arguments, routeTemplate);
							openApiOperation = OverrideResponsesObject(openApiOperation, httpMethod.Exceptions);
							apiPathItem.AddOperation(OperationType.Post, openApiOperation);
							break;
						}
					//section for delete
					case "delete":
						{
							var openApiOperation = BuildDeleteOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary, routeTemplate);
							openApiOperation = OverrideParametersObject(apiConfiguration, openApiOperation, arguments, routeTemplate);
							openApiOperation = OverrideResponsesObject(openApiOperation, httpMethod.Exceptions);
							apiPathItem.AddOperation(OperationType.Delete, openApiOperation);
							break;
						}
				}
			}

			if (apiPathItem != null)
			{
				if (!paths.ContainsKey(pathKey))
					paths.Add(pathKey, apiPathItem);
				else
					paths[pathKey] = apiPathItem;
			}

			openApiDocument.Paths = SortOpenApiDocPaths(paths);

			return openApiDocument;
		}

		/// <summary>
		/// Returns OpenApiOperation object for Get using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		/// <param name="httpMethod">Method supported by the operation</param>
		/// <param name="httpmethodDesc">Description of the httpMethod</param>
		/// <param name="httpMethodPermission">Permission for the httpMethod</param>
		/// <param name="httpMethodSummary">Summary for the httpMethod</param>
		/// <param name="httpRouteTemplate">route template for component reference name.</param>
		private OpenApiOperation BuildGetOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary, string httpRouteTemplate = "")
		{
			var operation = new OpenApiOperation();
			var showAdditionalResponseHeader = false;
			var tagName = apiConfiguration.ResourceName;
			//if (!IsSpecBased(apiConfiguration) && !IsBpa(apiConfiguration) && !string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
			//{
			//    tagName = string.Concat(tagName, " (", apiConfiguration.ParentResourceName, ")");
			//}
			operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = tagName } };
			//summaart for spec-based 
			if (IsSpecBased(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
				{
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				}
				else
				{
					if (!string.IsNullOrEmpty(apiConfiguration.PrimaryTableName))
					{
						operation.Summary = string.Format("Returns resources from {0} from {1}.", apiConfiguration.PrimaryTableName, string.Concat(apiConfiguration.PrimaryApplication, "-", apiConfiguration.PrimaryEntity));
					}
					else
					{
						operation.Summary = string.Format("Returns resources from {0}.", apiConfiguration.PrimaryEntity);
					}
				}
			}
			else if (IsBpa(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				else
					operation.Summary = string.Format("Returns resources from {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
			}
			else if (IsEthos(apiConfiguration) || IsWeb(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				else
					operation.Summary = string.Format("Returns {0} resources.", apiConfiguration.ResourceName);
			}
			else if (!string.IsNullOrEmpty(httpMethodSummary))
			{
				operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
			}

			if (!string.IsNullOrEmpty(httpmethodDesc))
				operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
			if (!string.IsNullOrEmpty(httpMethodPermission))
			{
				operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
			}

			// Add audience
			operation.AddExtension("x-audience", new OpenApiString(apiConfiguration.Audience));

			// Add deprecated date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.DeprecatedOn)) operation.AddExtension("x-deprecated-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.DeprecatedOn)));

			// Add sunset date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.SunsetOn)) operation.AddExtension("x-sunset-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.SunsetOn)));

			//add parameters section
			var parameters = new List<OpenApiParameter>();
			var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_{0}_{1}");
			if (!string.IsNullOrEmpty(httpRouteTemplate))
			{
				componentSchemaPrefix = String.Concat(httpRouteTemplate.Replace("/", "_").Replace("{", "").Replace("}", ""), "_{0}_{1}");
				if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
				{
					componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiConfiguration.ParentResourceName, "_{0}_{1}");
				}
			}
			if (!SupportGetByIdOnly(apiConfiguration) && !IsLegacy(apiConfiguration))
			{
				var limitDesc = "The maximum number of resources requested for this result set.";
				if (apiConfiguration.PageLimit != null)
				{
					limitDesc = string.Concat(limitDesc, " The maximum valid limit value is ", apiConfiguration.PageLimit + ".");
				}
				parameters.Add(GetPathItemParameters("limit", ParameterLocation.Query, false, limitDesc, "integer", componentSchemaPrefix));
				parameters.Add(GetPathItemParameters("offset", ParameterLocation.Query, false, "The 0 based index for a collection of resources for the page requested.", "integer", componentSchemaPrefix));
				if ((IsEthos(apiConfiguration) || IsWeb(apiConfiguration)) && queryNamesVersionConfigs != null && queryNamesVersionConfigs.ContainsKey("criteria"))
					parameters.Add(GetPathItemParameters("criteria", ParameterLocation.Query, false, "The filter criteria as a single URL query parameter. Use this parameter or the individual parameters listed. This must be a JSON representation that can be validated against the schema. Limit and Offset are the only supported additional parameters on the URL.", "string", componentSchemaPrefix, false, true));
				showAdditionalResponseHeader = true;
			}
			//Display Id for only composite Key
			if (IsCompositeKey(apiConfiguration) && !SupportGetAllOnly(apiConfiguration))
			{
				var idRequired = false;
				if (SupportGetByIdOnly(apiConfiguration))
					idRequired = true;
				parameters.Add(GetPathItemParameters("id", ParameterLocation.Query, idRequired, "Must be a JSON representation of the properties that make up the id block of a single record. No additional parameters on the URL are allowed. ", "string", componentSchemaPrefix, false, true));
			}

			//check to see if there is a name query
			bool hasNameQuery = false;
			if (apiVersionConfig != null && apiVersionConfig.ExtendedDataFilterList != null)
			{
				var nameQueries = apiVersionConfig.ExtendedDataFilterList.Where(query => query.NamedQuery);
				if (nameQueries != null && nameQueries.Any())
				{
					hasNameQuery = true;
				}
			}
			if (hasNameQuery)
				parameters.Add(GetPathItemParameters("namedQuery", ParameterLocation.Query, false, "A named query is specified as a query parameter and may require arguments which must be expressed using JSON (where the arguments are provided as name-value pairs akin to the ad-hoc query syntax used for filtering by 'equality', as described above).", "object", componentSchemaPrefix, false, true));
			operation.Parameters = parameters;
			//add response section
			var responses = new OpenApiResponses
			{
				{ "200", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "200", showAdditionalResponseHeader) },
				{ "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
				{ "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
				{ "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
				{ "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
				{ "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
				{ "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
				{ "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
			};
			operation.Responses = responses;
			//add security to the operation            
			operation.Security.Add(BuildOpenApiSecurityRequirement());
			return operation;
		}

		/// <summary>
		/// Returns OpenApiOperation object for Get using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		/// <param name="httpMethod">Method supported by the operation</param>
		/// <param name="httpmethodDesc">Description of the httpMethod</param>
		/// <param name="httpMethodPermission">Permission for the httpMethod</param>
		/// <param name="httpMethodSummary">Summary for the httpMethod</param>
		/// <param name="httpRouteTemplate">route template for component reference name.</param>
		private OpenApiOperation BuildGetbyIdOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary, string httpRouteTemplate = "")
		{
			var operation = new OpenApiOperation();
			var tagName = apiConfiguration.ResourceName;
			//if (!IsSpecBased(apiConfiguration) && !IsBpa(apiConfiguration) && !string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
			//{
			//    tagName = string.Concat(tagName, " (", apiConfiguration.ParentResourceName, ")");
			//}
			operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = tagName } };
			//summary for spec-based 
			if (IsSpecBased(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
				{
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				}
				else
				{
					if (!string.IsNullOrEmpty(apiConfiguration.PrimaryTableName))
						operation.Summary = string.Format("Returns the requested resource from {0} from {1}.", apiConfiguration.PrimaryTableName, string.Concat(apiConfiguration.PrimaryApplication, "-", apiConfiguration.PrimaryEntity));
					else
						operation.Summary = string.Format("Returns the requested resource from {0}.", apiConfiguration.PrimaryEntity);
				}
			}
			else if (IsBpa(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
				{
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				}
				else
				{
					operation.Summary = string.Format("Returns the requested resource from {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
				}
			}
			else if (IsEthos(apiConfiguration) || IsWeb(apiConfiguration) || IsLegacy(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				else
					operation.Summary = string.Format("Returns the requested resource from {0}.", apiConfiguration.ResourceName);
			}
			if (!string.IsNullOrEmpty(httpmethodDesc))
				operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
			if (!string.IsNullOrEmpty(httpMethodPermission))
			{
				operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
			}

			// Add audience
			operation.AddExtension("x-audience", new OpenApiString(apiConfiguration.Audience));

			// Add deprecated date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.DeprecatedOn)) operation.AddExtension("x-deprecated-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.DeprecatedOn)));

			// Add sunset date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.SunsetOn)) operation.AddExtension("x-sunset-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.SunsetOn)));

			//add parameters section
			var parameters = new List<OpenApiParameter>();
			var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_{0}_{1}");
			if (!string.IsNullOrEmpty(httpRouteTemplate))
			{
				componentSchemaPrefix = String.Concat(httpRouteTemplate.Replace("/", "_").Replace("{", "").Replace("}", ""), "_{0}_{1}");
				if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
				{
					componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiConfiguration.ParentResourceName, "_{0}_{1}");
				}
			}
			//check to see if Id is a property or string. If string, check if this is a GUID
			bool isGuid = false;
			if (apiConfiguration != null && !string.IsNullOrEmpty(apiConfiguration.PrimaryGuidSource))
				isGuid = true;
			if (apiConfiguration != null && IsCompositeKey(apiConfiguration))
				parameters.Add(GetPathItemParameters("id", ParameterLocation.Query, true, "Must be a JSON representation of the  properties that make up the id block of a single record. No additional parameters on the URL are allowed. ", "string", componentSchemaPrefix, false, true));
			else
				parameters.Add(GetPathItemParameters("id", ParameterLocation.Path, true, "A global identifier of the resource for use in all external references.", "string", componentSchemaPrefix, isGuid));

			operation.Parameters = parameters;
			//add response section
			var responses = new OpenApiResponses
			{
				{ "200", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "200") },
				{ "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
				{ "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
				{ "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
				{ "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
				{ "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
				{ "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
				{ "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
			};
			operation.Responses = responses;
			//add security to the operation            
			operation.Security.Add(BuildOpenApiSecurityRequirement());
			return operation;
		}

		/// <summary>
		/// Override the operation parameters object if we have method arguments pulled in from the XML comments of the controller method
		/// being documented.
		/// </summary>
		/// <param name="apiConfiguration">Api Configuration to build OpenAPI operation.</param>
		/// <param name="operation">The OpenApiOperation object.</param>
		/// <param name="methodArguments">Dictionary of Method Arguments from XML comments.</param>
		/// <param name="routeTemplate">The route template with matching arguments.</param>
		/// <param name="selectedMethod">If we are on GET then we need to include query parameters.</param>
		/// <returns>An OpenApiOperation for this GET/PUT request.</returns>
		private OpenApiOperation OverrideParametersObject(EthosApiConfiguration apiConfiguration, OpenApiOperation operation, Dictionary<string, string> methodArguments, string routeTemplate, string selectedMethod = "")
		{
			if (methodArguments != null && methodArguments.Any())
			{
				var parameters = new List<OpenApiParameter>();
				var componentSchemaPrefix = String.Concat(routeTemplate.Replace("/", "_").Replace("{", "").Replace("}", ""), "_{0}_{1}");
				if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
				{
					componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiConfiguration.ParentResourceName, "_{0}_{1}");
				}
				foreach (var argument in methodArguments)
				{
					var key = argument.Key;
					var parmName = string.Concat("{", key, "}");
					var value = argument.Value;
					if (routeTemplate.Contains(parmName))
					{
						parameters.Add(GetPathItemParameters(key, ParameterLocation.Path, true, value, "string", ""));
					}
					else if (selectedMethod.ToLower().StartsWith("get"))
					{
						if (key == "page")
						{

							parameters.Add(GetPathItemParameters("limit", ParameterLocation.Query, false, string.Concat("The maximum number of resources requested for this result set.", Environment.NewLine, Environment.NewLine, "<b>/api/", routeTemplate, "?limit={limit}</b>"), "integer", ""));
							parameters.Add(GetPathItemParameters("offset", ParameterLocation.Query, false, string.Concat("The 0 based index for a collection of resources for the page requested.", Environment.NewLine, Environment.NewLine, "<b>/api/", routeTemplate, "?offset={offset}</b>"), "integer", ""));
						}
						else if (key == "criteria")
						{

							parameters.Add(GetPathItemParameters("criteria", ParameterLocation.Query, false, string.Concat("The filter criteria as a single URL query parameter. Use this parameter or the individual parameters listed. This must be a JSON representation that can be validated against the schema. Limit and Offset are the only supported additional parameters on the URL.", Environment.NewLine, Environment.NewLine, "<b>/api/", routeTemplate, "?", key, "={", key, "}</b>"), "string", componentSchemaPrefix, false, true));
						}
						else
						{
							value = string.Concat(value, Environment.NewLine, Environment.NewLine, "<b>/api/", routeTemplate, "?", key, "={", key, "}</b>");
							if (queryNamesDtoTypes.ContainsKey(key))
							{
								parameters.Add(GetPathItemParameters(key, ParameterLocation.Query, false, value, "string", componentSchemaPrefix, false, true));
							}
							else
							{
								parameters.Add(GetPathItemParameters(key, ParameterLocation.Query, false, value, "string", ""));
							}
						}
					}
					else if (selectedMethod == "")
					{
						value = string.Concat(value, Environment.NewLine, Environment.NewLine, "<b>/api/", routeTemplate, "?", key, "={", key, "}</b>");
						if (queryNamesDtoTypes.ContainsKey(key))
						{
							parameters.Add(GetPathItemParameters(key, ParameterLocation.Query, false, value, "string", componentSchemaPrefix, false, true));
						}
						else
						{
							parameters.Add(GetPathItemParameters(key, ParameterLocation.Query, false, value, "string", ""));
						}
					}
				}
				operation.Parameters = parameters;
			}

			return operation;
		}

		/// <summary>
		/// Override the operation responses object if we have exceptions pulled in from the XML comments of the controller method
		/// being documented.
		/// </summary>
		/// <param name="operation"></param>
		/// <param name="methodExceptions"></param>
		/// <returns>An OpenApiOperation for this request.</returns>
		private OpenApiOperation OverrideResponsesObject(OpenApiOperation operation, List<string> methodExceptions)
		{
			var responses = new OpenApiResponses { };
			var errMediaTypeContent = new OpenApiMediaType();
			var errorContentType = "application/vnd.hedtech.integration.errors.v2+json";
			if (useV2errors)
			{
				errMediaTypeContent.Schema = new OpenApiSchema() { Type = "array", Items = new OpenApiSchema() { Reference = new OpenApiReference() { Id = "errors_2_0_0", Type = ReferenceType.Schema } } };
			}
			else
			{
				errorContentType = "application/json";
				errMediaTypeContent.Schema = new OpenApiSchema() { Type = "object", Reference = new OpenApiReference() { Id = "errors", Type = ReferenceType.Schema } };
			}
			if (operation.Responses.ContainsKey("200"))
				responses.Add("200", operation.Responses.First(ore => ore.Key == "200").Value);
			if (operation.Responses.ContainsKey("204"))
				responses.Add("204", operation.Responses.First(ore => ore.Key == "204").Value);

			if (methodExceptions != null && methodExceptions.Any())
			{
				foreach (var argument in methodExceptions)
				{
					var response = new OpenApiResponse() { Description = string.Concat("Failure. ", argument) };
					response.Content.Add(errorContentType, errMediaTypeContent);

					if (argument.Contains("400") || argument.Contains("badrequest", StringComparison.OrdinalIgnoreCase))
						responses.Add("400", response);
					else if (argument.Contains("401") || argument.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
						responses.Add("401", response);
					else if (argument.Contains("403") || argument.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
						responses.Add("403", response);
					else if (argument.Contains("404") || argument.Contains("notfound", StringComparison.OrdinalIgnoreCase))
						responses.Add("404", response);
					else if (argument.Contains("405") || argument.Contains("methodnotallowed", StringComparison.OrdinalIgnoreCase))
						responses.Add("405", response);
					else if (argument.Contains("500") || argument.Contains("notacceptable", StringComparison.OrdinalIgnoreCase))
						responses.Add("500", response);
				}
			}
			if (!responses.ContainsKey("401"))
			{
				var response = new OpenApiResponse() { Description = "Failure. Unauthorized indicates that the requested resource requires authentication." };
				responses.Add("401", response);
			}
			if (!responses.ContainsKey("403"))
			{
				var response = new OpenApiResponse() { Description = "Failure. Forbidden indicates that the user does not have the necessary permissions for the resource." };
				response.Content.Add(errorContentType, errMediaTypeContent);
				responses.Add("403", response);
			}
			if (!responses.ContainsKey("404"))
			{
				var response = new OpenApiResponse() { Description = "Failure. NotFound indicates that the requested resource does not exist on the server." };
				responses.Add("404", response);
			}
			if (useV2errors)
			{
				if (!responses.ContainsKey("405"))
				{
					var response = new OpenApiResponse() { Description = "Failure. MethodNotAllowed indicates that the client tried to use an HTTP method that the resource does not allow." };
					response.Content.Add(errorContentType, errMediaTypeContent);
					responses.Add("405", response);
				}
				if (!responses.ContainsKey("406"))
				{
					var response = new OpenApiResponse() { Description = "Failure. NotAcceptable indicates that the request was not able to generate any of the client’s preferred media types, as indicated by the Accept request header." };
					response.Content.Add(errorContentType, errMediaTypeContent);
					responses.Add("406", response);
				}
			}
			if (!responses.ContainsKey("400"))
			{
				var response = new OpenApiResponse() { Description = "Failure. BadRequest indicates that the server cannot or will not process the request due to something that is perceived to be a client error." };
				response.Content.Add(errorContentType, errMediaTypeContent);
				responses.Add("400", response);
			}
			if (!responses.ContainsKey("500"))
			{
				var response = new OpenApiResponse() { Description = "Server error, unexpected configuration or data" };
				response.Content.Add(errorContentType, errMediaTypeContent);
				responses.Add("500", response);
			}

			operation.Responses = responses;

			return operation;
		}

		/// <summary>
		/// Returns OpenApiOperation object for Put using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		/// <param name="httpMethod">Method supported by the operation</param>
		/// <param name="httpmethodDesc">Description of the httpMethod</param>
		/// <param name="httpMethodPermission">Permission for the httpMethod</param>
		///  <param name="httpMethodSummary">Summary for the httpMethod</param>
		/// <param name="httpRouteTemplate">route template for component reference name.</param>
		private OpenApiOperation BuildPutOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary, string httpRouteTemplate = "")
		{
			var operation = new OpenApiOperation();
			var tagName = apiConfiguration.ResourceName;
			//if (!IsSpecBased(apiConfiguration) && !IsBpa(apiConfiguration) && !string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
			//{
			//    tagName = string.Concat(tagName, " (", apiConfiguration.ParentResourceName, ")");
			//}
			operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = tagName } };
			//summaart for spec-based 
			if (IsSpecBased(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
				{
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				}
				else
				{
					if (!string.IsNullOrEmpty(apiConfiguration.PrimaryTableName))
						operation.Summary = string.Format("Updates requested resource from {0} from {1}.", apiConfiguration.PrimaryTableName, string.Concat(apiConfiguration.PrimaryApplication, "-", apiConfiguration.PrimaryEntity));
					else
						operation.Summary = string.Format("Updates requested resource from from {0}.", apiConfiguration.PrimaryEntity);
				}
			}
			else if (IsBpa(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
				{
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				}
				else
				{
					operation.Summary = string.Format("Updates requested resource from {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
				}
			}
			else if (IsEthos(apiConfiguration) || IsWeb(apiConfiguration) || IsLegacy(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				else
					operation.Summary = string.Format("Updates requested resource from {0}.", apiConfiguration.ResourceName);
			}
			if (!string.IsNullOrEmpty(httpmethodDesc))
				operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
			if (!string.IsNullOrEmpty(httpMethodPermission))
			{
				operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
			}

			// Add audience
			operation.AddExtension("x-audience", new OpenApiString(apiConfiguration.Audience));

			// Add deprecated date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.DeprecatedOn)) operation.AddExtension("x-deprecated-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.DeprecatedOn)));

			// Add sunset date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.SunsetOn)) operation.AddExtension("x-sunset-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.SunsetOn)));

			//add parameters section
			var parameters = new List<OpenApiParameter>();
			var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_{0}_{1}");
			if (!string.IsNullOrEmpty(httpRouteTemplate))
			{
				componentSchemaPrefix = String.Concat(httpRouteTemplate.Replace("/", "_").Replace("{", "").Replace("}", ""), "_{0}_{1}");
				if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
				{
					componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiConfiguration.ParentResourceName, "_{0}_{1}");
				}
			}

			//check to see if Id is a property or string. If string, check if this is a GUID
			bool isGuid = !string.IsNullOrEmpty(apiConfiguration?.PrimaryGuidSource);

			if (apiConfiguration != null && IsCompositeKey(apiConfiguration))
				parameters.Add(GetPathItemParameters("id", ParameterLocation.Query, true, "Must be a JSON representation of the  properties that make up the id block of a single record. No additional parameters on the URL are allowed. ", "string", componentSchemaPrefix, isGuid, true));

			// If endpoint has and id/guid as part of the route add it
			if (httpRouteTemplate.Contains("{id}"))
				parameters.Add(GetPathItemParameters("id", ParameterLocation.Path, true, "A global identifier of the resource for use in all external references.", "string", componentSchemaPrefix, isGuid));

			operation.Parameters = parameters;
			//add request section
			operation.RequestBody = BuildPathItemPutPostRequestBody(apiVersionConfig, "put", componentSchemaPrefix);
			//add response section
			var responses = new OpenApiResponses
			{
				{ "200", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "200") },
				{ "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
				{ "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
				{ "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
				{ "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
				{ "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
				{ "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
				{ "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
			};
			operation.Responses = responses;
			//add security to the operation            
			operation.Security.Add(BuildOpenApiSecurityRequirement());
			return operation;
		}

		/// <summary>
		/// Returns true if the api is spec-based
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		private bool IsSpecBased(EthosApiConfiguration apiConfiguration)
		{
			bool isSpecBased = false;
			if (apiConfiguration != null && apiConfiguration.ApiType == "A")
				isSpecBased = true;
			return isSpecBased;
		}

		/// <summary>
		/// Returns true if the api is Business Process Based
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		private bool IsBpa(EthosApiConfiguration apiConfiguration)
		{
			bool isBPA = false;
			if (apiConfiguration != null && apiConfiguration.ApiType == "T")
				isBPA = true;
			return isBPA;
		}

		/// <summary>
		/// Returns true if the api is Ethos or Ethos Enabled
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		private bool IsEthos(EthosApiConfiguration apiConfiguration)
		{
			bool isEthos = false;
			if (apiConfiguration != null && apiConfiguration.ApiType.ToLower() == "ethos")
				isEthos = true;
			return isEthos;
		}

		/// <summary>
		/// Returns true if the api is Ethos or Ethos Enabled
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		private bool IsEthosEnabled(EthosApiConfiguration apiConfiguration)
		{
			bool isEthosEnabled = false;
			if (apiConfiguration != null && apiConfiguration.ApiType.ToLower() == "web-ethos")
				isEthosEnabled = true;
			return isEthosEnabled;
		}

		/// <summary>
		/// Returns true if the api is web enabled
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		private bool IsWeb(EthosApiConfiguration apiConfiguration)
		{
			bool IsWeb = false;
			if (apiConfiguration != null && (apiConfiguration.ApiType.ToLower() == "web-nonethos" || apiConfiguration.ApiType.ToLower() == "web-ethos"))
				IsWeb = true;
			return IsWeb;
		}

		/// <summary>
		/// Returns true if the api is Ethos or Ethos Enabled
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		private bool IsLegacy(EthosApiConfiguration apiConfiguration)
		{
			bool isLegacy = false;
			if (apiConfiguration != null && (apiConfiguration.ApiType.ToLower() == "legacy" || apiConfiguration.ApiType.ToLower() == "web-nonethos" || apiConfiguration.ApiType.ToLower() == "web-ethos"))
				isLegacy = true;
			return isLegacy;
		}

		/// <summary>
		/// Returns true if the api only supports get by Id
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		private bool SupportGetByIdOnly(EthosApiConfiguration apiConfiguration)
		{
			bool supportGetByIdOnly = false;
			//if we have only one method and it is get by Id, then we are good. 
			if (apiConfiguration != null && apiConfiguration.HttpMethods != null && apiConfiguration.HttpMethods.Any())
			{
				if (apiConfiguration.HttpMethods.Count == 1)
				{
					var methodSupported = apiConfiguration.HttpMethods.FirstOrDefault();
					if (methodSupported != null && methodSupported.Method.ToLower() == "get_id")
						supportGetByIdOnly = true;
				}
				//we could have get_id along with put, post and delete. 
				else
				{
					supportGetByIdOnly = true;
					var versionSupported = apiConfiguration.HttpMethods.Where(z => z.Method.Equals("GET_ALL") || z.Method.Equals("GET") || z.Method.Equals("POST_QAPI")).ToList();
					if (versionSupported != null && versionSupported.Any())
						supportGetByIdOnly = false;
				}
			}
			return supportGetByIdOnly;
		}

		/// <summary>
		/// Returns true if the api only supports get all
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		private bool SupportGetAllOnly(EthosApiConfiguration apiConfiguration)
		{
			bool supportGetAllOnly = false;
			if (apiConfiguration != null && apiConfiguration.HttpMethods != null && apiConfiguration.HttpMethods.Any() && apiConfiguration.HttpMethods.Count == 1)
			{
				var methodSupported = apiConfiguration.HttpMethods.FirstOrDefault();
				if (methodSupported != null && methodSupported.Method.ToLower() == "get_all")
					supportGetAllOnly = true;
			}
			return supportGetAllOnly;
		}

		/// <summary>
		/// Returns true if the api only supports qapi post or post
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		private bool SupportPostOnly(EthosApiConfiguration apiConfiguration)
		{
			bool supportPostOnly = false;
			if (apiConfiguration != null && apiConfiguration.HttpMethods != null && apiConfiguration.HttpMethods.Any() && apiConfiguration.HttpMethods.Count == 1)
			{
				var methodSupported = apiConfiguration.HttpMethods.FirstOrDefault();
				if (methodSupported != null && (methodSupported.Method.ToLower() == "post_qapi" || methodSupported.Method.ToLower() == "post"))
					supportPostOnly = true;
			}
			return supportPostOnly;
		}

		/// <summary>
		/// Returns true if the api uses a composite Key
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		private bool IsCompositeKey(EthosApiConfiguration apiConfiguration)
		{
			bool isCompositeKey = false;
			if (apiConfiguration != null && apiConfiguration.ColleagueKeyNames != null && apiConfiguration.ColleagueKeyNames.Count > 1 && IsBpa(apiConfiguration) && string.IsNullOrEmpty(apiConfiguration.PrimaryGuidSource))
				isCompositeKey = true;
			return isCompositeKey;
		}

		/// <summary>
		/// Returns OpenApiOperation object for QAPI Post using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		/// <param name="httpMethod">Method supported by the operation</param>
		/// <param name="httpmethodDesc">Description of the httpMethod</param>
		/// <param name="httpMethodPermission">Permission for the httpMethod</param>
		/// <param name="httpMethodSummary">Summary for the httpMethod</param>
		/// <param name="httpRouteTemplate">route template for component reference name.</param>
		private OpenApiOperation BuildQapiPostOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary, string httpRouteTemplate = "")
		{
			var operation = new OpenApiOperation();
			var tagName = apiConfiguration.ResourceName;
			//if (!IsSpecBased(apiConfiguration) && !IsBpa(apiConfiguration) && !string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
			//{
			//    tagName = string.Concat(tagName, " (", apiConfiguration.ParentResourceName, ")");
			//}
			operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = tagName } };
			//summaart for spec-based 
			if (IsSpecBased(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
				{
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				}
				else
				{
					if (!string.IsNullOrEmpty(apiConfiguration.PrimaryTableName))
						operation.Summary = string.Format("Returns requested resource from {0} from {1}.", apiConfiguration.PrimaryTableName, string.Concat(apiConfiguration.PrimaryApplication, "-", apiConfiguration.PrimaryEntity));
					else
						operation.Summary = string.Format("Returns requested resource from {0}.", apiConfiguration.PrimaryEntity);
				}
			}
			else if (IsBpa(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
				{
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				}
				else
				{
					operation.Summary = string.Format("Returns requested resource from {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
				}
			}
			else if (IsEthos(apiConfiguration) || IsWeb(apiConfiguration) || IsLegacy(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				else
					operation.Summary = string.Format("Returns requested resource from {0}.", apiConfiguration.ResourceName);
			}
			if (!string.IsNullOrEmpty(httpmethodDesc))
				operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
			if (!string.IsNullOrEmpty(httpMethodPermission))
			{
				operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
			}

			// Add audience
			operation.AddExtension("x-audience", new OpenApiString(apiConfiguration.Audience));

			// Add deprecated date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.DeprecatedOn)) operation.AddExtension("x-deprecated-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.DeprecatedOn)));

			// Add sunset date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.SunsetOn)) operation.AddExtension("x-sunset-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.SunsetOn)));

			//add parameters section
			var parameters = new List<OpenApiParameter>();
			var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_{0}_{1}");
			if (!string.IsNullOrEmpty(httpRouteTemplate))
			{
				componentSchemaPrefix = String.Concat(httpRouteTemplate.Replace("/", "_").Replace("{", "").Replace("}", ""), "_{0}_{1}");
				if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
				{
					componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiConfiguration.ParentResourceName, "_{0}_{1}");
				}
			}
			var limitDesc = "The maximum number of resources requested for this result set.";
			if (apiConfiguration.PageLimit != null)
			{
				limitDesc = string.Concat(limitDesc, " The maximum valid limit value is ", apiConfiguration.PageLimit);
			}
			if (!IsLegacy(apiConfiguration))
			{
				parameters.Add(GetPathItemParameters("limit", ParameterLocation.Query, false, limitDesc, "integer", componentSchemaPrefix));
				parameters.Add(GetPathItemParameters("offset", ParameterLocation.Query, false, "The 0 based index for a collection of resources for the page requested.", "integer", componentSchemaPrefix));
			}
			operation.Parameters = parameters;
			//add request section
			operation.RequestBody = BuildPathItemPutPostRequestBody(apiVersionConfig, "query", componentSchemaPrefix);
			//add response section
			var responses = new OpenApiResponses
			{
				{ "200", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "200", true) },
				{ "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
				{ "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
				{ "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
				{ "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
				{ "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
				{ "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
				{ "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
			};
			operation.Responses = responses;
			//add security to the operation            
			operation.Security.Add(BuildOpenApiSecurityRequirement());
			return operation;
		}


		/// <summary>
		/// Returns OpenApiOperation object for Post using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		/// <param name="httpMethod">Method supported by the operation</param>
		/// <param name="httpmethodDesc">Description of the httpMethod</param>
		/// <param name="httpMethodPermission">Permission for the httpMethod</param>
		/// <param name="httpMethodSummary">Summary for the httpMethod</param>
		/// <param name="httpRouteTemplate">route template for component reference name.</param>
		private OpenApiOperation BuildPostOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary, string httpRouteTemplate = "")
		{
			var operation = new OpenApiOperation();
			var tagName = apiConfiguration.ResourceName;
			//if (!IsSpecBased(apiConfiguration) && !IsBpa(apiConfiguration) && !string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
			//{
			//    tagName = string.Concat(tagName, " (", apiConfiguration.ParentResourceName, ")");
			//}
			operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = tagName } };
			//summaart for spec-based 
			if (IsSpecBased(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
				{
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				}
				else
				{
					operation.Summary = string.Format("Creates a new resource in {0}.", apiConfiguration.PrimaryEntity);
				}
			}
			else if (IsBpa(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
				{
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				}
				else
				{
					operation.Summary = string.Format("Creates a new resource in {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
				}
			}
			else if (IsEthos(apiConfiguration) || IsWeb(apiConfiguration) || IsLegacy(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				else
					operation.Summary = string.Format("Creates requested resource in {0}.", apiConfiguration.ResourceName);
			}
			if (!string.IsNullOrEmpty(httpmethodDesc))
				operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
			if (!string.IsNullOrEmpty(httpMethodPermission))
			{
				operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
			}

			// Add audience
			operation.AddExtension("x-audience", new OpenApiString(apiConfiguration.Audience));

			// Add deprecated date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.DeprecatedOn)) operation.AddExtension("x-deprecated-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.DeprecatedOn)));

			// Add sunset date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.SunsetOn)) operation.AddExtension("x-sunset-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.SunsetOn)));

			var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_{0}_{1}");
			if (!string.IsNullOrEmpty(httpRouteTemplate))
			{
				componentSchemaPrefix = String.Concat(httpRouteTemplate.Replace("/", "_").Replace("{", "").Replace("}", ""), "_{0}_{1}");
				if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
				{
					componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiConfiguration.ParentResourceName, "_{0}_{1}");
				}
			}
			//add parameters section
			var parameters = new List<OpenApiParameter>();
			operation.Parameters = parameters;
			//add request section
			operation.RequestBody = BuildPathItemPutPostRequestBody(apiVersionConfig, "post", componentSchemaPrefix);
			//add response section
			var responses = new OpenApiResponses
			{
				{ "200", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "200") },
				{ "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
				{ "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
				{ "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
				{ "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
				{ "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
				{ "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
				{ "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
			};
			operation.Responses = responses;
			//add security to the operation            
			operation.Security.Add(BuildOpenApiSecurityRequirement());
			return operation;
		}

		/// <summary>
		/// Returns OpenApiOperation object for Delete using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		/// <param name="httpMethod">Method supported by the operation</param>
		/// <param name="httpmethodDesc">Description of the httpMethod</param>
		/// <param name="httpMethodPermission">Permission for the httpMethod</param>
		/// <param name="httpMethodSummary">Summary for the httpMethod</param>
		/// <param name="httpRouteTemplate">route template for component reference name.</param>
		private OpenApiOperation BuildDeleteOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary, string httpRouteTemplate = "")
		{
			var operation = new OpenApiOperation();
			var tagName = apiConfiguration.ResourceName;
			//if (!IsSpecBased(apiConfiguration) && !IsBpa(apiConfiguration) && !string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
			//{
			//    tagName = string.Concat(tagName, " (", apiConfiguration.ParentResourceName, ")");
			//}
			operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = tagName } };
			//summaart for spec-based 
			if (IsSpecBased(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				else
					operation.Summary = string.Format("Deletes requested resource from {0}.", apiConfiguration.PrimaryEntity);
			}
			else if (IsBpa(apiConfiguration))
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				else
					operation.Summary = string.Format("Deletes requested resource from {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
			}
			else
			{
				if (!string.IsNullOrEmpty(httpMethodSummary))
					operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
				else
					operation.Summary = string.Format("Deletes requested resource from {0}.", apiConfiguration.ResourceName);
			}
			if (!string.IsNullOrEmpty(httpmethodDesc))
				operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
			if (!string.IsNullOrEmpty(httpMethodPermission))
			{
				operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
			}

			// Add audience
			operation.AddExtension("x-audience", new OpenApiString(apiConfiguration.Audience));

			// Add deprecated date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.DeprecatedOn)) operation.AddExtension("x-deprecated-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.DeprecatedOn)));

			// Add sunset date
			if (!string.IsNullOrWhiteSpace(apiConfiguration.SunsetOn)) operation.AddExtension("x-sunset-on", new OpenApiDateTime(DateTimeOffset.Parse(apiConfiguration.SunsetOn)));

			//add parameters section
			var parameters = new List<OpenApiParameter>();
			var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_{0}_{1}");
			if (!string.IsNullOrEmpty(httpRouteTemplate))
			{
				componentSchemaPrefix = String.Concat(httpRouteTemplate.Replace("/", "_").Replace("{", "").Replace("}", ""), "_{0}_{1}");
				if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
				{
					componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiConfiguration.ParentResourceName, "_{0}_{1}");
				}
			}
			//check to see if Id is a property or string. If string, check if this is a GUID
			bool isGuid = false;
			if (apiConfiguration != null && !string.IsNullOrEmpty(apiConfiguration.PrimaryGuidSource))
				isGuid = true;
			if (apiConfiguration != null && IsCompositeKey(apiConfiguration))
				parameters.Add(GetPathItemParameters("id", ParameterLocation.Query, true, "Must be a JSON representation of the properties that make up the id block of a single record. No additional parameters on the URL are allowed.", "string", componentSchemaPrefix, isGuid, true));
			else
				parameters.Add(GetPathItemParameters("id", ParameterLocation.Path, true, "A global identifier of the resource for use in all external references.", "string", componentSchemaPrefix, isGuid));

			operation.Parameters = parameters;
			//add response section
			var responses = new OpenApiResponses
			{
				{ "204", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "204") },
				{ "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
				{ "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
				{ "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
				{ "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
				{ "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
				{ "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
				{ "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
			};
			operation.Responses = responses;
			//add security to the operation            
			operation.Security.Add(BuildOpenApiSecurityRequirement());
			return operation;
		}

		/// <summary>
		/// Returns OpenApiRequestBody object using the API configuration info
		/// </summary>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		/// <param name="httpMethod">Method supported by the operation</param>
		/// <param name="schemaPrefix">prefix for the content schema</param>

		private OpenApiRequestBody BuildPathItemPutPostRequestBody(Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string schemaPrefix)
		{
			var requestBody = new OpenApiRequestBody();
			var mediaTypeContent = new OpenApiMediaType();
			mediaTypeContent.Schema = new OpenApiSchema() { Type = "array", Items = new OpenApiSchema() { Reference = new OpenApiReference() { Id = string.Format(schemaPrefix, httpMethod, "request"), Type = ReferenceType.Schema } } };
			if (requestedContentTypes != null && requestedContentTypes.Any())
			{
				foreach (var requestedType in requestedContentTypes)
				{
					requestBody.Content.Add(requestedType, mediaTypeContent);
				}
			}
			else
			{
				if (apiVersionConfig.ExtendedSchemaType.StartsWith("application"))
					requestBody.Content.Add(apiVersionConfig.ExtendedSchemaType, mediaTypeContent);
				else if (apiVersionConfig.ExtendedSchemaType.Contains("/"))
					requestBody.Content.Add(string.Format(apiVersionConfig.ExtendedSchemaType), mediaTypeContent);
				else
					requestBody.Content.Add(string.Format("application/{0}", apiVersionConfig.ExtendedSchemaType), mediaTypeContent);
			}
			return requestBody;
		}

		/// <summary>
		/// Returns static OpenApiSecurityRequirement object
		/// </summary>
		private OpenApiSecurityRequirement BuildOpenApiSecurityRequirement()
		{
			var securityRequirement = new OpenApiSecurityRequirement
			{
				{ new OpenApiSecurityScheme() { Type = SecuritySchemeType.Http , Reference = new OpenApiReference() { Id = "EthosIntegrationBearer", Type = ReferenceType.SecurityScheme }}, new List<string>() { } },
				{ new OpenApiSecurityScheme() { Type = SecuritySchemeType.Http,  Reference = new OpenApiReference() { Id = "BasicAuth", Type = ReferenceType.SecurityScheme }}, new List<string>() {  } }
			};
			return securityRequirement;
		}

		/// <summary>
		/// Returns OpenApiResponse object using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		/// <param name="httpMethod">Method supported by the operation</param>
		/// <param name="schema_prefix">prefix for the content schema</param>
		/// <param name="returnCode">http response return code</param>
		/// <param name="showAddResponseHeader">http response return code</param>
		private OpenApiResponse BuildPathItemResponse(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string schema_prefix, string returnCode, bool showAddResponseHeader = false)
		{
			var response = new OpenApiResponse();
			var errMediaTypeContent = new OpenApiMediaType();
			if (httpMethod == "qapi") httpMethod = "query";
			var errorContentType = "application/vnd.hedtech.integration.errors.v2+json";
			if (useV2errors)
			{
				errMediaTypeContent.Schema = new OpenApiSchema() { Type = "array", Items = new OpenApiSchema() { Reference = new OpenApiReference() { Id = "errors_2_0_0", Type = ReferenceType.Schema } } };
			}
			else
			{
				errorContentType = "application/json";
				errMediaTypeContent.Schema = new OpenApiSchema() { Type = "object", Reference = new OpenApiReference() { Id = "errors", Type = ReferenceType.Schema } };
			}
			switch (returnCode)
			{
				case "200":
					{
						response.Description = "OK";
						//x-media-type header
						var mediaTypeHeader = new OpenApiHeader();
						if (IsLegacy(apiConfiguration))
							mediaTypeHeader.Description = "The media type with the version number of the response.";
						else
							mediaTypeHeader.Description = "The full semantic version with the media type of the response.";
						mediaTypeHeader.Schema = new OpenApiSchema() { Type = "string" };
						mediaTypeHeader.Required = true;
						response.Headers.Add("X-Media-Type", mediaTypeHeader);
						//X-Content-Restricted
						var restrictedContentHeader = new OpenApiHeader();
						restrictedContentHeader.Description = "If the resource is not a full representation of the resource, partial is returned. Otherwise, this header is not included.";
						restrictedContentHeader.Schema = new OpenApiSchema() { Type = "string" };
						response.Headers.Add("X-Content-Restricted", restrictedContentHeader);
						if (showAddResponseHeader && !IsLegacy(apiConfiguration))
						{
							//x-total-count header
							var totalCountHeader = new OpenApiHeader();
							totalCountHeader.Description = "Specifies the total number of resources that satisfy the query.";
							totalCountHeader.Schema = new OpenApiSchema() { Type = "integer" };
							response.Headers.Add("X-Total-Count", totalCountHeader);
							// X-Max-Page-Size
							var maxPageSizeHeader = new OpenApiHeader();
							maxPageSizeHeader.Description = "Specifies the maximum number of resources returned in a response.";
							maxPageSizeHeader.Schema = new OpenApiSchema() { Type = "integer" };
							response.Headers.Add("X-Max-Page-Size", maxPageSizeHeader);
						}
						var mediaTypeContent = new OpenApiMediaType();
						mediaTypeContent.Schema = new OpenApiSchema() { Type = "array", Items = new OpenApiSchema() { Reference = new OpenApiReference() { Id = string.Format(schema_prefix, httpMethod, "response"), Type = ReferenceType.Schema } } };
						if (apiVersionConfig.ExtendedSchemaType.StartsWith("application"))
							response.Content.Add(apiVersionConfig.ExtendedSchemaType, mediaTypeContent);
						else
							response.Content.Add(string.Format("application/{0}", apiVersionConfig.ExtendedSchemaType), mediaTypeContent);
						break;
					}
				case "401":
					{
						response.Description = "Failure. Unauthorized indicates that the requested resource requires authentication.";
						break;
					}
				case "403":
					{
						response.Description = "Failure. Forbidden indicates that the user does not have the required permissions for the resource.";
						response.Content.Add(errorContentType, errMediaTypeContent);
						break;
					}
				case "404":
					{
						response.Description = "Failure. NotFound indicates that the requested resource does not exist on the server.";
						break;
					}
				case "405":
					{
						response.Description = "Failure. MethodNotAllowed indicates that the client tried to use an HTTP method that the resource does not allow.";
						response.Content.Add(errorContentType, errMediaTypeContent);
						break;
					}
				case "406":
					{
						response.Description = "Failure. NotAcceptable indicates that the request was not able to generate any of the client’s preferred media types, as indicated by the Accept request header.";
						response.Content.Add(errorContentType, errMediaTypeContent);
						break;
					}
				case "400":
					{
						response.Description = "Failure. BadRequest indicates that the server cannot or will not process the request due to something that is perceived to be a client error.";
						response.Content.Add(errorContentType, errMediaTypeContent);
						break;
					}
				case "500":
					{
						response.Description = "Server error, unexpected configuration or data";
						//response.Content.Add(errorContentType, errMediaTypeContent);
						break;
					}
				case "204":
					{
						response.Description = "OK, No Content.";
						break;
					}

			}


			return response;

		}

		/// <summary>
		/// Returning OpenApiServers Info
		/// </summary>
		private IList<OpenApiServer> BuildOpenApiServersProperty(EthosApiConfiguration apiConfiguration)
		{
			List<OpenApiServer> servers = new List<OpenApiServer>();
			servers.Add(AddEthosServers("Ethos Integration API U.S.", "https://integrate.elluciancloud.com"));
			servers.Add(AddEthosServers("Ethos Integration API Canada.", "https://integrate.elluciancloud.ca"));
			servers.Add(AddEthosServers("Ethos Integration API Europe.", "https://integrate.elluciancloud.ie"));
			servers.Add(AddEthosServers("Ethos Integration API Asia-Pacific.", "https://integrate.elluciancloud.com.au"));
			servers.Add(AddEthosServers("Custom server URL.", "{server_url}", "http://localhost"));
			return servers;
		}

		/// <summary>
		/// Returning OpenApiInfo object using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		private OpenApiInfo BuildOpenApiInfoProperty(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
		{
			var info = new OpenApiInfo();
			info.Title = apiConfiguration.ResourceName;
			info.Description = apiConfiguration.Description;
			info.Version = apiVersionConfig.ApiVersionNumber;
			info.AddExtension("x-source-system", new OpenApiString(sourceSystem));
			if (IsBpa(apiConfiguration))
			{
				info.AddExtension("x-source-name", new OpenApiString(apiConfiguration.ProcessId));
				info.AddExtension("x-source-title", new OpenApiString(apiConfiguration.ProcessDesc));
				info.AddExtension("x-api-type", new OpenApiString("bus-proc"));
			}
			else if (IsSpecBased(apiConfiguration))
			{
				if (string.IsNullOrEmpty(apiConfiguration.PrimaryTableName))
					info.AddExtension("x-source-name", new OpenApiString(apiConfiguration.PrimaryEntity));
				else
					info.AddExtension("x-source-name", new OpenApiString(string.Concat(apiConfiguration.PrimaryApplication, "-", apiConfiguration.PrimaryEntity, " ", apiConfiguration.PrimaryTableName)));
				var resourceName = apiConfiguration.ResourceName;
				if (!string.IsNullOrEmpty(resourceName))
				{
					if (resourceName.StartsWith("x-")) resourceName = resourceName.Substring(2);
					resourceName = resourceName.Replace('-', ' ');
					resourceName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resourceName);
					info.AddExtension("x-source-title", new OpenApiString(resourceName));
				}
				info.AddExtension("x-api-type", new OpenApiString("specification"));
			}
			else if (IsEthos(apiConfiguration))
			{
				var resourceName = apiConfiguration.ResourceName;
				if (!string.IsNullOrEmpty(resourceName))
				{
					resourceName = resourceName.Replace('-', ' ');
					resourceName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resourceName);
					info.AddExtension("x-source-title", new OpenApiString(resourceName));
				}
				info.AddExtension("x-api-type", new OpenApiString("ethos"));
			}
			else if (IsEthosEnabled(apiConfiguration))
			{
				var resourceName = apiConfiguration.ResourceName;
				if (!string.IsNullOrEmpty(resourceName))
				{
					resourceName = resourceName.Replace('-', ' ');
					resourceName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resourceName);
					info.AddExtension("x-source-title", new OpenApiString(resourceName));
				}
				info.AddExtension("x-api-type", new OpenApiString("web-ethos"));
			}
			else
			{
				var resourceName = apiConfiguration.ResourceName;
				if (!string.IsNullOrEmpty(resourceName))
				{
					resourceName = resourceName.Replace('-', ' ');
					resourceName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resourceName);
					info.AddExtension("x-source-title", new OpenApiString(resourceName));
				}
				info.AddExtension("x-api-type", new OpenApiString("web-nonethos"));
			}

			//display the staus from version if it is there, otherwise use it from the API status.
			string apiStatus = string.IsNullOrEmpty(apiVersionConfig.VersionReleaseStatus) ? apiConfiguration.ReleaseStatus : apiVersionConfig.VersionReleaseStatus;

			if (apiStatus.Equals("B"))
			{
				info.AddExtension("x-release-status", new OpenApiString("beta"));
				info.Version = string.Concat(apiVersionConfig.ApiVersionNumber, "-beta");
			}
			else if (apiStatus.Equals("R"))
				info.AddExtension("x-release-status", new OpenApiString("ga"));
			else
				info.AddExtension("x-release-status", new OpenApiString("prerelease"));

			// Convert source domain from abbreviation to full display name
			if (apiConfiguration.ApiDomain == "ADV")
				info.AddExtension("x-source-domain", new OpenApiString("Advancement"));
			else if (apiConfiguration.ApiDomain == "FA")
				info.AddExtension("x-source-domain", new OpenApiString("Financial Aid"));
			else if (apiConfiguration.ApiDomain == "CF")
				info.AddExtension("x-source-domain", new OpenApiString("Finance"));
			else if (apiConfiguration.ApiDomain == "CORE")
				info.AddExtension("x-source-domain", new OpenApiString("Foundation"));
			else if (apiConfiguration.ApiDomain == "HR")
				info.AddExtension("x-source-domain", new OpenApiString("Human Resources"));
			else if (apiConfiguration.ApiDomain == "REC")
				info.AddExtension("x-source-domain", new OpenApiString("Recruitment"));
			else if (apiConfiguration.ApiDomain == "ST")
				info.AddExtension("x-source-domain", new OpenApiString("Student"));
			else if (!string.IsNullOrEmpty(apiConfiguration.ApiDomain))
				info.AddExtension("x-source-domain", new OpenApiString(apiConfiguration.ApiDomain));
			else
				info.AddExtension("x-source-domain", new OpenApiString("Foundation"));

			return info;
		}

		/// <summary>
		/// Returning OpenApiInfo object using the API configuration info
		/// </summary>
		/// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
		/// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
		/// <param name="info">Existing Info object</param>
		private OpenApiInfo UpdateOpenApiInfoProperty(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, OpenApiInfo info)
		{
			if (info == null || string.IsNullOrEmpty(info.Title))
			{
				info = BuildOpenApiInfoProperty(apiConfiguration, apiVersionConfig);
			}
			else
			{
				OpenApiString openApiDomain = (info.Extensions.FirstOrDefault(dict => dict.Key == "x-source-domain").Value as OpenApiString);
				var origDomain = openApiDomain.Value;
				var newDomain = ConvertDomainFromEnum(ConvertDomain2Enum(apiConfiguration.ApiDomain));
				string origDomainString = "", newDomainString = "";
				if (origDomain != newDomain)
				{
					origDomainString = string.Concat("<b>(", origDomain, ")</b>", Environment.NewLine, Environment.NewLine);
					newDomainString = string.Concat("<b>(", newDomain, ")</b>", Environment.NewLine, Environment.NewLine);
					info.Description = string.IsNullOrEmpty(info.Description) ? apiConfiguration.Description : string.Concat(origDomainString, info.Description, Environment.NewLine, Environment.NewLine, newDomainString, apiConfiguration.Description);
					info.AddExtension("x-source-domain", new OpenApiString(newDomain));
				}
			}

			return info;
		}

		private OpenApiParameter GetPathItemParameters(string name, ParameterLocation input, bool required, string description, string schemaType, string componentSchemaPrefix, bool isGuid = false, bool hasRef = false)
		{
			var parameter = new OpenApiParameter();
			parameter.Name = name;
			parameter.Description = description;
			parameter.In = input;
			parameter.Required = required;
			if (!string.IsNullOrEmpty(schemaType) && !hasRef)
			{
				if (!isGuid)
					parameter.Schema = new OpenApiSchema() { Type = schemaType };
				else
					parameter.Schema = new OpenApiSchema() { Type = schemaType, Format = "guid", Pattern = GUID_PATTERN };
			}
			else if (!string.IsNullOrEmpty(schemaType) && hasRef)
			{
				if (name != "criteria")
				{
					var schema = new OpenApiSchema() { Type = schemaType, Reference = new OpenApiReference() { Id = string.Format(componentSchemaPrefix, name, "parameter"), Type = ReferenceType.Schema } };
					parameter.Schema = schema;
				}
				else
				{
					var schema = new OpenApiSchema() { Type = schemaType, Reference = new OpenApiReference() { Id = string.Format(componentSchemaPrefix, "query", "request"), Type = ReferenceType.Schema } };
					parameter.Schema = schema;
				}
			}
			return parameter;
		}

		private OpenApiServer AddEthosServers(string description, string url, string variable = "")
		{
			var server = new OpenApiServer();
			server.Description = description;
			server.Url = url;
			if (!string.IsNullOrEmpty(variable))
			{
				var serverVar = new OpenApiServerVariable();
				serverVar.Default = variable;
				var serverVarDict = new Dictionary<string, OpenApiServerVariable>
				{
					{ "server_url", serverVar }
				};
				server.Variables = serverVarDict;
			}
			return server;
		}

		/// <summary>
		/// Determine if the type is parent
		/// </summary>
		/// <param name="type">Type</param>
		/// <returns>boolean</returns>
		private bool IsParent(Type type)
		{
			return
				   (type.GetCustomAttributes(typeof(DataContractAttribute), true).Any()
					|| type.GetCustomAttributes(typeof(JsonObjectAttribute), true).Any());
		}

		/// <summary>
		/// Get the name to be displayed
		/// </summary>
		/// <param name="prop">PropertyInfo</param>
		/// <returns>string</returns>
		private string GetDisplayName(PropertyInfo prop)
		{
			if (prop == null)
				return string.Empty;
			try
			{
				var dataMemberAttributes = (DataMemberAttribute[])prop.GetCustomAttributes(typeof(DataMemberAttribute), false);
				if (dataMemberAttributes != null && dataMemberAttributes.Any())
					return dataMemberAttributes.FirstOrDefault(x => !(string.IsNullOrEmpty(x.Name))).Name;
				var jsonPropertyAttributes = (JsonPropertyAttribute[])prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false);
				if (jsonPropertyAttributes != null && jsonPropertyAttributes.Any())
					return jsonPropertyAttributes.FirstOrDefault(x => !(string.IsNullOrEmpty(x.PropertyName)))?.PropertyName;
			}
			catch (Exception ex)
			{
				var message = ex.Message;
			}

			return string.Empty;
		}

		/// <summary>
		/// If the property has a FilterPropertyAttribute, then return the name of the filter. 
		/// </summary>
		/// <param name="prop">propertyinfo</param>
		/// <returns>List of strings representing filter names associated to a property.</returns>
		private List<string> GetFilterName(PropertyInfo prop)
		{
			var filterNames = new List<string>();
			FilterPropertyAttribute[] customAttributes = (FilterPropertyAttribute[])prop.GetCustomAttributes(typeof(FilterPropertyAttribute), true);
			if (customAttributes != null)
			{
				foreach (var customAttribute in customAttributes)
				{
					if (customAttribute.Name.FirstOrDefault() != null)
					{
						if (!string.IsNullOrEmpty(customAttribute.Name.FirstOrDefault()) && !customAttribute.Ignore)
						{
							filterNames.Add(customAttribute.Name.FirstOrDefault());
						}
					}
				}
			}
			return filterNames;
		}

		/// <summary>
		/// Determine if a property has the FilterProperty attribute and should be displayed 
		/// </summary>
		/// <param name="prop">propertyinfo</param>
		/// <param name="filtername">string</param>
		/// <returns>boolean</returns>
		private bool IsFilter(PropertyInfo prop, string filtername)
		{
			FilterPropertyAttribute[] customAttributes = (FilterPropertyAttribute[])prop.GetCustomAttributes(typeof(FilterPropertyAttribute), false);  //prop.GetCustomAttributes();
			if (customAttributes != null)
			{
				foreach (var customAttribute in customAttributes)
				{
					if (customAttribute.Name != null)
					{
						if ((customAttribute.Name.Contains(filtername)) && (!customAttribute.Ignore))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// IterateProperties
		/// </summary>
		/// <param name="filterGroupName">string representing the filterGroup</param>
		/// <param name="T">property type</param>
		/// <param name="baseName">property name</param>
		/// <param name="checkForFilterGroup">validate the property is a member of a filterGroup.  used when a parent object is defined as 
		/// a filterable property, and all the children are then a memeber of that filter.</param>
		/// <returns>IEnumerable</returns>
		private IEnumerable<string> IterateProperties(string filterGroupName, Type T, string baseName = "", bool checkForFilterGroup = true)
		{
			var props = T.GetProperties();

			if (props == null)
				yield break;

			foreach (var property in props)
			{
				var name = GetDisplayName(property); // property.Name;
				var type = GetGenericType(property.PropertyType);

				// Is the property a parent type AND a member of a filter group
				// if so, then return all the children associated with it
				if ((IsParent(type)) && (IsFilter(property, filterGroupName)))
				{
					foreach (var info in IterateProperties(filterGroupName, type, name, false))
					{
						yield return string.IsNullOrEmpty(baseName) ? info : string.Format("{0}.{1}", baseName, info);
					}
				}
				//If the property is a parent that may have filterable properties, continue processing
				else if (IsParent(type))
				{
					foreach (var info in IterateProperties(filterGroupName, type, name, checkForFilterGroup))
					{
						yield return string.IsNullOrEmpty(baseName) ? info : string.Format("{0}.{1}", baseName, info);
					}
				}
				else
				{
					if ((!checkForFilterGroup) || (IsFilter(property, filterGroupName)))
					{
						var displayName = GetDisplayName(property);
						yield return string.IsNullOrEmpty(baseName) ? displayName : string.Format("{0}.{1}", baseName, displayName);
					}
				}
			}
		}

		/// <summary>
		/// Get the generic type for a list
		/// </summary>
		/// <param name="T">Type</param>
		/// <returns>Type</returns>
		private Type GetGenericType(Type T) => T.IsGenericType ? T.GetGenericArguments()[0] : T;

		private string ConvertJsonPropertyType(string jsonPropertyType, string jsonTitle = "", string conversion = "")
		{
			if ((!string.IsNullOrEmpty(jsonTitle)) && (jsonTitle.EndsWith("[]"))) return "array";
			if (string.IsNullOrEmpty(jsonPropertyType)) return "string";

			string openApiSchemaType = jsonPropertyType.ToLower();

			// For types that are no or should not be equal to themselves, update the value
			switch (openApiSchemaType)
			{
				case "decimal":
					openApiSchemaType = "number";
					break;
				case "long":
					openApiSchemaType = "integer";
					break;
				case "number":
					openApiSchemaType = string.IsNullOrEmpty(conversion) || conversion.Equals("MD0", StringComparison.OrdinalIgnoreCase) ? "integer" : "number";
					break;
				case "bool":
					openApiSchemaType = "boolean";
					break;
				case "date":
				case "time":
				case "datetime":
					openApiSchemaType = "string";
					break;
				default:
					break;
			}

			return openApiSchemaType;
		}

		private string GetJsonPropertyPattern(string jsonPropertyType) =>
			string.IsNullOrWhiteSpace(jsonPropertyType) ? string.Empty :
			jsonPropertyType.ToLower().Equals("date") ? "^(-?(?:[1-9][0-9]*)?[0-9]{4})-(1[0-2]|0[1-9])-(3[0-1]|0[1-9]|[1-2][0-9])$" :
			jsonPropertyType.ToLower().Equals("datetime") ? "^(-?(?:[1-9][0-9]*)?[0-9]{4})-(1[0-2]|0[1-9])-(3[0-1]|0[1-9]|[1-2][0-9])T(2[0-3]|[0-1][0-9]):([0-5][0-9]):([0-5][0-9])(\\.[0-9]+)?(Z|[+-](?:2[0-3]|[0-1][0-9]):[0-5][0-9])?$" :
			string.Empty;

		/// <summary>
		/// GetIdOpenApiSchemaFromExtensibleData
		/// </summary>
		/// <param name="apiVersionConfig">Domain.Base.Entities.EthosExtensibleData</param>
		/// <param name="apiConfiguration">EthosApiConfiguration</param>
		/// <returns>OpenApiSchema</returns>

		private OpenApiSchema GetIdOpenApiSchemaFromExtensibleDataAsync(EthosApiConfiguration apiConfiguration,
			Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
		{
			var IdSchema = new OpenApiSchema();
			IdSchema.Type = "object";
			var extendedDataListSorted = apiVersionConfig.ExtendedDataList.OrderBy(ex => ex.FullJsonPath).ToList();
			if (apiConfiguration.ColleagueKeyNames != null && apiConfiguration.ColleagueKeyNames.Any())
			{
				foreach (var key in apiConfiguration.ColleagueKeyNames)
				{
					//find the extendedData for each of the id field
					var extendedData = extendedDataListSorted.FirstOrDefault(prop => prop.ColleagueColumnName == key);
					if (extendedData != null)
					{
						var response = BuildOpenApiSchemaResponse(extendedData);
						IdSchema.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);
						IdSchema.Required.Add(extendedData.JsonTitle);

					}
				}
			}
			return IdSchema;
		}

		/// <summary>
		/// GetIdOpenApiSchemaFromExtensibleData
		/// </summary>
		/// <param name="apiVersionConfig">Domain.Base.Entities.EthosExtensibleData</param>
		/// <param name="apiConfiguration">EthosApiConfiguration</param>
		/// <param name="queryName"></param>
		/// <returns>OpenApiSchema</returns>

		private OpenApiSchema GetFilterOpenApiSchemaFromExtensibleDataAsync(EthosApiConfiguration apiConfiguration,
			Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string queryName = "")
		{
			var filterSchema = new OpenApiSchema();
			filterSchema.Type = "object";

			if (apiVersionConfig.ExtendedDataList.Count == 1 && apiVersionConfig.ExtendedDataList[0] != null && apiVersionConfig.ExtendedDataList[0].JsonTitle == "IEnumerable[]")
			{
				filterSchema.Type = "string";
			}
			else
			{
				var extendedDataListSorted = apiVersionConfig.ExtendedDataList.OrderBy(ex => ex.FullJsonPath).ToList();
				if (apiVersionConfig.ExtendedDataFilterList != null && apiVersionConfig.ExtendedDataFilterList.Any())
				{
					foreach (var filter in apiVersionConfig.ExtendedDataFilterList)
					{
						//find the extendedData for each of the id field
						try
						{
							var extendedData = extendedDataListSorted.FirstOrDefault(prop => prop.JsonPath == filter.JsonPath && prop.JsonTitle == filter.JsonTitle);
							if (extendedData != null)
							{
								if (!string.IsNullOrEmpty(queryName) && extendedData.filterNames != null && !extendedData.filterNames.Contains(queryName) && !IsSpecBased(apiConfiguration) && !IsBpa(apiConfiguration))
								{
									continue;
								}

								try
								{
									var propSplit =
									extendedData.FullJsonPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

									if (!propSplit.Any()) continue;

									var count = propSplit.Count();

									if (count == 1)
									{
										var response = BuildOpenApiSchemaResponse(extendedData);

										filterSchema.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);


									}
									else
									{
										var parentSchema = filterSchema;
										for (int i = 0; i < count; i++)
										{
											OpenApiSchema childSchema = null;

											if (childSchema == null && i < count - 1)
											{
												if (!parentSchema.Properties.TryGetValue(propSplit[i].Replace("[]", ""), out childSchema))
												{

													if (propSplit[i].Contains("[]"))
													{
														childSchema = new OpenApiSchema { Type = "array" };
													}
													else
													{
														childSchema = new OpenApiSchema { Type = "object" };
													}
													childSchema.Properties = new Dictionary<string, OpenApiSchema>();
													if (parentSchema.Type != "array")
														parentSchema.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
													else
													{
														if (parentSchema.Items == null)
														{
															var childSchemaItems = new OpenApiSchema() { Type = "object" };
															childSchemaItems.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
															parentSchema.Items = childSchemaItems;
														}
														else
														{
															var childSchemaItems = parentSchema.Items;
															if (!childSchemaItems.Properties.TryGetValue(propSplit[i].Replace("[]", ""), out childSchema))
															{
																if (propSplit[i].Contains("[]"))
																{
																	childSchema = new OpenApiSchema { Type = "array" };
																}
																else
																{
																	childSchema = new OpenApiSchema { Type = "object" };
																}
																childSchema.Properties = new Dictionary<string, OpenApiSchema>();
																childSchemaItems.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
															}
														}
													}
												}
												parentSchema = childSchema;
											}
											else if (parentSchema != null && i == count - 1)
											{
												if (parentSchema.Type == "array")
												{
													//childSchema = new OpenApiSchema { Type = "array" };
													if (parentSchema.Items == null)
													{
														var childSchemaItems = new OpenApiSchema() { Type = "object" };
														childSchemaItems.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
														parentSchema.Items = childSchemaItems;
													}
													else
													{
														var childSchemaItems = parentSchema.Items;
														childSchemaItems.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
														parentSchema.Items = childSchemaItems;
													}
													//parentSchema.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
												}
												else
												{
													parentSchema.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
												}
											}
										}

									}


								}
								catch (Exception e)
								{
									if (_logger != null)
									{
										_logger.LogError(e, "Failed to corretly generate schema.");
									}
								}
								//var response = BuildOpenApiSchemaResponse(extendedData);
								//filterSchema.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);
							}
						}
						catch (Exception)
						{
							_logger.LogError(filter.JsonTitle);
						}
					}
				}
			}
			return filterSchema;
		}

		/// <summary>
		/// Build Schema for NameQuery component
		/// </summary>
		/// <param name="apiVersionConfig">Domain.Base.Entities.EthosExtensibleData</param>
		/// <param name="apiConfiguration">EthosApiConfiguration</param>
		/// <returns>OpenApiSchema</returns>

		private OpenApiSchema GetNameQueryOpenApiSchemaFromExtensibleDataAsync(EthosApiConfiguration apiConfiguration,
			Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
		{
			var nameQuerySchema = new OpenApiSchema();
			nameQuerySchema.Type = "object";
			var nameQueryFilters = apiVersionConfig.ExtendedDataFilterList.Where(query => query.NamedQuery);
			var extendedDataListSorted = apiVersionConfig.ExtendedDataList.OrderBy(ex => ex.FullJsonPath).ToList();
			if (nameQueryFilters != null && nameQueryFilters.Any())
			{
				foreach (var query in nameQueryFilters)
				{
					var extendedData = new Domain.Base.Entities.EthosExtensibleDataRow(query.ColleagueColumnName, query.ColleagueFileName, query.JsonTitle, query.JsonPath, query.JsonPropertyType, "", query.ColleaguePropertyLength);
					if (extendedData != null)
					{
						extendedData.Description = query.Description;
						var response = BuildOpenApiSchemaResponse(extendedData, false, false);
						nameQuerySchema.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);
					}
				}
			}
			return nameQuerySchema;
		}
		/// <summary>
		/// GetOpenApiSchemaFromExtensibleData
		/// </summary>
		/// <param name="extendConfig">Domain.Base.Entities.EthosExtensibleData</param>
		/// <param name="ethosApiConfiguration">EthosApiConfiguration</param>
		/// <param name="putPostMethod">whether this is Put or POST</param>
		/// <returns>OpenApiSchema</returns>

		private OpenApiSchema GetOpenApiSchemaFromExtensibleDataAsync(EthosApiConfiguration ethosApiConfiguration,
			Domain.Base.Entities.EthosExtensibleData extendConfig, string putPostMethod = "")
		{
			OpenApiSchema schemaRootNode = null;
			var extendedDataListSorted = new List<Ellucian.Colleague.Domain.Base.Entities.EthosExtensibleDataRow>();
			extendedDataListSorted.AddRange(extendConfig.ExtendedDataList);
			SortedSet<string> requiredProperties = new SortedSet<string>();
			bool isPutPost = (putPostMethod == "put" || putPostMethod == "post");
			schemaRootNode = new OpenApiSchema()
			{

				Type = "object",
				//Properties = new Dictionary<string, OpenApiSchema>(),
				//AdditionalPropertiesAllowed = false,                   
				//Title = extendConfig.ApiResourceName + "_" + extendConfig.ApiVersionNumber

			};

			if (extendConfig.ExtendedDataList.Count == 1 && extendConfig.ExtendedDataList[0] != null && extendConfig.ExtendedDataList[0].JsonTitle == "IEnumerable[]")
			{
				schemaRootNode.Type = "string";
			}
			else
			{

				//set up the Id property
				if (!string.IsNullOrEmpty(ethosApiConfiguration.PrimaryGuidSource))
				{
					schemaRootNode.Properties.Add("id",
									new OpenApiSchema
									{
										Type = "string",
										Title = "ID",
										Format = "guid",
										Description = "The global identifier for the resource.",
										Pattern = GUID_PATTERN

									});
					//if this is not a schema for put/post then Id is going to be there in the response and hence it is required.
					if (putPostMethod != "put")
						requiredProperties.Add("id");
				}
				else
				{
					//do this for Business process API
					if (IsBpa(ethosApiConfiguration))
					{
						// we just have a string id property
						//we have Id object. 
						if (ethosApiConfiguration != null && IsCompositeKey(ethosApiConfiguration))
						{
							var IdProperty = new OpenApiSchema()
							{
								Type = "object",
								Title = "ID",
								Description = "The identifiers for the resource",

							};
							foreach (var key in ethosApiConfiguration.ColleagueKeyNames)
							{
								//find the extendedData for each of the id field
								var extendedData = extendedDataListSorted.FirstOrDefault(prop => prop.ColleagueColumnName == key);
								if (extendedData != null)
								{
									var response = BuildOpenApiSchemaResponse(extendedData);
									IdProperty.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);
									//remove from the list
									extendedDataListSorted.Remove(extendedData);
								}
							}
							schemaRootNode.Properties.Add("id", IdProperty);

						}
						else
						{
							//if there is only one key then we are going to display that as an Id. 
							if (ethosApiConfiguration != null && ethosApiConfiguration.ColleagueKeyNames != null)
							{
								var key = ethosApiConfiguration.ColleagueKeyNames.FirstOrDefault();
								int? length = null;
								//get additional information about the id property from the list if it is also in the list.
								var idDataInfo = extendedDataListSorted.FirstOrDefault(prop => prop.ColleagueColumnName == key);
								if (idDataInfo != null)
								{
									length = idDataInfo.ColleaguePropertyLength;
								}
								var extendedData = new Domain.Base.Entities.EthosExtensibleDataRow(key, ethosApiConfiguration.ColleagueFileNames.FirstOrDefault(), "id", "/", "string", "", length);
								extendedData.Description = "The identifier for the resource";
								if (idDataInfo != null)
								{
									extendedData.Conversion = idDataInfo.Conversion;
									extendedData.TransColumnName = idDataInfo.TransColumnName;
									extendedData.TransFileName = idDataInfo.TransFileName;
									extendedData.TransTableName = idDataInfo.TransTableName;
								}
								var response = BuildOpenApiSchemaResponse(extendedData);
								schemaRootNode.Properties.Add("id", response);
							}
						}
						//if this is not a schema for put/post then Id is going to be there in the response and hence it is required.
						if (putPostMethod != "put")
							requiredProperties.Add("id");
					}
				}
				//for put/post we need to remove those fields for the extendedDataList that are inquiry 
				foreach (var extendedData in extendedDataListSorted)
				{
					try
					{
						bool skipRecord = false;
						//skip inquiry fields in PUT/POST
						if (putPostMethod == "put" || putPostMethod == "post")
						{
							if ((extendedData.DatabaseUsageType != "K" || putPostMethod == "put") && extendConfig.InquiryFields != null && extendConfig.InquiryFields.Any() && extendConfig.InquiryFields.Contains(extendedData.ColleagueColumnName))
							{
								skipRecord = true;
							}
						}
						//skip preparedResponse in Get Response
						if (putPostMethod != "put" && putPostMethod != "post" && IspredefinedInputs(extendedData))
						{
							skipRecord = true;
						}
						if (!skipRecord)
						{
							var propSplit =
						extendedData.FullJsonPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

							if (!propSplit.Any()) continue;

							var count = propSplit.Count();
							//this is the main level
							if (count == 1)
							{
								if (!schemaRootNode.Properties.ContainsKey(extendedData.JsonTitle.Replace("[]", "")))
								{
									var response = BuildOpenApiSchemaResponse(extendedData);
									schemaRootNode.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);
									if (extendedData.Required)
									{
										requiredProperties.Add(extendedData.JsonTitle);
									}
								}
							}
							// this is embedded levels
							else
							{
								var parentSchema = schemaRootNode;
								SortedSet<string> requiredArrayProperties = new SortedSet<string>();
								for (int i = 0; i < count; i++)
								{
									OpenApiSchema childSchema = null;

									if (childSchema == null && i < count - 1)
									{
										if (!parentSchema.Properties.TryGetValue(propSplit[i].Replace("[]", ""), out childSchema))
										{

											if (propSplit[i].Contains("[]"))
											{
												childSchema = new OpenApiSchema { Type = "array" };
											}
											else
											{
												childSchema = new OpenApiSchema { Type = "object" };
											}
											childSchema.Properties = new Dictionary<string, OpenApiSchema>();
											if (parentSchema.Type != "array")
											{
												if (!parentSchema.Properties.ContainsKey(propSplit[i].Replace("[]", "")))
												{
													parentSchema.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
												}
											}
											else
											{
												if (parentSchema.Items == null)
												{
													if (!childSchema.Properties.ContainsKey(propSplit[i].Replace("[]", "")))
													{
														var childSchemaItems = new OpenApiSchema() { Type = "object" };
														childSchemaItems.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
														if (extendedData.Required)
														{
															if (childSchemaItems.Required == null && !childSchemaItems.Required.Any())
																childSchemaItems.Required = new SortedSet<string>() { propSplit[i] };
															else
																childSchemaItems.Required.Add(propSplit[i]);
														}
														parentSchema.Items = childSchemaItems;
													}
												}
												else
												{
													var childSchemaItems = parentSchema.Items;
													if (!childSchemaItems.Properties.TryGetValue(propSplit[i].Replace("[]", ""), out childSchema))
													{
														if (propSplit[i].Contains("[]"))
														{
															childSchema = new OpenApiSchema { Type = "array" };
														}
														else
														{
															childSchema = new OpenApiSchema { Type = "object" };
														}
														childSchema.Properties = new Dictionary<string, OpenApiSchema>();
														childSchemaItems.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
													}
													parentSchema.Items = childSchemaItems;
												}
											}
										}
										parentSchema = childSchema;
									}
									else if (parentSchema != null && i == count - 1)
									{
										if (parentSchema.Type == "array")
										{
											//childSchema = new OpenApiSchema { Type = "array" };
											if (parentSchema.Items == null)
											{
												var childSchemaItems = new OpenApiSchema() { Type = "object" };
												childSchemaItems.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
												if (extendedData.Required)
												{
													if (childSchemaItems.Required == null && !childSchemaItems.Required.Any())
														childSchemaItems.Required = new SortedSet<string>() { propSplit[i] };
													else
														childSchemaItems.Required.Add(propSplit[i]);
												}
												parentSchema.Items = childSchemaItems;
											}
											else
											{
												var childSchemaItems = parentSchema.Items;
												if (!childSchemaItems.Properties.ContainsKey(propSplit[i]))
												{
													childSchemaItems.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
													if (extendedData.Required)
													{
														if (childSchemaItems.Required == null && !childSchemaItems.Required.Any())
															childSchemaItems.Required = new SortedSet<string>() { propSplit[i] };
														else
															childSchemaItems.Required.Add(propSplit[i]);
													}
													parentSchema.Items = childSchemaItems;
												}
											}
										}
										else
										{
											if (!parentSchema.Properties.ContainsKey(propSplit[i]))
											{
												parentSchema.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
												if (extendedData.Required)
												{
													if (parentSchema.Required == null && !parentSchema.Required.Any())
														parentSchema.Required = new SortedSet<string>() { propSplit[i] };
													else
														parentSchema.Required.Add(propSplit[i]);
												}
											}
										}
									}
								}
							}
						}
					}
					catch (Exception e)
					{
						if (_logger != null)
						{
							_logger.LogError(e, "Failed to corretly generate schema.");
						}
					}
				}
			}
			//report the required properties for main level properties. 
			if (requiredProperties.Any())
				schemaRootNode.Required = requiredProperties;

			return schemaRootNode;
		}

		private OpenApiPaths SortOpenApiDocPaths(OpenApiPaths paths)
		{
			OpenApiPaths openApiPaths = new OpenApiPaths();
			var sortedPaths = paths.OrderBy(sp => sp.Key);
			foreach (var path in sortedPaths)
			{
				var pathKey = path.Key;
				var pathItem = path.Value;
				var sortedOperations = pathItem.Operations.OrderBy(sp => sp.Key);
				IDictionary<OperationType, OpenApiOperation> openApiOperations = new Dictionary<OperationType, OpenApiOperation>();
				foreach (var operation in sortedOperations)
				{
					openApiOperations.Add(operation.Key, operation.Value);
				}
				pathItem.Operations = openApiOperations;
				openApiPaths.Add(pathKey, pathItem);
			}

			return openApiPaths;
		}


		private OpenApiSchema BuildOpenApiSchemaResponse(Domain.Base.Entities.EthosExtensibleDataRow extendedData, bool isIdSchema = false, bool displayLineage = true)
		{
			var propertyType = ConvertJsonPropertyType(extendedData.JsonPropertyType, extendedData.JsonTitle, extendedData.Conversion);
			var OpenApiSchema = new OpenApiSchema
			{
				Type = propertyType,
				MaxLength = propertyType == "string" ? extendedData.ColleaguePropertyLength : null
			};
			//if this is an array, create items property 
			if (OpenApiSchema.Type == "array")
			{
				var arrayItems = new OpenApiSchema() { Type = "string", MaxLength = extendedData.ColleaguePropertyLength };
				OpenApiSchema.Items = arrayItems;
			}
			if (!string.IsNullOrEmpty(extendedData.Description))
			{
				OpenApiSchema.Description = extendedData.Description.Replace(DmiString._VM, ' ').Replace(DmiString._SM, ' ');
			}
			//OpenApiSchema.AdditionalPropertiesAllowed = false;

			if ((extendedData.JsonPropertyType.Equals("date", StringComparison.OrdinalIgnoreCase))
			|| (extendedData.JsonPropertyType.Equals("date-time", StringComparison.OrdinalIgnoreCase)))
			{
				OpenApiSchema.MaxLength = null;
			}
			else if (!string.IsNullOrEmpty(extendedData.TransType))
			{
				if (extendedData.TransType.Equals("G", StringComparison.OrdinalIgnoreCase))
				{
					OpenApiSchema.Pattern = GUID_PATTERN;
					OpenApiSchema.MaxLength = Guid.Empty.ToString().Length;
					OpenApiSchema.Format = "guid";
				}
			}
			else if (!string.IsNullOrEmpty(extendedData.Conversion))
			{
				OpenApiSchema.Format = extendedData.Conversion;
			}

			if (string.IsNullOrEmpty(OpenApiSchema.Pattern))
			{
				var pattern = GetJsonPropertyPattern(extendedData.JsonPropertyType);
				if (!string.IsNullOrEmpty(pattern))
				{
					OpenApiSchema.Pattern = pattern;
				}
			}

			if (!string.IsNullOrEmpty(extendedData.JsonTitle))
			{
				try
				{
					var jsonTitle = extendedData.JsonTitle.Replace("[]", "");
					OpenApiSchema.Title = string.Concat(Char.ToUpperInvariant(jsonTitle[0]), jsonTitle.Substring(1));
				}
				catch (Exception)
				{
					OpenApiSchema.Title = extendedData.JsonTitle;
				}

			}
			if (displayLineage)
			{
				//do not show the lineage for prepared response
				if (!string.IsNullOrEmpty(extendedData.ColleagueColumnName) && !IspredefinedInputs(extendedData))
				{
					if (!extendedData.ColleagueColumnName.EndsWith(".TRANSLATION"))
						OpenApiSchema.AddExtension("x-lineageReferenceObject", new OpenApiString(extendedData.ColleagueColumnName));
					else
					{
						if (!string.IsNullOrEmpty(extendedData.TransColumnName))
							OpenApiSchema.AddExtension("x-lineageReferenceObject", new OpenApiString(extendedData.TransColumnName));
					}


				}
				//do not show the lineage for prepared response
				if (!string.IsNullOrEmpty(extendedData.TransFileName) && !IspredefinedInputs(extendedData) && !extendedData.ColleagueColumnName.EndsWith(".TRANSLATION"))
				{
					if (!string.IsNullOrEmpty(extendedData.TransTableName))
						OpenApiSchema.AddExtension("x-lineageLookupReferenceObject", new OpenApiString(string.Concat(extendedData.TransFileName, " - ", extendedData.TransTableName)));
					else
						OpenApiSchema.AddExtension("x-lineageLookupReferenceObject", new OpenApiString(extendedData.TransFileName));
				}
			}
			//for prepared response, we want to show the default value as well as possible values. 
			// 
			if (IspredefinedInputs(extendedData))
			{

				//this has the default value
				OpenApiSchema.Default = new OpenApiString(extendedData.TransType);
				// this has the potential values
				if (!string.IsNullOrEmpty(extendedData.TransFileName))
				{
					//var responseValues = new List<OpenApiString>();
					if (extendedData.TransFileName.Contains(";"))
					{
						var values = extendedData.TransFileName.Split(';');
						foreach (var val in values)
						{
							OpenApiSchema.Enum.Add(new OpenApiString(val));
						}
					}
					else
					{
						OpenApiSchema.Enum.Add(new OpenApiString(extendedData.TransFileName));
					}
				}
			}

			// Add any enumerations to schema property
			extendedData.Enumerations.ForEach(val => OpenApiSchema.Enum.Add(new OpenApiString(val.EnumerationValue)));

			return OpenApiSchema;
		}

		/// <summary>
		/// Returns true if data field is predefinedInputs
		/// </summary>
		/// <param name="extendedData">EDMV column data</param>
		private bool IspredefinedInputs(Domain.Base.Entities.EthosExtensibleDataRow extendedData)
		{
			bool ispredefinedInputs = false;
			if (extendedData != null && !string.IsNullOrEmpty(extendedData.JsonPath) && extendedData.JsonPath.Contains("predefinedInputs"))
				ispredefinedInputs = true;
			return ispredefinedInputs;
		}

		/// <summary>
		/// Gets all enum member values for given enum type
		/// </summary>
		/// <param name="fieldType">enum type</param>
		/// <returns>enum member values</returns>
		private static string[] GetEnumNames(Type fieldType)
		{
			string[] enumNames;
			var names = Enum.GetNames(fieldType);
			var enumMemberValues = new List<string>();
			foreach (var name in names)
			{
				if (((EnumMemberAttribute[])fieldType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).Any())
				{
					var enumMemberAttribute = ((EnumMemberAttribute[])fieldType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
					if (enumMemberAttribute != null)
						enumMemberValues.Add(enumMemberAttribute.Value);
				}
				else
				{
					enumMemberValues.Add(name);
				}
			}
			enumNames = enumMemberValues.ToArray();
			return enumNames;
		}

		private static string GetJsonConverterTypeName(PropertyInfo pi)
		{
			string converterTypeName = "";
			if (pi != null)
			{
				var jsonConverterAttributes = (JsonConverterAttribute[])pi.GetCustomAttributes(typeof(JsonConverterAttribute), false);
				if (jsonConverterAttributes != null && jsonConverterAttributes.Any())
				{
					var converterType = jsonConverterAttributes.FirstOrDefault(x => x.ConverterType != null && !string.IsNullOrEmpty(x.ConverterType.Name));
					converterTypeName = converterType.ConverterType.Name;
				}
			}
			return converterTypeName;
		}

		private static string GetJsonPropertyTypeForDateTime(PropertyInfo pi)
		{
			string jsonConverterTypeName = GetJsonConverterTypeName(pi);
			string jsonPropertyType = !string.IsNullOrEmpty(jsonConverterTypeName) && jsonConverterTypeName == nameof(DateOnlyConverter) ? "date" : "datetime";
			return jsonPropertyType;
		}

		/// <summary>
		/// Gets the api name
		/// </summary>
		/// <param name="routeTemplate"></param>
		/// <returns></returns>
		private string GetApiName(string routeTemplate)
		{
			// Allow for multi-part routes where the API name is each part without any parameters.
			string apiName = routeTemplate;
			var routeSplit = routeTemplate.Split('/');
			if (routeSplit.Count() >= 2)
			{
				apiName = string.Empty;
				foreach (var routePiece in routeSplit)
				{
					if (!routePiece.Contains('{') && routePiece != "qapi")
						apiName = string.Concat(apiName, "-", routePiece);
				}
				apiName = apiName.TrimStart('-');
			}

			return apiName;
		}
	}



	class MetadataVersionNumberComparer : IComparer<string>
	{
		/// <summary>
		/// Compare strings which represent semantic version numbers and/or integers
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns>x is greater return 1 else if y is greater return -1</returns>
		public int Compare(string x, string y)
		{
			try
			{
				//remove -beta from the version comparision
				x = x.Replace("-beta", "");
				y = y.Replace("-beta", "");

				if (x == y || x == string.Empty) return 1;

				var first = x.Split(new char[] { '.' }).Select(xx => int.Parse(xx)).ToList();
				var second = y.Split(new char[] { '.' }).Select(yy => int.Parse(yy)).ToList();

				var stackFirst = new Queue<int>(first);
				var stackSecond = new Queue<int>(second);

				var largest = first.Count > second.Count ? first.Count : second.Count;

				for (int i = 0; i < largest; i++)
				{
					if ((stackFirst.Count == 0) && (stackSecond.Count > 0))
					{
						return -1;
					}
					else if ((stackFirst.Count > 0) && (stackSecond.Count == 0))
					{
						return 1;
					}
					else
					{
						var s1 = stackFirst.Dequeue();
						var s2 = stackSecond.Dequeue();

						if (s1 > s2)
						{
							return 1;
						}
						else if (s1 < s2)
						{
							return -1;
						}
						else continue;
					}
				}

				return 0;
			}
			catch
			{
				//display error message for invalid version like x
				throw new KeyNotFoundException("Requested version is not supported.");
			}
		}
	}


}

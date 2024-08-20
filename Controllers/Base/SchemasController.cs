// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.
// this controller is obselete as it is replaced by metadata endpoint. However
// we are leaving the code here as it has some important logic that might be needed in the future. 
// for generating schemas for self service APIs. 

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.EthosExtend;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific version information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    [Route("/[controller]/[action]")]
    public class SchemasController : BaseCompressedApiController
    {
        private const string EEDM_WEBAPI_SCHEMAS_CACHE_KEY = "EEDM_WEBAPI_SCHEMAS_CACHE_KEY";
        private const string appJsonContentType = "application/json";
        private const string mediaFormat = "application/vnd.hedtech.integration";
        private const string httpMethodConstraintName = "httpMethod";
        private const string headerVersionConstraintName = "headerVersion";
        private const string isEEdmSupported = "isEedmSupported";
        private const string isEthosEnabled = "isEthosEnabled";

        private const string GUID_PATTERN = "^[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}$";

        private ICacheProvider _cacheProvider;
        private readonly IBulkLoadRequestService _bulkLoadRequestService;
        private readonly ILogger _logger;
        private readonly IEthosApiBuilderService _ethosApiBuilderService;
        private readonly List<string> versionedSupportedMethods = new List<string>() { "put", "post", "get" };
        private readonly List<string> versionlessSupportedMethods = new List<string>() { "get", "delete" };

        /// <summary>
        ///SchemasController
        /// </summary>
        [Obsolete]
        public SchemasController(IBulkLoadRequestService bulkLoadRequestService,
            IEthosApiBuilderService ethosApiBuilderService, ICacheProvider cacheProvider, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _cacheProvider = cacheProvider;
            _bulkLoadRequestService = bulkLoadRequestService;
            this._logger = logger;
            _ethosApiBuilderService = ethosApiBuilderService;
        }

        /// <summary>
        /// Retrieves version information for the Colleague Web API.
        /// </summary>
        /// <returns>Version information.</returns>
        [ValidateQueryStringFilter(new string[] { "criteria" }, false, true)]
        [Obsolete]
        [HttpGet]
        [HeaderVersionRoute("/schemas/{selectedSchema}/{selectedSchema2}/{selectedSchema3}", "*", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetApiSchemas", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<object>>> GetSchemas(
            [FromQuery] string selectedSchema = "", [FromQuery] string selectedSchema2 = "",
           [FromQuery] string selectedSchema3 = "", [FromQuery] string criteria = "")
        {

            bool bypassCache = false;
            if ((Request != null) && (Request.GetTypedHeaders().CacheControl != null))
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(selectedSchema))
            {

                return CreateHttpResponseException(new IntegrationApiException("",
                    new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.",
                    "resource path segment is required.")));
            }
            if (!string.IsNullOrEmpty(selectedSchema2))
                selectedSchema = string.Concat(selectedSchema.Trim() + "/" + selectedSchema2.Trim());
            if (!string.IsNullOrEmpty(selectedSchema3))
                selectedSchema = string.Concat(selectedSchema.Trim() + "/" + selectedSchema3.Trim());


            string title = string.Empty;
            if (!string.IsNullOrEmpty(criteria))
            {

                var jObject = JObject.Parse(criteria);
                if (jObject != null)
                {
                    JToken token = null;
                    var found = jObject.TryGetValue("id", out token);
                    // var token = jObject.SelectToken("title");
                    if (found && token != null)
                    {
                        title = token.ToString();
                        // JObject parse will potentially remove the '+' sign from the  filter.  Need to add that back in
                        if (title.EndsWith(" json"))
                        {
                            //title.Replace(" json", "+json");
                            int place = title.LastIndexOf(" json");
                            if (place != -1)
                            {
                                title = title.Remove(place, 5).Insert(place, "+json");
                            }
                        }
                    }
                    else
                    {
                        throw new ColleagueWebApiException(

                                    jObject.Properties().FirstOrDefault().Name + " is an invalid query parameter for filtering.");
                    }
                }
            }

            string acceptHeader = string.Empty;
            if (Request != null && Request.Headers != null && !StringValues.IsNullOrEmpty(Request.Headers.Accept))
            {
                acceptHeader = Request.Headers.Accept.ToString();
                if (acceptHeader == "*/*")
                {
                    acceptHeader = "";
                }
            }

            List<ApiSchemas> resourcesDtoList = new List<ApiSchemas>();

            var routeCollection = new Microsoft.AspNetCore.Routing.Route[0]; // Configuration.Routes;

            var httpRoutes = routeCollection
                   .Where(r => r.Defaults.Keys != null && (r.Defaults.Keys.Contains(isEEdmSupported) && r.Defaults[isEEdmSupported].Equals(true) ||
                   r.Defaults.Keys.Contains(isEthosEnabled) && r.Defaults[isEthosEnabled].Equals(true)))
                     .ToList();

            if (!string.IsNullOrEmpty(selectedSchema))
            {
                httpRoutes = httpRoutes.Where(x => x.RouteTemplate.StartsWith(selectedSchema)).ToList();
            }
            resourcesDtoList = await GetSchemasAsync(httpRoutes, selectedSchema, title, bypassCache);
            if (!string.IsNullOrEmpty(selectedSchema))
            {
                resourcesDtoList = resourcesDtoList.Where(rd => rd.Name == selectedSchema).ToList();
            }
            if (resourcesDtoList == null && selectedSchema.Split('/').Count() > 0)
            {
                resourcesDtoList = resourcesDtoList.Where(rd => rd.Name == selectedSchema[0].ToString()).ToList();
            }
            var represenation = resourcesDtoList.OrderBy(item => item.Name).SelectMany(sc => sc.Representations);
            return Ok(represenation.Where(sc => !string.IsNullOrEmpty(sc.Schema)).Select(sc => JsonConvert.DeserializeObject(sc.Schema)) as IEnumerable<object>);
        }


        /// <summary>
        /// Update not supported
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        [HttpPut]
        [Obsolete]
        public IActionResult PutSchemas([FromBody] object schema)
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
        [Obsolete]
        public IActionResult PostSchemas([FromBody] object schema)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete not supported
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete]
        [Obsolete]
        public IActionResult DeleteSchemas(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


        /// <summary>
        /// Gets all the schemas
        /// </summary>
        /// <param name="httpRoutes"></param>
        /// <param name="selectedSchema"></param>
        /// <param name="filterVersion"></param>
        /// <param name="bypassCache"></param>
        [Obsolete]
        private async Task<List<ApiSchemas>> GetSchemasAsync(List<Route> httpRoutes,
            string selectedSchema = "", string filterVersion = "", bool bypassCache = false)
        {

            var resourcesList = new List<ApiSchemas>();

            if (bypassCache == false)
            {
                if (_cacheProvider != null && _cacheProvider.Contains(EEDM_WEBAPI_SCHEMAS_CACHE_KEY))
                {
                    resourcesList = _cacheProvider[EEDM_WEBAPI_SCHEMAS_CACHE_KEY] as List<ApiSchemas>;
                    return resourcesList;
                }
            }

            #region data model resources 

            // At the moment, we only expose schemas for spec based endpoints, however, this section
            // of code can be uncommented to expose native data model APIs.

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

            if (!string.IsNullOrEmpty(selectedSchema))
            {
                httpRoutes = httpRoutes.Where(x => x.RouteTemplate.StartsWith(selectedSchema)).ToList();
            }

            foreach (var httpRoute in httpRoutes)
            {
                try
                {
                    //Get the route template
                    var routeTemplate = httpRoute.RouteTemplate;
                    var apiName = string.Empty;
                    //var versionless = false;

                    //gets api name
                    apiName = routeTemplate;

                    ApiSchemas resourceDto = null;
                    JsonSchema returnedSchema = null;
                    //Dtos.SchemaRepresentation representationDto = null;

                    //Allowed http method
                    var allowedMethod = string.Empty;

                    //get all constraints
                    var constraints = httpRoute.Constraints;

                    if (((HttpMethodRouteConstraint)constraints[httpMethodConstraintName]).AllowedMethods != null &&
                        ((HttpMethodRouteConstraint)constraints[httpMethodConstraintName]).AllowedMethods.Any())
                    {
                        allowedMethod = ((HttpMethodRouteConstraint)httpRoute.Constraints[httpMethodConstraintName]).AllowedMethods.ToList()[0];
                    }

                    var headerVersionConstraintConstraintName = ((Web.Http.Routes.HeaderVersionConstraint)constraints[headerVersionConstraintName]);
                    if (headerVersionConstraintConstraintName != null)
                    {
                        var headerVersionConstraint = headerVersionConstraintConstraintName.GetType();

                        var pField = headerVersionConstraint
                                          .GetField("_customMediaTypes", BindingFlags.NonPublic | BindingFlags.Instance);
                        headerVersionConstraintValue = (string[])pField.GetValue(((Web.Http.Routes.HeaderVersionConstraint)constraints[headerVersionConstraintName]));
                    }

                    // If this is a data model API and contains a route with /id or /guid then skip it.  This includes skipping the PUT
                    //  slightly conccerned that we might have a route that is PUT only, and this is going to get skipped.
                    if ((headerVersionConstraintValue != null) && (headerVersionConstraintValue.Any()) &&
                        (headerVersionConstraintValue[0].Contains(mediaFormat)))

                    {
                        var splitRoute = routeTemplate.Split('/');

                        if ((splitRoute.Count() > 1) &&
                            (((string.Equals("id", splitRoute[1].ToString(), StringComparison.OrdinalIgnoreCase))
                            || (string.Equals("guid", splitRoute[1].ToString(), StringComparison.OrdinalIgnoreCase)))))
                            continue;
                    }
                    else
                    {
                        continue;  // we are not exposing self-service endpoints at this time...
                    }
                    try
                    {
                        // this would apply to PUT/POST where we would want to use the content-type to define the xMediaType
                        if (constraints.ContainsKey("contentType"))
                        {
                            var contentTypeConstraint = ((HttpMethodRouteConstraint)constraints["contentType"])
                                            .GetType();

                            var pField2 = contentTypeConstraint
                                              .GetField("contentType", BindingFlags.NonPublic | BindingFlags.Instance);
                            var contentType = (string)pField2.GetValue((HttpMethodRouteConstraint)constraints["contentType"]);
                            headerVersionConstraintValue = new string[] { contentType };
                        }
                    }
                    catch (Exception ex)
                    { //do not throw 
                    }

                    // If we are provided with a filtering value, then check here
                    if ((!string.IsNullOrWhiteSpace(filterVersion)) && (headerVersionConstraintValue != null) && (!string.IsNullOrWhiteSpace(headerVersionConstraintValue[0])))
                    {
                        if ((!filterVersion.Equals(headerVersionConstraintValue[0]))
                             && (!filterVersion.Equals(string.Concat(apiName + "_" + headerVersionConstraintValue[0]))))
                        {

                            continue;
                        }
                    }

                    //Check to see if resource list has the resource
                    if (!resourcesList.Any(res => res.Name.Equals(apiName, StringComparison.OrdinalIgnoreCase))
                      && (resourceDto == null))
                    {
                        resourceDto = new ApiSchemas() { Name = apiName };
                        resourceDto.Representations = new List<Dtos.SchemaRepresentation>();
                        resourcesList.Add(resourceDto);
                    }

                    else
                        resourceDto = resourcesList.FirstOrDefault(res => res.Name.Equals(apiName, StringComparison.OrdinalIgnoreCase));

                    var tempXMediaType = string.Empty;
                    if (headerVersionConstraintValue != null && headerVersionConstraintValue.Any() && headerVersionConstraintValue[0].Contains(mediaFormat))
                    {
                        tempXMediaType = headerVersionConstraintValue[0];

                        if (resourceDto.Representations != null && resourceDto.Representations
                                .Any(r => r.XMediaType.Equals(tempXMediaType, StringComparison.OrdinalIgnoreCase)))
                            continue;
                    }
                    var versionOnly = ExtractVersionNumberOnly(tempXMediaType);
                    object controller = string.Empty;
                    object action = string.Empty;
                    object requestedContentType = string.Empty;


                    httpRoute.Defaults.TryGetValue("action", out action);
                    httpRoute.Defaults.TryGetValue("controller", out controller);
                    httpRoute.Defaults.TryGetValue("RequestedContentType", out requestedContentType);

                    var controlleraction = controlleractionlist
                        .FirstOrDefault(x => x.Controller == string.Concat(controller.ToString(), "Controller"));
                    var controllerType = controlleraction.DeclaringType;

                    var controllerAction = action.ToString();

                    if (controllerType != null)
                    {
                        MethodInfo[] methods = controllerType.GetMethods().Where(m => m.Name == controllerAction).ToArray();
                        if (methods != null && methods.Count() > 0)
                        {
                            if (methods.Count() > 1)
                            {
                                // the above LINQ should result in just one, if not, throwing an exception here to make future troubleshooting easier
                                throw new ColleagueWebApiException("ApiDocumentationProvider.GetActionMethodInfo found more than one matching method.");
                            }
                        }
                        returnedSchema = GetJsonSchemaForApi(methods.FirstOrDefault());
                    }
                    if (returnedSchema != null)
                    {
                        returnedSchema.Id = apiName + "_" + tempXMediaType;
                        var newRepresentation = new Dtos.SchemaRepresentation()
                        {
                            XMediaType = tempXMediaType,
                            VersionNumber = versionOnly,
                            Schema = returnedSchema.ToString()
                        };

                        resourceDto.Representations.Add(newRepresentation);
                    }
                }
                catch (Exception ex)
                {
                    var e = ex.Message;
                    //no need to throw since not all routes will have _customMediaTypes field
                }
            }
            #endregion

            #region extendedResources

            SchemaVersionNumberComparer versionNumberComparer = null;
            try
            {
                var newResourcesDto = false;
                var allExtendedEthosConfigurations = (await _ethosApiBuilderService.GetAllExtendedEthosConfigurations(bypassCache, true)).ToList();

                allExtendedEthosConfigurations = (allExtendedEthosConfigurations.Where(x => x.ApiResourceName == selectedSchema || x.ParentApi == selectedSchema)).ToList(); // "x-parking-tickets")).ToList();         

                foreach (var extendedEthosConfiguration in allExtendedEthosConfigurations)
                {
                    var originalApiName = extendedEthosConfiguration.ApiResourceName;

                    var parentApi = extendedEthosConfiguration.ParentApi;

                    var apiName = string.IsNullOrEmpty(parentApi) ? originalApiName : parentApi;

                    ApiSchemas resourceDto = null;

                    if (!resourcesList.Any(res => res.Name.Equals(apiName, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (resourceDto == null)
                            resourceDto = new ApiSchemas() { Name = apiName };
                    }

                    dynamic dto = new ExpandoObject();


                    resourceDto = resourcesList.FirstOrDefault(res => res.Name.Equals(apiName, StringComparison.OrdinalIgnoreCase));

                    if (resourceDto == null)
                    {
                        newResourcesDto = true;
                        resourceDto = new ApiSchemas() { Name = apiName };
                    }

                    if (resourceDto.Representations == null) resourceDto.Representations = new List<Dtos.SchemaRepresentation>();

                    var represDto = resourceDto.Representations.FirstOrDefault(repr => repr.XMediaType.Contains(appJsonContentType));
                    EthosResourceRouteInfo routeInfo = new EthosResourceRouteInfo()
                    {
                        ResourceName = apiName
                    };
                    var ethosApiConfiguration = await _ethosApiBuilderService.GetEthosApiConfigurationByResource(routeInfo);

                    if ((represDto == null) && (string.IsNullOrEmpty(parentApi)))
                    {
                        var supported = extendedEthosConfiguration.HttpMethodsSupported
                            .Where(z => versionlessSupportedMethods.Contains(z.ToLower()))
                            .Select(x => x.ToLower()).ToList();


                        var schemaAppJson = await GetJsonSchemaFromExtensibleDataAsync(ethosApiConfiguration, extendedEthosConfiguration, apiName + "_" + appJsonContentType);

                        var newRepresentationSchema = new Dtos.SchemaRepresentation()
                        {
                            XMediaType = appJsonContentType,
                            VersionNumber = extendedEthosConfiguration.ApiVersionNumber,
                            //Schema = this.GetJsonSchemaFromDict(extendedEthosConfiguration.ExtendedDataList).ToString()
                            Schema = schemaAppJson


                        };
                    }
                    // alternative representations will never be the default route
                    else if (string.IsNullOrEmpty(parentApi))
                    {
                        // If a custom endpoint has already been added, and defined as versionless, 
                        // then we need to determine if this route should be the versionless instead.  
                        // The versionless route will always have the largest version number.                         
                        if (versionNumberComparer == null)
                        {
                            versionNumberComparer = new SchemaVersionNumberComparer();
                        }
                        if (versionNumberComparer.Compare(extendedEthosConfiguration.ApiVersionNumber, represDto.VersionNumber) == 1)
                        {
                            represDto.VersionNumber = extendedEthosConfiguration.ApiVersionNumber;
                        }
                    }


                    var xMediaType = (string.IsNullOrEmpty(parentApi))
                        ? mediaFormat + ".v" + extendedEthosConfiguration.ApiVersionNumber + "+json"
                        : mediaFormat + "." + originalApiName + ".v" + extendedEthosConfiguration.ApiVersionNumber + "+json";

                    if ((!string.IsNullOrWhiteSpace(filterVersion)) && (!string.IsNullOrWhiteSpace(xMediaType))
                        && (!filterVersion.Equals(xMediaType)) && (!filterVersion.Equals(string.Concat(apiName + "_" + xMediaType))))
                    {
                        continue;
                    }

                    var versionSupported = extendedEthosConfiguration.HttpMethodsSupported
                            .Where(z => versionedSupportedMethods.Contains(z.ToLower()))
                            .Select(x => x.ToLower()).ToList();

                    var jsonSchema = string.Empty;

                    JsonSchema existingSchema = null;
                    var existing = resourceDto.Representations
                     .Where(x => (x.XMediaType.Equals(xMediaType))
                          && (x.VersionNumber.Equals(extendedEthosConfiguration.ApiVersionNumber)))
                     .FirstOrDefault();

                    if (existing != null && !string.IsNullOrEmpty(existing.Schema))
                        existingSchema = JsonSchema.Parse(existing.Schema);

                    if (!string.IsNullOrEmpty(parentApi))
                    {
                        var routeInfoAltRep = routeInfo = new EthosResourceRouteInfo();
                        routeInfoAltRep.ResourceName = extendedEthosConfiguration.ApiResourceName;
                        var ethosApiConfigurationAltRep = await _ethosApiBuilderService.GetEthosApiConfigurationByResource(routeInfoAltRep);
                        jsonSchema = await GetJsonSchemaFromExtensibleDataAsync(ethosApiConfigurationAltRep, extendedEthosConfiguration,
                            apiName + "_" + xMediaType, existingSchema);
                    }
                    else
                    {

                        jsonSchema = await GetJsonSchemaFromExtensibleDataAsync(ethosApiConfiguration, extendedEthosConfiguration,
                             apiName + "_" + xMediaType, existingSchema);
                    }

                    if (!string.IsNullOrEmpty(jsonSchema))
                    {
                        var newRepresentation = new Dtos.SchemaRepresentation()
                        {
                            XMediaType = xMediaType,
                            VersionNumber = extendedEthosConfiguration.ApiVersionNumber,
                            Schema = jsonSchema
                        };

                        if (existing != null)
                            existing.Schema = jsonSchema;
                        else
                            resourceDto.Representations.Add(newRepresentation);


                        if (newResourcesDto)
                        {
                            resourcesList.Add(resourceDto);
                            newResourcesDto = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //do nothing for now....
            }
            #endregion

            _cacheProvider.Add(EEDM_WEBAPI_SCHEMAS_CACHE_KEY, resourcesList, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
            });
            return resourcesList;
        }

        /// <summary>
        /// Extract the version number from a customMediaType.  Extracts integers or semantic versions.
        /// </summary>
        /// <param name="original"></param>
        /// <returns>Version number.  May contain none, or unknown number of decimals</returns>
        [Obsolete]
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
        /// publishing the major version route
        /// </summary>
        /// <param name="representations"></param>
        /// <param name="representationToCompare"></param>    
        [Obsolete]
        private void AddMajorVersion(List<Dtos.SchemaRepresentation> representations, Dtos.SchemaRepresentation representationToCompare)
        {
            if (representationToCompare == null || representations == null
                || string.IsNullOrEmpty(representationToCompare.VersionNumber)
                || string.IsNullOrEmpty(representationToCompare.XMediaType))
                return;
            try
            {
                //versionNumber is expected to be a string in the format {i} or {i.i.i} where i is an integer
                var versionNumber = representationToCompare.VersionNumber;
                //var xMediaType = representationToCompare.XMediaType;
                var first = versionNumber.Split(new char[] { '.' })
                       .Select(xx => int.Parse(xx)).ToList();

                // if the version isnt found, or is already a single digit, then we dont need to do anything else
                if (first == null || !first.Any() || first.Count == 1)
                {
                    return;
                }

                var majorVersionXMediaType = representationToCompare
                        .XMediaType.Replace(versionNumber, first[0].ToString());
                //does the major version representation already exist for this mediaType?
                var found = representations.Any(r => r.XMediaType == majorVersionXMediaType);

                if (found)
                    return;

                var majorVersionRepresentation = new Dtos.SchemaRepresentation()
                {
                    MajorVersionAdded = representationToCompare.MajorVersionAdded,
                    VersionNumber = representationToCompare.VersionNumber,
                    XMediaType = representationToCompare.XMediaType
                };
                majorVersionRepresentation.XMediaType = majorVersionXMediaType;
                // majorVersionRepresentation.MajorVersionAdded = true;
                representations.Add(majorVersionRepresentation);
            }
            catch (Exception)
            {  // do nothing 
            }
            return;

        }

        /// <summary>
        /// Get Filters and named queries
        /// </summary>
        /// <param name="filters">collection of string</param>
        /// <param name="namedQueries">collection of named query objects</param>
        /// <param name="T">type</param>
        /// <param name="controllerAction">controller method name</param>
        /// <returns>collection of filters used in criteria filtergroup</returns>
        [Obsolete]
        private List<string> GetFilters(List<string> filters, List<NamedQuery> namedQueries, Type T, string controllerAction)
        {
            var queryStringFilters = (QueryStringFilterFilter[])T.GetMethod(controllerAction)
                  .GetCustomAttributes(typeof(QueryStringFilterFilter), false);

            if (queryStringFilters != null)
            {
                var queryStringFilter = queryStringFilters.FirstOrDefault(x => x.FilterGroupName == "criteria");

                if (queryStringFilter != null)
                {
                    var filterCacheKey = string.Concat("criteria_", controllerAction, "_", T.FullName);

                    if (_cacheProvider != null && _cacheProvider.Contains(filterCacheKey))
                    {
                        filters = _cacheProvider[filterCacheKey] as List<string>;
                    }
                    else
                    {
                        filters = IterateProperties("criteria", queryStringFilter.FilterType).ToList();

                        _cacheProvider.Add(filterCacheKey, filters, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions()
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
                        });
                    }
                }

                var namedFilters = queryStringFilters.Where(x => x.FilterGroupName != "criteria");
                if (namedFilters != null && namedFilters.Any())
                {
                    foreach (var namedFilter in namedFilters)
                    {
                        NamedQuery namedQuery = new NamedQuery();
                        List<string> namedQueryFilters = null;

                        namedQuery.Name = namedFilter.FilterGroupName;
                        var filterCacheKey = string.Concat(namedFilter.FilterGroupName, "_", controllerAction, "_", T.FullName);

                        if (_cacheProvider != null && _cacheProvider.Contains(filterCacheKey))
                        {
                            namedQueryFilters = _cacheProvider[filterCacheKey] as List<string>;
                        }
                        else
                        {
                            //filters = IterateProperties("criteria", queryStringFilter.FilterType).ToList();
                            namedQueryFilters = IterateProperties(namedFilter.FilterGroupName, namedFilter.FilterType).ToList();
                            _cacheProvider.Add(filterCacheKey, namedQueryFilters, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions()
                            {
                                AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
                            });
                        }

                        if (namedQueryFilters != null && namedQueryFilters.Any())
                        {
                            namedQuery.Filters = namedQueryFilters;
                        }
                        namedQueries.Add(namedQuery);
                    }
                }
            }
            return filters;
        }

        /// <summary>
        /// Determine if the type is parent
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>boolean</returns>
        [Obsolete]
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
        [Obsolete]
        private string GetDisplayName(PropertyInfo prop)
        {
            if (prop == null)
                return string.Empty;

            var dataMemberAttributes = (DataMemberAttribute[])prop.GetCustomAttributes(typeof(DataMemberAttribute), false);
            if (dataMemberAttributes != null && dataMemberAttributes.Any())
                return dataMemberAttributes.FirstOrDefault(x => !(string.IsNullOrEmpty(x.Name))).Name;
            var jsonPropertyAttributes = (JsonPropertyAttribute[])prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false);
            if (jsonPropertyAttributes != null && jsonPropertyAttributes.Any())
                return jsonPropertyAttributes.FirstOrDefault(x => !(string.IsNullOrEmpty(x.PropertyName))).PropertyName;

            return string.Empty;
        }

        [Obsolete]
        private int DefaultPageSize(Type T, string controllerAction)
        {
            try
            {
                var customAttribute = (PagingFilter)T.GetMethod(controllerAction).GetCustomAttribute(typeof(PagingFilter), false);

                if (customAttribute.DefaultLimit != 0)
                {
                    return customAttribute.DefaultLimit;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// Determine if a property has the FilterProperty attribute and should be displayed 
        /// </summary>
        /// <param name="prop">propertyinfo</param>
        /// <param name="filtername">string</param>
        /// <returns>boolean</returns>
        [Obsolete]
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
        [Obsolete]
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

        [Obsolete]
        /// <summary>
        /// Get the generic type for a list
        /// </summary>
        /// <param name="T">Type</param>
        /// <returns>Type</returns>
        private Type GetGenericType(Type T)
        {
            if (!T.IsGenericType)
                return T;

            return T.GetGenericArguments()[0];
        }

        /// <summary>
        /// Get filters used for versions 6 and 7
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="T"></param>
        /// <param name="controllerAction"></param>
        /// <returns></returns>
        [Obsolete]
        private List<string> LegacyFilters(List<string> keywords, Type T, string controllerAction)
        {
            List<string> filters = new List<string>();

            ValidateQueryStringFilter[] attrs = (ValidateQueryStringFilter[])T.GetMethod(controllerAction).GetCustomAttributes(typeof(ValidateQueryStringFilter), false);
            if (attrs != null)
            {
                foreach (var attr in attrs)
                {
                    if (attr.ValidQueryParameters != null)
                    {
                        var queryParameters = attr.ValidQueryParameters.Where(q => !(string.IsNullOrEmpty(q)) && !keywords.Contains(q));
                        foreach (var queryParameter in queryParameters)
                        {
                            if (!(string.IsNullOrEmpty(queryParameter)))
                                filters.Add(queryParameter);
                        }
                    }
                }
            }
            return filters;
        }

        /// <summary>
        /// Get filters used for versions 6 and 7
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="T"></param>
        /// <param name="controllerAction"></param>
        /// <returns></returns>

        [Obsolete]
        private List<NamedQuery> LegacyNamedQueries(List<string> keywords, Type T, string controllerAction)
        {
            var filters = new List<NamedQuery>();
            try
            {
                ValidateQueryStringFilter[] attrs = (ValidateQueryStringFilter[])T.GetMethod(controllerAction).GetCustomAttributes(typeof(ValidateQueryStringFilter), false);
                if (attrs != null)
                {
                    foreach (var attr in attrs)
                    {
                        if (attr.NamedQueries != null)
                        {
                            var queryParameters = attr.NamedQueries.Where(q => !(string.IsNullOrEmpty(q)) && !keywords.Contains(q));
                            foreach (var queryParameter in queryParameters)
                            {
                                if (!(string.IsNullOrEmpty(queryParameter)))
                                    filters.Add(new NamedQuery() { Name = queryParameter, Filters = new List<string>() { queryParameter } });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // do not throw error is named queries can not be retrieved on legacy versions. 
            }
            return filters;
        }

        /// <summary>
        /// Gets the api name
        /// </summary>
        /// <param name="routeTemplate"></param>
        /// <returns></returns>
        [Obsolete]
        private string GetApiName(string routeTemplate)
        {
            var idx = routeTemplate.IndexOf("/");
            string apiName = string.Empty;

            if (routeTemplate.StartsWith("qapi"))
            {
                apiName = routeTemplate.Substring(idx + 1);
                return apiName;
            }

            if (idx != -1)
            {
                apiName = routeTemplate.Substring(0, idx);
            }
            else
            {
                apiName = routeTemplate;
            }

            return apiName;
        }

        [Obsolete]
        private JsonSchema GetJsonSchemaForApi(MethodInfo mi)
        {
            JsonSchema jsonSchemaResult = null;

            var dtoTypeAsArrayInput = false;
            var dtoTypeNameInput = string.Empty;

            // get type name and if array...
            Type type = mi.ReturnType;

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
            if (type.IsGenericType && !type.IsValueType)
            {
                type = type.GetGenericArguments().SingleOrDefault();
            }
            if (type.Name == "IActionResult")
            {
                var customAttributes = mi.GetCustomAttributes().Where(ca => ca.GetType().Name == "QueryStringFilterFilter");
                if (customAttributes != null)
                {

                    foreach (var attr in customAttributes)
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
                dtoTypeNameInput = type.AssemblyQualifiedName;
                if (!string.IsNullOrEmpty(dtoTypeNameInput))
                {
                    try
                    {
                        jsonSchemaResult = GetJsonSchemaForDto(dtoTypeNameInput);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                }
            }

            return jsonSchemaResult;
        }

        [Obsolete]
        private JsonSchema GetJsonSchemaForDto(string dtoTypeNameInput)
        {

            Type dtoType = Type.GetType(dtoTypeNameInput);

            JsonSchemaGenerator g = new JsonSchemaGenerator();

            JsonSchema s = g.Generate(dtoType);

            s.AllowAdditionalProperties = false;
            Dictionary<string, JsonSchema> properties = new Dictionary<string, JsonSchema>(s.Properties);

            var origProperties = dtoType.GetProperties();

            if (properties != null)
            {
                int index = 0;
                foreach (var js in properties)
                {
                    PropertyInfo pi = null;
                    FieldInfo fi = null;
                    MemberInfo[] mi = null;
                    var orgProperty = origProperties.ElementAtOrDefault(index);
                    if (orgProperty != null && !string.IsNullOrEmpty(orgProperty.Name))
                    {
                        pi = orgProperty;
                        fi = dtoType.GetField(orgProperty.Name);
                        mi = dtoType.GetMember(orgProperty.Name);
                    }
                    else
                    {
                        pi = dtoType.GetProperty(js.Key);
                        fi = dtoType.GetField(js.Key);
                    }

                    string memberName = js.Key;
                    if (pi != null)
                    {
                        memberName = GetDisplayName(pi);
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
                                                memberName = arg.TypedValue.Value.ToString();
                                            }
                                        }
                                    }
                                    if (custAttr != null && custAttr.ConstructorArguments != null)
                                    {
                                        foreach (var arg in custAttr.ConstructorArguments)
                                        {
                                            if (arg.ArgumentType.Name == "String")
                                            {
                                                memberName = arg.Value.ToString();
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

                    if (!s.Properties.ContainsKey(memberName))
                    {
                        s.Properties.Add(memberName, new Newtonsoft.Json.Schema.JsonSchema());
                        s.Properties[memberName].Type = JsonSchemaType.String;
                    }
                    if (pi != null)
                    {
                        s.Properties[memberName].Description = pi.Name;
                    }
                    if (fieldType != null)
                    {

                        if (fieldType.IsGenericType
                            && fieldType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        {
                            Type underlyingType = Nullable.GetUnderlyingType(fieldType);
                            if (underlyingType == typeof(DateTime))
                                s.Properties[memberName].Format = "date-time";
                            else if (underlyingType.BaseType == typeof(Enum))
                            {
                                var enumNames = Enum.GetNames(underlyingType);

                                s.Properties[memberName].Enum = new JArray(enumNames.Select(x => x.ToLowerInvariant()).ToArray());
                                s.Properties[memberName].Type = JsonSchemaType.String;
                            }
                        }
                        else if (fieldType == typeof(DateTime))
                        {
                            s.Properties[memberName].Format = "date-time";
                        }
                        else if (fieldType.IsEnum)
                        {
                            var enumNames = Enum.GetNames(fieldType);
                            s.Properties[memberName].Enum = new JArray(enumNames.Select(x => x.ToLowerInvariant()).ToArray());
                            s.Properties[memberName].Type = JsonSchemaType.String;
                        }
                    }

                    index++;
                }
            }


            string schemaJson = s.ToString();
            schemaJson = schemaJson.Replace("\r\n", "");  //remove all newline chars
            schemaJson = schemaJson.Replace(" ", "");    // remove all blanks
            schemaJson = schemaJson.Replace(",\"null\"", ""); // remove all null entries
            s = JsonSchema.Parse(schemaJson);   // put it back together 
            return s;
        }

        [Obsolete]
        private JsonSchemaType ConvertJsonPropertyType(string jsonPropertyType, string jsonTitle = "", string conversion = "")

        {
            JsonSchemaType jsonSchemaType = JsonSchemaType.String;
            if ((!string.IsNullOrEmpty(jsonTitle)) && (jsonTitle.EndsWith("[]")))
            {
                return JsonSchemaType.Array;

            }

            if (string.IsNullOrEmpty(jsonPropertyType))
                return jsonSchemaType;

            switch (jsonPropertyType.ToLower())
            {
                case "string":
                    jsonSchemaType = JsonSchemaType.String;
                    break;
                case "number":
                    if (!string.IsNullOrEmpty(conversion))
                    {
                        if (conversion.Equals("MD0", StringComparison.OrdinalIgnoreCase))
                        {
                            jsonSchemaType = JsonSchemaType.Integer;
                        }
                        else
                        {
                            jsonSchemaType = JsonSchemaType.Float;
                        }
                    }
                    else
                    {
                        jsonSchemaType = Newtonsoft.Json.Schema.JsonSchemaType.Integer;
                    }


                    break;
                case "date":
                    jsonSchemaType = JsonSchemaType.String;
                    break;
                case "time":
                    jsonSchemaType = JsonSchemaType.String;
                    break;
                case "datetime":
                    jsonSchemaType = JsonSchemaType.String;
                    break;
                default:
                    jsonSchemaType = JsonSchemaType.String;
                    break;
            }

            return jsonSchemaType;
        }

        [Obsolete]
        private string GetJsonPropertyPattern(string jsonPropertyType)

        {

            if (string.IsNullOrEmpty(jsonPropertyType))
                return string.Empty;

            string retVal = string.Empty;

            switch (jsonPropertyType.ToLower())
            {


                case "string":
                    retVal = string.Empty;
                    break;
                case "number":
                    retVal = string.Empty;
                    break;
                case "date":
                    retVal = "^(-?(?:[1-9][0-9]*)?[0-9]{4})-(1[0-2]|0[1-9])-(3[0-1]|0[1-9]|[1-2][0-9])$";
                    break;
                case "time":
                    retVal = string.Empty;
                    break;
                case "datetime":
                    retVal = "^(-?(?:[1-9][0-9]*)?[0-9]{4})-(1[0-2]|0[1-9])-(3[0-1]|0[1-9]|[1-2][0-9])T(2[0-3]|[0-1][0-9]):([0-5][0-9]):([0-5][0-9])(\\.[0-9]+)?(Z|[+-](?:2[0-3]|[0-1][0-9]):[0-5][0-9])?$";
                    break;
                default:
                    retVal = string.Empty;
                    break;
            }

            return retVal;
        }

        /// <summary>
        /// GetJsonSchemaFromExtensibleData
        /// </summary>
        /// <param name="extendConfig">Domain.Base.Entities.EthosExtensibleData</param>
        /// <param name="existingSchema"></param>
        /// <param name="id"></param>
        /// <param name="ethosApiConfiguration">EthosApiConfiguration</param>
        /// <returns>JsonSchema</returns>
        [Obsolete]
        private async Task<string> GetJsonSchemaFromExtensibleDataAsync(EthosApiConfiguration ethosApiConfiguration,
            Domain.Base.Entities.EthosExtensibleData extendConfig, string id = "", JsonSchema existingSchema = null)
        {
            if (ethosApiConfiguration == null || extendConfig == null || extendConfig.ExtendedDataList == null || !extendConfig.ExtendedDataList.Any())
            {
                return null;
            }

            JsonSchema schemaRootNode = null;
            List<string> required = new List<string>();
            if (existingSchema != null)
                schemaRootNode = existingSchema;
            else
            {

                schemaRootNode = new JsonSchema()
                {
                    Id = id,
                    Description = string.IsNullOrEmpty(extendConfig.Description) ? "" :
                        extendConfig.Description.Replace(DmiString._VM, ' ').Replace(DmiString._SM, ' '),
                    Type = JsonSchemaType.Object,
                    Properties = new Dictionary<string, JsonSchema>(),
                    AllowAdditionalProperties = false,
                    AllowAdditionalItems = false,
                    Title = extendConfig.ApiResourceName + "_" + extendConfig.ApiVersionNumber

                };

                required.Add("id");



                if (!string.IsNullOrEmpty(ethosApiConfiguration.PrimaryGuidSource))
                    schemaRootNode.Properties.Add("id",
                                    new JsonSchema
                                    {
                                        Type = ConvertJsonPropertyType("string"),
                                        Title = "ID",
                                        Format = "guid",
                                        Description = "The global identifier for the resource",
                                        Pattern = GUID_PATTERN,
                                        Required = true
                                    });
                else
                {
                    schemaRootNode.Properties.Add("id",
                                       new JsonSchema
                                       {
                                           Type = ConvertJsonPropertyType("string"),
                                           Title = "ID",
                                           Description = "The derived identifier for the resource",
                                           Required = true
                                       });
                }

            }

            var extendedDataListSorted = extendConfig.ExtendedDataList.OrderBy(ex => ex.FullJsonPath);
            foreach (var extendedData in extendedDataListSorted)
            {
                try
                {
                    var propSplit =
                    extendedData.FullJsonPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                    if (!propSplit.Any()) continue;

                    var count = propSplit.Count();

                    if (count == 1)
                    {
                        var response = BuildJsonSchemaResponse(extendedData);

                        schemaRootNode.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);
                        if (response.Required == true)
                            required.Add(extendedData.JsonTitle);
                    }
                    else
                    {
                        var parentSchema = schemaRootNode;

                        for (int i = 0; i < count; i++)
                        {
                            JsonSchema childSchema = null;

                            if (childSchema == null && i < count - 1)
                            {
                                if (!parentSchema.Properties.TryGetValue(propSplit[i].Replace("[]", ""), out childSchema))
                                {

                                    if (propSplit[i].Contains("[]"))
                                    {
                                        childSchema = new JsonSchema { Type = JsonSchemaType.Array };
                                    }
                                    else
                                    {
                                        childSchema = new JsonSchema { Type = JsonSchemaType.Object };
                                    }
                                    childSchema.Properties = new Dictionary<string, JsonSchema>();
                                    parentSchema.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
                                }
                                parentSchema = childSchema;
                            }
                            else if (parentSchema != null && i == count - 1)
                            {
                                parentSchema.Properties.Add(propSplit[i], BuildJsonSchemaResponse(extendedData));
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

            if (!required.Any())
                return schemaRootNode.ToString();

            try
            {
                SchemaRequiredArray test = new SchemaRequiredArray();
                test.Required = required;
                var strRequired = JsonConvert.SerializeObject(required);

                var requiredBlock = ", \"required\": " + strRequired + " } ";

                var schemaApplicationJson = schemaRootNode.ToString();
                int index = schemaApplicationJson.LastIndexOf("}");
                if (index >= 0)
                    return new StringBuilder(schemaApplicationJson).Replace("}", requiredBlock.ToString(), index, 1).ToString();
                else
                    return schemaRootNode.ToString();
            }
            catch (Exception)
            {
                return schemaRootNode.ToString();
            }
        }

        [Obsolete]
        private JsonSchema BuildJsonSchemaResponse(Domain.Base.Entities.EthosExtensibleDataRow extendedData)
        {
            var jsonSchema = new JsonSchema
            {
                Type = ConvertJsonPropertyType(extendedData.JsonPropertyType, extendedData.JsonTitle, extendedData.Conversion),
                MaximumLength = extendedData.ColleaguePropertyLength
            };


            if ((extendedData.JsonPropertyType.Equals("date", StringComparison.OrdinalIgnoreCase))
            || (extendedData.JsonPropertyType.Equals("date-time", StringComparison.OrdinalIgnoreCase)))
            {
                jsonSchema.MaximumLength = null;
            }
            else if (!string.IsNullOrEmpty(extendedData.TransType))
            {
                if (extendedData.TransType.Equals("G", StringComparison.OrdinalIgnoreCase))
                {
                    jsonSchema.Pattern = GUID_PATTERN;
                    jsonSchema.MaximumLength = Guid.Empty.ToString().Length;
                    jsonSchema.Format = "guid";

                }
                else
                {
                    jsonSchema.MaximumLength = null;
                }
            }
            else if (!string.IsNullOrEmpty(extendedData.Conversion))
            {
                jsonSchema.Format = extendedData.Conversion;
            }

            if (string.IsNullOrEmpty(jsonSchema.Pattern))
            {
                var pattern = GetJsonPropertyPattern(extendedData.JsonPropertyType);
                if (!string.IsNullOrEmpty(pattern))
                {
                    jsonSchema.Pattern = pattern;
                }
            }

            if (!string.IsNullOrEmpty(extendedData.JsonTitle))
            {
                try
                {
                    var jsonTitle = extendedData.JsonTitle.Replace("[]", "");
                    jsonSchema.Title = Char.ToUpperInvariant(jsonTitle[0]) + jsonTitle.Substring(1);
                }
                catch (Exception)
                {
                    jsonSchema.Title = extendedData.JsonTitle;
                }

            }


            if (!string.IsNullOrEmpty(extendedData.Description))
            {
                jsonSchema.Description = extendedData.Description.Replace(DmiString._VM, ' ').Replace(DmiString._SM, ' ');
            }
            if (extendedData.Required)
            {
                jsonSchema.Required = extendedData.Required;
            }
            return jsonSchema;
        }
    }

    [Obsolete]
    class SchemaRequiredArray
    {
        [DataMember(Name = "required", EmitDefaultValue = false)]
        public List<string> Required { get; set; }
    }
    [Obsolete]
    class SchemaVersionNumberComparer : IComparer<string>
    {
        /// <summary>
        /// Compare strings which represent semantic version numbers and/or integers
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>x is greater return 1 else if y is greater return -1</returns>
        public int Compare(string x, string y)
        {


            if (x == y) return 0;

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
    }


}

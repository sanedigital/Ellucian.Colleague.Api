// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using System.Linq;
using Ellucian.Colleague.Domain.Exceptions;
using System.Net;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to AcademicCatalog data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicCatalogController : BaseCompressedApiController
    {
         private readonly IAcademicCatalogService _academicCatalogService; 
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AcademicCatalogController class.
        /// </summary>
        /// <param name="academicCatalogService">Service of type <see cref="IAcademicCatalogService">IAcademicCatalogService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AcademicCatalogController(IAcademicCatalogService academicCatalogService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {

            _academicCatalogService = academicCatalogService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Academic Catalogs.
        /// </summary>
        /// <returns>All <see cref="Dtos.AcademicCatalog">AcademicCatalog</see>objects.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/academic-catalogs", "6.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetDefaultAcademicCatalogs", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.AcademicCatalog2>>> GetAcademicCatalogs2Async()
        {
            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                var items = await _academicCatalogService.GetAcademicCatalogs2Async(bypassCache);

                AddEthosContextProperties(await _academicCatalogService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _academicCatalogService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              items.Select(a => a.Id).ToList()));

                return Ok(items);
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
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an academic catalog by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicCatalog">AcademicCatalog</see>object.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/academic-catalogs/{id}", "6.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetDefaultAcademicCatalogById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AcademicCatalog2>> GetAcademicCatalogById2Async(string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

            try
            {
                var item = await _academicCatalogService.GetAcademicCatalogByGuid2Async(id);

                if(item != null)
                {
                    AddEthosContextProperties(await _academicCatalogService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _academicCatalogService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { item.Id }));
                }

                return item;
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

        /// <remarks>FOR USE WITH ELLUCIAN SS</remarks>
        /// <summary>
        /// Retrieves all Academic Catalogs.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>All <see cref="Catalog">Catalog</see>objects.</returns>
        /// <note>Academic catalog data is cached for 24 hours.</note>
        [HttpGet]

        [HeaderVersionRoute("/academic-catalogs", 1, false, Name = "GetAllCatalogs")]
        public async Task<ActionResult<IEnumerable<Catalog>>> GetAllAcademicCatalogsAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                IEnumerable<Catalog> cats = await _academicCatalogService.GetAllAcademicCatalogsAsync(bypassCache);
                return Ok(cats);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving active programs";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                var message = "Unable to retrieve academic catalog data.  See Logging for more details.  Exception thrown: " + ex.Message;
                return CreateHttpResponseException(message);
            }
        }

        /// <summary>        
        /// Creates a AcademicCatalog.
        /// </summary>
        /// <param name="academicCatalog"><see cref="Dtos.AcademicCatalog">AcademicCatalog</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicCatalog">AcademicCatalog</see></returns>
        [HttpPost]


        [HeaderVersionRoute("/academic-catalogs", "6.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAcademicCatalogV6_1_0")]
        public ActionResult<Dtos.AcademicCatalog> PostAcademicCatalogs([FromBody] Dtos.AcademicCatalog academicCatalog)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a AcademicCatalog.
        /// </summary>
        /// <param name="id">Id of the AcademicCatalog to update</param>
        /// <param name="academicCatalog"><see cref="Dtos.AcademicCatalog">AcademicCatalog</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicCatalog">AcademicCatalog</see></returns>
        [HttpPut]


        [HeaderVersionRoute("/academic-catalogs/{id}", "6.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAcademicCatalogV6_1_0")]
        public ActionResult<Dtos.AcademicCatalog> PutAcademicCatalogs([FromRoute] string id, [FromBody] Dtos.AcademicCatalog academicCatalog)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing AcademicCatalog
        /// </summary>
        /// <param name="id">Id of the AcademicCatalog to delete</param>
        [HttpDelete]
        [Route("/academic-catalogs/{id}", Name = "DeleteAcademicCatalog", Order = -10)]
        public IActionResult DeleteAcademicCatalogs([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

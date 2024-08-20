// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

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
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Geographic Area Types data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class GeographicAreasController : BaseCompressedApiController
    {
        private readonly IGeographicAreaService _geographicAreaService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GeographicAreasController class.
        /// </summary>
        /// <param name="geographicAreaService">Service of type <see cref="IGeographicAreaService">IGeographicAreaService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GeographicAreasController(IGeographicAreaService geographicAreaService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _geographicAreaService = geographicAreaService;
            this._logger = logger;
        }
        
        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves all geographic areas.
        /// </summary>
        /// <returns>All GeographicArea objects.</returns>
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/geographic-areas", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetGeographicAreas", IsEedmSupported = true)]
        public async Task<IActionResult> GetGeographicAreasAsync(Paging page)
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

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _geographicAreaService.GetGeographicAreasAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _geographicAreaService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _geographicAreaService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.GeographicArea>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves a geographic area by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.GeographicAreas">GeographicArea.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/geographic-areas/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetGeographicAreaById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.GeographicArea>> GetGeographicAreaByIdAsync(string id)
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

                var area = await _geographicAreaService.GetGeographicAreaByGuidAsync(id);

                if (area != null)
                {

                    AddEthosContextProperties(await _geographicAreaService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _geographicAreaService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { area.Id }));
                }


                return area;
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Updates a GeographicArea.
        /// </summary>
        /// <param name="geographicArea"><see cref="GeographicArea">GeographicArea</see> to update</param>
        /// <returns>Newly updated <see cref="GeographicArea">GeographicArea</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/geographic-areas/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGeographicAreaV6")]
        public async Task<ActionResult<Dtos.GeographicArea>> PutGeographicAreaAsync([FromBody] Dtos.GeographicArea geographicArea)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a GeographicArea.
        /// </summary>
        /// <param name="geographicArea"><see cref="GeographicArea">GeographicArea</see> to create</param>
        /// <returns>Newly created <see cref="GeographicArea">GeographicArea</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/geographic-areas", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGeographicAreaV6")]
        public async Task<ActionResult<Dtos.GeographicArea>> PostGeographicAreaAsync([FromBody] Dtos.GeographicArea geographicArea)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing GeographicArea
        /// </summary>
        /// <param name="id">Id of the GeographicArea to delete</param>
        [HttpDelete]
        [Route("/geographic-areas/{id}", Name = "DeleteGeographicArea", Order = -10)]
        public async Task<IActionResult> DeleteGeographicAreaAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

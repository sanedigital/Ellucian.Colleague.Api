// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Filters;
using System.Linq;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Source data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SourcesController : BaseCompressedApiController
    {
        private readonly ISourceService _sourceService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SourcesController class.
        /// </summary>
        /// <param name="sourceService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SourcesController(ISourceService sourceService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _sourceService = sourceService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Retrieves all sources.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All Source objects.</returns>
        /// 
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/sources", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSources", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Source>>> GetSourcesAsync()
        {
            bool bypassCache = false; 
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var getSources = await _sourceService.GetSourcesAsync(bypassCache);

                if (getSources != null && getSources.Any())
                {
                    AddEthosContextProperties(await _sourceService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _sourceService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              getSources.Select(a => a.Id).ToList()));
                }

                return Ok(getSources);                
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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Retrieves a source by ID.
        /// </summary>
        /// <param name="id">Id of Source to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.Source">Source.</see></returns>
        /// 
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/sources/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSourceById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Source>> GetSourceByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _sourceService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _sourceService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _sourceService.GetSourceByIdAsync(id);
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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Creates a Source.
        /// </summary>
        /// <param name="source"><see cref="Dtos.Source">Source</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.Source">Source</see></returns>
        [HttpPost]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/sources", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSourcesV6")]
        public async Task<ActionResult<Dtos.Source>> PostSourcesAsync([FromBody] Dtos.Source source)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Updates a Source.
        /// </summary>
        /// <param name="id">Id of the Source to update</param>
        /// <param name="source"><see cref="Dtos.Source">Source</see> to create</param>
        /// <returns>Updated <see cref="Dtos.Source">Source</see></returns>
        [HttpPut]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/sources/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSourcesV6")]
        public async Task<ActionResult<Dtos.Source>> PutSourcesAsync([FromRoute] string id, [FromBody] Dtos.Source source)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Delete (DELETE) an existing Source
        /// </summary>
        /// <param name="id">Id of the Source to delete</param>
        [HttpDelete]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [Route("/sources/{id}", Name = "DefaultDeleteSources", Order = -10)]
        public async Task<IActionResult> DeleteSourcesAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;

using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Controller for Source Context
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class SourceContextController : BaseCompressedApiController
    {
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the  Source Context Controller class.
        /// </summary>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SourceContextController(IDemographicService demographicService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _demographicService = demographicService;
            this._logger = logger;
        }
        #region Get Methods

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Retrieves all source contexts.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All SourceContext objects</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/source-contexts", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSourceContexts", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<SourceContext>>> GetSourceContextsAsync()
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
                return Ok(await _demographicService.GetSourceContextsAsync(bypassCache));
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
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Retrieves a source context by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.SourceContext">SourceContext.</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/source-contexts/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSourceContextsById", IsEedmSupported = true)]
        public async Task<ActionResult<SourceContext>> GetSourceContextsByIdAsync(string id)
        {
            try
            {
                return await _demographicService.GetSourceContextsByIdAsync(id);
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

        #endregion

        #region Post Methods

        /// <summary>
        /// Creates a  Source Context.
        /// </summary>
        /// <param name="sourceContext"><see cref="SourceContext">SourceContext</see> to create</param>
        /// <returns>Newly created <see cref="SourceContext">SourceContext</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]        
        [HttpPost]
        [HeaderVersionRoute("/source-contexts", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSourceContextsV6")]
        public async Task<ActionResult<SourceContext>> PostSourceContextsAsync([FromBody] SourceContext sourceContext)
        {
            //Create is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods

        /// <summary>
        /// Updates a  Source Context.
        /// </summary>
        /// <param name="id">Id of the  Source Context to update</param>
        /// <param name="sourceContext"><see cref="SourceContext">SourceContext</see> to create</param>
        /// <returns>Updated <see cref="SourceContext">SourceContext</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/source-contexts/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSourceContextsV6")]
        public async Task<ActionResult<SourceContext>> PutSourceContextsAsync([FromRoute] string id, [FromBody] SourceContext sourceContext)
        {
            //Update is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Delete Methods

        /// <summary>
        /// Delete (DELETE) an existing  Source Context
        /// </summary>
        /// <param name="id">Id of the  Source Context to delete</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/source-contexts/{id}", Name = "DefaultDeleteSourceContexts", Order = -10)]
        public async Task<ActionResult<SourceContext>> DeleteSourceContextsAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion
    }
}

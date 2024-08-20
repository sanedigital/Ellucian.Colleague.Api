// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to RelationshipStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RelationshipStatusesController : BaseCompressedApiController
    {
        private readonly IRelationshipStatusesService _relationshipStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RelationshipStatusesController class.
        /// </summary>
        /// <param name="relationshipStatusesService">Service of type <see cref="IRelationshipStatusesService">IRelationshipStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RelationshipStatusesController(IRelationshipStatusesService relationshipStatusesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _relationshipStatusesService = relationshipStatusesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all relationshipStatuses
        /// </summary>
        /// <returns>List of RelationshipStatuses <see cref="Dtos.RelationshipStatuses"/> objects representing matching relationshipStatuses</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/relationship-statuses", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRelationshipStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.RelationshipStatuses>>> GetRelationshipStatusesAsync()
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                AddDataPrivacyContextProperty((await _relationshipStatusesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return Ok(await _relationshipStatusesService.GetRelationshipStatusesAsync(bypassCache));
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
        /// Read (GET) a relationshipStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired relationshipStatuses</param>
        /// <returns>A relationshipStatuses object <see cref="Dtos.RelationshipStatuses"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/relationship-statuses/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRelationshipStatusesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.RelationshipStatuses>> GetRelationshipStatusesByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _relationshipStatusesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _relationshipStatusesService.GetRelationshipStatusesByGuidAsync(guid);
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
        /// Create (POST) a new relationshipStatuses
        /// </summary>
        /// <param name="relationshipStatuses">DTO of the new relationshipStatuses</param>
        /// <returns>A relationshipStatuses object <see cref="Dtos.RelationshipStatuses"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/relationship-statuses", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRelationshipStatusesV100")]
        public async Task<ActionResult<Dtos.RelationshipStatuses>> PostRelationshipStatusesAsync([FromBody] Dtos.RelationshipStatuses relationshipStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing relationshipStatuses
        /// </summary>
        /// <param name="guid">GUID of the relationshipStatuses to update</param>
        /// <param name="relationshipStatuses">DTO of the updated relationshipStatuses</param>
        /// <returns>A relationshipStatuses object <see cref="Dtos.RelationshipStatuses"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/relationship-statuses/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRelationshipStatusesV100")]
        public async Task<ActionResult<Dtos.RelationshipStatuses>> PutRelationshipStatusesAsync([FromRoute] string guid, [FromBody] Dtos.RelationshipStatuses relationshipStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a relationshipStatuses
        /// </summary>
        /// <param name="guid">GUID to desired relationshipStatuses</param>
        [HttpDelete]
        [Route("/relationship-statuses/{guid}", Name = "DefaultDeleteRelationshipStatuses", Order = -10)]
        public async Task<IActionResult> DeleteRelationshipStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

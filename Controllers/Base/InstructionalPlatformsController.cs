// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Net;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Instructional Platform data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class InstructionalPlatformsController : BaseCompressedApiController
    {
        private readonly IInstructionalPlatformService _instructionalPlatformService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructionalPlatformsController class.
        /// </summary>
        /// <param name="instructionalPlatformService">Service of type <see cref="IInstructionalPlatformService">IInstructionalPlatformService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public InstructionalPlatformsController(IInstructionalPlatformService instructionalPlatformService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _instructionalPlatformService = instructionalPlatformService;
            this._logger = logger;
        }

        #region Get Methods

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Instructional Platforms.
        /// </summary>
        /// <returns>All <see cref="InstructionalPlatform"> </see>InstructionalPlatform.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/instructional-platforms", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstructionalPlatforms", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<InstructionalPlatform>>> GetInstructionalPlatformsAsync()
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
                return Ok(await _instructionalPlatformService.GetInstructionalPlatformsAsync(bypassCache));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, "Permissions exception");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Argument exception");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, "Repository exception");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e, "Integration API exception");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurred");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Instructional Platform by GUID.
        /// </summary>
        /// <param name="id">Id of the Instructional Platform to retrieve</param>
        /// <returns>An <see cref="InstructionalPlatform">InstructionalPlatform </see>object.</returns>
        [HttpGet]
        [HeaderVersionRoute("/instructional-platforms/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstructionalPlatformsById", IsEedmSupported = true)]
        public async Task<ActionResult<InstructionalPlatform>> GetInstructionalPlatformsByIdAsync(string id)
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
                return await _instructionalPlatformService.GetInstructionalPlatformByGuidAsync(id, bypassCache);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, "Permissions exception");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Argument exception");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e, "Repository exception");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e, "Integration API exception");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurred");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        #endregion

        #region Post Methods

        /// <summary>
        /// Creates a instructionalPlatform.
        /// </summary>
        /// <param name="instructionalPlatform"><see cref="InstructionalPlatform">InstructionalPlatform</see> to create</param>
        /// <returns>Newly created <see cref="InstructionalPlatform">InstructionalPlatform</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/instructional-platforms", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstructionalPlatformsV6")]
        public async Task<ActionResult<InstructionalPlatform>> PostInstructionalPlatformsAsync([FromBody] InstructionalPlatform instructionalPlatform)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods

        /// <summary>
        /// Creates a instructionalPlatform.
        /// </summary>
        /// <param name="id">Id of InstructionalPlatform to create</param>
        /// <param name="instructionalPlatform"><see cref="InstructionalPlatform">InstructionalPlatform</see> to create</param>
        /// <returns>Newly created <see cref="InstructionalPlatform">InstructionalPlatform</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/instructional-platforms/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstructionalPlatformsV6")]
        public async Task<ActionResult<InstructionalPlatform>> PutInstructionalPlatformsAsync([FromRoute] string id, [FromBody] InstructionalPlatform instructionalPlatform)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Delete Methods

        /// <summary>
        /// Delete (DELETE) an existing Intructional Platform
        /// </summary>
        /// <param name="id">Id of the Instructional Platform to delete</param>
        [HttpDelete]
        [Route("/instructional-platforms/{id}", Name = "DefaultDeleteInstructionalPlatforms", Order = -10)]
        public async Task<IActionResult> DeleteInstructionalPlatformsAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

    }
}

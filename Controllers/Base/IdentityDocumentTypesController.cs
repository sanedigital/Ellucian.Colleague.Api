// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Identity Document Types data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class IdentityDocumentTypesController : BaseCompressedApiController
    {
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the IdentityDocumentTypesController class.
        /// </summary>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public IdentityDocumentTypesController(IDemographicService demographicService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _demographicService = demographicService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 5</remarks>
        /// <summary>
        /// Retrieves all identity document types.
        /// </summary>
        /// <returns>All IdentityDocumentTypes objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/identity-document-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetIdentityDocumentTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.IdentityDocumentType>>> GetIdentityDocumentTypesAsync()
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
                return Ok(await _demographicService.GetIdentityDocumentTypesAsync(bypassCache));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 5</remarks>
        /// <summary>
        /// Retrieves a identity document type by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.IdentityDocumentType">IdentityDocumentType.</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/identity-document-types/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetIdentityDocumentTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.IdentityDocumentType>> GetIdentityDocumentTypeByIdAsync(string id)
        {
            try
            {
                return await _demographicService.GetIdentityDocumentTypeByGuidAsync(id);
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(e);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
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
        /// Updates a IdentityDocumentType.
        /// </summary>
        /// <param name="identityDocumentType"><see cref="Dtos.IdentityDocumentType">IdentityDocumentType</see> to update</param>
        /// <returns>Newly updated <see cref="Dtos.IdentityDocumentType">IdentityDocumentType</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/identity-document-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutIdentityDocumentTypesV6")]
        public async Task<ActionResult<Dtos.IdentityDocumentType>> PutIdentityDocumentTypeAsync([FromBody] Dtos.IdentityDocumentType identityDocumentType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            
        }

        /// <summary>
        /// Creates a IdentityDocumentType.
        /// </summary>
        /// <param name="identityDocumentType"><see cref="Dtos.IdentityDocumentType">IdentityDocumentType</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.IdentityDocumentType">IdentityDocumentType</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/identity-document-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostIdentityDocumentTypesV6")]
        public async Task<ActionResult<Dtos.IdentityDocumentType>> PostIdentityDocumentTypeAsync([FromBody] Dtos.IdentityDocumentType identityDocumentType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            
        }

        /// <summary>
        /// Delete (DELETE) an existing IdentityDocumentType
        /// </summary>
        /// <param name="id">Id of the IdentityDocumentTypes to delete</param>
        [HttpDelete]
        [Route("/identity-document-types/{id}", Name = "DeleteIdentityDocumentTypes", Order = -10)]
        public async Task<IActionResult> DeleteIdentityDocumentTypeAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            
        }
    }
}

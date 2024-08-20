// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to RelationshipTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RelationshipTypesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IRelationshipTypesService _relationshipTypesService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";
        /// <summary>
        /// Initializes a new instance of the RelationshipTypesController class.
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="relationshipTypesService">Service of type <see cref="IRelationshipTypesService">IRelationshipTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RelationshipTypesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IRelationshipTypesService relationshipTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _relationshipTypesService = relationshipTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Relationship Types.
        /// </summary>
        /// <returns>All <see cref="RelationshipType">relationship types.</see></returns>
        /// <note>Relationship Types are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/relationship-types", 1, false, Name = "GetRelationshipTypesV1")]
        public async Task<ActionResult<IEnumerable<RelationshipType>>> GetAsync()
        {
            try
            {
                var relationshipTypeCollection = await _referenceDataRepository.GetRelationshipTypesAsync();

                // Get the right adapter for the type mapping
                var relationshipTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RelationshipType, RelationshipType>();

                // Map the RelationshipType entity to the program DTO
                var relationshipTypeDtoCollection = new List<RelationshipType>();
                foreach (var relationshipType in relationshipTypeCollection)
                {
                    relationshipTypeDtoCollection.Add(relationshipTypeDtoAdapter.MapToType(relationshipType));
                }

                return relationshipTypeDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Return all relationshipTypes
        /// </summary>
        /// <returns>List of RelationshipTypes <see cref="Dtos.RelationshipTypes"/> objects representing matching relationshipTypes</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]        
        [HttpGet]
        [HeaderVersionRoute("/relationship-types", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRelationshipTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.RelationshipTypes>>> GetRelationshipTypesAsync()
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
                var relationshipTypes = await _relationshipTypesService.GetRelationshipTypesAsync(bypassCache);

                if (relationshipTypes != null && relationshipTypes.Any())
                {

                    AddEthosContextProperties(
                      await _relationshipTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _relationshipTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                          relationshipTypes.Select(i => i.Id).ToList()));
                }
                return Ok(relationshipTypes);

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
        /// Read (GET) a relationshipTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired relationshipTypes</param>
        /// <returns>A relationshipTypes object <see cref="Dtos.RelationshipTypes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/relationship-types/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRelationshipTypesByGuid")]
        public async Task<ActionResult<Dtos.RelationshipTypes>> GetRelationshipTypesByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _relationshipTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _relationshipTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _relationshipTypesService.GetRelationshipTypesByGuidAsync(guid);
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
        /// Create (POST) a new relationshipTypes
        /// </summary>
        /// <param name="relationshipTypes">DTO of the new relationshipTypes</param>
        /// <returns>A relationshipTypes object <see cref="Dtos.RelationshipTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/relationship-types", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRelationshipTypesV1.0.0")]
        public async Task<ActionResult<Dtos.RelationshipTypes>> PostRelationshipTypesAsync([FromBody] Dtos.RelationshipTypes relationshipTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing relationshipTypes
        /// </summary>
        /// <param name="guid">GUID of the relationshipTypes to update</param>
        /// <param name="relationshipTypes">DTO of the updated relationshipTypes</param>
        /// <returns>A relationshipTypes object <see cref="Dtos.RelationshipTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/relationship-types/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRelationshipTypesV1.0.0")]
        public async Task<ActionResult<Dtos.RelationshipTypes>> PutRelationshipTypesAsync([FromRoute] string guid, [FromBody] Dtos.RelationshipTypes relationshipTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a relationshipTypes
        /// </summary>
        /// <param name="guid">GUID to desired relationshipTypes</param>
        [HttpDelete]
        [Route("/relationship-types/{guid}", Name = "DefaultDeleteRelationshipTypes", Order = -10)]
        public async Task<IActionResult> DeleteRelationshipTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

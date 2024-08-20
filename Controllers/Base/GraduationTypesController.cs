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
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to GraduationTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    [Metadata(ApiDescription = "Provides access to graduation types.", ApiDomain = "CORE")]
    public class GraduationTypesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IGraduationTypesService _graduationTypesService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";
        /// <summary>
        /// Initializes a new instance of the GraduationTypesController class.
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="graduationTypesService">Service of type <see cref="IGraduationTypesService">IGraduationTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GraduationTypesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IGraduationTypesService graduationTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _graduationTypesService = graduationTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all graduationTypes
        /// </summary>
        /// <returns>List of GraduationTypes <see cref="Dtos.Base.GraduationTypes"/> objects representing matching graduationTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Returns all codes from GRADUATION.TYPES in CORE.VALCODES.",
        HttpMethodDescription = "Returns all codes from GRADUATION.TYPES in CORE.VALCODES.")]
        [HeaderVersionRoute("/graduation-types", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetGraduationTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.GraduationTypes>>> GetGraduationTypesAsync()
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
                var graduationTypes = await _graduationTypesService.GetGraduationTypesAsync(bypassCache);

                if (graduationTypes != null && graduationTypes.Any())
                {

                    AddEthosContextProperties(
                      await _graduationTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _graduationTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                          graduationTypes.Select(i => i.Id).ToList()));
                }
                return Ok(graduationTypes);

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
        /// Read (GET) a graduationTypes using a ID
        /// </summary>
        /// <param name="id">ID to desired graduationTypes</param>
        /// <returns>A graduationTypes object <see cref="Dtos.Base.GraduationTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Returns the code requested by id from GRADUATION.TYPES in CORE.VALCODES.",
        HttpMethodDescription = "Returns the code requested by id from GRADUATION.TYPES in CORE.VALCODES.")]
        [HeaderVersionRoute("/graduation-types/{id}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetGraduationTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Base.GraduationTypes>> GetGraduationTypesByGuidAsync(string id)
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
                AddEthosContextProperties(
                   await _graduationTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _graduationTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _graduationTypesService.GetGraduationTypesByGuidAsync(id);
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
        /// Create (POST) a new graduationTypes
        /// </summary>
        /// <param name="graduationTypes">DTO of the new graduationTypes</param>
        /// <returns>A graduationTypes object <see cref="Dtos.Base.GraduationTypes"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/graduation-types", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGraduationTypesAsync")]
        public async Task<ActionResult<Dtos.Base.GraduationTypes>> PostGraduationTypesAsync([FromBody] Dtos.Base.GraduationTypes graduationTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing graduationTypes
        /// </summary>
        /// <param name="graduationTypes">DTO of the updated graduationTypes</param>
        /// <returns>A graduationTypes object <see cref="Dtos.Base.GraduationTypes"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/graduation-types/{id}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGraduationTypesAsync")]
        public async Task<ActionResult<Dtos.Base.GraduationTypes>> PutGraduationTypesAsync([FromBody] Dtos.Base.GraduationTypes graduationTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a graduationTypes
        /// </summary>
        /// <param name="id">ID to desired graduationTypes</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [Route("/graduation-types/{id}", Name = "DeleteGraduationTypesAsync", Order = -10)]
        public async Task<IActionResult> DeleteGraduationTypesAsync(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

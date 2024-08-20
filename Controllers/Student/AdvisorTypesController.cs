// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Advisor Type data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdvisorTypesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IAdvisorTypesService _advisorTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdvisorTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="advisorTypesService">Service of type <see cref="IAdvisorTypesService">IAdvisorTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdvisorTypesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, IAdvisorTypesService advisorTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _advisorTypesService = advisorTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Gets all of the Advisor Types
        /// </summary>
        /// <returns>All <see cref="AdvisorType">AdvisorTypes</see></returns>
        /// <note>AdvisorType is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/advisor-types", 1, false, Name = "GetAdvisorTypes")]
        public async Task<ActionResult<IEnumerable<AdvisorType>>> GetAsync()
        {
            try
            {
                var advisorTypeCollection = await _referenceDataRepository.GetAdvisorTypesAsync();

                // Get the right adapter for the type mapping
                var advisorTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AdvisorType, AdvisorType>();

                // Map the AdvisorType entity to the program DTO
                var advisorTypeDtoCollection = new List<AdvisorType>();
                foreach (var advisorType in advisorTypeCollection)
                {
                    advisorTypeDtoCollection.Add(advisorTypeDtoAdapter.MapToType(advisorType));
                }

                return advisorTypeDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving advisor types";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Exception occurred while retrieving advisor types";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Return all advisorTypes
        /// </summary>
        /// <returns>List of AdvisorTypes <see cref="Dtos.AdvisorTypes"/> objects representing matching advisorTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/advisor-types", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdvisorTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AdvisorTypes>>> GetAdvisorTypesAsync()
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
                var advisorTypes = await _advisorTypesService.GetAdvisorTypesAsync(bypassCache);

                if (advisorTypes != null && advisorTypes.Any())
                {
                    AddEthosContextProperties(await _advisorTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _advisorTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              advisorTypes.Select(a => a.Id).ToList()));
                }

                return Ok(advisorTypes);                
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
        /// Read (GET) a advisorTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired advisorTypes</param>
        /// <returns>A advisorTypes object <see cref="Dtos.AdvisorTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/advisor-types/{guid}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdvisorTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdvisorTypes>> GetAdvisorTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _advisorTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _advisorTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _advisorTypesService.GetAdvisorTypesByGuidAsync(guid);
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
        /// Create (POST) a new advisorTypes
        /// </summary>
        /// <param name="advisorTypes">DTO of the new advisorTypes</param>
        /// <returns>A advisorTypes object <see cref="Dtos.AdvisorTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/advisor-types", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdvisorTypesV8")]
        public async Task<ActionResult<Dtos.AdvisorTypes>> PostAdvisorTypesAsync([FromBody] Dtos.AdvisorTypes advisorTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing advisorTypes
        /// </summary>
        /// <param name="guid">GUID of the advisorTypes to update</param>
        /// <param name="advisorTypes">DTO of the updated advisorTypes</param>
        /// <returns>A advisorTypes object <see cref="Dtos.AdvisorTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/advisor-types/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdvisorTypesV8")]
        public async Task<ActionResult<Dtos.AdvisorTypes>> PutAdvisorTypesAsync([FromRoute] string guid, [FromBody] Dtos.AdvisorTypes advisorTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a advisorTypes
        /// </summary>
        /// <param name="guid">GUID to desired advisorTypes</param>
        [HttpDelete]
        [Route("/advisor-types/{guid}", Name = "DefaultDeleteAdvisorTypes")]
        public async Task<IActionResult> DeleteAdvisorTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Visa Type data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class VisaTypesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly IDemographicService _demographicService;

        /// <summary>
        /// Initializes a new instance of the VisaTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public VisaTypesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IDemographicService demographicService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _demographicService = demographicService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Visa Types.
        /// </summary>
        /// <returns>All <see cref="VisaType">Visa Type codes and descriptions.</see></returns>
        /// <note>VisaType is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/visa-types", 1, false, Name = "GetVisaTypes")]
        public IEnumerable<VisaType> Get()
        {
            var visaTypeCollection = _referenceDataRepository.VisaTypes;

            // Get the right adapter for the type mapping
            var visaTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.VisaType, VisaType>();

            // Map the visaType entity to the program DTO
            var visaTypeDtoCollection = new List<VisaType>();
            foreach (var visaType in visaTypeCollection)
            {
                visaTypeDtoCollection.Add(visaTypeDtoAdapter.MapToType(visaType));
            }

            return visaTypeDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Visa Types
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Dtos.VisaType">Visa Types.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/visa-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVisaTypesV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.VisaType>>> GetVisaTypesAsync()
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
                var visaType = await _demographicService.GetVisaTypesAsync(bypassCache);

                if (visaType != null && visaType.Any())
                {
                    AddEthosContextProperties(await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              visaType.Select(a => a.Id).ToList()));
                }
                return Ok(visaType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Visa Type by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.VisaType">Visa Type.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/visa-types/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVisaTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.VisaType>> GetVisaTypeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _demographicService.GetVisaTypeByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>        
        /// Creates an Visa Type
        /// </summary>
        /// <param name="visaType"><see cref="Dtos.VisaType">VisaType</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.VisaType">VisaType</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/visa-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostVisaTypeV6")]
        public async Task<ActionResult<Dtos.VisaType>> PostVisaTypeAsync([FromBody] Dtos.VisaType visaType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>        
        /// Updates an Visa Type.
        /// </summary>
        /// <param name="id">Id of the Visa Type to update</param>
        /// <param name="visaType"><see cref="Dtos.VisaType">VisaType</see> to create</param>
        /// <returns>Updated <see cref="Dtos.VisaType">VisaType</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/visa-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutVisaTypeV6")]
        public async Task<ActionResult<Dtos.VisaType>> PutVisaTypeAsync([FromRoute] string id, [FromBody] Dtos.VisaType visaType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Visa Type
        /// </summary>
        /// <param name="id">Id of the Visa Type to delete</param>
        [HttpDelete]
        [Route("/visa-types/{id}", Name = "DefaultDeleteVisaType", Order = -10)]
        public async Task<IActionResult> DeleteVisaTypeAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

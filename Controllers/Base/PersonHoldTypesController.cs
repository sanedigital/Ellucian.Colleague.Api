// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to person hold types
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonHoldTypesController :BaseCompressedApiController
    {
        private readonly IPersonHoldTypeService _personHoldTypeService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// PersonHoldTypesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="personHoldTypeService">Service of type <see cref="IPersonHoldTypeService">IPersonHoldTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonHoldTypesController(IAdapterRegistry adapterRegistry, IPersonHoldTypeService personHoldTypeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _personHoldTypeService = personHoldTypeService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4.5</remarks>
        /// <summary>
        /// Retrieves all person hold types
        /// </summary>
        /// <returns>All PersonHoldTypes objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/person-hold-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonHoldTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PersonHoldType>>> GetPersonHoldTypesAsync()
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
                var items = await _personHoldTypeService.GetPersonHoldTypesAsync(bypassCache);

                AddEthosContextProperties(
                    await _personHoldTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personHoldTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4.5</remarks>
        /// <summary>
        /// Retrieves a person hold type by ID
        /// </summary>
        /// <returns>A PersonHoldType object.</returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/person-hold-types/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonHoldTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PersonHoldType>> GetPersonHoldTypeByIdAsync([FromRoute] string id)
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
                return await _personHoldTypeService.GetPersonHoldTypeByGuid2Async(id, bypassCache);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Delete (DELETE) an existing PersonHoldType
        /// </summary>
        /// <param name="id">Id of the PersonHoldType to delete</param>
        [HttpDelete]
        [Route("/person-hold-types/{id}", Name = "DefaultDeletePersonHoldTypes", Order = -10)]
        public async Task<ActionResult<Dtos.PersonHoldType>> DeletePersonHoldTypesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a PersonHoldType.
        /// </summary>
        /// <param name="personHoldType"><see cref="PersonHoldType">PersonHoldType</see> to update</param>
        /// <returns>Newly updated <see cref="PersonHoldType">PersonHoldType</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/person-hold-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonHoldTypesV6")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PersonHoldType>> PutPersonHoldTypesAsync([FromBody] Ellucian.Colleague.Dtos.PersonHoldType personHoldType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a PersonHoldType.
        /// </summary>
        /// <param name="personHoldType"><see cref="PersonHoldType">PersonHoldType</see> to create</param>
        /// <returns>Newly created <see cref="PersonHoldType">PersonHoldType</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/person-hold-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonHoldTypesV6")]
        public async Task<ActionResult<Dtos.PersonHoldType>> PostPersonHoldTypesAsync([FromBody] Dtos.PersonHoldType personHoldType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

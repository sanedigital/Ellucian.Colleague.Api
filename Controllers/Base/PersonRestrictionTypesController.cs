// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
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


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to RestrictionTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonRestrictionTypesController : BaseCompressedApiController
    {
        private readonly IRestrictionTypeService _restrictionTypeService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// RestrictionTypesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="restrictionTypeService">Service of type <see cref="IRestrictionTypeService">IRestrictionTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonRestrictionTypesController(IAdapterRegistry adapterRegistry, IRestrictionTypeService restrictionTypeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _restrictionTypeService = restrictionTypeService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Retrieves all restriction types
        /// </summary>
        /// <returns>All RestrictionType objects.</returns>
        /// <note>Restriction is cached for 24 hours.</note>
        [Obsolete("Obsolete as of HeDM Version 4, use Accept Header Version 4 instead.")]
        [HttpGet]
        [HeaderVersionRoute("/restriction-types", 1, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonRestrictionTypes")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.RestrictionType>>> GetRestrictionTypesAsync()
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
                return Ok(await _restrictionTypeService.GetRestrictionTypesAsync(bypassCache));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Retrieves a restriction type by GUID
        /// </summary>
        /// <returns>A RestrictionType object.</returns>
        [Obsolete("Obsolete as of HeDM Version 4, use Accept Header Version 4 instead.")]
        [HttpGet]
        [HeaderVersionRoute("/restriction-types/{guid}", 1, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonRestrictionTypeByGuid")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.RestrictionType>> GetRestrictionTypeByGuidAsync(string guid)
        {
            try
            {
                return await _restrictionTypeService.GetRestrictionTypeByGuidAsync(guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Retrieves all restriction types
        /// </summary>
        /// <returns>All RestrictionType objects.</returns>
        [Obsolete("Obsolete as of HeDM Version 4.5, use person-hold-types API instead.")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.RestrictionType2>>> GetRestrictionTypes2Async()
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
                return Ok(await _restrictionTypeService.GetRestrictionTypes2Async(bypassCache));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Retrieves a restriction type by ID
        /// </summary>
        /// <returns>A RestrictionType object.</returns>
        [Obsolete("Obsolete as of HeDM Version 4.5, use person-hold-types API instead.")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.RestrictionType2>> GetRestrictionTypeById2Async(string id)
        {
            try
            {
                return await _restrictionTypeService.GetRestrictionTypeByGuid2Async(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates a RestrictionType.
        /// </summary>
        /// <param name="restrictionType"><see cref="RestrictionType2">RestrictionType</see> to update</param>
        /// <returns>Newly updated <see cref="RestrictionType2">RestrictionType</see></returns>
        [Obsolete("Obsolete as of HeDM Version 4.5, use person-hold-types API instead.")]
        [HttpPut]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.RestrictionType2>> PutRestrictionTypesAsync([FromBody] Ellucian.Colleague.Dtos.RestrictionType2 restrictionType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a PersonHoldType.
        /// </summary>
        /// <param name="personHoldType"><see cref="PersonHoldType">PersonHoldType</see> to update</param>
        /// <returns>Newly updated <see cref="PersonHoldType">PersonHoldType</see></returns>
        [HttpPut]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PersonHoldType>> PutPersonHoldTypesAsync([FromBody] Ellucian.Colleague.Dtos.PersonHoldType personHoldType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a RestrictionType.
        /// </summary>
        /// <param name="restrictionType"><see cref="RestrictionType2">RestrictionType</see> to create</param>
        /// <returns>Newly created <see cref="RestrictionType2">RestrictionType</see></returns>
        [Obsolete("Obsolete as of HeDM Version 4.5, use person-hold-types API instead.")]
        [HttpPost]
        public async Task<ActionResult<Dtos.RestrictionType2>> PostRestrictionTypesAsync([FromBody] Dtos.RestrictionType2 restrictionType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a PersonHoldType.
        /// </summary>
        /// <param name="personHoldType"><see cref="PersonHoldType">PersonHoldType</see> to create</param>
        /// <returns>Newly created <see cref="PersonHoldType">PersonHoldType</see></returns>
        [HttpPost]
        public async Task<ActionResult<Dtos.PersonHoldType>> PostPersonHoldTypesAsync([FromBody] Dtos.PersonHoldType personHoldType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing RestrictionType
        /// </summary>
        /// <param name="id">Id of the RestrictionType to delete</param>
        [Obsolete("Obsolete as of HeDM Version 4.5, use person-hold-types API instead.")]
        [HttpDelete]
        public async Task<ActionResult<Dtos.RestrictionType2>> DeleteRestrictionTypesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing PersonHoldType
        /// </summary>
        /// <param name="id">Id of the PersonHoldType to delete</param>
        [HttpDelete]
        public async Task<ActionResult<Dtos.PersonHoldType>> DeletePersonHoldTypesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

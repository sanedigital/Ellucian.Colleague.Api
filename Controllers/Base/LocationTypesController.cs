// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to LocationType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class LocationTypesController : BaseCompressedApiController
    {
        
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILocationTypeService _locationTypeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the LocationTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="locationTypeService">Service of type <see cref="ILocationTypeService">ILocationTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public LocationTypesController(IAdapterRegistry adapterRegistry, ILocationTypeService locationTypeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _locationTypeService = locationTypeService;
            this._logger = logger;
        }

        
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all location types.
        /// </summary>
        /// <returns>All <see cref="Dtos.LocationTypeItem">EmailType</see> objects.</returns>
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.LocationTypeItem>>> GetLocationTypesAsync()
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
                return Ok(await _locationTypeService.GetLocationTypesAsync(bypassCache));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an location type by GUID.
        /// </summary>
        /// /// <param name="id">Unique ID representing the Location Type to get</param>
        /// <returns>An <see cref="Dtos.LocationTypeItem">LocationType</see> object.</returns>
        public async Task<ActionResult<Ellucian.Colleague.Dtos.LocationTypeItem>> GetLocationTypeByGuidAsync(string id)
        {
            try
            {
                return await _locationTypeService.GetLocationTypeByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete an existing Location type in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Unique ID representing the Location Type to delete</param>
        [HttpDelete]
        public IActionResult DeleteLocationTypes(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Update a Location Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="LocationType"><see cref="LocationTypeItem">LocationType</see> to update</param>
        [HttpPut]
        public ActionResult<Ellucian.Colleague.Dtos.LocationTypeItem> PutLocationTypes([FromBody] Ellucian.Colleague.Dtos.LocationTypeItem LocationType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Post Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Create a Location Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="LocationType"><see cref="LocationType">LocationType</see> to create</param>
        [HttpPost]
        public ActionResult<Ellucian.Colleague.Dtos.LocationTypeItem> PostLocationTypes([FromBody] Ellucian.Colleague.Dtos.LocationTypeItem LocationType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}

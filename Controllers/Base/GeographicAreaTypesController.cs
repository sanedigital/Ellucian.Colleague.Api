// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
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
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Geographic Area Types data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class GeographicAreaTypesController : BaseCompressedApiController
    {
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GeographicAreaTypesController class.
        /// </summary>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GeographicAreaTypesController(IDemographicService demographicService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _demographicService = demographicService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves all geographic area types.
        /// </summary>
        /// <returns>All GeographicAreaType objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/geographic-area-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetGeographicAreaTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.GeographicAreaType>>> GetGeographicAreaTypesAsync()
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
                return Ok(await _demographicService.GetGeographicAreaTypesAsync(bypassCache));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves a geographic area types by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.GeographicAreaTypes">GeographicAreaType.</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/geographic-area-types/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetGeographicAreaTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.GeographicAreaType>> GetGeographicAreaTypeByIdAsync(string id)
        {
            try
            {
                return await _demographicService.GetGeographicAreaTypeByGuidAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a GeographicAreaType.
        /// </summary>
        /// <param name="geographicAreaType"><see cref="GeographicAreaType">GeographicAreaType</see> to update</param>
        /// <returns>Newly updated <see cref="GeographicAreaType">GeographicAreaType</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/geographic-area-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGeographicAreaTypeV6")]
        public async Task<ActionResult<Dtos.GeographicAreaType>> PutGeographicAreaTypeAsync([FromBody] Dtos.GeographicAreaType geographicAreaType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a GeographicAreaType.
        /// </summary>
        /// <param name="geographicAreaType"><see cref="GeographicAreaType">GeographicAreaType</see> to create</param>
        /// <returns>Newly created <see cref="GeographicAreaType">GeographicAreaType</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/geographic-area-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGeographicAreaTypeV6")]
        public async Task<ActionResult<Dtos.GeographicAreaType>> PostGeographicAreaTypeAsync([FromBody] Dtos.GeographicAreaType geographicAreaType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing GeographicAreaType
        /// </summary>
        /// <param name="id">Id of the GeographicAreaType to delete</param>
        [HttpDelete]
        [Route("/geographic-area-types/{id}", Name = "DeleteGeographicAreaType", Order = -10)]
        public async Task<IActionResult> DeleteGeographicAreaTypeAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

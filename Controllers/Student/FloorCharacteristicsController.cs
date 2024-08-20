// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to FloorCharacteristics
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class FloorCharacteristicsController : BaseCompressedApiController
    {
        private readonly IFloorCharacteristicsService _floorCharacteristicsService;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// Initializes a new instance of the FloorCharacteristicsController class.
        /// </summary>
        /// <param name="floorCharacteristicsService">Service of type <see cref="IFloorCharacteristicsService">IFloorCharacteristicsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FloorCharacteristicsController(IFloorCharacteristicsService floorCharacteristicsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _floorCharacteristicsService = floorCharacteristicsService;
            this._logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Return all floorCharacteristics
        /// </summary>
        /// <returns>List of FloorCharacteristics <see cref="Dtos.FloorCharacteristics"/> objects representing matching floorCharacteristics</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/floor-characteristics", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFloorCharacteristics", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.FloorCharacteristics>>> GetFloorCharacteristicsAsync()
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
                var floorCharacteristics = await _floorCharacteristicsService.GetFloorCharacteristicsAsync(bypassCache);

                if (floorCharacteristics != null && floorCharacteristics.Any())
                {
                    AddEthosContextProperties(await _floorCharacteristicsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _floorCharacteristicsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              floorCharacteristics.Select(a => a.Id).ToList()));
                }
                return Ok(floorCharacteristics);
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
        /// Read (GET) a floorCharacteristics using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired floorCharacteristics</param>
        /// <returns>A floorCharacteristics object <see cref="Dtos.FloorCharacteristics"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/floor-characteristics/{guid}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFloorCharacteristicsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FloorCharacteristics>> GetFloorCharacteristicsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _floorCharacteristicsService.GetFloorCharacteristicsByGuidAsync(guid);
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
        /// Create (POST) a new floorCharacteristics
        /// </summary>
        /// <param name="floorCharacteristics">DTO of the new floorCharacteristics</param>
        /// <returns>A floorCharacteristics object <see cref="Dtos.FloorCharacteristics"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/floor-characteristics", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFloorCharacteristicsV8")]
        public async Task<ActionResult<Dtos.FloorCharacteristics>> PostFloorCharacteristicsAsync([FromBody] Dtos.FloorCharacteristics floorCharacteristics)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing floorCharacteristics
        /// </summary>
        /// <param name="guid">GUID of the floorCharacteristics to update</param>
        /// <param name="floorCharacteristics">DTO of the updated floorCharacteristics</param>
        /// <returns>A floorCharacteristics object <see cref="Dtos.FloorCharacteristics"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/floor-characteristics/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFloorCharacteristicsV8")]
        public async Task<ActionResult<Dtos.FloorCharacteristics>> PutFloorCharacteristicsAsync([FromRoute] string guid, [FromBody] Dtos.FloorCharacteristics floorCharacteristics)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a floorCharacteristics
        /// </summary>
        /// <param name="guid">GUID to desired floorCharacteristics</param>
        [HttpDelete]
        [Route("/floor-characteristics/{guid}", Name = "DefaultDeleteFloorCharacteristics", Order = -10)]
        public async Task<IActionResult> DeleteFloorCharacteristicsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

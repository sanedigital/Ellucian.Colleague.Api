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
    /// Provides access to RoommateCharacteristics
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class RoommateCharacteristicsController : BaseCompressedApiController
    {
        private readonly IRoommateCharacteristicsService _roommateCharacteristicsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RoommateCharacteristicsController class.
        /// </summary>
        /// <param name="roommateCharacteristicsService">Service of type <see cref="IRoommateCharacteristicsService">IRoommateCharacteristicsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RoommateCharacteristicsController(IRoommateCharacteristicsService roommateCharacteristicsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _roommateCharacteristicsService = roommateCharacteristicsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all roommateCharacteristics
        /// </summary>
        /// <returns>List of RoommateCharacteristics <see cref="Dtos.RoommateCharacteristics"/> objects representing matching roommateCharacteristics</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/roommate-characteristics", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRoommateCharacteristics", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.RoommateCharacteristics>>> GetRoommateCharacteristicsAsync()
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
                var roommateCharacteristics = await _roommateCharacteristicsService.GetRoommateCharacteristicsAsync(bypassCache);

                if (roommateCharacteristics != null && roommateCharacteristics.Any())
                {
                    AddEthosContextProperties(await _roommateCharacteristicsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _roommateCharacteristicsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              roommateCharacteristics.Select(a => a.Id).ToList()));
                }                

                return Ok(roommateCharacteristics);
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
        /// Read (GET) a roommateCharacteristics using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired roommateCharacteristics</param>
        /// <returns>A roommateCharacteristics object <see cref="Dtos.RoommateCharacteristics"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/roommate-characteristics/{guid}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRoommateCharacteristicsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.RoommateCharacteristics>> GetRoommateCharacteristicsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _roommateCharacteristicsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _roommateCharacteristicsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _roommateCharacteristicsService.GetRoommateCharacteristicsByGuidAsync(guid);
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
        /// Create (POST) a new roommateCharacteristics
        /// </summary>
        /// <param name="roommateCharacteristics">DTO of the new roommateCharacteristics</param>
        /// <returns>A roommateCharacteristics object <see cref="Dtos.RoommateCharacteristics"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/roommate-characteristics", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRoommateCharacteristicsV8")]
        public async Task<ActionResult<Dtos.RoommateCharacteristics>> PostRoommateCharacteristicsAsync([FromBody] Dtos.RoommateCharacteristics roommateCharacteristics)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing roommateCharacteristics
        /// </summary>
        /// <param name="guid">GUID of the roommateCharacteristics to update</param>
        /// <param name="roommateCharacteristics">DTO of the updated roommateCharacteristics</param>
        /// <returns>A roommateCharacteristics object <see cref="Dtos.RoommateCharacteristics"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/roommate-characteristics/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRoommateCharacteristicsV8")]
        public async Task<ActionResult<Dtos.RoommateCharacteristics>> PutRoommateCharacteristicsAsync([FromRoute] string guid, [FromBody] Dtos.RoommateCharacteristics roommateCharacteristics)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a roommateCharacteristics
        /// </summary>
        /// <param name="guid">GUID to desired roommateCharacteristics</param>
        [HttpDelete]
        [Route("/roommate-characteristics/{guid}", Name = "DefaultDeleteRoommateCharacteristics", Order = -10)]
        public async Task<IActionResult> DeleteRoommateCharacteristicsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

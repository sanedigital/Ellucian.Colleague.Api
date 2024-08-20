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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to VeteranStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class VeteranStatusesController : BaseCompressedApiController
    {
        private readonly IVeteranStatusesService _veteranStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the VeteranStatusesController class.
        /// </summary>
        /// <param name="veteranStatusesService">Service of type <see cref="IVeteranStatusesService">IVeteranStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public VeteranStatusesController(IVeteranStatusesService veteranStatusesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _veteranStatusesService = veteranStatusesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all veteranStatuses
        /// </summary>
        /// <returns>List of VeteranStatuses <see cref="Dtos.VeteranStatuses"/> objects representing matching veteranStatuses</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/veteran-statuses", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetVeteranStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.VeteranStatuses>>> GetVeteranStatusesAsync()
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
                var veteranStatuses = await _veteranStatusesService.GetVeteranStatusesAsync(bypassCache);

                if (veteranStatuses != null && veteranStatuses.Any())
                {
                    AddEthosContextProperties(await _veteranStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _veteranStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              veteranStatuses.Select(a => a.Id).ToList()));
                }

                return Ok(veteranStatuses);                
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
        /// Read (GET) a veteranStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired veteranStatuses</param>
        /// <returns>A veteranStatuses object <see cref="Dtos.VeteranStatuses"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/veteran-statuses/{guid}", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVeteranStatusesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.VeteranStatuses>> GetVeteranStatusesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _veteranStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _veteranStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _veteranStatusesService.GetVeteranStatusesByGuidAsync(guid);
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
        /// Create (POST) a new veteranStatuses
        /// </summary>
        /// <param name="veteranStatuses">DTO of the new veteranStatuses</param>
        /// <returns>A veteranStatuses object <see cref="Dtos.VeteranStatuses"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/veteran-statuses", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostVeteranStatusesV9")]
        public async Task<ActionResult<Dtos.VeteranStatuses>> PostVeteranStatusesAsync([FromBody] Dtos.VeteranStatuses veteranStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing veteranStatuses
        /// </summary>
        /// <param name="guid">GUID of the veteranStatuses to update</param>
        /// <param name="veteranStatuses">DTO of the updated veteranStatuses</param>
        /// <returns>A veteranStatuses object <see cref="Dtos.VeteranStatuses"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/veteran-statuses/{guid}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutVeteranStatusesV9")]
        public async Task<ActionResult<Dtos.VeteranStatuses>> PutVeteranStatusesAsync([FromRoute] string guid, [FromBody] Dtos.VeteranStatuses veteranStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a veteranStatuses
        /// </summary>
        /// <param name="guid">GUID to desired veteranStatuses</param>
        [HttpDelete]
        [Route("/veteran-statuses/{guid}", Name = "DefaultDeleteVeteranStatuses", Order = -10)]
        public async Task<IActionResult> DeleteVeteranStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

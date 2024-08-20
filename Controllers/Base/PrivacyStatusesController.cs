// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Privacy Statuses data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PrivacyStatusesController : BaseCompressedApiController
    {
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PrivacyStatusesController class.
        /// </summary>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PrivacyStatusesController(IDemographicService demographicService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _demographicService = demographicService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves all privacy statuses.
        /// </summary>
        /// <returns>All PrivacyStatus objects.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/privacy-statuses", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPrivacyStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PrivacyStatus>>> GetPrivacyStatusesAsync()
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

                var privacyStatuses = await _demographicService.GetPrivacyStatusesAsync(bypassCache);
                AddEthosContextProperties(
                    await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        privacyStatuses.Select(i => i.Id).ToList()));

                return Ok(privacyStatuses);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving privacy statuses";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves a privacy statuses by ID.
        /// </summary>
        /// <returns>A PrivacyStatus.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/privacy-statuses/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPrivacyStatusById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PrivacyStatus>> GetPrivacyStatusByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _demographicService.GetPrivacyStatusByGuidAsync(id);
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
        /// Updates a PrivacyStatus.
        /// </summary>
        /// <param name="privacyStatus">PrivacyStatus to update</param>
        /// <returns>Newly updated PrivacyStatus</returns>
        [HttpPut]
        [HeaderVersionRoute("/privacy-statuses/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPrivacyStatusesV6")]
        public async Task<ActionResult<Dtos.PrivacyStatus>> PutPrivacyStatusAsync([FromBody] Dtos.PrivacyStatus privacyStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a PrivacyStatus.
        /// </summary>
        /// <param name="privacyStatus">PrivacyStatus to create</param>
        /// <returns>Newly created PrivacyStatus</returns>
        [HttpPost]
        [HeaderVersionRoute("/privacy-statuses", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPrivacyStatusV6")]
        public async Task<ActionResult<Dtos.PrivacyStatus>> PostPrivacyStatusAsync([FromBody] Dtos.PrivacyStatus privacyStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing PrivacyStatus
        /// </summary>
        /// <param name="id">Id of the PrivacyStatus to delete</param>
        [HttpDelete]
        [Route("/privacy-statuses/{id}", Name = "DeletePrivacyStatus", Order = -10)]
        public async Task<IActionResult> DeletePrivacyStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

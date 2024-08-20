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
    /// Provides access to Citizenship Statuses data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CitizenshipStatusesController : BaseCompressedApiController
    {
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RacesController class.
        /// </summary>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CitizenshipStatusesController(IDemographicService demographicService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _demographicService = demographicService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves all citizenship statuses.
        /// </summary>
        /// <returns>All CitizenshipStatuses objects.</returns>
        /// 
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
                [HttpGet]
                [HeaderVersionRoute("/citizenship-statuses", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetCitizenshipStatuses", IsEedmSupported = true)]
                public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CitizenshipStatus>>> GetCitizenshipStatusesAsync()
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

                var citizenshipStatuses = await _demographicService.GetCitizenshipStatusesAsync(bypassCache);

                if (citizenshipStatuses != null && citizenshipStatuses.Any())
                {
                    AddEthosContextProperties(await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              citizenshipStatuses.Select(a => a.Id).ToList()));
                }
                return Ok(citizenshipStatuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves a citizenship status by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CitizenshipStatus">CitizenshipStatus.</see></returns>
        /// 
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
                [HttpGet]
                [HeaderVersionRoute("/citizenship-statuses/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetCitizenshipStatusById", IsEedmSupported = true)]
                public async Task<ActionResult<Ellucian.Colleague.Dtos.CitizenshipStatus>> GetCitizenshipStatusByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _demographicService.GetCitizenshipStatusByGuidAsync(id);
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
        /// Updates a CitizenshipStatus.
        /// </summary>
        /// <param name="citizenshipStatus"><see cref="CitizenshipStatus">CitizenshipStatus</see> to update</param>
        /// <returns>Newly updated <see cref="CitizenshipStatus">CitizenshipStatus</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/citizenship-statuses/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCitizenshipStatusV6")]
        public async Task<ActionResult<Dtos.CitizenshipStatus>> PutCitizenshipStatusAsync([FromBody] Dtos.CitizenshipStatus citizenshipStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a CitizenshipStatus.
        /// </summary>
        /// <param name="citizenshipStatus"><see cref="CitizenshipStatus">CitizenshipStatus</see> to create</param>
        /// <returns>Newly created <see cref="CitizenshipStatus">CitizenshipStatus</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/citizenship-statuses", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCitizenshipStatusV6")]
        public async Task<ActionResult<Dtos.CitizenshipStatus>> PostCitizenshipStatusAsync([FromBody] Dtos.CitizenshipStatus citizenshipStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing CitizenshipStatus
        /// </summary>
        /// <param name="id">Id of the CitizenshipStatus to delete</param>
        [HttpDelete]
        [Route("/citizenship-statuses/{id}", Name = "DeleteCitizenshipStatus", Order = -10)]
        public async Task<IActionResult> DeleteCitizenshipStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

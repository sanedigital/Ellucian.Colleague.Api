// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Coordination.HumanResources.Services;
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

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to Job Change Reasons data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class JobChangeReasonsController : BaseCompressedApiController
    {
        private readonly IJobChangeReasonService _jobChangeReasonService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the JobChangeReasonsController class.
        /// </summary>
        /// <param name="jobChangeReasonService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public JobChangeReasonsController(IJobChangeReasonService jobChangeReasonService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _jobChangeReasonService = jobChangeReasonService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves all rehire types.
        /// </summary>
        /// <returns>All JobChangeReason objects.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/job-change-reasons", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetJobChangeReasons", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.JobChangeReason>>> GetJobChangeReasonsAsync()
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
                var rehireTypes = await _jobChangeReasonService.GetJobChangeReasonsAsync(bypassCache);

                if (rehireTypes != null && rehireTypes.Any())
                {
                    AddEthosContextProperties(await _jobChangeReasonService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _jobChangeReasonService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              rehireTypes.Select(a => a.Id).ToList()));
                }

                return Ok(rehireTypes);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves a rehire type by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.JobChangeReason">JobChangeReason.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/job-change-reasons/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetJobChangeReasonById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.JobChangeReason>> GetJobChangeReasonByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _jobChangeReasonService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _jobChangeReasonService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _jobChangeReasonService.GetJobChangeReasonByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates a JobChangeReason.
        /// </summary>
        /// <param name="jobChangeReason"><see cref="JobChangeReason">JobChangeReason</see> to update</param>
        /// <returns>Newly updated <see cref="JobChangeReason">JobChangeReason</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/job-change-reasons/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutJobChangeReasonsV7")]
        public async Task<ActionResult<Dtos.JobChangeReason>> PutJobChangeReasonAsync([FromBody] Dtos.JobChangeReason jobChangeReason)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a JobChangeReason.
        /// </summary>
        /// <param name="jobChangeReason"><see cref="JobChangeReason">JobChangeReason</see> to create</param>
        /// <returns>Newly created <see cref="JobChangeReason">JobChangeReason</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/job-change-reasons", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostJobChangeReasonsV7")]
        public async Task<ActionResult<Dtos.JobChangeReason>> PostJobChangeReasonAsync([FromBody] Dtos.JobChangeReason jobChangeReason)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing JobChangeReason
        /// </summary>
        /// <param name="id">Id of the JobChangeReason to delete</param>
        [HttpDelete]
        [Route("/job-change-reasons/{id}", Name = "DeleteJobChangeReasons")]
        public async Task<IActionResult> DeleteJobChangeReasonAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

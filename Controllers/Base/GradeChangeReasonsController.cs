// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provide access to grade change reason
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class GradeChangeReasonsController :BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private IGradeChangeReasonService _gradeChangeReasonService;

        /// <summary>
        /// Initializes a new instance of the GradeChangeReasonController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="gradeChangeReasonService">Service of type <see cref="IGradeChangeReasonService">IGradeChangeReasonService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GradeChangeReasonsController(IAdapterRegistry adapterRegistry, IGradeChangeReasonService gradeChangeReasonService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _gradeChangeReasonService = gradeChangeReasonService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves information for all grade change reasons.
        /// </summary>
        /// <returns>All <see cref="Dtos.GradeChangeReason">GradeChangeReasons</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/grade-change-reasons", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetGradeChangeReasons", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.GradeChangeReason>>> GetGradeChangeReasonsAsync()
        {
            bool bypassCache = false;
            if(Request.GetTypedHeaders().CacheControl != null)
            {
                if(Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var gradeChangeReasons = await _gradeChangeReasonService.GetAsync(bypassCache);

                if (gradeChangeReasons != null && gradeChangeReasons.Any())
                {
                    AddEthosContextProperties(await _gradeChangeReasonService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _gradeChangeReasonService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              gradeChangeReasons.Select(a => a.Id).ToList()));
                }

                return Ok(gradeChangeReasons);                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves grade change reason by id
        /// </summary>
        /// <param name="id">The id of the grade change reason</param>
        /// <returns>The requested <see cref="Dtos.GradeChangeReason">GradeChangeReason</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/grade-change-reasons/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetGradeChangeReasonById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GradeChangeReason>> GetGradeChangeReasonByIdAsync(string id)
        {
            try
            {
                if(string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException(id, "Grade Change Reason id cannot be null or empty");
                }
                AddEthosContextProperties(
                    await _gradeChangeReasonService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _gradeChangeReasonService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _gradeChangeReasonService.GetGradeChangeReasonByIdAsync(id);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Create a grade change reason
        /// </summary>
        /// <param name="gradeChangeReason">grade</param>
        /// <returns>A section object <see cref="Dtos.GradeChangeReason"/> in HeDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/grade-change-reasons", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGradeChangeReasonV6")]
        public async Task<ActionResult<Dtos.GradeChangeReason>> PostGradeChangeReasonAsync([FromBody] Ellucian.Colleague.Dtos.GradeChangeReason gradeChangeReason)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update a grade change reason
        /// </summary>
        /// <param name="id">desired id for a grade change reason</param>
        /// <param name="gradeChangeReason">grade change reason</param>
        /// <returns>A section object <see cref="Dtos.GradeChangeReason"/> in HeDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/grade-change-reasons/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGradeChangeReasonV6")]
        public async Task<ActionResult<Dtos.GradeChangeReason>> PutGradeChangeReasonAsync([FromRoute] string id, [FromBody] Dtos.GradeChangeReason gradeChangeReason)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a grade change reason
        /// </summary>
        /// <param name="id">id to desired grade change reason</param>
        /// <returns>A section object <see cref="Dtos.GradeChangeReason"/> in HeDM format</returns>
        [HttpDelete]
        [Route("/grade-change-reasons/{id}", Name = "DeleteGradeChangeReason", Order = -10)]
        public async Task<ActionResult<Dtos.GradeChangeReason>> DeleteGradeChangeReasonByIdAsync(string id)
        {
            //Delete is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to employee termination reason data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentTerminationReasonsController : BaseCompressedApiController
    {
        private readonly IEmploymentStatusEndingReasonService _employmentStatusEndingReasonService;
        private readonly ILogger _logger;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="employmentStatusEndingReasonService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmploymentTerminationReasonsController(IEmploymentStatusEndingReasonService employmentStatusEndingReasonService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _employmentStatusEndingReasonService = employmentStatusEndingReasonService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves all employment termination reasons.
        /// </summary>
        /// <returns>All employmentT termination reason objects.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/employment-termination-reasons", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmEmploymentTerminationReasons", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.EmploymentStatusEndingReason>>> GetEmploymentTerminationReasonsAsync()
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
                var employmentTerminationReasons = await _employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonsAsync(bypassCache);

                if (employmentTerminationReasons != null && employmentTerminationReasons.Any())
                {
                    AddEthosContextProperties(await _employmentStatusEndingReasonService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _employmentStatusEndingReasonService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              employmentTerminationReasons.Select(a => a.Id).ToList()));
                }
                return Ok(employmentTerminationReasons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves a employment termination reason by Id.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.EmploymentStatusEndingReason">EmploymentStatusEndingReason.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/employment-termination-reasons/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmEmploymentTerminationReasonById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.EmploymentStatusEndingReason>> GetEmploymentTerminationReasonByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _employmentStatusEndingReasonService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _employmentStatusEndingReasonService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates (PUT) an employment termination reason.
        /// </summary>
        /// <param name="employmentStatusEndingReason"><see cref="EmploymentStatusEndingReason">EmploymentStatusEndingReason</see> to update</param>
        /// <returns>Newly updated <see cref="EmploymentStatusEndingReason">EmploymentStatusEndingReason</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/employment-termination-reasons/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmEmploymentTerminationReason")]
        public async Task<ActionResult<Dtos.EmploymentStatusEndingReason>> PutEmploymentTerminationReasonAsync([FromBody] Dtos.EmploymentStatusEndingReason employmentStatusEndingReason)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates (POST) an employment termination reason.
        /// </summary>
        /// <param name="employmentStatusEndingReason"><see cref="EmploymentStatusEndingReason">EmploymentStatusEndingReason</see> to create</param>
        /// <returns>Newly created <see cref="EmploymentStatusEndingReason">EmploymentStatusEndingReason</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/employment-termination-reasons", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmEmploymentTerminationReason")]
        public async Task<ActionResult<Dtos.EmploymentStatusEndingReason>> PostEmploymentTerminationReasonAsync([FromBody] Dtos.EmploymentStatusEndingReason employmentStatusEndingReason)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing employment termination reason.
        /// </summary>
        /// <param name="id">Id of the employment termination reason to delete.</param>
        [HttpDelete]
        [Route("/employment-termination-reasons/{id}", Name = "DeleteHedmEmploymentTerminationReason")]
        public async Task<IActionResult> DeleteEmploymentTerminationReasonAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

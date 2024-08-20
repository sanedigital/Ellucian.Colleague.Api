// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes Employment Leave of Absence Reasons data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentLeaveOfAbsenceReasonsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IEmploymentStatusEndingReasonService _employmentStatusEndingReasonService;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="employmentStatusEndingReasonService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmploymentLeaveOfAbsenceReasonsController(ILogger logger, IEmploymentStatusEndingReasonService employmentStatusEndingReasonService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this._employmentStatusEndingReasonService = employmentStatusEndingReasonService;
        }

        /// <summary>
        /// Returns all employment leave of absence reasons
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/employment-leave-of-absence-reasons", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmEmploymentLeaveOfAbsenceReasons", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.EmploymentStatusEndingReason>>> GetAllEmploymentLeaveOfAbsenceReasonsAsync()
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
                var employmentLeaveOfAbsenceReasons = await _employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonsAsync(bypassCache);

                if (employmentLeaveOfAbsenceReasons != null && employmentLeaveOfAbsenceReasons.Any())
                {
                    AddEthosContextProperties(await _employmentStatusEndingReasonService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _employmentStatusEndingReasonService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              employmentLeaveOfAbsenceReasons.Select(a => a.Id).ToList()));
                }

                return Ok(employmentLeaveOfAbsenceReasons);                
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting employment leave of absence reasons.");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns an employment leave of absence reason.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/employment-leave-of-absence-reasons/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmploymentLeaveOfAbsenceReasonById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmploymentStatusEndingReason>> GetEmploymentLeaveOfAbsenceReasonByIdAsync([FromRoute] string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _employmentStatusEndingReasonService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _employmentStatusEndingReasonService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e, "No employment leave of absence reason was found for guid " + id + ".");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting employment leave of absence reason.");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// PutEmploymentLeaveOfAbsenceReasonAsync.
        /// </summary>
        /// <param name="employmentStatusEndingReason"></param>
        /// <returns></returns>
        [HttpPut]
        [HeaderVersionRoute("/employment-leave-of-absence-reasons/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmEmploymentLeaveOfAbsenceReason")]
        public async Task<ActionResult<Dtos.EmploymentStatusEndingReason>> PutEmploymentLeaveOfAbsenceReasonAsync([FromBody] Dtos.EmploymentStatusEndingReason employmentStatusEndingReason)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// PostEmploymentLeaveOfAbsenceReasonAsync.
        /// </summary>
        /// <param name="employmentStatusEndingReason"></param>
        /// <returns></returns>
        [HttpPost]
        [HeaderVersionRoute("/employment-leave-of-absence-reasons", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmEmploymentLeaveOfAbsenceReason")]
        public async Task<ActionResult<Dtos.EmploymentStatusEndingReason>> PostEmploymentLeaveOfAbsenceReasonAsync([FromBody] Dtos.EmploymentStatusEndingReason employmentStatusEndingReason)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// DeleteEmploymentLeaveOfAbsenceReasonAsync.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/employment-leave-of-absence-reasons/{id}", Name = "DeleteEmploymentLeaveOfAbsenceReason")]
        public async Task<IActionResult> DeleteEmploymentLeaveOfAbsenceReasonAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

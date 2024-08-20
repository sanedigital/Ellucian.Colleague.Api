// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to Employee Classifications data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentClassificationsController : BaseCompressedApiController
    {
        private readonly IEmploymentClassificationService _employmentClassificationService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmployeeClassificationsController class.
        /// </summary>
        /// <param name="employmentClassificationsService">Service of type <see cref="IEmploymentClassificationService">IEmploymentClassificationsService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmploymentClassificationsController(IEmploymentClassificationService employmentClassificationsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _employmentClassificationService = employmentClassificationsService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves all employment classifications.
        /// </summary>
        /// <returns>All EmploymentClassification objects.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/employment-classifications", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmEmploymentClassifications", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.EmploymentClassification>>> GetEmploymentClassificationsAsync()
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
                var employmentClassification = await _employmentClassificationService.GetEmploymentClassificationsAsync(bypassCache);

                if (employmentClassification != null && employmentClassification.Any())
                {
                    AddEthosContextProperties(await _employmentClassificationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _employmentClassificationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              employmentClassification.Select(a => a.Id).ToList()));
                }

                return Ok(employmentClassification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves a employment classification by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.EmploymentClassification">EmploymentClassification.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/employment-classifications/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmEmploymentClassificationsById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.EmploymentClassification>> GetEmploymentClassificationByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _employmentClassificationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _employmentClassificationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _employmentClassificationService.GetEmploymentClassificationByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates a EmploymentClassification.
        /// </summary>
        /// <param name="employmentClassification"><see cref="Dtos.EmploymentClassification">EmploymentClassification</see> to update</param>
        /// <returns>Newly updated <see cref="Dtos.EmploymentClassification">EmploymentClassification</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/employment-classifications/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmEmploymentClassifications")]
        public async Task<ActionResult<Dtos.EmploymentClassification>> PutEmploymentClassificationAsync([FromBody] Dtos.EmploymentClassification employmentClassification)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a EmploymentClassification.
        /// </summary>
        /// <param name="employmentClassification"><see cref="Dtos.EmploymentClassification">EmploymentClassification</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.EmploymentClassification">EmploymentClassification</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/employment-classifications", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmEmploymentClassifications")]
        public async Task<ActionResult<Dtos.EmploymentClassification>> PostEmploymentClassificationAsync([FromBody] Dtos.EmploymentClassification employmentClassification)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing EmploymentClassification
        /// </summary>
        /// <param name="id">Id of the EmploymentClassification to delete</param>
        [HttpDelete]
        [Route("/employment-classifications/{id}", Name = "DeleteHedmEmploymentClassifications")]
        public async Task<IActionResult> DeleteEmploymentClassificationAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to FinancialAidAwardPeriods data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidAwardPeriodsController : BaseCompressedApiController
    {
        private readonly IFinancialAidAwardPeriodService _financialAidAwardPeriodService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidAwardPeriodsController class.
        /// </summary>
        /// <param name="financialAidAwardPeriodService">Repository of type <see cref="IFinancialAidAwardPeriodService">IFinancialAidAwardPeriodService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidAwardPeriodsController(IFinancialAidAwardPeriodService financialAidAwardPeriodService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _financialAidAwardPeriodService = financialAidAwardPeriodService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all financial aid award periods.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All FinancialAidAwardPeriod objects.</returns>
        [HttpGet, ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-award-periods", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidAwardPeriods", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.FinancialAidAwardPeriod>>> GetFinancialAidAwardPeriodsAsync()
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            
            try
            {                
                var items = await _financialAidAwardPeriodService.GetFinancialAidAwardPeriodsAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(
                        await _financialAidAwardPeriodService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _financialAidAwardPeriodService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            items.Select(i => i.Id).ToList()));
                }

                return Ok(items);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Retrieves an Financial Aid Award Periods by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.FinancialAidAwardPeriod">FinancialAidAwardPeriod</see>object.</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/financial-aid-award-periods/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidAwardPeriodById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodByIdAsync(string id)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                AddEthosContextProperties(
                    await _financialAidAwardPeriodService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _financialAidAwardPeriodService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _financialAidAwardPeriodService.GetFinancialAidAwardPeriodByGuidAsync(id);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Creates a Financial Aid Award Period.
        /// </summary>
        /// <param name="financialAidAwardPeriod"><see cref="Dtos.FinancialAidAwardPeriod">FinancialAidAwardPeriod</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.FinancialAidAwardPeriod">FinancialAidAwardPeriod</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/financial-aid-award-periods", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFinancialAidAwardPeriodsV7")]
        public async Task<ActionResult<Dtos.FinancialAidAwardPeriod>> PostFinancialAidAwardPeriodAsync([FromBody] Dtos.FinancialAidAwardPeriod financialAidAwardPeriod)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Financial Aid Award Period.
        /// </summary>
        /// <param name="id">Id of the Financial Aid Award Period to update</param>
        /// <param name="financialAidAwardPeriod"><see cref="Dtos.FinancialAidAwardPeriod">FinancialAidAwardPeriod</see> to create</param>
        /// <returns>Updated <see cref="Dtos.FinancialAidAwardPeriod">FinancialAidAwardPeriod</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/financial-aid-award-periods/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFinancialAidAwardPeriodsV7")]
        public async Task<ActionResult<Dtos.FinancialAidAwardPeriod>> PutFinancialAidAwardPeriodAsync([FromRoute] string id, [FromBody] Dtos.FinancialAidAwardPeriod financialAidAwardPeriod)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Financial Aid Award Period.
        /// </summary>
        /// <param name="id">ID of the Financial Aid Award Period to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/financial-aid-award-periods/{id}", Name = "DeleteFinancialAidAwardPeriods", Order = -10)]
        public async Task<IActionResult> DeleteFinancialAidAwardPeriodAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

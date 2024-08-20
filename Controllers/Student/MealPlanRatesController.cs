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
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to MealPlanRates
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class MealPlanRatesController : BaseCompressedApiController
    {
        private readonly IMealPlanRatesService _mealPlanRatesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MealPlanRatesController class.
        /// </summary>
        /// <param name="mealPlanRatesService">Service of type <see cref="IMealPlanRatesService">IMealPlanRatesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public MealPlanRatesController(IMealPlanRatesService mealPlanRatesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _mealPlanRatesService = mealPlanRatesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all mealPlanRates
        /// </summary>
        /// <returns>List of MealPlanRates <see cref="Dtos.MealPlanRates"/> objects representing matching mealPlanRates</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/meal-plan-rates", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetMealPlanRates", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.MealPlanRates>>> GetMealPlanRatesAsync()
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
                var rates = await _mealPlanRatesService.GetMealPlanRatesAsync(bypassCache);
                if (rates != null && rates.Any())
                {
                    AddEthosContextProperties(
                        await _mealPlanRatesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _mealPlanRatesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         rates.Select(i => i.Id).ToList()));
                }
                return Ok(rates);

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
        /// Read (GET) a mealPlanRates using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired mealPlanRates</param>
        /// <returns>A mealPlanRates object <see cref="Dtos.MealPlanRates"/> in EEDM format</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/meal-plan-rates/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetMealPlanRatesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.MealPlanRates>> GetMealPlanRatesByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                var rate = await _mealPlanRatesService.GetMealPlanRatesByGuidAsync(guid);  // TODO: Honor bypassCache

                if (rate != null)
                {

                    AddEthosContextProperties(await _mealPlanRatesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _mealPlanRatesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { rate.Id }));
                }
                return Ok(rate);
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
        /// Create (POST) a new mealPlanRates
        /// </summary>
        /// <param name="mealPlanRates">DTO of the new mealPlanRates</param>
        /// <returns>A mealPlanRates object <see cref="Dtos.MealPlanRates"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/meal-plan-rates", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostMealPlanRatesV10")]
        public async Task<ActionResult<Dtos.MealPlanRates>> PostMealPlanRatesAsync([FromBody] Dtos.MealPlanRates mealPlanRates)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing mealPlanRates
        /// </summary>
        /// <param name="guid">GUID of the mealPlanRates to update</param>
        /// <param name="mealPlanRates">DTO of the updated mealPlanRates</param>
        /// <returns>A mealPlanRates object <see cref="Dtos.MealPlanRates"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/meal-plan-rates/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutMealPlanRatesV10")]
        public async Task<ActionResult<Dtos.MealPlanRates>> PutMealPlanRatesAsync([FromRoute] string guid, [FromBody] Dtos.MealPlanRates mealPlanRates)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a mealPlanRates
        /// </summary>
        /// <param name="guid">GUID to desired mealPlanRates</param>
        [HttpDelete]
        [Route("/meal-plan-rates/{guid}", Name = "DefaultDeleteMealPlanRates", Order = -10)]
        public async Task<IActionResult> DeleteMealPlanRatesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

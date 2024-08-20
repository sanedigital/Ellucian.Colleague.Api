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
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to MealPlans
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class MealPlansController : BaseCompressedApiController
    {
        private readonly IMealPlansService _mealPlansService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MealPlansController class.
        /// </summary>
        /// <param name="mealPlansService">Service of type <see cref="IMealPlansService">IMealPlansService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public MealPlansController(IMealPlansService mealPlansService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _mealPlansService = mealPlansService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all mealPlans
        /// </summary>
        /// <returns>List of MealPlans <see cref="Dtos.MealPlans"/> objects representing matching mealPlans</returns>
        [HttpGet]       
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/meal-plans", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetMealPlans", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.MealPlans>>> GetMealPlansAsync()
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
                var plans = await _mealPlansService.GetMealPlansAsync(bypassCache);
                if (plans != null && plans.Any())
                {
                    AddEthosContextProperties(
                        await _mealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _mealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         plans.Select(i => i.Id).ToList()));
                }
                return Ok(plans);
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
        /// Read (GET) a mealPlans using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired mealPlans</param>
        /// <returns>A mealPlans object <see cref="Dtos.MealPlans"/> in EEDM format</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/meal-plans/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetMealPlansByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.MealPlans>> GetMealPlansByGuidAsync(string guid)
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

                var plan = await _mealPlansService.GetMealPlansByGuidAsync(guid);  // TODO: Honor bypassCache

                if (plan != null)
                {

                    AddEthosContextProperties(await _mealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _mealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { plan.Id }));
                }
                return plan;
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
        /// Create (POST) a new mealPlans
        /// </summary>
        /// <param name="mealPlans">DTO of the new mealPlans</param>
        /// <returns>A mealPlans object <see cref="Dtos.MealPlans"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/meal-plans", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostMealPlansV10")]
        public async Task<ActionResult<Dtos.MealPlans>> PostMealPlansAsync([FromBody] Dtos.MealPlans mealPlans)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing mealPlans
        /// </summary>
        /// <param name="guid">GUID of the mealPlans to update</param>
        /// <param name="mealPlans">DTO of the updated mealPlans</param>
        /// <returns>A mealPlans object <see cref="Dtos.MealPlans"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/meal-plans/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutMealPlansV10")]
        public async Task<ActionResult<Dtos.MealPlans>> PutMealPlansAsync([FromRoute] string guid, [FromBody] Dtos.MealPlans mealPlans)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a mealPlans
        /// </summary>
        /// <param name="guid">GUID to desired mealPlans</param>
        [HttpDelete]
        [Route("/meal-plans/{guid}", Name = "DefaultDeleteMealPlans", Order = -10)]
        public async Task<IActionResult> DeleteMealPlansAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

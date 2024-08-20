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
    /// Provides access to MealTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class MealTypesController : BaseCompressedApiController
    {
        private readonly IMealTypesService _mealTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MealTypesController class.
        /// </summary>
        /// <param name="mealTypesService">Service of type <see cref="IMealTypesService">IMealTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public MealTypesController(IMealTypesService mealTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _mealTypesService = mealTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all mealTypes
        /// </summary>
                /// <returns>List of MealTypes <see cref="Dtos.MealTypes"/> objects representing matching mealTypes</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/meal-types", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetMealTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.MealTypes>>> GetMealTypesAsync()
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
                var mealTypes = await _mealTypesService.GetMealTypesAsync(bypassCache);

                if (mealTypes != null && mealTypes.Any())
                {
                    AddEthosContextProperties(await _mealTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _mealTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              mealTypes.Select(a => a.Id).ToList()));
                }
                return Ok(mealTypes);
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
        /// Read (GET) a mealTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired mealTypes</param>
        /// <returns>A mealTypes object <see cref="Dtos.MealTypes"/> in EEDM format</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/meal-types/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetMealTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.MealTypes>> GetMealTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                   await _mealTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _mealTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));

                return await _mealTypesService.GetMealTypesByGuidAsync(guid);
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
        /// Create (POST) a new mealTypes
        /// </summary>
        /// <param name="mealTypes">DTO of the new mealTypes</param>
        /// <returns>A mealTypes object <see cref="Dtos.MealTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/meal-types", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostMealTypesV10")]
        public async Task<ActionResult<Dtos.MealTypes>> PostMealTypesAsync([FromBody] Dtos.MealTypes mealTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing mealTypes
        /// </summary>
        /// <param name="guid">GUID of the mealTypes to update</param>
        /// <param name="mealTypes">DTO of the updated mealTypes</param>
        /// <returns>A mealTypes object <see cref="Dtos.MealTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/meal-types/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutMealTypesV10")]
        public async Task<ActionResult<Dtos.MealTypes>> PutMealTypesAsync([FromRoute] string guid, [FromBody] Dtos.MealTypes mealTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a mealTypes
        /// </summary>
        /// <param name="guid">GUID to desired mealTypes</param>
        [HttpDelete]
        [Route("/meal-types/{guid}", Name = "DefaultDeleteMealTypes", Order = -10)]
        public async Task<IActionResult> DeleteMealTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
using Ellucian.Web.Http.ModelBinding;

using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to MealPlanRequests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class MealPlanRequestsController : BaseCompressedApiController
    {
        private readonly IMealPlanRequestsService _mealPlanRequestsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MealPlanRequestsController class.
        /// </summary>
        /// <param name="mealPlanRequestsService">Service of type <see cref="IMealPlanRequestsService">IMealPlanRequestsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public MealPlanRequestsController(IMealPlanRequestsService mealPlanRequestsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _mealPlanRequestsService = mealPlanRequestsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all mealPlanRequests
        /// </summary>
        /// <returns>List of MealPlanRequests <see cref="Dtos.MealPlanRequests"/> objects representing matching mealPlanRequests</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanRequest, StudentPermissionCodes.CreateMealPlanRequest })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/meal-plan-requests", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetMealPlanRequests", IsEedmSupported = true)]
        public async Task<IActionResult> GetMealPlanRequestsAsync(Paging page)
        {      
            try
            {
                var bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                _mealPlanRequestsService.ValidatePermissions(GetPermissionsMetaData());

                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems =  await _mealPlanRequestsService.GetMealPlanRequestsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _mealPlanRequestsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _mealPlanRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.MealPlanRequests>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a mealPlanRequests using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired mealPlanRequests</param>
        /// <returns>A mealPlanRequests object <see cref="Dtos.MealPlanRequests"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanRequest, StudentPermissionCodes.CreateMealPlanRequest })]

        [HttpGet]
        [HeaderVersionRoute("/meal-plan-requests/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetMealPlanRequestsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.MealPlanRequests>> GetMealPlanRequestsByGuidAsync(string guid)
        {            
            try
            {
                _mealPlanRequestsService.ValidatePermissions(GetPermissionsMetaData());

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

                var mealPlans = await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(guid, bypassCache);

                if (mealPlans != null)
                {

                    AddEthosContextProperties(await _mealPlanRequestsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _mealPlanRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { mealPlans.Id }));
                }


                return mealPlans;
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Create (POST) a new mealPlanRequests
        /// </summary>
        /// <param name="mealPlanRequests">DTO of the new mealPlanRequests</param>
        /// <returns>A mealPlanRequests object <see cref="Dtos.MealPlanRequests"/> in EEDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateMealPlanRequest)]
        [HttpPost]
        [HeaderVersionRoute("/meal-plan-requests", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostMealPlanRequestsV10", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.MealPlanRequests>> PostMealPlanRequestsAsync([FromBody] Dtos.MealPlanRequests mealPlanRequests)
        {
            _mealPlanRequestsService.ValidatePermissions(GetPermissionsMetaData());

            if (mealPlanRequests == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null meal plan request argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(mealPlanRequests.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request body.")));
            }
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _mealPlanRequestsService.ImportExtendedEthosData(await ExtractExtendedData(await _mealPlanRequestsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the meal plan request
                var mealPlanReturn = await _mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequests);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _mealPlanRequestsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _mealPlanRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { mealPlanReturn.Id }));

                return mealPlanReturn;
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
        /// Update (PUT) an existing mealPlanRequests
        /// </summary>
        /// <param name="guid">GUID of the mealPlanRequests to update</param>
        /// <param name="mealPlanRequests">DTO of the updated mealPlanRequests</param>
        /// <returns>A mealPlanRequests object <see cref="Dtos.MealPlanRequests"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateMealPlanRequest)]
        [HttpPut]
        [HeaderVersionRoute("/meal-plan-requests/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutMealPlanRequestsV10", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.MealPlanRequests>> PutMealPlanRequestsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.MealPlanRequests mealPlanRequests)
        {
            _mealPlanRequestsService.ValidatePermissions(GetPermissionsMetaData());

            if (mealPlanRequests == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null meal plan request argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (!guid.Equals(mealPlanRequests.Id, StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("Id not the same as in request body.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            try
            {
                //get Data Privacy List
                var dpList = await _mealPlanRequestsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _mealPlanRequestsService.ImportExtendedEthosData(await ExtractExtendedData(await _mealPlanRequestsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var mealPlanRequestReturn = await _mealPlanRequestsService.PutMealPlanRequestsAsync(guid,
                    await PerformPartialPayloadMerge(mealPlanRequests, async () => await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _mealPlanRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return mealPlanRequestReturn;  

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
        /// Delete (DELETE) a mealPlanRequests
        /// </summary>
        /// <param name="guid">GUID to desired mealPlanRequests</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/meal-plan-requests/{guid}", Name = "DefaultDeleteMealPlanRequests", Order = -10)]
        public async Task<IActionResult> DeleteMealPlanRequestsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

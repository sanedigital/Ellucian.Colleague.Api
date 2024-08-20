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
    /// Provides access to StudentMealPlans
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class StudentMealPlansController : BaseCompressedApiController
    {
        private readonly IStudentMealPlansService _studentMealPlansService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentMealPlansController class.
        /// </summary>
        /// <param name="studentMealPlansService">Service of type <see cref="IStudentMealPlansService">IStudentMealPlansService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentMealPlansController(IStudentMealPlansService studentMealPlansService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentMealPlansService = studentMealPlansService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentMealPlans
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// /// <param name="criteria">mealplan  search criteria in JSON format</param>
        /// <returns>List of StudentMealPlans <see cref="Dtos.StudentMealPlans"/> objects representing matching studentMealPlans</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentMealPlans))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/meal-plan-assignments", "10.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentMealPlansV1010", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentMealPlansAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            var criteriaFilter = GetFilterObject<Dtos.StudentMealPlans>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.StudentMealPlans>>(new List<Dtos.StudentMealPlans>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _studentMealPlansService.GetStudentMealPlansAsync(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentMealPlans>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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

        #region 16.0.0

        /// <summary>
        /// Return all studentMealPlans
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// /// <param name="criteria">mealplan  search criteria in JSON format</param>
        /// <returns>List of StudentMealPlans <see cref="Dtos.StudentMealPlans2"/> objects representing matching studentMealPlans</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentMealPlans2))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/meal-plan-assignments", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentMealPlans", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentMealPlans2Async(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            var criteriaFilter = GetFilterObject<Dtos.StudentMealPlans2>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.StudentMealPlans2>>(new List<Dtos.StudentMealPlans2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _studentMealPlansService.GetStudentMealPlans2Async(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentMealPlans2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a studentMealPlans using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans2"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment })]
        [HttpGet]
        [HeaderVersionRoute("/meal-plan-assignments/{guid}", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentMealPlansByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentMealPlans2>> GetStudentMealPlansByGuid2Async(string guid)
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
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                var mealPlans = await _studentMealPlansService.GetStudentMealPlansByGuid2Async(guid);

                if (mealPlans != null)
                {

                    AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
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
        /// Update (PUT) an existing studentMealPlans
        /// </summary>
        /// <param name="guid">GUID of the studentMealPlans to update</param>
        /// <param name="studentMealPlans2">DTO of the updated studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans2"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateMealPlanAssignment)]
        [HttpPut]
        [HeaderVersionRoute("/meal-plan-assignments/{guid}", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentMealPlansV16", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentMealPlans2>> PutStudentMealPlans2Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentMealPlans2 studentMealPlans2)
        {

            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentMealPlans2 == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null  studentMealPlans argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(studentMealPlans2.Id))
            {
                studentMealPlans2.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(studentMealPlans2.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != studentMealPlans2.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _studentMealPlansService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _studentMealPlansService.ImportExtendedEthosData(await ExtractExtendedData(await _studentMealPlansService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                Dtos.StudentMealPlans2 existingMealPlan = null;
                try
                {
                    existingMealPlan = await _studentMealPlansService.GetStudentMealPlansByGuid2Async(guid);
                    if (studentMealPlans2.Consumption != null)
                    {
                        existingMealPlan.Consumption = studentMealPlans2.Consumption;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception occurred while reading the Student Meal Plan.");
                }

                //do update with partial logic
                var studentMealPlanReturn = await _studentMealPlansService.PutStudentMealPlans2Async(guid,
                    await PerformPartialPayloadMerge(studentMealPlans2, existingMealPlan,
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return studentMealPlanReturn;

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
        /// Create (POST) a new studentMealPlans
        /// </summary>
        /// <param name="studentMealPlans2">DTO of the new studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans2"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateMealPlanAssignment)]
        [HttpPost]
        [HeaderVersionRoute("/meal-plan-assignments", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentMealPlansV16", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentMealPlans2>> PostStudentMealPlans2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentMealPlans2 studentMealPlans2)
        {
            if (studentMealPlans2 == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null StudentMealPlans argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _studentMealPlansService.ImportExtendedEthosData(await ExtractExtendedData(await _studentMealPlansService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the student meal plan
                var mealPlanReturn = await _studentMealPlansService.PostStudentMealPlans2Async(studentMealPlans2);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { mealPlanReturn.Id }));

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

        #endregion

        /// <summary>
        /// Read (GET) a studentMealPlans using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment })]
        [HeaderVersionRoute("/meal-plan-assignments/{guid}", "10.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentMealPlansByGuidV1010", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentMealPlans>> GetStudentMealPlansByGuidAsync(string guid)
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
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                var mealPlans = await _studentMealPlansService.GetStudentMealPlansByGuidAsync(guid);

                if (mealPlans != null)
                {

                    AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
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
        /// Create (POST) a new studentMealPlans
        /// </summary>
        /// <param name="studentMealPlans">DTO of the new studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans2"/> in EEDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateMealPlanAssignment)]
        [HeaderVersionRoute("/meal-plan-assignments", "10.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentMealPlansV1010", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentMealPlans>> PostStudentMealPlansAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentMealPlans studentMealPlans)
        {
            if (studentMealPlans == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null StudentMealPlans argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _studentMealPlansService.ImportExtendedEthosData(await ExtractExtendedData(await _studentMealPlansService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the student meal plan
                var mealPlanReturn = await _studentMealPlansService.PostStudentMealPlansAsync(studentMealPlans);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { mealPlanReturn.Id }));

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
        /// Update (PUT) an existing studentMealPlans
        /// </summary>
        /// <param name="guid">GUID of the studentMealPlans to update</param>
        /// <param name="studentMealPlans">DTO of the updated studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateMealPlanAssignment)]
        [HeaderVersionRoute("/meal-plan-assignments/{guid}", "10.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentMealPlansV1010", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentMealPlans>> PutStudentMealPlansAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentMealPlans studentMealPlans)
        {

            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentMealPlans == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null  studentMealPlans argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(studentMealPlans.Id))
            {
                studentMealPlans.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(studentMealPlans.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != studentMealPlans.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _studentMealPlansService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _studentMealPlansService.ImportExtendedEthosData(await ExtractExtendedData(await _studentMealPlansService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                Dtos.StudentMealPlans existingMealPlan = null;
                try
                {
                    existingMealPlan = await _studentMealPlansService.GetStudentMealPlansByGuidAsync(guid);
                    if (studentMealPlans.Consumption != null)
                    {
                        existingMealPlan.Consumption = studentMealPlans.Consumption;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception occurred while reading the Student Meal Plan.");
                }

                //do update with partial logic
                var studentMealPlanReturn = await _studentMealPlansService.PutStudentMealPlansAsync(guid,
                    await PerformPartialPayloadMerge(studentMealPlans, existingMealPlan,
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return studentMealPlanReturn;

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
        /// Delete (DELETE) a studentMealPlans
        /// </summary>
        /// <param name="guid">GUID to desired studentMealPlans</param>
        [HttpDelete]
        [Route("/meal-plan-assignments/{guid}", Name = "DefaultDeleteStudentMealPlans", Order = -10)]
        public async Task<IActionResult> DeleteStudentMealPlansAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// The controller for student financial aid awards for the Ellucian Data Model.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Authorize]
    public class StudentFinancialAidAwardsController : BaseCompressedApiController
    {
        private readonly IStudentFinancialAidAwardService studentFinancialAidAwardService;
        private readonly ILogger _logger;
        private readonly IAdapterRegistry AdapterRegistry;

        /// <summary>
        /// This constructor initializes the StudentFinancialAidAwardController object
        /// </summary>
        /// <param name="studentFinancialAidAwardService">student financial aid awards service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        /// <param name="adapterRegistry"></param>
        public StudentFinancialAidAwardsController(IStudentFinancialAidAwardService studentFinancialAidAwardService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings, IAdapterRegistry adapterRegistry) : base(actionContextAccessor, apiSettings)
        {
            this.studentFinancialAidAwardService = studentFinancialAidAwardService;
            this._logger = logger;
            AdapterRegistry = adapterRegistry;
        }

        /// <summary>
        /// Retrieves a specified student financial aid award for the data model version 7
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the non-restricted version using student-financial-aid-awards.
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <returns>A StudentFinancialAidAward DTO</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [HeaderVersionRoute("/student-financial-aid-awards/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentFinancialAidAwardById7", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentFinancialAidAward>> GetByIdAsync([FromRoute] string id)
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
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await studentFinancialAidAwardService.GetByIdAsync(id, false);
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
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error getting student financial aid award");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a specified student financial aid award for the data model version 11.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the non-restricted version using student-financial-aid-awards.
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <returns>A StudentFinancialAidAward DTO</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/student-financial-aid-awards/{id}", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentFinancialAidAwardByIdDefault", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentFinancialAidAward2>> GetById2Async([FromRoute] string id)
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
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }
                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await studentFinancialAidAwardService.GetById2Async(id, false, bypassCache);
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
                _logger.LogError(e, "Unknown error getting student financial aid award");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }              

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 7
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the non-restricted version using student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/student-financial-aid-awards", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllStudentFinancialAidAwards7", IsEedmSupported = true)]
        public async Task<IActionResult> GetAsync(Paging page)
        {
            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                
                var pageOfItems = await studentFinancialAidAwardService.GetAsync(page.Offset, page.Limit, bypassCache, false);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAward>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error getting student financial aid award");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 11.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the non-restricted version using student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentFinancialAidAward2))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/student-financial-aid-awards", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllStudentFinancialAidAwards11", IsEedmSupported = true, Order = -5)]
        public async Task<IActionResult> Get2Async(Paging page, QueryStringFilter criteria)
        {
            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.StudentFinancialAidAward2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(new List<Dtos.StudentFinancialAidAward2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await studentFinancialAidAwardService.Get2Async(page.Offset, page.Limit, criteriaObj, string.Empty, bypassCache, false);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error getting student financial aid award");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 11.1.0.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the non-restricted version using student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentFinancialAidAward2))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/student-financial-aid-awards", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllStudentFinancialAidAwardsDefault", IsEedmSupported = true)]
        public async Task<IActionResult> Get3Async(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
        {
            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(new List<Dtos.StudentFinancialAidAward2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.StudentFinancialAidAward2>(_logger, "criteria");

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }


                var pageOfItems = await studentFinancialAidAwardService.Get2Async(page.Offset, page.Limit, criteriaObj, personFilterValue, bypassCache, false);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
                _logger.LogError(e, "Unknown error getting student financial aid award");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a specified student financial aid award for the data model version 7
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the restricted version using restricted-student-financial-aid-awards.
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <returns>A StudentFinancialAidAward DTO</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [HeaderVersionRoute("/restricted-student-financial-aid-awards/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRestrictedStudentFinancialAidAwardById7", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentFinancialAidAward>> GetRestrictedByIdAsync([FromRoute] string id)
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
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }
                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await studentFinancialAidAwardService.GetByIdAsync(id, true);
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
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error getting student financial aid award");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a specified student financial aid award for the data model version 11.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the restricted version using restricted-student-financial-aid-awards.
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <returns>A StudentFinancialAidAward DTO</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/restricted-student-financial-aid-awards/{id}", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRestrictedStudentFinancialAidAwardByIdDefault", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentFinancialAidAward2>> GetRestrictedById2Async([FromRoute] string id)
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
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }
                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return await studentFinancialAidAwardService.GetById2Async(id, true, bypassCache);
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
                _logger.LogError(e, "Unknown error getting student financial aid award");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }        

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 7
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the restricted version using restricted-student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/restricted-student-financial-aid-awards", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRestrictedAllStudentFinancialAidAwards7", IsEedmSupported = true)]
        public async Task<IActionResult> GetRestrictedAsync(Paging page)
        {
            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                var pageOfItems = await studentFinancialAidAwardService.GetAsync(page.Offset, page.Limit, bypassCache, true);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAward>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error getting student financial aid award");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 11.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the restricted version using restricted-student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentFinancialAidAward2))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/restricted-student-financial-aid-awards", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRestrictedAllStudentFinancialAidAwards11", IsEedmSupported = true, Order = -5)]
        public async Task<IActionResult> GetRestricted2Async(Paging page, QueryStringFilter criteria)
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
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.StudentFinancialAidAward2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(new List<Dtos.StudentFinancialAidAward2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await studentFinancialAidAwardService.Get2Async(page.Offset, page.Limit, criteriaObj, string.Empty, bypassCache, true);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
                _logger.LogError(e, "Unknown error getting student financial aid award");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 11.1.0.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the restricted version using restricted-student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentFinancialAidAward2))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/restricted-student-financial-aid-awards", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetRestrictedAllStudentFinancialAidAwardsDefault", IsEedmSupported = true)]
        public async Task<IActionResult> GetRestricted3Async(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
        {
            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(new List<Dtos.StudentFinancialAidAward2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.StudentFinancialAidAward2>(_logger, "criteria");

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }


                var pageOfItems = await studentFinancialAidAwardService.Get2Async(page.Offset, page.Limit, criteriaObj, personFilterValue, bypassCache, true);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
                _logger.LogError(e, "Unknown error getting student financial aid award");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
        
        /// <summary>
        /// Update a single student financial aid award for the data model version 7 or 11
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <param name="studentFinancialAidAwardDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentFinancialAidAward</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-financial-aid-awards/{id}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentFinancialAidAward1110")]
        [HeaderVersionRoute("/student-financial-aid-awards/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentFinancialAidAward7")]
        [HeaderVersionRoute("/restricted-student-financial-aid-awards/{id}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRestrictedStudentFinancialAidAward1110")]
        [HeaderVersionRoute("/restricted-student-financial-aid-awards/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRestrictedStudentFinancialAidAward7")]
        public async Task<ActionResult<Dtos.StudentFinancialAidAward>> UpdateAsync([FromRoute] string id, [FromBody] Dtos.StudentFinancialAidAward2 studentFinancialAidAwardDto)
        {
            //PUT is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Create a single student financial aid award for the data model version 7 or 11
        /// </summary>
        /// <param name="studentFinancialAidAwardDto">Student Financial Aid Award DTO from Body of request</param>
        /// <returns>A single StudentFinancialAidAward</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-financial-aid-awards", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentFinancialAidAward1110")]
        [HeaderVersionRoute("/student-financial-aid-awards", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentFinancialAidAward7")]
        [HeaderVersionRoute("/restricted-student-financial-aid-awards", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRestrictedStudentFinancialAidAward1110")]
        [HeaderVersionRoute("/restricted-student-financial-aid-awards", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRestrictedStudentFinancialAidAward7")]
        public async Task<ActionResult<Dtos.StudentFinancialAidAward>> CreateAsync([FromBody] Dtos.StudentFinancialAidAward2 studentFinancialAidAwardDto)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete a single student financial aid award for the data model version 6
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/student-financial-aid-awards/{id}", Name = "DeleteStudentFinancialAidAward", Order = -10)]
        [Route("/restricted-student-financial-aid-awards/{id}", Name = "DeleteRestrictedStudentFinancialAidAward", Order = -10)]
        public async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Retrieves a specified student financial aid award for the data model version 11.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the non-restricted version using student-financial-aid-awards.
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <returns>A StudentFinancialAidAward DTO</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/student-financial-aid-awards/ant-fa/{id}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentFinancialAidAwardsAntFa", IsEedmSupported = true, IsEthosEnabled = true)]
        [HeaderVersionRoute("/student-financial-aid-awards-ant-fa/{id}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetStudentFinancialAidAwardsAntFa", IsEthosEnabled = true)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<IEnumerable<Dtos.Student.AnticipatedFa>>> GetAnticipatedFAByIdAsync([FromRoute] string id)
        {
            var antFaObjectsEntities = await studentFinancialAidAwardService.GetAntFaAsync(id);

            var dtoConverter = AdapterRegistry.GetAdapter<Domain.Student.Entities.AnticipatedFa, Dtos.Student.AnticipatedFa>();

            return Ok(antFaObjectsEntities.Select(antFaObjects =>
                   dtoConverter.MapToType(antFaObjects)));
        }
    }
}

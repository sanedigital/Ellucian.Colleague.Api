// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
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
    /// Exposes Employee data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmployeesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IEmployeeService employeeService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="employeeService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmployeesController(ILogger logger, IEmployeeService employeeService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.employeeService = employeeService;
        }

        /// <summary>
        /// Get a single employee using a guid.
        /// </summary>
        /// <param name="id">Guid of the employee to retrieve</param>
        /// <returns>Returns a single Employee object. <see cref="Dtos.Employee"/></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewEmployeeData, HumanResourcesPermissionCodes.UpdateEmployee })]
        [HttpGet]
        [HeaderVersionRoute("/employees/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEmployeeById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Employee>> GetEmployeeByIdAsync([FromRoute] string id)
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
                employeeService.ValidatePermissions(GetPermissionsMetaData());
                var employee = await employeeService.GetEmployeeByGuidAsync(id);

                if (employee != null)
                {

                    AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { employee.Id }));
                }


                return employee;

            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting employee");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a single employee using a guid.
        /// </summary>
        /// <param name="id">Guid of the employee to retrieve</param>
        /// <returns>Returns a single Employee object. <see cref="Dtos.Employee2"/></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewEmployeeData, HumanResourcesPermissionCodes.UpdateEmployee })]
        [HttpGet]
        [HeaderVersionRoute("/employees/{id}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEmployeeV11ById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Employee2>> GetEmployee2ByIdAsync([FromRoute] string id)
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
                employeeService.ValidatePermissions(GetPermissionsMetaData());
                var employee = await employeeService.GetEmployee2ByIdAsync(id);

                if (employee != null)
                {

                    AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { employee.Id }));
                }


                return employee;

            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting employee");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a single employee using a guid.
        /// </summary>
        /// <param name="id">Guid of the employee to retrieve</param>
        /// <returns>Returns a single Employee object. <see cref="Dtos.Employee2"/></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewEmployeeData, HumanResourcesPermissionCodes.UpdateEmployee })]
        [HttpGet]
        [HeaderVersionRoute("/employees/{id}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEmployeeByIdDefault", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Employee2>> GetEmployee3ByIdAsync([FromRoute] string id)
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
                employeeService.ValidatePermissions(GetPermissionsMetaData());
                var employee = await employeeService.GetEmployee3ByIdAsync(id);

                if (employee != null)
                {

                    AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { employee.Id }));
                }


                return employee;

            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting employee");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all employees using paging and including filters if necessary.
        /// </summary>
        /// <param name="page">Paging offset and limit.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatusEligibility">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <param name="rehireableStatusType">Rehireable code filter.</param>
        /// <returns>Returns a list of Employee objects using paging.  <see cref="Dtos.Employee"/></returns>
        [HttpGet, PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewEmployeeData, HumanResourcesPermissionCodes.UpdateEmployee })]
        [ValidateQueryStringFilter(new string[] { "person", "campus", "status", "startOn", "endOn", "rehireableStatusEligibility", "rehireableStatusType" }, false, true)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/employees", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllEmployees", IsEedmSupported = true)]
        public async Task<IActionResult> GetEmployeesAsync(Paging page,
            [FromQuery] string person = "", [FromQuery] string campus = "", [FromQuery] string status = "",
            [FromQuery] string startOn = "", [FromQuery] string endOn = "", [FromQuery] string rehireableStatusEligibility = "", [FromQuery] string rehireableStatusType = "")
        {
            string criteria = string.Concat(person, campus, status, startOn, endOn, rehireableStatusEligibility,rehireableStatusType);

            //valid query parameter but empty argument
            if ((!string.IsNullOrEmpty(criteria)) && (string.IsNullOrEmpty(criteria.Replace("\"", ""))))
            {
                return new PagedActionResult<IEnumerable<Dtos.Employee>>(new List<Dtos.Employee>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            if (person == null || campus == null || status == null || startOn == null || endOn == null
                || rehireableStatusEligibility == null || rehireableStatusType == null)
            {
                return new PagedActionResult<IEnumerable<Dtos.Employee>>(new List<Dtos.Employee>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

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
                page = new Paging(100, 0);
            }
            try
            {
                employeeService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await employeeService.GetEmployeesAsync(page.Offset, page.Limit, bypassCache, person, campus, status, startOn, endOn, rehireableStatusEligibility, rehireableStatusType);

                AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Employee>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting employee");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all employees for V11 using paging and including filters if necessary.
        /// </summary>
        /// <param name="page">Paging offset and limit.</param>
        /// <param name="criteria">Filter Criteria, includes person, campus, status, startOn, endOn, rehireableStatus.eligibility, and rehireableStatus.type.</param>
        /// <returns>Returns a list of Employee objects using paging.  <see cref="Dtos.Employee2"/></returns>
        [HttpGet, ValidateQueryStringFilter(), PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewEmployeeData, HumanResourcesPermissionCodes.UpdateEmployee })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Employee2)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/employees", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllEmployeesV11", IsEedmSupported = true)]
        public async Task<IActionResult> GetEmployees2Async(Paging page, QueryStringFilter criteria)
        {
            try
            {
                employeeService.ValidatePermissions(GetPermissionsMetaData());
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
                    page = new Paging(100, 0);
                }

                string person = string.Empty, campus = string.Empty, status = string.Empty, startOn = string.Empty,
                    endOn = string.Empty, rehireableStatusEligibility = string.Empty, rehireableStatusType = string.Empty, contractType = string.Empty, contractDetail = string.Empty;
                var rawFilterData = GetFilterObject<Dtos.Employee2>(logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Employee2>>(new List<Dtos.Employee2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                if (rawFilterData != null)
                {
                    person = rawFilterData.Person != null ? rawFilterData.Person.Id : null;
                    campus = rawFilterData.Campus != null ? rawFilterData.Campus.Id : null;
                    status = rawFilterData.Status.ToString();
                    startOn = rawFilterData.StartOn.ToString();
                    endOn = rawFilterData.EndOn.ToString();
                    if (rawFilterData.RehireableStatus != null)
                    {
                        rehireableStatusEligibility = rawFilterData.RehireableStatus.Eligibility.ToString();
                        if (rawFilterData.RehireableStatus.Type != null)
                            rehireableStatusType = rawFilterData.RehireableStatus.Type.Id;
                    }
                    if (rawFilterData.Contract != null)
                    {
                        if (rawFilterData.Contract.Type != null)
                        {
                            contractType = rawFilterData.Contract.Type.ToString();
                        }
                        if (rawFilterData.Contract.Detail != null)
                        {
                            if (rawFilterData.Contract.Detail.Id != null)
                                contractDetail = rawFilterData.Contract.Detail.Id.ToString();
                        }
                    }
                }
                var pageOfItems = await employeeService.GetEmployees2Async(page.Offset, page.Limit, bypassCache, person, campus, status, startOn, endOn, rehireableStatusEligibility, rehireableStatusType
                    , contractType, contractDetail);

                AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Employee2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting employee");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all employees for V12 using paging and including filters if necessary.
        /// </summary>
        /// <param name="page">Paging offset and limit.</param>
        /// <param name="criteria">Filter Criteria, includes person, campus, status, startOn, endOn, rehireableStatus.eligibility, and rehireableStatus.type.</param>
        /// <returns>Returns a list of Employee objects using paging.  <see cref="Dtos.Employee2"/></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ValidateQueryStringFilter(), PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewEmployeeData, HumanResourcesPermissionCodes.UpdateEmployee })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Employee2)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/employees", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllEmployeesDefault", IsEedmSupported = true)]
        public async Task<IActionResult> GetEmployees3Async(Paging page,
            QueryStringFilter criteria)
        {
            string person = string.Empty, campus = string.Empty, status = string.Empty, startOn = string.Empty,
                    endOn = string.Empty, rehireableStatusEligibility = string.Empty, rehireableStatusType = string.Empty, contractType = string.Empty, contractDetail = string.Empty; 

            try
            {
                employeeService.ValidatePermissions(GetPermissionsMetaData());
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
                    page = new Paging(100, 0);
                }
              
                var rawFilterData = GetFilterObject<Dtos.Employee2>(logger, "criteria");
                 
                 if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Employee2>>(new List<Dtos.Employee2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                if (rawFilterData != null)
                {
                    person = rawFilterData.Person != null ? rawFilterData.Person.Id : null;
                    campus = rawFilterData.Campus != null ? rawFilterData.Campus.Id : null;
                    status = rawFilterData.Status.ToString();
                    startOn = rawFilterData.StartOn.ToString();
                    endOn = rawFilterData.EndOn.ToString();
                    if (rawFilterData.RehireableStatus != null)
                    {
                        rehireableStatusEligibility = rawFilterData.RehireableStatus.Eligibility.ToString();
                        if (rawFilterData.RehireableStatus.Type != null)
                            rehireableStatusType = rawFilterData.RehireableStatus.Type.Id;
                    }
                    if (rawFilterData.Contract != null)
                    {
                        if (rawFilterData.Contract.Type != null)
                        {
                            contractType = rawFilterData.Contract.Type.ToString();
                        }
                        if (rawFilterData.Contract.Detail != null)
                        {
                            if (rawFilterData.Contract.Detail.Id != null)
                            contractDetail = rawFilterData.Contract.Detail.Id.ToString();
                        }
                    }
                }
                var pageOfItems = await employeeService.GetEmployees3Async(page.Offset, page.Limit, bypassCache,
                    person, campus, status, startOn, endOn, rehireableStatusEligibility, rehireableStatusType, contractType, contractDetail);

                AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Employee2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting employee");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update an existing employee
        /// </summary>
        /// <param name="id">Employee GUID for update.</param>
        /// <param name="employeeDto">Employee DTO request for update</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        [HttpPut]
        [HeaderVersionRoute("/employees/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmployee")]
        [HeaderVersionRoute("/employees/{id}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmployeeV11")]
        public async Task<ActionResult<Dtos.Employee>> PutEmployeeAsync([FromRoute] string id, [FromBody] Dtos.Employee employeeDto)
        {
            //Put is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Create a new employee record
        /// </summary>
        /// <param name="employeeDto">Employee DTO request for update</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        [HttpPost]
        [HeaderVersionRoute("/employees", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEmployee")]
        [HeaderVersionRoute("/employees", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEmployeeV11")]
        public async Task<ActionResult<Dtos.Employee>> PostEmployeeAsync([FromBody] Dtos.Employee employeeDto)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Create a new employee record v12
        /// </summary>
        /// <param name="employeeDto">Employee DTO request for update</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(HumanResourcesPermissionCodes.UpdateEmployee)]
        [HttpPost]
        [HeaderVersionRoute("/employees", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEmployeeV12", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Employee2>> PostEmployee3Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Employee2 employeeDto)
        {
            if (employeeDto == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null employeeDto argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (!employeeDto.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }
            try
            {
                employeeService.ValidatePermissions(GetPermissionsMetaData());
                await employeeService.ImportExtendedEthosData(await ExtractExtendedData(await employeeService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));
                var employee = await employeeService.PostEmployee2Async(employeeDto);
                AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { employee.Id }));
                return employee;
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                if (e.Errors == null || e.Errors.Count() <= 0)
                {
                    return CreateHttpResponseException(e.Message);
                }
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update an existing employee v12
        /// </summary>
        /// <param name="id">Employee GUID for update.</param>
        /// <param name="employeeDto">Employee DTO request for update</param>
        /// <returns>A employeeDto object <see cref="Dtos.Employee2"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(HumanResourcesPermissionCodes.UpdateEmployee )]
        [HttpPut]
        [HeaderVersionRoute("/employees/{id}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmployeeV12", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Employee2>> PutEmployee3Async([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Employee2 employeeDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (employeeDto == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null employeeDto argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(employeeDto.Id))
            {
                employeeDto.Id = id.ToLowerInvariant();
            }
            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            else if ((string.Equals(id, Guid.Empty.ToString())) || (string.Equals(employeeDto.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (id.ToLowerInvariant() != employeeDto.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
            try
            {
                employeeService.ValidatePermissions(GetPermissionsMetaData());
                if (employeeDto.HomeOrganization != null && !string.IsNullOrEmpty(employeeDto.HomeOrganization.Id))
                {
                    throw new ArgumentNullException("The Home Organization Id is not allowed for a PUT or POST request. ", "employee.homeOrganization.id");
                }
                //get Data Privacy List

                var dpList = await employeeService.GetDataPrivacyListByApi(GetRouteResourceName(), true);
                
                await employeeService.ImportExtendedEthosData(await ExtractExtendedData(await employeeService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                //this is a section to check all those attributes that cannot be updated in a PUT request. 
                var origDto = new Dtos.Employee2();
                try
                {
                    origDto = await employeeService.GetEmployee3ByIdAsync(id);
                }
                catch (KeyNotFoundException)
                {
                    origDto = null;
                }

            var employee =  await employeeService.PutEmployee2Async(id,
                            await PerformPartialPayloadMerge(employeeDto,
                                    origDto,
                                    dpList,
                                    logger), origDto);
                AddEthosContextProperties(dpList,
              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { employee.Id }));
                return employee;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                if (e.Errors == null || e.Errors.Count() <= 0)
                {
                    return CreateHttpResponseException(e.Message);
                }
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Delete an existing employee
        /// </summary>
        /// <param name="id">Employee GUID for update.</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        [HttpDelete]
        [Route("/employees/{id}", Name = "DeleteEmployee")]
        public async Task<IActionResult> DeleteEmployeeAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Gets a list of employees matching the given criteria
        /// </summary>
        /// <param name="criteria">An object that specifies search criteria</param>
        /// <returns>The response value is a list of Person DTOs for the matching set of employees.</returns>
        /// <exception cref="HttpResponseException">Http Response Exception</exception>
        /// <accessComments>
        /// Users with the following permission codes can query employee names:
        /// VIEW.ALL.EARNINGS.STATEMENTS
        /// VIEW.EMPLOYEE.DATA
        /// APPROVE.REJECT.TIME.ENTRY
        /// VIEW.EMPLOYEE.W2
        /// VIEW.EMPLOYEE.1095C
        /// VIEW.ALL.TIME.HISTORY
        /// VIEW.ALL.TOTAL.COMPENSATION
        /// APPROVE.REJECT.LEAVE.REQUEST
        /// ADD.ALL.HR.PROXY
        /// </accessComments>
        /// <note>PersonBase is cached for 24 hours.</note>
        [HttpPost]
        [HeaderVersionRoute("/qapi/employees", 1, false, "application/vnd.ellucian-employee-name-search.v{0}+json", Name = "QueryEmployeeNames")]
        public async Task<ActionResult<IEnumerable<Dtos.Base.Person>>> QueryEmployeeNamesByPostAsync([FromBody] Dtos.Base.EmployeeNameQueryCriteria criteria)
        {
            try
            {
                return Ok(await employeeService.QueryEmployeeNamesByPostAsync(criteria));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("User doesn't have the permission to query the employee information.", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException("Unknown error occurred while querying the employee information.", HttpStatusCode.BadRequest);
            }
        }
    }
}

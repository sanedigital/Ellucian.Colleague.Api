// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmployeeLeavePlans
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmployeeLeavePlansController : BaseCompressedApiController
    {
        private readonly IEmployeeLeavePlansService employeeLeavePlansService;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string invalidPermissionsErrorMessage = "The current user does not have the permissions to perform the requested operation.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Initializes a new instance of the EmployeeLeavePlansController class.
        /// </summary>
        /// <param name="employeeLeavePlansService">Service of type <see cref="IEmployeeLeavePlansService">IEmployeeLeavePlansService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmployeeLeavePlansController(IEmployeeLeavePlansService employeeLeavePlansService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.employeeLeavePlansService = employeeLeavePlansService;
            this.logger = logger;
        }

        /// <summary>
        /// Return all employeeLeavePlans for EEDM
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of EmployeeLeavePlans <see cref="Dtos.EmployeeLeavePlans"/> objects representing matching employeeLeavePlans</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(HumanResourcesPermissionCodes.ViewEmployeeLeavePlans)]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 200 })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/employee-leave-plans", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEmployeeLeavePlans", IsEedmSupported = true)]
        public async Task<IActionResult> GetEmployeeLeavePlansAsync(Paging page)
        {
            logger.LogDebug("********* Start - Process to get employee leave plans - Start *********");
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
                employeeLeavePlansService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                logger.LogDebug("Calling GetEmployeeLeavePlansAsync service method");
                var pageOfItems = await employeeLeavePlansService.GetEmployeeLeavePlansAsync(page.Offset, page.Limit, bypassCache);
                logger.LogDebug("Calling GetEmployeeLeavePlansAsync service method completed");

                AddEthosContextProperties(
                    await employeeLeavePlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await employeeLeavePlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                logger.LogDebug("********* End - Process to get employee leave plans - End *********");
                return new PagedActionResult<IEnumerable<Dtos.EmployeeLeavePlans>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Returns all EmployeeLeavePlan objects that you have permission to access. As an employeee, you will have access to only your leave plans.
        /// As a supervisor (or the proxy of a supervisor), you will have access to the leave plans of your (or your proxy's) direct reports.
        /// As a leave approver, you have access to the leave plans of the employees whose leave requests you handle.
        /// This is used by Self Service.
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for passing the effective person id in a proxy scenario</param>
        /// <accessComments>
        /// 1. As an employee you have access to your own leave plans.
        /// 2. As a supervisor with the APPROVE.REJECT.TIME.ENTRY permission, you have access to your own leave plans and your supervisees' leave plans.
        /// 3. As the proxy of a supervisor, you have access to that supervisor's leave plans and that supervisor's supervisees' leave plans.
        /// 4. As an admin, you have access to anyone's leave plans.
        /// 5. As a leave approver with the APPROVE.REJECT.LEAVE.REQUEST permission, you have access to the leave plans of the employees whose leave requests you handle.
        /// </accessComments>
        /// <returns>A collection of EmployeeLeavePlan objects</returns>
        [HttpGet]
        [HeaderVersionRoute("/employee-leave-plans", 2, false, Name = "GetAllEmployeeLeavePlansV2")]
        public async Task<ActionResult<IEnumerable<EmployeeLeavePlan>>> GetEmployeeLeavePlansV2Async(string effectivePersonId = null)
        {
            try
            {
                logger.LogDebug("********* Start - Process to get employee leave plans - V2 - Start *********");
                var leavePlans = await employeeLeavePlansService.GetEmployeeLeavePlansV2Async(effectivePersonId: effectivePersonId, bypassCache: false);
                logger.LogDebug("********* End - Process to get employee leave plans - V2 - End *********");
                return Ok(leavePlans);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage);
            }
        }

        /// <summary>
        /// Returns all EmployeeLeavePlan objects that you have permission to access. As an employeee, you will have access to only your leave plans.
        /// As a supervisor (or the proxy of a supervisor), you will have access to the leave plans of your (or your proxy's) direct reports.
        /// As a leave approver, you have access to the leave plans of the employees whose leave requests you handle.
        /// This is used by Self Service.
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for passing the effective person id in a proxy scenario</param>
        /// <accessComments>
        /// 1. As an employee you have access to your own leave plans.
        /// 2. As the proxy of a supervisor, you have access to that supervisor's leave plans and that supervisor's supervisees' leave plans.
        /// 3. As an admin, you have access to anyone's leave plans.
        /// 4. As a leave approver with the APPROVE.REJECT.LEAVE.REQUEST permission, you have access to the leave plans of the employees whose leave requests you handle.
        /// </accessComments>
        /// <returns>A collection of EmployeeLeavePlan objects</returns>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/employee-leave-plans-exp", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllEmployeeLeavePlansV1.0.0", IsEthosEnabled = true)]
        [HeaderVersionRoute("/employee-leave-plans", 3, false, Name = "GetAllEmployeeLeavePlansV3")]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets a list of EmployeeLeavePlan objects for the currently authenticated API user.",
            HttpMethodDescription = "Gets a list of EmployeeLeavePlan objects for the currently authenticated API user.")]
        public async Task<ActionResult<IEnumerable<EmployeeLeavePlan>>> GetEmployeeLeavePlansV3Async(string effectivePersonId = null)
        {
            try
            {
                logger.LogDebug("********* Start - Process to get employee leave plans - V3 - Start *********");
                var leavePlans = await employeeLeavePlansService.GetEmployeeLeavePlansV3Async(effectivePersonId: effectivePersonId, bypassCache: false);
                logger.LogDebug("********* End - Process to get employee leave plans - V3 - End *********");
                return Ok(leavePlans);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage);
            }
        }

        /// <summary>
        /// Returns EmployeeLeavePlan objects based on criteria provided.
        /// 
        /// The endpoint will not return the requested EmployeeTimeSummary if:
        ///     1.  400 - criteria was not provided
        ///     2.  403 - criteria contains Ids that do not have permission to get requested EmployeeLeavePlan
        ///     3.  404 - EmployeeLeavePlan resources requested do not exist
        /// </summary>
        /// <param name="criteria">Criteria used to select EmployeeLeavePlan objects <see cref="EmployeeLeavePlanQueryCriteria">.</see></param>
        /// <returns>A collection of <see cref="EmployeeLeavePlan"> objects.</see></returns>
        /// <accessComments>
        /// When a supervisor Id is provided as part of the criteria, the authenticated user must have supervisory permissions
        /// or be a proxy for supervisor. If no supervisor Id is provided, only EmployeeLeavePlan objects for the authenticated user
        /// may be requested.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/employee-leave-plans", 1, true, Name = "QueryEmployeeLeavePlanAsync")]
        public async Task<ActionResult<IEnumerable<EmployeeLeavePlan>>> QueryEmployeeLeavePlanAsync([FromBody] EmployeeLeavePlanQueryCriteria criteria)
        {
            logger.LogDebug("********* Start - Process to Query employee leave plan - Start *********");
            if (criteria == null)
            {
                var message = string.Format("criteria is required for QueryEmployeeLeavePlanAsync.");
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(criteria.SupervisorId) && criteria.SuperviseeIds == null)
            {
                var message = string.Format("Criteria must include a supervisor Id or supervisee Id(s)");
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var leaveplans = await employeeLeavePlansService.QueryEmployeeLeavePlanAsync(criteria);
                logger.LogDebug("********* End - Process to Query employee leave plan - End *********");
                return Ok(leaveplans);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to QueryEmployeeLeavePlanAsync - {0}", pe.Message);
                logger.LogError(pe, message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a employeeLeavePlans using a GUID for EEDM
        /// </summary>
        /// <param name="guid">GUID to desired employeeLeavePlans</param>
        /// <returns>A employeeLeavePlans object <see cref="Dtos.EmployeeLeavePlans"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(HumanResourcesPermissionCodes.ViewEmployeeLeavePlans)]
        [HttpGet]
        [HeaderVersionRoute("/employee-leave-plans/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmployeeLeavePlansByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmployeeLeavePlans>> GetEmployeeLeavePlansByGuidAsync(string guid)
        {
            logger.LogDebug("********* Start - Process to Get employee leave plans by GUID - Start *********");
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
                employeeLeavePlansService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await employeeLeavePlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await employeeLeavePlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                logger.LogDebug(string.Format("Calling GetEmployeeLeavePlansByGuidAsync service method with guid {0}", guid));
                var leaveplans = await employeeLeavePlansService.GetEmployeeLeavePlansByGuidAsync(guid);

                logger.LogDebug("********* End - Process to Get employee leave plans by GUID - End *********");
                return leaveplans;
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
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new employeeLeavePlans for EEDM
        /// </summary>
        /// <param name="employeeLeavePlans">DTO of the new employeeLeavePlans</param>
        /// <returns>A employeeLeavePlans object <see cref="Dtos.EmployeeLeavePlans"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/employee-leave-plans", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEmployeeLeavePlansV11")]
        public async Task<ActionResult<Dtos.EmployeeLeavePlans>> PostEmployeeLeavePlansAsync([FromBody] Dtos.EmployeeLeavePlans employeeLeavePlans)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employeeLeavePlans for EEDM
        /// </summary>
        /// <param name="guid">GUID of the employeeLeavePlans to update</param>
        /// <param name="employeeLeavePlans">DTO of the updated employeeLeavePlans</param>
        /// <returns>A employeeLeavePlans object <see cref="Dtos.EmployeeLeavePlans"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/employee-leave-plans/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmployeeLeavePlansV11")]
        public async Task<ActionResult<Dtos.EmployeeLeavePlans>> PutEmployeeLeavePlansAsync([FromRoute] string guid, [FromBody] Dtos.EmployeeLeavePlans employeeLeavePlans)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employeeLeavePlans for EEDM
        /// </summary>
        /// <param name="guid">GUID to desired employeeLeavePlans</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/employee-leave-plans/{guid}", Name = "DefaultDeleteEmployeeLeavePlans")]
        public async Task<IActionResult> DeleteEmployeeLeavePlansAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

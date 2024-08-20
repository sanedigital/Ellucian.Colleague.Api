/* Copyright 2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes Organizational Chart data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    [Metadata(ApiDescription = "Provides access to organizational chart.", ApiDomain = "HR")]
    public class OrganizationalChartController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IOrganizationalChartService organizationalChartService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="organizationalChartService"></param>
        /// <param name="apiSettings"></param>
        /// <param name="actionContextAccessor"></param>
        public OrganizationalChartController(ILogger logger, IOrganizationalChartService organizationalChartService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) :
            base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.organizationalChartService = organizationalChartService;
        }

        /// <summary>
        /// Gets a list of employees for the organizational chart.
        /// </summary>
        /// <param name="rootEmployeeId">The employee id of the root employee to build the org chat off of</param>
        /// <returns>A list of <see cref="OrgChartEmployee"> objects.</see></returns>
        /// <accessComments>
        /// </accessComments>

        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/org-chart", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetOrganizationalChartAsyncV1.0.0", IsEthosEnabled = true)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Builds an Organizational Chart.",
            HttpMethodDescription = "Gets employee information to be used in building the organizational chart.")]
        //ProCode API for Experience
        public async Task<ActionResult<IEnumerable<OrgChartEmployee>>> GetOrganizationalChartAsync(string rootEmployeeId)
        {
            try
            {
                if (string.IsNullOrEmpty(rootEmployeeId)) throw new Exception("Please provide the Employee Id for the org-chart.");
                var result = await organizationalChartService.GetOrganizationalChartAsync(rootEmployeeId);
                return Ok(result);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to GetOrganizationalChartAsync - {0}", pe.Message);
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets a single employee for the org chart.
        /// </summary>
        /// <param name="rootEmployeeId">The employee id of the root employee to build the org chat off of</param>
        /// <returns>A single <see cref="OrgChartEmployee"> object.</see></returns>
        /// <accessComments>
        /// </accessComments>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/org-chart-employee", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetOrganizationalChartEmployeeAsyncV1.0.0", IsEthosEnabled = true)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Builds a single employee in a Organizational Chart",
            HttpMethodDescription = "Gets employee information to be used in building the organizational chart.")]
        //ProCode API for Experience
        public async Task<ActionResult<OrgChartEmployee>> GetOrganizationalChartEmployeeAsync(string rootEmployeeId)
        {
            try
            {
                if (string.IsNullOrEmpty(rootEmployeeId)) throw new Exception("Please provide the Employee Id for the org-chart.");
                return await organizationalChartService.GetOrganizationalChartEmployeeAsync(rootEmployeeId);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to GetOrganizationalChartAsync - {0}", pe.Message);
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets a list of employees matching the given search criteria.
        /// </summary>
        /// <param name="criteria">An object that specifies search criteria.</param>
        /// <returns>A list of <see cref="EmployeeSearchResult"> objects.</see></returns>
        /// <accessComments>
        /// Only the users with VIEW.ORG.CHART permission can query employee names.
        /// </accessComments>
        [HttpPost, PermissionsFilter(HumanResourcesPermissionCodes.ViewOrgChart)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/org-chart-employee-search", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "QueryEmployeesByPostAsyncV1.0.0", IsEthosEnabled = true)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets a list of employees matching the given search criteria.",
            HttpMethodDescription = "Gets a list of employees matching the given search criteria.", HttpMethodPermission = "VIEW.ORG.CHART")]
        //ProCode API for Experience
        public async Task<ActionResult<IEnumerable<EmployeeSearchResult>>> QueryEmployeesByPostAsync([ModelBinder(typeof(EedmModelBinder))] EmployeeNameQueryCriteria criteria)
        {
            try
            {
                if (criteria == null) throw new ArgumentNullException("criteria", "Search criteria cannot be null.");
                organizationalChartService.ValidatePermissions(GetPermissionsMetaData());
                return Ok(await organizationalChartService.QueryEmployeesByPostAsync(criteria));
            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                return CreateHttpResponseException(ane.Message, HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to access QueryEmployeesByPostAsync";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

    }
}
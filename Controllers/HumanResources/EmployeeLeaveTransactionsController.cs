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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmployeeLeaveTransactions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmployeeLeaveTransactionsController : BaseCompressedApiController
    {
        private readonly IEmployeeLeaveTransactionsService _employeeLeaveTransactionsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmployeeLeaveTransactionsController class.
        /// </summary>
        /// <param name="employeeLeaveTransactionsService">Service of type <see cref="IEmployeeLeaveTransactionsService">IEmployeeLeaveTransactionsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmployeeLeaveTransactionsController(IEmployeeLeaveTransactionsService employeeLeaveTransactionsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _employeeLeaveTransactionsService = employeeLeaveTransactionsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all employeeLeaveTransactions
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of EmployeeLeaveTransactions <see cref="Dtos.EmployeeLeaveTransactions"/> objects representing matching employeeLeaveTransactions</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(HumanResourcesPermissionCodes.ViewEmployeeLeaveTransactions)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/employee-leave-transactions", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEmployeeLeaveTransactions", IsEedmSupported = true)]
        public async Task<IActionResult> GetEmployeeLeaveTransactionsAsync(Paging page)
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
                _employeeLeaveTransactionsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                var pageOfItems = await _employeeLeaveTransactionsService.GetEmployeeLeaveTransactionsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _employeeLeaveTransactionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _employeeLeaveTransactionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.EmployeeLeaveTransactions>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a employeeLeaveTransactions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employeeLeaveTransactions</param>
        /// <returns>A employeeLeaveTransactions object <see cref="Dtos.EmployeeLeaveTransactions"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(HumanResourcesPermissionCodes.ViewEmployeeLeaveTransactions)]
        [HttpGet]
        [HeaderVersionRoute("/employee-leave-transactions/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmployeeLeaveTransactionsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmployeeLeaveTransactions>> GetEmployeeLeaveTransactionsByGuidAsync(string guid)
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
                _employeeLeaveTransactionsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _employeeLeaveTransactionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _employeeLeaveTransactionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _employeeLeaveTransactionsService.GetEmployeeLeaveTransactionsByGuidAsync(guid);
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
        /// Create (POST) a new employeeLeaveTransactions
        /// </summary>
        /// <param name="employeeLeaveTransactions">DTO of the new employeeLeaveTransactions</param>
        /// <returns>A employeeLeaveTransactions object <see cref="Dtos.EmployeeLeaveTransactions"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/employee-leave-transactions", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEmployeeLeaveTransactionsV11")]
        public async Task<ActionResult<Dtos.EmployeeLeaveTransactions>> PostEmployeeLeaveTransactionsAsync([FromBody] Dtos.EmployeeLeaveTransactions employeeLeaveTransactions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employeeLeaveTransactions
        /// </summary>
        /// <param name="guid">GUID of the employeeLeaveTransactions to update</param>
        /// <param name="employeeLeaveTransactions">DTO of the updated employeeLeaveTransactions</param>
        /// <returns>A employeeLeaveTransactions object <see cref="Dtos.EmployeeLeaveTransactions"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/employee-leave-transactions/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmployeeLeaveTransactionsV11")]
        public async Task<ActionResult<Dtos.EmployeeLeaveTransactions>> PutEmployeeLeaveTransactionsAsync([FromRoute] string guid, [FromBody] Dtos.EmployeeLeaveTransactions employeeLeaveTransactions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employeeLeaveTransactions
        /// </summary>
        /// <param name="guid">GUID to desired employeeLeaveTransactions</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/employee-leave-transactions/{guid}", Name = "DefaultDeleteEmployeeLeaveTransactions")]
        public async Task<IActionResult> DeleteEmployeeLeaveTransactionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

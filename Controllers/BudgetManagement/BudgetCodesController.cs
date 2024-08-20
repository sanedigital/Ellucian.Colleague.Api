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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Domain.BudgetManagement;

namespace Ellucian.Colleague.Api.Controllers.BudgetManagement
{
    /// <summary>
    /// Provides access to BudgetCodes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.BudgetManagement)]
    public class BudgetCodesController : BaseCompressedApiController
    {
        private readonly IBudgetCodesService _budgetCodesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BudgetCodesController class.
        /// </summary>
        /// <param name="budgetCodesService">Service of type <see cref="IBudgetCodesService">IBudgetCodesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BudgetCodesController(IBudgetCodesService budgetCodesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _budgetCodesService = budgetCodesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all budgetCodes
        /// </summary>
        /// <returns>List of BudgetCodes <see cref="Dtos.BudgetCodes"/> objects representing matching budgetCodes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BudgetManagementPermissionCodes.ViewBudgetCode })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/budget-codes", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetBudgetCodes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.BudgetCodes>>> GetBudgetCodesAsync()
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
                _budgetCodesService.ValidatePermissions(GetPermissionsMetaData());
                var items = await _budgetCodesService.GetBudgetCodesAsync(bypassCache);

                AddEthosContextProperties(
                 await _budgetCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _budgetCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     items.Select(i => i.Id).Distinct().ToList()));

                return Ok(items);

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
        /// Read (GET) a budgetCodes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired budgetCodes</param>
        /// <returns>A budgetCodes object <see cref="Dtos.BudgetCodes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BudgetManagementPermissionCodes.ViewBudgetCode })]
        [HttpGet]
        [HeaderVersionRoute("/budget-codes/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetBudgetCodesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.BudgetCodes>> GetBudgetCodesByGuidAsync(string guid)
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
                _budgetCodesService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _budgetCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _budgetCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _budgetCodesService.GetBudgetCodesByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new budgetCodes
        /// </summary>
        /// <param name="budgetCodes">DTO of the new budgetCodes</param>
        /// <returns>A budgetCodes object <see cref="Dtos.BudgetCodes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/budget-codes", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostBudgetCodesV12")]
        public async Task<ActionResult<Dtos.BudgetCodes>> PostBudgetCodesAsync([FromBody] Dtos.BudgetCodes budgetCodes)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing budgetCodes
        /// </summary>
        /// <param name="guid">GUID of the budgetCodes to update</param>
        /// <param name="budgetCodes">DTO of the updated budgetCodes</param>
        /// <returns>A budgetCodes object <see cref="Dtos.BudgetCodes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/budget-codes/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutBudgetCodesV12")]
        public async Task<ActionResult<Dtos.BudgetCodes>> PutBudgetCodesAsync([FromRoute] string guid, [FromBody] Dtos.BudgetCodes budgetCodes)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a budgetCodes
        /// </summary>
        /// <param name="guid">GUID to desired budgetCodes</param>
        [HttpDelete]
        [Route("/budget-codes/{guid}", Name = "DefaultDeleteBudgetCodes", Order = -10)]
        public async Task<IActionResult> DeleteBudgetCodesAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

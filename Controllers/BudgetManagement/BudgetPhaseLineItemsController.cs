// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Domain.BudgetManagement;

namespace Ellucian.Colleague.Api.Controllers.BudgetManagement
{
    /// <summary>
    /// Provides access to BudgetPhaseLineItems
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.BudgetManagement)]
    public class BudgetPhaseLineItemsController : BaseCompressedApiController
    {
        private readonly IBudgetPhaseLineItemsService _budgetPhaseLineItemsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BudgetPhaseLineItemsController class.
        /// </summary>
        /// <param name="budgetPhaseLineItemsService">Service of type <see cref="IBudgetPhaseLineItemsService">IBudgetPhaseLineItemsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BudgetPhaseLineItemsController(IBudgetPhaseLineItemsService budgetPhaseLineItemsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _budgetPhaseLineItemsService = budgetPhaseLineItemsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all budgetPhaseLineItems
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">The default named query implementation for filtering</param>
        /// <returns>List of BudgetPhaseLineItems <see cref="Dtos.BudgetPhaseLineItems"/> objects representing matching budgetPhaseLineItems</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter))]
        [PermissionsFilter(new string[] { BudgetManagementPermissionCodes.ViewBudgetPhaseLineItems })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.BudgetPhaseLineItems))]
        [HttpGet]
        [HeaderVersionRoute("/budget-phase-line-items", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetBudgetPhaseLineItems", IsEedmSupported = true)]
        public async Task<IActionResult> GetBudgetPhaseLineItemsAsync(Paging page, QueryStringFilter criteria = null)
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
                _budgetPhaseLineItemsService.ValidatePermissions(GetPermissionsMetaData());

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                List<string> accountingStringComponentValues = null;
                var budgetPhase = string.Empty;

                var criteriaObj = GetFilterObject<Dtos.BudgetPhaseLineItems>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.BudgetPhaseLineItems>>(new List<Dtos.BudgetPhaseLineItems>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);


                if (criteriaObj != null)
                {
                    if (criteriaObj.BudgetPhase != null && !string.IsNullOrEmpty(criteriaObj.BudgetPhase.Id))
                    {
                        budgetPhase = criteriaObj.BudgetPhase.Id;
                    }

                    if (criteriaObj.AccountingStringComponentValues != null && criteriaObj.AccountingStringComponentValues.Any())
                    {
                        accountingStringComponentValues = new List<string>();

                        criteriaObj.AccountingStringComponentValues.ForEach(i => 
                        {
                            accountingStringComponentValues.Add(i.Id);
                        });
                    }
                }
                    
               var pageOfItems = await _budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(page.Offset, page.Limit, budgetPhase, 
                                 accountingStringComponentValues, bypassCache);

                AddEthosContextProperties(
                  await _budgetPhaseLineItemsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _budgetPhaseLineItemsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.BudgetPhaseLineItems>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a budgetPhaseLineItems using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired budgetPhaseLineItems</param>
        /// <returns>A budgetPhaseLineItems object <see cref="Dtos.BudgetPhaseLineItems"/> in EEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter)),PermissionsFilter(new string[] { BudgetManagementPermissionCodes.ViewBudgetPhaseLineItems }) ]
        [HttpGet]
        [HeaderVersionRoute("/budget-phase-line-items/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetBudgetPhaseLineItemsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.BudgetPhaseLineItems>> GetBudgetPhaseLineItemsByGuidAsync(string guid)
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

                _budgetPhaseLineItemsService.ValidatePermissions(GetPermissionsMetaData());

                AddEthosContextProperties(
               await _budgetPhaseLineItemsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
               await _budgetPhaseLineItemsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                   new List<string>() { guid }));
                return await _budgetPhaseLineItemsService.GetBudgetPhaseLineItemsByGuidAsync(guid);
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
        /// Create (POST) a new budgetPhaseLineItems
        /// </summary>
        /// <param name="budgetPhaseLineItems">DTO of the new budgetPhaseLineItems</param>
        /// <returns>A budgetPhaseLineItems object <see cref="Dtos.BudgetPhaseLineItems"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/budget-phase-line-items", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostBudgetPhaseLineItemsV12")]
        public async Task<ActionResult<Dtos.BudgetPhaseLineItems>> PostBudgetPhaseLineItemsAsync([FromBody] Dtos.BudgetPhaseLineItems budgetPhaseLineItems)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing budgetPhaseLineItems
        /// </summary>
        /// <param name="guid">GUID of the budgetPhaseLineItems to update</param>
        /// <param name="budgetPhaseLineItems">DTO of the updated budgetPhaseLineItems</param>
        /// <returns>A budgetPhaseLineItems object <see cref="Dtos.BudgetPhaseLineItems"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/budget-phase-line-items/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutBudgetPhaseLineItemsV12")]
        public async Task<ActionResult<Dtos.BudgetPhaseLineItems>> PutBudgetPhaseLineItemsAsync([FromRoute] string guid, [FromBody] Dtos.BudgetPhaseLineItems budgetPhaseLineItems)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a budgetPhaseLineItems
        /// </summary>
        /// <param name="guid">GUID to desired budgetPhaseLineItems</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/budget-phase-line-items/{guid}", Name = "DefaultDeleteBudgetPhaseLineItems", Order = -10)]
        public async Task<IActionResult> DeleteBudgetPhaseLineItemsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

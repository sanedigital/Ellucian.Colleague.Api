// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.Filters;
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
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to LedgerActivities
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class LedgerActivitiesController : BaseCompressedApiController
    {
        private readonly ILedgerActivityService _ledgerActivityService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the LedgerActivitiesController class.
        /// </summary>
        /// <param name="ledgerActivityService">Service of type <see cref="ILedgerActivityService">ILedgerActivityService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public LedgerActivitiesController(ILedgerActivityService ledgerActivityService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _ledgerActivityService = ledgerActivityService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all ledgerActivities
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <param name="fiscalYear"></param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), ValidateQueryStringFilter(), PermissionsFilter(ColleagueFinancePermissionCodes.ViewLedgerActivities)]
        [QueryStringFilterFilter("criteria", typeof(LedgerActivityFilter)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("fiscalYear", typeof(FiscalYearFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HeaderVersionRoute("/ledger-activities", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetLedgerActivities", IsEedmSupported = true, IsBulkSupported = true)]
        public async Task<IActionResult> GetLedgerActivitiesAsync(Paging page, QueryStringFilter criteria, QueryStringFilter fiscalYear = null)
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
                _ledgerActivityService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                string fiscalYearId = string.Empty, fiscalPeriod = string.Empty, reportingSegment = string.Empty, transactionDate = string.Empty;
                var ledgerActivityFilter = GetFilterObject<LedgerActivityFilter>(_logger, "criteria");
                var fiscalYearObj = GetFilterObject<Dtos.Filters.FiscalYearFilter>(_logger, "fiscalYear");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.LedgerActivity>>(new List<Dtos.LedgerActivity>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                if (ledgerActivityFilter != null)
                {
                    if ((ledgerActivityFilter.FiscalYear != null) && !(string.IsNullOrEmpty(ledgerActivityFilter.FiscalYear.Id)))
                        fiscalYearId = ledgerActivityFilter.FiscalYear.Id;

                    // To prevent a breaking change, this version accepts the fiscalPeriod filter using eith "fiscalPeriod" or "period".  
                    // If both are provided, and are different values,  return an empty set.
                    if ((ledgerActivityFilter.FiscalPeriod != null) && !(string.IsNullOrEmpty(ledgerActivityFilter.FiscalPeriod.Id))
                        && (ledgerActivityFilter.Period != null) && !(string.IsNullOrEmpty(ledgerActivityFilter.Period.Id))
                        && ledgerActivityFilter.Period.Id != ledgerActivityFilter.FiscalPeriod.Id)
                    {
                        return new PagedActionResult<IEnumerable<Dtos.LedgerActivity>>(new List<Dtos.LedgerActivity>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }
                    if ((ledgerActivityFilter.FiscalPeriod != null) && !(string.IsNullOrEmpty(ledgerActivityFilter.FiscalPeriod.Id)))
                        fiscalPeriod = ledgerActivityFilter.FiscalPeriod.Id;
                    else if ((ledgerActivityFilter.Period != null) && !(string.IsNullOrEmpty(ledgerActivityFilter.Period.Id)))
                        fiscalPeriod = ledgerActivityFilter.Period.Id;

                    if (!string.IsNullOrEmpty(ledgerActivityFilter.ReportingSegment))
                        reportingSegment = ledgerActivityFilter.ReportingSegment;
                    if (ledgerActivityFilter.TransactionDate.HasValue)
                        transactionDate = ledgerActivityFilter.TransactionDate.ToString();
                }

                if (fiscalYearObj != null && fiscalYearObj.FiscalYear != null)
                {
                    // To prevent a breaking change, this version accepts the fiscalYear filter using eith a standard filter or a named query 
                    // If both are provided return an empty set.  This should never be hit since standard + named queries are not permitted.
                    if (!string.IsNullOrEmpty(fiscalYearId))
                    {
                        return new PagedActionResult<IEnumerable<Dtos.LedgerActivity>>(new List<Dtos.LedgerActivity>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                    }
                    fiscalYearId =
                        string.IsNullOrEmpty(fiscalYearObj.FiscalYear.Id) ?
                        null :
                        fiscalYearObj.FiscalYear.Id;
                }

                var pageOfItems = await _ledgerActivityService.GetLedgerActivitiesAsync(page.Offset, page.Limit, fiscalYearId, fiscalPeriod, reportingSegment, transactionDate, bypassCache);

                AddEthosContextProperties(
                  await _ledgerActivityService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _ledgerActivityService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.LedgerActivity>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
            catch (InvalidOperationException e)
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
        /// Read (GET) a ledgerActivities using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired ledgerActivities</param>
        /// <returns>A ledgerActivities object <see cref="Dtos.LedgerActivity"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.ViewLedgerActivities)]
        [HeaderVersionRoute("/ledger-activities/{guid}", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetLedgerActivitiesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.LedgerActivity>> GetLedgerActivitiesByGuidAsync(string guid)
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
                _ledgerActivityService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                  await _ledgerActivityService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _ledgerActivityService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _ledgerActivityService.GetLedgerActivityByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new ledgerActivities
        /// </summary>
        /// <param name="ledgerActivities">DTO of the new ledgerActivities</param>
        /// <returns>A ledgerActivities object <see cref="Dtos.LedgerActivity"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/ledger-activities", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostLedgerActivitiesV11_1_0")]
        public async Task<ActionResult<Dtos.LedgerActivity>> PostLedgerActivitiesAsync([FromBody] Dtos.LedgerActivity ledgerActivities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing ledgerActivities
        /// </summary>
        /// <param name="guid">GUID of the ledgerActivities to update</param>
        /// <param name="ledgerActivities">DTO of the updated ledgerActivities</param>
        /// <returns>A ledgerActivities object <see cref="Dtos.LedgerActivity"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/ledger-activities/{guid}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutLedgerActivitiesV11_1_0")]
        public async Task<ActionResult<Dtos.LedgerActivity>> PutLedgerActivitiesAsync([FromRoute] string guid, [FromBody] Dtos.LedgerActivity ledgerActivities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a ledgerActivities
        /// </summary>
        /// <param name="guid">GUID to desired ledgerActivities</param>
        [HttpDelete]
        [Route("/ledger-activities/{guid}", Name = "DefaultDeleteLedgerActivities", Order = -10)]
        public async Task<IActionResult> DeleteLedgerActivitiesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

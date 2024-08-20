// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json.Linq;
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
    /// Provides access to Grants
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class GrantsController : BaseCompressedApiController
    {
        private readonly IGrantsService _grantsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GrantsController class.
        /// </summary>
        /// <param name="grantsService">Service of type <see cref="IGrantsService">IGrantsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GrantsController(IGrantsService grantsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _grantsService = grantsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all grant.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <param name="fiscalYear"></param>
        /// <returns></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewGrants })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Grant))]
        [QueryStringFilterFilter("fiscalYear", typeof(Dtos.Filters.FiscalYearFilter))]
        [HttpGet]
        [HeaderVersionRoute("/grants", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetGrants", IsEedmSupported = true)]
        public async Task<IActionResult> GetGrantsAsync(Paging page, QueryStringFilter criteria = null, QueryStringFilter fiscalYear = null)
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
                _grantsService.ValidatePermissions(GetPermissionsMetaData());

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                string reportingSegment = string.Empty, fiscalYearId = string.Empty;

                var criteriaObj = GetFilterObject<Dtos.Grant>(_logger, "criteria");

                var fiscalYearObj = GetFilterObject<Dtos.Filters.FiscalYearFilter>(_logger, "fiscalYear");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.Grant>>(new List<Dtos.Grant>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                if (criteriaObj != null && ! string.IsNullOrEmpty(criteriaObj.ReportingSegment))
                {
                    reportingSegment = criteriaObj.ReportingSegment;
                }

                if (fiscalYearObj != null && fiscalYearObj.FiscalYear != null)
                {
                    fiscalYearId = 
                        string.IsNullOrEmpty(fiscalYearObj.FiscalYear.Id) ? 
                        null : 
                        fiscalYearObj.FiscalYear.Id;
                }

                var pageOfItems = await _grantsService.GetGrantsAsync(page.Offset, page.Limit, reportingSegment, fiscalYearId, bypassCache);

                AddEthosContextProperties(
                  await _grantsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _grantsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Grant>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a grant using a GUID.
        /// </summary>
        /// <param name="guid">GUID to desired grant</param>
        /// <returns>A grant object <see cref="Dtos.Grant"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewGrants })]
        [HttpGet]
        [HeaderVersionRoute("/grants/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetGrantsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Grant>> GetGrantsByGuidAsync(string guid)
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
                _grantsService.ValidatePermissions(GetPermissionsMetaData());

                AddEthosContextProperties(
                   await _grantsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _grantsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _grantsService.GetGrantsByGuidAsync(guid);
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
        /// Create (POST) a new grant.
        /// </summary>
        /// <param name="grant">DTO of the new grant</param>
        /// <returns>A grants object <see cref="Dtos.Grant"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/grants", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGrantsV11")]
        public async Task<ActionResult<Dtos.Grant>> PostGrantsAsync([FromBody] Dtos.Grant grant)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing grant.
        /// </summary>
        /// <param name="guid">GUID of the grants to update</param>
        /// <param name="grant">DTO of the updated grants</param>
        /// <returns>A grants object <see cref="Dtos.Grant"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/grants/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGrantsV11")]
        public async Task<ActionResult<Dtos.Grant>> PutGrantsAsync([FromRoute] string guid, [FromBody] Dtos.Grant grant)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a grant.
        /// </summary>
        /// <param name="guid">GUID to desired grants</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/grants/{guid}", Name = "DefaultDeleteGrants", Order = -10)]
        public async Task<IActionResult> DeleteGrantsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

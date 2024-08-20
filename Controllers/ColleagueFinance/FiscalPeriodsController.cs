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
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to FiscalPeriods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class FiscalPeriodsController : BaseCompressedApiController
    {
        private readonly IFiscalPeriodsService _fiscalPeriodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FiscalPeriodsController class.
        /// </summary>
        /// <param name="fiscalPeriodsService">Service of type <see cref="IFiscalPeriodsService">IFiscalPeriodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FiscalPeriodsController(IFiscalPeriodsService fiscalPeriodsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _fiscalPeriodsService = fiscalPeriodsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all fiscalPeriods
        /// </summary>
        /// <returns>List of FiscalPeriods <see cref="Dtos.FiscalPeriods"/> objects representing matching fiscalPeriods</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.FiscalPeriods)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/fiscal-periods", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFiscalPeriods", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.FiscalPeriods>>> GetFiscalPeriodsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            string fiscalYear = string.Empty;

            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var fiscalPeriodsFilter = GetFilterObject<Dtos.FiscalPeriods>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.FiscalPeriods>(new List<Dtos.FiscalPeriods>());

                if (fiscalPeriodsFilter != null)
                {
                    if ((fiscalPeriodsFilter.FiscalYear != null) && (!string.IsNullOrEmpty(fiscalPeriodsFilter.FiscalYear.Id)))
                    {
                        fiscalYear = fiscalPeriodsFilter.FiscalYear.Id;
                    }
                }
                var fiscalPeriods = await _fiscalPeriodsService.GetFiscalPeriodsAsync(bypassCache, fiscalYear);

                if (fiscalPeriods != null && fiscalPeriods.Any())
                {
                    AddEthosContextProperties(await _fiscalPeriodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _fiscalPeriodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              fiscalPeriods.Select(a => a.Id).ToList()));
                }
                return Ok(fiscalPeriods);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a fiscalPeriods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired fiscalPeriods</param>
        /// <returns>A fiscalPeriods object <see cref="Dtos.FiscalPeriods"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/fiscal-periods/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFiscalPeriodsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FiscalPeriods>> GetFiscalPeriodsByGuidAsync(string guid)
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
                // AddDataPrivacyContextProperty((await _fiscalPeriodsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                // return await _fiscalPeriodsService.GetFiscalPeriodsByGuidAsync(guid, bypassCache);
                AddEthosContextProperties(
                  await _fiscalPeriodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _fiscalPeriodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _fiscalPeriodsService.GetFiscalPeriodsByGuidAsync(guid, bypassCache);

            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Create (POST) a new fiscalPeriods
        /// </summary>
        /// <param name="fiscalPeriods">DTO of the new fiscalPeriods</param>
        /// <returns>A fiscalPeriods object <see cref="Dtos.FiscalPeriods"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/fiscal-periods", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFiscalPeriodsV11")]
        public async Task<ActionResult<Dtos.FiscalPeriods>> PostFiscalPeriodsAsync([FromBody] Dtos.FiscalPeriods fiscalPeriods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing fiscalPeriods
        /// </summary>
        /// <param name="guid">GUID of the fiscalPeriods to update</param>
        /// <param name="fiscalPeriods">DTO of the updated fiscalPeriods</param>
        /// <returns>A fiscalPeriods object <see cref="Dtos.FiscalPeriods"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/fiscal-periods/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFiscalPeriodsV11")]
        public async Task<ActionResult<Dtos.FiscalPeriods>> PutFiscalPeriodsAsync([FromRoute] string guid, [FromBody] Dtos.FiscalPeriods fiscalPeriods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a fiscalPeriods
        /// </summary>
        /// <param name="guid">GUID to desired fiscalPeriods</param>
        [HttpDelete]
        [Route("/fiscal-periods/{guid}", Name = "DefaultDeleteFiscalPeriods", Order = -10)]
        public async Task<IActionResult> DeleteFiscalPeriodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

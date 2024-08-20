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
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;

namespace Ellucian.Colleague.Api.Controllers.BudgetManagement
{
    /// <summary>
    /// Provides access to BudgetPhases
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.BudgetManagement)]
    public class BudgetPhasesController : BaseCompressedApiController
    {
        private readonly IBudgetPhasesService _budgetPhasesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BudgetPhasesController class.
        /// </summary>
        /// <param name="budgetPhasesService">Service of type <see cref="IBudgetPhasesService">IBudgetPhasesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BudgetPhasesController(IBudgetPhasesService budgetPhasesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _budgetPhasesService = budgetPhasesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all budgetPhases
        /// </summary>
         /// <param name="criteria">Filter criteria</param>
        /// <returns>List of BudgetPhases <see cref="Dtos.BudgetPhases"/> objects representing matching budgetPhases</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.BudgetPhases))]
        [HttpGet]
        [HeaderVersionRoute("/budget-phases", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetBudgetPhases", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.BudgetPhases>>> GetBudgetPhasesAsync(QueryStringFilter criteria)
        {
            string budgetCode = string.Empty;

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
                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.BudgetPhases>();

                var criteriaObj = GetFilterObject<Dtos.BudgetPhases>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    budgetCode = criteriaObj.BudgetCode != null ? criteriaObj.BudgetCode.Id : string.Empty;
                }             
                
                var items = await _budgetPhasesService.GetBudgetPhasesAsync(budgetCode, bypassCache);

                AddEthosContextProperties(
                  await _budgetPhasesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _budgetPhasesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
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
        /// Read (GET) a budgetPhases using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired budgetPhases</param>
        /// <returns>A budgetPhases object <see cref="Dtos.BudgetPhases"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/budget-phases/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetBudgetPhasesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.BudgetPhases>> GetBudgetPhasesByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _budgetPhasesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _budgetPhasesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _budgetPhasesService.GetBudgetPhasesByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new budgetPhases
        /// </summary>
        /// <param name="budgetPhases">DTO of the new budgetPhases</param>
        /// <returns>A budgetPhases object <see cref="Dtos.BudgetPhases"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/budget-phases", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostBudgetPhasesV12")]
        public async Task<ActionResult<Dtos.BudgetPhases>> PostBudgetPhasesAsync([FromBody] Dtos.BudgetPhases budgetPhases)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing budgetPhases
        /// </summary>
        /// <param name="guid">GUID of the budgetPhases to update</param>
        /// <param name="budgetPhases">DTO of the updated budgetPhases</param>
        /// <returns>A budgetPhases object <see cref="Dtos.BudgetPhases"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/budget-phases/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutBudgetPhasesV12")]
        public async Task<ActionResult<Dtos.BudgetPhases>> PutBudgetPhasesAsync([FromRoute] string guid, [FromBody] Dtos.BudgetPhases budgetPhases)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a budgetPhases
        /// </summary>
        /// <param name="guid">GUID to desired budgetPhases</param>
        [HttpDelete]
        [Route("/budget-phases/{guid}", Name = "DefaultDeleteBudgetPhases", Order = -10)]
        public async Task<IActionResult> DeleteBudgetPhasesAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

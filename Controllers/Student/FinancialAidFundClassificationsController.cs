// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to FinancialAidFundClassifications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidFundClassificationsController : BaseCompressedApiController
    {
        private readonly IFinancialAidFundClassificationsService _financialAidFundClassificationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidFundClassificationsController class.
        /// </summary>
        /// <param name="financialAidFundClassificationsService">Service of type <see cref="IFinancialAidFundClassificationsService">IFinancialAidFundClassificationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidFundClassificationsController(IFinancialAidFundClassificationsService financialAidFundClassificationsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _financialAidFundClassificationsService = financialAidFundClassificationsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all financialAidFundClassifications
        /// </summary>
                /// <returns>List of FinancialAidFundClassifications <see cref="Dtos.FinancialAidFundClassifications"/> objects representing matching financialAidFundClassifications</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-fund-classifications", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFinancialAidFundClassifications", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidFundClassifications>>> GetFinancialAidFundClassificationsAsync()
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
                var items = await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsAsync(bypassCache);

                AddEthosContextProperties(await _financialAidFundClassificationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _financialAidFundClassificationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              items.Select(a => a.Id).ToList()));

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
        /// Read (GET) a financialAidFundClassifications using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired financialAidFundClassifications</param>
        /// <returns>A financialAidFundClassifications object <see cref="Dtos.FinancialAidFundClassifications"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-fund-classifications/{guid}", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidFundClassificationsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FinancialAidFundClassifications>> GetFinancialAidFundClassificationsByGuidAsync(string guid)
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
                var classification = await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsByGuidAsync(guid);

                if (classification != null)
                {

                    AddEthosContextProperties(await _financialAidFundClassificationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _financialAidFundClassificationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { classification.Id }));
                }

                return classification;
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
        /// Create (POST) a new financialAidFundClassifications
        /// </summary>
        /// <param name="financialAidFundClassifications">DTO of the new financialAidFundClassifications</param>
        /// <returns>A financialAidFundClassifications object <see cref="Dtos.FinancialAidFundClassifications"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/financial-aid-fund-classifications", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFinancialAidFundClassificationsV9")]
        public async Task<ActionResult<Dtos.FinancialAidFundClassifications>> PostFinancialAidFundClassificationsAsync([FromBody] Dtos.FinancialAidFundClassifications financialAidFundClassifications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing financialAidFundClassifications
        /// </summary>
        /// <param name="guid">GUID of the financialAidFundClassifications to update</param>
        /// <param name="financialAidFundClassifications">DTO of the updated financialAidFundClassifications</param>
        /// <returns>A financialAidFundClassifications object <see cref="Dtos.FinancialAidFundClassifications"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/financial-aid-fund-classifications/{guid}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFinancialAidFundClassificationsV9")]
        public async Task<ActionResult<Dtos.FinancialAidFundClassifications>> PutFinancialAidFundClassificationsAsync([FromRoute] string guid, [FromBody] Dtos.FinancialAidFundClassifications financialAidFundClassifications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a financialAidFundClassifications
        /// </summary>
        /// <param name="guid">GUID to desired financialAidFundClassifications</param>
        [HttpDelete]
        [Route("/financial-aid-fund-classifications/{guid}", Name = "DefaultDeleteFinancialAidFundClassifications", Order = -10)]
        public async Task<IActionResult> DeleteFinancialAidFundClassificationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

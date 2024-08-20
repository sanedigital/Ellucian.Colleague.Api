// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Tax Codes data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CommerceTaxCodesController : BaseCompressedApiController
    {
        private readonly ICommerceTaxCodeService _commerceTaxCodeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the TaxCodesController class.
        /// </summary>
        /// <param name="taxCodeService">Service of type <see cref="ICommerceTaxCodeService">ITaxCodeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CommerceTaxCodesController(ICommerceTaxCodeService taxCodeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _commerceTaxCodeService = taxCodeService;
            this._logger = logger;
        }

        #region commerce-tax-codes

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 8</remarks>
        /// <summary>
        /// Retrieves all tax codes.
        /// </summary>
        /// <returns>All CommerceTaxCode objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/commerce-tax-codes", "8.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmCommerceTaxCodes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CommerceTaxCode>>> GetCommerceTaxCodesAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var items = await _commerceTaxCodeService.GetCommerceTaxCodesAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _commerceTaxCodeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _commerceTaxCodeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(a => a.Id).ToList()));
                }

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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 8</remarks>
        /// <summary>
        /// Retrieves a commerce tax code by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CommerceTaxCode">CommerceTaxCode.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/commerce-tax-codes/{guid}", "8.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmCommerceTaxCodesById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.CommerceTaxCode>> GetCommerceTaxCodeByIdAsync(string guid)
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
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

            try
            {
                var item = await _commerceTaxCodeService.GetCommerceTaxCodeByGuidAsync(guid);

                if (item != null)
                {
                    AddEthosContextProperties(await _commerceTaxCodeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _commerceTaxCodeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { item.Id }));
                }

                return item;
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
        /// Updates a CommerceTaxCode.
        /// </summary>
        /// /// <param name="guid">GUID of the commerceTaxCodeRates to update</param>
        /// <param name="taxCode"><see cref="CommerceTaxCode">CommerceTaxCode</see> to update</param>
        /// <returns>Newly updated <see cref="CommerceTaxCode">CommerceTaxCode</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/commerce-tax-codes/{guid}", "8.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmCommerceTaxCodes810")]
        public async Task<ActionResult<Dtos.CommerceTaxCode>> PutCommerceTaxCodeAsync([FromRoute] string guid, [FromBody] Dtos.CommerceTaxCode taxCode)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a CommerceTaxCode.
        /// </summary>
        /// <param name="taxCode"><see cref="CommerceTaxCode">CommerceTaxCode</see> to create</param>
        /// <returns>Newly created <see cref="CommerceTaxCode">CommerceTaxCode</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/commerce-tax-codes", "8.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmCommerceTaxCodes810")]
        public async Task<ActionResult<Dtos.CommerceTaxCode>> PostCommerceTaxCodeAsync([FromBody] Dtos.CommerceTaxCode taxCode)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing CommerceTaxCode
        /// </summary>
        /// <param name="guid">Id of the CommerceTaxCode to delete</param>
        [HttpDelete]
        [Route("/commerce-tax-codes/{guid}", Name = "DeleteHedmCommerceTaxCodes", Order = -10)]
        public async Task<IActionResult> DeleteCommerceTaxCodeAsync(string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region commerce-tax-code-rates
        /// <summary>
        /// Return all commerceTaxCodeRates
        /// </summary>
        /// <returns>List of CommerceTaxCodeRates <see cref="Dtos.CommerceTaxCodeRates"/> objects representing matching commerceTaxCodeRates</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/commerce-tax-code-rates", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCommerceTaxCodeRates", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CommerceTaxCodeRates>>> GetCommerceTaxCodeRatesAsync()
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
                var commerceTaxCodeRates = await _commerceTaxCodeService.GetCommerceTaxCodeRatesAsync(bypassCache);

                if (commerceTaxCodeRates != null && commerceTaxCodeRates.Any())
                {
                    AddEthosContextProperties(await _commerceTaxCodeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _commerceTaxCodeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              commerceTaxCodeRates.Select(a => a.Id).ToList()));
                }
                return Ok(commerceTaxCodeRates);
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
        /// Read (GET) a commerceTaxCodeRates using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired commerceTaxCodeRates</param>
        /// <returns>A commerceTaxCodeRates object <see cref="Dtos.CommerceTaxCodeRates"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/commerce-tax-code-rates/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCommerceTaxCodeRatesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.CommerceTaxCodeRates>> GetCommerceTaxCodeRatesByGuidAsync(string guid)
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
                   await _commerceTaxCodeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _commerceTaxCodeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _commerceTaxCodeService.GetCommerceTaxCodeRatesByGuidAsync(guid);
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
        /// Create (POST) a new commerceTaxCodeRates
        /// </summary>
        /// <param name="commerceTaxCodeRates">DTO of the new commerceTaxCodeRates</param>
        /// <returns>A commerceTaxCodeRates object <see cref="Dtos.CommerceTaxCodeRates"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/commerce-tax-code-rates", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCommerceTaxCodeRatesV1.0.0")]
        public async Task<ActionResult<Dtos.CommerceTaxCodeRates>> PostCommerceTaxCodeRatesAsync([FromBody] Dtos.CommerceTaxCodeRates commerceTaxCodeRates)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing commerceTaxCodeRates
        /// </summary>
        /// <param name="guid">GUID of the commerceTaxCodeRates to update</param>
        /// <param name="commerceTaxCodeRates">DTO of the updated commerceTaxCodeRates</param>
        /// <returns>A commerceTaxCodeRates object <see cref="Dtos.CommerceTaxCodeRates"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/commerce-tax-code-rates/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCommerceTaxCodeRatesV1.0.0")]
        public async Task<ActionResult<Dtos.CommerceTaxCodeRates>> PutCommerceTaxCodeRatesAsync([FromRoute] string guid, [FromBody] Dtos.CommerceTaxCodeRates commerceTaxCodeRates)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a commerceTaxCodeRates
        /// </summary>
        /// <param name="guid">GUID to desired commerceTaxCodeRates</param>
        [HttpDelete]
        [Route("/commerce-tax-code-rates/{guid}", Name = "DefaultDeleteCommerceTaxCodeRates", Order = -10)]
        public async Task<IActionResult> DeleteCommerceTaxCodeRatesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
        #endregion
    }
}

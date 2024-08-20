// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to TaxFormComponents
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class TaxFormComponentsController : BaseCompressedApiController
    {
        private readonly ITaxFormComponentsService _taxFormComponentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the TaxFormComponentsController class.
        /// </summary>
        /// <param name="taxFormComponentsService">Service of type <see cref="ITaxFormComponentsService">ITaxFormComponentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TaxFormComponentsController(ITaxFormComponentsService taxFormComponentsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _taxFormComponentsService = taxFormComponentsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all taxFormComponents
        /// </summary>
        /// <returns>List of TaxFormComponents <see cref="Dtos.TaxFormComponents"/> objects representing matching taxFormComponents</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/tax-form-components", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetTaxFormComponents", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.TaxFormComponents>>> GetTaxFormComponentsAsync()
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
                var taxFormComponents = await _taxFormComponentsService.GetTaxFormComponentsAsync(bypassCache);

                if (taxFormComponents != null && taxFormComponents.Any())
                {
                    AddEthosContextProperties(await _taxFormComponentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _taxFormComponentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              taxFormComponents.Select(a => a.Id).ToList()));
                }
                return Ok(taxFormComponents);
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
        /// Read (GET) a taxFormComponents using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired taxFormComponents</param>
        /// <returns>A taxFormComponents object <see cref="Dtos.TaxFormComponents"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/tax-form-components/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetTaxFormComponentsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.TaxFormComponents>> GetTaxFormComponentsByGuidAsync(string guid)
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
                   await _taxFormComponentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _taxFormComponentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _taxFormComponentsService.GetTaxFormComponentsByGuidAsync(guid);
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
        /// Create (POST) a new taxFormComponents
        /// </summary>
        /// <param name="taxFormComponents">DTO of the new taxFormComponents</param>
        /// <returns>A taxFormComponents object <see cref="Dtos.TaxFormComponents"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/tax-form-components", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostTaxFormComponentsV1.0.0")]
        public async Task<ActionResult<Dtos.TaxFormComponents>> PostTaxFormComponentsAsync([FromBody] Dtos.TaxFormComponents taxFormComponents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing taxFormComponents
        /// </summary>
        /// <param name="guid">GUID of the taxFormComponents to update</param>
        /// <param name="taxFormComponents">DTO of the updated taxFormComponents</param>
        /// <returns>A taxFormComponents object <see cref="Dtos.TaxFormComponents"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/tax-form-components/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutTaxFormComponentsV1.0.0")]
        public async Task<ActionResult<Dtos.TaxFormComponents>> PutTaxFormComponentsAsync([FromRoute] string guid, [FromBody] Dtos.TaxFormComponents taxFormComponents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a taxFormComponents
        /// </summary>
        /// <param name="guid">GUID to desired taxFormComponents</param>
        [HttpDelete]
        [Route("/tax-form-components/{guid}", Name = "DefaultDeleteTaxFormComponents", Order = -10)]
        public async Task<IActionResult> DeleteTaxFormComponentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

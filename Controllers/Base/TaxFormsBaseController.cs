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
    /// Provides access to TaxForms
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class TaxFormsBaseController : BaseCompressedApiController
    {
        private readonly ITaxFormsBaseService _taxFormsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the TaxFormsController class.
        /// </summary>
        /// <param name="taxFormsService">Service of type <see cref="ITaxFormsBaseService">ITaxFormsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TaxFormsBaseController(ITaxFormsBaseService taxFormsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _taxFormsService = taxFormsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all taxForms
        /// </summary>
        /// <returns>List of TaxForms <see cref="Dtos.TaxForms"/> objects representing matching taxForms</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/tax-forms", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetTaxForms", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.TaxForms>>> GetTaxFormsAsync()
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
                var taxForms = await _taxFormsService.GetTaxFormsAsync(bypassCache);

                if (taxForms != null && taxForms.Any())
                {
                    AddEthosContextProperties(await _taxFormsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _taxFormsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              taxForms.Select(a => a.Id).ToList()));
                }
                return Ok(taxForms);
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
        /// Read (GET) a taxForms using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired taxForms</param>
        /// <returns>A taxForms object <see cref="Dtos.TaxForms"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/tax-forms/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetTaxFormsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.TaxForms>> GetTaxFormsByGuidAsync(string guid)
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
                   await _taxFormsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _taxFormsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _taxFormsService.GetTaxFormsByGuidAsync(guid);
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
        /// Create (POST) a new taxForms
        /// </summary>
        /// <param name="taxForms">DTO of the new taxForms</param>
        /// <returns>A taxForms object <see cref="Dtos.TaxForms"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/tax-forms", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostTaxFormsV1.0.0")]
        public async Task<ActionResult<Dtos.TaxForms>> PostTaxFormsAsync([FromBody] Dtos.TaxForms taxForms)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing taxForms
        /// </summary>
        /// <param name="guid">GUID of the taxForms to update</param>
        /// <param name="taxForms">DTO of the updated taxForms</param>
        /// <returns>A taxForms object <see cref="Dtos.TaxForms"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/tax-forms/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutTaxFormsV1.0.0")]
        public async Task<ActionResult<Dtos.TaxForms>> PutTaxFormsAsync([FromRoute] string guid, [FromBody] Dtos.TaxForms taxForms)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a taxForms
        /// </summary>
        /// <param name="guid">GUID to desired taxForms</param>
        [HttpDelete]
        [Route("/tax-forms/{guid}", Name = "DefaultDeleteTaxForms", Order = -10)]
        public async Task<IActionResult> DeleteTaxFormsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

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
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;

using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to FinancialDocumentTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class FinancialDocumentTypesController : BaseCompressedApiController
    {
        private readonly IFinancialDocumentTypesService _financialDocumentTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialDocumentTypesController class.
        /// </summary>
        /// <param name="financialDocumentTypesService">Service of type <see cref="IFinancialDocumentTypesService">IFinancialDocumentTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        /// 
        public FinancialDocumentTypesController(IFinancialDocumentTypesService financialDocumentTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _financialDocumentTypesService = financialDocumentTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all financialDocumentTypes
        /// </summary>
        /// <returns>List of FinancialDocumentTypes <see cref="Dtos.FinancialDocumentTypes"/> objects representing matching financialDocumentTypes</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/financial-document-types", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFinancialDocumentTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.FinancialDocumentTypes>>> GetFinancialDocumentTypesAsync()
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
                AddDataPrivacyContextProperty((await _financialDocumentTypesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return Ok(await _financialDocumentTypesService.GetFinancialDocumentTypesAsync(bypassCache));
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
        /// Read (GET) a financialDocumentTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired financialDocumentTypes</param>
        /// <returns>A financialDocumentTypes object <see cref="Dtos.FinancialDocumentTypes"/> in EEDM format</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/financial-document-types/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialDocumentTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FinancialDocumentTypes>> GetFinancialDocumentTypesByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _financialDocumentTypesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _financialDocumentTypesService.GetFinancialDocumentTypesByGuidAsync(guid);
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
        /// Create (POST) a new financialDocumentTypes
        /// </summary>
        /// <param name="financialDocumentTypes">DTO of the new financialDocumentTypes</param>
        /// <returns>A financialDocumentTypes object <see cref="Dtos.FinancialDocumentTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/financial-document-types", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFinancialDocumentTypesV11")]
        public async Task<ActionResult<Dtos.FinancialDocumentTypes>> PostFinancialDocumentTypesAsync([FromBody] Dtos.FinancialDocumentTypes financialDocumentTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing financialDocumentTypes
        /// </summary>
        /// <param name="guid">GUID of the financialDocumentTypes to update</param>
        /// <param name="financialDocumentTypes">DTO of the updated financialDocumentTypes</param>
        /// <returns>A financialDocumentTypes object <see cref="Dtos.FinancialDocumentTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/financial-document-types/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFinancialDocumentTypesV11")]
        public async Task<ActionResult<Dtos.FinancialDocumentTypes>> PutFinancialDocumentTypesAsync([FromRoute] string guid, [FromBody] Dtos.FinancialDocumentTypes financialDocumentTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a financialDocumentTypes
        /// </summary>
        /// <param name="guid">GUID to desired financialDocumentTypes</param>
        [HttpDelete]
        [Route("/financial-document-types/{guid}", Name = "DefaultDeleteFinancialDocumentTypes", Order = -10)]
        public async Task<IActionResult> DeleteFinancialDocumentTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

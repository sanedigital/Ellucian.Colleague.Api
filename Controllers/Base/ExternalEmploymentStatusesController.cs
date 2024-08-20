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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to ExternalEmploymentStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ExternalEmploymentStatusesController : BaseCompressedApiController
    {
        private readonly IExternalEmploymentStatusesService _externalEmploymentStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ExternalEmploymentStatusesController class.
        /// </summary>
        /// <param name="externalEmploymentStatusesService">Service of type <see cref="IExternalEmploymentStatusesService">IExternalEmploymentStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ExternalEmploymentStatusesController(IExternalEmploymentStatusesService externalEmploymentStatusesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _externalEmploymentStatusesService = externalEmploymentStatusesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all externalEmploymentStatuses
        /// </summary>
        /// <returns>List of ExternalEmploymentStatuses <see cref="Dtos.ExternalEmploymentStatuses"/> objects representing matching externalEmploymentStatuses</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/external-employment-statuses", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetExternalEmploymentStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmploymentStatuses>>> GetExternalEmploymentStatusesAsync()
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
                var externalEmploymentStatuses = await _externalEmploymentStatusesService.GetExternalEmploymentStatusesAsync(bypassCache);

                if (externalEmploymentStatuses != null && externalEmploymentStatuses.Any())
                {
                    AddEthosContextProperties(await _externalEmploymentStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _externalEmploymentStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              externalEmploymentStatuses.Select(a => a.Id).ToList()));
                }

                return Ok(externalEmploymentStatuses);
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
        /// Read (GET) a externalEmploymentStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired externalEmploymentStatuses</param>
        /// <returns>A externalEmploymentStatuses object <see cref="Dtos.ExternalEmploymentStatuses"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/external-employment-statuses/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetExternalEmploymentStatusesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ExternalEmploymentStatuses>> GetExternalEmploymentStatusesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _externalEmploymentStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _externalEmploymentStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _externalEmploymentStatusesService.GetExternalEmploymentStatusesByGuidAsync(guid);
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
        /// Create (POST) a new externalEmploymentStatuses
        /// </summary>
        /// <param name="externalEmploymentStatuses">DTO of the new externalEmploymentStatuses</param>
        /// <returns>A externalEmploymentStatuses object <see cref="Dtos.ExternalEmploymentStatuses"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/external-employment-statuses", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostExternalEmploymentStatusesV10")]
        public async Task<ActionResult<Dtos.ExternalEmploymentStatuses>> PostExternalEmploymentStatusesAsync([FromBody] Dtos.ExternalEmploymentStatuses externalEmploymentStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing externalEmploymentStatuses
        /// </summary>
        /// <param name="guid">GUID of the externalEmploymentStatuses to update</param>
        /// <param name="externalEmploymentStatuses">DTO of the updated externalEmploymentStatuses</param>
        /// <returns>A externalEmploymentStatuses object <see cref="Dtos.ExternalEmploymentStatuses"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/external-employment-statuses/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutExternalEmploymentStatusesV10")]
        public async Task<ActionResult<Dtos.ExternalEmploymentStatuses>> PutExternalEmploymentStatusesAsync([FromRoute] string guid, [FromBody] Dtos.ExternalEmploymentStatuses externalEmploymentStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a externalEmploymentStatuses
        /// </summary>
        /// <param name="guid">GUID to desired externalEmploymentStatuses</param>
        [HttpDelete]
        [Route("/external-employment-statuses/{guid}", Name = "DefaultDeleteExternalEmploymentStatuses", Order = -10)]
        public async Task<IActionResult> DeleteExternalEmploymentStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

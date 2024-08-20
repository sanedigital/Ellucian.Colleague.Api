// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to FixedAssetDesignations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class FixedAssetDesignationsController : BaseCompressedApiController
    {
        private readonly IFixedAssetDesignationsService _fixedAssetDesignationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FixedAssetDesignationsController class.
        /// </summary>
        /// <param name="fixedAssetDesignationsService">Service of type <see cref="IFixedAssetDesignationsService">IFixedAssetDesignationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FixedAssetDesignationsController(IFixedAssetDesignationsService fixedAssetDesignationsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _fixedAssetDesignationsService = fixedAssetDesignationsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all fixedAssetDesignations
        /// </summary>
        /// <returns>List of FixedAssetDesignations <see cref="Dtos.FixedAssetDesignations"/> objects representing matching fixedAssetDesignations</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/fixed-asset-designations", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFixedAssetDesignations", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.FixedAssetDesignations>>> GetFixedAssetDesignationsAsync()
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
                var fixedAssetDesignations = await _fixedAssetDesignationsService.GetFixedAssetDesignationsAsync(bypassCache);

                if (fixedAssetDesignations != null && fixedAssetDesignations.Any())
                {
                    AddEthosContextProperties(await _fixedAssetDesignationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _fixedAssetDesignationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              fixedAssetDesignations.Select(a => a.Id).ToList()));
                }
                return Ok(fixedAssetDesignations);
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
        /// Read (GET) a fixedAssetDesignations using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssetDesignations</param>
        /// <returns>A fixedAssetDesignations object <see cref="Dtos.FixedAssetDesignations"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/fixed-asset-designations/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFixedAssetDesignationsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FixedAssetDesignations>> GetFixedAssetDesignationsByGuidAsync(string guid)
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
                   await _fixedAssetDesignationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _fixedAssetDesignationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _fixedAssetDesignationsService.GetFixedAssetDesignationsByGuidAsync(guid);
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
        /// Create (POST) a new fixedAssetDesignations
        /// </summary>
        /// <param name="fixedAssetDesignations">DTO of the new fixedAssetDesignations</param>
        /// <returns>A fixedAssetDesignations object <see cref="Dtos.FixedAssetDesignations"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/fixed-asset-designations", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFixedAssetDesignationsV1.0.0")]
        public async Task<ActionResult<Dtos.FixedAssetDesignations>> PostFixedAssetDesignationsAsync([FromBody] Dtos.FixedAssetDesignations fixedAssetDesignations)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing fixedAssetDesignations
        /// </summary>
        /// <param name="guid">GUID of the fixedAssetDesignations to update</param>
        /// <param name="fixedAssetDesignations">DTO of the updated fixedAssetDesignations</param>
        /// <returns>A fixedAssetDesignations object <see cref="Dtos.FixedAssetDesignations"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/fixed-asset-designations/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFixedAssetDesignationsV1.0.0")]
        public async Task<ActionResult<Dtos.FixedAssetDesignations>> PutFixedAssetDesignationsAsync([FromRoute] string guid, [FromBody] Dtos.FixedAssetDesignations fixedAssetDesignations)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a fixedAssetDesignations
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssetDesignations</param>
        [HttpDelete]
        [Route("/fixed-asset-designations/{guid}", Name = "DefaultDeleteFixedAssetDesignations", Order = -10)]
        public async Task<IActionResult> DeleteFixedAssetDesignationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

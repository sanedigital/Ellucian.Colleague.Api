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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to BeneficiaryPreferenceTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class BeneficiaryPreferenceTypesController : BaseCompressedApiController
    {
        private readonly IBeneficiaryPreferenceTypesService _beneficiaryPreferenceTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BeneficiaryPreferenceTypesController class.
        /// </summary>
        /// <param name="beneficiaryPreferenceTypesService">Service of type <see cref="IBeneficiaryPreferenceTypesService">IBeneficiaryPreferenceTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BeneficiaryPreferenceTypesController(IBeneficiaryPreferenceTypesService beneficiaryPreferenceTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _beneficiaryPreferenceTypesService = beneficiaryPreferenceTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all beneficiaryPreferenceTypes
        /// </summary>
        /// <returns>List of BeneficiaryPreferenceTypes <see cref="Dtos.BeneficiaryPreferenceTypes"/> objects representing matching beneficiaryPreferenceTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/beneficiary-preference-types", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetBeneficiaryPreferenceTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.BeneficiaryPreferenceTypes>>> GetBeneficiaryPreferenceTypesAsync()
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
                var beneficiaryPreferenceTypes = await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesAsync(bypassCache);

                if (beneficiaryPreferenceTypes != null && beneficiaryPreferenceTypes.Any())
                {
                    AddEthosContextProperties(await _beneficiaryPreferenceTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _beneficiaryPreferenceTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              beneficiaryPreferenceTypes.Select(a => a.Id).ToList()));
                }
                return Ok(beneficiaryPreferenceTypes);
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

        /// <summary>BeneficiaryPreferenceTypesController
        /// Read (GET) a beneficiaryPreferenceTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired beneficiaryPreferenceTypes</param>
        /// <returns>A beneficiaryPreferenceTypes object <see cref="Dtos.BeneficiaryPreferenceTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/beneficiary-preference-types/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetBeneficiaryPreferenceTypesByGuid")]
        public async Task<ActionResult<Dtos.BeneficiaryPreferenceTypes>> GetBeneficiaryPreferenceTypesByGuidAsync(string guid)
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
                    await _beneficiaryPreferenceTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _beneficiaryPreferenceTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesByGuidAsync(guid);
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
        /// Create (POST) a new beneficiaryPreferenceTypes
        /// </summary>
        /// <param name="beneficiaryPreferenceTypes">DTO of the new beneficiaryPreferenceTypes</param>
        /// <returns>A beneficiaryPreferenceTypes object <see cref="Dtos.BeneficiaryPreferenceTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/beneficiary-preference-types", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostBeneficiaryPreferenceTypesV11")]
        public async Task<ActionResult<Dtos.BeneficiaryPreferenceTypes>> PostBeneficiaryPreferenceTypesAsync([FromBody] Dtos.BeneficiaryPreferenceTypes beneficiaryPreferenceTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing beneficiaryPreferenceTypes
        /// </summary>
        /// <param name="guid">GUID of the beneficiaryPreferenceTypes to update</param>
        /// <param name="beneficiaryPreferenceTypes">DTO of the updated beneficiaryPreferenceTypes</param>
        /// <returns>A beneficiaryPreferenceTypes object <see cref="Dtos.BeneficiaryPreferenceTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/beneficiary-preference-types/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutBeneficiaryPreferenceTypesV11")]
        public async Task<ActionResult<Dtos.BeneficiaryPreferenceTypes>> PutBeneficiaryPreferenceTypesAsync([FromRoute] string guid, [FromBody] Dtos.BeneficiaryPreferenceTypes beneficiaryPreferenceTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a beneficiaryPreferenceTypes
        /// </summary>
        /// <param name="guid">GUID to desired beneficiaryPreferenceTypes</param>
        [HttpDelete]
        [Route("/beneficiary-preference-types/{guid}", Name = "DefaultDeleteBeneficiaryPreferenceTypes")]
        public async Task<IActionResult> DeleteBeneficiaryPreferenceTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

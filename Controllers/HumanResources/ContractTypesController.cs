// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
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


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to ContractTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class ContractTypesController : BaseCompressedApiController
    {
        private readonly IContractTypesService _contractTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ContractTypesController class.
        /// </summary>
        /// <param name="contractTypesService">Service of type <see cref="IContractTypesService">IContractTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ContractTypesController(IContractTypesService contractTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _contractTypesService = contractTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all contractTypes
        /// </summary>
        /// <returns>List of ContractTypes <see cref="Dtos.ContractTypes"/> objects representing matching contractTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/contract-types", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetContractTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ContractTypes>>> GetContractTypesAsync()
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
                AddDataPrivacyContextProperty((await _contractTypesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                
                var contractTypes = await _contractTypesService.GetContractTypesAsync(bypassCache);

                if (contractTypes != null && contractTypes.Any())
                {
                    AddEthosContextProperties(await _contractTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _contractTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              contractTypes.Select(a => a.Id).ToList()));
                }

                return Ok(contractTypes);                
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
        /// Read (GET) a contractTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired contractTypes</param>
        /// <returns>A contractTypes object <see cref="Dtos.ContractTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/contract-types/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetContractTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ContractTypes>> GetContractTypesByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _contractTypesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                    await _contractTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _contractTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _contractTypesService.GetContractTypesByGuidAsync(guid);
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
        /// Create (POST) a new contractTypes
        /// </summary>
        /// <param name="contractTypes">DTO of the new contractTypes</param>
        /// <returns>A contractTypes object <see cref="Dtos.ContractTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/contract-types", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostContractTypesV11")]
        public async Task<ActionResult<Dtos.ContractTypes>> PostContractTypesAsync([FromBody] Dtos.ContractTypes contractTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing contractTypes
        /// </summary>
        /// <param name="guid">GUID of the contractTypes to update</param>
        /// <param name="contractTypes">DTO of the updated contractTypes</param>
        /// <returns>A contractTypes object <see cref="Dtos.ContractTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/contract-types/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutContractTypesV11")]
        public async Task<ActionResult<Dtos.ContractTypes>> PutContractTypesAsync([FromRoute] string guid, [FromBody] Dtos.ContractTypes contractTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a contractTypes
        /// </summary>
        /// <param name="guid">GUID to desired contractTypes</param>
        [HttpDelete]
        [Route("/contract-types/{guid}", Name = "DefaultDeleteContractTypes")]
        public async Task<IActionResult> DeleteContractTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

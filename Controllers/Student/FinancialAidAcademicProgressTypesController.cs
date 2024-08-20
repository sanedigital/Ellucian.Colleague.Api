// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to FinancialAidAcademicProgressTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidAcademicProgressTypesController : BaseCompressedApiController
    {
        private readonly IFinancialAidAcademicProgressTypesService _financialAidAcademicProgressTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidAcademicProgressTypesController class.
        /// </summary>
        /// <param name="financialAidAcademicProgressTypesService">Service of type <see cref="IFinancialAidAcademicProgressTypesService">IFinancialAidAcademicProgressTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public FinancialAidAcademicProgressTypesController(IFinancialAidAcademicProgressTypesService financialAidAcademicProgressTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _financialAidAcademicProgressTypesService = financialAidAcademicProgressTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all financialAidAcademicProgressTypes
        /// </summary>
        /// <returns>List of FinancialAidAcademicProgressTypes <see cref="Dtos.FinancialAidAcademicProgressTypes"/> objects representing matching financialAidAcademicProgressTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-academic-progress-types", "15", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidAcademicProgressTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressTypes>>> GetFinancialAidAcademicProgressTypesAsync()
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
                var progressTypes = await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesAsync(bypassCache);

                if (progressTypes != null && progressTypes.Any())
                {
                    AddEthosContextProperties(
                      await _financialAidAcademicProgressTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _financialAidAcademicProgressTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                          progressTypes.Select(i => i.Id).ToList()));
                }
                return Ok(progressTypes);
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
        /// Read (GET) a financialAidAcademicProgressTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired financialAidAcademicProgressTypes</param>
        /// <returns>A financialAidAcademicProgressTypes object <see cref="Dtos.FinancialAidAcademicProgressTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-academic-progress-types/{guid}", "15", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidAcademicProgressTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FinancialAidAcademicProgressTypes>> GetFinancialAidAcademicProgressTypesByGuidAsync(string guid)
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
                var progressType = await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesByGuidAsync(guid);
                if (progressType != null)
                {

                    AddEthosContextProperties(await _financialAidAcademicProgressTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _financialAidAcademicProgressTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { progressType.Id }));
                }
                return progressType;
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
        /// Create (POST) a new financialAidAcademicProgressTypes
        /// </summary>
        /// <param name="financialAidAcademicProgressTypes">DTO of the new financialAidAcademicProgressTypes</param>
        /// <returns>A financialAidAcademicProgressTypes object <see cref="Dtos.FinancialAidAcademicProgressTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/financial-aid-academic-progress-types", "15", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFinancialAidAcademicProgressTypesV15")]
        public async Task<ActionResult<Dtos.FinancialAidAcademicProgressTypes>> PostFinancialAidAcademicProgressTypesAsync([FromBody] Dtos.FinancialAidAcademicProgressTypes financialAidAcademicProgressTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing financialAidAcademicProgressTypes
        /// </summary>
        /// <param name="guid">GUID of the financialAidAcademicProgressTypes to update</param>
        /// <param name="financialAidAcademicProgressTypes">DTO of the updated financialAidAcademicProgressTypes</param>
        /// <returns>A financialAidAcademicProgressTypes object <see cref="Dtos.FinancialAidAcademicProgressTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/financial-aid-academic-progress-types/{guid}", "15", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFinancialAidAcademicProgressTypesV15")]
        public async Task<ActionResult<Dtos.FinancialAidAcademicProgressTypes>> PutFinancialAidAcademicProgressTypesAsync([FromRoute] string guid, [FromBody] Dtos.FinancialAidAcademicProgressTypes financialAidAcademicProgressTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a financialAidAcademicProgressTypes
        /// </summary>
        /// <param name="guid">GUID to desired financialAidAcademicProgressTypes</param>
        [HttpDelete]
        [Route("/financial-aid-academic-progress-types/{guid}", Name = "DefaultDeleteFinancialAidAcademicProgressTypes", Order = -10)]
        public async Task<IActionResult> DeleteFinancialAidAcademicProgressTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

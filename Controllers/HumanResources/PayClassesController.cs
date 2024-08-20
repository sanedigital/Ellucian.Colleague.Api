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
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PayClasses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayClassesController : BaseCompressedApiController
    {
        private readonly IPayClassesService _payClassesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PayClassesController class.
        /// </summary>
        /// <param name="payClassesService">Service of type <see cref="IPayClassesService">IPayClassesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PayClassesController(IPayClassesService payClassesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _payClassesService = payClassesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all payClasses
        /// </summary>
                /// <returns>List of PayClasses <see cref="Dtos.PayClasses"/> objects representing matching payClasses</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/pay-classes", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPayClassesV11", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PayClasses>>> GetPayClassesAsync()
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
                var payClassEntities = await _payClassesService.GetPayClassesAsync(bypassCache);

                AddEthosContextProperties(
                    await _payClassesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _payClassesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        payClassEntities.Select(i => i.Id).ToList()));

                return Ok(payClassEntities);
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
        /// Read (GET) a payClasses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired payClasses</param>
        /// <returns>A payClasses object <see cref="Dtos.PayClasses"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/pay-classes/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPayClassesByGuidV11")]
        public async Task<ActionResult<Dtos.PayClasses>> GetPayClassesByGuidAsync(string guid)
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
                     await _payClassesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _payClassesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         new List<string>() { guid }));

                return await _payClassesService.GetPayClassesByGuidAsync(guid);
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
        /// Return all payClasses
        /// </summary>
        /// <returns>List of PayClasses <see cref="Dtos.PayClasses"/> objects representing matching payClasses</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Ellucian.Colleague.Dtos.PayClasses2))]
        [HeaderVersionRoute("/pay-classes", "12.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPayClasses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PayClasses2>>> GetPayClasses2Async(QueryStringFilter criteria)
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
                var criteriaObj = GetFilterObject<Ellucian.Colleague.Dtos.PayClasses2>(_logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new List<Ellucian.Colleague.Dtos.PayClasses2>(new List<Ellucian.Colleague.Dtos.PayClasses2>());
                var payClassEntities = await _payClassesService.GetPayClasses2Async(bypassCache);
                if (criteriaObj != null && !string.IsNullOrEmpty(criteriaObj.Code) && payClassEntities != null && payClassEntities.Any())
                {
                    var code = criteriaObj.Code;
                    payClassEntities = payClassEntities.Where(c => c.Code == code);
                }

                AddEthosContextProperties(
                    await _payClassesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _payClassesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        payClassEntities.Select(i => i.Id).ToList()));

                return Ok(payClassEntities);
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
        /// Read (GET) a payClasses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired payClasses</param>
        /// <returns>A payClasses object <see cref="Dtos.PayClasses"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/pay-classes/{guid}", "12.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPayClassesByGuid")]
        public async Task<ActionResult<Dtos.PayClasses2>> GetPayClassesByGuid2Async(string guid)
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
                     await _payClassesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _payClassesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         new List<string>() { guid }));

                return await _payClassesService.GetPayClassesByGuid2Async(guid);
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
        /// Create (POST) a new payClasses
        /// </summary>
        /// <param name="payClasses">DTO of the new payClasses</param>
        /// <returns>A payClasses object <see cref="Dtos.PayClasses"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/pay-classes", "12.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPayClassesV1210")]
        [HeaderVersionRoute("/pay-classes", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPayClassesV11")]
        public async Task<ActionResult<Dtos.PayClasses>> PostPayClassesAsync([FromBody] Dtos.PayClasses payClasses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing payClasses
        /// </summary>
        /// <param name="guid">GUID of the payClasses to update</param>
        /// <param name="payClasses">DTO of the updated payClasses</param>
        /// <returns>A payClasses object <see cref="Dtos.PayClasses"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/pay-classes/{guid}", "12.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPayClassesV1210")]
        [HeaderVersionRoute("/pay-classes/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPayClassesV11")]
        public async Task<ActionResult<Dtos.PayClasses>> PutPayClassesAsync([FromRoute] string guid, [FromBody] Dtos.PayClasses payClasses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a payClasses
        /// </summary>
        /// <param name="guid">GUID to desired payClasses</param>
        [HttpDelete]
        [Route("/pay-classes/{guid}", Name = "DefaultDeletePayClasses", Order = -10)]
        public async Task<IActionResult> DeletePayClassesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

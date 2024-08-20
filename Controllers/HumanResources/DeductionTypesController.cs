// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using System.Linq;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes deduction types data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class DeductionTypesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IDeductionTypesService _deductionTypesService;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="deductionTypesService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DeductionTypesController(ILogger logger, IDeductionTypesService deductionTypesService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this._deductionTypesService = deductionTypesService;
        }

        /// <summary>
        /// Returns all deduction types.
        /// </summary>
        /// <returns></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/deduction-types", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmDeductionTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.DeductionType>>> GetAllDeductionTypesAsync()
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

                var items = await _deductionTypesService.GetDeductionTypesAsync(bypassCache);

                AddEthosContextProperties(
                  await _deductionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _deductionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return Ok(items);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting deduction types.");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns all deduction types.
        /// </summary>
        /// <returns></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/deduction-types", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmDeductionTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.DeductionType2>>> GetAllDeductionTypes2Async()
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

                var items = await _deductionTypesService.GetDeductionTypes2Async(bypassCache);

                AddEthosContextProperties(
                  await _deductionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _deductionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return Ok(items);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Returns a deduction type.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/deduction-types/{id}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetDeductionTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.DeductionType2>> GetDeductionTypeById2Async([FromRoute] string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                  await _deductionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _deductionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _deductionTypesService.GetDeductionTypeById2Async(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Returns a deduction type.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/deduction-types/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmDeductionTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.DeductionType>> GetDeductionTypeByIdAsync([FromRoute] string id)
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
                AddEthosContextProperties(
                    await _deductionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _deductionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _deductionTypesService.GetDeductionTypeByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e, "No deduction types was found for guid " + id + ".");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting deduction type.");
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// PutDeductionTypeAsync
        /// </summary>
        /// <param name="deductionType"></param>
        /// <returns></returns>
        [HttpPut]
        [HeaderVersionRoute("/deduction-types/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmDeductionType")]
        public async Task<ActionResult<Dtos.DeductionType>> PutDeductionTypeAsync([FromBody] Dtos.DeductionType deductionType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// PutDeductionTypeAsync
        /// </summary>
        /// <param name="deductionType"></param>
        /// <returns></returns>
        [HttpPut]
        [HeaderVersionRoute("/deduction-types/{id}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmDeductionTypeV11")]
        public async Task<ActionResult<Dtos.DeductionType2>> PutDeductionType2Async([FromBody] Dtos.DeductionType2 deductionType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// PostDeductionTypeAsync
        /// </summary>
        /// <param name="deductionType"></param>
        /// <returns></returns>
        [HttpPost]
        [HeaderVersionRoute("/deduction-types", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmDeductionType")]
        public async Task<ActionResult<Dtos.DeductionType>> PostDeductionTypeAsync([FromBody] Dtos.DeductionType deductionType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// PostDeductionTypeAsync
        /// </summary>
        /// <param name="deductionType"></param>
        /// <returns></returns>
        [HttpPost]
        [HeaderVersionRoute("/deduction-types", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmDeductionTypeV11")]
        public async Task<ActionResult<Dtos.DeductionType2>> PostDeductionType2Async([FromBody] Dtos.DeductionType2 deductionType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// DeleteDeductionTypeAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/deduction-types/{id}", Name = "DeleteDeductionType")]
        public async Task<IActionResult> DeleteDeductionTypeAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

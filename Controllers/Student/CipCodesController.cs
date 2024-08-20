// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using System;
using Ellucian.Colleague.Dtos;
using System.Net;
using Ellucian.Web.Http.Filters;
using System.Linq;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Security;

using Ellucian.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to CipCode data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CipCodesController : BaseCompressedApiController
    {
        private readonly ICipCodeService _cipCodeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CipCodesController class.
        /// </summary>
        /// <param name="cipCodeService">Service of type <see cref="ICipCodeService">ICipService</see>/></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CipCodesController(ICipCodeService cipCodeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _cipCodeService = cipCodeService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 1.1.0</remarks>
        /// <summary>
        /// Retrieves all student statuses.
        /// </summary>
        /// <returns>All <see cref="CipCode">CipCodes.</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/cip-codes", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetCipCodes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<CipCode>>> GetCipCodesAsync()
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
                var cipCodes = await _cipCodeService.GetCipCodesAsync(bypassCache);

                if (cipCodes != null && cipCodes.Any())
                {
                    AddEthosContextProperties(await _cipCodeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _cipCodeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              cipCodes.Select(a => a.Id).ToList()));
                }

                return Ok(cipCodes);                
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves an student status by ID.
        /// </summary>
        /// <returns>A <see cref="CipCode">CipCode.</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/cip-codes/{id}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCipCodesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<CipCode>> GetCipCodeByIdAsync(string id)
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
                    await _cipCodeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _cipCodeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _cipCodeService.GetCipCodeByGuidAsync(id, bypassCache);
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
        /// Updates a CipCode.
        /// </summary>
        /// <param name="id">Id to PUT data to</param>
        /// <param name="cipCode"><see cref="CipCode">CipCode</see> to update</param>
        /// <returns>Newly updated <see cref="CipCode">CipCode</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/cip-codes/{id}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCipCodesV1.0.0")]
        public async Task<ActionResult<CipCode>> PutCipCodeAsync([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] CipCode cipCode)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a CipCode.
        /// </summary>
        /// <param name="cipCode"><see cref="CipCode">CipCode</see> to create</param>
        /// <returns>Newly created <see cref="CipCode">CipCode</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/cip-codes", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCipCodesV1.0.0")]
        public async Task<ActionResult<CipCode>> PostCipCodeAsync([FromBody] CipCode cipCode)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing CipCode
        /// </summary>
        /// <param name="id">Id of the CipCode to delete</param>
        [HttpDelete]
        [Route("/cip-codes/{id}", Name = "DefaultDeleteCipCodes", Order = -10)]
        public async Task<IActionResult> DeleteCipCodeAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

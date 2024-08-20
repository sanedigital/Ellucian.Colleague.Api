// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
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

using Ellucian.Web.Http.Filters;
using System.Linq;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to EmploymentVocations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EmploymentVocationsController : BaseCompressedApiController
    {
        private readonly IEmploymentVocationService _employmentVocationService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentVocationsController class.
        /// </summary>
        /// <param name="employmentVocationService">Service of type <see cref="IEmploymentVocationService">IEmploymentVocationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmploymentVocationsController(IEmploymentVocationService employmentVocationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _employmentVocationService = employmentVocationService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all employmentVocations
        /// </summary>
        /// <returns>List of EmploymentVocation <see cref="Dtos.EmploymentVocation"/> objects representing matching employmentVocations</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/employment-vocations", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEmploymentVocations", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.EmploymentVocation>>> GetEmploymentVocationsAsync()
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
                var items = await _employmentVocationService.GetEmploymentVocationsAsync(bypassCache);

                AddEthosContextProperties(await _employmentVocationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _employmentVocationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              items.Select(a => a.Id).ToList()));

                return Ok(items);
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
            catch (InvalidOperationException e)
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
        /// Read (GET) a employmentVocation using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employmentVocation</param>
        /// <returns>A employmentVocations object <see cref="Dtos.EmploymentVocation"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/employment-vocations/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmploymentVocationsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmploymentVocation>> GetEmploymentVocationByGuidAsync(string guid)
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
                var employmentVocation = await _employmentVocationService.GetEmploymentVocationByGuidAsync(guid);

                if (employmentVocation != null)
                {

                    AddEthosContextProperties(await _employmentVocationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _employmentVocationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { employmentVocation.Id }));
                }

                return employmentVocation;
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
            catch (InvalidOperationException e)
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
        /// Create (POST) a new employmentVocation
        /// </summary>
        /// <param name="employmentVocation">DTO of the new employmentVocation</param>
        /// <returns>A employmentVocations object <see cref="Dtos.EmploymentVocation"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/employment-vocations", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostVocationsV1")]
        public async Task<ActionResult<Dtos.EmploymentVocation>> PostEmploymentVocationAsync([FromBody] Dtos.EmploymentVocation employmentVocation)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employmentVocation
        /// </summary>
        /// <param name="guid">GUID of the employmentVocation to update</param>
        /// <param name="employmentVocation">DTO of the updated employmentVocation</param>
        /// <returns>A employmentVocation object <see cref="Dtos.EmploymentVocation"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/employment-vocations/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmploymentVocationsV10")]
        public async Task<ActionResult<Dtos.EmploymentVocation>> PutEmploymentVocationAsync([FromRoute] string guid, [FromBody] Dtos.EmploymentVocation employmentVocation)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employmentVocation
        /// </summary>
        /// <param name="guid">GUID to desired employmentVocation</param>
        [HttpDelete]
        [Route("/employment-vocations/{guid}", Name = "DefaultDeleteVocations", Order = -10)]
        public async Task<IActionResult> DeleteEmploymentVocationAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

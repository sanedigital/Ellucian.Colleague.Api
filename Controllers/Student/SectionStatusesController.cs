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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to SectionStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionStatusesController : BaseCompressedApiController
    {
        private readonly ISectionStatusesService _sectionStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SectionStatusesController class.
        /// </summary>
        /// <param name="sectionStatusesService">Service of type <see cref="ISectionStatusesService">ISectionStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionStatusesController(ISectionStatusesService sectionStatusesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _sectionStatusesService = sectionStatusesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all sectionStatuses
        /// </summary>
        /// <returns>List of SectionStatuses <see cref="Dtos.SectionStatuses"/> objects representing matching sectionStatuses</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/section-statuses", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetSectionStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.SectionStatuses>>> GetSectionStatusesAsync()
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
                var sectionStatuses = await _sectionStatusesService.GetSectionStatusesAsync(bypassCache);

                if (sectionStatuses != null && sectionStatuses.Any())
                {
                    AddEthosContextProperties(await _sectionStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              sectionStatuses.Select(a => a.Id).ToList()));
                }
                return Ok(sectionStatuses);
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
        /// Read (GET) a sectionStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired sectionStatuses</param>
        /// <returns>A sectionStatuses object <see cref="Dtos.SectionStatuses"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/section-statuses/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSectionStatusesByGuid")]
        public async Task<ActionResult<Dtos.SectionStatuses>> GetSectionStatusesByGuidAsync(string guid)
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
                     await _sectionStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _sectionStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         new List<string>() { guid }));
                return await _sectionStatusesService.GetSectionStatusesByGuidAsync(guid);
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
        /// Create (POST) a new sectionStatuses
        /// </summary>
        /// <param name="sectionStatuses">DTO of the new sectionStatuses</param>
        /// <returns>A sectionStatuses object <see cref="Dtos.SectionStatuses"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/section-statuses", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSectionStatusesV11")]
        public async Task<ActionResult<Dtos.SectionStatuses>> PostSectionStatusesAsync([FromBody] Dtos.SectionStatuses sectionStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing sectionStatuses
        /// </summary>
        /// <param name="guid">GUID of the sectionStatuses to update</param>
        /// <param name="sectionStatuses">DTO of the updated sectionStatuses</param>
        /// <returns>A sectionStatuses object <see cref="Dtos.SectionStatuses"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/section-statuses/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSectionStatusesV11")]
        public async Task<ActionResult<Dtos.SectionStatuses>> PutSectionStatusesAsync([FromRoute] string guid, [FromBody] Dtos.SectionStatuses sectionStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a sectionStatuses
        /// </summary>
        /// <param name="guid">GUID to desired sectionStatuses</param>
        [HttpDelete]
        [Route("/section-statuses/{guid}", Name = "DefaultDeleteSectionStatuses", Order = -10)]
        public async Task<IActionResult> DeleteSectionStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

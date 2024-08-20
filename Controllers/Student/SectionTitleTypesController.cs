// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to SectionTitleTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionTitleTypesController : BaseCompressedApiController
    {
        private readonly ISectionTitleTypesService _sectionTitleTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SectionTitleTypesController class.
        /// </summary>
        /// <param name="sectionTitleTypesService">Service of type <see cref="ISectionTitleTypesService">ISectionTitleTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionTitleTypesController(ISectionTitleTypesService sectionTitleTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _sectionTitleTypesService = sectionTitleTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all sectionTitleTypes
        /// </summary>
        /// <returns>List of SectionTitleTypes <see cref="Dtos.SectionTitleType"/> objects representing matching sectionTitleTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/section-title-types", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSectionTitleTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.SectionTitleType>>> GetSectionTitleTypesAsync()
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
                var sectionTitleTypes = await _sectionTitleTypesService.GetSectionTitleTypesAsync(bypassCache);

                if (sectionTitleTypes != null && sectionTitleTypes.Any())
                {
                    AddEthosContextProperties(await _sectionTitleTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _sectionTitleTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              sectionTitleTypes.Select(a => a.Id).ToList()));
                }
                return Ok(sectionTitleTypes);
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
        /// Read (GET) a sectionTitleTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired sectionTitleTypes</param>
        /// <returns>A sectionTitleTypes object <see cref="Dtos.SectionTitleType"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/section-title-types/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSectionTitleTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionTitleType>> GetSectionTitleTypeByGuidAsync(string guid)
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
                   await _sectionTitleTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionTitleTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _sectionTitleTypesService.GetSectionTitleTypeByGuidAsync(guid);
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
        /// Create (POST) a new sectionTitleTypes
        /// </summary>
        /// <param name="sectionTitleTypes">DTO of the new sectionTitleTypes</param>
        /// <returns>A sectionTitleTypes object <see cref="Dtos.SectionTitleType"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/section-title-types", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSectionTitleTypesV1")]
        public async Task<ActionResult<Dtos.SectionTitleType>> PostSectionTitleTypeAsync([FromBody] Dtos.SectionTitleType sectionTitleTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing sectionTitleTypes
        /// </summary>
        /// <param name="guid">GUID of the sectionTitleTypes to update</param>
        /// <param name="sectionTitleTypes">DTO of the updated sectionTitleTypes</param>
        /// <returns>A sectionTitleTypes object <see cref="Dtos.SectionTitleType"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/section-title-types/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSectionTitleTypesV1")]
        public async Task<ActionResult<Dtos.SectionTitleType>> PutSectionTitleTypeAsync([FromRoute] string guid, [FromBody] Dtos.SectionTitleType sectionTitleTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a sectionTitleTypes
        /// </summary>
        /// <param name="guid">GUID to desired sectionTitleTypes</param>
        [HttpDelete]
        [Route("/section-title-types/{guid}", Name = "DefaultDeleteSectionTitleTypes", Order = -10)]
        public async Task<IActionResult> DeleteSectionTitleTypeAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

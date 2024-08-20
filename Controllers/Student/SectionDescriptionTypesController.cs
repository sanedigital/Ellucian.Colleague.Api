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
    /// Provides access to SectionDescriptionTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionDescriptionTypesController : BaseCompressedApiController
    {
        private readonly ISectionDescriptionTypesService _sectionDescriptionTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SectionDescriptionTypesController class.
        /// </summary>
        /// <param name="sectionDescriptionTypesService">Service of type <see cref="ISectionDescriptionTypesService">ISectionDescriptionTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionDescriptionTypesController(ISectionDescriptionTypesService sectionDescriptionTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _sectionDescriptionTypesService = sectionDescriptionTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all SectionDescriptionTypes
        /// </summary>
        /// <returns>List of SectionDescriptionTypes <see cref="Dtos.SectionDescriptionTypes"/> objects representing matching SectionDescriptionTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/section-description-types", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSectionDescriptionTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.SectionDescriptionTypes>>> GetSectionDescriptionTypesAsync()
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
                var sectionDescriptionTypes = await _sectionDescriptionTypesService.GetSectionDescriptionTypesAsync(bypassCache);

                if (sectionDescriptionTypes != null && sectionDescriptionTypes.Any())
                {
                    AddEthosContextProperties(await _sectionDescriptionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _sectionDescriptionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              sectionDescriptionTypes.Select(a => a.Id).ToList()));
                }
                return Ok(sectionDescriptionTypes);
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
        /// Read (GET) a SectionDescriptionTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired SectionDescriptionTypes</param>
        /// <returns>A SectionDescriptionTypes object <see cref="Dtos.SectionDescriptionTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/section-description-types/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSectionDescriptionTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionDescriptionTypes>> GetSectionDescriptionTypeByGuidAsync(string guid)
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
                   await _sectionDescriptionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionDescriptionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _sectionDescriptionTypesService.GetSectionDescriptionTypeByGuidAsync(guid);
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
        /// Create (POST) a new SectionDescriptionTypes
        /// </summary>
        /// <param name="sectionDescriptionTypes">DTO of the new SectionDescriptionTypes</param>
        /// <returns>A SectionDescriptionTypes object <see cref="Dtos.SectionDescriptionTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/section-description-types", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSectionDescriptionTypes")]
        public async Task<ActionResult<Dtos.SectionDescriptionTypes>> PostSectionDescriptionTypesAsync([FromBody] Dtos.SectionDescriptionTypes sectionDescriptionTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing SectionDescriptionTypes
        /// </summary>
        /// <param name="guid">GUID of the SectionDescriptionTypes to update</param>
        /// <param name="sectionDescriptionTypes">DTO of the updated sectionDescriptionTypes</param>
        /// <returns>A sectionDescriptionTypes object <see cref="Dtos.SectionDescriptionTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/section-description-types/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSectionDescriptionTypes")]
        public async Task<ActionResult<Dtos.SectionDescriptionTypes>> PutSectionDescriptionTypesAsync([FromRoute] string guid, [FromBody] Dtos.SectionDescriptionTypes sectionDescriptionTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a sectionDescriptionTypes
        /// </summary>
        /// <param name="guid">GUID to desired sectionDescriptionTypes</param>
        [HttpDelete]
        [Route("/section-description-types/{guid}", Name = "DeleteSectionDescriptionTypes", Order = -10)]
        public async Task<IActionResult> DeleteSectionDescriptionTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

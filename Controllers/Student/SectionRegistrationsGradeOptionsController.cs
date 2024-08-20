// Copyright 2021-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
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
    /// Provides access to SectionRegistrationsGradeOptions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionRegistrationsGradeOptionsController : BaseCompressedApiController
    {
        private readonly ISectionRegistrationService _sectionRegistrationService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SectionRegistrationsGradeOptionsController class.
        /// </summary>
        /// <param name="sectionRegistrationService">Service of type <see cref="ISectionRegistrationService">ISectionRegistrationService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionRegistrationsGradeOptionsController(ISectionRegistrationService sectionRegistrationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _sectionRegistrationService = sectionRegistrationService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all section registrations grade options.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.SectionRegistrationsGradeOptions))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HttpGet]
        [HeaderVersionRoute("/section-registrations", "1.0.0", false, RouteConstants.HedtechIntegrationSectionRegistrationGradeOptionsFormat, Name = "GetSectionRegistrationsGradeOptionsV1", IsEedmSupported = true)]
        public async Task<IActionResult> GetSectionRegistrationsGradeOptionsAsync(Paging page, QueryStringFilter criteria)
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
                _sectionRegistrationService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.SectionRegistrationsGradeOptions>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.SectionRegistrationsGradeOptions>>(new List<Dtos.SectionRegistrationsGradeOptions>(), page, 0,
                        HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);


                Tuple<IEnumerable<Dtos.SectionRegistrationsGradeOptions>, int> pageOfItems = 
                    await _sectionRegistrationService.GetSectionRegistrationsGradeOptionsAsync(page.Offset, page.Limit, criteriaObj, bypassCache);

                AddEthosContextProperties(
                  await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.SectionRegistrationsGradeOptions>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a sectionRegistrationsGradeOptions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired sectionRegistrationsGradeOptions</param>
        /// <returns>A sectionRegistrationsGradeOptions object <see cref="Dtos.SectionRegistrationsGradeOptions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations })]
        [HttpGet]
        [HeaderVersionRoute("/section-registrations/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationSectionRegistrationGradeOptionsFormat, Name = "GetSectionRegistrationsGradeOptionsByGuidV1", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionRegistrationsGradeOptions>> GetSectionRegistrationsGradeOptionsByGuidAsync(string guid)
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
                _sectionRegistrationService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _sectionRegistrationService.GetSectionRegistrationsGradeOptionsByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new sectionRegistrationsGradeOptions
        /// </summary>
        /// <param name="sectionRegistrationsGradeOptions">DTO of the new sectionRegistrationsGradeOptions</param>
        /// <returns>A sectionRegistrationsGradeOptions object <see cref="Dtos.SectionRegistrationsGradeOptions"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/section-registrations", "1.0.0", false, RouteConstants.HedtechIntegrationSectionRegistrationGradeOptionsFormat, Name = "PostSectionRegistrationsGradeOptionsV1")]
        public async Task<ActionResult<Dtos.SectionRegistrationsGradeOptions>> PostSectionRegistrationsGradeOptionsAsync([FromBody] Dtos.SectionRegistrationsGradeOptions sectionRegistrationsGradeOptions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException("The Create or Update operation is only available using the default representation for section-registrations.", HttpStatusCode.NotAcceptable);

        }

        /// <summary>
        /// Update (PUT) an existing sectionRegistrationsGradeOptions
        /// </summary>
        /// <param name="guid">GUID of the sectionRegistrationsGradeOptions to update</param>
        /// <param name="sectionRegistrationsGradeOptions">DTO of the updated sectionRegistrationsGradeOptions</param>
        /// <returns>A sectionRegistrationsGradeOptions object <see cref="Dtos.SectionRegistrationsGradeOptions"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/section-registrations/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationSectionRegistrationGradeOptionsFormat, Name = "PutSectionRegistrationsGradeOptionsV1")]
        public async Task<ActionResult<Dtos.SectionRegistrationsGradeOptions>> PutSectionRegistrationsGradeOptionsAsync([FromRoute] string guid, [FromBody] Dtos.SectionRegistrationsGradeOptions sectionRegistrationsGradeOptions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException("The Create or Update operation is only available using the default representation for section-registrations.", HttpStatusCode.NotAcceptable);

        }

        /// <summary>
        /// Delete (DELETE) a sectionRegistrationsGradeOptions
        /// </summary>
        /// <param name="guid">GUID to desired sectionRegistrationsGradeOptions</param>
        [HttpDelete]
        public async Task<IActionResult> DeleteSectionRegistrationsGradeOptionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

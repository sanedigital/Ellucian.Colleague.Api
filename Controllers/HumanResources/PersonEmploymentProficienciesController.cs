// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources;
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


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PersonEmploymentProficiencies
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonEmploymentProficienciesController : BaseCompressedApiController
    {
        private readonly IPersonEmploymentProficienciesService _personEmploymentProficienciesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonEmploymentProficienciesController class.
        /// </summary>
        /// <param name="personEmploymentProficienciesService">Service of type <see cref="IPersonEmploymentProficienciesService">IPersonEmploymentProficienciesService</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="apiSettings"></param>
        public PersonEmploymentProficienciesController(ILogger logger, IPersonEmploymentProficienciesService personEmploymentProficienciesService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _personEmploymentProficienciesService = personEmploymentProficienciesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all personEmploymentProficiencies
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of PersonEmploymentProficiencies <see cref="Dtos.PersonEmploymentProficiencies"/> objects representing matching personEmploymentProficiencies</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), PermissionsFilter(HumanResourcesPermissionCodes.ViewPersonEmpProficiencies)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/person-employment-proficiencies", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonEmploymentProficiencies", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonEmploymentProficienciesAsync(Paging page)
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
                _personEmploymentProficienciesService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _personEmploymentProficienciesService.GetPersonEmploymentProficienciesAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                   await _personEmploymentProficienciesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personEmploymentProficienciesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonEmploymentProficiencies>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a personEmploymentProficiencies using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personEmploymentProficiencies</param>
        /// <returns>A personEmploymentProficiencies object <see cref="Dtos.PersonEmploymentProficiencies"/> in EEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(HumanResourcesPermissionCodes.ViewPersonEmpProficiencies)]
        [HttpGet]
        [HeaderVersionRoute("/person-employment-proficiencies/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonEmploymentProficienciesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonEmploymentProficiencies>> GetPersonEmploymentProficienciesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
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
                _personEmploymentProficienciesService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                  await _personEmploymentProficienciesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personEmploymentProficienciesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));

                return await _personEmploymentProficienciesService.GetPersonEmploymentProficienciesByGuidAsync(guid);
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
        /// Create (POST) a new personEmploymentProficiencies
        /// </summary>
        /// <param name="personEmploymentProficiencies">DTO of the new personEmploymentProficiencies</param>
        /// <returns>A personEmploymentProficiencies object <see cref="Dtos.PersonEmploymentProficiencies"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/person-employment-proficiencies", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonEmploymentProficienciesV10")]
        public async Task<ActionResult<Dtos.PersonEmploymentProficiencies>> PostPersonEmploymentProficienciesAsync([FromBody] Dtos.PersonEmploymentProficiencies personEmploymentProficiencies)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personEmploymentProficiencies
        /// </summary>
        /// <param name="guid">GUID of the personEmploymentProficiencies to update</param>
        /// <param name="personEmploymentProficiencies">DTO of the updated personEmploymentProficiencies</param>
        /// <returns>A personEmploymentProficiencies object <see cref="Dtos.PersonEmploymentProficiencies"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/person-employment-proficiencies/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonEmploymentProficienciesV10")]
        public async Task<ActionResult<Dtos.PersonEmploymentProficiencies>> PutPersonEmploymentProficienciesAsync([FromRoute] string guid, [FromBody] Dtos.PersonEmploymentProficiencies personEmploymentProficiencies)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personEmploymentProficiencies
        /// </summary>
        /// <param name="guid">GUID to desired personEmploymentProficiencies</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/person-employment-proficiencies/{guid}", Name = "DefaultDeletePersonEmploymentProficiencies")]
        public async Task<IActionResult> DeletePersonEmploymentProficienciesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

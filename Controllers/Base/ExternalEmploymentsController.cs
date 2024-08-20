// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to ExternalEmployments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ExternalEmploymentsController : BaseCompressedApiController
    {
        private readonly IExternalEmploymentsService _externalEmploymentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ExternalEmploymentsController class.
        /// </summary>
        /// <param name="externalEmploymentsService">Service of type <see cref="IExternalEmploymentsService">IExternalEmploymentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ExternalEmploymentsController(IExternalEmploymentsService externalEmploymentsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _externalEmploymentsService = externalEmploymentsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all externalEmployments
        /// </summary>

        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of ExternalEmployments <see cref="Dtos.ExternalEmployments"/> objects representing matching externalEmployments</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(BasePermissionCodes.ViewAnyExternalEmployments)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/external-employments", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetExternalEmployments", IsEedmSupported = true)]
        public async Task<IActionResult> GetExternalEmploymentsAsync(Paging page)
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
                _externalEmploymentsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _externalEmploymentsService.GetExternalEmploymentsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _externalEmploymentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _externalEmploymentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.ExternalEmployments>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a externalEmployments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired externalEmployments</param>
        /// <returns>A externalEmployments object <see cref="Dtos.ExternalEmployments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.ViewAnyExternalEmployments)]
        [HttpGet]
        [HeaderVersionRoute("/external-employments/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetExternalEmploymentsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.ExternalEmployments>> GetExternalEmploymentsByGuidAsync(string guid)
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
                _externalEmploymentsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _externalEmploymentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _externalEmploymentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _externalEmploymentsService.GetExternalEmploymentsByGuidAsync(guid);
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
        /// Create (POST) a new externalEmployments
        /// </summary>
        /// <param name="externalEmployments">DTO of the new externalEmployments</param>
        /// <returns>A externalEmployments object <see cref="Dtos.ExternalEmployments"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/external-employments", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostExternalEmploymentsV10")]
        public async Task<ActionResult<Dtos.ExternalEmployments>> PostExternalEmploymentsAsync([FromBody] Dtos.ExternalEmployments externalEmployments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing externalEmployments
        /// </summary>
        /// <param name="guid">GUID of the externalEmployments to update</param>
        /// <param name="externalEmployments">DTO of the updated externalEmployments</param>
        /// <returns>A externalEmployments object <see cref="Dtos.ExternalEmployments"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/external-employments/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutExternalEmploymentsV10")]
        public async Task<ActionResult<Dtos.ExternalEmployments>> PutExternalEmploymentsAsync([FromRoute] string guid, [FromBody] Dtos.ExternalEmployments externalEmployments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a externalEmployments
        /// </summary>
        /// <param name="guid">GUID to desired externalEmployments</param>
        [HttpDelete]
        [Route("/external-employments/{guid}", Name = "DefaultDeleteExternalEmployments", Order = -10)]
        public async Task<IActionResult> DeleteExternalEmploymentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

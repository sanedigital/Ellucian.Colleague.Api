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

using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionApplicationSupportingItems
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationSupportingItemsController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationSupportingItemsService _admissionApplicationSupportingItemsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationSupportingItemsController class.
        /// </summary>
        /// <param name="admissionApplicationSupportingItemsService">Service of type <see cref="IAdmissionApplicationSupportingItemsService">IAdmissionApplicationSupportingItemsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdmissionApplicationSupportingItemsController(IAdmissionApplicationSupportingItemsService admissionApplicationSupportingItemsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _admissionApplicationSupportingItemsService = admissionApplicationSupportingItemsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all admissionApplicationSupportingItems
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">Filtering Criteria</param>
        /// <returns>List of AdmissionApplicationSupportingItems <see cref="Dtos.AdmissionApplicationSupportingItems"/> objects representing matching admissionApplicationSupportingItems</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplicationSupportingItems, StudentPermissionCodes.UpdateApplicationSupportingItems })]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AdmissionApplicationSupportingItems))]
        [HttpGet]
        [HeaderVersionRoute("/admission-application-supporting-items", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplicationSupportingItems", IsEedmSupported = true)]
        public async Task<IActionResult> GetAdmissionApplicationSupportingItemsAsync(Paging page, QueryStringFilter criteria = null)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (CheckForEmptyFilterParameters())
            {
                return new PagedActionResult<IEnumerable<Dtos.AdmissionApplication3>>(new List<Dtos.AdmissionApplication3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

            var filterCriteria = GetFilterObject<Dtos.AdmissionApplicationSupportingItems>(_logger, "criteria");
            try
            {
                _admissionApplicationSupportingItemsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _admissionApplicationSupportingItemsService.GetAdmissionApplicationSupportingItemsAsync(page.Offset, page.Limit, bypassCache, filterCriteria);

                AddEthosContextProperties(
                    await _admissionApplicationSupportingItemsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _admissionApplicationSupportingItemsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.AdmissionApplicationSupportingItems>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a admissionApplicationSupportingItems using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItems</param>
        /// <returns>A admissionApplicationSupportingItems object <see cref="Dtos.AdmissionApplicationSupportingItems"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplicationSupportingItems, StudentPermissionCodes.UpdateApplicationSupportingItems })]
        [HttpGet]
        [HeaderVersionRoute("/admission-application-supporting-items/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplicationSupportingItemsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplicationSupportingItems>> GetAdmissionApplicationSupportingItemsByGuidAsync(string guid)
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
                _admissionApplicationSupportingItemsService.ValidatePermissions(GetPermissionsMetaData());
                var returnval = await _admissionApplicationSupportingItemsService.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
                AddEthosContextProperties(
                                        await _admissionApplicationSupportingItemsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                        await _admissionApplicationSupportingItemsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));
                return returnval;
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
        /// Update (PUT) an existing AdmissionApplicationSupportingItems
        /// </summary>
        /// <param name="guid">GUID of the admissionApplicationSupportingItems to update</param>
        /// <param name="admissionApplicationSupportingItems">DTO of the updated admissionApplicationSupportingItems</param>
        /// <returns>A AdmissionApplicationSupportingItems object <see cref="Dtos.AdmissionApplicationSupportingItems"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.UpdateApplicationSupportingItems)]
        [HttpPut]
        [HeaderVersionRoute("/admission-application-supporting-items/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionApplicationSupportingItemsV12", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplicationSupportingItems>> PutAdmissionApplicationSupportingItemsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplicationSupportingItems admissionApplicationSupportingItems)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (admissionApplicationSupportingItems == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null admissionApplicationSupportingItems argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(admissionApplicationSupportingItems.Id))
            {
                admissionApplicationSupportingItems.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, admissionApplicationSupportingItems.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _admissionApplicationSupportingItemsService.ValidatePermissions(GetPermissionsMetaData());
                await _admissionApplicationSupportingItemsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationSupportingItemsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var returnval =  await _admissionApplicationSupportingItemsService.UpdateAdmissionApplicationSupportingItemsAsync(
                  await PerformPartialPayloadMerge(admissionApplicationSupportingItems, async () => await _admissionApplicationSupportingItemsService.GetAdmissionApplicationSupportingItemsByGuidAsync(guid, true),
                  await _admissionApplicationSupportingItemsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                  _logger));

                AddEthosContextProperties(
                                            await _admissionApplicationSupportingItemsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                                            await _admissionApplicationSupportingItemsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));
                return returnval;
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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new admissionApplicationSupportingItems
        /// </summary>
        /// <param name="admissionApplicationSupportingItems">DTO of the new admissionApplicationSupportingItems</param>
        /// <returns>A admissionApplicationSupportingItems object <see cref="Dtos.AdmissionApplicationSupportingItems"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.UpdateApplicationSupportingItems)]
        [HttpPost]
        [HeaderVersionRoute("/admission-application-supporting-items", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionApplicationSupportingItemsV12", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplicationSupportingItems>> PostAdmissionApplicationSupportingItemsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplicationSupportingItems admissionApplicationSupportingItems)
        {
            if (admissionApplicationSupportingItems == null)
            {
                return CreateHttpResponseException("Request body must contain a valid admissionApplicationSupportingItems.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(admissionApplicationSupportingItems.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null admissionApplicationSupportingItems id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (!admissionApplicationSupportingItems.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            try
            {
                _admissionApplicationSupportingItemsService.ValidatePermissions(GetPermissionsMetaData());
                await _admissionApplicationSupportingItemsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationSupportingItemsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var returnval = await _admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);

                AddEthosContextProperties(
                                           await _admissionApplicationSupportingItemsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                                           await _admissionApplicationSupportingItemsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { returnval.Id }));
                return returnval;
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
            catch (ConfigurationException e)
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
        /// Delete (DELETE) a admissionApplicationSupportingItems
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItems</param>
        [HttpDelete]
        [Route("/admission-application-supporting-items/{guid}", Name = "DefaultDeleteAdmissionApplicationSupportingItems")]
        public async Task<IActionResult> DeleteAdmissionApplicationSupportingItemsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}

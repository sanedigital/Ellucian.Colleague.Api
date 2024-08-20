// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
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
    /// Provides access to AdmissionApplications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationsController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationsService _admissionApplicationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationsController class.
        /// </summary>
        /// <param name="admissionApplicationsService">Service of type <see cref="IAdmissionApplicationsService">IAdmissionApplicationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdmissionApplicationsController(IAdmissionApplicationsService admissionApplicationsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _admissionApplicationsService = admissionApplicationsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all admissionApplications
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications })]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 200 })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/admission-applications", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAdmissionApplicationsV6", IsEedmSupported = true)]
        public async Task<IActionResult> GetAdmissionApplicationsAsync(Paging page)
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
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }


                var pageOfItems = await _admissionApplicationsService.GetAdmissionApplicationsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                  await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.AdmissionApplication>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Return all admissionApplications
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications })]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 200 })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/admission-applications", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAdmissionApplicationsV11", IsEedmSupported = true)]
        public async Task<IActionResult> GetAdmissionApplications2Async(Paging page)
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
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _admissionApplicationsService.GetAdmissionApplications2Async(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                  await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.AdmissionApplication2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Return all admissionApplications
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria">Filtering Criteria</param>
        /// <param name="personFilter">PersonFilter Named Query</param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications })]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 200 })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AdmissionApplication3))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [HeaderVersionRoute("/admission-applications", "16.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplications", IsEedmSupported = true)]
        public async Task<IActionResult> GetAdmissionApplications3Async(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var filterCriteria = GetFilterObject<Dtos.AdmissionApplication3>(_logger, "criteria");
            var personFilterFilter = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");

            if (CheckForEmptyFilterParameters())
            {
                return new PagedActionResult<IEnumerable<Dtos.AdmissionApplication3>>(new List<Dtos.AdmissionApplication3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

            try
            {
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _admissionApplicationsService.GetAdmissionApplications3Async(page.Offset, page.Limit, filterCriteria, personFilterFilter, bypassCache);

                AddEthosContextProperties(
                  await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.AdmissionApplication3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Read (GET) a admissionApplications using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication"/> in EEDM format</returns>
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/admission-applications/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAdmissionApplicationsByGuidV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplication>> GetAdmissionApplicationsByGuidAsync(string guid)
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
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));
                return await _admissionApplicationsService.GetAdmissionApplicationsByGuidAsync(guid);
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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Read (GET) a admissionApplications using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication2"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications })]
        [HeaderVersionRoute("/admission-applications/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAdmissionApplicationsByGuidV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplication2>> GetAdmissionApplicationsByGuid2Async(string guid)
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
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                     await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { guid }));
                return await _admissionApplicationsService.GetAdmissionApplicationsByGuid2Async(guid);
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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Read (GET) a admissionApplications using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication3"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications })]
        [HeaderVersionRoute("/admission-applications/{guid}", "16.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplicationsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplication3>> GetAdmissionApplicationsByGuid3Async(string guid)
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
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                     await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { guid }));
                return await _admissionApplicationsService.GetAdmissionApplicationsByGuid3Async(guid);
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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Create (POST) a new admissionApplications
        /// </summary>
        /// <param name="admissionApplications">DTO of the new admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/admission-applications", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionApplicationsV6")]
        [HeaderVersionRoute("/admission-applications", "16.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionApplicationsV16_1_0", Order = -5)]
        public async Task<ActionResult<Dtos.AdmissionApplication>> PostAdmissionApplicationsAsync([FromBody] Dtos.AdmissionApplication admissionApplications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Create (POST) a new admissionApplication
        /// </summary>
        /// <param name="admissionApplication">DTO of the new admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication2"/> in HeDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.UpdateApplications)]
        [HeaderVersionRoute("/admission-applications", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionApplicationsV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplication2>> PostAdmissionApplications2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplication2 admissionApplication)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (admissionApplication == null)
            {
                return CreateHttpResponseException("Request body must contain a valid admission application.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(admissionApplication.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null admissionApplications id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (admissionApplication.Id != Guid.Empty.ToString())
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentNullException("admissionApplicationsDto", "On a post you can not define a GUID.")));
            }

            try
            {
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _admissionApplicationsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var admissionApplicationReturn = await _admissionApplicationsService.CreateAdmissionApplicationAsync(admissionApplication, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _admissionApplicationsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { admissionApplicationReturn.Id }));

                return Ok(admissionApplicationReturn);
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
        /// Update (PUT) an existing admissionApplications
        /// </summary>
        /// <param name="guid">GUID of the admissionApplications to update</param>
        /// <param name="admissionApplications">DTO of the updated admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/admission-applications/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionApplicationsV6", Order = -5)]
        [HeaderVersionRoute("/admission-applications/{guid}", "16.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionApplicationsV16_1_0", Order = -5)]
        public async Task<ActionResult<Dtos.AdmissionApplication>> PutAdmissionApplicationsAsync([FromRoute] string guid, [FromBody] Dtos.AdmissionApplication admissionApplications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing AdmissionApplication
        /// </summary>
        /// <param name="guid">GUID of the admissionApplications to update</param>
        /// <param name="admissionApplication">DTO of the updated admissionApplications</param>
        /// <returns>A AdmissionApplications object <see cref="Dtos.AdmissionApplication2"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.UpdateApplications)]
        [HeaderVersionRoute("/admission-applications/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionApplicationsV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AdmissionApplication2>> PutAdmissionApplications2Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplication2 admissionApplication)
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
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (admissionApplication == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null admissionApplications argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(admissionApplication.Id))
            {
                admissionApplication.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, admissionApplication.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _admissionApplicationsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _admissionApplicationsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var admissionApplicationReturn = await _admissionApplicationsService.UpdateAdmissionApplicationAsync(guid,
                  await PerformPartialPayloadMerge(admissionApplication, async () => await _admissionApplicationsService.GetAdmissionApplicationsByGuid2Async(guid),
                  dpList, _logger), bypassCache);

                AddEthosContextProperties(dpList,
                    await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));


                return admissionApplicationReturn;
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
        /// Delete (DELETE) a admissionApplications
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplications</param>
        [HttpDelete]
        [Route("/admission-applications/{guid}", Name = "DefaultDeleteAdmissionApplications", Order = -10)]
        public async Task<IActionResult> DeleteAdmissionApplicationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #region  Admission Applications Submissions

        /// <summary>
        /// Create (POST) a new admissionApplicationSubmission
        /// </summary>
        /// <param name="admissionApplication">DTO of the new admissionApplicationsSubmissions</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplicationSubmission"/> in HeDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ContentTypeConstraint( contentTypes: new[] { RouteConstants.HedtechIntegrationAdmissionApplicationsSubmissionsFormat , RouteConstants.HedtechIntegrationAdmissionApplicationsSubmissionsFormat }
                              , versions : new[] { "1.0.0", "1.1.0"})]
        [HeaderVersionRoute("/admission-applications", "16.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionApplicationsSubmissionsV16_1_0", IsEedmSupported = true, Order = -15)]
        public async Task<ActionResult<Dtos.AdmissionApplication3>> PostAdmissionApplicationsSubmissionsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplicationSubmission admissionApplication)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (admissionApplication == null)
            {
                return CreateHttpResponseException("Request body must contain a valid admission application submission.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(admissionApplication.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null admissionApplicationsSubmission id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _admissionApplicationsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var admissionApplicationReturn = await _admissionApplicationsService.CreateAdmissionApplicationsSubmissionAsync(admissionApplication, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _admissionApplicationsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { admissionApplicationReturn.Id }));

                return admissionApplicationReturn;
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
        /// Update (PUT) an existing AdmissionApplicationsSubmissions
        /// </summary>
        /// <param name="guid">GUID of the admissionApplications to update</param>
        /// <param name="admissionApplicationsSubmissions">DTO of the updated admissionApplications</param>
        /// <returns>A AdmissionApplications object <see cref="Dtos.AdmissionApplication3"/> in EEDM format</returns>
        [HttpPut]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ContentTypeConstraint(contentTypes: new[] { RouteConstants.HedtechIntegrationAdmissionApplicationsSubmissionsFormat, RouteConstants.HedtechIntegrationAdmissionApplicationsSubmissionsFormat }
                              , versions: new[] { "1.0.0", "1.1.0"})]
        [HeaderVersionRoute("/admission-applications/{guid}", "16.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionApplicationsSubmissionsV16_1_0", IsEedmSupported = true, Order = -15)]
        public async Task<ActionResult<Dtos.AdmissionApplication3>> PutAdmissionApplicationsSubmissionsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplicationSubmission admissionApplicationsSubmissions)
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
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (admissionApplicationsSubmissions == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null admissionApplicationsSubmissions argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            if (string.IsNullOrEmpty(admissionApplicationsSubmissions.Id))
            {
                admissionApplicationsSubmissions.Id = guid.ToLowerInvariant();
            }

            try
            {
                //get Data Privacy List
                var dpList = await _admissionApplicationsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _admissionApplicationsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var admissionApplicationSubmissionsOrig = await _admissionApplicationsService.GetAdmissionApplicationsSubmissionsByGuidAsync(guid, true);

                var mergedAdmissionApplicationSubmissions = await PerformPartialPayloadMerge(admissionApplicationsSubmissions, admissionApplicationSubmissionsOrig, dpList, _logger);

                // Error if attempt is made to unset an existing educational goal.
                if (admissionApplicationSubmissionsOrig != null)
                {
                    if (admissionApplicationSubmissionsOrig.EducationalGoal != null && !string.IsNullOrEmpty(admissionApplicationSubmissionsOrig.EducationalGoal.Id))
                    {
                        if (mergedAdmissionApplicationSubmissions != null)
                        {
                            if (mergedAdmissionApplicationSubmissions.EducationalGoal == null || string.IsNullOrEmpty(mergedAdmissionApplicationSubmissions.EducationalGoal.Id))
                            {
                                throw new IntegrationApiException("Missing educationalGoal",
                                    IntegrationApiUtility.GetDefaultApiError("The educationalGoal is stored to APP.ORIG.EDUC.GOAL and may not be unset."));
                            }
                        }
                    }
                }

                var admissionApplicationReturn = await _admissionApplicationsService.UpdateAdmissionApplicationsSubmissionAsync(guid,
                    mergedAdmissionApplicationSubmissions, bypassCache);

                AddEthosContextProperties(dpList,
                    await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));


                return admissionApplicationReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
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
        #endregion
    }
}

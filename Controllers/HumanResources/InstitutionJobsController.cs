// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Newtonsoft.Json;
using Ellucian.Colleague.Domain.Base.Exceptions;

using Ellucian.Web.Http.ModelBinding;
using Newtonsoft.Json.Linq;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to InstitutionJobs
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class InstitutionJobsController : BaseCompressedApiController
    {
        private readonly IInstitutionJobsService _institutionJobsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstitutionJobsController class.
        /// </summary>
        /// <param name="institutionJobsService">Service of type <see cref="IInstitutionJobsService">IInstitutionJobsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public InstitutionJobsController(IInstitutionJobsService institutionJobsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _institutionJobsService = institutionJobsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all InstitutionJobs
        /// </summary>
        /// <returns>List of InstitutionJobs <see cref="Dtos.InstitutionJobs"/> objects representing matching institutionJobs</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewInstitutionJob, HumanResourcesPermissionCodes.CreateInstitutionJob })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.InstitutionJobs))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HttpGet]
        [HeaderVersionRoute("/institution-jobs", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstitutionJobsV8", IsEedmSupported = true)]
        public async Task<IActionResult> GetInstitutionJobsAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;

            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }
            string person = string.Empty, employer = string.Empty, position = string.Empty, department = string.Empty,
                startOn = string.Empty, endOn = string.Empty, status = string.Empty, classification = string.Empty,
                preference = string.Empty;

            var criteriaValues = GetFilterObject<Dtos.InstitutionJobs>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.InstitutionJobs>>(new List<Dtos.InstitutionJobs>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            if (criteriaValues != null)
            {
                if (criteriaValues.Person != null && !string.IsNullOrEmpty(criteriaValues.Person.Id))
                    person = criteriaValues.Person.Id;
                if (criteriaValues.Employer != null && !string.IsNullOrEmpty(criteriaValues.Employer.Id))
                    employer = criteriaValues.Employer.Id;
                if (criteriaValues.Position != null && !string.IsNullOrEmpty(criteriaValues.Position.Id))
                    position = criteriaValues.Position.Id;
                if (criteriaValues.Department != null && !string.IsNullOrEmpty(criteriaValues.Department))
                    department = criteriaValues.Department;
                if (criteriaValues.StartOn != null && criteriaValues.StartOn != default(DateTime))
                    startOn = criteriaValues.StartOn.ToShortDateString();
                if (criteriaValues.EndOn != null)
                    endOn = criteriaValues.EndOn.Value.ToShortDateString();
                if (criteriaValues.Status != null)
                    status = criteriaValues.Status.ToString();
                if (criteriaValues.Classification != null && !string.IsNullOrEmpty(criteriaValues.Classification.Id))
                    classification = criteriaValues.Classification.Id;
                if (criteriaValues.Preference != null && criteriaValues.Preference != Dtos.EnumProperties.JobPreference.NotSet)
                    preference = criteriaValues.Preference.ToString();
            }
            try
            {
                _institutionJobsService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _institutionJobsService.GetInstitutionJobsAsync(page.Offset, page.Limit, person, employer, position,
                    department, startOn, endOn, status, classification, preference, bypassCache);
               
                AddEthosContextProperties(
                  await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.InstitutionJobs>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch
                (KeyNotFoundException e)
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
        /// Return all InstitutionJobs
        /// </summary>
        /// <returns>List of InstitutionJobs <see cref="Dtos.InstitutionJobs2"/> objects representing matching institutionJobs</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewInstitutionJob, HumanResourcesPermissionCodes.CreateInstitutionJob })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.InstitutionJobs2))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HttpGet]
        [HeaderVersionRoute("/institution-jobs", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstitutionJobsV11", IsEedmSupported = true)]
        public async Task<IActionResult> GetInstitutionJobs2Async(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;

            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }
            string person = string.Empty, employer = string.Empty, position = string.Empty, department = string.Empty,
                startOn = string.Empty, endOn = string.Empty, status = string.Empty, classification = string.Empty,
                preference = string.Empty;

            var criteriaValues = GetFilterObject<Dtos.InstitutionJobs2>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.InstitutionJobs2>>(new List<Dtos.InstitutionJobs2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            if (criteriaValues != null)
            {
                if (criteriaValues.Person != null && !string.IsNullOrEmpty(criteriaValues.Person.Id))
                    person = criteriaValues.Person.Id;
                if (criteriaValues.Employer != null && !string.IsNullOrEmpty(criteriaValues.Employer.Id))
                    employer = criteriaValues.Employer.Id;
                if (criteriaValues.Position != null && !string.IsNullOrEmpty(criteriaValues.Position.Id))
                    position = criteriaValues.Position.Id;
                if (criteriaValues.Department != null && !string.IsNullOrEmpty(criteriaValues.Department))
                    department = criteriaValues.Department;
                if (criteriaValues.StartOn != null && criteriaValues.StartOn != default(DateTime))
                    startOn = criteriaValues.StartOn.ToShortDateString();
                if (criteriaValues.Status != null)
                    status = criteriaValues.Status.ToString();
                if (criteriaValues.EndOn != null)
                    endOn = criteriaValues.EndOn.Value.ToShortDateString();
                if (criteriaValues.Classification != null && !string.IsNullOrEmpty(criteriaValues.Classification.Id))
                    classification = criteriaValues.Classification.Id;
                if (criteriaValues.Preference != Dtos.EnumProperties.JobPreference.NotSet)
                    preference = criteriaValues.Preference.ToString();
            }
            try
            {
                _institutionJobsService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _institutionJobsService.GetInstitutionJobs2Async(page.Offset, page.Limit, person, employer, position,
                    department, startOn, endOn, status, classification, preference, bypassCache);

                AddEthosContextProperties(
                 await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch
                (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden );
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
        /// Return all InstitutionJobs
        /// </summary>
        /// <returns>List of InstitutionJobs <see cref="Dtos.InstitutionJobs3"/> objects representing matching institutionJobs</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewInstitutionJob, HumanResourcesPermissionCodes.CreateInstitutionJob })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.InstitutionJobs3))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HttpGet]
        [HeaderVersionRoute("/institution-jobs", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstitutionJobs", IsEedmSupported = true)]
        public async Task<IActionResult> GetInstitutionJobs3Async(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;

            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }
            string person = string.Empty, employer = string.Empty, position = string.Empty, department = string.Empty,
                startOn = string.Empty, endOn = string.Empty, status = string.Empty, classification = string.Empty,
                preference = string.Empty;

            var criteriaValues = GetFilterObject<Dtos.InstitutionJobs3>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.InstitutionJobs3>>(new List<Dtos.InstitutionJobs3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            var filterQualifiers = GetFilterQualifiers(_logger);

            if (criteriaValues != null)
            {
                if (criteriaValues.Person != null && !string.IsNullOrEmpty(criteriaValues.Person.Id))
                    person = criteriaValues.Person.Id;
                if (criteriaValues.Employer != null && !string.IsNullOrEmpty(criteriaValues.Employer.Id))
                    employer = criteriaValues.Employer.Id;
                if (criteriaValues.Position != null && !string.IsNullOrEmpty(criteriaValues.Position.Id))
                    position = criteriaValues.Position.Id;
                if (criteriaValues.Department != null && !string.IsNullOrEmpty(criteriaValues.Department.Id))
                    department = criteriaValues.Department.Id;
                if (criteriaValues.StartOn != null && criteriaValues.StartOn != default(DateTime))
                    startOn = criteriaValues.StartOn.ToShortDateString();
                if (criteriaValues.EndOn != null)
                    endOn = criteriaValues.EndOn.Value.ToShortDateString();
                if (criteriaValues.Status != null)
                    status = criteriaValues.Status.ToString();
                if (criteriaValues.Classification != null && !string.IsNullOrEmpty(criteriaValues.Classification.Id))
                    classification = criteriaValues.Classification.Id;
                if (criteriaValues.Preference != Dtos.EnumProperties.JobPreference2.NotSet)
                    preference = criteriaValues.Preference.ToString();
            }
            try
            {
                _institutionJobsService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _institutionJobsService.GetInstitutionJobs3Async(page.Offset, page.Limit, person, employer, position,
                    department, startOn, endOn, status, classification, preference, bypassCache, filterQualifiers);

                AddEthosContextProperties(
                 await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch
                (KeyNotFoundException e)
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
        /// Read (GET) an InstitutionJobs using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        /// <returns>An InstitutionJobs DTO object <see cref="Dtos.InstitutionJobs"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewInstitutionJob, HumanResourcesPermissionCodes.CreateInstitutionJob })]
        [HttpGet]
        [HeaderVersionRoute("/institution-jobs/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstitutionJobsByGuidV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.InstitutionJobs>> GetInstitutionJobsByGuidAsync(string guid)
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
                _institutionJobsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                  await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _institutionJobsService.GetInstitutionJobsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden );
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
        /// Read (GET) an InstitutionJobs using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        /// <returns>An InstitutionJobs DTO object <see cref="Dtos.InstitutionJobs2"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewInstitutionJob, HumanResourcesPermissionCodes.CreateInstitutionJob })]
        [HttpGet]
        [HeaderVersionRoute("/institution-jobs/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstitutionJobsByGuidV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.InstitutionJobs2>> GetInstitutionJobsByGuid2Async(string guid)
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
                _institutionJobsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                  await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _institutionJobsService.GetInstitutionJobsByGuid2Async(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden );
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
        /// Read (GET) an InstitutionJobs using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        /// <returns>An InstitutionJobs DTO object <see cref="Dtos.InstitutionJobs3"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewInstitutionJob, HumanResourcesPermissionCodes.CreateInstitutionJob })]
        [HttpGet]
        [HeaderVersionRoute("/institution-jobs/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstitutionJobsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.InstitutionJobs3>> GetInstitutionJobsByGuid3Async(string guid)
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
                _institutionJobsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                  await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _institutionJobsService.GetInstitutionJobsByGuid3Async(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden );
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
        /// Create (POST) a new institutionJobs
        /// </summary>
        /// <param name="institutionJobs">DTO of the new institutionJobs</param>
        /// <returns> V8 and V11 of institution jobs is not supported, Returns an error</returns>
        [HttpPost]
        [HeaderVersionRoute("/institution-jobs", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstitutionJobsV8")]
        public async Task<ActionResult<Dtos.InstitutionJobs>> PostInstitutionJobsAsync([FromBody] Dtos.InstitutionJobs institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create (POST) a new institutionJobs
        /// </summary>
        /// <param name="institutionJobs">DTO of the new institutionJobs</param>
        /// <returns> V8 and V11 of institution jobs is not supported, Returns an error</returns>
        [HttpPost]
        [HeaderVersionRoute("/institution-jobs", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstitutionJobsV11")]
        public async Task<ActionResult<Dtos.InstitutionJobs2>> PostInstitutionJobs2Async([FromBody] Dtos.InstitutionJobs2 institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create (POST) a new institutionJobs v12
        /// </summary>
        /// <param name="institutionJobs">DTO of the new institutionJobs</param>
        /// <returns>An InstitutionJobs DTO object <see cref="Dtos.InstitutionJobs3"/> in EEDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(HumanResourcesPermissionCodes.CreateInstitutionJob)]
        [HttpPost]
        [HeaderVersionRoute("/institution-jobs", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstitutionJobsV12", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.InstitutionJobs3>> PostInstitutionJobs3Async([ModelBinder(typeof(EedmModelBinder))] Dtos.InstitutionJobs3 institutionJobs)
        {
           
            if (institutionJobs == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null institutionJobs argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
          
            try
            {
                _institutionJobsService.ValidatePermissions(GetPermissionsMetaData());
                if (institutionJobs.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("institutionJobsDto", "Nil GUID must be used in POST operation.");
                }
                ValidateInstitutionJobs2(institutionJobs);

                //call import extend method that needs the extracted extension data and the config
                await _institutionJobsService.ImportExtendedEthosData(await ExtractExtendedData(await _institutionJobsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var institutionJobsReturn = await _institutionJobsService.PostInstitutionJobsAsync(institutionJobs);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _institutionJobsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { institutionJobsReturn.Id }));

                return institutionJobsReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                if (e.Errors == null || e.Errors.Count <= 0)
                {
                    return CreateHttpResponseException(e.Message);
                }
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
        /// Update (PUT) an existing institutionJobs
        /// </summary>
        /// <param name="guid">GUID of the institutionJobs to update</param>
        /// <param name="institutionJobs">DTO of the updated institutionJobs</param>
        /// <returns>V8 and V11 of institution jobs is not supported, Returns an error</returns>
        [HttpPut]
        [HeaderVersionRoute("/institution-jobs/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstitutionJobsV8")]
        public async Task<ActionResult<Dtos.InstitutionJobs>> PutInstitutionJobsAsync([FromRoute] string guid, [FromBody] Dtos.InstitutionJobs institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing institutionJobs
        /// </summary>
        /// <param name="guid">GUID of the institutionJobs to update</param>
        /// <param name="institutionJobs">DTO of the updated institutionJobs</param>
        /// <returns>V8 and V11 of institution jobs is not supported, Returns an error</returns>
        [HttpPut]
        [HeaderVersionRoute("/institution-jobs/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstitutionJobsV11")]
        public async Task<ActionResult<Dtos.InstitutionJobs2>> PutInstitutionJobs2Async([FromRoute] string guid, [FromBody] Dtos.InstitutionJobs2 institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing institutionJobs
        /// </summary>
        /// <param name="guid">GUID of the institutionJobs to update</param>
        /// <param name="institutionJobs">DTO of the updated institutionJobs</param>
        /// <returns>An InstitutionJobs DTO object <see cref="Dtos.InstitutionJobs3"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(HumanResourcesPermissionCodes.CreateInstitutionJob)]
        [HttpPut]
        [HeaderVersionRoute("/institution-jobs/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstitutionJobsV12", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.InstitutionJobs3>> PutInstitutionJobs3Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.InstitutionJobs3 institutionJobs)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (institutionJobs == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null institutionJobs argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(institutionJobs.Id))
            {
                institutionJobs.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(institutionJobs.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != institutionJobs.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _institutionJobsService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _institutionJobsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);    
                            
                //call import extend method that needs the extracted extension dataa and the config
                await _institutionJobsService.ImportExtendedEthosData(await ExtractExtendedData(await _institutionJobsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var institutionJobsReturn = await _institutionJobsService.PutInstitutionJobsAsync(
                  await PerformPartialPayloadMerge(institutionJobs, async () => await _institutionJobsService.GetInstitutionJobsByGuid3Async(guid, true),
                  dpList, _logger));

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(dpList,
                   await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { institutionJobsReturn.Id }));

                return institutionJobsReturn;

            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                if (e.Errors == null || e.Errors.Count <= 0)
                {
                    return CreateHttpResponseException(e.Message);
                }
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
        /// Delete (DELETE) a institutionJobs
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        [HttpDelete]
        [Route("/institution-jobs/{guid}", Name = "DefaultDeleteInstitutionJobs", Order = -10)]
        public async Task<IActionResult> DeleteInstitutionJobsAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Helper method to validate Institution-Jobs PUT/POST.
        /// </summary>
        /// <param name="institutionJobs"><see cref="Dtos.InstitutionJobs"/>InstitutionJobs DTO object of type</param>
        private void ValidateInstitutionJobs(Dtos.InstitutionJobs institutionJobs)
        {

            if (institutionJobs == null)
            {
                throw new ArgumentNullException("institutionJobs", "The body is required when submitting an institutionJobs. ");
            }

        }

        /// <summary>
        /// Helper method to validate Institution-Jobs PUT/POST.
        /// </summary>
        /// <param name="institutionJobs"><see cref="Dtos.InstitutionJobs3"/>InstitutionJobs DTO object of type</param>
        private void ValidateInstitutionJobs2(Dtos.InstitutionJobs3 institutionJobs)
        {
            if (institutionJobs == null)
            {
                throw new ArgumentNullException("institutionJobs", "The body is required when submitting an institutionJobs. ");
            }

        }

    }
}

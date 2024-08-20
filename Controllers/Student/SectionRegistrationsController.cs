// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;

using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Web.Http.ModelBinding;

using System.Net;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to SectionRegistration
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionRegistrationsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ISectionRegistrationService _sectionRegistrationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// SectionRegistrationStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="sectionRegistrationService">Service of type <see cref="ICurriculumService">ISectionRegistrationService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionRegistrationsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ISectionRegistrationService sectionRegistrationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _sectionRegistrationService = sectionRegistrationService;
            this._logger = logger;
        }

        #region Get Methods

        #region section-registrations V16.0.0

        /// <summary>
        /// Get section registration get by guid.
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations })]
        [HeaderVersionRoute("/section-registrations/{guid}", "16.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmSectionRegistration", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionRegistration4>> GetSectionRegistrationByGuid3Async([FromRoute] string guid)
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
                var sectionRegistration = await _sectionRegistrationService.GetSectionRegistrationByGuid3Async(guid);

                if (sectionRegistration != null)
                {

                    AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { sectionRegistration.Id }));
                }

                return sectionRegistration;
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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
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
        /// Gets section registrations with filter V16.0.0.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <param name="academicPeriod"></param>
        /// <param name="sectionInstructor"></param>
        /// <param name="registrationStatusesByAcademicPeriod"></param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.SectionRegistration4))]
        [QueryStringFilterFilter("academicPeriod", typeof(Dtos.Filters.AcademicPeriodNamedQueryFilter))]
        [QueryStringFilterFilter("sectionInstructor", typeof(Dtos.Filters.SectionInstructorQueryFilter))]
        [QueryStringFilterFilter("registrationStatusesByAcademicPeriod", typeof(Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [HeaderVersionRoute("/section-registrations", "16.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmSectionRegistrations", IsEedmSupported = true, IsBulkSupported = true)]
        public async Task<IActionResult> GetSectionRegistrations3Async(Paging page, QueryStringFilter criteria, QueryStringFilter academicPeriod,
            QueryStringFilter sectionInstructor, QueryStringFilter registrationStatusesByAcademicPeriod)
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
                var criteriaObj = GetFilterObject<Dtos.SectionRegistration4>(_logger, "criteria");

                //academicPeriod
                string academicPeriodFilterValue = string.Empty;
                var academicPeriodFilterObj = GetFilterObject<Dtos.Filters.AcademicPeriodNamedQueryFilter>(_logger, "academicPeriod");
                if (academicPeriodFilterObj != null && academicPeriodFilterObj.AcademicPeriod != null && !string.IsNullOrEmpty(academicPeriodFilterObj.AcademicPeriod.Id))
                {
                    academicPeriodFilterValue = academicPeriodFilterObj.AcademicPeriod.Id != null ? academicPeriodFilterObj.AcademicPeriod.Id : null;
                }

                //sectionInstructor
                string sectionInstructorFilterValue = string.Empty;
                var sectionInstructorFilterObj = GetFilterObject<Dtos.Filters.SectionInstructorQueryFilter>(_logger, "sectionInstructor");
                if (sectionInstructorFilterObj != null && sectionInstructorFilterObj.SectionInstructorId != null && !string.IsNullOrEmpty(sectionInstructorFilterObj.SectionInstructorId.Id))
                {
                    sectionInstructorFilterValue = sectionInstructorFilterObj.SectionInstructorId.Id != null ? sectionInstructorFilterObj.SectionInstructorId.Id : null;
                }

                var registrationStatusesByAcademicPeriodObj = GetFilterObject<Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter>(_logger, "registrationStatusesByAcademicPeriod");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.SectionRegistration4>>(new List<Dtos.SectionRegistration4>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _sectionRegistrationService.GetSectionRegistrations3Async(page.Offset, page.Limit, criteriaObj, academicPeriodFilterValue,
                                        sectionInstructorFilterValue, registrationStatusesByAcademicPeriodObj, bypassCache);

                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.SectionRegistration4>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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

        #endregion section-registrations V16.0.0

        /// <summary>
        /// Get section registration
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations })]
        [HeaderVersionRoute("/section-registrations/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionRegistration6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionRegistration2>> GetSectionRegistrationAsync([FromRoute] string guid)
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
                var sectionRegistration = await _sectionRegistrationService.GetSectionRegistrationAsync(guid);

                if (sectionRegistration != null)
                {

                    AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { sectionRegistration.Id }));
                }


                return sectionRegistration;
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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
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
        /// Get section registration V7
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations })]
        [HttpGet]
        [HeaderVersionRoute("/section-registrations/{guid}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionRegistration7", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionRegistration3>> GetSectionRegistration2Async([FromRoute] string guid)
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
                var sectionRegistration = await _sectionRegistrationService.GetSectionRegistration2Async(guid);

                if (sectionRegistration != null)
                {

                    AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { sectionRegistration.Id }));
                }


                return sectionRegistration;
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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
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
        /// Gets section registrations with filter
        /// </summary>
        /// <param name="page"></param>
        /// <param name="section"></param>
        /// <param name="registrant"></param>
        /// <returns></returns>
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations })]
        [ValidateQueryStringFilter(new string[] { "section", "registrant" }, false, true)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/section-registrations", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionRegistrations6", IsEedmSupported = true)]
        public async Task<IActionResult> GetSectionRegistrationsAsync(Paging page, [FromQuery] string section = "",
            [FromQuery] string registrant = "")
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
                var pageOfItems = await
                        _sectionRegistrationService.GetSectionRegistrationsAsync(page.Offset, page.Limit, section, registrant);

                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.SectionRegistration2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Gets section registrations with filter V7
        /// </summary>
        /// <param name="page"></param>
        /// <param name="section"></param>
        /// <param name="registrant"></param>
        /// <returns></returns>
        [HttpGet, TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations })]
        [ValidateQueryStringFilter(new string[] { "section", "registrant" }, false, true)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/section-registrations", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmSectionRegistrations7", IsEedmSupported = true)]
        public async Task<IActionResult> GetSectionRegistrations2Async(Paging page, [FromQuery] string section = "",
            [FromQuery] string registrant = "")
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

                var pageOfItems = await _sectionRegistrationService.GetSectionRegistrations2Async(page.Offset, page.Limit,
                     section, registrant);

                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.SectionRegistration3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Return all sections-registrations-checking
        /// </summary>
        [HttpGet]
        [HeaderVersionRoute("/sections-registrations-checking", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetSectionsRegistrationsCheckingV1Default")]
        //public async Task<IActionResult> GetSectionRegistrationsChecking(Paging page)
        public async Task<IActionResult> GetSectionRegistrationsChecking()
        {
            //Get is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Return sections-registrations-checking
        /// </summary>
        [HttpGet]
        [HeaderVersionRoute("/sections-registrations-checking/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetSectionsRegistrationsCheckingByIdV1Default")]
        //public async Task<IActionResult> GetSectionRegistrationsCheckingById(Paging page)
        public async Task<IActionResult> GetSectionRegistrationsCheckingById()
        {
            //Get is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods

        /// <summary>
        /// Update (PUT) section registrations
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <param name="sectionRegistration">DTO of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(SectionPermissionCodes.UpdateRegistrations)]
        [HeaderVersionRoute("/section-registrations/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmSectionRegistration6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionRegistration2>> PutSectionRegistrationAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration2 sectionRegistration)
        {
            try
            {
                _sectionRegistrationService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("Null sectionRegistration guid", "guid is a required property.");
                }
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Nil GUID cannot be used in PUT operation.");
                }

                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    sectionRegistration.Id = guid.ToUpperInvariant();
                }

                //get Data Privacy List
                var dpList = await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionRegistrationReturn = await _sectionRegistrationService.UpdateSectionRegistrationAsync(guid,
                    await PerformPartialPayloadMerge(sectionRegistration, async () => await _sectionRegistrationService.GetSectionRegistrationAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return sectionRegistrationReturn;

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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
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
        /// Update (PUT) section registrations
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <param name="sectionRegistration">DTO of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration3"/> object</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(SectionPermissionCodes.UpdateRegistrations)]
        [HttpPut]
        [HeaderVersionRoute("/section-registrations/{guid}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmSectionRegistration7", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionRegistration3>> PutSectionRegistration2Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration3 sectionRegistration)
        {
            try
            {
                _sectionRegistrationService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("Null sectionRegistration guid", "guid is a required property.");
                }
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Nil GUID cannot be used in PUT operation.");
                }
                if (!guid.Equals(sectionRegistration.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("GUID not the same as in request body.");
                }
                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    sectionRegistration.Id = guid.ToUpperInvariant();
                }

                //get Data Privacy List
                var dpList = await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionRegistrationReturn = await _sectionRegistrationService.UpdateSectionRegistration2Async(guid,
                    await PerformPartialPayloadMerge(sectionRegistration, async () => await _sectionRegistrationService.GetSectionRegistration2Async(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return sectionRegistrationReturn; 

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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
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
        /// Update (PUT) section registrations
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <param name="sectionRegistration">DTO of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration4"/> object</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(SectionPermissionCodes.UpdateRegistrations)]
        [HeaderVersionRoute("/section-registrations/{guid}", "16.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSectionRegistrationsV16_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionRegistration4>> PutSectionRegistrations3Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration4 sectionRegistration)
        {
            try
            {
                _sectionRegistrationService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("Null sectionRegistration guid", "guid is a required property.");
                }
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Nil GUID cannot be used in PUT operation.");
                }
                if (!guid.Equals(sectionRegistration.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("GUID not the same as in request body.");
                }
                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    sectionRegistration.Id = guid.ToUpperInvariant();
                }

                //get Data Privacy List
                var dpList = await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionRegistrationReturn = await _sectionRegistrationService.UpdateSectionRegistration3Async(guid,
                    await PerformPartialPayloadMerge(sectionRegistration, async () => await _sectionRegistrationService.GetSectionRegistrationByGuid3Async(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return sectionRegistrationReturn;

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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
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
        /// Update (PUT) section-registrations-checking
        /// </summary>
        [HttpPut]
        [HeaderVersionRoute("/sections-registrations-checking/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSectionsRegistrationsCheckingV1")]
        public async Task<IActionResult> PutSectionRegistrationsCheckingById()
        {
            //Put is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Post Methods

        /// <summary>
        /// Create (POST) section registrations
        /// </summary>
        /// <param name="sectionRegistration">A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(SectionPermissionCodes.UpdateRegistrations)]
        [HeaderVersionRoute("/section-registrations", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmSectionRegistration6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionRegistration2>> PostSectionRegistrationAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration2 sectionRegistration)
        {
            try
            {
                _sectionRegistrationService.ValidatePermissions(GetPermissionsMetaData());
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    throw new ArgumentNullException("Null sectionRegistration id", "Id is a required property.");
                }
                //call import extend method that needs the extracted extension data and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section registration
                var sectionRegistrationReturn = await _sectionRegistrationService.CreateSectionRegistrationAsync(sectionRegistration);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionRegistrationReturn.Id }));

                return sectionRegistrationReturn;

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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
        /// Create (POST) section registrations
        /// </summary>
        /// <param name="sectionRegistration">A SectionRegistration <see cref="Dtos.SectionRegistration3"/> object</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration3"/> object</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(SectionPermissionCodes.UpdateRegistrations)]
        [HeaderVersionRoute("/section-registrations", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmSectionRegistration7", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionRegistration3>> PostSectionRegistration2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration3 sectionRegistration)
        {
            try
            {
                _sectionRegistrationService.ValidatePermissions(GetPermissionsMetaData());
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    throw new ArgumentNullException("Null sectionRegistration id", "Id is a required property.");
                }
                //call import extend method that needs the extracted extension data and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section registration
                var sectionRegistrationReturn = await _sectionRegistrationService.CreateSectionRegistration2Async(sectionRegistration);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionRegistrationReturn.Id }));

                return sectionRegistrationReturn;
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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
        /// Create (POST) section registrations
        /// </summary>
        /// <param name="sectionRegistration">A SectionRegistration <see cref="Dtos.SectionRegistration4"/> object</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration4"/> object</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(SectionPermissionCodes.UpdateRegistrations)]
        [HeaderVersionRoute("/section-registrations", "16.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSectionRegistrationsV16_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.SectionRegistration4>> PostSectionRegistrations3Async([ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration4 sectionRegistration)
        {
            try
            {
                _sectionRegistrationService.ValidatePermissions(GetPermissionsMetaData());
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    throw new ArgumentNullException("Null sectionRegistration id", "Id is a required property.");
                }
                //call import extend method that needs the extracted extension data and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section registration
                var sectionRegistrationReturn = await _sectionRegistrationService.CreateSectionRegistration3Async(sectionRegistration);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionRegistrationReturn.Id }));

                return sectionRegistrationReturn;
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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
        /// Create (POST) section registrations checking
        /// </summary>
        /// <param name="sectionRegistrations">A SectionRegistration <see cref="Dtos.SectionRegistration4"/> object</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistrations"/> object</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(SectionPermissionCodes.PerformRegistrationChecks)]        
        [HeaderVersionRoute("/sections-registrations-checking", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSectionsRegistrationsCheckingV1", IsEedmSupported = true)]
        public async Task<IActionResult> PostSectionsRegistrationsCheckingAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistrations sectionRegistrations)
        {
            try
            {
                try
                {
                    _sectionRegistrationService.ValidatePermissions(GetPermissionsMetaData());
                }
                catch (PermissionsException ex)
                {
                    // Swap "create" with "perform" in the error message so it returns: 
                    // "...does not have permission to perform sections-registrations-checking."       
                    if (!string.IsNullOrEmpty(ex.Message))
                    {
                        var message = ex.Message.Replace("create", "perform");
                        throw new PermissionsException(message);
                    }
                    throw;
                }
                if (sectionRegistrations == null)
                {
                    throw new ArgumentNullException("Null sectionRegistrations argument", "The request body is required.");
                }
                
                var sectionRegistrationReturn = await _sectionRegistrationService.CheckSectionRegistrations(sectionRegistrations);

                // nothing to return if no errors.  Will issue a 204 No Content
                return NoContent();
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
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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

        #endregion

        #region Delete Methods

        /// <summary>
        /// Delete (DELETE) an existing section-registrations
        /// </summary>
        /// <param name="guid">Id of the section-registration to delete</param>
        [HttpDelete]
        [Route("/section-registrations/{guid}", Name = "DeleteHedmSectionRegistration", Order = -10)]
        public async Task<IActionResult> DeleteSectionRegistrationAsync([FromRoute] string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing sections-registrations-checking
        /// </summary>
        [HttpDelete]
        [HeaderVersionRoute("/sections-registrations-checking/{id}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DeleteHedmSectionsRegistrationsCheckingV1")]
        //public async Task<IActionResult> DeleteSectionsRegistrationsCheckingAsync([FromUri] string guid)
        public async Task<IActionResult> DeleteSectionsRegistrationsCheckingAsync()
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
  
    }
}

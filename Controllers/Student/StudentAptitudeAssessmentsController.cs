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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;

using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAptitudeAssessments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAptitudeAssessmentsController : BaseCompressedApiController
    {
        private readonly IStudentAptitudeAssessmentsService _studentAptitudeAssessmentsService;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// Initializes a new instance of the StudentAptitudeAssessmentsController class.
        /// </summary>
        /// <param name="studentAptitudeAssessmentsService">Service of type <see cref="IStudentAptitudeAssessmentsService">IStudentAptitudeAssessmentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentAptitudeAssessmentsController(IStudentAptitudeAssessmentsService studentAptitudeAssessmentsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentAptitudeAssessmentsService = studentAptitudeAssessmentsService;
            this._logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Return all studentAptitudeAssessments
        /// </summary>
        /// <returns>List of StudentAptitudeAssessments <see cref="Dtos.StudentAptitudeAssessments"/> objects representing matching studentAptitudeAssessments</returns>
        [HttpGet]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/student-aptitude-assessments", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAptitudeAssessmentsV9", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentAptitudeAssessmentsAsync(Paging page)
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
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentAptitudeAssessments>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Return all studentAptitudeAssessments
        /// </summary>
        /// <returns>List of StudentAptitudeAssessments <see cref="Dtos.StudentAptitudeAssessments"/> objects representing matching studentAptitudeAssessments</returns>
        [HttpGet]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAptitudeAssessments)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter()]
        [HeaderVersionRoute("/student-aptitude-assessments", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAptitudeAssessmentsV11", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentAptitudeAssessments2Async(Paging page, QueryStringFilter criteria)
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
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                var criteriaObj = GetFilterObject<Dtos.StudentAptitudeAssessments>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentAptitudeAssessments>>(new List<Dtos.StudentAptitudeAssessments>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                string studentFilter = (criteriaObj != null && criteriaObj.Student != null ? criteriaObj.Student.Id : "");

                var pageOfItems = await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessments2Async(studentFilter, page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentAptitudeAssessments>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Return all studentAptitudeAssessments
        /// </summary>
        /// <returns>List of StudentAptitudeAssessments <see cref="Dtos.StudentAptitudeAssessments"/> objects representing matching studentAptitudeAssessments</returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAptitudeAssessments2)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter()]
        [HeaderVersionRoute("/student-aptitude-assessments", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAptitudeAssessments", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentAptitudeAssessments3Async(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
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
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if ((personFilterObj != null) && (personFilterObj.personFilter != null))
                {
                    personFilterValue = personFilterObj.personFilter.Id;
                }

                var criteriaObj = GetFilterObject<Dtos.StudentAptitudeAssessments2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentAptitudeAssessments2>>(new List<Dtos.StudentAptitudeAssessments2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                string studentFilter = (criteriaObj != null && criteriaObj.Student != null ? criteriaObj.Student.Id : "");
                string assessmentFilter = (criteriaObj != null && criteriaObj.Assessment != null ? criteriaObj.Assessment.Id : "");

                var pageOfItems = await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessments3Async(studentFilter,
                    assessmentFilter, personFilterValue, page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentAptitudeAssessments2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a studentAptitudeAssessments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-aptitude-assessments/{guid}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAptitudeAssessmentsByGuidV9", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAptitudeAssessments>> GetStudentAptitudeAssessmentsByGuidAsync(string guid)
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
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuidAsync(guid);
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
        /// Read (GET) a studentAptitudeAssessments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-aptitude-assessments/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentAptitudeAssessmentsByGuidV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAptitudeAssessments>> GetStudentAptitudeAssessmentsByGuid2Async(string guid)
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
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuid2Async(guid);
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
        /// Read (GET) a studentAptitudeAssessments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/student-aptitude-assessments/{guid}", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentAptitudeAssessmentsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAptitudeAssessments2>> GetStudentAptitudeAssessmentsByGuid3Async(string guid)
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
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuid3Async(guid);
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
        /// Update (PUT) an existing studentAptitudeAssessments
        /// </summary>
        /// <param name="guid">GUID of the studentAptitudeAssessments to update</param>
        /// <param name="studentAptitudeAssessments">DTO of the updated studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-aptitude-assessments/{guid}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAptitudeAssessmentsV9")]
        public async Task<ActionResult<Dtos.StudentAptitudeAssessments>> PutStudentAptitudeAssessmentsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments studentAptitudeAssessments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing StudentAptitudeAssessments
        /// </summary>
        /// <param name="guid">GUID of the studentAptitudeAssessments to update</param>
        /// <param name="studentAptitudeAssessments">DTO of the updated studentAptitudeAssessments</param>
        /// <returns>A StudentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/student-aptitude-assessments/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAptitudeAssessmentsV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAptitudeAssessments>> PutStudentAptitudeAssessments2Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments studentAptitudeAssessments)
        {
            if (studentAptitudeAssessments.Source != null && (studentAptitudeAssessments.Source.Id == string.Empty || studentAptitudeAssessments.Source.Id == Guid.Empty.ToString()))
                return CreateHttpResponseException(new IntegrationApiException("Null source id",
                    IntegrationApiUtility.GetDefaultApiError("Source id cannot be empty.")));
            if (studentAptitudeAssessments.SpecialCircumstances != null)
            {
                foreach (var circ in studentAptitudeAssessments.SpecialCircumstances)
                {
                    if ((string.IsNullOrEmpty(circ.Id)) || (circ.Id == Guid.Empty.ToString()))
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Null special circumstances id",
                            IntegrationApiUtility.GetDefaultApiError("Special circumstances id cannot be empty.")));
                    }
                }
            }

            try
            {
                // call import extend method that needs the extracted extension data and the config
                await _studentAptitudeAssessmentsService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAptitudeAssessmentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                // merge and update the assessment
                var assessment = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(
                    await PerformPartialPayloadMerge(studentAptitudeAssessments, async () => await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuid2Async(guid, true),
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                    _logger));

                // store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { assessment.Id }));

                return assessment;
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
        /// Update (PUT) an existing StudentAptitudeAssessments
        /// </summary>
        /// <param name="guid">GUID of the studentAptitudeAssessments to update</param>
        /// <param name="studentAptitudeAssessments">DTO of the updated studentAptitudeAssessments</param>
        /// <returns>A StudentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/student-aptitude-assessments/{guid}", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentAptitudeAssessmentsV16", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAptitudeAssessments2>> PutStudentAptitudeAssessments3Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments2 studentAptitudeAssessments)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                  IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentAptitudeAssessments == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null studentAptitudeAssessments argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            //if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            //{
            //    return CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            //}
            //if (string.IsNullOrEmpty(studentAptitudeAssessments.Id))
            //{
            //    studentAptitudeAssessments.Id = guid.ToLowerInvariant();
            //}
            //else if (!string.Equals(guid, studentAptitudeAssessments.Id, StringComparison.InvariantCultureIgnoreCase))
            //{
            //    return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
            //        IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            //}

            //validations to occur before partial put
            if (studentAptitudeAssessments.Source != null && (studentAptitudeAssessments.Source.Id == string.Empty || studentAptitudeAssessments.Source.Id == Guid.Empty.ToString()))
                return CreateHttpResponseException(new IntegrationApiException("Null source id",
                    IntegrationApiUtility.GetDefaultApiError("Source id cannot be empty.")));
            if (studentAptitudeAssessments.SpecialCircumstances != null)
            {
                foreach (var circ in studentAptitudeAssessments.SpecialCircumstances)
                {
                    if ((string.IsNullOrEmpty(circ.Id)) || (circ.Id == Guid.Empty.ToString()))
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Null special circumstances id",
                            IntegrationApiUtility.GetDefaultApiError("Special circumstances id cannot be empty.")));
                    }
                }
            }

            try
            {
                // call import extend method that needs the extracted extension data and the config
                await _studentAptitudeAssessmentsService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAptitudeAssessmentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                // merge and update the assessment
                var assessment = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessments2Async(
                    await PerformPartialPayloadMerge(studentAptitudeAssessments, async () => await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuid3Async(guid, true),
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                    _logger));

                // store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { assessment.Id }));

                return assessment;
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
        /// Create (POST) a new studentAptitudeAssessments
        /// </summary>
        /// <param name="studentAptitudeAssessments">DTO of the new studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-aptitude-assessments", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAptitudeAssessmentsV9")]
        public async Task<ActionResult<Dtos.StudentAptitudeAssessments>> PostStudentAptitudeAssessmentsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments studentAptitudeAssessments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create (POST) a new studentAptitudeAssessments
        /// </summary>
        /// <param name="studentAptitudeAssessments">DTO of the new studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in HeDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPost]
        [HeaderVersionRoute("/student-aptitude-assessments", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAptitudeAssessmentsV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAptitudeAssessments>> PostStudentAptitudeAssessments2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments studentAptitudeAssessments)
        {
            if (studentAptitudeAssessments == null)
            {
                return CreateHttpResponseException("Request body must contain a valid studentAptitudeAssessments.", HttpStatusCode.BadRequest);
            }
           
            if (studentAptitudeAssessments.Source != null && (studentAptitudeAssessments.Source.Id == string.Empty || studentAptitudeAssessments.Source.Id == Guid.Empty.ToString()))
                return CreateHttpResponseException(new IntegrationApiException("Null source id",
                    IntegrationApiUtility.GetDefaultApiError("Source id cannot be empty.")));
            if (studentAptitudeAssessments.SpecialCircumstances != null)
            {
                foreach (var circ in studentAptitudeAssessments.SpecialCircumstances)
                {
                    if ((string.IsNullOrEmpty(circ.Id)) || (circ.Id == Guid.Empty.ToString()))
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Null special circumstances id",
                            IntegrationApiUtility.GetDefaultApiError("Special circumstances id cannot be empty.")));
                    }
                }
            }
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _studentAptitudeAssessmentsService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAptitudeAssessmentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the assessment
                var assessment = await _studentAptitudeAssessmentsService.CreateStudentAptitudeAssessmentsAsync(studentAptitudeAssessments);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { assessment.Id }));

                return assessment;

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
        /// Create (POST) a new studentAptitudeAssessments
        /// </summary>
        /// <param name="studentAptitudeAssessments">DTO of the new studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in HeDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/student-aptitude-assessments", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentAptitudeAssessmentsV16", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentAptitudeAssessments2>> PostStudentAptitudeAssessments3Async([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments2 studentAptitudeAssessments)
        {
            if (studentAptitudeAssessments == null)
            {
                return CreateHttpResponseException("Request body must contain a valid studentAptitudeAssessments.", HttpStatusCode.BadRequest);
            }
            //if (studentAptitudeAssessments.Id != Guid.Empty.ToString())
            //{
            //    return CreateHttpResponseException(new IntegrationApiException("Null guid must be supplied to create operation",
            //        IntegrationApiUtility.GetDefaultApiError("Null guid must be supplied to create operation")));
            //}
            //if (studentAptitudeAssessments.Source != null && (studentAptitudeAssessments.Source.Id == string.Empty || studentAptitudeAssessments.Source.Id == Guid.Empty.ToString()))
            //    return CreateHttpResponseException(new IntegrationApiException("Null source id",
            //        IntegrationApiUtility.GetDefaultApiError("Source id cannot be empty.")));
            //if (studentAptitudeAssessments.SpecialCircumstances != null)
            //{
            //    foreach (var circ in studentAptitudeAssessments.SpecialCircumstances)
            //    {
            //        if ((string.IsNullOrEmpty(circ.Id)) || (circ.Id == Guid.Empty.ToString()))
            //        {
            //            return CreateHttpResponseException(new IntegrationApiException("Null special circumstances id",
            //                IntegrationApiUtility.GetDefaultApiError("Special circumstances id cannot be empty.")));
            //        }
            //    }
            //}
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _studentAptitudeAssessmentsService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAptitudeAssessmentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the assessment
                var assessment = await _studentAptitudeAssessmentsService.CreateStudentAptitudeAssessments2Async(studentAptitudeAssessments);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { assessment.Id }));

                return assessment;
                
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
        /// Delete (DELETE) a studentAptitudeAssessments
        /// </summary>
        /// <param name="guid">GUID to desired studentAptitudeAssessments</param>
        [HttpDelete]
        [Route("/student-aptitude-assessments/{guid}", Name = "DefaultDeleteStudentAptitudeAssessments", Order = -10)]
        public async Task<IActionResult> DeleteStudentAptitudeAssessmentsAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null studentAptitudeAssessments id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                await _studentAptitudeAssessmentsService.DeleteStudentAptitudeAssessmentAsync(guid);
                return NoContent();
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
    }
}

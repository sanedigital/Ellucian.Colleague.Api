// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Course Types data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CourseTypesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ICourseCategoriesService _courseCategoriesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CourseTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        ///  /// <param name="courseCategoriesService">Service of type <see cref="ICourseCategoriesService">ICourseCategoriesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CourseTypesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository,
            ICourseCategoriesService courseCategoriesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _courseCategoriesService = courseCategoriesService;
            this._logger = logger;
        }

        // GET /api/CourseType
        /// <summary>
        /// Retrieves all Course Types.
        /// </summary>
        /// <returns>All Course Type codes and descriptions.</returns>
        /// <note>CourseType is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/course-types", 1, true, Name = "GetCourseTypes")]
        public async Task<ActionResult<IEnumerable<CourseType>>> GetAsync()
        {
            try
            {
                var CourseTypeCollection = await _referenceDataRepository.GetCourseTypesAsync();

                // Get the right adapter for the type mapping
                var CourseTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseType, CourseType>();

                // Map the CourseType entity to the program DTO
                var CourseTypeDtoCollection = new List<CourseType>();
                foreach (var CourseType in CourseTypeCollection)
                {
                    CourseTypeDtoCollection.Add(CourseTypeDtoAdapter.MapToType(CourseType));
                }

                return CourseTypeDtoCollection;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, "Timeout exception has occurred while retrieving course types");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString() + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Return all courseCategories
        /// </summary>
        /// <returns>List of CourseCategories <see cref="Dtos.CourseCategories"/> objects representing matching courseCategories</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/course-categories", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCourseCategories", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CourseCategories>>> GetCourseCategoriesAsync()
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
                AddDataPrivacyContextProperty((await _courseCategoriesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                var courseTypes = await _courseCategoriesService.GetCourseCategoriesAsync(bypassCache);

                if (courseTypes != null && courseTypes.Any())
                {
                    AddEthosContextProperties(await _courseCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _courseCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              courseTypes.Select(a => a.Id).ToList()));
                }

                return Ok(courseTypes);                
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
        /// Read (GET) a courseCategories using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired courseCategories</param>
        /// <returns>A courseCategories object <see cref="Dtos.CourseCategories"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/course-categories/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCourseCategoriesByGuid")]
        public async Task<ActionResult<Dtos.CourseCategories>> GetCourseCategoriesByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _courseCategoriesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                    await _courseCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _courseCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _courseCategoriesService.GetCourseCategoriesByGuidAsync(guid);
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
        /// Create (POST) a new courseCategories
        /// </summary>
        /// <param name="courseCategories">DTO of the new courseCategories</param>
        /// <returns>A courseCategories object <see cref="Dtos.CourseCategories"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/course-categories", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCourseCategoriesV11")]
        public async Task<ActionResult<Dtos.CourseCategories>> PostCourseCategoriesAsync([FromBody] Dtos.CourseCategories courseCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing courseCategories
        /// </summary>
        /// <param name="guid">GUID of the courseCategories to update</param>
        /// <param name="courseCategories">DTO of the updated courseCategories</param>
        /// <returns>A courseCategories object <see cref="Dtos.CourseCategories"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/course-categories/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCourseCategoriesV11")]
        public async Task<ActionResult<Dtos.CourseCategories>> PutCourseCategoriesAsync([FromRoute] string guid, [FromBody] Dtos.CourseCategories courseCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a courseCategories
        /// </summary>
        /// <param name="guid">GUID to desired courseCategories</param>
        [HttpDelete]
        [Route("/course-categories/{guid}", Name = "DefaultDeleteCourseCategories", Order = -10)]
        public async Task<IActionResult> DeleteCourseCategoriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

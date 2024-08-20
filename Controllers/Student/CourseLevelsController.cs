// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using System.Linq;
using System.Net;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Course Level data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CourseLevelsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CourseLevelsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CourseLevelsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _curriculumService = curriculumService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Course Levels.
        /// </summary>
        /// <returns>All <see cref="CourseLevel">Course Level</see> codes and descriptions.</returns>
        /// <note>CourseLevel is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/course-levels", 1, false, Name = "GetCourseLevels")]
        public async Task<ActionResult<IEnumerable<CourseLevel>>> GetAsync()
        {
            try
            {
                var courseLevelCollection = await _referenceDataRepository.GetCourseLevelsAsync();

                // Get the right adapter for the type mapping
                var courseLevelDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseLevel, CourseLevel>();

                // Map the courselevel entity to the program DTO
                var courseLevelDtoCollection = new List<CourseLevel>();
                foreach (var courseLevel in courseLevelCollection)
                {
                    courseLevelDtoCollection.Add(courseLevelDtoAdapter.MapToType(courseLevel));
                }

                return courseLevelDtoCollection;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, "Timeout exception has occurred while retrieving course levels");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString() + ex.StackTrace);
                throw;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Retrieves all course levels.
        /// </summary>
        /// <returns>All <see cref="CourseLevel2">CourseLevels.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/course-levels", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEedmCourseLevels2V6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.CourseLevel2>>> GetCourseLevels2Async()
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var courseLevels = await _curriculumService.GetCourseLevels2Async(bypassCache);

                if (courseLevels != null && courseLevels.Any())
                {
                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              courseLevels.Select(a => a.Id).ToList()));
                }
                return Ok(courseLevels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Retrieves a course level by ID.
        /// </summary>
        /// <returns>A <see cref="CourseLevel2">CourseLevel.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/course-levels/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCourseLevelById2", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.CourseLevel2>> GetCourseLevelById2Async(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _curriculumService.GetCourseLevelById2Async(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

         /// <summary>
        /// Creates a Course Level.
        /// </summary>
        /// <param name="courseLevel"><see cref="CourseLevel2">CourseLevel</see> to create</param>
        /// <returns>Newly created <see cref="CourseLevel2">InstructionalMethod</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/course-levels", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCourseLevelsV6")]
        public async Task<ActionResult<Dtos.CourseLevel2>> PostCourseLevelsAsync([FromBody] Dtos.CourseLevel2 courseLevel)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a Course Level.
        /// </summary>
        /// <param name="id">Id of the Course Level to update</param>
        /// <param name="courseLevel"><see cref="CourseLevel2">CourseLevel</see> to create</param>
        /// <returns>Updated <see cref="CourseLevel">CourseLevel</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/course-levels/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCourseLevelsV6")]
        public async Task<ActionResult<Dtos.CourseLevel2>> PutCourseLevelsAsync([FromRoute] string id, [FromBody] Dtos.CourseLevel2 courseLevel)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Course Level.
        /// </summary>
        /// <param name="id">Id of the Course Level to delete</param>
        [HttpDelete]
        [Route("/course-levels/{id}", Name = "DeleteCourseLevels", Order = -10)]
        public async Task<IActionResult> DeleteCourseLevelsAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

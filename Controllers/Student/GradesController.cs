// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using System;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Linq;
using Ellucian.Web.Security;
using System.Net;
using System.Net.Http;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Grade data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class GradesController : BaseCompressedApiController
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IGradeService _gradeService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GradesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="gradeRepository">Repository of type <see cref="IGradeRepository">IGradeRepository</see></param>
        /// <param name="gradeService">Service of type <see cref="IGradeService">IGradeService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public GradesController(IAdapterRegistry adapterRegistry, IGradeRepository gradeRepository, IGradeService gradeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _gradeRepository = gradeRepository;
            _gradeService = gradeService;
            this._logger = logger;
        }

        #region Grades

        /// <summary>
        /// Retrieves information for all Grades.
        /// </summary>
        /// <returns>All <see cref="Grade">Grades</see></returns>
        /// <accessComments>Any authenticated user may retrieve grade code information.</accessComments>
        /// <note>Grade information is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/grades", 1, true, Name = "GetGrades")]
        public async Task<ActionResult<IEnumerable<Grade>>> GetAsync()
        {
            try
            {
                var gradeDtoCollection = new List<Grade>();
                var gradeCollection = await _gradeRepository.GetAsync();
                // Get the right adapter for the type mapping
                var gradeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Grade>();
                // Map the grade entity to the grade DTO
                foreach (var grade in gradeCollection)
                {
                    gradeDtoCollection.Add(gradeDtoAdapter.MapToType(grade));
                }

                return gradeDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving grades";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception tex)
            {
                string message = "Exception occurred while retrieving grades";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets PilotGrades based on query criteria such as studentIds and term.
        /// Requires VIEW.STUDENT.INFORMATION permission.
        /// </summary>
        /// <param name="criteria">A <see cref="GradeQueryCriteria"/> grade criteria object</param>
        /// <returns><see cref="PilotGrade"/>PilotGrades that match the criteria parameters.</returns>
        /// <accessComments>
        /// User with permission of VIEW.STUDENT.INFORMATION can access student grades.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/grades", 1, false, RouteConstants.EllucianJsonPilotMediaTypeFormat, Name = "QueryPilotGrades")]
        public async Task<ActionResult<IEnumerable<PilotGrade>>> QueryPilotGradesAsync([FromBody] GradeQueryCriteria criteria)
        {
            var gradeDtoCollection = new List<Dtos.Student.PilotGrade>();
            var studentIds = criteria.StudentIds;
            var term = criteria.Term;
            try
            {
                var gradeCollection = await _gradeService.GetPilotGradesAsync(studentIds, term);
                // Get the right adapter for the type mapping
                var gradeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PilotGrade, PilotGrade>();
                // Map the grade entity to the grade DTO
                foreach (var grade in gradeCollection)
                {
                    gradeDtoCollection.Add(gradeDtoAdapter.MapToType(grade));
                }
                return gradeDtoCollection;
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
          
        }

        /// <summary>
        /// Gets grades based on query criteria such as studentIds and term.
        /// Requires VIEW.STUDENT.INFORMATION permission.
        /// </summary>
        /// <param name="criteria">A <see cref="GradeQueryCriteria"/> grade criteria object</param>
        /// <returns><see cref="PilotGrade"/>PilotGrades that match the criteria parameters.</returns>
        /// <accessComments>
        /// User with permission of VIEW.STUDENT.INFORMATION can access student grades.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/grades", 2, false, RouteConstants.EllucianJsonPilotMediaTypeFormat, Name = "QueryPilotGradesV2")]
        public async Task<ActionResult<IEnumerable<PilotGrade>>> QueryPilotGrades2Async([FromBody] GradeQueryCriteria criteria)
        {
            var gradeDtoCollection = new List<Dtos.Student.PilotGrade>();
            var studentIds = criteria.StudentIds;
            var term = criteria.Term;
            try
            {
                var gradeCollection = await _gradeService.GetPilotGrades2Async(studentIds, term);
                // Get the right adapter for the type mapping
                var gradeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PilotGrade, PilotGrade>();
                // Map the grade entity to the grade DTO
                foreach (var grade in gradeCollection)
                {
                    gradeDtoCollection.Add(gradeDtoAdapter.MapToType(grade));
                }

                return gradeDtoCollection;
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region grade-definitions

        /// <summary>
        /// Retrieves information for all Grades.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Grade">Grades</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/grade-definitions", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmGrades", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Grade>>> GetHedmAsync()
        {
            bool bypassCache = false;
            if(Request.GetTypedHeaders().CacheControl != null)
            {
                if(Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var gradeEntities = await _gradeService.GetAsync(bypassCache);

                AddEthosContextProperties(
                        await _gradeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _gradeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            gradeEntities.Select(ge => ge.Id).ToList()));

                return Ok(gradeEntities);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }
        /// <summary>
        /// Retrieves grade by id
        /// </summary>
        /// <param name="id">The Id of the grade</param>
        /// <returns>The requested <see cref="Grade">Grade</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/grade-definitions/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmGradeById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Grade>> GetByIdHedmAsync(string id)
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
                AddEthosContextProperties(
                           await _gradeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                           await _gradeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                               new List<string>() { id }));

                return await _gradeService.GetGradeByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Create a grade
        /// </summary>
        /// <param name="grade">grade</param>
        /// <returns>A section object <see cref="Dtos.Grade"/> in HeDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/grade-definitions", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGradeV6")]
        public async Task<ActionResult<Dtos.Grade>> PostGradeAsync([FromBody] Ellucian.Colleague.Dtos.Grade grade)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update a grade
        /// </summary>
        /// <param name="id">desired id for a grade</param>
        /// <param name="grade">grade</param>
        /// <returns>A section object <see cref="Dtos.Grade"/> in HeDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/grade-definitions/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGradeV6")]
        public async Task<ActionResult<Dtos.Grade>> PutGradeAsync([FromRoute] string id, [FromBody] Dtos.Grade grade)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a grade
        /// </summary>
        /// <param name="id">id to desired grade</param>
        /// <returns>A section object <see cref="Dtos.Grade"/> in HeDM format</returns>
        [HttpDelete]
        [Route("/grade-definitions/{id}", Name = "DeleteGrade", Order = -10)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Grade>> DeleteGradeByIdAsync(string id)
        {
            //Delete is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region grade-definitions-maximum

        /// <summary>
        /// Retrieves information for all Grades.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="GradeDefinitionsMaximum">GradeDefinitionsMaximum</see></returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/grade-definitions", 6, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "GetGradeDefinitionsMaximumV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.GradeDefinitionsMaximum>>> GetGradeDefinitionsMaximumAsync()
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
                var defs = await _gradeService.GetGradesDefinitionsMaximumAsync(bypassCache);
                if (defs != null && defs.Any())
                {
                    AddEthosContextProperties(
                        await _gradeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _gradeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         defs.Select(i => i.Id).ToList()));
                }
                return Ok(defs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }
        /// <summary>
        /// Retrieves grade by id
        /// </summary>
        /// <param name="id">The Id of the grade</param>
        /// <returns>The requested <see cref="Dtos.GradeDefinitionsMaximum">GradeDefinitionsMaximum</see></returns>                            
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/grade-definitions/{id}", 6, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "GetGradeDefinitionsMaximumByIdV6", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.GradeDefinitionsMaximum>> GetGradeDefinitionsMaximumByIdAsync(string id)
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
                var def = await _gradeService.GetGradesDefinitionsMaximumIdAsync(id);  // TODO: Honor bypassCache

                if (def != null)
                {

                    AddEthosContextProperties(await _gradeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _gradeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { def.Id }));
                }
                return def;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Create a GradeDefinitionsMaximum
        /// </summary>
        /// <param name="grade">grade</param>
        /// <returns>A GradeDefinitionsMaximum object <see cref="Dtos.GradeDefinitionsMaximum"/> in HeDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/grade-definitions", 6, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "PostGradeDefinitionsMaximumV6")]
        public async Task<ActionResult<Dtos.GradeDefinitionsMaximum>> PostGradeDefinitionsMaximumAsync([FromBody] Ellucian.Colleague.Dtos.GradeDefinitionsMaximum grade)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update a GradeDefinitionsMaximum
        /// </summary>
        /// <param name="id">desired id for a grade</param>
        /// <param name="grade">grade</param>
        /// <returns>A GradeDefinitionsMaximum object <see cref="Dtos.GradeDefinitionsMaximum"/> in HeDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/grade-definitions/{id}", 6, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "PutGradeDefinitionsMaximumV6")]
        public async Task<ActionResult<Dtos.Grade>> PutGradeDefinitionsMaximumAsync([FromRoute] string id, [FromBody] Dtos.GradeDefinitionsMaximum grade)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a GradeDefinitionsMaximum
        /// </summary>
        /// <param name="id">id to desired grade</param>
        /// <returns>A GradeDefinitionsMaximum object <see cref="Dtos.GradeDefinitionsMaximum"/> in HeDM format</returns>
        [HttpDelete]
        [HeaderVersionRoute("/grade-definitions/{id}", 6, false, RouteConstants.HedtechIntegrationMaximumMediaTypeFormat, Name = "DeleteGradeDefinitionsMaximum")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.GradeDefinitionsMaximum>> DeleteGradeDefinitionsMaximumByIdAsync(string id)
        {
            //Delete is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Anonymous Grading

        /// <summary>
        /// Retrieves all Anonymous Grading Ids for a student either by terms or by sections.
        /// </summary>
        /// <param name="criteria">A <see cref="AnonymousGradingQueryCriteria ">AnonymousGradingQueryCriteria</see> used to retrieve Anonymous Grading Ids for a student.</param>
        /// <returns>A collection of <see cref="StudentAnonymousGrading">Student Anonymous Grading Ids</see> for student</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if Anonymous Grading Ids are not found, either for specified terms or sections</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the student id is not provided.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        /// An authenticated student may view their Anonymous Grading Ids for academic terms or course sections.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/anonymous-grading-ids", 1, true, Name = "QueryAnonymousGradingIds")]
        public async Task<ActionResult<IEnumerable<StudentAnonymousGrading>>> QueryAnonymousGradingIdsAsync([FromBody] AnonymousGradingQueryCriteria criteria)
        {

            try
            {
                if (criteria == null || string.IsNullOrWhiteSpace(criteria.StudentId))
                {
                    throw new ArgumentNullException("studentId", "a student id is required in order to retrieve grading ids for a student");
                }

                if ((criteria.TermIds != null && criteria.TermIds.Any()) && (criteria.SectionIds != null && criteria.SectionIds.Any()))
                {
                    throw new ArgumentException("either term ids or course section ids may be provided but not both");
                }

                return Ok(await _gradeService.QueryAnonymousGradingIdsAsync(criteria));
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while querying anonymous grading Ids";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (ArgumentNullException anex)
            {
                // student id was not provided.
                var exceptionMsg = "A student id was not provided.";
                _logger.LogError(anex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.BadRequest);
            }
            catch (ArgumentException anex)
            {
                // invalid parameters passed
                var exceptionMsg = "Either term ids or course section ids may be provided but not both..";
                _logger.LogError(anex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                string exceptionMsg = "User is not permitted to retrieve anonymous grading ids for the student.";
                _logger.LogError(peex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Any other error, described in returned message
                string exceptionMsg = "Could not retrieve anonymous grading ids for this student.";
                _logger.LogError(ex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.BadRequest);
            }
        }

        #endregion Anonymous Grading
    }
}

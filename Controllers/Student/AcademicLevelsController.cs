// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

using System.Net;
using System.Linq;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
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
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AcademicLevels
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Route("/[controller]/[action]")]
    public class AcademicLevelsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// AcademicLevelsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AcademicLevelsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _curriculumService = curriculumService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Academic Levels.
        /// </summary>
        /// <returns>All <see cref="AcademicLevel">Academic Level</see> codes and descriptions.</returns>
        /// <note>AcademicLevel is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/academic-levels", 1, false, Name = "GetAcademicLevels")]
        public async Task<ActionResult<IEnumerable<AcademicLevel>>> GetAsync()
        {
            try
            {
                var academicLevelCollection = await _studentReferenceDataRepository.GetAcademicLevelsAsync();

                // Get the right adapter for the type mapping
                var academicLevelDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel, AcademicLevel>();

                // Map the academiclevel entity to the program DTO
                var academicLevelDtoCollection = new List<AcademicLevel>();
                foreach (var academicLevel in academicLevelCollection)
                {
                    academicLevelDtoCollection.Add(academicLevelDtoAdapter.MapToType(academicLevel));
                }

                return academicLevelDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving academic levels";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString() + ex.StackTrace);
                throw;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 5</remarks>
        /// <summary>
        /// Retrieves all academic levels.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevels.</see></returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/academic-levels", "6.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmAcademicLevels3", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AcademicLevel2>>> GetAcademicLevels3Async()
        {
            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var items = await _curriculumService.GetAcademicLevels2Async(bypassCache);

                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 5</remarks>
        /// <summary>
        /// Retrieves an academic level by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/academic-levels/{id}", "6.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAcademicLevelById3", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.AcademicLevel2>> GetAcademicLevelById3Async(string id)
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
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Academic Level id is required.");
                }

                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _curriculumService.GetAcademicLevelById2Async(id);
            }
            catch (PermissionsException e)
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
        /// Updates a AcademicLevel.
        /// </summary>
        /// <param name="academicLevel"><see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see> to update</param>
        /// <returns>Newly updated <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see></returns>
        [HttpPut]
        public async Task<ActionResult<Dtos.AcademicLevel2>> PutAcademicLevelsAsync([FromBody] Dtos.AcademicLevel2 academicLevel)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a AcademicLevel.
        /// </summary>
        /// <param name="academicLevel"><see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see> to create</param>
        /// <returns>Newly created <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see></returns>
        [HttpPost]
        public async Task<ActionResult<Dtos.AcademicLevel2>> PostAcademicLevelsAsync([FromBody] Dtos.AcademicLevel2 academicLevel)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a AcademicLevel.
        /// </summary>
        /// <param name="academicLevel"><see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see> to update</param>
        /// <returns>Newly updated <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/academic-levels/{id}", "6.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAcademicLevels2_V6.1.0")]
        public async Task<ActionResult<Dtos.AcademicLevel2>> PutAcademicLevels2Async([FromBody] Dtos.AcademicLevel2 academicLevel)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a AcademicLevel.
        /// </summary>
        /// <param name="academicLevel"><see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see> to create</param>
        /// <returns>Newly created <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/academic-levels", "6.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAcademicLevels2_V6.1.0")]
        public async Task<ActionResult<Dtos.AcademicLevel2>> PostAcademicLevels2Async([FromBody] Dtos.AcademicLevel2 academicLevel)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing AcademicLevel
        /// </summary>
        /// <param name="id">Id of the AcademicLevel to delete</param>
        [HttpDelete]
        [Route("/academic-levels/{id}", Name = "DeleteAcademicLevels2", Order = -10)]
        public async Task<IActionResult> DeleteAcademicLevels2Async(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

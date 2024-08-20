// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Subject data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SubjectsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SubjectsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SubjectsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _curriculumService = curriculumService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Subjects. Includes an indicator as to whether to show the subject in the course catalog.
        /// </summary>
        /// <returns>All <see cref="Subject">Subject</see> codes and descriptions.</returns>
        /// <note>Subject is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/subjects", 1, false, Name = "GetSubjects")]
        public async Task<ActionResult<IEnumerable<Subject>>> GetAsync()
        {
            try
            {
                var subjectCollection = await _referenceDataRepository.GetSubjectsAsync();

                // Get the right adapter for the type mapping
                var subjectDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Subject, Subject>();

                // Map the subject entity to the program DTO
                var subjectDtoCollection = new List<Subject>();
                foreach (var subject in subjectCollection)
                {
                    subjectDtoCollection.Add(subjectDtoAdapter.MapToType(subject));
                }
                return subjectDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving subjects";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception tex)
            {
                string message = "Exception occurred while retrieving subjects";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Retrieves all subjects.
        /// </summary>
        /// <returns>All <see cref="Dtos.Subject2">Subjects.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/subjects", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSubjects2Async", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Subject2>>> GetSubjects2Async()
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
                var subjectEntities = await _curriculumService.GetSubjects2Async(bypassCache);

                AddEthosContextProperties(
                        await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            subjectEntities.Select(sc => sc.Id).ToList()));

                return Ok(subjectEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Retrieves a subject by GUID.
        /// </summary>
        /// <returns>A <see cref="Dtos.Subject2">Subject.</see></returns> 
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/subjects/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSubjectByGuid2Async", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Subject2>> GetSubjectByGuid2Async(string id)
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
                        await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            new List<string>() { id }));

                return await _curriculumService.GetSubjectByGuid2Async(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a Subject.
        /// </summary>
        /// <param name="subject"><see cref="Dtos.Subject2">Subject</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.Subject2">Subject</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/subjects", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSubjectV6")]
        public ActionResult<Dtos.Subject2> PostSubject([FromBody] Dtos.Subject2 subject)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a Subject.
        /// </summary>
        /// <param name="id">Id of the Subject to update</param>
        /// <param name="subject"><see cref="Dtos.Subject2">Subject</see> to create</param>
        /// <returns>Updated <see cref="Dtos.Subject2">Subject</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/subjects/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSubjectV6")]
        public ActionResult<Dtos.Subject2> PutSubject([FromRoute] string id, [FromBody] Dtos.Subject2 subject)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete an existing Subject
        /// </summary>
        /// <param name="id">Id of the Subject to delete</param>
        [HttpDelete]
        [Route("/subjects/{id}", Name = "DeleteSubject", Order = -10)]
        public IActionResult DeleteSubject([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

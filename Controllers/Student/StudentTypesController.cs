// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using System;
using System.Linq;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to StudentType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTypesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentTypesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _curriculumService = curriculumService;
            this._logger = logger;
        }
        /// <summary>
        /// Get all studentTypes.
        /// </summary>
        /// <returns>List of <see cref="StudentType">StudentType</see> data.</returns>
        /// <note>StudentType is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/student-types", 1, false, Name = "GetStudentTypes")]
        public async Task<ActionResult<IEnumerable<StudentType>>> GetAsync()
        {
            var studentTypeCollection = await _referenceDataRepository.GetStudentTypesAsync(false);

            // Get the right adapter for the type mapping
            var studentTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentType, StudentType>();

            // Map the student type entity to the student type DTO
            var studentTypeDtoCollection = new List<StudentType>();
            foreach (var studentType in studentTypeCollection)
            {
                studentTypeDtoCollection.Add(studentTypeDtoAdapter.MapToType(studentType));
            }

            return studentTypeDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves all student types.
        /// </summary>
        /// <returns>All <see cref="StudentType">StudentTypes.</see></returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-types", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentTypes", IsEedmSupported = true)]
        [HeaderVersionRoute("/student-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentTypesV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.StudentType>>> GetStudentTypesAsync()
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
                var items = await _curriculumService.GetStudentTypesAsync(bypassCache);

                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves an student type by ID.
        /// </summary>
        /// <returns>A <see cref="StudentType">StudentType.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-types/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentTypesByGuid", IsEedmSupported = true)]
        [HeaderVersionRoute("/student-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentTypesByGuidV6", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.StudentType>> GetStudentTypeByIdAsync(string id)
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
                    throw new ArgumentNullException("Student type id is required.");
                }

                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _curriculumService.GetStudentTypeByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a StudentType.
        /// </summary>
        /// <param name="studentType"><see cref="StudentType">StudentType</see> to update</param>
        /// <returns>Newly updated <see cref="StudentType">StudentType</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/student-types/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentTypesV7")]
        [HeaderVersionRoute("/student-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentTypesV6")]
        public async Task<ActionResult<Dtos.StudentType>> PutStudentTypeAsync([FromBody] Dtos.StudentType studentType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a StudentType.
        /// </summary>
        /// <param name="studentType"><see cref="StudentType">StudentType</see> to create</param>
        /// <returns>Newly created <see cref="StudentType">StudentType</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/student-types", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentTypesV7")]
        [HeaderVersionRoute("/student-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentTypesV6")]
        public async Task<ActionResult<Dtos.StudentType>> PostStudentTypeAsync([FromBody] Dtos.StudentType studentType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing StudentType
        /// </summary>
        /// <param name="id">Id of the StudentType to delete</param>
        [HttpDelete]
        [Route("/student-types/{id}", Name = "DefaultDeleteStudentTypes", Order = -10)]
        public async Task<IActionResult> DeleteStudentTypeAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

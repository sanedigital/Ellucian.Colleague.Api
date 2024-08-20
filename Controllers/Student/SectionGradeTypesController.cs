// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Section Grade Type data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Route("/[controller]/[action]")]
    public class SectionGradeTypesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SectionGradeTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionGradeTypesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _curriculumService = curriculumService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Section Grade Types.
        /// </summary>
        /// <returns>All <see cref="SectionGradeType">Section Grade Type</see> codes and descriptions.</returns>
        public async Task<ActionResult<IEnumerable<SectionGradeType>>> GetAsync()
        {
            var sectionGradeTypeCollection = await _referenceDataRepository.GetSectionGradeTypesAsync();

            // Get the right adapter for the type mapping
            var sectionGradeTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType, SectionGradeType>();

            // Map the sectiongradetype entity to the program DTO
            var sectionGradeTypeDtoCollection = new List<SectionGradeType>();
            foreach (var sectionGradeType in sectionGradeTypeCollection)
            {
                sectionGradeTypeDtoCollection.Add(sectionGradeTypeDtoAdapter.MapToType(sectionGradeType));
            }

            return sectionGradeTypeDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Retrieves all section grade types.
        /// </summary>
        /// <returns>All <see cref="SectionGradeType">SectionGradeTypes.</see></returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/section-grade-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSectionGradeTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.SectionGradeType>>> GetSectionGradeTypesAsync()
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
                return Ok(await _curriculumService.GetSectionGradeTypesAsync(bypassCache));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Retrieves a section grade type by ID.
        /// </summary>
        /// <returns>A <see cref="SectionGradeType">SectionGradeType.</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/section-grade-types/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSectionGradeTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.SectionGradeType>> GetSectionGradeTypeByIdAsync(string id)
        {
            try
            {
                return await _curriculumService.GetSectionGradeTypeByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

         /// <summary>
        /// Creates a Section Grade Type.
        /// </summary>
        /// <param name="sectionGradeType"><see cref="SectionGradeType">SectionGradeType</see> to create</param>
        /// <returns>Newly created <see cref="SectionGradeType">SectionGradeType</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/section-grade-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSectionGradeTypesV6")]
        public async Task<ActionResult<Dtos.SectionGradeType>> PostSectionGradeTypesAsync([FromBody] Dtos.SectionGradeType sectionGradeType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a Section Grade Type.
        /// </summary>
        /// <param name="id">Id of the Section Grade Type to update</param>
        /// <param name="sectionGradeType"><see cref="SectionGradeType">SectionGradeType</see> to create</param>
        /// <returns>Updated <see cref="SectionGradeType">SectionGradeType</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/section-grade-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSectionGradeTypesV6")]
        public async Task<ActionResult<Dtos.SectionGradeType>> PutSectionGradeTypesAsync([FromRoute] string id, [FromBody] Dtos.SectionGradeType sectionGradeType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Section Grade Type.
        /// </summary>
        /// <param name="id">Id of the Section Grade Type to delete</param>
        [HttpDelete]
        [Route("/section-grade-types/{id}", Name = "DeleteSectionGradeTypes", Order = -10)]
        public async Task<IActionResult> DeleteSectionGradeTypesAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

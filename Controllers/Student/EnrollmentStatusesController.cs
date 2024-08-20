// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;

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
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to EnrollmentStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class EnrollmentStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// EnrollmentStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EnrollmentStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _curriculumService = curriculumService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all EnrollmentStatuses.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.EnrollmentStatus">EnrollmentStatus.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/enrollment-statuses", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEnrollmentStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.EnrollmentStatus>>> GetEnrollmentStatusesAsync()
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

                var enrollmentStatuses = await _curriculumService.GetEnrollmentStatusesAsync(bypassCache);

                if (enrollmentStatuses != null && enrollmentStatuses.Any())
                {
                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              enrollmentStatuses.Select(a => a.Id).ToList()));
                }

                return Ok(enrollmentStatuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a EnrollmentStatus by ID.
        /// </summary>
        /// <param name="id">ID to desired EnrollmentStatus</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.EnrollmentStatus">EnrollmentStatus.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/enrollment-statuses/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEnrollmentStatusById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.EnrollmentStatus>> GetEnrollmentStatusByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _curriculumService.GetEnrollmentStatusByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a EnrollmentStatus.
        /// </summary>
        /// <param name="enrollmentStatus"><see cref="EnrollmentStatus">EnrollmentStatus</see> to update</param>
        /// <returns>Newly updated <see cref="EnrollmentStatus">EnrollmentStatus</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/enrollment-statuses/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEnrollmentStatusV6")]
        public async Task<ActionResult<Dtos.EnrollmentStatus>> PutEnrollmentStatusAsync([FromBody] Dtos.EnrollmentStatus enrollmentStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a EnrollmentStatus.
        /// </summary>
        /// <param name="enrollmentStatus"><see cref="EnrollmentStatus">EnrollmentStatus</see> to create</param>
        /// <returns>Newly created <see cref="EnrollmentStatus">EnrollmentStatus</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/enrollment-statuses", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEnrollmentStatusV6")]
        public async Task<ActionResult<Dtos.EnrollmentStatus>> PostEnrollmentStatusAsync([FromBody] Dtos.EnrollmentStatus enrollmentStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing EnrollmentStatus.
        /// </summary>
        /// <param name="id">Id of the EnrollmentStatus to delete</param>
        [HttpDelete]
        [Route("/enrollment-statuses/{id}", Name = "DeleteEnrollmentStatus", Order = -10)]
        public async Task<IActionResult> DeleteEnrollmentStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

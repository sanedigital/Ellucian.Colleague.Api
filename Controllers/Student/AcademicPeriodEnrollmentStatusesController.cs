// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using System.Net;
using System.Net.Http.Headers;
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
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to AcademicPeriodEnrollmentStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicPeriodEnrollmentStatusesController : BaseCompressedApiController
    {
        //private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// AcademicPeriodEnrollmentStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AcademicPeriodEnrollmentStatusesController(IAdapterRegistry adapterRegistry, ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            //_studentReferenceDataRepository = studentReferenceDataRepository;
            _curriculumService = curriculumService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Retrieves all AcademicPeriodEnrollmentStatus.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus">EnrollmentStatus.</see></returns>
        /// 
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/academic-period-enrollment-statuses", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAcademicPeriodEnrollmentStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus>>> GetAcademicPeriodEnrollmentStatusesAsync()
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

                var academicPeriodEnrollmentStatus = await _curriculumService.GetAcademicPeriodEnrollmentStatusesAsync(bypassCache);

                if (academicPeriodEnrollmentStatus != null && academicPeriodEnrollmentStatus.Any())
                {
                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              academicPeriodEnrollmentStatus.Select(a => a.Id).ToList()));
                }

                return Ok(academicPeriodEnrollmentStatus);                
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Retrieves a AcademicPeriodEnrollmentStatus by ID.
        /// </summary>
        /// <param name="id">ID to desired AcademicPeriodEnrollmentStatus</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus">AcademicPeriodEnrollmentStatus</see></returns>
        /// 
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/academic-period-enrollment-statuses/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAcademicPeriodEnrollmentStatusById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus>> GetAcademicPeriodEnrollmentStatusByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _curriculumService.GetAcademicPeriodEnrollmentStatusByGuidAsync(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Updates a AcademicPeriodEnrollmentStatus.
        /// </summary>
        /// <param name="id"><see cref="id">id</see></param>
        /// <param name="academicPeriodEnrollmentStatus"><see cref="AcademicPeriodEnrollmentStatus">AcademicPeriodEnrollmentStatus</see> to update</param>
        /// <returns>Newly updated <see cref="AcademicPeriodEnrollmentStatus">AcademicPeriodEnrollmentStatus</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/academic-period-enrollment-statuses/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAcademicPeriodEnrollmentStatusesV6")]
        public async Task<ActionResult<Dtos.AcademicPeriodEnrollmentStatus>> PutAcademicPeriodEnrollmentStatusAsync([FromRoute] string id, [FromBody] Dtos.AcademicPeriodEnrollmentStatus academicPeriodEnrollmentStatus)
        {
            //Create is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a AcademicPeriodEnrollmentStatus.
        /// </summary>
        /// <param name="academicPeriodEnrollmentStatus"><see cref="AcademicPeriodEnrollmentStatus">AcademicPeriodEnrollmentStatus</see> to create</param>
        /// <returns>Newly created <see cref="AcademicPeriodEnrollmentStatus">AcademicPeriodEnrollmentStatus</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/academic-period-enrollment-statuses", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAcademicPeriodEnrollmentStatusesV6")]
        public async Task<ActionResult<Dtos.AcademicPeriodEnrollmentStatus>> PostAcademicPeriodEnrollmentStatusAsync([FromBody] Dtos.AcademicPeriodEnrollmentStatus academicPeriodEnrollmentStatus)
        {
            //Update is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing AcademicPeriodEnrollmentStatus.
        /// </summary>
        /// <param name="id">Id of the AcademicPeriodEnrollmentStatus to delete</param>
        [HttpDelete]
        [Route("/academic-period-enrollment-statuses/{id}", Name = "DefaultDeleteAcademicPeriodEnrollmentStatuses")]
        public async Task<IActionResult> DeleteAcademicPeriodEnrollmentStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

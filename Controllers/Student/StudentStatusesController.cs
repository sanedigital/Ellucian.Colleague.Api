// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos;
using System.Net;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to StudentStatus data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentStatusesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _curriculumService = curriculumService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves all student statuses.
        /// </summary>
        /// <returns>All <see cref="StudentStatus">StudentStatuses.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/student-statuses", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentStatuses", IsEedmSupported = true)]
        [HeaderVersionRoute("/student-statuses", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentStatusesV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.StudentStatus>>> GetStudentStatusesAsync()
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
                var studentStatuses = await _curriculumService.GetStudentStatusesAsync(bypassCache);

                if (studentStatuses != null && studentStatuses.Any())
                {
                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              studentStatuses.Select(a => a.Id).ToList()));
                }

                return Ok(studentStatuses);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves an student status by ID.
        /// </summary>
        /// <returns>A <see cref="StudentStatus">StudentStatus.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-statuses/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentStatusesByGuid", IsEedmSupported = true)]
        [HeaderVersionRoute("/student-statuses/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentStatusesByGuidV6", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.StudentStatus>> GetStudentStatusByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _curriculumService.GetStudentStatusByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e, string.Format("No student status was found for guid '{0}'.", id));
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a StudentStatus.
        /// </summary>
        /// <param name="studentStatus"><see cref="StudentStatus">StudentStatus</see> to update</param>
        /// <returns>Newly updated <see cref="StudentStatus">StudentStatus</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/student-statuses/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentStatusesV7")]
        [HeaderVersionRoute("/student-statuses/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentStatusesV6")]
        public async Task<ActionResult<Dtos.StudentStatus>> PutStudentStatusAsync([FromBody] Dtos.StudentStatus studentStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a StudentStatus.
        /// </summary>
        /// <param name="studentStatus"><see cref="StudentStatus">StudentStatus</see> to create</param>
        /// <returns>Newly created <see cref="StudentStatus">StudentStatus</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/student-statuses", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentStatusesV7")]
        [HeaderVersionRoute("/student-statuses", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentStatusesV6")]
        public async Task<ActionResult<Dtos.StudentStatus>> PostStudentStatusAsync([FromBody] Dtos.StudentStatus studentStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing StudentStatus
        /// </summary>
        /// <param name="id">Id of the StudentStatus to delete</param>
        [HttpDelete]
        [Route("/student-statuses/{id}", Name = "DefaultDeleteStudentStatuses", Order = -10)]
        public async Task<IActionResult> DeleteStudentStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

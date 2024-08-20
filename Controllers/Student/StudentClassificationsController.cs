// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student.Transcripts;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using StudentClassification = Ellucian.Colleague.Dtos.StudentClassification;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Accesses Student classification data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentClassificationsController : BaseCompressedApiController
    {
        private readonly IStudentService _studentService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentsClassificationsController class.
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="studentService">studentService</param>
        /// <param name="logger">logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentClassificationsController(IAdapterRegistry adapterRegistry, IStudentService studentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentService = studentService;
            _adapterRegistry = adapterRegistry;
            this._logger = logger;
        }

        /// <summary>
        /// Gets all student classification
        /// </summary>
        /// <returns></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/student-classifications", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentClassifications", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<StudentClassification>>> GetStudentClassificationsAsync()
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
                var classificationEntities = await _studentService.GetAllStudentClassificationsAsync(bypassCache);

                AddEthosContextProperties(
                        await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            classificationEntities.Select(sc => sc.Id).ToList()));

                return Ok(classificationEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Retrieves a student classification by id.
        /// </summary>
        /// <param name="id">Id of students classification to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.StudentClassification">student classification.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-classifications/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentClassificationById", IsEedmSupported = true)]
        public async Task<ActionResult<StudentClassification>> GetStudentClassificationByIdAsync(string id)
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
                        await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            new List<string>() { id }));

                return await _studentService.GetStudentClassificationByGuidAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// POST student classification
        /// </summary>
        /// <param name="studentClassification"></param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.StudentClassification">student Classification.</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/student-classifications", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentClassification")]
        public async Task<ActionResult<StudentClassification>> PostStudentClassificationAsync([FromBody] StudentClassification studentClassification)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// PUT student classification
        /// </summary>
        /// <param name="id"></param>
        /// <param name="studentClassification"></param>
        /// <returns>Dtos.StudentsClassification</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-classifications/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentClassification")]
        public async Task<ActionResult<StudentClassification>> PutStudentClassificationAsync([FromRoute] string id, [FromBody] StudentClassification studentClassification)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete student classification
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/student-classifications/{id}", Name = "DefaultDeleteStudentClassification", Order = -10)]
        public async Task<IActionResult> DeleteStudentClassificationAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

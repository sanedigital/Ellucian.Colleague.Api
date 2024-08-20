// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Accesses Student cohorts data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentCohortsController : BaseCompressedApiController
    {
        private readonly IStudentService _studentService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentsCohortsController class.
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="studentService">studentService</param>
        /// <param name="logger">logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentCohortsController(IAdapterRegistry adapterRegistry, IStudentService studentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentService = studentService;
            _adapterRegistry = adapterRegistry;
            this._logger = logger;
        }

        /// <summary>
        /// Gets all student cohorts
        /// </summary>
        /// <returns></returns>
        [ValidateQueryStringFilter(new[] { "code" } ), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter( "criteria", typeof( Dtos.Filters.CodeItemFilter ) )]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-cohorts", "7.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultStudentCohorts", IsEedmSupported = true)]
        [HeaderVersionRoute("/student-cohorts", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentCohorts", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.StudentCohort>>> GetStudentCohortsAsync( QueryStringFilter criteria )
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
                //Criteria
                var criteriaObj = GetFilterObject<Dtos.Filters.CodeItemFilter>( _logger, "criteria" );

                if( CheckForEmptyFilterParameters() )
                {
                    return new List<Dtos.StudentCohort>();
                }

                var items = await _studentService.GetAllStudentCohortsAsync( criteriaObj, bypassCache );

                AddEthosContextProperties(
                    await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch  (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Retrieves a student cohort by guid.
        /// </summary>
        /// <param name="id">Guid of students cohort to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.StudentCohort">student cohort.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-cohorts/{id}", "7.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultStudentCohortById", IsEedmSupported = true)]
        [HeaderVersionRoute("/student-cohorts/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentCohortById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.StudentCohort>> GetStudentCohortByIdAsync(string id)
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

                return await _studentService.GetStudentCohortByGuidAsync(id, bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
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

        /// <summary>
        /// POST student cohort
        /// </summary>
        /// <param name="studentCohort"></param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.StudentCohort">student cohort.</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/student-cohorts", "7.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentCohortV720")]
        [HeaderVersionRoute("/student-cohorts", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentCohort")]
        public async Task<ActionResult<Dtos.StudentCohort>> PostStudentCohortAsync([FromBody] Dtos.StudentCohort studentCohort)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// PUT student cohort
        /// </summary>
        /// <param name="id"></param>
        /// <param name="studentCohort"></param>
        /// <returns>Dtos.StudentsCohort</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-cohorts/{id}", "7.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentCohortV720")]
        [HeaderVersionRoute("/student-cohorts/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentCohort")]
        public async Task<ActionResult<Dtos.StudentCohort>> PutStudentCohortAsync([FromRoute] string id, [FromBody] Dtos.StudentCohort studentCohort)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete student cohort
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/student-cohorts/{id}", Name = "DefaultDeleteStudentCohort", Order = -10)]
        public async Task<IActionResult> DeleteStudentCohortAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

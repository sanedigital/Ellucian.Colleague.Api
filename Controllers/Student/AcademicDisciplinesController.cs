// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Linq;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Academic Disciplines data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicDisciplinesController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IAcademicDisciplineService _academicDisciplineService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AcademicDisciplinesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="academicDisciplineService">Service of type <see cref="IAcademicDisciplineService">IAcademicDisciplineService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public AcademicDisciplinesController(IAdapterRegistry adapterRegistry, IAcademicDisciplineService academicDisciplineService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _academicDisciplineService = academicDisciplineService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM V15 (Same as V10, but adds filter and named query) </remarks>
        /// <summary>
        /// Retrieves all Academic Disciplines.  
        /// </summary>
        /// <returns>All <see cref="Dtos.AcademicDiscipline3">AcademicDiscipline </see>objects.</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AcademicDiscipline3))]
        [QueryStringFilterFilter("majorStatus", typeof(Dtos.Filters.MajorStatusFilter))]
        [HeaderVersionRoute("/academic-disciplines", "15", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAcademicDisciplinesDefault", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.AcademicDiscipline3>>> GetAcademicDisciplines3Async(QueryStringFilter criteria, QueryStringFilter majorStatus)
        {
            Dtos.EnumProperties.MajorStatus status = Dtos.EnumProperties.MajorStatus.NotSet;
            string type = "";


            bool bypassCache = false;
            if (Request != null && Request.Headers != null && Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (CheckForEmptyFilterParameters())
            {
                return CreateHttpResponseException(new IntegrationApiException("Empty filter parameter supplied."), HttpStatusCode.BadRequest);
            }
            
            try
            {
                // Check for discipline type filter
                var criteriaObj = GetFilterObject<Dtos.AcademicDiscipline3>(_logger, "criteria");

                if (criteriaObj != null)
                {
                    if (criteriaObj.Type == AcademicDisciplineType2.Major) type = "major";
                    if (criteriaObj.Type == AcademicDisciplineType2.Minor) type = "minor";
                    if (criteriaObj.Type == AcademicDisciplineType2.Concentration) type = "concentration";
                }

                // Check for named query majorStatus
                var majorStatusObj = GetFilterObject<Dtos.Filters.MajorStatusFilter>(_logger, "majorStatus");
                if (majorStatusObj != null)
                {
                    status = majorStatusObj.MajorStatus;
                }

                var disciplineDtos = await _academicDisciplineService.GetAcademicDisciplines3Async(status, type, bypassCache);

                AddEthosContextProperties(await _academicDisciplineService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _academicDisciplineService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          disciplineDtos.Select(dd => dd.Id).ToList()));
                return Ok(disciplineDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }
                        
        /// <remarks>FOR USE WITH ELLUCIAN EEDM V7, V10</remarks>
        /// <summary>
        /// Retrieves all Academic Disciplines. 
        /// </summary>
        /// <returns>All <see cref="Dtos.AcademicDiscipline2">AcademicDiscipline </see>objects.</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/academic-disciplines", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAcademicDisciplinesV10", IsEedmSupported = true)]
        [HeaderVersionRoute("/academic-disciplines", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAcademicDisciplinesV7", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.AcademicDiscipline2>>> GetAcademicDisciplines2Async()
        {
            bool bypassCache = false;
            if (Request != null && Request.Headers != null && Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var disciplineDtos = await _academicDisciplineService.GetAcademicDisciplines2Async(bypassCache);

                AddEthosContextProperties(await _academicDisciplineService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _academicDisciplineService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          disciplineDtos.Select(dd => dd.Id).ToList()));
                return Ok(disciplineDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }
        /// <remarks>FOR USE WITH ELLUCIAN EEDM V6</remarks>
        /// <summary>
        /// Retrieves all Academic Disciplines.
        /// </summary>
        /// <returns>All <see cref="Dtos.AcademicDiscipline">AcademicDiscipline </see>objects.</returns>
        /// 
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/academic-disciplines", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAcademicDisciplinesV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.AcademicDiscipline>>> GetAcademicDisciplinesAsync()
        {

            bool bypassCache = false;
            if (Request != null && Request.Headers != null && Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var disciplineDtos = await _academicDisciplineService.GetAcademicDisciplinesAsync(bypassCache);
                var x = disciplineDtos.First().Id;

                AddEthosContextProperties(await _academicDisciplineService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _academicDisciplineService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          disciplineDtos.Select(dd => dd.Id).ToList()));
                return Ok(disciplineDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an academic discipline by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicDiscipline2">AcademicDiscipline2 </see>object.</returns>
        /// 
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/academic-disciplines/{id}", "15", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAcademicDisciplinesByIdDefault", IsEedmSupported = true)]
        [HeaderVersionRoute("/academic-disciplines/{id}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAcademicDisciplinesByIdV10", IsEedmSupported = true)]
        [HeaderVersionRoute("/academic-disciplines/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAcademicDisciplinesByIdV7", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AcademicDiscipline2>> GetAcademicDiscipline2ByIdAsync(string id)
        {
            bool bypassCache = false;
            if (Request != null && Request.Headers != null && Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                var disciplineDto = await _academicDisciplineService.GetAcademicDiscipline2ByGuidAsync(id);
                if (disciplineDto == null)
                {
                    throw new KeyNotFoundException("Academic Discipline not found for GUID " + id);
                }

                AddEthosContextProperties(await _academicDisciplineService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _academicDisciplineService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          new List<string>() { disciplineDto.Id }));
                return disciplineDto;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an academic discipline by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicDiscipline">AcademicDiscipline </see>object.</returns>
        /// 
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/academic-disciplines/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAcademicDisciplinesByIdV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AcademicDiscipline>> GetAcademicDisciplineByIdAsync(string id)
        {
            bool bypassCache = false;
            if (Request != null && Request.Headers != null && Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var disciplineDto = await _academicDisciplineService.GetAcademicDisciplineByGuidAsync(id);
                if (disciplineDto == null)
                {
                    throw new KeyNotFoundException("Academic Discipline not found for GUID " + id);
                }

                AddEthosContextProperties(await _academicDisciplineService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _academicDisciplineService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          new List<string>() { disciplineDto.Id }));
                return disciplineDto;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }



        /// <summary>        
        /// Creates a AcademicDiscipline.
        /// </summary>
        /// <param name="academicDiscipline"><see cref="Dtos.AcademicDiscipline">AcademicDiscipline</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicDiscipline">AcademicDiscipline</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/academic-disciplines", "15", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAcademicDisciplinesV15")]
        [HeaderVersionRoute("/academic-disciplines", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAcademicDisciplinesV10")]
        [HeaderVersionRoute("/academic-disciplines", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAcademicDisciplinesV7")]
        [HeaderVersionRoute("/academic-disciplines", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAcademicDisciplinesV6")]
        public ActionResult<Dtos.AcademicDiscipline> PostAcademicDisciplines([FromBody] Dtos.AcademicDiscipline academicDiscipline)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a AcademicDiscipline.
        /// </summary>
        /// <param name="id">Id of the AcademicDiscipline to update</param>
        /// <param name="academicDiscipline"><see cref="Dtos.AcademicDiscipline">AcademicDiscipline</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicDiscipline">AcademicDiscipline</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/academic-disciplines/{id}", "15", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAcademicDisciplinesV15")]
        [HeaderVersionRoute("/academic-disciplines/{id}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAcademicDisciplinesV10")]
        [HeaderVersionRoute("/academic-disciplines/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAcademicDisciplinesV7")]
        [HeaderVersionRoute("/academic-disciplines/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAcademicDisciplinesV6")]
        public ActionResult<Dtos.AcademicDiscipline> PutAcademicDisciplines([FromRoute] string id, [FromBody] Dtos.AcademicDiscipline academicDiscipline)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing AcademicDiscipline
        /// </summary>
        /// <param name="id">Id of the AcademicDiscipline to delete</param>
        [HttpDelete]
        [Route("/academic-disciplines/{id}", Name = "DeleteAcademicDisciplines", Order = -10)]
        public IActionResult DeleteAcademicDisciplines([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

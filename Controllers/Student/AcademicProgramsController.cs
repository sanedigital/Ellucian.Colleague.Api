// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using System;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Security;
using System.Net;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to AcademicProgram data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Route("/[controller]/[action]")]
    public class AcademicProgramsController : BaseCompressedApiController
    {
        private readonly IAcademicProgramService _academicProgramService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AcademicProgramController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="academicProgramService">Repository of type <see cref="IAcademicProgramService">IAcademicPeriodService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AcademicProgramsController(IAdapterRegistry adapterRegistry, IAcademicProgramService academicProgramService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _academicProgramService = academicProgramService;
            this._logger = logger;
        }

        /// <summary>
        /// Get all academicPrograms.
        /// </summary>
        /// <returns>List of <see cref="AcademicProgram">AcademicProgram</see> data.</returns>
        /// <note>AcademicProgram is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/academic-programs", 1, false, Name = "GetAcademicPrograms")]
        public async Task<ActionResult<IEnumerable<AcademicProgram>>> GetAsync()
        {
            try
            {
                return Ok(await _academicProgramService.GetAsync());
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving academic programs.";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EeDM</remarks>
        /// <summary>
        /// Retrieves an academic program by GUID.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicProgram">AcademicProgram</see>object.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/academic-programs/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmAcademicProgramsById_V6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AcademicProgram2>> GetAcademicProgramByIdV6Async(string id)
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
                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _academicProgramService.GetAcademicProgramByGuidV6Async(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Get all academicPrograms for HeDM version 6.
        /// </summary>
        /// <returns>List of <see cref="AcademicProgram">AcademicProgram</see> data.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/academic-programs", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmAcademicPrograms_V6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.AcademicProgram2>>> GetAcademicProgramsV6Async()
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

                var items = await _academicProgramService.GetAcademicProgramsV6Async(bypassCache);

                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EeDM</remarks>
        /// <summary>
        /// Retrieves an academic program by GUID in V10 format.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicProgram">AcademicProgram</see>object.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/academic-programs/{id}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmAcademicProgramsById_V10", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AcademicProgram3>> GetAcademicProgramById3Async(string id)
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
                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _academicProgramService.GetAcademicProgramByGuid3Async(id);
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
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Get all academicPrograms for HeDM version 10.
        /// </summary>
        /// <returns>List of <see cref="AcademicProgram">AcademicProgram</see> data.</returns>        
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/academic-programs", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmAcademicPrograms_V10", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.AcademicProgram3>>> GetAcademicPrograms3Async()
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

                var items = await _academicProgramService.GetAcademicPrograms3Async(bypassCache);

                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EeDM</remarks>
        /// <summary>
        /// Read (GET) a academicPrograms using a GUID
        /// </summary>
        /// <param name="id">GUID to desired academicPrograms</param>
        /// <returns>A academicPrograms object <see cref="Dtos.AcademicProgram4"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/academic-programs/{id}", "15.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmAcademicProgramsById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AcademicProgram4>> GetAcademicProgramById4Async(string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _academicProgramService.GetAcademicProgramByGuid4Async(id);
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
        /// Return all academicPrograms
        /// </summary>
        /// <returns>List of AcademicPrograms <see cref="Dtos.AcademicProgram4"/> objects representing matching academicPrograms</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("academicCatalog", typeof(Dtos.Filters.AcademicCatalogFilter))]
        [QueryStringFilterFilter("recruitmentProgram", typeof(Dtos.Filters.RecruitmentProgramFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AcademicProgram4))]
        [HeaderVersionRoute("/academic-programs", "15.2.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHedmAcademicPrograms", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.AcademicProgram4>>> GetAcademicPrograms4Async(QueryStringFilter academicCatalog, QueryStringFilter recruitmentProgram, 
            QueryStringFilter criteria)
        {
            var academicCatalogId = string.Empty;
            var recruitmentProgActive = string.Empty;
            bool bypassCache = false;

            try
            {
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }              

                var academicCatalogFilter = GetFilterObject<Dtos.Filters.AcademicCatalogFilter>(_logger, "academicCatalog");

                var recruitmentProgramFilter = GetFilterObject<Dtos.Filters.RecruitmentProgramFilter>(_logger, "recruitmentProgram");

                var criteriaObj = GetFilterObject<Dtos.AcademicProgram4>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.AcademicProgram4>(new List<Dtos.AcademicProgram4>());

                if ((academicCatalogFilter != null) && (academicCatalogFilter.AcademicCatalog != null)
                        && (!string.IsNullOrEmpty(academicCatalogFilter.AcademicCatalog.Id)))
                {
                    academicCatalogId = academicCatalogFilter.AcademicCatalog.Id;
                }

                if (recruitmentProgramFilter.RecruitmentProgram.HasValue)
                {
                    recruitmentProgActive = "active";
                }

                var items = await _academicProgramService.GetAcademicPrograms4Async(academicCatalogId, recruitmentProgActive, criteriaObj, bypassCache);

                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return Ok(items);
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
        
        #region PUT/POST
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Creates a AcademicProgram.
        /// </summary>
        /// <param name="academicProgram"><see cref="Dtos.AcademicProgram">AcademicProgram</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicProgram">AcademicProgram</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/academic-programs", "15.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmAcademicPrograms_V1520")]
        [HeaderVersionRoute("/academic-programs", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmAcademicPrograms_V10")]
        [HeaderVersionRoute("/academic-programs", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmAcademicPrograms_V6")]
        public ActionResult<Dtos.AcademicProgram> PostAcademicProgram([FromBody] Dtos.AcademicProgram academicProgram)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Updates a Academic Program.
        /// </summary>
        /// <param name="id">Id of the Academic Program to update</param>
        /// <param name="academicProgram"><see cref="Dtos.AcademicProgram">AcademicProgram</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicProgram">AcademicProgram</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/academic-programs/{id}", "15.2.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmAcademicPrograms_V1520")]
        [HeaderVersionRoute("/academic-programs/{id}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmAcademicPrograms_V10")]
        [HeaderVersionRoute("/academic-programs/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmAcademicPrograms_V6")]
        public IActionResult PutAcademicProgram([FromRoute] string id, [FromBody] Dtos.AcademicProgram academicProgram)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Creates a AcademicProgram.
        /// </summary>
        /// <param name="academicProgram"><see cref="Dtos.AcademicProgram">AcademicProgram</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicProgram3">AcademicProgram</see></returns>
        [HttpPost]
        public IActionResult PostAcademicProgram3([FromBody] Dtos.AcademicProgram3 academicProgram)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Updates a Academic Program.
        /// </summary>
        /// <param name="id">Id of the Academic Program to update</param>
        /// <param name="academicProgram"><see cref="Dtos.AcademicProgram">AcademicProgram</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicProgra3m">AcademicProgram</see></returns>
        [HttpPut]
        public IActionResult PutAcademicProgram3([FromRoute] string id, [FromBody] Dtos.AcademicProgram3 academicProgram)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing Academic Program
        /// </summary>
        /// <param name="id">Id of the Academic Program to delete</param>
        [HttpDelete]
        [Route("/academic-programs/{id}", Name = "DeleteHedmAcademicPrograms")]
        public IActionResult DeleteAcademicProgram([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

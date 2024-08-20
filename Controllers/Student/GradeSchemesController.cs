// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
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
    /// Provides access to Grade Scheme data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Route("/[controller]/[action]")]
    public class GradeSchemesController : BaseCompressedApiController
    {
        private readonly IGradeSchemeService _gradeSchemeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GradeSchemesController class.
        /// </summary>
        /// <param name="gradeSchemeService">Service of type <see cref="IGradeSchemeService">IGradeSchemeService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GradeSchemesController(IGradeSchemeService gradeSchemeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _gradeSchemeService = gradeSchemeService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all grade schemes.
        /// </summary>
        /// <returns>All GradeScheme objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.GradeScheme>>> GetGradeSchemesAsync()
        {
            try
            {
                return Ok(await _gradeSchemeService.GetGradeSchemesAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a grade scheme by ID
        /// </summary>
        /// <param name="id">ID of the grade scheme</param>
        /// <returns>A grade scheme</returns>
        /// <accessComments>Any authenticated user can retrieve grade scheme information.</accessComments>
        /// <note>Grade scheme data is cached for 24 hours.</note>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/grade-schemes/{id}", 1, false, Name = "GetNonEthosGradeSchemeByIdAsync")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Student.GradeScheme>> GetNonEthosGradeSchemeByIdAsync([FromRoute] string id)
        {
            try
            {
                return await _gradeSchemeService.GetNonEthosGradeSchemeByIdAsync(id);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                var message = "Session has expired while retrieving grade scheme information.";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, string.Format("Could not retrieve a grade scheme with ID {0}.", id));
                return CreateHttpResponseException(string.Format("Could not retrieve a grade scheme with ID {0}.", id), System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves a grade scheme by GUID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.GradeScheme">GradeScheme.</see></returns>
        public async Task<ActionResult<Ellucian.Colleague.Dtos.GradeScheme>> GetGradeSchemeByGuidAsync(string guid)
        {
            try
            {
                return await _gradeSchemeService.GetGradeSchemeByGuidAsync(guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all grade schemes.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All GradeScheme objects.</returns>

        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/grade-schemes", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHeDMGradeSchemes2", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.GradeScheme2>>> GetGradeSchemes2Async()
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
                var gradeSchemeDtos = await _gradeSchemeService.GetGradeSchemes2Async(bypassCache);
                AddEthosContextProperties(
                    await _gradeSchemeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _gradeSchemeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        gradeSchemeDtos.Select(i => i.Id).ToList()));
                return Ok(gradeSchemeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves a grade scheme by ID.
        /// </summary>
        /// <param name="id">Id of Grade Scheme to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.GradeScheme2">GradeScheme.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/grade-schemes/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetGradeSchemeById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.GradeScheme2>> GetGradeSchemeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _gradeSchemeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _gradeSchemeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _gradeSchemeService.GetGradeSchemeByIdAsync(id);                 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Creates a GradeScheme.
        /// </summary>
        /// <param name="gradeScheme"><see cref="Dtos.GradeScheme2">GradeScheme</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.GradeScheme2">GradeScheme</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/grade-schemes", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGradeSchemeV6")]
        public async Task<ActionResult<Dtos.GradeScheme2>> PostGradeSchemeAsync([FromBody] Dtos.GradeScheme2 gradeScheme)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Updates a Grade Scheme.
        /// </summary>
        /// <param name="id">Id of the Grade Scheme to update</param>
        /// <param name="gradeScheme"><see cref="Dtos.GradeScheme2">GradeScheme</see> to create</param>
        /// <returns>Updated <see cref="Dtos.GradeScheme2">GradeScheme</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/grade-schemes/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGradeSchemeV6")]
        public async Task<ActionResult<Dtos.GradeScheme2>> PutGradeSchemeAsync([FromRoute] string id, [FromBody] Dtos.GradeScheme2 gradeScheme)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing Grade Scheme
        /// </summary>
        /// <param name="id">Id of the Grade Scheme to delete</param>
        [HttpDelete]
        [Route("/grade-schemes/{id}", Name = "DeleteGradeScheme", Order = -10)]
        public async Task<IActionResult> DeleteGradeSchemeAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


    }
}

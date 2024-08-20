// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Routes;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Controller for Academic Credentials
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AcademicCredentialsController : BaseCompressedApiController
    {
        private readonly IAcademicCredentialService _academicCredentialService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the Academic Credentials Controller class.
        /// </summary>
        /// <param name="academicCredentialService">Service of type <see cref="IAcademicCredentialService">IAcademicCredentialService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AcademicCredentialsController(IAcademicCredentialService academicCredentialService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _academicCredentialService = academicCredentialService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Academic Credentials
        /// </summary>
        /// <returns>All <see cref="Dtos.AcademicCredential">Academic Credentials.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/academic-credentials", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAcademicCredentials", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.AcademicCredential>>> GetAcademicCredentialsAsync()
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
                var items = await _academicCredentialService.GetAcademicCredentialsAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _academicCredentialService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _academicCredentialService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  items.Select(a => a.Id).ToList()));
                }

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Academic Credential by GUID.
        /// </summary>
        /// <returns>A <see cref="Dtos.AcademicCredential">Academic Credential.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/academic-credentials/{guid}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAcademicCredentialByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.AcademicCredential>> GetAcademicCredentialByGuidAsync(string guid)
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
                var item = await _academicCredentialService.GetAcademicCredentialByGuidAsync(guid);

                if (item != null)
                {
                    AddEthosContextProperties(await _academicCredentialService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache), 
                        await _academicCredentialService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), 
                        new List<string>() { item.Id }));
                }

                return item;
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>        
        /// Creates an Academic Credential
        /// </summary>
        /// <param name="academicCredential"><see cref="Dtos.AcademicCredential">AcademicCredential</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicCredential">AcademicCredential</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/academic-credentials", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAcademicCredentialsV6")]
        public async Task<IActionResult> PostAcademicCredentialAsync([FromBody] Dtos.AcademicCredential academicCredential)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>        
        /// Updates an AcademicCredential.
        /// </summary>
        /// <param name="id">Id of the Academic Credential to update</param>
        /// <param name="academicCredential"><see cref="Dtos.AcademicCredential">AcademicCredential</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicCredential">AcademicCredential</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/academic-credentials/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAcademicCredentialsV6")]
        public async Task<ActionResult<Dtos.AcademicCredential>> PutAcademicCredentialAsync([FromRoute] string id, [FromBody] Dtos.AcademicCredential academicCredential)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Academic Credential
        /// </summary>
        /// <param name="id">Id of the Academic Credential to delete</param>
        [HttpDelete]
        [Route("/academic-credentials/{id}", Name = "DeleteAcademicCredentials", Order = -10)]
        public async Task<IActionResult> DeleteAcademicCredentialAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

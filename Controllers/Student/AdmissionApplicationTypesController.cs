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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Controller for Admission Application Types
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationTypesController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationTypesService _admissionApplicationTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationTypeController class.
        /// </summary>
        /// <param name="admissionApplicationTypesService">Service of type <see cref="IAdmissionApplicationTypesService">IAdmissionApplicationTypesService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdmissionApplicationTypesController(IAdmissionApplicationTypesService admissionApplicationTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _admissionApplicationTypesService = admissionApplicationTypesService;
            this._logger = logger;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Admission Application Types
        /// </summary>
        /// <returns>All <see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes.</see></returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/admission-application-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplicationTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationTypes>>> GetAdmissionApplicationTypesAsync()
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
                return Ok(await _admissionApplicationTypesService.GetAdmissionApplicationTypesAsync(bypassCache));
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

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Admission Application Type by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes.</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/admission-application-types/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdmissionApplicationTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.AdmissionApplicationTypes>> GetAdmissionApplicationTypeByIdAsync(string id)
        {
            try
            {
                return await _admissionApplicationTypesService.GetAdmissionApplicationTypesByGuidAsync(id);
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
        /// Updates an AdmissionApplicationTypes.
        /// </summary>
        /// <param name="admissionApplicationTypes"><see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes</see> to update</param>
        /// <returns>Newly updated <see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/admission-application-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdmissionApplicationType")]
        public ActionResult<Dtos.AdmissionApplicationTypes> PutAdmissionApplicationType([FromBody] Dtos.AdmissionApplicationTypes admissionApplicationTypes)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a AdmissionApplicationTypes.
        /// </summary>
        /// <param name="admissionApplicationTypes"><see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/admission-application-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdmissionApplicationType")]
        public ActionResult<Dtos.AdmissionApplicationTypes> PostAdmissionApplicationType([FromBody] Dtos.AdmissionApplicationTypes admissionApplicationTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing AdmissionApplicationTypes
        /// </summary>
        /// <param name="id">Id of the AdmissionApplicationTypes to delete</param>
        [HttpDelete]
        [Route("/admission-application-types/{id}", Name = "DefaultDeleteAdmissionApplicationType")]
        public ActionResult<Dtos.AdmissionApplicationTypes> DeleteAdmissionApplicationType(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

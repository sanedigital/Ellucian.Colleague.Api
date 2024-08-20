// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentResidentialCategories
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class StudentResidentialCategoriesController : BaseCompressedApiController
    {
        private readonly IStudentResidentialCategoriesService _studentResidentialCategoriesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentResidentialCategoriesController class.
        /// </summary>
        /// <param name="studentResidentialCategoriesService">Service of type <see cref="IStudentResidentialCategoriesService">IStudentResidentialCategoriesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentResidentialCategoriesController(IStudentResidentialCategoriesService studentResidentialCategoriesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _studentResidentialCategoriesService = studentResidentialCategoriesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all studentResidentialCategories
        /// </summary>
        /// <returns>List of StudentResidentialCategories <see cref="Dtos.StudentResidentialCategories"/> objects representing matching studentResidentialCategories</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/student-residential-categories", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentResidentialCategories", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.StudentResidentialCategories>>> GetStudentResidentialCategoriesAsync()
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
                var studentResidentialCategories = await _studentResidentialCategoriesService.GetStudentResidentialCategoriesAsync(bypassCache);

                if (studentResidentialCategories != null && studentResidentialCategories.Any())
                {
                    AddEthosContextProperties(await _studentResidentialCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _studentResidentialCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              studentResidentialCategories.Select(a => a.Id).ToList()));
                }
                return Ok(studentResidentialCategories);
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
        /// Read (GET) a studentResidentialCategories using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentResidentialCategories</param>
        /// <returns>A studentResidentialCategories object <see cref="Dtos.StudentResidentialCategories"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-residential-categories/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentResidentialCategoriesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentResidentialCategories>> GetStudentResidentialCategoriesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                   await _studentResidentialCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _studentResidentialCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentResidentialCategoriesService.GetStudentResidentialCategoriesByGuidAsync(guid);
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
        /// Create (POST) a new studentResidentialCategories
        /// </summary>
        /// <param name="studentResidentialCategories">DTO of the new studentResidentialCategories</param>
        /// <returns>A studentResidentialCategories object <see cref="Dtos.StudentResidentialCategories"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-residential-categories", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentResidentialCategoriesV10")]
        public async Task<ActionResult<Dtos.StudentResidentialCategories>> PostStudentResidentialCategoriesAsync([FromBody] Dtos.StudentResidentialCategories studentResidentialCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentResidentialCategories
        /// </summary>
        /// <param name="guid">GUID of the studentResidentialCategories to update</param>
        /// <param name="studentResidentialCategories">DTO of the updated studentResidentialCategories</param>
        /// <returns>A studentResidentialCategories object <see cref="Dtos.StudentResidentialCategories"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-residential-categories/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentResidentialCategoriesV10")]
        public async Task<ActionResult<Dtos.StudentResidentialCategories>> PutStudentResidentialCategoriesAsync([FromRoute] string guid, [FromBody] Dtos.StudentResidentialCategories studentResidentialCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentResidentialCategories
        /// </summary>
        /// <param name="guid">GUID to desired studentResidentialCategories</param>
        [HttpDelete]
        [Route("/student-residential-categories/{guid}", Name = "DefaultDeleteStudentResidentialCategories", Order = -10)]
        public async Task<IActionResult> DeleteStudentResidentialCategoriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmploymentDepartments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentDepartmentsController : BaseCompressedApiController
    {
        private readonly IEmploymentDepartmentsService _employmentDepartmentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentDepartmentsController class.
        /// </summary>
        /// <param name="employmentDepartmentsService">Service of type <see cref="IEmploymentDepartmentsService">IEmploymentDepartmentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmploymentDepartmentsController(IEmploymentDepartmentsService employmentDepartmentsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _employmentDepartmentsService = employmentDepartmentsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all employmentDepartments
        /// </summary>
        /// <returns>List of EmploymentDepartments <see cref="Dtos.EmploymentDepartments"/> objects representing matching employmentDepartments</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.EmploymentDepartments))]
        [HeaderVersionRoute("/employment-departments", "12.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmploymentDepartments", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.EmploymentDepartments>>> GetEmploymentDepartmentsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            string code = string.Empty;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var criteriaObj = GetFilterObject<Dtos.EmploymentDepartments>(_logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.EmploymentDepartments>(new List<Dtos.EmploymentDepartments>());
                var items = await _employmentDepartmentsService.GetEmploymentDepartmentsAsync(bypassCache);
                if (criteriaObj != null && !string.IsNullOrEmpty(criteriaObj.Code) && items != null && items.Any())
                {
                    code = criteriaObj.Code;
                    items = items.Where(c => c.Code == code);                    
                }
                AddEthosContextProperties(await _employmentDepartmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _employmentDepartmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  items.Select(a => a.Id).ToList()));

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

        /// <summary>
        /// Read (GET) a employmentDepartments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employmentDepartments</param>
        /// <returns>A employmentDepartments object <see cref="Dtos.EmploymentDepartments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/employment-departments/{guid}", "12.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmploymentDepartmentsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.EmploymentDepartments>> GetEmploymentDepartmentsByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                var employmentDepartment = await _employmentDepartmentsService.GetEmploymentDepartmentsByGuidAsync(guid);

                if (employmentDepartment != null)
                {

                    AddEthosContextProperties(await _employmentDepartmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _employmentDepartmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { employmentDepartment.Id }));
                }

                return employmentDepartment;
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
        /// Create (POST) a new employmentDepartments
        /// </summary>
        /// <param name="employmentDepartments">DTO of the new employmentDepartments</param>
        /// <returns>A employmentDepartments object <see cref="Dtos.EmploymentDepartments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/employment-departments", "12.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEmploymentDepartmentsV1210")]
        public async Task<ActionResult<Dtos.EmploymentDepartments>> PostEmploymentDepartmentsAsync([FromBody] Dtos.EmploymentDepartments employmentDepartments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employmentDepartments
        /// </summary>
        /// <param name="guid">GUID of the employmentDepartments to update</param>
        /// <param name="employmentDepartments">DTO of the updated employmentDepartments</param>
        /// <returns>A employmentDepartments object <see cref="Dtos.EmploymentDepartments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/employment-departments/{guid}", "12.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmploymentDepartmentsV1210")]
        public async Task<ActionResult<Dtos.EmploymentDepartments>> PutEmploymentDepartmentsAsync([FromRoute] string guid, [FromBody] Dtos.EmploymentDepartments employmentDepartments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employmentDepartments
        /// </summary>
        /// <param name="guid">GUID to desired employmentDepartments</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/employment-departments/{guid}", Name = "DefaultDeleteEmploymentDepartments")]
        public async Task<IActionResult> DeleteEmploymentDepartmentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
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


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to InstructorStaffTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstructorStaffTypesController : BaseCompressedApiController
    {
        private readonly IInstructorStaffTypesService _instructorStaffTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructorStaffTypesController class.
        /// </summary>
        /// <param name="instructorStaffTypesService">Service of type <see cref="IInstructorStaffTypesService">IInstructorStaffTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public InstructorStaffTypesController(IInstructorStaffTypesService instructorStaffTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _instructorStaffTypesService = instructorStaffTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all instructorStaffTypes
        /// </summary>
        /// <returns>List of InstructorStaffTypes <see cref="Dtos.InstructorStaffTypes"/> objects representing matching instructorStaffTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/instructor-staff-types", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstructorStaffTypesV8", IsEedmSupported = true)]
        [HeaderVersionRoute("/instructor-staff-types", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstructorStaffTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstructorStaffTypes>>> GetInstructorStaffTypesAsync()
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
                var instructorStaffTypes = await _instructorStaffTypesService.GetInstructorStaffTypesAsync(bypassCache);

                if (instructorStaffTypes != null && instructorStaffTypes.Any())
                {
                    AddEthosContextProperties(await _instructorStaffTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _instructorStaffTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              instructorStaffTypes.Select(a => a.Id).ToList()));
                }

                return Ok(instructorStaffTypes);                
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
        /// Read (GET) a instructorStaffTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired instructorStaffTypes</param>
        /// <returns>A instructorStaffTypes object <see cref="Dtos.InstructorStaffTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/instructor-staff-types/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstructorStaffTypesByGuidV8", IsEedmSupported = true)]
        [HeaderVersionRoute("/instructor-staff-types/{guid}", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstructorStaffTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.InstructorStaffTypes>> GetInstructorStaffTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _instructorStaffTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _instructorStaffTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _instructorStaffTypesService.GetInstructorStaffTypesByGuidAsync(guid);
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
        /// Create (POST) a new instructorStaffTypes
        /// </summary>
        /// <param name="instructorStaffTypes">DTO of the new instructorStaffTypes</param>
        /// <returns>A instructorStaffTypes object <see cref="Dtos.InstructorStaffTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/instructor-staff-types", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstructorStaffTypesV8")]
        [HeaderVersionRoute("/instructor-staff-types", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstructorStaffTypesV9")]
        public async Task<ActionResult<Dtos.InstructorStaffTypes>> PostInstructorStaffTypesAsync([FromBody] Dtos.InstructorStaffTypes instructorStaffTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing instructorStaffTypes
        /// </summary>
        /// <param name="guid">GUID of the instructorStaffTypes to update</param>
        /// <param name="instructorStaffTypes">DTO of the updated instructorStaffTypes</param>
        /// <returns>A instructorStaffTypes object <see cref="Dtos.InstructorStaffTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/instructor-staff-types/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstructorStaffTypesV8")]
        [HeaderVersionRoute("/instructor-staff-types/{guid}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstructorStaffTypesV9")]
        public async Task<ActionResult<Dtos.InstructorStaffTypes>> PutInstructorStaffTypesAsync([FromRoute] string guid, [FromBody] Dtos.InstructorStaffTypes instructorStaffTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a instructorStaffTypes
        /// </summary>
        /// <param name="guid">GUID to desired instructorStaffTypes</param>
        [HttpDelete]
        [Route("/instructor-staff-types/{guid}", Name = "DefaultDeleteInstructorStaffTypes", Order = -10)]
        public async Task<IActionResult> DeleteInstructorStaffTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

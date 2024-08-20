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
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to InstructorTenureTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class InstructorTenureTypesController : BaseCompressedApiController
    {
        private readonly IInstructorTenureTypesService _instructorTenureTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructorTenureTypesController class.
        /// </summary>
        /// <param name="instructorTenureTypesService">Service of type <see cref="IInstructorTenureTypesService">IInstructorTenureTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public InstructorTenureTypesController(IInstructorTenureTypesService instructorTenureTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _instructorTenureTypesService = instructorTenureTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all instructorTenureTypes
        /// </summary>
        /// <returns>List of InstructorTenureTypes <see cref="Dtos.InstructorTenureTypes"/> objects representing matching instructorTenureTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/instructor-tenure-types", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstructorTenureTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstructorTenureTypes>>> GetInstructorTenureTypesAsync()
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
                var instructorTenureTypes = await _instructorTenureTypesService.GetInstructorTenureTypesAsync(bypassCache);

                if (instructorTenureTypes != null && instructorTenureTypes.Any())
                {
                    AddEthosContextProperties(await _instructorTenureTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _instructorTenureTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              instructorTenureTypes.Select(a => a.Id).ToList()));
                }
                return Ok(instructorTenureTypes);
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
        /// Read (GET) a instructorTenureTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired instructorTenureTypes</param>
        /// <returns>A instructorTenureTypes object <see cref="Dtos.InstructorTenureTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/instructor-tenure-types/{guid}", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstructorTenureTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.InstructorTenureTypes>> GetInstructorTenureTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _instructorTenureTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _instructorTenureTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _instructorTenureTypesService.GetInstructorTenureTypesByGuidAsync(guid);
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
        /// Create (POST) a new instructorTenureTypes
        /// </summary>
        /// <param name="instructorTenureTypes">DTO of the new instructorTenureTypes</param>
        /// <returns>A instructorTenureTypes object <see cref="Dtos.InstructorTenureTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/instructor-tenure-types", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstructorTenureTypesV9")]
        public async Task<ActionResult<Dtos.InstructorTenureTypes>> PostInstructorTenureTypesAsync([FromBody] Dtos.InstructorTenureTypes instructorTenureTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing instructorTenureTypes
        /// </summary>
        /// <param name="guid">GUID of the instructorTenureTypes to update</param>
        /// <param name="instructorTenureTypes">DTO of the updated instructorTenureTypes</param>
        /// <returns>A instructorTenureTypes object <see cref="Dtos.InstructorTenureTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/instructor-tenure-types/{guid}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstructorTenureTypesV9")]
        public async Task<ActionResult<Dtos.InstructorTenureTypes>> PutInstructorTenureTypesAsync([FromRoute] string guid, [FromBody] Dtos.InstructorTenureTypes instructorTenureTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a instructorTenureTypes
        /// </summary>
        /// <param name="guid">GUID to desired instructorTenureTypes</param>
        [HttpDelete]
        [Route("/instructor-tenure-types/{guid}", Name = "DefaultDeleteInstructorTenureTypes")]
        public async Task<IActionResult> DeleteInstructorTenureTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}

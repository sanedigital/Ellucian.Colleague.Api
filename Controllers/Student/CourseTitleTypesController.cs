// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to CourseTitleTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CourseTitleTypesController : BaseCompressedApiController
    {
        private readonly ICourseTitleTypesService _courseTitleTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CourseTitleTypesController class.
        /// </summary>
        /// <param name="courseTitleTypesService">Service of type <see cref="ICourseTitleTypesService">ICourseTitleTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CourseTitleTypesController(ICourseTitleTypesService courseTitleTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _courseTitleTypesService = courseTitleTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all courseTitleTypes
        /// </summary>
        /// <returns>List of CourseTitleTypes <see cref="CourseTitleType"/> objects representing matching courseTitleTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/course-title-types", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCourseTitleTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<CourseTitleType>>> GetCourseTitleTypesAsync()
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
                var courseTitleTypes = await _courseTitleTypesService.GetCourseTitleTypesAsync(bypassCache);

                if (courseTitleTypes != null && courseTitleTypes.Any())
                {
                    AddEthosContextProperties(await _courseTitleTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _courseTitleTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              courseTitleTypes.Select(a => a.Id).ToList()));
                }
                return Ok(courseTitleTypes);
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
        /// Read (GET) a courseTitleTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired courseTitleTypes</param>
        /// <returns>A courseTitleTypes object <see cref="CourseTitleType"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/course-title-types/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCourseTitleTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<CourseTitleType>> GetCourseTitleTypeByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _courseTitleTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _courseTitleTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _courseTitleTypesService.GetCourseTitleTypeByGuidAsync(guid);
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
        /// Create (POST) a new courseTitleTypes
        /// </summary>
        /// <param name="courseTitleTypes">DTO of the new courseTitleTypes</param>
        /// <returns>A courseTitleTypes object <see cref="CourseTitleType"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/course-title-types", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCourseTitleTypesV1.0.0")]
        public async Task<ActionResult<CourseTitleType>> PostCourseTitleTypesAsync([FromBody] CourseTitleType courseTitleTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing courseTitleTypes
        /// </summary>
        /// <param name="guid">GUID of the courseTitleTypes to update</param>
        /// <param name="courseTitleTypes">DTO of the updated courseTitleTypes</param>
        /// <returns>A courseTitleTypes object <see cref="CourseTitleType"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/course-title-types/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCourseTitleTypesV1.0.0")]
        public async Task<ActionResult<CourseTitleType>> PutCourseTitleTypesAsync([FromRoute] string guid, [FromBody] CourseTitleType courseTitleTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a courseTitleTypes
        /// </summary>
        /// <param name="guid">GUID to desired courseTitleTypes</param>
        [HttpDelete]
        [Route("/course-title-types/{guid}", Name = "DefaultDeleteCourseTitleTypes", Order = -10)]
        public async Task<IActionResult> DeleteCourseTitleTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}
